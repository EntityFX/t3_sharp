using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Simulator.Common;
using T3Simulator.InOrder;
using T3Assembler;
using System.Collections.Generic;
using System.Linq;
using TritTypes;

namespace T3Simulator.InOrder.Tests
{
    [TestClass]
    public class BenchmarkTests
    {
        private T3InOrderProcessor<Word18> AssembleAndRun(string sourceCode)
        {
            var asm = new T3InOrderAssembler(T3Config.T3_18);
            var binary = asm.Assemble(sourceCode);
            var words = binary.Select(x => Word18.FromInt128(x)).ToList();
            var proc = new T3InOrderProcessor<Word18>(T3Config.T3_18);
            proc.LoadProgram(words);
            proc.Run();
            return proc;
        }

        [TestMethod]
        public void CounterReset_ResetCounters_ResetsToZero()
        {
            var proc = new T3InOrderProcessor<Word18>(T3Config.T3_18);
            string asm = @"start: LI R0, 100
LI R1, 200
ADD R2, R0, R1
HALT";
            proc.LoadProgram(new T3InOrderAssembler(T3Config.T3_18).Assemble(asm).Select(x => Word18.FromInt128(x)).ToList());
            proc.Run();
            Assert.IsTrue(proc.CycleCount > 0);
            proc.ResetCounters();
            Assert.AreEqual(0, proc.CycleCount);
        }

        [TestMethod]
        public void RunIterations_AccumulatesCounters()
        {
            string asm = @"start: LI R0, 100
LI R1, 200
ADD R2, R0, R1
HALT";
            var words = new T3InOrderAssembler(T3Config.T3_18).Assemble(asm).Select(x => Word18.FromInt128(x)).ToList();
            var proc = new T3InOrderProcessor<Word18>(T3Config.T3_18);
            proc.LoadProgram(words); proc.ResetCounters();
            proc.RunIterations(5);
            Assert.IsTrue(proc.CycleCount >= 10, $"Cycles: {proc.CycleCount}");
            Assert.IsTrue(proc.InstructionCount >= 10, $"Instr: {proc.InstructionCount}");
        }

        [TestMethod]
        public void Dhrystone_Core_ExecutesCorrectly()
        {
            string asm = @"
                LI R0, start
                JMP R0
            proc1:
                MUL R1,R1,R1
                MUL R2,R2,R2
                ADD R2,R1,R2
                RET
            start:
                LI R0,10
                LI R1,5
                STORE R1,R0
                LI R0,10
                LOAD R1,R0
                LI R2,3
                LI R3,proc1
                CALL R3
                LI R0,10
                LOAD R3,R0
                LI R4,3
                MUL R3,R3,R4
                LI R4,7
                ADD R3,R3,R4
                LI R4,2
                DIV R3,R3,R4
                STORE R3,R0
                HALT
            ";
            var proc = AssembleAndRun(asm);
            Assert.AreEqual(11, proc.ReadWord(10).ToLong(), "IntGlob = (5*3+7)/2 = 11");
        }

        [TestMethod]
        public void Dhrystone_10Iterations_Benchmark()
        {
            string asm = @"
                LI R0,s
                JMP R0
            proc1:
                MUL R1,R1,R1
                MUL R2,R2,R2
                ADD R2,R1,R2
                RET
            s:
                LI R0,10
                LI R1,5
                STORE R1,R0
                LI R0,10
                LOAD R1,R0
                LI R2,3
                LI R3,proc1
                CALL R3
                LI R0,10
                LOAD R3,R0
                LI R4,3
                MUL R3,R3,R4
                LI R4,7
                ADD R3,R3,R4
                LI R4,2
                DIV R3,R3,R4
                STORE R3,R0
                LI R0,0
                LI R1,100
                LI R2,10
            loop_f:
                MOV R3,R0
                MUL R3,R3,R0
                ADD R3,R3,R0
                ADD R3,R3,R1
                LI R4,1
                ADD R4,R4,R0
                STORE R4,R3
                LI R5,1
                ADD R0,R0,R5
                CMP R0,R2
                LI R5,loop_f
                JL R5
                HALT
            ";
            var asmObj = new T3InOrderAssembler(T3Config.T3_18);
            var words = asmObj.Assemble(asm).Select(x => Word18.FromInt128(x)).ToList();
            var proc = new T3InOrderProcessor<Word18>(T3Config.T3_18);
            proc.LoadProgram(words); proc.ResetCounters();
            proc.RunIterations(10);
            Assert.IsTrue(proc.CycleCount >= 50, $"Dhrystone 10x cycles: {proc.CycleCount}");
            Assert.IsTrue(proc.InstructionCount >= 30, $"Dhrystone 10x instr: {proc.InstructionCount}");
        }

