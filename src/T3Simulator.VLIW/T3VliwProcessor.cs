using System;
using System.Collections.Generic;
using System.Numerics;
using TritTypes;
using T3Simulator.Common;

namespace T3Simulator.VLIW
{
    /// <summary>
    /// High-performance VLIW implementation of the T3 processor (T3-54 only).
    /// Executes bundles of 3 instructions in parallel with conflict detection.
    /// </summary>
    public class T3VliwProcessor : ProcessorBase
    {
        private BigInteger[] _shadowRegisters;
        private bool _isSpeculating;

        public T3VliwProcessor() : base(T3Config.T3_54)
        {
        }

        public override bool Step()
        {
            if (IsHalted) return false;

            // 1. Fetch Bundle
            BigInteger currentWord = ReadWord(PC);
            VliwBundle bundle = VliwBundle.Decode(currentWord);

            // 2. Conflict Detection
            if (HasRegisterConflict(bundle))
            {
                throw new InvalidOperationException($"VLIW Bundle at PC {PC} has a register write conflict.");
            }

            // 3. Predication & Execution
            bool hasBranch = false;
            int memorySlot = -1;
            int branchSlot = -1;

            for (int i = 0; i < 3; i++)
            {
                VliwSlot slot = GetSlot(bundle, i);
                if (slot.IsNoOp || !EvaluatePredicate(slot.Instruction.PredicateIndex)) continue;

                if (slot.Instruction.Opcode.IsMemoryOp())
                {
                    if (memorySlot == -1) memorySlot = i;
                }
                if (slot.Instruction.Opcode.IsBranchOp())
                {
                    if (branchSlot == -1) branchSlot = i;
                }
            }

            for (int i = 0; i < 3; i++)
            {
                VliwSlot slot = GetSlot(bundle, i);
                if (slot.IsNoOp || !EvaluatePredicate(slot.Instruction.PredicateIndex)) continue;

                if (slot.Instruction.Opcode.IsMemoryOp() && memorySlot != i)
                {
                    IncrementStalls();
                    IncrementCycles(1);
                    return true; 
                }

                if (slot.Instruction.Opcode.IsBranchOp())
                {
                    if (branchSlot != i) continue;
                    hasBranch = true;
                }

                ExecuteVliwInstruction(slot.Instruction);
            }

            IncrementInstructions();
            if (!hasBranch) PC++;
            IncrementCycles(1); 
            return true;
        }

        private VliwSlot GetSlot(VliwBundle bundle, int index) => index switch
        {
            0 => bundle.Slot0,
            1 => bundle.Slot1,
            2 => bundle.Slot2,
            _ => throw new ArgumentOutOfRangeException()
        };

        private bool HasRegisterConflict(VliwBundle bundle)
        {
            int[] writeRegs = new int[3];
            bool[] writes = new bool[3];

            for (int i = 0; i < 3; i++)
            {
                VliwSlot slot = GetSlot(bundle, i);
                if (!slot.IsNoOp && EvaluatePredicate(slot.Instruction.PredicateIndex))
                {
                    if (slot.Instruction.Opcode.WritesToRegister())
                    {
                        writeRegs[i] = (int)slot.Instruction.Operand1;
                        writes[i] = true;
                    }
                }
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = i + 1; j < 3; j++)
                {
                    if (writes[i] && writes[j] && writeRegs[i] == writeRegs[j])
                        return true;
                }
            }
            return false;
        }

        private bool EvaluatePredicate(int predIndex)
        {
            if (predIndex == 0) return true;
            return GetPredicateFlag(predIndex) == 1;
        }

        private int GetPredicateFlag(int predIndex)
        {
            string prStr = new Word27(PR).ToTritString();
            int start = (predIndex - 1) * 3;
            string flag = prStr.Substring(start, 3);
            return (int)BalancedTernary.ParseToLong(flag);
        }

