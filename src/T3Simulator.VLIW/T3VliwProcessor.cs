using System;
using System.Collections.Generic;
using System.Numerics;
using TritTypes;
using T3Simulator.Common;

namespace T3Simulator.VLIW
{
    /// <summary>
    /// High-performance VLIW implementation of the T3 processor.
    /// Executes bundles of 3 instructions in parallel with conflict detection.
    /// </summary>
    public class T3VliwProcessor<TWord> : ProcessorBase<TWord> where TWord : IT3Word<TWord>
    {
        private TWord[] _shadowRegisters;
        private bool _isSpeculating;

        public T3VliwProcessor(T3Config config) : base(config)
        {
        }

        public override bool Step()
        {
            if (IsHalted) return false;

            // 1. Fetch Bundle
            TWord currentWord = ReadWord(PC);
            VliwBundle<TWord> bundle = VliwBundle<TWord>.Decode(currentWord);

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
                VliwSlot<TWord> slot = GetSlot(bundle, i);
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
                VliwSlot<TWord> slot = GetSlot(bundle, i);
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

        private VliwSlot<TWord> GetSlot(VliwBundle<TWord> bundle, int index) => index switch
        {
            0 => bundle.Slot0,
            1 => bundle.Slot1,
            2 => bundle.Slot2,
            _ => throw new ArgumentOutOfRangeException()
        };

        private bool HasRegisterConflict(VliwBundle<TWord> bundle)
        {
            int[] writeRegs = new int[3];
            bool[] writes = new bool[3];

            for (int i = 0; i < 3; i++)
            {
                VliwSlot<TWord> slot = GetSlot(bundle, i);
                if (!slot.IsNoOp && EvaluatePredicate(slot.Instruction.PredicateIndex))
                {
                        if (slot.Instruction.Opcode.WritesToRegister())
                        {
                            writeRegs[i] = (int)slot.Instruction.Operand1.ToInt128();
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
            string prStr = PR.ToTritString();
            // Predicates are stored in the lowest 27 trits.
            // String is MSB -> LSB. Low trits are at the end.
            int start = prStr.Length - (predIndex * 3);
            string flag = prStr.Substring(start, 3);
            return (int)BalancedTernary.ParseToLong(flag);
        }

        private void ExecuteVliwInstruction(Instruction<TWord> instr)
        {
            int op1 = (int)instr.Operand1.ToInt128();
            int op2 = (int)instr.Operand2.ToInt128();

            switch (instr.Opcode)
            {
                case Opcode.HALT: IsHalted = true; break;
                case Opcode.LOAD: SetRegisterValue(op1, ReadWord((long)GetRegisterValue(op2).ToInt128())); break;
                case Opcode.STORE: WriteWord((long)GetRegisterValue(op2).ToInt128(), GetRegisterValue(op1)); break;
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
                case Opcode.NEG: SetRegisterValue(op1, GetRegisterValue(op1).Negate()); break;
                case Opcode.CMP: Cond = T3Alu.Compare(GetRegisterValue(op1), GetRegisterValue(op2)); break;
                case Opcode.TRITAND: SetRegisterValue(op1, T3Alu.TritAnd(GetRegisterValue(op1), GetRegisterValue(op2))); break;
                case Opcode.TRITOR: SetRegisterValue(op1, T3Alu.TritOr(GetRegisterValue(op1), GetRegisterValue(op2))); break;
                case Opcode.TRITXOR: SetRegisterValue(op1, T3Alu.TritXor(GetRegisterValue(op1), GetRegisterValue(op2))); break;
                case Opcode.SHL: SetRegisterValue(op1, T3Alu.ShiftLeft(GetRegisterValue(op1), (int)GetRegisterValue(op2).ToInt128())); break;
                case Opcode.SHR: SetRegisterValue(op1, T3Alu.ShiftRight(GetRegisterValue(op1), (int)GetRegisterValue(op2).ToInt128())); break;
                case Opcode.JMP: PC = (long)GetRegisterValue(op1).ToInt128(); break;
                case Opcode.JE: if (Cond == 0) PC = (long)GetRegisterValue(op1).ToInt128(); else PC++; break;
                case Opcode.JNE: if (Cond != 0) PC = (long)GetRegisterValue(op1).ToInt128(); else PC++; break;
                case Opcode.JL: if (Cond < 0) PC = (long)GetRegisterValue(op1).ToInt128(); else PC++; break;
                case Opcode.JG: if (Cond > 0) PC = (long)GetRegisterValue(op1).ToInt128(); else PC++; break;
                case Opcode.JM: if (Cond == 0) PC = (long)GetRegisterValue(op1).ToInt128(); else PC++; break;
                case Opcode.CALL:
                    SP -= 2;
                    WriteWord(SP, FromLong(PC + 1));
                    WriteWord(SP + 1, FromLong(WP));
                    WP = (int)RegisterWindow.CalculateNextWp(WP);
                    PC = (long)GetRegisterValue(op1).ToInt128();
                    break;
                case Opcode.RET:
                    PC = (long)ReadWord(SP).ToInt128();
                    WP = (int)ReadWord(SP + 1).ToInt128();
                    SP += 2;
                    break;
                case Opcode.PUSH: SP--; WriteWord(SP, GetRegisterValue(op1)); break;
                case Opcode.POP: SetRegisterValue(op1, ReadWord(SP)); SP++; break;
                case Opcode.IN: SetRegisterValue(op1, DeviceManager.Read((long)GetRegisterValue(op2).ToInt128())); break;
                case Opcode.OUT: DeviceManager.Write((long)GetRegisterValue(op2).ToInt128(), GetRegisterValue(op1)); break;
                case Opcode.INI: SetRegisterValue(op1, DeviceManager.Read((long)instr.Operand2.ToInt128())); break;
                case Opcode.OUTI: DeviceManager.Write((long)instr.Operand2.ToInt128(), GetRegisterValue(op1)); break;
                case Opcode.SPEK:
                    _isSpeculating = true;
                    _shadowRegisters = (TWord[])Registers.Clone();
                    break;
                case Opcode.COMMIT:
                    _isSpeculating = false;
                    _shadowRegisters = null;
                    break;
                case Opcode.ROLLBACK:
                    if (_isSpeculating)
                    {
                        Registers = (TWord[])_shadowRegisters.Clone();
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

        private void ExecuteSimdInstruction(Instruction<TWord> instr)
        {
            TWord valA = GetRegisterValue((int)instr.Operand1.ToInt128());
            TWord valB = (instr.Operand2.ToInt128() < 9) ? GetRegisterValue((int)instr.Operand2.ToInt128()) : instr.Operand2;

            Trit[] tritsA = WordToTritArray(valA);
            Trit[] tritsB = WordToTritArray(valB);
            Trit[] resultTrits = new Trit[tritsA.Length];

            int segmentSize = tritsA.Length / 3;

            for (int seg = 0; seg < 3; seg++)
            {
                int offset = seg * segmentSize;
                Trit[] segA = new Trit[segmentSize];
                Trit[] segB = new Trit[segmentSize];
                for (int i = 0; i < segmentSize; i++)
                {
                    segA[i] = tritsA[offset + i];
                    segB[i] = tritsB[offset + i];
                }

                Trit[] resSeg = new Trit[segmentSize];
                switch (instr.Opcode)
                {
                    case Opcode.VADD3:
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
                        int cmpRes = CompareTritArrays(segA, segB);
                        SetPredicateBit(seg, cmpRes);
                        resSeg = segA;
                        break;
                    default:
                        resSeg = segA;
                        break;
                }

                for (int i = 0; i < segmentSize; i++) resultTrits[offset + i] = resSeg[i];
            }

            if (instr.Opcode != Opcode.VCMP)
            {
                SetRegisterValue((int)instr.Operand1.ToInt128(), TritArrayToWord(resultTrits));
            }
        }

        private Trit[] WordToTritArray(TWord val)
        {
            string s = val.ToTritString();
            return TritArray.FromString(s);
        }

        private TWord TritArrayToWord(Trit[] trits)
        {
            Int128 val = BalancedTernary.ParseToInt128(TritArray.ToString(trits));
            return TWord.FromInt128(val);
        }

        private Trit[] AddTritArrays(Trit[] a, Trit[] b)
        {
            Trit[] res = new Trit[a.Length];
            int carry = 0;
            for (int i = a.Length - 1; i >= 0; i--)
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
            // Use the existing Word <-> TritArray conversion to modify the flag
            Trit[] trits = WordToTritArray(PR);
            
            // Predicates p1..p8 are stored in the lowest 24 trits of PR.
            // Segment 0 -> p1, Segment 1 -> p2, Segment 2 -> p3.
            // Each flag is 3 trits.
            int offset = trits.Length - (segment + 1) * 3;
            
            // Store the value (-1, 0, 1) in the lowest trit of the 3-trit flag.
            trits[offset] = Trit.FromInt(0);
            trits[offset + 1] = Trit.FromInt(0);
            trits[offset + 2] = Trit.FromInt(value);
            
            PR = TritArrayToWord(trits);
        }

        private TWord GetRegisterValue(int logicalIndex) => Registers[RegisterWindow.GetPhysicalIndex(logicalIndex, WP)];
        private void SetRegisterValue(int logicalIndex, TWord value) => Registers[RegisterWindow.GetPhysicalIndex(logicalIndex, WP)] = value;
    }
}