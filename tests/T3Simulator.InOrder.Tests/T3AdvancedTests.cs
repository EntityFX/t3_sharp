using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Simulator.Common;
using T3Simulator.InOrder;
using System.Collections.Generic;
using TritTypes;
using System;
using T3Assembler;

namespace T3Simulator.InOrder.Tests
{
    [TestClass]
    public class T3AdvancedTests
    {
        private Word18 Encode(int opcode, int op1, long op2, int pred = 0)
        {
            // New ISA: [Opcode+Pred (6)] [Op1 (3)] [Op2 (3)] [Op3/Imm6 (3/6)] [Reserve (3)]
            int v = pred * 28 + opcode;
            string sOp = ToBalancedTernary(v, 6);
            string sOp1 = ToBalancedTernary(op1, 3);
            string sOp2 = ToBalancedTernary(op2, 3);
            
            // For these tests, we assume R-type unless specified.
            // For simplicity in this helper, we use 3 trits for op3 and 3 trits for reserve.
            string sOp3 = "000"; 
            string sRes = "000";
            
            return Word18.FromLong(BalancedTernary.ParseToLong(sOp + sOp1 + sOp2 + sOp3 + sRes));
        }

        private string ToBalancedTernary(long value, int digits)
        {
            string s = TritTypes.BalancedTernary.ToTernaryString(value);
            if (s.Length > digits) s = s.Substring(s.Length - digits);
            if (s.Length < digits) s = s.PadLeft(digits, '0');
            return s;
        }

        private T3InOrderProcessor<Word18> CreateProcessor()
        {
            return new T3InOrderProcessor<Word18>(T3Config.T3_18);
        }

