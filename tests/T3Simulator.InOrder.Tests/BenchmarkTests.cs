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
        [Timeout(30000)]
        public void CounterReset_ResetCounters_ResetsToZero()
        {
            var proc = new T3InOrderProcessor<Word18>(T3Config.T3_18);
            string asm = @"start: LI R0, 100
LI RW, 200
ADD RX, R0, RW
HALT";
            proc.LoadProgram(new T3InOrderAssembler(T3Config.T3_18).Assemble(asm).Select(x => Word18.FromInt128(x)).ToList());
            proc.Run();
            Assert.IsTrue(proc.CycleCount > 0);
            proc.ResetCounters();
            Assert.AreEqual(0, proc.CycleCount);
        }

        [TestMethod]
        [Timeout(30000)]
        public void RunIterations_AccumulatesCounters()
        {
            string asm = @"start: LI R0, 100
LI RW, 200
ADD RX, R0, RW
HALT";
            var words = new T3InOrderAssembler(T3Config.T3_18).Assemble(asm).Select(x => Word18.FromInt128(x)).ToList();
            var proc = new T3InOrderProcessor<Word18>(T3Config.T3_18);
            proc.LoadProgram(words); proc.ResetCounters();
            proc.RunIterations(5);
            Assert.IsTrue(proc.CycleCount >= 10, $"Cycles: {proc.CycleCount}");
            Assert.IsTrue(proc.InstructionCount >= 10, $"Instr: {proc.InstructionCount}");
        }

        [TestMethod]
        [Timeout(30000)]
        public void Dhrystone_Core_ExecutesCorrectly()
        {
            string asm = @"
                LI R0, start
                JMP R0
            proc1:
                MUL RW,RW,RW
                MUL RX,RX,RX
                ADD RX,RW,RX
                RET
            start:
                LI R0,10
                LI RW,5
                STORE RW,R0
                LI R0,10
                LOAD RW,R0
                LI RX,3
                LI RY,proc1
                CALL RY
                LI R0,10
                LOAD RY,R0
                LI RZ,3
                MUL RY,RY,RZ
                LI RZ,7
                ADD RY,RY,RZ
                LI RZ,2
                DIV RY,RY,RZ
                STORE RY,R0
                HALT
            ";
            var proc = AssembleAndRun(asm);
            Assert.AreEqual(11, proc.ReadWord(10).ToLong(), "IntGlob = (5*3+7)/2 = 11");
        }

        [TestMethod]
        [Timeout(30000)]
        public void Dhrystone_10Iterations_Benchmark()
        {
            string asm = @"
                LI R0,s
                JMP R0
            proc1:
                MUL RW,RW,RW
                MUL RX,RX,RX
                ADD RX,RW,RX
                RET
            s:
                LI R0,10
                LI RW,5
                STORE RW,R0
                LI R0,10
                LOAD RW,R0
                LI RX,3
                LI RY,proc1
                CALL RY
                LI R0,10
                LOAD RY,R0
                LI RZ,3
                MUL RY,RY,RZ
                LI RZ,7
                ADD RY,RY,RZ
                LI RZ,2
                DIV RY,RY,RZ
                STORE RY,R0
                LI R0,0
                LI RW,100
                LI RX,10
            loop_f:
                MOV RY,R0
                MUL RY,RY,R0
                ADD RY,RY,R0
                ADD RY,RY,RW
                LI RZ,1
                ADD RZ,RZ,R0
                STORE RZ,RY
                LI R0,1
                ADD R0,R0,R0
                CMP R0,RX
                LI R0,loop_f
                JL R0
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
        [Timeout(30000)]
        public void Whetstone_FPU_ExecutesCorrectly()
        {
            string asm = @"
                LI R0,main
                JMP R0
            mod1:
                ITOF R0,R0
                ITOF RW,RW
                ITOF RX,RX
                FADD R0,R0,RW
                FSUB R0,R0,RX
                FADD R0,R0,RW
                FTOI R0,R0
                RET
            mod2:
                ITOF R0,R0
                ITOF RW,RW
                FMUL RX,R0,RW
                FDIV RX,RX,RW
                FTOI RX,RX
                RET
            main:
                LI R0,9
                LI RW,3
                LI RX,1
                LI RY,mod1
                CALL RY
                LI R0,9
                LI RW,3
                LI RX,mod2
                CALL RX
                HALT
            ";
            var proc = AssembleAndRun(asm);
            Assert.IsTrue(proc.CycleCount > 0, "Whetstone should execute FPU instructions");
        }

        [TestMethod]
        [Timeout(30000)]
        public void Whetstone_10Iterations_Benchmark()
        {
            string asm = @"
                LI R0,m
                JMP R0
            m1:
                ITOF R0,R0
                ITOF RW,RW
                ITOF RX,RX
                FADD R0,R0,RW
                FSUB R0,R0,RX
                FADD R0,R0,RW
                FTOI R0,R0
                RET
            m2:
                ITOF R0,R0
                ITOF RW,RW
                FMUL RX,R0,RW
                FDIV RX,RX,RW
                FTOI RX,RX
                RET
            m:
                LI R0,9
                LI RW,3
                LI RX,1
                LI RY,m1
                CALL RY
                LI R0,9
                LI RW,3
                LI RX,m2
                CALL RX
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
        [Timeout(30000)]
        public void MWMIPS_MemFill_ExecutesCorrectly()
        {
            string asm = @"
                LI R0,m
                JMP R0
            memfill:
                LI R0,50
                LI RW,5
                LI RX,42
            fill_lp:
                STORE RX,R0
                LI RY,1
                ADD R0,R0,RY
                SUB RW,RW,RY
                LI RY,0
                CMP RW,RY
                LI RZ,fill_lp
                JG RZ
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
        [Timeout(30000)]
        public void MWMIPS_10Iterations_Benchmark()
        {
            string asm = @"
                LI R0,m
                JMP R0
            mf:
                LI R0,50
                LI RW,5
                LI RX,42
            fl:
                STORE RX,R0
                LI RY,1
                ADD R0,R0,RY
                SUB RW,RW,RY
                LI RY,0
                CMP RW,RY
                LI RZ,fl
                JG RZ
                RET
            mc:
                LI R0,50
                LI RW,60
                LI RX,5
            cl:
                LOAD RY,R0
                STORE RY,RW
                LI RZ,1
                ADD R0,R0,RZ
                ADD RW,RW,RZ
                SUB RX,RX,RZ
                LI RZ,0
                CMP RX,RZ
                LI R0,cl
                JG R0
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
        [Timeout(30000)]
        public void AllBenchmarks_Combined()
        {
            var asmObj = new T3InOrderAssembler(T3Config.T3_18);
            string d = @"LI R0,s
JMP R0
p1:MUL RW,RW,RW
MUL RX,RX,RX
ADD RX,RW,RX
RET
s:LI R0,10
LI RW,5
STORE RW,R0
LI R0,10
LOAD RW,R0
LI RX,3
LI RY,p1
CALL RY
LI R0,10
LOAD RY,R0
LI RZ,3
MUL RY,RY,RZ
LI RZ,7
ADD RY,RY,RZ
LI RZ,2
DIV RY,RY,RZ
STORE RY,R0
HALT";
            RunBenchmark(asmObj, d, 10, "Dhrystone", 5, 3);

            string w = @"LI R0,m
JMP R0
m1:ITOF R0,R0
ITOF RW,RW
ITOF RX,RX
FADD R0,R0,RW
FSUB R0,R0,RX
FADD R0,R0,RW
FTOI R0,R0
RET
m:LI R0,9
LI RW,3
LI RX,1
LI RY,m1
CALL RY
HALT";
            RunBenchmark(asmObj, w, 5, "Whetstone", 5, 3);

            string m = @"LI R0,s
JMP R0
mf:LI R0,50
LI RW,5
LI RX,42
fl:STORE RX,R0
LI RY,1
ADD R0,R0,RY
SUB RW,RW,RY
LI RY,0
CMP RW,RY
LI RZ,fl
JG RZ
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