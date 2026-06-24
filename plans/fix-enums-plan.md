# Plan to fix Enum-related tests

## 1. Analysis of `Compile_Enum_Basic` Failure
**Error:** `System.Exception: Expected Semicolon but got Identifier(c) at 4:8`
**Source:**
```c
enum Color { Red = 1, Green, Blue = -1 };
tint main() {
    Color c = Green;
    return c;
}
```
**Cause:** The parser failed to recognize `Color` as a type in the statement `Color c = Green;`. This happens because the `IsType()` method returned `false` for the identifier `Color`, causing the parser to treat `Color c` as an expression. Since an expression cannot be followed by another identifier without an operator, the parser expected a semicolon after `Color` and failed when it encountered `c`.

**Solution:** 
- Ensure that `ParseEnumDef` correctly adds the enum name to the `_typeNames` set.
- Verify that `IsType()` correctly checks this set for identifiers.
- (Current implementation seems to do this, but may be failing due to tokenization or order of operations in some environments).

## 2. Analysis of `Compile_Enum_InFunction` Failure
**Error:** `Assert.AreEqual failed. Expected:<1>. Actual:<51>.`
**Source:**
```c
enum Color { Red = 1, Green, Blue = -1 };
tint get_val(Color c) { return c; }
tint main() {
    return get_val(Red);
}
```
**Cause:** The code compiles and runs, but returns a garbage value (51) instead of the expected value of `Red` (1). This suggests a runtime issue, likely in the calling convention (ABI) or the way constants are handled.

**Potential Issues:**
- **Enum Constant Generation:** The `CodeGenerator` populates `_enumConstants` at the start. `Red` should be 1.
- **Parameter Passing:** The caller pushes the value of `Red` (1) onto the stack. The callee `get_val` pops it into `R3` and stores it in a local variable `c`.
- **Return Value:** The function returns the value of `c` by loading it from the stack into `R2`.
- **Stack Corruption:** The return value `51` might be a remnant of the stack or a register that wasn't correctly initialized/restored.

## 3. Implementation Steps
1. **Verify Parser:** Double-check `Parser.cs` for any logic that might skip enum registration.
2. **Debug CodeGen:** Add temporary debug prints to the generated assembly to trace the value of `Red` and the variable `c`.
3. **Verify ABI:** Ensure that the function prologue and epilogue in `CodeGen.cs` correctly preserve the stack and registers.
4. **Test Fixes:** Run the affected tests and verify they pass.