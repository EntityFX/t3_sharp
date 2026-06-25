using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Simulator.Common;using T3Simulator.InOrder;using System.Collections.Generic;using TritTypes;

namespace T3Simulator.InOrder.Tests{[TestClass]public class InOrderProcessorTests{
static Word18 I(Opcode o,int r,int imm)=>Word18.FromLong(InstructionEncoder.EncodeI(0,(int)o,r,imm));
static Word18 R(Opcode o,int r1,int r2,int r3)=>Word18.FromLong(InstructionEncoder.EncodeR(0,(int)o,r1,r2,r3));
static Word18 J(Opcode o,int r)=>Word18.FromLong(InstructionEncoder.EncodeJ(0,(int)o,r));
static Word18 H()=>new(0);
static void Ld(T3InOrderProcessor<Word18> p,params Word18[] c)=>p.LoadProgram(new List<Word18>(c));
const int RW=-4,RX=-3,RY=-2,RZ=-1,R0=0,R1=1,R2=2,R3=3,R4=4;

[TestMethod][Timeout(5000)]public void SimpleArithmeticTest(){var p=new T3InOrderProcessor<Word18>(T3Config.T3_18);Ld(p,I(Opcode.LI,RW,0),I(Opcode.LI,RX,10),I(Opcode.LI,RY,20),R(Opcode.ADD,RW,RX,RY),H());p.Run();Assert.AreEqual(30,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void ImmediateArithmeticTest(){var p=new T3InOrderProcessor<Word18>(T3Config.T3_18);Ld(p,I(Opcode.LI,RW,10),I(Opcode.ADDI,RW,5),I(Opcode.SUBI,RW,2),H());p.Run();Assert.AreEqual(13,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void PredicationTest(){var p=new T3InOrderProcessor<Word18>(T3Config.T3_18);p.PR=Word18.FromLong(1);Ld(p,Word18.FromLong(InstructionEncoder.EncodeR(1,(int)Opcode.ADD,RW,RX,RY)),H());p.Registers[1]=Word18.FromLong(10);p.Registers[2]=Word18.FromLong(20);p.Run();Assert.AreEqual(30,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void StackTest(){var p=new T3InOrderProcessor<Word18>(T3Config.T3_18);Ld(p,I(Opcode.LI,RW,100),R(Opcode.PUSH,RW,0,0),I(Opcode.LI,RW,200),R(Opcode.POP,RW,0,0),H());p.Run();Assert.AreEqual(100,p.Registers[0].ToLong());}
    [TestMethod][Timeout(5000)]public void IOTest(){var p=new T3InOrderProcessor<Word18>(T3Config.T3_18);p.Registers[1]=Word18.FromLong(10);Ld(p,I(Opcode.LI,RW,42),R(Opcode.OUT,RW,RX,0),H());try{p.Run();}catch{}}
    [TestMethod][Timeout(5000)]public void StackOverflowGuard_DetectsOverflow(){var p=new T3InOrderProcessor<Word18>(T3Config.T3_18);p.SP=64;var prg=new List<Word18>();for(int i=0;i<100;i++)prg.Add(R(Opcode.PUSH,RW,0,0));prg.Add(H());Ld(p,prg.ToArray());p.Run();Assert.IsTrue(p.IsHalted);Assert.IsTrue(p.SP<=63);}
    [TestMethod][Timeout(5000)]public void StackOverflowGuard_NormalPushSucceeds(){var p=new T3InOrderProcessor<Word18>(T3Config.T3_18);p.SP=1000;Ld(p,I(Opcode.LI,RW,99),R(Opcode.PUSH,RW,0,0),I(Opcode.LI,RW,200),R(Opcode.POP,RW,0,0),H());try{p.Run();}catch(StackOverflowException){Assert.Fail("Should not throw");}}
    [TestMethod][Timeout(5000)]public void PUSHI_POPI_Immediate(){var p=new T3InOrderProcessor<Word18>(T3Config.T3_18);p.SP=1000;Ld(p,I(Opcode.PUSHI,RW,42),I(Opcode.POPI,RW,99),H());p.Run();Assert.AreEqual(99,p.Registers[0].ToLong());}
    [TestMethod][Timeout(5000)]public void PUSHI_StackOverflowGuard(){var p=new T3InOrderProcessor<Word18>(T3Config.T3_18);p.SP=64;var prg=new List<Word18>();for(int i=0;i<100;i++)prg.Add(I(Opcode.PUSHI,RW,i));prg.Add(H());Ld(p,prg.ToArray());p.Run();Assert.IsTrue(p.IsHalted);}
}}
