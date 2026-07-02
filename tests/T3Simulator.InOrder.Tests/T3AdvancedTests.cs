using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Simulator.Common;using T3Simulator.InOrder;using System.Collections.Generic;using TritTypes;
namespace T3Simulator.InOrder.Tests{[TestClass]public class T3AdvancedTests{
static Word18 I(Opcode o,int r,long imm)=>Word18.FromLong(InstructionEncoder.EncodeI(0,0,+1,(int)o,r,imm));
static Word18 R(Opcode o,int r1,int r2,int r3)=>Word18.FromLong(InstructionEncoder.EncodeR(0,0,0,(int)o,r1,r2,r3));
static Word18 J(Opcode o,int r)=>Word18.FromLong(InstructionEncoder.EncodeJ(0,0,-1,(int)o,r));
static Word18 H()=>new(0);
static void Ld(T3InOrderProcessor<Word18> p,params Word18[] c)=>p.LoadProgram(new List<Word18>(c));
const int RW=-4,RX=-3,RY=-2,RZ=-1,R0=0,R1=1,R2=2,R3=3,R4=4;

[TestMethod,Timeout(3000)]public void ProcCall_Stack(){var p=new T3InOrderProcessor<Word18>(T3Config.T3_18);Ld(p,I(Opcode.MOV,RW,5),I(Opcode.MOV,RX,4),J(Opcode.CALL,RX),H(),I(Opcode.MOV,RY,2),R(Opcode.MUL,RW,RW,RY),I(Opcode.MOV,RY,1),R(Opcode.ADD,RW,RW,RY),R(Opcode.RET,0,0,0),H());p.Run();Assert.AreEqual(11,p.Registers[0].ToLong());}
[TestMethod,Timeout(3000)]public void NestedBranch(){var p=new T3InOrderProcessor<Word18>(T3Config.T3_18);Ld(p,I(Opcode.MOV,R3,5),I(Opcode.MOV,RW,10),I(Opcode.MOV,RX,20),R(Opcode.CMP,RW,RX,0),J(Opcode.JG,R3),I(Opcode.MOV,RW,1),H());p.Run();Assert.AreEqual(1,p.Registers[0].ToLong());}
[TestMethod,Timeout(3000)]public void DoubleLoop(){var p=new T3InOrderProcessor<Word18>(T3Config.T3_18);Ld(p,I(Opcode.MOV,RW,0),I(Opcode.MOV,RX,1),I(Opcode.MOV,RY,4),I(Opcode.MOV,R1,1),I(Opcode.MOV,R0,4),R(Opcode.ADD,RW,RW,RX),R(Opcode.ADD,RX,RX,R1),R(Opcode.CMP,RX,RY,RZ),J(Opcode.JL,R0),H());p.Run();Assert.AreEqual(6,p.Registers[0].ToLong());}
[TestMethod,Timeout(3000)]public void Fibonacci(){var p=new T3InOrderProcessor<Word18>(T3Config.T3_18);Ld(p,I(Opcode.MOV,RW,0),I(Opcode.MOV,RX,1),I(Opcode.MOV,RY,5),I(Opcode.MOV,R0,4),R(Opcode.ADD,R3,RW,RX),R(Opcode.MOV,RW,RX,0),R(Opcode.MOV,RX,R3,0),I(Opcode.MOV,R1,1),R(Opcode.ADD,RZ,RZ,R1),R(Opcode.CMP,RZ,RY,RZ),J(Opcode.JL,R0),H());p.Run();Assert.AreEqual(5,p.Registers[0].ToLong());}
[TestMethod,Timeout(3000)]public void ArrayAddition(){var p=new T3InOrderProcessor<Word18>(T3Config.T3_18);Ld(p,I(Opcode.MOV,RW,1),I(Opcode.MOV,RX,100),R(Opcode.ST,RW,RX,RZ),I(Opcode.MOV,RW,2),I(Opcode.MOV,R1,1),R(Opcode.ADD,RX,RX,R1),R(Opcode.ST,RW,RX,RZ),I(Opcode.MOV,RX,100),R(Opcode.LD,RW,RX,RZ),I(Opcode.MOV,R1,1),R(Opcode.ADD,RX,RX,R1),R(Opcode.LD,RZ,RX,RZ),R(Opcode.ADD,RW,RW,RZ),H());p.Run();Assert.AreEqual(3,p.Registers[0].ToLong());}
}}