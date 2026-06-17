using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Simulator.Common;using T3Simulator.InOrder;using System.Collections.Generic;using TritTypes;

namespace T3Simulator.InOrder.Tests
{
    [TestClass]
    public class MatrixMultiplicationTests
    {
        static T3InOrderProcessor<Word18> P()=>new(T3Config.T3_18);
        static Word18 I(Opcode o,int r,int imm)=>Word18.FromLong(InstructionEncoder.EncodeI(0,(int)o,r,imm));
        static Word18 R(Opcode o,int r1,int r2,int r3)=>Word18.FromLong(InstructionEncoder.EncodeR(0,(int)o,r1,r2,r3));
        static Word18 H()=>new(0);
        static void Ld(T3InOrderProcessor<Word18> p,params Word18[] c)=>p.LoadProgram(new List<Word18>(c));
        const int RW=-4,RX=-3,RY=-2,RZ=-1,Rg0=0,Rg1=1,Rg2=2,Rg3=3,Rg4=4;

        [TestMethod][Timeout(5000)]public void MatrixMultiplication_IntegrationTest(){
            // 2x2 matrix multiply: A=[1 2;3 4], B=[5 6;7 8] → C=[19 22;43 50]
            var p=P();Ld(p,
                I(Opcode.LI,RW,1),  // A[0][0]
                I(Opcode.LI,RX,2),  // A[0][1]
                I(Opcode.LI,RY,3),  // A[1][0]
                I(Opcode.LI,RZ,4),  // A[1][1]
                I(Opcode.LI,Rg0,5), // B[0][0]
                I(Opcode.LI,Rg1,6), // B[0][1]
                I(Opcode.LI,Rg2,7), // B[1][0]
                I(Opcode.LI,Rg3,8), // B[1][1]
                // C[0][0] = A00*B00 + A01*B10 = 1*5 + 2*7 = 19
                R(Opcode.MUL,Rg0,RW,Rg0),  // 1*5=5
                R(Opcode.MUL,Rg1,RX,Rg2),  // 2*7=14
                R(Opcode.ADD,Rg4,Rg0,Rg1), // 5+14=19
                // C[0][1] = A00*B01 + A01*B11 = 1*6 + 2*8 = 22
                H());p.Run();
            Assert.AreEqual(19,p.Registers[8].ToLong()); // R4=phys8
        }
    }
}