        [TestMethod]
        public void Whetstone_FPU_ExecutesCorrectly()
        {
            string asm = @"
                LI R0,main
                JMP R0
            mod1:
                ITOF R0,R0
                ITOF R1,R1
                ITOF R2,R2
                FADD R0,R0,R1
                FSUB R0,R0,R2
                FADD R0,R0,R1
                FTOI R0,R0
                RET
            mod2:
                ITOF R0,R0
                ITOF R1,R1
                FMUL R2,R0,R1
                FDIV R2,R2,R1
                FTOI R2,R2
                RET
            main:
                LI R0,9
                LI R1,3
                LI R2,1
                LI R3,mod1
                CALL R3
                LI R0,9
                LI R1,3
                LI R2,mod2
                CALL R2
                HALT
            ";
            var proc = AssembleAndRun(asm);
            Assert.IsTrue(proc.CycleCount > 0, "Whetstone should execute FPU instructions");
        }

        [TestMethod]
        public void Whetstone_10Iterations_Benchmark()
        {
            string asm = @"
                LI R0,m
                JMP R0
            m1:
                ITOF R0,R0
                ITOF R1,R1
                ITOF R2,R2
                FADD R0,R0,R1
                FSUB R0,R0,R2
                FADD R0,R0,R1
                FTOI R0,R0
                RET
            m2:
                ITOF R0,R0
                ITOF R1,R1
                FMUL R2,R0,R1
                FDIV R2,R2,R1
                FTOI R2,R2
                RET
            m:
                LI R0,9
                LI R1,3
                LI R2,1
                LI R3,m1
                CALL R3
                LI R0,9
                LI R1,3
                LI R2,m2
                CALL R2
                HALT
            ";
            var asmObj = new T3InOrderAssembler(T3Config.T3_18);
            var words = asmObj.Assemble(asm).Select(x => Word18.FromInt128(x)).ToList();
            var proc = new T3InOrderProcessor<Word18>(T3Config.T3_18);
            proc.LoadProgram(words); proc.ResetCounters();
            proc.RunIterations(10);
            Assert.IsTrue(proc.CycleCount >= 50, $"Whetstone 10x cycles: {proc.CycleCount}");
            Assert.IsTrue(proc.InstructionCount >= 30, $"Whetstone 10x instr: {proc.InstructionCount}");
        }

        [TestMethod]
        public void MWMIPS_MemFill_ExecutesCorrectly()
        {
            string asm = @"
                LI R0,m
                JMP R0
            memfill:
                LI R0,50
                LI R1,5
                LI R2,42
            fill_lp:
                STORE R2,R0
                LI R3,1
                ADD R0,R0,R3
                SUB R1,R1,R3
                LI R3,0
                CMP R1,R3
                LI R4,fill_lp
                JG R4
                RET
            m:
                LI R0,memfill
                CALL R0
                HALT
            ";
            var proc = AssembleAndRun(asm);
            Assert.AreEqual(42, proc.ReadWord(50).ToLong(), "mem[50] should be 42");
        }

