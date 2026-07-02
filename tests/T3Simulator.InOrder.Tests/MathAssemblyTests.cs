using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Simulator.Common;using T3Simulator.InOrder;using System.Collections.Generic;using TritTypes;

namespace T3Simulator.InOrder.Tests{[TestClass]public class MathAssemblyTests{
static T3InOrderProcessor<Word18> P()=>new(T3Config.T3_18);
static Word18 I(Opcode o,int r,long imm)=>Word18.FromLong(InstructionEncoder.EncodeI(0,0,+1,(int)o,r,imm));
static Word18 R(Opcode o,int r1,int r2,int r3)=>Word18.FromLong(InstructionEncoder.EncodeR(0,0,0,(int)o,r1,r2,r3));
static Word18 J(Opcode o,int r)=>Word18.FromLong(InstructionEncoder.EncodeJ(0,0,-1,(int)o,r));
static Word18 H()=>new(0);
static void Ld(T3InOrderProcessor<Word18> p,params Word18[] c)=>p.LoadProgram(new List<Word18>(c));
const int Rw=-4,Rx=-3,Ry=-2,Rz=-1,R0=0,R1=1,R2=2,R3=3,R4=4;

[TestMethod][Timeout(5000)]public void Quadratic_Discriminant_Integer(){
    var p=P();Ld(p,I(Opcode.MOV,Rw,1),I(Opcode.MOV,Rx,-3),I(Opcode.MOV,Ry,2),I(Opcode.MOV,R1,4),
        R(Opcode.MUL,Rz,Rx,Rx),R(Opcode.MUL,R0,Rw,Ry),R(Opcode.MUL,R0,R0,R1),R(Opcode.SUB,Rz,Rz,R0),H());p.Run();
    Assert.AreEqual(1,p.Registers[3].ToLong());}
[TestMethod][Timeout(5000)]public void FpuCompareAndBranch_CorrectlyJumps(){
    var p=P();Ld(p,I(Opcode.MOV,R3,7),I(Opcode.MOV,Rw,10),I(Opcode.MOV,Rx,3),R(Opcode.ITF,-4,Rw,0),R(Opcode.ITF,-3,Rx,0),R(Opcode.CMP,-4,-3,0),J(Opcode.JG,R3),I(Opcode.MOV,Rw,1),H());p.Run();
    Assert.AreEqual(1,p.Registers[0].ToLong());}
[TestMethod][Timeout(5000)]public void TaylorSin_ApproximatesSinX(){
    var p=P();Ld(p,I(Opcode.MOV,Rw,1),R(Opcode.MUL,R1,Rw,Rw),R(Opcode.MUL,R1,R1,Rw),I(Opcode.DIV,R1,6),R(Opcode.SUB,Rw,Rw,R1),H());p.Run();
    Assert.AreEqual(1,p.Registers[0].ToLong());}
}}