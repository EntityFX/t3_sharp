using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Simulator.Common;using T3Simulator.InOrder;using System.Collections.Generic;using TritTypes;

namespace T3Simulator.InOrder.Tests{[TestClass]public class FpuInstructionTests{
static T3InOrderProcessor<Word18> P()=>new(T3Config.T3_18);
static Word18 I(Opcode o,int r,int imm)=>Word18.FromLong(InstructionEncoder.EncodeI(0,(int)o,r,imm));
static Word18 R(Opcode o,int r1,int r2,int r3)=>Word18.FromLong(InstructionEncoder.EncodeR(0,(int)o,r1,r2,r3));
static Word18 J(Opcode o,int r)=>Word18.FromLong(InstructionEncoder.EncodeJ(0,(int)o,r));
static Word18 H()=>new(0);
static void Ld(T3InOrderProcessor<Word18> p,params Word18[] c)=>p.LoadProgram(new List<Word18>(c));
const int Rg0=0,Rg1=1,Rg2=2,Rg3=3,Rg4=4;
const int FW=-4,FX=-3,FY=-2,FZ=-1,F0=0,F1=1,F2=2,F3=3,F4=4;
const int RW=-4,RX=-3,RY=-2,RZ=-1,R0=0,R1=1;


[TestMethod][Timeout(5000)]public void FADD_Works(){var p=P();Ld(p,I(Opcode.LI,RW,3),I(Opcode.LI,RX,4),R(Opcode.ITOF,FW,RW,0),R(Opcode.ITOF,FX,RX,0),R(Opcode.FADD,FZ,FW,FX),R(Opcode.FTOI,RZ,FZ,0),H());p.Run();Assert.AreEqual(7,p.Registers[3].ToLong());}
[TestMethod][Timeout(5000)]public void FSUB_Works(){var p=P();Ld(p,I(Opcode.LI,RW,10),I(Opcode.LI,RX,3),R(Opcode.ITOF,FW,RW,0),R(Opcode.ITOF,FX,RX,0),R(Opcode.FSUB,FW,FW,FX),R(Opcode.FTOI,RW,FW,0),H());p.Run();Assert.AreEqual(7,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void FMUL_Works(){var p=P();Ld(p,I(Opcode.LI,RW,3),I(Opcode.LI,RX,4),R(Opcode.ITOF,FW,RW,0),R(Opcode.ITOF,FX,RX,0),R(Opcode.FMUL,FW,FW,FX),R(Opcode.FTOI,RW,FW,0),H());p.Run();Assert.AreEqual(12,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void FDIV_Works(){var p=P();Ld(p,I(Opcode.LI,RW,12),I(Opcode.LI,RX,3),R(Opcode.ITOF,FW,RW,0),R(Opcode.ITOF,FX,RX,0),R(Opcode.FDIV,FW,FW,FX),R(Opcode.FTOI,RW,FW,0),H());p.Run();Assert.AreEqual(4,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void FSQRT_Works(){var p=P();Ld(p,I(Opcode.LI,RW,9),R(Opcode.ITOF,FW,RW,0),R(Opcode.FSQRT,FW,FW,0),R(Opcode.FTOI,RW,FW,0),H());p.Run();Assert.AreEqual(3,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void FABS_Works(){var p=P();Ld(p,I(Opcode.LI,RW,-5),R(Opcode.ITOF,FW,RW,0),R(Opcode.FABS,FW,FW,0),R(Opcode.FTOI,RW,FW,0),H());p.Run();Assert.AreEqual(5,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void FNEG_Works(){var p=P();Ld(p,I(Opcode.LI,RW,5),R(Opcode.ITOF,FW,RW,0),R(Opcode.FNEG,FW,FW,0),R(Opcode.FTOI,RW,FW,0),H());p.Run();Assert.AreEqual(-5,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void FZERO_Works(){var p=P();Ld(p,I(Opcode.FZERO,FW,0),R(Opcode.FTOI,RW,FW,0),H());p.Run();Assert.AreEqual(0,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void FCMP_Greater(){var p=P();Ld(p,I(Opcode.LI,RW,10),I(Opcode.LI,RX,3),R(Opcode.ITOF,FW,RW,0),R(Opcode.ITOF,FX,RX,0),R(Opcode.FCMP,FW,FX,0),H());p.Run();Assert.AreEqual(1,p.Cond);}
[TestMethod][Timeout(5000)]public void FCMP_Less(){var p=P();Ld(p,I(Opcode.LI,RW,3),I(Opcode.LI,RX,10),R(Opcode.ITOF,FW,RW,0),R(Opcode.ITOF,FX,RX,0),R(Opcode.FCMP,FW,FX,0),H());p.Run();Assert.AreEqual(-1,p.Cond);}
[TestMethod][Timeout(5000)]public void FTOI_Works(){var p=P();Ld(p,I(Opcode.LI,RW,42),R(Opcode.ITOF,FW,RW,0),R(Opcode.FTOI,RX,FW,0),H());p.Run();Assert.AreEqual(42,p.Registers[1].ToLong());}
[TestMethod][Timeout(5000)]public void ITOF_Works(){var p=P();Ld(p,I(Opcode.LI,RW,27),R(Opcode.ITOF,FW,RW,0),R(Opcode.FTOI,RX,FW,0),H());p.Run();Assert.AreEqual(27,p.Registers[1].ToLong());}
[TestMethod][Timeout(5000)]public void FSW_FLW_RoundTrip(){var p=P();Ld(p,I(Opcode.LI,RW,42),R(Opcode.ITOF,FW,RW,0),I(Opcode.LI,Rg1,100),R(Opcode.FSW,FW,Rg1,0),I(Opcode.FZERO,FW,0),R(Opcode.FLW,FW,Rg1,0),R(Opcode.FTOI,Rg2,FW,0),H());p.Run();Assert.AreEqual(42,p.Registers[6].ToLong());}
[TestMethod][Timeout(5000)]public void FMOV_FtoF(){var p=P();Ld(p,I(Opcode.LI,RW,10),R(Opcode.ITOF,FW,RW,0),R(Opcode.FMOV,FX,FW,0),R(Opcode.FTOI,RW,FX,0),H());p.Run();Assert.AreEqual(10,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void FMOV_RtoF(){var p=P();Ld(p,I(Opcode.LI,RW,15),R(Opcode.FMOV,FW,RW,1),R(Opcode.FTOI,RX,FW,0),H());p.Run();Assert.AreEqual(15,p.Registers[1].ToLong());}
[TestMethod][Timeout(5000)]public void FMOV_FtoR(){var p=P();Ld(p,I(Opcode.LI,RW,25),R(Opcode.ITOF,FW,RW,0),R(Opcode.FMOV,Rg2,FW,2),H());p.Run();Assert.AreEqual(25,p.Registers[6].ToLong());}
[TestMethod][Timeout(5000)]public void FSWAP_Works(){var p=P();Ld(p,I(Opcode.LI,RW,5),I(Opcode.LI,Rg1,8),R(Opcode.ITOF,FW,RW,0),R(Opcode.ITOF,FX,Rg1,0),R(Opcode.FSWAP,FW,FX,0),R(Opcode.FTOI,RW,FW,0),H());p.Run();Assert.AreEqual(8,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void FCLASS_Works(){var p=P();Ld(p,I(Opcode.LI,RW,5),R(Opcode.ITOF,FW,RW,0),R(Opcode.FCLASS,FW,FW,0),R(Opcode.FTOI,RW,FW,0),H());p.Run();Assert.AreEqual(4,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void FADD_Predicated_True(){var p=P();p.PR=Word18.FromLong(531441);Ld(p,R(Opcode.LI,RW,10,0),R(Opcode.LI,RX,20,0),R(Opcode.FADD,RW,RW,RX),H());p.Run();Assert.AreEqual(0,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void FADD_Predicated_Honored(){var p=P();p.PR=Word18.FromLong(531441);Ld(p,Word18.FromLong(InstructionEncoder.EncodeR(1,(int)Opcode.LI,RW,10,0)),Word18.FromLong(InstructionEncoder.EncodeR(1,(int)Opcode.LI,RX,20,0)),Word18.FromLong(InstructionEncoder.EncodeR(1,(int)Opcode.FADD,RW,RW,RX)),H());p.Run();Assert.AreEqual(30,p.Registers[0].ToLong());}
}}