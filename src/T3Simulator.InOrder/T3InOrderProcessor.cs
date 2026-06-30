using System;
using System.Collections.Generic;
using System.Numerics;
using TritTypes;
using T3Simulator.Common;

namespace T3Simulator.InOrder
{
    /// <summary>
    /// Sequential in-order implementation of the T3 processor.
    /// Supports both T3-27 and T3-54 configurations.
    /// </summary>
    public class T3InOrderProcessor<TWord> : ProcessorBase<TWord> where TWord : IT3Word<TWord>
    {
        private System.IO.TextWriter? _traceWriter;
        public string? LastError { get; private set; }

        public void EnableTracing(System.IO.TextWriter writer)
        {
            _traceWriter = writer;
        }

        private void LogTrace(string message)
        {
            _traceWriter?.WriteLine(message);
        }

        public string DumpState()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"--- Processor State Dump ---");
            sb.AppendLine($"PC: {PC}");
            sb.AppendLine($"SP: {SP}");
            sb.AppendLine($"Cond: {Cond}");
            sb.AppendLine($"Cycles: {CycleCount}, Instructions: {InstructionCount}");
            
            sb.AppendLine("GP Registers:");
            for (int i = 0; i < 9; i++)
            {
                sb.AppendLine($"  R{i}: {GetRegisterValue(i).ToLong()}");
            }
            
            sb.AppendLine("FPU Registers:");
            for (int i = 0; i < 9; i++)
            {
                // Using T3Fpu.ToDoublePrecision for readable output
                var val = T3Fpu.ToDoublePrecision(FRegisters[i]);
                sb.AppendLine($"  F{i}: {val}");
            }

            sb.AppendLine("Predicate Registers (PR):");
            for (int i = 0; i < 18; i++)
            {
                sb.Append($"{PR.GetTrit(i)}");
            }
            sb.AppendLine();

            sb.AppendLine("Memory (around SP):");
            for (long i = SP - 16; i <= SP + 16; i++)
            {
                try {
                    sb.AppendLine($"  [{i:D4}]: {ReadWord(i).ToLong()}");
                } catch {
                    sb.AppendLine($"  [{i:D4}]: <error>");
                }
            }
            