        private void ExecuteVliwInstruction(Instruction instr)
        {
            int op1 = (int)instr.Operand1;
            int op2 = (int)instr.Operand2;

            switch (instr.Opcode)
            {
                case Opcode.HALT: IsHalted = true; break;
                case Opcode.LI: SetRegisterValue(op1, instr.Operand2); break;
                case Opcode.LIMM:
                    PC++;
                    SetRegisterValue(op1, ReadWord(PC));
                    break;
                case Opcode.MOV: SetRegisterValue(op1, GetRegisterValue(op2)); break;
                case Opcode.ADD:
                case Opcode.SUB:
                case Opcode.MUL:
                case Opcode.DIV:
                case Opcode.MOD:
                    SetRegisterValue(op1, T3Alu.Execute(instr.Opcode, GetRegisterValue(op1), GetRegisterValue(op2), Config));
                    break;
                case Opcode.NEG: SetRegisterValue(op1, -GetRegisterValue(op1)); break;
                case Opcode.CMP: Cond = T3Alu.Compare(GetRegisterValue(op1), GetRegisterValue(op2)); break;
                case Opcode.JMP: PC = (long)GetRegisterValue(op1); break;
                case Opcode.JE: if (Cond == 0) PC = (long)GetRegisterValue(op1); else PC++; break;
                case Opcode.JNE: if (Cond != 0) PC = (long)GetRegisterValue(op1); else PC++; break;
                case Opcode.JL: if (Cond < 0) PC = (long)GetRegisterValue(op1); else PC++; break;
                case Opcode.JG: if (Cond > 0) PC = (long)GetRegisterValue(op1); else PC++; break;
                case Opcode.JM: if (Cond == 0) PC = (long)GetRegisterValue(op1); else PC++; break;
                case Opcode.CALL:
                    SP -= 2;
                    WriteWord(SP, PC + 1);
                    WriteWord(SP + 1, WP);
                    WP = RegisterWindow.CalculateNextWp(WP);
                    PC = (long)GetRegisterValue(op1);
                    break;
                case Opcode.RET:
                    PC = (long)ReadWord(SP);
                    WP = (long)ReadWord(SP + 1);
                    SP += 2;
                    break;
                case Opcode.PUSH: SP--; WriteWord(SP, GetRegisterValue(op1)); break;
                case Opcode.POP: SetRegisterValue(op1, ReadWord(SP)); SP++; break;
                case Opcode.IN: SetRegisterValue(op1, DeviceManager.Read((long)GetRegisterValue(op2))); break;
                case Opcode.OUT: DeviceManager.Write((long)GetRegisterValue(op2), GetRegisterValue(op1)); break;
                case Opcode.INI: SetRegisterValue(op1, DeviceManager.Read(instr.Operand2)); break;
                case Opcode.OUTI: DeviceManager.Write(instr.Operand2, GetRegisterValue(op1)); break;
                case Opcode.SPEK:
                    _isSpeculating = true;
                    _shadowRegisters = (BigInteger[])Registers.Clone();
                    break;
                case Opcode.COMMIT:
                    _isSpeculating = false;
                    _shadowRegisters = null;
                    break;
                case Opcode.ROLLBACK:
                    if (_isSpeculating)
                    {
                        Registers = (BigInteger[])_shadowRegisters.Clone();
                        _isSpeculating = false;
                        _shadowRegisters = null;
                    }
                    break;
                case Opcode.VADD3:
                case Opcode.VSUB3:
                case Opcode.VMUL3:
                case Opcode.VDOT3:
                case Opcode.VCMP:
                case Opcode.VTRITAND3:
                case Opcode.VTRITOR3:
                case Opcode.VTRITXOR3:
                case Opcode.VSHL3:
                case Opcode.VSHR3:
                    ExecuteSimdInstruction(instr);
                    break;
                default:
                    throw new InvalidOperationException($"Instruction {instr.Opcode} not implemented in VLIW.");
            }
        }