        [TestMethod]
        public void MWMIPS_10Iterations_Benchmark()
        {
            string asm = @"
                LI R0,m
                JMP R0
            mf:
                LI R0,50
                LI R1,5
                LI R2,42
            fl:
                STORE R2,R0
                LI R3,1
                ADD R0,R0,R3
                SUB R1,R1,R3
                LI R3,0
                CMP R1,R3
                LI R4,fl
                JG R4
                RET
            mc:
                LI R0,50
                LI R1,60
                LI R2,5
            cl:
                LOAD R3,R0
                STORE R3,R1
                LI R4,1
                ADD R0,R0,R4
                ADD R1,R1,R4
                SUB R2,R2,R4
                LI R4,0
                CMP R2,R4
                LI R5,cl
                JG R5
                RET
            m:
                LI R0,mf
                CALL R0
                LI R0,mc
                CALL R0
                HALT
            ";
            var asmObj = new T3InOrderAssembler(T3Config.T3_18);
            var words = asmObj.Assemble(asm).Select(x => Word18.FromInt128(x)).ToList();
            var proc = new T3InOrderProcessor<Word18>(T3Config.T3_18);
            proc.LoadProgram(words); proc.ResetCounters();
            proc.RunIterations(10);
            Assert.IsTrue(proc.CycleCount >= 50, $"MWMIPS 10x cycles: {proc.CycleCount}");
            Assert.IsTrue(proc.InstructionCount >= 20, $"MWMIPS 10x instr: {proc.InstructionCount}");
        }

        [TestMethod]
        public void AllBenchmarks_Combined()
        {
            var asmObj = new T3InOrderAssembler(T3Config.T3_18);
            string d = @"LI R0,s
JMP R0
p1:MUL R1,R1,R1
MUL R2,R2,R2
ADD R2,R1,R2
RET
s:LI R0,10
LI R1,5
STORE R1,R0
LI R0,10
LOAD R1,R0
LI R2,3
LI R3,p1
CALL R3
LI R0,10
LOAD R3,R0
LI R4,3
MUL R3,R3,R4
LI R4,7
ADD R3,R3,R4
LI R4,2
DIV R3,R3,R4
STORE R3,R0
HALT";
            RunBenchmark(asmObj, d, 10, "Dhrystone", 5, 3);

            string w = @"LI R0,m
JMP R0
m1:ITOF R0,R0
ITOF R1,R1
ITOF R2,R2
FADD R0,R0,R1
FSUB R0,R0,R2
FADD R0,R0,R1
FTOI R0,R0
RET
m:LI R0,9
LI R1,3
LI R2,1
LI R3,m1
CALL R3
HALT";
            RunBenchmark(asmObj, w, 5, "Whetstone", 5, 3);

            string m = @"LI R0,s
JMP R0
mf:LI R0,50
LI R1,5
LI R2,42
fl:STORE R2,R0
LI R3,1
ADD R0,R0,R3
SUB R1,R1,R3
LI R3,0
CMP R1,R3
LI R4,fl
JG R4
RET
s:LI R0,mf
CALL R0
HALT";
            RunBenchmark(asmObj, m, 5, "MWMIPS", 5, 3);
        }

        private void RunBenchmark(T3InOrderAssembler asmObj, string source, int iterations,
                                   string name, long minCycles, long minInstr)
        {
            var words = asmObj.Assemble(source).Select(x => Word18.FromInt128(x)).ToList();
            var proc = new T3InOrderProcessor<Word18>(T3Config.T3_18);
            proc.LoadProgram(words); proc.ResetCounters();
            proc.RunIterations(iterations);
            Assert.IsTrue(proc.CycleCount >= minCycles, $"{name}: expected >= {minCycles} cycles, got {proc.CycleCount}");
            Assert.IsTrue(proc.InstructionCount >= minInstr, $"{name}: expected >= {minInstr} instr, got {proc.InstructionCount}");
        }
    }
}