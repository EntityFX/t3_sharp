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
            long fullOpcode = pred * 45 + opcode;
            string sOp = ToBalancedTernary(fullOpcode, 6);
            string sOp1 = ToBalancedTernary(op1, 6);
            string sOp2 = ToBalancedTernary(op2, 6);
            return Word18.FromLong(BalancedTernary.ParseToLong(sOp + sOp1 + sOp2));
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
            long fullOpcode = pred * 45 + opcode;
            string sOp = ToBalancedTernary(fullOpcode, 6);
            string sOp1 = ToBalancedTernary(op1, 9);
            string sOp2 = TritTypes.BalancedTernary.ToTernaryString(op2);
            if (sOp2.Length > 9) sOp2 = sOp2.Substring(sOp2.Length - 9);
            if (sOp2.Length < 9) sOp2 = sOp2.PadLeft(9, '0');
            
            string instruction = sOp + sOp1 + sOp2 + "000";
            string word = instruction.PadLeft(54, '0');
            return Word54.FromInt128(TritTypes.BalancedTernary.ParseToInt128(word));
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_ArrayAddition()
        {
            var proc = CreateProcessor();
            
            // Arrays: A[0..2] = {1, 2, 3}, B[0..2] = {4, 5, 6}
            // Result C[0..2] = {5, 7, 9}
            // Addresses: A=100, B=110, C=120
            
            List<Word18> program = new List<Word18>();
            // R0: ptrA, R1: ptrB, R2: ptrC, R3: count, R4: valA, R5: valB, R6: index, R7: loop_start, R8: end_addr
            program.Add(Encode(4, 0, 100)); // 0
            program.Add(Encode(4, 1, 110)); // 1
            program.Add(Encode(4, 2, 120)); // 2
            program.Add(Encode(4, 3, 3));   // 3
            
            // Init A {1,2,3}
            program.Add(Encode(4, 4, 1)); program.Add(Encode(4, 5, 100)); program.Add(Encode(2, 4, 5)); // 4,5,6
            program.Add(Encode(4, 4, 2)); program.Add(Encode(4, 5, 101)); program.Add(Encode(2, 4, 5)); // 7,8,9
            program.Add(Encode(4, 4, 3)); program.Add(Encode(4, 5, 102)); program.Add(Encode(2, 4, 5)); // 10,11,12
            
            // Init B {4,5,6}
            program.Add(Encode(4, 4, 4)); program.Add(Encode(4, 5, 110)); program.Add(Encode(2, 4, 5)); // 13,14,15
            program.Add(Encode(4, 4, 5)); program.Add(Encode(4, 5, 111)); program.Add(Encode(2, 4, 5)); // 16,17,18
            program.Add(Encode(4, 4, 6)); program.Add(Encode(4, 5, 112)); program.Add(Encode(2, 4, 5)); // 19,20,21
            
            program.Add(Encode(4, 8, 37));  // 22: R8 = 37 (HALT)
            program.Add(Encode(4, 7, 25));  // 23: R7 = 25 (Loop start)
            program.Add(Encode(4, 6, 0));   // 24: R6 = 0 (index)
            
            // Loop start: PC 25
            program.Add(Encode(1, 4, 0));   // 25: R4 = mem[R0]
            program.Add(Encode(1, 5, 1));   // 26: R5 = mem[R1]
            program.Add(Encode(6, 4, 5));   // 27: R4 = R4 + R5
            program.Add(Encode(2, 4, 2));   // 28: mem[R2] = R4
            program.Add(Encode(4, 4, 1));   // 29: R4 = 1 (const 1)
            program.Add(Encode(6, 0, 4));   // 30: R0++
            program.Add(Encode(6, 1, 4));   // 31: R1++
            program.Add(Encode(6, 2, 4));   // 32: R2++
            program.Add(Encode(6, 6, 4));   // 33: R6++
            program.Add(Encode(17, 6, 3));  // 34: CMP R6, R3
            program.Add(Encode(19, 8, 0));  // 35: JE R8
            program.Add(Encode(18, 7, 0));  // 36: JMP R7
            program.Add(Encode(0, 0, 0));   // 37: HALT
            
            proc.LoadProgram(program);
            proc.Run();
            
            proc.Reset();
            
            // Verify results: mem[120]=5, mem[121]=7, mem[122]=9
            List<Word18> verifyProg = new List<Word18>();
            verifyProg.Add(Encode(4, 0, 120)); // R0 = 120
            verifyProg.Add(Encode(1, 1, 0));   // R1 = mem[120]
            verifyProg.Add(Encode(4, 0, 121)); // R0 = 121
            verifyProg.Add(Encode(1, 2, 0));   // R2 = mem[121]
            verifyProg.Add(Encode(4, 0, 122)); // R0 = 122
            verifyProg.Add(Encode(1, 3, 0));   // R3 = mem[122]
            verifyProg.Add(Encode(0, 0, 0));   // HALT
            
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
            // func(x) { return x * 2 + 1 }
            // main:
            //   LI A, 5
            //   CALL func
            //   HALT
            // func:
            //   LI B, 2
            //   MUL E, B
            //   LI B, 1
            //   ADD E, B
            //   MOV A, E
            //   RET
            
            List<Word18> program = new List<Word18>();
            program.Add(Encode(4, 0, 5));  // 0: A = 5
            program.Add(Encode(4, 1, 4));  // 1: R1 = 4 (addr of func)
            program.Add(Encode(24, 1, 0)); // 2: CALL R1
            program.Add(Encode(0, 0, 0));  // 3: HALT
            
            // func:
            program.Add(Encode(4, 1, 2));  // 4: B = 2
            program.Add(Encode(8, 4, 1));  // 5: E = E * B (E is main's A)
            program.Add(Encode(4, 1, 1));  // 6: B = 1
            program.Add(Encode(6, 4, 1));  // 7: E = E + B
            program.Add(Encode(25, 0, 0)); // 8: RET
            
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
            
            List<Word54> program = new List<Word54>();
            
            // LIMM A, 3^20
            Int128 val = (Int128)Math.Pow(3, 20);
            program.Add(EncodeInt128(5, 0, 0)); // LIMM A, [next]
            program.Add(Word54.FromInt128(val)); // The value itself
            program.Add(EncodeInt128(4, 1, 2)); // LI B, 2
            program.Add(EncodeInt128(8, 0, 1)); // MUL A, B -> 3^20 * 2
            program.Add(EncodeInt128(0, 0, 0)); // HALT
            
            proc.LoadProgram(program);
            proc.Run();
            
            Assert.AreEqual(val * 2, proc.GetState().Registers[0].ToInt128());
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
