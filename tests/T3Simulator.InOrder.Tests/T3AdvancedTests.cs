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
        private Word18 Encode(int opcode, int op1, long op2, long op3 = 0, int pred = 0)
        {
            // New ISA: [Opcode+Pred (6)] [Op1 (3)] [Op2 (3)] [Op3/Imm6 (3/6)] [Reserve (3)]
            int v = pred * 28 + opcode;
            string sOp = ToBalancedTernary(v, 6);
            string sOp1 = ToBalancedTernary(op1, 3);
            string sOp2 = ToBalancedTernary(op2, 3);
            
            string sOp3 = ToBalancedTernary(op3, 3);
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
            string word = instruction.PadLeft(54, '0');
            return Word54.FromInt128(TritTypes.BalancedTernary.ParseToInt128(word));
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
            // res = sum(i+j) = 18
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

        [TestMethod]
        [Timeout(30000)]
        public void Test_ArrayAddition()
        {
            var proc = CreateProcessor();
            var assembler = new T3InOrderAssembler(T3Config.T3_18);

            string asm = @"
                ; Initialize Array A: {1, 2, 3} at 100
                LI RX, 100
                LI RY, 1
                STORE RY, RX
                LI RX, 101
                LI RY, 2
                STORE RY, RX
                LI RX, 102
                LI RY, 3
                STORE RY, RX

                ; Initialize Array B: {4, 5, 6} at 110
                LI RX, 110
                LI RY, 4
                STORE RY, RX
                LI RX, 111
                LI RY, 5
                STORE RY, RX
                LI RX, 112
                LI RY, 6
                STORE RY, RX

                ; Array Addition Loop
                LI A, 0       ; i = 0
                LI B, 100     ; base A
                LI C, 110     ; base B
                LI D, 120     ; base C
                LI E, 3       ; limit = 3

            loop:
                MOV I, A
                ADD I, B
                LOAD F, I     ; F = A[i]
                
                MOV I, A
                ADD I, C
                LOAD G, I     ; G = B[i]
                
                ADD H, F, G   ; H = A[i] + B[i]
                
                MOV I, A
                ADD I, D
                STORE H, I    ; C[i] = H
                
                ADDI A, A, 1  ; i++
                CMP A, E
                LI G, end
                JE G
                LI G, loop
                JMP G
            end:
                HALT
            ";

            var program = assembler.Assemble(asm).Select(x => Word18.FromLong((long)x)).ToList();
            proc.LoadProgram(program);
            proc.Run();

            // Verify C = {5, 7, 9} at 120
            Assert.AreEqual(5, proc.ReadWord(120).ToInt128());
            Assert.AreEqual(7, proc.ReadWord(121).ToInt128());
            Assert.AreEqual(9, proc.ReadWord(122).ToInt128());
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_RecursiveFibonacci()
        {
            var proc = CreateProcessor();
            var assembler = new T3InOrderAssembler(T3Config.T3_18);

            string asm = @"
                LI B, 6       ; n = 6
                LI D, fib
                CALL D
                HALT

            fib:
                LI D, 1
                CMP B, D
                LI D, base_case
                JL D
                LI D, base_case
                JE D

                PUSH B        ; Save n
                SUBI B, B, 1
                LI D, fib
                CALL D        ; C = Fib(n-1)
                PUSH C        ; Save Fib(n-1)
                
                POP B         ; B = Fib(n-1)
                POP D         ; D = n
                PUSH B        ; Save Fib(n-1) back to stack
                
                SUBI D, D, 2
                MOV B, D
                LI D, fib
                CALL D        ; C = Fib(n-2)
                
                POP B         ; B = Fib(n-1)
                ADD C, C, B   ; C = Fib(n-2) + Fib(n-1)
                RET

            base_case:
                MOV C, B
                RET
            ";

            var program = assembler.Assemble(asm).Select(x => Word18.FromLong((long)x)).ToList();
            proc.LoadProgram(program);
            proc.Run();

            // Fib(6) = 8. B=R1, C=R2.
            Assert.AreEqual(8, proc.GetState().Registers[2].ToInt128()); 
        }
    }
}
