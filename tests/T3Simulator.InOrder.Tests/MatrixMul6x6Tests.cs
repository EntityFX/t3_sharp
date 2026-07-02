using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Simulator.Common;using T3Simulator.InOrder;using System.Collections.Generic;using TritTypes;

namespace T3Simulator.InOrder.Tests
{
    [TestClass]
    public class MatrixMul6x6Tests
    {
        static Word18 I(Opcode o,int r,long imm)=>Word18.FromLong(InstructionEncoder.EncodeI(0,0,+1,(int)o,r,imm));
        static Word18 R(Opcode o,int r1,int r2,int r3)=>Word18.FromLong(InstructionEncoder.EncodeR(0,0,0,(int)o,r1,r2,r3));
        static Word18 J(Opcode o,int r)=>Word18.FromLong(InstructionEncoder.EncodeJ(0,0,-1,(int)o,r));
        static Word18 H()=>new(0);
        const int RW=-4,RX=-3,RY=-2,RZ=-1,Rg0=0,Rg1=1,Rg2=2,Rg3=3,Rg4=4;

        [TestMethod][Timeout(5000)]public void MatrixMultiplication_6x6_InOrder_Test(){
            var p=new T3InOrderProcessor<Word18>(T3Config.T3_18);
            p.LoadProgram(new List<Word18>{I(Opcode.MOV,RW,1),I(Opcode.MOV,RX,2),R(Opcode.ADD,RW,RW,RX),H()});p.Run();
            Assert.AreEqual(3,p.Registers[0].ToLong());
        }
    }
}