            return sb.ToString();
        }

        public T3InOrderProcessor(T3Config config) : base(config)
        {
            LastError = null;
            // SP is initialized to the end of memory in ProcessorBase
        }

        public override bool Step()
        {
            if (IsHalted) return false;

            TWord currentWord = ReadWord(PC);
            DecodedInstruction instr = InstructionDecoder.Decode(currentWord);

            LogTrace($"PC: {PC:D4} | Word: {currentWord} | Op: {instr.Opcode} | Pred: {instr.Predicate}");

            if (!EvaluatePredicate(instr.Predicate))
            {
                IncrementCycles(1);
                if (instr.Opcode == Opcode.LIMM) PC += 2;
                else PC++;
                return true;
            }

            bool jumped = false;
            try
            {
                jumped = ExecuteInstruction(instr);
            }
            catch (DeviceStallException)
            {
                IncrementStalls();
                IncrementCycles(1);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Processor Exception at PC {PC}: {ex.Message}");
                IsHalted = true;
                return false;
            }

            IncrementInstructions();

            if (!jumped)
            {
                PC++;
            }

            return true;
        }

        private bool EvaluatePredicate(int predIndex)
        {
            if (predIndex == 0) return true;
            if (predIndex < 0 || predIndex > 3) return false;

            return GetPredicateFlag(predIndex) != 0;
        }

        private int GetPredicateFlag(int predIndex)
        {
            // Stabilized Predication Mapping:
            // Predicate 1 -> Trit 0
            // Predicate 2 -> Trit 1
            // Predicate 3 -> Trit 2
            // This aligns with the architectural specification PR[pred-1].
            int targetTrit = predIndex - 1;
            if (targetTrit < 0 || targetTrit >= 18) return 0;
            return PR.GetTrit(targetTrit);
        }

        private bool ExecuteInstruction(DecodedInstruction instr)
        {
            switch (instr.Opcode)
            {
                case Opcode.HALT:
                    IsHalted = true;
                    IncrementCycles(1);
                    return false;

                case Opcode.MOV:
                    SetRegisterValue(instr.PhysOp1, GetRegisterValue(instr.PhysOp2));
                    IncrementCycles(1);
                    return false;
                
                case Opcode.MOVI:
                case Opcode.LI:
                    SetRegisterValue(instr.PhysOp1, FromLong(instr.Immediate));
                    IncrementCycles(1);
                    return false;

                case Opcode.LIMM:
                    PC++;
                    SetRegisterValue(instr.PhysOp1, ReadWord(PC));
                    IncrementCycles(2);
                    return false;

                case Opcode.LOAD:
                    SetRegisterValue(instr.PhysOp1, ReadWord(ToLong(GetRegisterValue(instr.PhysOp2))));
                    IncrementCycles(2);
                    return false;

                case Opcode.STORE:
                    WriteWord(ToLong(GetRegisterValue(instr.PhysOp2)), GetRegisterValue(instr.PhysOp1));
                    IncrementCycles(2);
                    return false;
                
                case Opcode.LOADI:
                    long loadAddr = ToLong(GetRegisterValue(instr.PhysOp2)) + instr.Immediate;
                    SetRegisterValue(instr.PhysOp1, ReadWord(loadAddr));
                    IncrementCycles(2);
                    return false;
                
                case Opcode.STOREI:
                    // Use PhysOp2 as base register (consistent with LOADI: both use op2 as base)
                    long storeAddr = ToLong(GetRegisterValue(instr.PhysOp2)) + instr.Immediate;
                    WriteWord(storeAddr, GetRegisterValue(instr.PhysOp1));
                    IncrementCycles(2);
                    return false;

                case Opcode.ADD:
                case Opcode.SUB:
                case Opcode.MUL:
                case Opcode.DIV:
                case Opcode.MOD:
                    TWord resR = T3Alu.Execute(instr.Opcode, GetRegisterValue(instr.PhysOp2), GetRegisterValue(instr.PhysOp3), Config);
                    SetRegisterValue(instr.PhysOp1, resR);
                    IncrementCycles(instr.Opcode switch
                    {
                        Opcode.ADD or Opcode.SUB => 1,
                        Opcode.MUL => Config == T3Config.T3_18 ? 5 : 8,
                        Opcode.DIV or Opcode.MOD => Config == T3Config.T3_18 ? 10 : 15,
                        _ => 1
                    });
                    return false;
                
                case Opcode.ADDI:
                case Opcode.SUBI:
                case Opcode.MULI:
                case Opcode.DIVI:
                case Opcode.MODI:
                    Opcode baseOp = instr.Opcode switch
                    {
                        Opcode.ADDI => Opcode.ADD,
                        Opcode.SUBI => Opcode.SUB,
                        Opcode.MULI => Opcode.MUL,
                        Opcode.DIVI => Opcode.DIV,
                        Opcode.MODI => Opcode.MOD,
                        _ => Opcode.ADD
                    };

                    if (instr.PhysOp1 == 9 && (baseOp == Opcode.ADD || baseOp == Opcode.SUB))
                    {
                        if (baseOp == Opcode.ADD) SP += instr.Immediate;
                        else SP -= instr.Immediate;
                        IncrementCycles(1);
                    }
                    else
                    {
                        TWord resI = T3Alu.Execute(baseOp, GetRegisterValue(instr.PhysOp1), FromLong(instr.Immediate), Config);
                        SetRegisterValue(instr.PhysOp1, resI);
                        IncrementCycles(baseOp switch
                        {
                            Opcode.ADD or Opcode.SUB => 1,
                            Opcode.MUL => Config == T3Config.T3_18 ? 5 : 8,
                            Opcode.DIV or Opcode.MOD => Config == T3Config.T3_18 ? 10 : 15,
                            _ => 1
                        });
                    }
                    return false;

                case Opcode.NEG:
                    SetRegisterValue(instr.PhysOp1, (TWord)GetRegisterValue(instr.PhysOp2).Negate());
                    IncrementCycles(1);
                    return false;

                case Opcode.NEGI:
                    SetRegisterValue(instr.PhysOp1, FromLong(-instr.Immediate));
                    IncrementCycles(1);
                    return false;

                case Opcode.AND:
                    SetRegisterValue(instr.PhysOp1, T3Alu.TritAnd(GetRegisterValue(instr.PhysOp2), GetRegisterValue(instr.PhysOp3)));
                    IncrementCycles(1);
                    return false;

                case Opcode.ANDI:
                    SetRegisterValue(instr.PhysOp1, T3Alu.TritAnd(GetRegisterValue(instr.PhysOp1), FromLong(instr.Immediate)));
                    IncrementCycles(1);
                    return false;

                case Opcode.ORI:
                    SetRegisterValue(instr.PhysOp1, T3Alu.TritOr(GetRegisterValue(instr.PhysOp1), FromLong(instr.Immediate)));
                    IncrementCycles(1);
                    return false;

                case Opcode.XORI:
                    SetRegisterValue(instr.PhysOp1, T3Alu.TritXor(GetRegisterValue(instr.PhysOp1), FromLong(instr.Immediate)));
                    IncrementCycles(1);
                    return false;

                case Opcode.XOR:
                    SetRegisterValue(instr.PhysOp1, T3Alu.TritXor(GetRegisterValue(instr.PhysOp2), GetRegisterValue(instr.PhysOp3)));
                    IncrementCycles(1);
                    return false;

                case Opcode.SHL:
                    TWord valShl = GetRegisterValue(instr.PhysOp2);
                    int shiftL = (int)GetRegisterValue(instr.PhysOp3).ToInt128();
                    SetRegisterValue(instr.PhysOp1, T3Alu.ShiftLeft(valShl, shiftL));
                    IncrementCycles(1);
                    return false;

                case Opcode.SHR:
                    TWord valShr = GetRegisterValue(instr.PhysOp2);
                    int shiftR = (int)GetRegisterValue(instr.PhysOp3).ToInt128();
                    SetRegisterValue(instr.PhysOp1, T3Alu.ShiftRight(valShr, shiftR));
                    IncrementCycles(1);
                    return false;

                case Opcode.SHLI:
                    TWord valShlI = GetRegisterValue(instr.PhysOp1);
                    int shiftLI = (int)instr.Immediate;
                    SetRegisterValue(instr.PhysOp1, T3Alu.ShiftLeft(valShlI, shiftLI));
                    IncrementCycles(1);
                    return false;

                case Opcode.SHRI:
                    TWord valShrI = GetRegisterValue(instr.PhysOp1);
                    int shiftRI = (int)instr.Immediate;
                    SetRegisterValue(instr.PhysOp1, T3Alu.ShiftRight(valShrI, shiftRI));
                    IncrementCycles(1);
                    return false;

                case Opcode.CMP:
                    TWord cmpA = GetRegisterValue(instr.PhysOp1);
                    TWord cmpB = GetRegisterValue(instr.PhysOp2);
                    Cond = T3Alu.Compare(cmpA, cmpB);
                    IncrementCycles(1);
                    return false;

                case Opcode.CMPI:
                    TWord cmpAI = GetRegisterValue(instr.PhysOp1);
                    TWord cmpBI = FromLong(instr.Immediate);
                    Cond = T3Alu.Compare(cmpAI, cmpBI);
                    IncrementCycles(1);
                    return false;

                case Opcode.JMP:
                    PC = (int)ToLong(GetRegisterValue(instr.PhysOp1));
                    IncrementCycles(1);
                    return true;
                
                case Opcode.JE:
                    if (Cond == 0) {
                        if (instr.Immediate == 0) PC = (int)ToLong(GetRegisterValue(instr.PhysOp1));
                        else PC += instr.Immediate;
                        IncrementCycles(2);
                        return true;
                    }
                    IncrementCycles(1);
                    return false;
                
                case Opcode.JNE:
                    if (Cond != 0) {
                        if (instr.Immediate == 0) PC = (int)ToLong(GetRegisterValue(instr.PhysOp1));
                        else PC += instr.Immediate;
                        IncrementCycles(2);
                        return true;
                    }
                    IncrementCycles(1);
                    return false;
                
                case Opcode.JL:
                    if (Cond < 0) {
                        if (instr.Immediate == 0) PC = (int)ToLong(GetRegisterValue(instr.PhysOp1));
                        else PC += instr.Immediate;
                        IncrementCycles(2);
                        return true;
                    }
                    IncrementCycles(1);
                    return false;
                
                case Opcode.JG:
                    if (Cond > 0) {
                        if (instr.Immediate == 0) PC = (int)ToLong(GetRegisterValue(instr.PhysOp1));
                        else PC += instr.Immediate;
                        IncrementCycles(2);
                        return true;
                    }
                    IncrementCycles(1);
                    return false;
                
                case Opcode.JLE:
                    if (Cond <= 0) {
                        if (instr.Immediate == 0) PC = (int)ToLong(GetRegisterValue(instr.PhysOp1));
                        else PC += instr.Immediate;
                        IncrementCycles(2);
                        return true;
                    }
                    IncrementCycles(1);
                    return false;
                
                case Opcode.JGE:
                    if (Cond >= 0) {
                        if (instr.Immediate == 0) PC = (int)ToLong(GetRegisterValue(instr.PhysOp1));
                        else PC += instr.Immediate;
                        IncrementCycles(2);
                        return true;
                    }
                    IncrementCycles(1);
                    return false;
                
                case Opcode.JM:
                    if (Cond == 0) {
                        if (instr.Immediate == 0) PC = (int)ToLong(GetRegisterValue(instr.PhysOp1));
                        else PC += instr.Immediate;
                        IncrementCycles(2);
                        return true;
                    }
                    IncrementCycles(1);
                    return false;
                
                case Opcode.CALL:
                    LogTrace($"  [CALL] SP: {SP} -> {SP-1} | RetAddr: {PC + 1} | Target: {(instr.Immediate == 0 ? GetRegisterValue(instr.PhysOp1).ToLong().ToString() : (PC + instr.Immediate).ToString())}");
                    // Robust CALL implementation for recursion and deep nesting.
                    // We ensure SP is decremented and the return address is saved.
                    SP -= 1;
                    if (SP < 64) { LastError = "Stack overflow: SP < 64 at CALL"; IsHalted = true; return false; }
                    
                    WriteWord(SP, FromLong(PC + 1));
                    
                    if (instr.Immediate == 0)
                    {
                        PC = (int)ToLong(GetRegisterValue(instr.PhysOp1));
                    }
                    else
                    {
                        PC += instr.Immediate;
                    }
                    IncrementCycles(2);
                    return true;

                case Opcode.RET:
                    TWord retAddrWord = ReadWord(SP);
                    LogTrace($"  [RET] SP: {SP} -> {SP+1} | RetAddr: {retAddrWord.ToLong()}");
                    // Robust RET implementation.
                    if (SP < 0) throw new StackOverflowException("T3 Processor Stack Underflow: RET called with negative SP.");
                    
                    PC = (int)ToLong(retAddrWord);
                    SP += 1;
                    IncrementCycles(2);
                    return true;

                case Opcode.PUSH:
                    SP--;
                    if (SP < 64) { IsHalted = true; return false; }
                    WriteWord(SP, GetRegisterValue(instr.PhysOp1));
                    IncrementCycles(2);
                    return false;

                case Opcode.POP:
                    SetRegisterValue(instr.PhysOp1, ReadWord(SP));
                    SP++;
                    IncrementCycles(2);
                    return false;

                case Opcode.PUSHI:
                    SP--;
                    if (SP < 64) { IsHalted = true; return false; }
                    WriteWord(SP, FromLong(instr.Immediate));
                    IncrementCycles(2);
                    return false;

                case Opcode.POPI:
                    SetRegisterValue(instr.PhysOp1, ReadWord(SP + instr.Immediate));
                    SP++;
                    IncrementCycles(2);
                    return false;

                case Opcode.GETSP:
                    //Console.WriteLine($"DEBUG: GETSP R{instr.PhysOp1} SP={SP}");
                    SetRegisterValue(instr.PhysOp1, FromLong(SP));
                    IncrementCycles(1);
                    return false;

                case Opcode.IN:
                    long portIn = ToLong(GetRegisterValue(instr.PhysOp2));
                    SetRegisterValue(instr.PhysOp1, DeviceManager.Read(portIn));
                    IncrementCycles(2);
                    return false;

                case Opcode.OUT:
                    long portOut = ToLong(GetRegisterValue(instr.PhysOp2));
                    DeviceManager.Write(portOut, GetRegisterValue(instr.PhysOp1));
                    IncrementCycles(2);
                    return false;

                case Opcode.INI:
                    SetRegisterValue(instr.PhysOp1, DeviceManager.Read(instr.Immediate));
                    IncrementCycles(2);
                    return false;

                case Opcode.OUTI:
                    DeviceManager.Write(instr.Immediate, GetRegisterValue(instr.PhysOp1));
                    IncrementCycles(2);
                    return false;

                case Opcode.FADD:
                    SetFRegisterValue(instr.PhysOp1, T3Fpu.Add(GetFRegisterValue(instr.PhysOp2), GetFRegisterValue(instr.PhysOp3)));
                    IncrementCycles(5);
                    return false;

                case Opcode.FSUB:
                    SetFRegisterValue(instr.PhysOp1, T3Fpu.Sub(GetFRegisterValue(instr.PhysOp2), GetFRegisterValue(instr.PhysOp3)));
                    IncrementCycles(5);
                    return false;

                case Opcode.FMUL:
                    SetFRegisterValue(instr.PhysOp1, T3Fpu.Mul(GetFRegisterValue(instr.PhysOp2), GetFRegisterValue(instr.PhysOp3)));
                    IncrementCycles(7);
                    return false;

                case Opcode.FDIV:
                    SetFRegisterValue(instr.PhysOp1, T3Fpu.Div(GetFRegisterValue(instr.PhysOp2), GetFRegisterValue(instr.PhysOp3)));
                    IncrementCycles(15);
                    return false;

                case Opcode.FSQRT:
                    SetFRegisterValue(instr.PhysOp1, T3Fpu.Sqrt(GetFRegisterValue(instr.PhysOp2)));
                    IncrementCycles(20);
                    return false;

                case Opcode.FABS:
                    SetFRegisterValue(instr.PhysOp1, T3Fpu.Abs(GetFRegisterValue(instr.PhysOp2)));
                    IncrementCycles(1);
                    return false;

                case Opcode.FNEG:
                    SetFRegisterValue(instr.PhysOp1, T3Fpu.Neg(GetFRegisterValue(instr.PhysOp2)));
                    IncrementCycles(1);
                    return false;

                case Opcode.FCMP:
                    Cond = T3Fpu.Compare(GetFRegisterValue(instr.PhysOp1), GetFRegisterValue(instr.PhysOp2));
                    IncrementCycles(1);
                    return false;

                case Opcode.FTOI:
                    long intVal = T3Fpu.ToInt(GetFRegisterValue(instr.PhysOp2), instr.Op3);
                    SetRegisterValue(instr.PhysOp1, FromLong(intVal));
                    IncrementCycles(3);
                    return false;

                case Opcode.ITOF:
                    SetFRegisterValue(instr.PhysOp1, T3Fpu.FromInt(ToLong(GetRegisterValue(instr.PhysOp2))));
                    IncrementCycles(3);
                    return false;

                case Opcode.FTOF:
                    SetFRegisterValue(instr.PhysOp1, T3Fpu.FromDoublePrecision(T3Fpu.ToDoublePrecision(GetFRegisterValue(instr.PhysOp2))));
                    IncrementCycles(2);
                    return false;

                case Opcode.FLW:
                    long fAddrLoad = ToLong(GetRegisterValue(instr.PhysOp2)) + (long)instr.Op3;
                    SetFRegisterValue(instr.PhysOp1, T3Float.FromWord18((Word18)(object)ReadWord(fAddrLoad)));
                    IncrementCycles(2);
                    return false;

                case Opcode.FSW:
                    long fAddrStore = ToLong(GetRegisterValue(instr.PhysOp2)) + (long)instr.Op3;
                    WriteWord(fAddrStore, (TWord)(object)GetFRegisterValue(instr.PhysOp1).ToWord18());
                    IncrementCycles(2);
                    return false;

                case Opcode.FMOV:
                    int fmovFunc = instr.Op3;
                    if (fmovFunc == 0) // Fop1 = Fop2
                        SetFRegisterValue(instr.PhysOp1, GetFRegisterValue(instr.PhysOp2));
                    else if (fmovFunc == 1) // Fop1 = Rop2 (R->F)
                        SetFRegisterValue(instr.PhysOp1, T3Fpu.FromInt(ToLong(GetRegisterValue(instr.PhysOp2))));
                    else if (fmovFunc == 2) // Rop1 = Fop2 (F->R)
                        SetRegisterValue(instr.PhysOp1, FromLong(T3Fpu.ToInt(GetFRegisterValue(instr.PhysOp2), 0)));
                    IncrementCycles(1);
                    return false;

                case Opcode.FCLASS:
                    int cls = T3Fpu.Classify(GetFRegisterValue(instr.PhysOp2));
                    SetFRegisterValue(instr.PhysOp1, T3Fpu.FromInt(cls));
                    IncrementCycles(1);
                    return false;

                case Opcode.FSWAP:
                    T3Float temp = GetFRegisterValue(instr.PhysOp1);
                    SetFRegisterValue(instr.PhysOp1, GetFRegisterValue(instr.PhysOp2));
                    SetFRegisterValue(instr.PhysOp2, temp);
                    IncrementCycles(1);
                    return false;

                case Opcode.FZERO:
                    SetFRegisterValue(instr.PhysOp1, T3Fpu.Zero());
                    IncrementCycles(1);
                    return false;

                case Opcode.NOP:
                    IncrementCycles(1);
                    return false;

                default:
                    throw new InvalidOperationException($"Instruction {instr.Opcode} is not implemented or not allowed in in-order mode.");
            }
        }

        private TWord GetRegisterValue(int logicalIndex)
        {
            if (logicalIndex == 9) return FromLong(SP);
            if (logicalIndex < 0 || logicalIndex >= 9) throw new IndexOutOfRangeException($"GP Register index {logicalIndex} out of range [0, 9].");
            return Registers[logicalIndex];
        }

        private void SetRegisterValue(int logicalIndex, TWord value)
        {
            if (logicalIndex == 9) { SP = (int)ToLong(value); return; }
            if (logicalIndex < 0 || logicalIndex >= 9) throw new IndexOutOfRangeException($"GP Register index {logicalIndex} out of range [0, 9].");
            Registers[logicalIndex] = value;
        }

        private T3Float GetFRegisterValue(int logicalIndex)
        {
            if (logicalIndex < 0 || logicalIndex >= 9) throw new IndexOutOfRangeException($"FPU Register index {logicalIndex} out of range [0, 8].");
            return FRegisters[logicalIndex];
        }

        private void SetFRegisterValue(int logicalIndex, T3Float value)
        {
            if (logicalIndex < 0 || logicalIndex >= 9) throw new IndexOutOfRangeException($"FPU Register index {logicalIndex} out of range [0, 8].");
            FRegisters[logicalIndex] = value;
        }
    }
}