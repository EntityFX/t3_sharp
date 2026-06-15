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
        public T3InOrderProcessor(T3Config config) : base(config)
        {
        }

        public override bool Step()
        {
            if (IsHalted) return false;

            // 1. Fetch
            TWord currentWord = ReadWord(PC);

            // 2. Decode
            Instruction<TWord> instr = InstructionDecoder.Decode(currentWord);

            // DEBUG: Trace every instruction fetch
            // Console.WriteLine($"STEP: PC={PC} Word={currentWord} Opcode={instr.Opcode} Pred={instr.PredicateIndex}");

            // 3. Predicate Evaluation
            if (!EvaluatePredicate(instr.PredicateIndex))
            {
                // NOP - Instruction not executed
                IncrementCycles(1);
                PC++;
                return true;
            }

            // 4. Execute
            try
            {
                ExecuteInstruction(instr);
            }
            catch (DeviceStallException)
            {
                IncrementStalls();
                IncrementCycles(1);
                return true; // Stall, don't advance PC
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Processor Exception at PC {PC}: {ex.Message}");
                IsHalted = true;
                return false;
            }

            // 5. Update State
            IncrementInstructions();

            // PC is updated inside ExecuteInstruction for branches, 
            // otherwise it's incremented here.
            if (!IsJumpInstruction(instr.Opcode))
            {
                PC++;
            }

            return true;
        }

        private bool EvaluatePredicate(int predIndex)
        {
            if (predIndex == 0) return true; // Unconditional
            if (predIndex < 0 || predIndex > 3) return false;

            return GetPredicateFlag(predIndex) == 1;
        }

        private bool IsJumpInstruction(Opcode op)
        {
            return op switch
            {
                Opcode.JMP or Opcode.JE or Opcode.JNE or Opcode.JL or Opcode.JG or 
                Opcode.JM or Opcode.JLE or Opcode.JGE or Opcode.CALL or Opcode.RET or Opcode.LIMM => true,
                _ => false
            };
        }

        private int GetPredicateFlag(int predIndex)
        {
            string prStr = PR.ToTritString();
            // Predicates are encoded in the PR word.
            // p1: trits 12-14, p2: trits 9-11, p3: trits 6-8
            // (p0 is unconditional)
            int start = 15 - (predIndex * 3); 
            if (start < 0) return 0;
            string flag = prStr.Substring(start, 3);

            return (int)BalancedTernary.ParseToLong(flag);
        }

        private void ExecuteInstruction(Instruction<TWord> instr)
        {
            // Get operands (logical register indices or immediates)
            long op1 = (long)instr.Operand1;
            long op2 = (long)instr.Operand2;

            switch (instr.Opcode)
            {
                case Opcode.HALT:
                    IsHalted = true;
                    IncrementCycles(1);
                    break;

                case Opcode.MOV:
                    SetRegisterValue((int)op1, GetRegisterValue((int)op2));
                    IncrementCycles(1);
                    break;
                
                case Opcode.MOVI:
                case Opcode.LI:
                    SetRegisterValue((int)op1, FromLong(instr.Immediate));
                    IncrementCycles(1);
                    break;

                case Opcode.LIMM:
                    // LIMM reads the next word in memory as a constant
                    PC++;
                    SetRegisterValue((int)op1, ReadWord(PC));
                    IncrementCycles(2);
                    break;

                case Opcode.LOAD:
                    SetRegisterValue((int)op1, ReadWord(ToLong(GetRegisterValue((int)op2))));
                    IncrementCycles(2);
                    break;
                
                case Opcode.LOADI:
                    SetRegisterValue((int)op1, ReadWord(ToLong(GetRegisterValue((int)op2)) + instr.Immediate));
                    IncrementCycles(2);
                    break;
                
                case Opcode.STORE:
                    WriteWord(ToLong(GetRegisterValue((int)op2)), GetRegisterValue((int)op1));
                    IncrementCycles(2);
                    break;
                
                case Opcode.STOREI:
                    WriteWord(ToLong(GetRegisterValue((int)op2)) + instr.Immediate, GetRegisterValue((int)op1));
                    IncrementCycles(2);
                    break;

                case Opcode.ADD:
                case Opcode.SUB:
                case Opcode.MUL:
                case Opcode.DIV:
                case Opcode.MOD:
                    TWord resR = T3Alu.Execute(instr.Opcode, GetRegisterValue((int)op2), GetRegisterValue((int)instr.Op3), Config);
                    SetRegisterValue((int)op1, resR);
                    IncrementCycles(instr.Opcode switch
                    {
                        Opcode.ADD or Opcode.SUB => 1,
                        Opcode.MUL => Config == T3Config.T3_18 ? 5 : 8,
                        Opcode.DIV or Opcode.MOD => Config == T3Config.T3_18 ? 10 : 15,
                        _ => 1
                    });
                    break;
                
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
                    TWord resI = T3Alu.Execute(baseOp, GetRegisterValue((int)op2), FromLong(instr.Immediate), Config);
                    SetRegisterValue((int)op1, resI);
                    IncrementCycles(baseOp switch
                    {
                        Opcode.ADD or Opcode.SUB => 1,
                        Opcode.MUL => Config == T3Config.T3_18 ? 5 : 8,
                        Opcode.DIV or Opcode.MOD => Config == T3Config.T3_18 ? 10 : 15,
                        _ => 1
                    });
                    break;

                case Opcode.NEG:
                    SetRegisterValue((int)op1, (TWord)GetRegisterValue((int)op2).Negate());
                    IncrementCycles(1);
                    break;

                case Opcode.NEGI:
                    SetRegisterValue((int)op1, FromLong(-instr.Immediate));
                    IncrementCycles(1);
                    break;

                case Opcode.AND:
                    SetRegisterValue((int)op1, T3Alu.TritAnd(GetRegisterValue((int)op2), GetRegisterValue((int)instr.Op3)));
                    IncrementCycles(1);
                    break;

                case Opcode.ANDI:
                    SetRegisterValue((int)op1, T3Alu.TritAnd(GetRegisterValue((int)op2), FromLong(instr.Immediate)));
                    IncrementCycles(1);
                    break;

                case Opcode.OR:
                    SetRegisterValue((int)op1, T3Alu.TritOr(GetRegisterValue((int)op2), GetRegisterValue((int)instr.Op3)));
                    IncrementCycles(1);
                    break;

                case Opcode.ORI:
                    SetRegisterValue((int)op1, T3Alu.TritOr(GetRegisterValue((int)op2), FromLong(instr.Immediate)));
                    IncrementCycles(1);
                    break;

                case Opcode.XOR:
                    SetRegisterValue((int)op1, T3Alu.TritXor(GetRegisterValue((int)op2), GetRegisterValue((int)instr.Op3)));
                    IncrementCycles(1);
                    break;

                case Opcode.XORI:
                    SetRegisterValue((int)op1, T3Alu.TritXor(GetRegisterValue((int)op2), FromLong(instr.Immediate)));
                    IncrementCycles(1);
                    break;

                case Opcode.SHL:
                    TWord valShl = GetRegisterValue((int)op2);
                    int shiftL = (int)GetRegisterValue((int)instr.Op3).ToInt128();
                    SetRegisterValue((int)op1, T3Alu.ShiftLeft(valShl, shiftL));
                    IncrementCycles(1);
                    break;

                case Opcode.SHLI:
                    TWord valShlI = GetRegisterValue((int)op2);
                    int shiftLI = (int)instr.Immediate;
                    SetRegisterValue((int)op1, T3Alu.ShiftLeft(valShlI, shiftLI));
                    IncrementCycles(1);
                    break;

                case Opcode.SHR:
                    TWord valShr = GetRegisterValue((int)op2);
                    int shiftR = (int)GetRegisterValue((int)instr.Op3).ToInt128();
                    SetRegisterValue((int)op1, T3Alu.ShiftRight(valShr, shiftR));
                    IncrementCycles(1);
                    break;

                case Opcode.SHRI:
                    TWord valShrI = GetRegisterValue((int)op2);
                    int shiftRI = (int)instr.Immediate;
                    SetRegisterValue((int)op1, T3Alu.ShiftRight(valShrI, shiftRI));
                    IncrementCycles(1);
                    break;

                case Opcode.CMP:
                    TWord cmpA = GetRegisterValue((int)op1);
                    TWord cmpB = GetRegisterValue((int)op2);
                    Cond = T3Alu.Compare(cmpA, cmpB);
                    IncrementCycles(1);
                    break;

                case Opcode.CMPI:
                    TWord cmpAI = GetRegisterValue((int)op1);
                    TWord cmpBI = FromLong(instr.Immediate);
                    Cond = T3Alu.Compare(cmpAI, cmpBI);
                    IncrementCycles(1);
                    break;

                case Opcode.JMP:
                    if (instr.Operand2 != 0 && instr.Immediate == 0)
                    {
                        dynamic regVal = GetRegisterValue((int)op2);
                        PC = (int)regVal.ToLong();
                    }
                    else
                        PC += instr.Immediate;
                    IncrementCycles(1);
                    break;
                
                case Opcode.JE:
                    if (Cond == 0) {
                        if (instr.Operand2 != 0 && instr.Immediate == 0) PC = (int)ToLong(GetRegisterValue((int)instr.Operand2));
                        else PC += instr.Immediate;
                    } else PC++;
                    IncrementCycles(Cond == 0 ? 2 : 1);
                    break;
                
                case Opcode.JNE:
                    if (Cond != 0) {
                        if (instr.Operand2 != 0 && instr.Immediate == 0) PC = (int)ToLong(GetRegisterValue((int)instr.Operand2));
                        else PC += instr.Immediate;
                    } else PC++;
                    IncrementCycles(Cond != 0 ? 2 : 1);
                    break;
                
                case Opcode.JL:
                    if (Cond < 0) {
                        if (instr.Operand2 != 0 && instr.Immediate == 0) PC = (int)ToLong(GetRegisterValue((int)instr.Operand2));
                        else PC += instr.Immediate;
                    } else PC++;
                    IncrementCycles(Cond < 0 ? 2 : 1);
                    break;
                
                case Opcode.JG:
                    if (Cond > 0) {
                        if (instr.Operand2 != 0 && instr.Immediate == 0) PC = (int)ToLong(GetRegisterValue((int)instr.Operand2));
                        else PC += instr.Immediate;
                    } else PC++;
                    IncrementCycles(Cond > 0 ? 2 : 1);
                    break;
                
                case Opcode.JLE:
                    if (Cond <= 0) {
                        if (instr.Operand2 != 0 && instr.Immediate == 0) PC = (int)ToLong(GetRegisterValue((int)instr.Operand2));
                        else PC += instr.Immediate;
                    } else PC++;
                    IncrementCycles(Cond <= 0 ? 2 : 1);
                    break;
                
                case Opcode.JGE:
                    if (Cond >= 0) {
                        if (instr.Operand2 != 0 && instr.Immediate == 0) PC = (int)ToLong(GetRegisterValue((int)instr.Operand2));
                        else PC += instr.Immediate;
                    } else PC++;
                    IncrementCycles(Cond >= 0 ? 2 : 1);
                    break;
                
                case Opcode.JM:
                    if (Cond == 0) {
                        if (instr.Operand2 != 0 && instr.Immediate == 0) PC = (int)ToLong(GetRegisterValue((int)instr.Operand2));
                        else PC += instr.Immediate;
                    } else PC++;
                    IncrementCycles(Cond == 0 ? 2 : 1);
                    break;
                
                case Opcode.CALL:
                    SP -= 1;
                    WriteWord(SP, FromLong(PC + 1));
                    if (instr.Operand2 != 0 && instr.Immediate == 0)
                    {
                        dynamic regVal = GetRegisterValue((int)op2);
                        PC = (int)regVal.ToLong();
                    }
                    else
                        PC += instr.Immediate;
                    IncrementCycles(2);
                    break;

                case Opcode.RET:
                    // Pop return address into PC
                    PC = ToLong(ReadWord(SP));
                    SP += 1;
                    IncrementCycles(2);
                    break;

                case Opcode.PUSH:
                    SP--;
                    WriteWord(SP, GetRegisterValue((int)op1));
                    IncrementCycles(2);
                    break;

                case Opcode.POP:
                    SetRegisterValue((int)op1, ReadWord(SP));
                    SP++;
                    IncrementCycles(2);
                    break;

                case Opcode.IN:
                    long portIn = ToLong(GetRegisterValue((int)op2));
                    SetRegisterValue((int)op1, DeviceManager.Read(portIn));
                    IncrementCycles(2);
                    break;

                case Opcode.OUT:
                    long portOut = ToLong(GetRegisterValue((int)op2));
                    DeviceManager.Write(portOut, GetRegisterValue((int)op1));
                    IncrementCycles(2);
                    break;

                case Opcode.INI:
                    SetRegisterValue((int)op1, DeviceManager.Read((long)instr.Operand2));
                    IncrementCycles(2);
                    break;

                case Opcode.OUTI:
                    DeviceManager.Write((long)instr.Operand2, GetRegisterValue((int)op1));
                    IncrementCycles(2);
                    break;

                case Opcode.FADD:
                    FRegisters[(int)op1] = T3Fpu.Add(FRegisters[(int)op2], FRegisters[(int)instr.Op3]);
                    IncrementCycles(5);
                    break;

                case Opcode.FSUB:
                    FRegisters[(int)op1] = T3Fpu.Sub(FRegisters[(int)op2], FRegisters[(int)instr.Op3]);
                    IncrementCycles(5);
                    break;

                case Opcode.FMUL:
                    FRegisters[(int)op1] = T3Fpu.Mul(FRegisters[(int)op2], FRegisters[(int)instr.Op3]);
                    IncrementCycles(7);
                    break;

                case Opcode.FDIV:
                    FRegisters[(int)op1] = T3Fpu.Div(FRegisters[(int)op2], FRegisters[(int)instr.Op3]);
                    IncrementCycles(15);
                    break;

                case Opcode.FSQRT:
                    FRegisters[(int)op1] = T3Fpu.Sqrt(FRegisters[(int)op2]);
                    IncrementCycles(20);
                    break;

                case Opcode.FABS:
                    FRegisters[(int)op1] = T3Fpu.Abs(FRegisters[(int)op2]);
                    IncrementCycles(1);
                    break;

                case Opcode.FNEG:
                    FRegisters[(int)op1] = T3Fpu.Neg(FRegisters[(int)op2]);
                    IncrementCycles(1);
                    break;

                case Opcode.FCMP:
                    Cond = T3Fpu.Compare(FRegisters[(int)op1], FRegisters[(int)op2]);
                    IncrementCycles(1);
                    break;

                case Opcode.FTOI:
                    long intVal = T3Fpu.ToInt(FRegisters[(int)op2], instr.Func);
                    SetRegisterValue((int)op1, FromLong(intVal));
                    IncrementCycles(3);
                    break;

                case Opcode.ITOF:
                    FRegisters[(int)op1] = T3Fpu.FromInt(ToLong(GetRegisterValue((int)op2)));
                    IncrementCycles(3);
                    break;

                case Opcode.FTOF:
                    // func=0: tfloat→tdouble, func=1: tdouble→tfloat
                    // Both simulate round-trip through double precision (tdouble uses register pairs in real hw)
                    FRegisters[(int)op1] = T3Fpu.FromDoublePrecision(T3Fpu.ToDoublePrecision(FRegisters[(int)op2]));
                    IncrementCycles(2);
                    break;

                case Opcode.FLW:
                    long fAddrLoad = ToLong(GetRegisterValue((int)op2)) + (long)instr.Op3;
                    FRegisters[(int)op1] = T3Float.FromWord18((Word18)(object)ReadWord(fAddrLoad));
                    IncrementCycles(2);
                    break;

                case Opcode.FSW:
                    long fAddrStore = ToLong(GetRegisterValue((int)op2)) + (long)instr.Op3;
                    WriteWord(fAddrStore, (TWord)(object)FRegisters[(int)op1].ToWord18());
                    IncrementCycles(2);
                    break;

                case Opcode.FMOV:
                    if (instr.Func == 0) // Fop1 = Fop2
                        FRegisters[(int)op1] = FRegisters[(int)op2];
                    else if (instr.Func == 1) // Rop1 = Fop2
                        SetRegisterValue((int)op1, FromLong(T3Fpu.ToInt(FRegisters[(int)op2], 0)));
                    else if (instr.Func == 2) // Fop1 = Rop2
                        FRegisters[(int)op1] = T3Fpu.FromInt(ToLong(GetRegisterValue((int)op2)));
                    IncrementCycles(1);
                    break;

                case Opcode.FCLASS:
                    int cls = T3Fpu.Classify(FRegisters[(int)op2]);
                    FRegisters[(int)op1] = T3Fpu.FromInt(cls);
                    IncrementCycles(1);
                    break;

                case Opcode.FSWAP:
                    T3Float temp = FRegisters[(int)op1];
                    FRegisters[(int)op1] = FRegisters[(int)op2];
                    FRegisters[(int)op2] = temp;
                    IncrementCycles(1);
                    break;

                case Opcode.FZERO:
                    FRegisters[(int)op1] = T3Fpu.Zero();
                    IncrementCycles(1);
                    break;

                case Opcode.NOP:
                    // No operation
                    IncrementCycles(1);
                    break;

                default:
                    throw new InvalidOperationException($"Instruction {instr.Opcode} is not implemented or not allowed in in-order mode.");
            }
        }

        private TWord GetRegisterValue(int logicalIndex)
        {
            return Registers[logicalIndex];
        }

        private void SetRegisterValue(int logicalIndex, TWord value)
        {
            Registers[logicalIndex] = value;
        }
    }
}