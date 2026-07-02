using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Simulator.Common;using T3Simulator.InOrder;using System.Collections.Generic;using TritTypes;

namespace T3Simulator.InOrder.Tests{[TestClass]public class FpuInstructionTests{
static T3InOrderProcessor<Word18> P()=>new(T3Config.T3_18);
static Word18 I(Opcode o,int r,long imm)=>Word18.FromLong(InstructionEncoder.EncodeI(0,0,+1,(int)o,r,imm));
static Word18 IRg(int rg,Opcode o,int r,long imm)=>Word18.FromLong(InstructionEncoder.EncodeI(0,rg,+1,(int)o,r,imm));
static Word18 R(Opcode o,int r1,int r2,int r3)=>Word18.FromLong(InstructionEncoder.EncodeR(0,0,0,(int)o,r1,r2,r3));
static Word18 RRg(int rg,Opcode o,int r1,int r2,int r3)=>Word18.FromLong(InstructionEncoder.EncodeR(0,rg,0,(int)o,r1,r2,r3));
static Word18 H()=>new(0);
static void Ld(T3InOrderProcessor<Word18> p,params Word18[] c)=>p.LoadProgram(new List<Word18>(c));
const int Rg0=0,Rg1=1,Rg2=2,Rg3=3,Rg4=4;
const int FW=-4,FX=-3,FY=-2,FZ=-1,F0=0,F1=1,F2=2,F3=3,F4=4;
const int RW=-4,RX=-3,RY=-2,RZ=-1,R0=0,R1=1;

[TestMethod][Timeout(5000)]public void FADD_Works(){var p=P();Ld(p,I(Opcode.MOV,RW,3),I(Opcode.MOV,RX,4),RRg(-1,Opcode.ITF,FW,RW,0),RRg(-1,Opcode.ITF,FX,RX,0),RRg(-1,Opcode.ADD,FZ,FW,FX),RRg(-1,Opcode.FTI,RZ,FZ,0),H());p.Run();Assert.AreEqual(7,p.Registers[3].ToLong());}
[TestMethod][Timeout(5000)]public void FSUB_Works(){var p=P();Ld(p,I(Opcode.MOV,RW,10),I(Opcode.MOV,RX,3),RRg(-1,Opcode.ITF,FW,RW,0),RRg(-1,Opcode.ITF,FX,RX,0),RRg(-1,Opcode.SUB,FW,FW,FX),RRg(-1,Opcode.FTI,RW,FW,0),H());p.Run();Assert.AreEqual(7,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void FMUL_Works(){var p=P();Ld(p,I(Opcode.MOV,RW,3),I(Opcode.MOV,RX,4),RRg(-1,Opcode.ITF,FW,RW,0),RRg(-1,Opcode.ITF,FX,RX,0),RRg(-1,Opcode.MUL,FW,FW,FX),RRg(-1,Opcode.FTI,RW,FW,0),H());p.Run();Assert.AreEqual(12,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void FDIV_Works(){var p=P();Ld(p,I(Opcode.MOV,RW,12),I(Opcode.MOV,RX,3),RRg(-1,Opcode.ITF,FW,RW,0),RRg(-1,Opcode.ITF,FX,RX,0),RRg(-1,Opcode.DIV,FW,FW,FX),RRg(-1,Opcode.FTI,RW,FW,0),H());p.Run();Assert.AreEqual(4,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void FSQRT_Works(){var p=P();Ld(p,I(Opcode.MOV,RW,9),RRg(-1,Opcode.ITF,FW,RW,0),RRg(-1,Opcode.SQRT,FW,FW,0),RRg(-1,Opcode.FTI,RW,FW,0),H());p.Run();Assert.AreEqual(3,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void FABS_Works(){var p=P();Ld(p,I(Opcode.MOV,RW,-5),RRg(-1,Opcode.ITF,FW,RW,0),RRg(-1,Opcode.ABS,FW,FW,0),RRg(-1,Opcode.FTI,RW,FW,0),H());p.Run();Assert.AreEqual(5,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void FNEG_Works(){var p=P();Ld(p,I(Opcode.MOV,RW,5),RRg(-1,Opcode.ITF,FW,RW,0),RRg(-1,Opcode.NEG,FW,FW,0),RRg(-1,Opcode.FTI,RW,FW,0),H());p.Run();Assert.AreEqual(-5,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void FZERO_Works(){var p=P();Ld(p,IRg(-1,Opcode.MOV,FW,0),RRg(-1,Opcode.FTI,RW,FW,0),H());p.Run();Assert.AreEqual(0,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void FCMP_Greater(){var p=P();Ld(p,I(Opcode.MOV,RW,10),I(Opcode.MOV,RX,3),RRg(-1,Opcode.ITF,FW,RW,0),RRg(-1,Opcode.ITF,FX,RX,0),RRg(-1,Opcode.CMP,FW,FX,0),H());p.Run();Assert.AreEqual(1,p.CD);}
[TestMethod][Timeout(5000)]public void FCMP_Less(){var p=P();Ld(p,I(Opcode.MOV,RW,3),I(Opcode.MOV,RX,10),RRg(-1,Opcode.ITF,FW,RW,0),RRg(-1,Opcode.ITF,FX,RX,0),RRg(-1,Opcode.CMP,FW,FX,0),H());p.Run();Assert.AreEqual(-1,p.CD);}
[TestMethod][Timeout(5000)]public void FTOI_Works(){var p=P();Ld(p,I(Opcode.MOV,RW,42),RRg(-1,Opcode.ITF,FW,RW,0),RRg(-1,Opcode.FTI,RX,FW,0),H());p.Run();Assert.AreEqual(42,p.Registers[1].ToLong());}
[TestMethod][Timeout(5000)]public void ITOF_Works(){var p=P();Ld(p,I(Opcode.MOV,RW,27),RRg(-1,Opcode.ITF,FW,RW,0),RRg(-1,Opcode.FTI,RX,FW,0),H());p.Run();Assert.AreEqual(27,p.Registers[1].ToLong());}
[TestMethod][Timeout(5000)]public void FSW_FLW_RoundTrip(){var p=P();Ld(p,I(Opcode.MOV,RW,42),RRg(-1,Opcode.ITF,FW,RW,0),I(Opcode.MOV,Rg1,100),R(Opcode.ST,FW,Rg1,0),IRg(-1,Opcode.MOV,FW,0),R(Opcode.LD,FW,Rg1,0),RRg(-1,Opcode.FTI,Rg2,FW,0),H());p.Run();Assert.AreEqual(42,p.Registers[6].ToLong());}
[TestMethod][Timeout(5000)]public void FMOV_FtoF(){var p=P();Ld(p,I(Opcode.MOV,RW,10),RRg(-1,Opcode.ITF,FW,RW,0),RRg(-1,Opcode.MOV,FX,FW,0),RRg(-1,Opcode.FTI,RW,FX,0),H());p.Run();Assert.AreEqual(10,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void FMOV_RtoF(){var p=P();Ld(p,I(Opcode.MOV,RW,15),RRg(-1,Opcode.ITF,FW,RW,1),RRg(-1,Opcode.FTI,RX,FW,0),H());p.Run();Assert.AreEqual(15,p.Registers[1].ToLong());}
[TestMethod][Timeout(5000)]public void FMOV_FtoR(){var p=P();Ld(p,I(Opcode.MOV,RW,25),RRg(-1,Opcode.ITF,FW,RW,0),R(Opcode.MOV,Rg2,FW,2),H());p.Run();Assert.AreEqual(25,p.Registers[6].ToLong());}
[TestMethod][Timeout(5000)]public void FSWAP_Works(){var p=P();Ld(p,I(Opcode.MOV,RW,5),I(Opcode.MOV,Rg1,8),RRg(-1,Opcode.ITF,FW,RW,0),RRg(-1,Opcode.ITF,FX,Rg1,0),RRg(-1,Opcode.SWAP,FW,FX,0),RRg(-1,Opcode.FTI,RW,FW,0),H());p.Run();Assert.AreEqual(8,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void FCLASS_Works(){var p=P();Ld(p,I(Opcode.MOV,RW,5),RRg(-1,Opcode.ITF,FW,RW,0),RRg(-1,Opcode.CLASS,FW,FW,0),RRg(-1,Opcode.FTI,RW,FW,0),H());p.Run();Assert.AreEqual(4,p.Registers[0].ToLong());}
  [TestMethod][Timeout(5000)]public void FADD_Predicated_True(){var p=P();p.PR=Word18.FromLong(1);Ld(p,I(Opcode.MOV,RW,10),RRg(-1,Opcode.ITF,FW,RW,0),I(Opcode.MOV,RX,20),RRg(-1,Opcode.ITF,FX,RX,0),Word18.FromLong(InstructionEncoder.EncodeR(2,-1,0,(int)Opcode.ADD,FZ,FW,FX)),RRg(-1,Opcode.FTI,RZ,FZ,0),H());p.Run();Assert.AreEqual(0,p.Registers[3].ToLong());}
  [TestMethod][Timeout(5000)]public void FADD_Predicated_Honored(){var p=P();p.PR=Word18.FromLong(1);Ld(p,I(Opcode.MOV,RW,10),RRg(-1,Opcode.ITF,FW,RW,0),I(Opcode.MOV,RX,20),RRg(-1,Opcode.ITF,FX,RX,0),Word18.FromLong(InstructionEncoder.EncodeR(1,-1,0,(int)Opcode.ADD,FZ,FW,FX)),RRg(-1,Opcode.FTI,RZ,FZ,0),H());p.Run();Assert.AreEqual(30,p.Registers[3].ToLong());}
}}