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
    public class T3InOrderProcessor : ProcessorBase
    {
        public T3InOrderProcessor(T3Config config) : base(config)
        {
        }

        public override bool Step()
        {
            if (IsHalted) return false;

            // 1. Fetch
            BigInteger currentWord = ReadWord(PC);
            
            // 2. Decode
            // Note: For T3-54 in-order, it still fetches 27-trit instructions from memory 
            // according to the basic ISA, but can handle larger values.
            Instruction instr = InstructionDecoder.Decode27(currentWord);

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
            catch (DeviceStallException ex)
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

            // PR is stored as a word. We need to extract the 3-trit flag for predIndex.
            // predIndex 1 -> first 3 trits, etc.
            // For simplicity, we can use a helper or TritArray.
            // In T3-27, PR is 27 trits.
            
            // Temporary implementation: check if the corresponding part of PR is +1
            // This requires bit-manipulation of the balanced ternary representation.
            return GetPredicateFlag(predIndex) == 1;
        }

        private int GetPredicateFlag(int predIndex)
        {
            // Extract the 3-trit group from the PR register
            // This is a simplified version.
            string prStr = new Word27(PR).ToTritString();
            int start = (predIndex - 1) * 3;
            string flag = prStr.Substring(start, 3);
            
            // Evaluate the 3-trit flag as a balanced ternary number
            long val = BalancedTernary.ParseToLong(flag);
            return (int)val; 
            // Spec says: -1 (false), 0 (maybe), +1 (true)
        }

        private void ExecuteInstruction(Instruction instr)
        {
            // Get operands (logical register indices or immediates)
            int op1 = (int)instr.Operand1;
            int op2 = (int)instr.Operand2;

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

                case Opcode.LI:
                    SetRegisterValue(op1, instr.Operand2);
                    IncrementCycles(1);
                    break;

                case Opcode.LIMM:
                    PC++;
                    System.Numerics.BigInteger immVal = ReadWord(PC);
                    SetRegisterValue(op1, immVal);
                    IncrementCycles(2);
                    break;

                case Opcode.ADD:
                case Opcode.SUB:
                case Opcode.MUL:
                case Opcode.DIV:
                case Opcode.MOD:
                    System.Numerics.BigInteger res = T3Alu.Execute(instr.Opcode, GetRegisterValue(op1), GetRegisterValue(op2), Config);
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

                case Opcode.CMP:
                    System.Numerics.BigInteger diff = GetRegisterValue(op1) - GetRegisterValue(op2);
                    Cond = diff > 0 ? 1 : (diff < 0 ? -1 : 0);
                    IncrementCycles(1);
                    break;

                case Opcode.JMP:
                    PC = (long)GetRegisterValue(op1);
                    IncrementCycles(1);
                    break;

                case Opcode.JE:
                    if (Cond == 0) PC = (long)GetRegisterValue(op1);
                    else PC++;
                    IncrementCycles(Cond == 0 ? 2 : 1);
                    break;

                case Opcode.JNE:
                    if (Cond != 0) PC = (long)GetRegisterValue(op1);
                    else PC++;
                    IncrementCycles(Cond != 0 ? 2 : 1);
                    break;

                case Opcode.JL:
                    if (Cond < 0) PC = (long)GetRegisterValue(op1);
                    else PC++;
                    IncrementCycles(Cond < 0 ? 2 : 1);
                    break;

                case Opcode.JG:
                    if (Cond > 0) PC = (long)GetRegisterValue(op1);
                    else PC++;
                    IncrementCycles(Cond > 0 ? 2 : 1);
                    break;

                case Opcode.JM:
                    if (Cond == 0) PC = (long)GetRegisterValue(op1);
                    else PC++;
                    IncrementCycles(Cond == 0 ? 2 : 1);
                    break;

                case Opcode.CALL:
                    SP -= 2;
                    WriteWord(SP, PC + 1);
                    WriteWord(SP + 1, WP);
                    WP = RegisterWindow.CalculateNextWp(WP);
                    PC = (long)GetRegisterValue(op1);
                    IncrementCycles(2);
                    break;

                case Opcode.RET:
                    PC = (long)ReadWord(SP);
                    WP = (long)ReadWord(SP + 1);
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
                    long portIn = (long)GetRegisterValue(op2);
                    SetRegisterValue(op1, DeviceManager.Read(portIn));
                    IncrementCycles(2);
                    break;

                case Opcode.OUT:
                    long portOut = (long)GetRegisterValue(op2);
                    DeviceManager.Write(portOut, GetRegisterValue(op1));
                    IncrementCycles(2);
                    break;

                case Opcode.INI:
                    SetRegisterValue(op1, DeviceManager.Read(instr.Operand2));
                    IncrementCycles(2);
                    break;

                case Opcode.OUTI:
                    DeviceManager.Write(instr.Operand2, GetRegisterValue(op1));
                    IncrementCycles(2);
                    break;

                default:
                    throw new InvalidOperationException($"Instruction {instr.Opcode} is not implemented or not allowed in in-order mode.");
            }
        }

        private System.Numerics.BigInteger GetRegisterValue(int logicalIndex)
        {
            int physicalIndex = RegisterWindow.GetPhysicalIndex(logicalIndex, WP);
            return Registers[physicalIndex];
        }

        private void SetRegisterValue(int logicalIndex, System.Numerics.BigInteger value)
        {
            int physicalIndex = RegisterWindow.GetPhysicalIndex(logicalIndex, WP);
            Registers[physicalIndex] = value;
        }
    }
}