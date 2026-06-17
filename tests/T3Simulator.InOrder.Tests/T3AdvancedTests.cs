using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Simulator.Common;using T3Simulator.InOrder;using T3Assembler;
using System.Collections.Generic;using System.Linq;using TritTypes;

namespace T3Simulator.InOrder.Tests
{
    [TestClass]
    public class T3AdvancedTests
    {
        private T3InOrderProcessor<Word18> CreateProcessor() => new(T3Config.T3_18);

        private List<Word18> Asm(string code)
        {
            var a = new T3InOrderAssembler(T3Config.T3_18);
            return a.Assemble(code).Select(x=>Word18.FromInt128(x)).ToList();
        }

        [TestMethod] public void Test_ProcedureCall_WithStack()
        {
            var proc = CreateProcessor();
            string asm = @"LI RW,5
LI RX,func
CALL RX
HALT
func:
LI RY,2
MUL RW,RW,RY
LI RY,1
ADD RW,RW,RY
RET";
            var prog = Asm(asm);
            proc.LoadProgram(prog);
            try { proc.Run(); }
            catch (Exception ex) { Assert.Fail($"Run failed: {ex.Message}\nASM:\n{asm}\nWords: {string.Join(",", prog.Select(w=>w.ToLong()))}"); }
            for (int i = 0; i < 9; i++) { var v = proc.Registers[i].ToLong(); if (v != 0) System.Diagnostics.Debug.WriteLine($"  Reg[{i}]={v}"); }
            Assert.AreEqual(11, proc.Registers[0].ToLong(), $"Reg[0]={proc.Registers[0].ToLong()} Reg[1]={proc.Registers[1].ToLong()} Reg[2]={proc.Registers[2].ToLong()} SP={proc.SP} PC={proc.PC}");
        }

        [TestMethod] public void Test_NestedBranching()
        {
            var proc = CreateProcessor();
            string asm = @"LI RW,1; LI RX,1; LI RY,0; LI RZ,0; LI R0,nested; CMP RW,RZ; JG R0; LI RY,3; LI R1,end; JMP R1; nested: LI R0,set1; CMP RX,RZ; JG R0; LI RY,2; LI R1,end; JMP R1; set1: LI RY,1; end: HALT";
            proc.LoadProgram(Asm(asm)); proc.Run();
            Assert.AreEqual(1, proc.Registers[2].ToLong()); // RY=phys2
        }

        [TestMethod] public void Test_DoubleLoop()
        {
            var proc = CreateProcessor();
            string asm = @"LI RW,0; LI RX,0; LI RY,3; LI RZ,1;
loop_i: LI R0,0;
loop_j: MOV R1,RX; ADD R1,R0; ADD RW,R1; ADD R0,RZ;
LI R2,end_j; CMP R0,RY; JE R2; LI R3,loop_j; JMP R3;
end_j: ADD RX,RZ; LI R2,end_i; CMP RX,RY; JE R2; LI R3,loop_i; JMP R3;
end_i: HALT";
            proc.LoadProgram(Asm(asm)); proc.Run();
            Assert.AreEqual(18, proc.Registers[0].ToLong());
        }

        [TestMethod] public void Test_ArrayAddition()
        {
            var proc = CreateProcessor();
            string asm = @"LI RX,100; LI RY,1; STORE RY,RX; LI RX,101; LI RY,2; STORE RY,RX; LI RX,102; LI RY,3; STORE RY,RX;
LI RX,110; LI RY,4; STORE RY,RX; LI RX,111; LI RY,5; STORE RY,RX; LI RX,112; LI RY,6; STORE RY,RX;
LI RW,0; LI RX,100; LI RY,110; LI RZ,120; LI R0,3;
loop: MOV R1,RW; ADD R1,RX; LOAD R2,R1;
MOV R1,RW; ADD R1,RY; LOAD R3,R1;
ADD R4,R2,R3;
MOV R1,RW; ADD R1,RZ; STORE R4,R1;
ADDI RW,RW,1; CMP RW,R0; LI R4,end; JE R4; LI R4,loop; JMP R4;
end: HALT";
            proc.LoadProgram(Asm(asm)); proc.Run();
            Assert.AreEqual(5, proc.ReadWord(120).ToInt128());
            Assert.AreEqual(7, proc.ReadWord(121).ToInt128());
            Assert.AreEqual(9, proc.ReadWord(122).ToInt128());
        }

        [TestMethod] public void Test_RecursiveFibonacci()
        {
            var proc = CreateProcessor();
            string asm = @"LI RX,6; LI RZ,fib; CALL RZ; HALT;
fib: LI RZ,1; CMP RX,RZ; LI RZ,base_case; JL RZ; LI RZ,base_case; JE RZ;
PUSH RX; SUBI RX,RX,1; LI RZ,fib; CALL RZ; PUSH RY;
POP RX; POP RZ; PUSH RX;
SUBI RZ,RZ,2; MOV RX,RZ; LI RZ,fib; CALL RZ;
POP RX; ADD RY,RY,RX; RET;
base_case: MOV RY,RX; RET";
            proc.LoadProgram(Asm(asm)); proc.Run();
            Assert.AreEqual(8, proc.Registers[2].ToLong()); // Fib(6)=8
        }
    }
}