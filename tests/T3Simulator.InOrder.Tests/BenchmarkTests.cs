using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Simulator.Common;using T3Simulator.InOrder;using System.Collections.Generic;using System;using TritTypes;

namespace T3Simulator.InOrder.Tests{[TestClass]public class BenchmarkTests{
static T3InOrderProcessor<Word18> P()=>new(T3Config.T3_18);
static Word18 I(Opcode o,int r,int imm)=>Word18.FromLong(InstructionEncoder.EncodeI(0,(int)o,r,imm));
static Word18 R(Opcode o,int r1,int r2,int r3)=>Word18.FromLong(InstructionEncoder.EncodeR(0,(int)o,r1,r2,r3));
static Word18 J(Opcode o,int r)=>Word18.FromLong(InstructionEncoder.EncodeJ(0,(int)o,r));
static Word18 H()=>new(0);
static void Ld(T3InOrderProcessor<Word18> p,params Word18[] c)=>p.LoadProgram(new List<Word18>(c));
const int Rw=-4,Rx=-3,Ry=-2,Rz=-1,R0=0,R1=1,R2=2,R3=3,R4=4;

[TestMethod][Timeout(5000)]public void Simple_ADD_SUB_MUL(){var p=P();Ld(p,I(Opcode.LI,Rw,10),I(Opcode.LI,Rx,20),R(Opcode.ADD,Rw,Rw,Rx),R(Opcode.SUB,Ry,Rx,Rw),R(Opcode.MUL,Rz,Rw,Ry),H());p.Run();Assert.AreEqual(30,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void DhrystoneMini(){var p=P();Ld(p,I(Opcode.LI,Rw,5),I(Opcode.LI,Rx,3),R(Opcode.MUL,Ry,Rw,Rw),R(Opcode.MUL,Rz,Rx,Rx),R(Opcode.ADD,R0,Ry,Rz),H());p.Run();Assert.AreEqual(34,p.Registers[4].ToLong());}
[TestMethod][Timeout(5000)]public void WhetstoneMini(){var p=P();Ld(p,I(Opcode.LI,Rw,3),I(Opcode.LI,Rx,4),R(Opcode.ITOF,-4,Rw,0),R(Opcode.ITOF,-3,Rx,0),R(Opcode.FADD,-4,-4,-3),R(Opcode.FTOI,Rw,-4,0),H());p.Run();Assert.AreEqual(7,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void MWMIPS_Mini(){var p=P();Ld(p,I(Opcode.LI,Rw,42),I(Opcode.LI,Rx,50),R(Opcode.STORE,Rw,Rx,0),I(Opcode.LI,Ry,0),R(Opcode.LOAD,Ry,Rx,0),H());p.Run();Assert.AreEqual(42,p.Registers[2].ToLong());}
[TestMethod][Timeout(5000)]public void CallRet_Stack(){var p=P();Ld(p,I(Opcode.LI,Rw,4),J(Opcode.CALL,Rw),H(),I(Opcode.LI,R1,99),R(Opcode.RET,0,0,0),H());p.Run();Assert.AreEqual(99,p.Registers[5].ToLong());}
[TestMethod][Timeout(5000)]public void Loop_Sum10(){var p=P();Ld(p,
    I(Opcode.LI,Rw,0),I(Opcode.LI,Rx,1),I(Opcode.LI,Ry,11),I(Opcode.LI,Rz,0),I(Opcode.LI,R0,6), // addr for JMP
    R(Opcode.ADD,Rw,Rw,Rx),R(Opcode.ADD,Rx,Rx,R1),R(Opcode.CMP,Rx,Ry,Rz),J(Opcode.JNE,R0),H());
    p.Run();Assert.AreEqual(55,p.Registers[0].ToLong());}
}}