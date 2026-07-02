using System;
using System.Collections.Generic;
using TritTypes;
using T3Simulator.Common;

namespace T3Simulator.InOrder
{
    public class T3InOrderProcessor<TWord> : ProcessorBase<TWord> where TWord : IT3Word<TWord>
    {
        private System.IO.TextWriter? _traceWriter;
        public string? LastError { get; private set; }

        public void EnableTracing(System.IO.TextWriter writer) => _traceWriter = writer;
        private void LogTrace(string message) => _traceWriter?.WriteLine(message);

        private void LogFullState(DecodedInstruction instr, TWord currentWord)
        {
            if (_traceWriter == null) return;
            string regs = "";
            for (int i = 0; i < 5; i++)
                regs += $" R{i}:{Registers[i].ToLong()}";
            regs += $" SP:{SP} FP:{FP} CD:{CD}";
            _traceWriter.WriteLine($"[PC:{PC:D4}] {instr.Opcode} | {regs}");
        }

        public string DumpState()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"--- Processor State ---");
            sb.AppendLine($"PC: {PC}  SP: {SP}  FP: {FP}  HP: {HP}  CD: {CD}  WD: {WD}");
            sb.AppendLine($"Cycles: {CycleCount}, Instructions: {InstructionCount}");
            sb.AppendLine("GP Registers:");
            for (int i = 0; i < 9; i++)
                sb.AppendLine($"  R{i}: {Registers[i].ToLong()}");
            sb.AppendLine("FPU Registers:");
            for (int i = 0; i < 9; i++)
                sb.AppendLine($"  F{i}: {T3Fpu.ToDoublePrecision(FRegisters[i])}");
            sb.Append("PR: ");
            for (int i = 0; i < 18; i++) sb.Append($"{PR.GetTrit(i)}");
            sb.AppendLine();
            sb.AppendLine("Memory (around SP):");
            for (long i = SP - 16; i <= SP + 16; i++)
            {
                try { sb.AppendLine($"  [{i:D4}]: {ReadWord(i).ToLong()}"); }
                catch { sb.AppendLine($"  [{i:D4}]: <error>"); }
            }
            return sb.ToString();
        }

        public T3InOrderProcessor(T3Config config) : base(config) { LastError = null; }

        public override bool Step()
        {
            if (IsHalted) return false;
            TWord currentWord = ReadWord(PC);
            DecodedInstruction instr = InstructionDecoder.Decode(currentWord);
            LogFullState(instr, currentWord);
            LogTrace($"PC:{PC:D4} Op:{instr.Opcode} Pred:{instr.Predicate} RG:{instr.RegGroup} Fmt:{instr.Fmt}");

            if (!EvaluatePredicate(instr.Predicate))
            {
                IncrementCycles(1);
                PC += (instr.Opcode == Opcode.LIMM) ? 2 : 1;
                return true;
            }

            bool jumped = false;
            try { jumped = ExecuteInstruction(instr); }
            catch (DeviceStallException) { IncrementStalls(); IncrementCycles(1); return true; }
            catch (Exception ex) { Console.WriteLine($"Exception at PC {PC}: {ex.Message}"); IsHalted = true; return false; }

            IncrementInstructions();
            if (!jumped) PC++;
            return true;
        }

        private bool EvaluatePredicate(int predIndex)
        {
            if (predIndex == 0) return true;
            if (predIndex < 0 || predIndex > 3) return false;
            int targetTrit = predIndex - 1;
            if (targetTrit < 0 || targetTrit >= 18) return false;
            return PR.GetTrit(targetTrit) != 0;
        }

        // ===== Register access by RegGroup =====

        private TWord GetRegValue(int regGroup, int opRaw)
        {
            int idx = opRaw + 4; // balanced trit (-4..+4) → 0..8
            switch (regGroup)
            {
                case -1: // FPU
                    if (idx < 0 || idx >= 9) throw new IndexOutOfRangeException($"FPU idx {idx}");
                    return (TWord)(object)FRegisters[idx].ToWord18();
                case +1: // Special
                    return FromLong(GetSpecialReg(idx));
                default: // GP
                    return GetRegisterValue(idx);
            }
        }

        private void SetRegValue(int regGroup, int opRaw, TWord value)
        {
            int idx = opRaw + 4;
            switch (regGroup)
            {
                case -1:
                    if (idx < 0 || idx >= 9) throw new IndexOutOfRangeException($"FPU idx {idx}");
                    FRegisters[idx] = T3Float.FromWord18((Word18)(object)value);
                    break;
                case +1:
                    SetSpecialReg(idx, ToLong(value));
                    break;
                default:
                    SetRegisterValue(idx, value);
                    break;
            }
        }

        private T3Float GetFReg(int opRaw)
        {
            int idx = opRaw + 4;
            if (idx < 0 || idx >= 9) throw new IndexOutOfRangeException($"FPU idx {idx}");
            return FRegisters[idx];
        }

        private void SetFReg(int opRaw, T3Float value)
        {
            int idx = opRaw + 4;
            if (idx < 0 || idx >= 9) throw new IndexOutOfRangeException($"FPU idx {idx}");
            FRegisters[idx] = value;
        }

        private long GetSpecialReg(int idx) => idx switch
        {
            0 => FP, 1 => HP, 2 => SP, 3 => CD, 4 => (long)PR.ToLong(), 5 => WD,
            _ => throw new IndexOutOfRangeException($"Special idx {idx}")
        };

        private void SetSpecialReg(int idx, long value)
        {
            switch (idx)
            {
                case 0: FP = value; break;
                case 1: HP = value; break;
                case 2: SP = value; break;
                case 3: CD = (int)value; break;
                case 4: PR = FromLong(value); break;
                case 5: WD = (int)value; break;
                default: throw new IndexOutOfRangeException($"Special idx {idx}");
            }
        }

        // ===== Execute =====

        private bool ExecuteInstruction(DecodedInstruction instr)
        {
            int rg = instr.RegGroup;
            int fmt = instr.Fmt;

            switch (instr.Opcode)
            {
                case Opcode.HALT:
                    IsHalted = true; IncrementCycles(1); return false;
                case Opcode.NOP:
                    IncrementCycles(1); return false;

                case Opcode.LIMM:
                    PC++;
                    SetRegValue(rg, instr.Op1, ReadWord(PC));
                    IncrementCycles(2); return false;

                case Opcode.MOV:
                    if (fmt == 0)
                        SetRegValue(rg, instr.Op1, GetRegValue(rg, instr.Op2));
                    else
                        SetRegValue(rg, instr.Op1, FromLong(instr.Immediate));
                    IncrementCycles(1); return false;

                case Opcode.LD:
                    {
                        long baseAddr = ToLong(GetRegValue(0, instr.Op2)) + instr.Immediate;
                        TWord memVal = ReadWord(baseAddr);
                        if (rg == -1)
                            SetFReg(instr.Op1, T3Float.FromWord18((Word18)(object)memVal));
                        else
                            SetRegValue(rg, instr.Op1, memVal);
                        IncrementCycles(2); return false;
                    }

                case Opcode.ST:
                    {
                        long baseAddr = ToLong(GetRegValue(0, instr.Op2)) + instr.Immediate;
                        if (rg == -1)
                            WriteWord(baseAddr, (TWord)(object)GetFReg(instr.Op1).ToWord18());
                        else
                            WriteWord(baseAddr, GetRegValue(rg, instr.Op1));
                        IncrementCycles(2); return false;
                    }

                case Opcode.PUSH:
                    SP--;
                    if (SP < 64) { IsHalted = true; return false; }
                    if (fmt == 0) WriteWord(SP, GetRegValue(rg, instr.Op1));
                    else WriteWord(SP, FromLong(instr.Immediate));
                    IncrementCycles(2); return false;

                case Opcode.POP:
                    if (fmt == 0) SetRegValue(rg, instr.Op1, ReadWord(SP));
                    else SetRegValue(rg, instr.Op1, ReadWord(SP + instr.Immediate));
                    SP++;
                    IncrementCycles(2); return false;

                // === ALU: R-type (fmt=0) or I-type (fmt=+1) ===
                case Opcode.ADD:
                case Opcode.SUB:
                case Opcode.MUL:
                case Opcode.DIV:
                case Opcode.MOD:
                case Opcode.AND:
                case Opcode.OR:
                case Opcode.XOR:
                case Opcode.SHL:
                case Opcode.SHR:
                    {
                        TWord a, b;
                        if (fmt == 0) { a = GetRegValue(rg, instr.Op2); b = GetRegValue(rg, instr.Op3); }
                        else { a = GetRegValue(rg, instr.Op1); b = FromLong(instr.Immediate); }
                        TWord res;
                        if (instr.Opcode == Opcode.SHL || instr.Opcode == Opcode.SHR)
                        {
                            if (fmt == 0) res = instr.Opcode == Opcode.SHL
                                ? T3Alu.ShiftLeft(a, (int)b.ToInt128())
                                : T3Alu.ShiftRight(a, (int)b.ToInt128());
                            else res = instr.Opcode == Opcode.SHL
                                ? T3Alu.ShiftLeft(a, (int)instr.Immediate)
                                : T3Alu.ShiftRight(a, (int)instr.Immediate);
                        }
                        else
                        {
                            res = T3Alu.Execute(instr.Opcode, a, b, Config);
                        }
                        if (fmt == 0) SetRegValue(rg, instr.Op1, res);
                        else SetRegValue(rg, instr.Op1, res);
                        IncrementCycles(instr.Opcode switch
                        {
                            Opcode.ADD or Opcode.SUB => 1,
                            Opcode.MUL => Config == T3Config.T3_18 ? 5 : 8,
                            Opcode.DIV or Opcode.MOD => Config == T3Config.T3_18 ? 10 : 15,
                            _ => 1
                        });
                        return false;
                    }

                case Opcode.NEG:
                    if (fmt == 0)
                    {
                        TWord val = GetRegValue(rg, instr.Op2);
                        if (rg == -1) SetFReg(instr.Op1, T3Fpu.Neg(GetFReg(instr.Op2)));
                        else SetRegValue(rg, instr.Op1, (TWord)val.Negate());
                    }
                    else SetRegValue(rg, instr.Op1, FromLong(-instr.Immediate));
                    IncrementCycles(1); return false;

                case Opcode.ABS:
                    if (rg == -1) SetFReg(instr.Op1, T3Fpu.Abs(GetFReg(instr.Op2)));
                    else { var v = GetRegValue(rg, instr.Op2); SetRegValue(rg, instr.Op1, v); } // TODO: GP abs
                    IncrementCycles(1); return false;

                case Opcode.CMP:
                    {
                        if (rg == -1 && fmt == 0) // F.CMP
                        {
                            CD = T3Fpu.Compare(GetFReg(instr.Op1), GetFReg(instr.Op2));
                        }
                        else
                        {
                            TWord a = fmt == 0 ? GetRegValue(rg, instr.Op1) : GetRegValue(rg, instr.Op1);
                            TWord b = fmt == 0 ? GetRegValue(rg, instr.Op2) : FromLong(instr.Immediate);
                            CD = T3Alu.Compare(a, b);
                        }
                        IncrementCycles(1); return false;
                    }

                // === Control Flow ===
                case Opcode.JMP:
                    PC = (int)ToLong(GetRegValue(rg, instr.Op1));
                    IncrementCycles(1); return true;
                case Opcode.JE: return CondJump(0, instr);
                case Opcode.JNE: return CondJump(null, instr, neq: true);
                case Opcode.JL: return CondJump(null, instr, lt: true);
                case Opcode.JG: return CondJump(null, instr, gt: true);
                case Opcode.JLE: return CondJump(null, instr, le: true);
                case Opcode.JGE: return CondJump(null, instr, ge: true);
                case Opcode.JM: return CondJump(0, instr);

                case Opcode.CALL:
                    LogTrace($"[CALL] SP:{SP}→{SP-2} FP:{FP}→{SP-2}");
                    SP -= 2;
                    if (SP < 64) { LastError = "Stack overflow"; IsHalted = true; return false; }
                    WriteWord(SP + 1, FromLong(FP)); // save caller FP
                    WriteWord(SP, FromLong(PC + 1));  // ret addr
                    FP = SP;                          // new FP
                    PC = (int)ToLong(GetRegValue(rg, instr.Op1));
                    IncrementCycles(2); return true;

                case Opcode.RET:
                    {
                        TWord retAddr = ReadWord(SP);
                        TWord savedFp = ReadWord(SP + 1);
                        LogTrace($"[RET] SP:{SP}→{SP+2} FP:{FP}→{savedFp.ToLong()}");
                        PC = (int)ToLong(retAddr);
                        FP = ToLong(savedFp);
                        SP += 2;
                        IncrementCycles(2); return true;
                    }

                case Opcode.IN:
                    if (fmt == 0)
                    {
                        long port = ToLong(GetRegValue(rg, instr.Op2));
                        SetRegValue(rg, instr.Op1, DeviceManager.Read(port));
                    }
                    else SetRegValue(rg, instr.Op1, DeviceManager.Read(instr.Immediate));
                    IncrementCycles(2); return false;

                case Opcode.OUT:
                    if (fmt == 0)
                    {
                        long port = ToLong(GetRegValue(rg, instr.Op2));
                        DeviceManager.Write(port, GetRegValue(rg, instr.Op1));
                    }
                    else DeviceManager.Write(instr.Immediate, GetRegValue(rg, instr.Op1));
                    IncrementCycles(2); return false;

                // === FPU-specific ===
                case Opcode.SQRT:
                    SetFReg(instr.Op1, T3Fpu.Sqrt(GetFReg(instr.Op2)));
                    IncrementCycles(20); return false;

                case Opcode.FTI: // Float→Int (cross-group: Fop2 → Rop1)
                    {
                        long iv = T3Fpu.ToInt(GetFReg(instr.Op2), instr.Op3);
                        SetRegisterValue(instr.Op1 + 4, FromLong(iv));
                        IncrementCycles(3); return false;
                    }

                case Opcode.ITF: // Int→Float (cross-group: always read Op2 from GP)
                    SetFReg(instr.Op1, T3Fpu.FromInt(ToLong(GetRegValue(0, instr.Op2))));
                    IncrementCycles(3); return false;

                case Opcode.CLASS:
                    SetFReg(instr.Op1, T3Fpu.FromInt(T3Fpu.Classify(GetFReg(instr.Op2))));
                    IncrementCycles(1); return false;

                case Opcode.SWAP:
                    {
                        if (rg == -1)
                        {
                            T3Float t = GetFReg(instr.Op1);
                            SetFReg(instr.Op1, GetFReg(instr.Op2));
                            SetFReg(instr.Op2, t);
                        }
                        else
                        {
                            TWord t = GetRegValue(rg, instr.Op1);
                            SetRegValue(rg, instr.Op1, GetRegValue(rg, instr.Op2));
                            SetRegValue(rg, instr.Op2, t);
                        }
                        IncrementCycles(1); return false;
                    }

                default:
                    throw new InvalidOperationException($"Unsupported opcode: {instr.Opcode}");
            }
        }

        private bool CondJump(int? eq, DecodedInstruction instr, bool neq = false, bool lt = false, bool gt = false, bool le = false, bool ge = false)
        {
            bool take = false;
            if (eq.HasValue) take = CD == eq.Value;
            else if (neq) take = CD != 0;
            else if (lt) take = CD < 0;
            else if (gt) take = CD > 0;
            else if (le) take = CD <= 0;
            else if (ge) take = CD >= 0;

            if (take)
            {
                PC = (int)ToLong(GetRegValue(instr.RegGroup, instr.Op1));
                IncrementCycles(2); return true;
            }
            IncrementCycles(1); return false;
        }

        // ===== GP register helpers (backward-compat for PhysOp1/2/3) =====

        private TWord GetRegisterValue(int logicalIndex)
        {
            if (logicalIndex < 0 || logicalIndex >= 9) throw new IndexOutOfRangeException($"GP Register index {logicalIndex}");
            return Registers[logicalIndex];
        }

        private void SetRegisterValue(int logicalIndex, TWord value)
        {
            if (logicalIndex < 0 || logicalIndex >= 9) throw new IndexOutOfRangeException($"GP Register index {logicalIndex}");
            Registers[logicalIndex] = value;
        }
    }
}