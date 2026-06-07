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
    public class T3InOrderProcessor<TWord> : ProcessorBase<TWord> where TWord : INumber<TWord>
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
            // To extract trits, we temporarily convert the PR (TWord) to a Word type
            string prStr;
            if (typeof(TWord) == typeof(long))
            {
                prStr = new Word27((long)(object)PR).ToTritString();
            }
            else if (typeof(TWord) == typeof(Int128))
            {
                prStr = new Word54((Int128)(object)PR).ToTritString();
            }
            else
            {
                throw new NotSupportedException($"Unsupported word type for predicate evaluation: {typeof(TWord)}");
            }

            int start = (predIndex - 1) * 3;
            string flag = prStr.Substring(start, 3);
            
            return (int)BalancedTernary.ParseToLong(flag);
        }

        private void ExecuteInstruction(Instruction<TWord> instr)
        {
            // Get operands (logical register indices or immediates)
            int op1 = (int)ToLong(instr.Operand1);
            int op2 = (int)ToLong(instr.Operand2);

            switch (instr.Opcode)
            {
                case Opcode.HALT:
                    IsHalted = true;
                    IncrementCycles(1);
                    break;
                
                case Opcode.MOV:
                    SetRegisterValue(op1, GetRegisterValue(op2));
                    IncrementCycles(1);
                    break;

                case Opcode.LOAD:
                    long addrLoad = ToLong(GetRegisterValue(op2));
                    SetRegisterValue(op1, ReadWord(addrLoad));
                    IncrementCycles(2);
                    break;

                case Opcode.STORE:
                    long addrStore = ToLong(GetRegisterValue(op2));
                    WriteWord(addrStore, GetRegisterValue(op1));
                    IncrementCycles(2);
                    break;

                case Opcode.LI:
                    SetRegisterValue(op1, instr.Operand2);
                    IncrementCycles(1);
                    break;

                case Opcode.LIMM:
                    PC++;
                    TWord immVal = ReadWord(PC);
                    SetRegisterValue(op1, immVal);
                    IncrementCycles(2);
                    break;

                case Opcode.ADD:
                case Opcode.SUB:
                case Opcode.MUL:
                case Opcode.DIV:
                case Opcode.MOD:
                    TWord res = T3Alu.Execute(instr.Opcode, GetRegisterValue(op1), GetRegisterValue(op2), Config);
                    SetRegisterValue(op1, res);
                    IncrementCycles(instr.Opcode switch {
                        Opcode.ADD => 1,
                        Opcode.SUB => 1,
                        Opcode.MUL => Config == T3Config.T3_27 ? 5 : 8,
                        Opcode.DIV => Config == T3Config.T3_27 ? 10 : 15,
                        Opcode.MOD => Config == T3Config.T3_27 ? 10 : 15,
                        _ => 1
                    });
                    break;

                case Opcode.NEG:
                    SetRegisterValue(op1, -GetRegisterValue(op1));
                    IncrementCycles(1);
                    break;

                case Opcode.TRITAND:
                    TWord valAnd1 = GetRegisterValue(op1);
                    TWord valAnd2 = GetRegisterValue(op2);
                    if (typeof(TWord) == typeof(long))
                        SetRegisterValue(op1, (TWord)(object)((long)Word27.TritAnd(new Word27((long)(object)valAnd1), new Word27((long)(object)valAnd2))));
                    else
                        SetRegisterValue(op1, (TWord)(object)((Int128)Word54.TritAnd(new Word54((Int128)(object)valAnd1), new Word54((Int128)(object)valAnd2))));
                    IncrementCycles(1);
                    break;
                
                case Opcode.TRITOR:
                    TWord valOr1 = GetRegisterValue(op1);
                    TWord valOr2 = GetRegisterValue(op2);
                    if (typeof(TWord) == typeof(long))
                        SetRegisterValue(op1, (TWord)(object)((long)Word27.TritOr(new Word27((long)(object)valOr1), new Word27((long)(object)valOr2))));
                    else
                        SetRegisterValue(op1, (TWord)(object)((Int128)Word54.TritOr(new Word54((Int128)(object)valOr1), new Word54((Int128)(object)valOr2))));
                    IncrementCycles(1);
                    break;
                
                case Opcode.TRITXOR:
                    TWord valXor1 = GetRegisterValue(op1);
                    TWord valXor2 = GetRegisterValue(op2);
                    if (typeof(TWord) == typeof(long))
                        SetRegisterValue(op1, (TWord)(object)((long)Word27.TritXor(new Word27((long)(object)valXor1), new Word27((long)(object)valXor2))));
                    else
                        SetRegisterValue(op1, (TWord)(object)((Int128)Word54.TritXor(new Word54((Int128)(object)valXor1), new Word54((Int128)(object)valXor2))));
                    IncrementCycles(1);
                    break;
                
                case Opcode.SHL:
                    TWord valShl = GetRegisterValue(op1);
                    int shiftL = (int)Convert.ToInt32(GetRegisterValue(op2));
                    if (typeof(TWord) == typeof(long))
                        SetRegisterValue(op1, (TWord)(object)((long)(new Word27((long)(object)valShl) << shiftL)));
                    else
                        SetRegisterValue(op1, (TWord)(object)((Int128)(new Word54((Int128)(object)valShl) << shiftL)));
                    IncrementCycles(1);
                    break;
                
                case Opcode.SHR:
                    TWord valShr = GetRegisterValue(op1);
                    int shiftR = (int)Convert.ToInt32(GetRegisterValue(op2));
                    if (typeof(TWord) == typeof(long))
                        SetRegisterValue(op1, (TWord)(object)((long)(new Word27((long)(object)valShr) >> shiftR)));
                    else
                        SetRegisterValue(op1, (TWord)(object)((Int128)(new Word54((Int128)(object)valShr) >> shiftR)));
                    IncrementCycles(1);
                    break;

                case Opcode.CMP:
                    TWord diff = GetRegisterValue(op1) - GetRegisterValue(op2);
                    Cond = diff > TWord.Zero ? 1 : (diff < TWord.Zero ? -1 : 0);
                    IncrementCycles(1);
                    break;

                case Opcode.JMP:
                    PC = ToLong(GetRegisterValue(op1));
                    IncrementCycles(1);
                    break;

                case Opcode.JE:
                    if (Cond == 0) PC = ToLong(GetRegisterValue(op1));
                    else PC++;
                    IncrementCycles(Cond == 0 ? 2 : 1);
                    break;

                case Opcode.JNE:
                    if (Cond != 0) PC = ToLong(GetRegisterValue(op1));
                    else PC++;
                    IncrementCycles(Cond != 0 ? 2 : 1);
                    break;

                case Opcode.JL:
                    if (Cond < 0) PC = ToLong(GetRegisterValue(op1));
                    else PC++;
                    IncrementCycles(Cond < 0 ? 2 : 1);
                    break;

                case Opcode.JG:
                    if (Cond > 0) PC = ToLong(GetRegisterValue(op1));
                    else PC++;
                    IncrementCycles(Cond > 0 ? 2 : 1);
                    break;

                case Opcode.JM:
                    if (Cond == 0) PC = ToLong(GetRegisterValue(op1));
                    else PC++;
                    IncrementCycles(Cond == 0 ? 2 : 1);
                    break;

                case Opcode.CALL:
                    TWord targetPC = GetRegisterValue(op1);
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
                    WriteWord(SP, GetRegisterValue(op1));
                    IncrementCycles(2);
                    break;

                case Opcode.POP:
                    SetRegisterValue(op1, ReadWord(SP));
                    SP++;
                    IncrementCycles(2);
                    break;

                case Opcode.IN:
                    long portIn = ToLong(GetRegisterValue(op2));
                    SetRegisterValue(op1, DeviceManager.Read(portIn));
                    IncrementCycles(2);
                    break;

                case Opcode.OUT:
                    long portOut = ToLong(GetRegisterValue(op2));
                    DeviceManager.Write(portOut, GetRegisterValue(op1));
                    IncrementCycles(2);
                    break;

                case Opcode.INI:
                    SetRegisterValue(op1, DeviceManager.Read(ToLong(instr.Operand2)));
                    IncrementCycles(2);
                    break;

                case Opcode.OUTI:
                    DeviceManager.Write(ToLong(instr.Operand2), GetRegisterValue(op1));
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