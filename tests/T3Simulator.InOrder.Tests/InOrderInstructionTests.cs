using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Simulator.Common;using T3Simulator.InOrder;using System.Collections.Generic;using TritTypes;

namespace T3Simulator.InOrder.Tests{[TestClass]public class InOrderInstructionTests{
static T3InOrderProcessor<Word18> P()=>new(T3Config.T3_18);
static Word18 I(Opcode o,int r,int imm)=>Word18.FromLong(InstructionEncoder.EncodeI(0,(int)o,r,imm));
static Word18 R(Opcode o,int r1,int r2,int r3)=>Word18.FromLong(InstructionEncoder.EncodeR(0,(int)o,r1,r2,r3));
static Word18 J(Opcode o,int r)=>Word18.FromLong(InstructionEncoder.EncodeJ(0,(int)o,r));
static Word18 H()=>new(0);
static void Ld(T3InOrderProcessor<Word18> p,params Word18[] c)=>p.LoadProgram(new List<Word18>(c));
const int RW=-4,RX=-3,RY=-2,RZ=-1,R0=0,R1=1,R2=2,R3=3,R4=4;

[TestMethod][Timeout(5000)]public void Test_ADD(){var p=P();Ld(p,I(Opcode.LI,RW,10),I(Opcode.LI,RX,20),R(Opcode.ADD,RW,RW,RX),H());p.Run();Assert.AreEqual(30,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void Test_SUB(){var p=P();Ld(p,I(Opcode.LI,RW,50),I(Opcode.LI,RX,20),R(Opcode.SUB,RW,RW,RX),H());p.Run();Assert.AreEqual(30,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void Test_MUL(){var p=P();Ld(p,I(Opcode.LI,RW,5),I(Opcode.LI,RX,6),R(Opcode.MUL,RW,RW,RX),H());p.Run();Assert.AreEqual(30,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void Test_DIV(){var p=P();Ld(p,I(Opcode.LI,RW,30),I(Opcode.LI,RX,4),R(Opcode.DIV,RW,RW,RX),H());p.Run();Assert.AreEqual(7,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void Test_MOD(){var p=P();Ld(p,I(Opcode.LI,RW,30),I(Opcode.LI,RX,4),R(Opcode.MOD,RW,RW,RX),H());p.Run();Assert.AreEqual(2,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void Test_NEG(){var p=P();Ld(p,I(Opcode.LI,RW,15),R(Opcode.NEG,RW,RW,0),H());p.Run();Assert.AreEqual(-15,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void Test_AND(){var p=P();Ld(p,I(Opcode.LI,RW,1),I(Opcode.LI,RX,0),R(Opcode.AND,RW,RW,RX),H());p.Run();Assert.AreEqual(0,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void Test_OR(){var p=P();Ld(p,I(Opcode.LI,RW,1),I(Opcode.LI,RX,0),R(Opcode.OR,RW,RW,RX),H());p.Run();Assert.AreEqual(1,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void Test_XOR(){var p=P();Ld(p,I(Opcode.LI,RW,1),I(Opcode.LI,RX,-1),R(Opcode.XOR,RW,RW,RX),H());p.Run();Assert.AreEqual(0,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void Test_SHL(){var p=P();Ld(p,I(Opcode.LI,RW,5),I(Opcode.LI,RX,2),R(Opcode.SHL,RW,RW,RX),H());p.Run();Assert.AreEqual(45,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void Test_SHR(){var p=P();Ld(p,I(Opcode.LI,RW,45),I(Opcode.LI,RX,2),R(Opcode.SHR,RW,RW,RX),H());p.Run();Assert.AreEqual(5,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void Test_CMP_And_Branches(){
    var p=P();Ld(p,I(Opcode.LI,R3,5),I(Opcode.LI,RW,10),I(Opcode.LI,RX,20),R(Opcode.CMP,RW,RX,0),J(Opcode.JG,R3),I(Opcode.LI,RW,1),H());p.Run();Assert.AreEqual(1,p.Registers[0].ToLong());
}
[TestMethod][Timeout(5000)]public void Test_Call_Ret(){
    var p=P();Ld(p,I(Opcode.LI,RW,3),J(Opcode.CALL,RW),H(),I(Opcode.LI,R0,1),R(Opcode.ADD,RW,RW,R0),R(Opcode.RET,0,0,0),H());p.Run();Assert.AreEqual(11,p.Registers[0].ToLong());
}
[TestMethod][Timeout(5000)]public void Test_LIMM(){
    var p=P();Ld(p,I(Opcode.LIMM,RW,0),Word18.FromLong(12345),H());p.Run();Assert.AreEqual(12345,p.Registers[0].ToLong());
}
[TestMethod][Timeout(5000)]public void Test_LOAD_STORE(){
    var p=P();Ld(p,I(Opcode.LI,RW,100),I(Opcode.LI,RX,50),R(Opcode.STORE,RW,RX,0),I(Opcode.LI,RW,0),R(Opcode.LOAD,RW,RX,0),H());p.Run();Assert.AreEqual(100,p.Registers[0].ToLong());
}
[TestMethod][Timeout(5000)]public void Test_IO_Basic(){var p=P();var d=new Dev(42);p.SetOutputDevice(5,d);Ld(p,I(Opcode.LI,RW,5),I(Opcode.LI,R0,42),R(Opcode.OUT,R0,RW,0),H());p.Run();Assert.AreEqual(42,d.V);}
[TestMethod][Timeout(5000)]public void Test_JNE_JM(){
    var p=P();Ld(p,I(Opcode.LI,R3,5),I(Opcode.LI,RW,10),I(Opcode.LI,RX,20),R(Opcode.CMP,RW,RX,0),J(Opcode.JNE,R3),I(Opcode.LI,RW,-1),I(Opcode.LI,RW,1),H());p.Run();Assert.AreEqual(1,p.Registers[0].ToLong());
}
class Dev:IDevice<Word18>{public long V;long i;public Dev(long iv){i=iv;}public Word18 Read()=>Word18.FromLong(i);public void Write(Word18 v)=>V=v.ToLong();public bool DataReady=>true;}
}}