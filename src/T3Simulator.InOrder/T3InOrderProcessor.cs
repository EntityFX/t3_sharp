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
            if (instr.Opcode != Opcode.JMP && instr.Opcode != Opcode.JE && 
                instr.Opcode != Opcode.JNE && instr.Opcode != Opcode.JL && 
                instr.Opcode != Opcode.JG && instr.Opcode != Opcode.JM && 
                instr.Opcode != Opcode.CALL && instr.Opcode != Opcode.RET)
            {
                PC++;
            }

            return true;
        }

        private bool EvaluatePredicate(int predIndex)
        {
            if (predIndex == 0) return true; // Unconditional
            if (predIndex < 1 || predIndex > 8) return false;

            return GetPredicateFlag(predIndex) == 1;
        }

        private int GetPredicateFlag(int predIndex)
        {
            string prStr = PR.ToTritString();
            int start = (predIndex - 1) * 3;
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

                case Opcode.LOAD:
                    long addrLoad = ToLong(GetRegisterValue((int)op2));
                    SetRegisterValue((int)op1, ReadWord(addrLoad));
                    IncrementCycles(2);
                    break;

                case Opcode.STORE:
                    long addrStore = ToLong(GetRegisterValue((int)op2));
                    WriteWord(addrStore, GetRegisterValue((int)op1));
                    IncrementCycles(2);
                    break;

                case Opcode.LI:
                    SetRegisterValue((int)op1, FromLong(instr.Op2));
                    IncrementCycles(1);
                    break;

                case Opcode.LIMM:
                    PC++;
                    TWord immVal = ReadWord(PC);
                    SetRegisterValue((int)op1, immVal);
                    IncrementCycles(2);
                    break;

                case Opcode.ADD:
                case Opcode.SUB:
                case Opcode.MUL:
                case Opcode.DIV:
                case Opcode.MOD:
                    TWord res = T3Alu.Execute(instr.Opcode, GetRegisterValue((int)op1), GetRegisterValue((int)op2), Config);
                    SetRegisterValue((int)op1, res);
                    IncrementCycles(instr.Opcode switch {
                        Opcode.ADD => 1,
                        Opcode.SUB => 1,
                        Opcode.MUL => Config == T3Config.T3_18 ? 5 : 8,
                        Opcode.DIV => Config == T3Config.T3_18 ? 10 : 15,
                        Opcode.MOD => Config == T3Config.T3_18 ? 10 : 15,
                        _ => 1
                    });
                    break;

                case Opcode.NEG:
                    SetRegisterValue((int)op1, (TWord)GetRegisterValue((int)op1).Negate());
                    IncrementCycles(1);
                    break;

                case Opcode.TRITAND:
                    SetRegisterValue((int)op1, T3Alu.TritAnd(GetRegisterValue((int)op1), GetRegisterValue((int)op2)));
                    IncrementCycles(1);
                    break;
                
                case Opcode.TRITOR:
                    SetRegisterValue((int)op1, T3Alu.TritOr(GetRegisterValue((int)op1), GetRegisterValue((int)op2)));
                    IncrementCycles(1);
                    break;
                
                case Opcode.TRITXOR:
                    SetRegisterValue((int)op1, T3Alu.TritXor(GetRegisterValue((int)op1), GetRegisterValue((int)op2)));
                    IncrementCycles(1);
                    break;
                
                case Opcode.SHL:
                    TWord valShl = GetRegisterValue((int)op1);
                    int shiftL = (int)GetRegisterValue((int)op2).ToInt128();
                    SetRegisterValue((int)op1, T3Alu.ShiftLeft(valShl, shiftL));
                    IncrementCycles(1);
                    break;
                
                case Opcode.SHR:
                    TWord valShr = GetRegisterValue((int)op1);
                    int shiftR = (int)GetRegisterValue((int)op2).ToInt128();
                    SetRegisterValue((int)op1, T3Alu.ShiftRight(valShr, shiftR));
                    IncrementCycles(1);
                    break;

                case Opcode.CMP:
                    Cond = T3Alu.Compare(GetRegisterValue((int)op1), GetRegisterValue((int)op2));
                    IncrementCycles(1);
                    break;

                case Opcode.JMP:
                    PC = ToLong(GetRegisterValue((int)op1));
                    IncrementCycles(1);
                    break;

                case Opcode.JE:
                    if (Cond == 0) PC = ToLong(GetRegisterValue((int)op1));
                    else PC++;
                    IncrementCycles(Cond == 0 ? 2 : 1);
                    break;

                case Opcode.JNE:
                    if (Cond != 0) PC = ToLong(GetRegisterValue((int)op1));
                    else PC++;
                    IncrementCycles(Cond != 0 ? 2 : 1);
                    break;

                case Opcode.JL:
                    if (Cond < 0) PC = ToLong(GetRegisterValue((int)op1));
                    else PC++;
                    IncrementCycles(Cond < 0 ? 2 : 1);
                    break;

                case Opcode.JG:
                    if (Cond > 0) PC = ToLong(GetRegisterValue((int)op1));
                    else PC++;
                    IncrementCycles(Cond > 0 ? 2 : 1);
                    break;

                case Opcode.JM:
                    if (Cond == 0) PC = ToLong(GetRegisterValue((int)op1));
                    else PC++;
                    IncrementCycles(Cond == 0 ? 2 : 1);
                    break;

                case Opcode.CALL:
                    TWord targetPC = GetRegisterValue((int)op1);
                    SP -= 2;
                    WriteWord(SP, FromLong(PC + 1));
                    WriteWord(SP + 1, FromLong(WP));
                    WP = (int)RegisterWindow.CalculateNextWp(WP);
                    PC = ToLong(targetPC);
                    IncrementCycles(2);
                    break;

                case Opcode.RET:
                    PC = ToLong(ReadWord(SP));
                    WP = (int)ToLong(ReadWord(SP + 1));
                    SP += 2;
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

                default:
                    throw new InvalidOperationException($"Instruction {instr.Opcode} is not implemented or not allowed in in-order mode.");
            }
        }

        private TWord GetRegisterValue(int logicalIndex)
        {
            int physicalIndex = RegisterWindow.GetPhysicalIndex(logicalIndex, WP);
            return Registers[physicalIndex];
        }

        private void SetRegisterValue(int logicalIndex, TWord value)
        {
            int physicalIndex = RegisterWindow.GetPhysicalIndex(logicalIndex, WP);
            Registers[physicalIndex] = value;
        }
    }
}