        private void ExecuteSimdInstruction(Instruction instr)
        {
            // SIMD implementation for T3-54: Word54 is split into 3 segments of 18 trits each.
            BigInteger valA = GetRegisterValue((int)instr.Operand1);
            BigInteger valB = (instr.Operand2 < 27) ? GetRegisterValue((int)instr.Operand2) : instr.Operand2;

            // Convert to trit arrays for segment processing
            Trit[] tritsA = WordToTritArray(valA, 54);
            Trit[] tritsB = WordToTritArray(valB, 54);
            Trit[] resultTrits = new Trit[54];

            for (int seg = 0; seg < 3; seg++)
            {
                int offset = seg * 18;
                Trit[] segA = new Trit[18];
                Trit[] segB = new Trit[18];
                for (int i = 0; i < 18; i++)
                {
                    segA[i] = tritsA[offset + i];
                    segB[i] = tritsB[offset + i];
                }

                Trit[] resSeg = new Trit[18];
                switch (instr.Opcode)
                {
                    case Opcode.VADD3:
                        // Simple addition for SIMD segments (simplified)
                        resSeg = AddTritArrays(segA, segB);
                        break;
                    case Opcode.VSUB3:
                        resSeg = SubTritArrays(segA, segB);
                        break;
                    case Opcode.VTRITAND3:
                        resSeg = TritArray.And(segA, segB);
                        break;
                    case Opcode.VTRITOR3:
                        resSeg = TritArray.Or(segA, segB);
                        break;
                    case Opcode.VTRITXOR3:
                        resSeg = TritArray.Xor(segA, segB);
                        break;
                    case Opcode.VCMP:
                        // VCMP writes to PR. We handle it specially.
                        int cmpRes = CompareTritArrays(segA, segB);
                        SetPredicateBit(seg, cmpRes);
                        resSeg = segA; // No change to register
                        break;
                    default:
                        resSeg = segA;
                        break;
                }

                for (int i = 0; i < 18; i++) resultTrits[offset + i] = resSeg[i];
            }

            if (instr.Opcode != Opcode.VCMP)
            {
                SetRegisterValue((int)instr.Operand1, TritArrayToWord(resultTrits));
            }
        }

        private Trit[] WordToTritArray(BigInteger val, int length)
        {
            string s = new Word54(val).ToTritString();
            return TritArray.FromString(s);
        }

        private BigInteger TritArrayToWord(Trit[] trits)
        {
            return BigInteger.Parse(TritArray.ToString(trits)); // Simplified; should use BalancedTernary.Parse
        }

        private Trit[] AddTritArrays(Trit[] a, Trit[] b)
        {
            Trit[] res = new Trit[a.Length];
            int carry = 0;
            for (int i = 0; i < a.Length; i++)
            {
                int sum = a[i].Value + b[i].Value + carry;
                if (sum > 1) { res[i] = Trit.FromInt(sum - 3); carry = 1; }
                else if (sum < -1) { res[i] = Trit.FromInt(sum + 3); carry = -1; }
                else { res[i] = Trit.FromInt(sum); carry = 0; }
            }
            return res;
        }

        private Trit[] SubTritArrays(Trit[] a, Trit[] b)
        {
            Trit[] negB = new Trit[b.Length];
            for (int i = 0; i < b.Length; i++) negB[i] = -b[i];
            return AddTritArrays(a, negB);
        }

        private int CompareTritArrays(Trit[] a, Trit[] b)
        {
            for (int i = a.Length - 1; i >= 0; i--)
            {
                if (a[i].Value > b[i].Value) return 1;
                if (a[i].Value < b[i].Value) return -1;
            }
            return 0;
        }

        private void SetPredicateBit(int segment, int value)
        {
            // Update PR register for the given segment (3 trits)
            // Simplified implementation
            PR += (value * (BigInteger)Math.Pow(3, segment * 18)); 
        }

        private BigInteger GetRegisterValue(int logicalIndex) => Registers[RegisterWindow.GetPhysicalIndex(logicalIndex, WP)];
        private void SetRegisterValue(int logicalIndex, BigInteger value) => Registers[RegisterWindow.GetPhysicalIndex(logicalIndex, WP)] = value;
    }
}