        private Word54 EncodeInt128(int opcode, int op1, Int128 op2, int pred = 0)
        {
            // New ISA: [Opcode+Pred (6)] [Op1 (3)] [Op2 (3)] [Imm6 (6)]
            int v = pred * 28 + opcode;
            string sOp = ToBalancedTernary(v, 6);
            string sOp1 = ToBalancedTernary(op1, 3);
            string sOp2 = ToBalancedTernary(0, 3); // Not used for these Int128 tests typically
            
            // Immediate value for T3-54 could be larger, but the instruction format
            // for I-type in the spec says Imm6. 
            // However, these tests might be testing larger words.
            // Given the current T3InOrderAssembler and Decoder, we follow the 18-trit format.
            string sImm = TritTypes.BalancedTernary.ToTernaryString((long)op2);
            if (sImm.Length > 6) sImm = sImm.Substring(sImm.Length - 6);
            if (sImm.Length < 6) sImm = sImm.PadLeft(6, '0');

            string instruction = sOp + sOp1 + sOp2 + sImm;
            string word = instruction.PadRight(54, '0');
            return Word54.FromInt128(TritTypes.BalancedTernary.ParseToInt128(word));
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_ArrayAddition()
        {
            var proc = CreateProcessor();
            var assembler = new T3InOrderAssembler(T3Config.T3_18);

            // Arrays: A[0..2] = {1, 2, 3}, B[0..2] = {4, 5, 6}
            // Result C[0..2] = {5, 7, 9}
            // Addresses: A=100, B=110, C=120
            string asm = @"
                LI RW, 100
                LI RX, 110
                LI RY, 120
                LI RZ, 3
                
                LI R0, 1
                STOREI R0, RW, 0
                LI R0, 2
                STOREI R0, RW, 1
                LI R0, 3
                STOREI R0, RW, 2
                
                LI R0, 4
                STOREI R0, RX, 0
                LI R0, 5
                STOREI R0, RX, 1
                LI R0, 6
                STOREI R0, RX, 2
                
                LI R4, 0
                loop:
                    LOAD R0, RW
                    LOAD R1, RX
                    ADD R2, R0, R1
                    STOREI R2, RY, R4
                    ADDI R4, R4, 1
                    ADDI RW, RW, 1
                    ADDI RX, RX, 1
                    CMP R4, RZ
                    JE end
                    JMP loop
                end:
                    HALT
            ";
            
            var program = assembler.Assemble(asm).Select(x => Word18.FromLong((long)x)).ToList();
            proc.LoadProgram(program);
            proc.Run();
            
            proc.Reset();
            
            string verifyAsm = @"
                LI RW, 120
                LOAD R1, RW
                LI RW, 121
                LOAD R2, RW
                LI RW, 122
                LOAD R3, RW
                HALT
            ";
            var verifyProg = assembler.Assemble(verifyAsm).Select(x => Word18.FromLong((long)x)).ToList();
            proc.LoadProgram(verifyProg);
            proc.Run();
            
            Assert.AreEqual(5, proc.GetState().Registers[1]);
            Assert.AreEqual(7, proc.GetState().Registers[2]);
            Assert.AreEqual(9, proc.GetState().Registers[3]);
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_ProcedureCall_WithStack()
        {
            var proc = CreateProcessor();
            var assembler = new T3InOrderAssembler(T3Config.T3_18);

            string asm = @"
                LI RW, 5
                LI RX, func
                CALL RX
                HALT
            func:
                LI RY, 2
                MUL RW, RW, RY
                LI RY, 1
                ADD RW, RW, RY
                RET
            ";
            
            var program = assembler.Assemble(asm).Select(x => Word18.FromLong((long)x)).ToList();
            proc.LoadProgram(program);
            proc.Run();
            
            Assert.AreEqual(11, proc.GetState().Registers[0]);
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_T3_54_Int128()
        {
            // Test with T3-54 and Int128
            var proc = new T3InOrderProcessor<Word54>(T3Config.T3_54);
            var assembler = new T3InOrderAssembler(T3Config.T3_54);
            
            string asm = @"
                LIMM RW, 3486784401
                LIMM RX, 2
                MUL RW, RW, RX
                HALT
            ";
            
            var program = assembler.Assemble(asm).Select(x => Word54.FromInt128(x)).ToList();
            proc.LoadProgram(program);
            proc.Run();
            
            Assert.AreEqual((Int128)3486784401 * 2, proc.GetState().Registers[0].ToInt128());
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_NestedBranching()
        {
            var proc = CreateProcessor();
            var assembler = new T3InOrderAssembler(T3Config.T3_18);
            
            // if (A > 0) { if (B > 0) C = 1 else C = 2 } else C = 3
            string asm = @"
                LI A, 1
                LI B, 1
                LI C, 0
                LI D, 0
                LI E, nested
                CMP A, D
                JG E
                LI C, 3
                LI F, end
                JMP F
            nested:
                LI E, set1
                CMP B, D
                JG E
                LI C, 2
                LI F, end
                JMP F
            set1:
                LI C, 1
            end:
                HALT
            ";
            
            var program = assembler.Assemble(asm).Select(x => Word18.FromLong((long)x)).ToList();
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(1, proc.GetState().Registers[2]); // A=1, B=1 -> C=1
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_DoubleLoop()
        {
            var proc = CreateProcessor();
            var assembler = new T3InOrderAssembler(T3Config.T3_18);
            
            // Double loop to calculate sum of i+j for i=0..2, j=0..2
            // res = sum(i+j) = 0+0 + 0+1 + 0+2 + 1+0 + 1+1 + 1+2 + 2+0 + 2+1 + 2+2 = 18
            string asm = @"
                LI res, 0
                LI i, 0
                LI limit, 3
                LI one, 1
                
            loop_i:
                LI j, 0
            loop_j:
                ADD temp, i
                ADD temp, j
                ADD res, temp
                ADD j, one
                CMP j, limit
                JE end_j
                JMP loop_j
            end_j:
                ADD i, one
                CMP i, limit
                JE end_i
                JMP loop_i
            end_i:
                HALT
            ";
            // I need to map logical registers to indices for the assembler if it doesn't do it.
            // Wait, T3AssemblerCore.IsRegister checks "ABCDEFGHI". 
            // I should use registers A-I.
            
            string asmFixed = @"
                LI A, 0       ; A = res
                LI B, 0       ; B = i
                LI C, 3       ; C = limit
                LI D, 1       ; D = one
                
            loop_i:
                LI E, 0       ; E = j
            loop_j:
                MOV F, B      ; F = i
                ADD F, E      ; F = i + j
                ADD A, F      ; res += F
                ADD E, D      ; j++
                LI G, end_j
                CMP E, C      ; j == limit?
                JE G
                LI H, loop_j
                JMP H
            end_j:
                ADD B, D      ; i++
                LI G, end_i
                CMP B, C      ; i == limit?
                JE G
                LI H, loop_i
                JMP H
            end_i:
                HALT
            ";
            
            var program = assembler.Assemble(asmFixed).Select(x => Word18.FromLong((long)x)).ToList();
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(18, proc.GetState().Registers[0]);
        }
    }
}
