## Bug Analysis for `Equiv_NestedFunctionCalls_Compiler`

### The Test

```c
tint f(tint x){return x*2;}
tint g(tint x){return f(x+1);}
tint main(){return g(5);}
```

Expected: 12 (f(5+1) = f(6) = 12). Actual: 6.

### Root Cause

After deep analysis of the generated ASM trace and the `EmitCall` method in `CodeGenerator.cs`, I've identified the bug is in __the return value capture order__ in `EmitCall` (lines 518-574).

Currently, the code does:

1. CALL to function
2. Restore caller-saved GP registers (8 POPs)
3. Restore spilled live locals (POP R0 + STORE for each)
4. __THEN__ capture return value: `MOV r, R2`

The problem is that step 3 restores spilled locals by using `POP R0` followed by `STORE R0,R4`. The `STORE` instruction uses `EmitAddr(a+idx)` which sets R4 via `LIMM`. But more critically, the __register `R0`__ (register index 4) is being used both for restoring spilled locals AND potentially for the return value capture register. If `AllocR()` returns `R0` (register 4) for the capture, then the STORE in step 3 already consumed/modified that register context.

However, looking more carefully at the ASM trace output, the actual issue is clearer: the __return value (R2)__ gets overwritten by the spilled-local restore logic. Specifically, the `STORE R0,R4` instruction inside the restore loop uses R4 (Address Reg) and R0, but the key issue is what happens to __R2__ during this process.

Looking at trace: the MUL instruction in `f` gives `R3=6` (not 12), meaning `f` received parameter `x=3` instead of `x=6`. This means `x+1` in `g` computed 3 instead of 6, meaning `x` was loaded as 2 instead of 5.

__The actual root cause is in `GenBin` for binary operations that are evaluated as function arguments.__ When `g` evaluates `f(x+1)`, the `GenExpr` for the argument `x+1` calls `GenBin`. `GenBin` uses `PUSH`/`POP` internally for the left operand. But these stack operations happen __after__ the `EmitCall` spill/save sequence has set up the stack frame. The internal PUSH/POP from GenBin can displace the stack pointer, causing the subsequent spill-restore POPs in `EmitCall` to read wrong values.

More specifically: The spill loads `x` into `RW` and pushes it. Then caller-saved regs are pushed. Then GenBin for `x+1` does `PUSH RX` (for left op of `+`), then `POP RZ`, which consumes that push. But the __result `ADD`__ uses `R0` for its output. Then `PUSH R0` pushes the arg to `f`.

The issue is: after `f` returns, the `EmitCall` restore sequence does 8 POPs for caller-saved regs, then `POP R0` + `STORE` for spilled locals. But looking at the actual compiled ASM, the `POP R0` that restores the spilled `x` happens __after__ the caller-saved POPs. And then `MOV R3,R2` captures the return value. This should work...

__Final determination:__ The bug is in how `GenBin` emits `ADD R0,RZ,RY` where `R0` is register 4 (named "R0"). Looking at the trace, after `ADD R0,RZ,RY`, `R0` = 3 (not 6). This means the ADD operation itself produced 3 from 5+1. This could be a __ternary ALU encoding issue__ where the `ADD` operation behaves differently than expected for decimal inputs.

But wait — the interpreter gets 12, and other ADD tests pass (Equiv_Add works). So the ADD instruction itself works correctly.

__I believe the actual bug is in the T3InOrderProcessor's handling of LIMM instructions that precede the ADD.__ In the compiled code for `g`, there's a `LIMM R4,2001` before `LOAD RX,R4`. The LIMM is a 2-word instruction. If the PC tracking in the simulator incorrectly handles LIMM, it could read the immediate data as an instruction, causing the `LOAD` to execute at the wrong PC or read the wrong address.

Looking at the trace: `[PC=0005] CALL | ...` — this appears to show a CALL instruction being executed at PC=0005, which should be a data word (the immediate of LIMM). This suggests the simulator is __executing data as code__, causing the stack/register corruption that leads to the wrong result.

### Proposed Fix Plan

1. __Fix the T3InOrderProcessor's LIMM handling__ to ensure the immediate word is properly skipped and never executed as an instruction
2. __Alternatively__, fix the `EmitCall` in `CodeGenerator.cs` to capture the return value (MOV r, R2) __before__ restoring spilled locals, not after — this ensures R2 isn't clobbered by the restore process
3. __Verify__ by running the failing test

The safest approach is option 2: reorder the `EmitCall` epilogue to:

1. Capture return value first: `MOV r, R2`
2. Then restore caller-saved registers
3. Then restore spilled locals

This ensures R2 is captured before any restore operations potentially clobber it.
