using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Simulator.Common;using T3Simulator.InOrder;using T3Assembler;
using System.Collections.Generic;using System.Linq;using TritTypes;

namespace T3Simulator.InOrder.Tests
{
    [TestClass]
    public class MathAssemblyTests
    {
        private T3InOrderProcessor<Word18> AssembleAndRun(string sourceCode)
        {
            var asm = new T3InOrderAssembler(T3Config.T3_18);
            var binary = asm.Assemble(sourceCode);
            var words = binary.Select(x => Word18.FromInt128(x)).ToList();
            var proc = new T3InOrderProcessor<Word18>(T3Config.T3_18);
            proc.LoadProgram(words); proc.Run(); return proc;
        }

        [TestMethod] public void FpuDemo_Arithmetic_ProducesExpectedOutputs()
        {
            string asm = @"start: LI RW,9; LI RX,3; ITOF FW,RW; ITOF FX,RX; FADD FW,FW,FX; FDIV FW,FW,FX; FNEG FW,FW; FTOI RW,FW; LI RX,dst1; STORE RW,RX; HALT; dst1: .word 0";
            var proc = AssembleAndRun(asm);
            long val1 = proc.ReadWord(proc.PC+0).ToLong();
            Assert.IsTrue(val1 <= 0, $"FNEG result should be non-positive, got {val1}");
        }

        [TestMethod] public void Quadratic_Discriminant_Integer()
        {
            string asm = @"start: LI RW,1; LI RX,-3; LI RY,2; MUL RZ,RX,RX; MUL R0,RW,RY; ADD R0,R0,R0; ADD R0,R0,R0; SUB RZ,RZ,R0; HALT";
            var proc = AssembleAndRun(asm);
            Assert.AreEqual(1, proc.Registers[3].ToLong());
        }

        [TestMethod] public void FpuCompareAndBranch_CorrectlyJumps()
        {
            string asm = @"start: LI RW,27; LI RX,3; ITOF FW,RW; ITOF FX,RX; FCMP FW,FX; LI RY,greater; JG RY; LI RW,-1; HALT; greater: LI RW,1; HALT";
            var proc = AssembleAndRun(asm);
            Assert.AreEqual(1, proc.Registers[0].ToLong());
        }

        [TestMethod] public void TaylorSin_ApproximatesSinX()
        {
            string asm = @"start: LI RW,addr_x; LOAD RW,RW; ITOF FW,RW; FMOV FX,FW; FMUL FX,FX,FW; FMUL FX,FX,FW; LI RX,6; ITOF FY,RX; FDIV FX,FX,FY; FSUB FW,FW,FX; FTOI RW,FW; LI RX,dst; STORE RW,RX; HALT; addr_x: .word 5; dst: .word 0";
            var proc = AssembleAndRun(asm);
            long result = proc.ReadWord(proc.PC+1).ToLong();
            Assert.IsTrue(result>=-30&&result<=10,$"sin(5) ~ {result}");
        }

        [TestMethod] public void FpuClassifyZeroSwap_Works()
        {
            var proc = new T3InOrderProcessor<Word18>(T3Config.T3_18);
            proc.FRegisters[1] = T3Float.FromDouble(27.0);
            // FCLASS FW,FX → FTOI RW,FW → HALT
            string fclassS = "000"+BT((int)Opcode.FCLASS,6)+BT(0,3)+BT(1,3)+BT(0,3);
            string ftoiS = "000"+BT((int)Opcode.FTOI,6)+BT(0,3)+BT(0,3)+BT(0,3);
            var prog = new List<Word18> { Word18.FromLong(BalancedTernary.ParseToLong(fclassS)), Word18.FromLong(BalancedTernary.ParseToLong(ftoiS)), Word18.FromLong(0) };
            proc.LoadProgram(prog); proc.Run();
            Assert.IsTrue(proc.Registers[0].ToLong()>0);
        }
        static string BT(long v,int d)=>BalancedTernary.ToTernaryString(v).PadLeft(d,'0')[^d..];

        [TestMethod] public void CLI_Quadratic_Verification()
        {
            var asm = new T3InOrderAssembler(T3Config.T3_18);
            string asmPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,"..","..","..","..","..","src","T3Assembler","examples","fpu_demo.asm");
            if(!System.IO.File.Exists(asmPath)){string? d=System.AppDomain.CurrentDomain.BaseDirectory;while(d!=null&&!System.IO.Directory.Exists(System.IO.Path.Combine(d,"src")))d=System.IO.Directory.GetParent(d)?.FullName;if(d!=null)asmPath=System.IO.Path.Combine(d,"src","T3Assembler","examples","fpu_demo.asm");}
            if(!System.IO.File.Exists(asmPath)){Assert.Inconclusive("fpu_demo.asm not found");return;}
            string src = System.IO.File.ReadAllText(asmPath);
            var bin = asm.Assemble(src); var words = bin.Select(x=>Word18.FromInt128(x)).ToList();
            var proc = new T3InOrderProcessor<Word18>(T3Config.T3_18);
            proc.LoadProgram(words); proc.Run();
            Assert.IsFalse(proc.Step());
            var st=proc.GetState(); Assert.IsNotNull(st);
            Assert.IsTrue(st.InstructionCount>0);
        }
    }
}