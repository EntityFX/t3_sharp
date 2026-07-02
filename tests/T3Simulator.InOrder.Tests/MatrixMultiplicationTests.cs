using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Simulator.Common;using T3Simulator.InOrder;using System.Collections.Generic;using TritTypes;

namespace T3Simulator.InOrder.Tests
{
    [TestClass]
    public class MatrixMultiplicationTests
    {
        static T3InOrderProcessor<Word18> P()=>new(T3Config.T3_18);
        static Word18 I(Opcode o,int r,long imm)=>Word18.FromLong(InstructionEncoder.EncodeI(0,0,+1,(int)o,r,imm));
        static Word18 R(Opcode o,int r1,int r2,int r3)=>Word18.FromLong(InstructionEncoder.EncodeR(0,0,0,(int)o,r1,r2,r3));
        static Word18 J(Opcode o,int r)=>Word18.FromLong(InstructionEncoder.EncodeJ(0,0,-1,(int)o,r));
        static Word18 H()=>new(0);
        static void Ld(T3InOrderProcessor<Word18> p,params Word18[] c)=>p.LoadProgram(new List<Word18>(c));
        const int RW=-4,RX=-3,RY=-2,RZ=-1,Rg0=0,Rg1=1,Rg2=2,Rg3=3,Rg4=4;

        [TestMethod][Timeout(5000)]public void MatrixMultiplication_IntegrationTest(){
            var p=P();Ld(p,
                I(Opcode.MOV,RW,1),  I(Opcode.MOV,RX,2),  I(Opcode.MOV,RY,3),  I(Opcode.MOV,RZ,4),
                I(Opcode.MOV,Rg0,5), I(Opcode.MOV,Rg1,6), I(Opcode.MOV,Rg2,7), I(Opcode.MOV,Rg3,8),
                R(Opcode.MUL,Rg0,RW,Rg0),  R(Opcode.MUL,Rg1,RX,Rg2),  R(Opcode.ADD,Rg4,Rg0,Rg1),
                H());p.Run();
            Assert.AreEqual(19,p.Registers[8].ToLong());
        }
    }
}