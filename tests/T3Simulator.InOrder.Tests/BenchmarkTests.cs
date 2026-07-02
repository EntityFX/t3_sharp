using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Simulator.Common;
using T3Simulator.InOrder;
using System.Collections.Generic;
using TritTypes;
namespace T3Simulator.InOrder.Tests
{
    [TestClass]
    public class BenchmarkTests
    {
        static Word18 I(Opcode o, int r, long imm) => Word18.FromLong(InstructionEncoder.EncodeI(0,0,+1,(int)o,r,imm));
        static Word18 R(Opcode o, int r1, int r2, int r3) => Word18.FromLong(InstructionEncoder.EncodeR(0,0,0,(int)o,r1,r2,r3));
        static Word18 J(Opcode o, int r) => Word18.FromLong(InstructionEncoder.EncodeJ(0,0,-1,(int)o,r));
        static Word18 H() => new(0);
        static void Ld(T3InOrderProcessor<Word18> p, params Word18[] c) => p.LoadProgram(new List<Word18>(c));
        const int RW = -4, RX = -3, RY = -2, RZ = -1, R0 = 0, R1 = 1, R2 = 2, R3 = 3, R4 = 4;

        [TestMethod, Timeout(3000)] public void ADD() { var p = new T3InOrderProcessor<Word18>(T3Config.T3_18); Ld(p, I(Opcode.MOV, RW, 10), I(Opcode.MOV, RX, 20), R(Opcode.ADD, RW, RW, RX), H()); p.Run(); Assert.AreEqual(30, p.Registers[0].ToLong()); }
        [TestMethod, Timeout(3000)] public void SUB() { var p = new T3InOrderProcessor<Word18>(T3Config.T3_18); Ld(p, I(Opcode.MOV, RW, 50), I(Opcode.MOV, RX, 20), R(Opcode.SUB, RW, RW, RX), H()); p.Run(); Assert.AreEqual(30, p.Registers[0].ToLong()); }
        [TestMethod, Timeout(3000)] public void MUL() { var p = new T3InOrderProcessor<Word18>(T3Config.T3_18); Ld(p, I(Opcode.MOV, RW, 5), I(Opcode.MOV, RX, 6), R(Opcode.MUL, RW, RW, RX), H()); p.Run(); Assert.AreEqual(30, p.Registers[0].ToLong()); }
        [TestMethod, Timeout(3000)] public void DIV() { var p = new T3InOrderProcessor<Word18>(T3Config.T3_18); Ld(p, I(Opcode.MOV, RW, 30), I(Opcode.MOV, RX, 4), R(Opcode.DIV, RW, RW, RX), H()); p.Run(); Assert.AreEqual(7, p.Registers[0].ToLong()); }
        [TestMethod, Timeout(3000)] public void MOD() { var p = new T3InOrderProcessor<Word18>(T3Config.T3_18); Ld(p, I(Opcode.MOV, RW, 30), I(Opcode.MOV, RX, 4), R(Opcode.MOD, RW, RW, RX), H()); p.Run(); Assert.AreEqual(2, p.Registers[0].ToLong()); }
        [TestMethod, Timeout(3000)] public void NEG() { var p = new T3InOrderProcessor<Word18>(T3Config.T3_18); Ld(p, I(Opcode.MOV, RW, 15), R(Opcode.NEG, RW, RW, 0), H()); p.Run(); Assert.AreEqual(-15, p.Registers[0].ToLong()); }
        [TestMethod, Timeout(3000)] public void AND() { var p = new T3InOrderProcessor<Word18>(T3Config.T3_18); Ld(p, I(Opcode.MOV, RW, 1), I(Opcode.MOV, RX, 0), R(Opcode.AND, RW, RW, RX), H()); p.Run(); Assert.AreEqual(0, p.Registers[0].ToLong()); }
        [TestMethod, Timeout(3000)] public void OR() { var p = new T3InOrderProcessor<Word18>(T3Config.T3_18); Ld(p, I(Opcode.MOV, RW, 1), I(Opcode.MOV, RX, 0), R(Opcode.OR, RW, RW, RX), H()); p.Run(); Assert.AreEqual(1, p.Registers[0].ToLong()); }
        [TestMethod, Timeout(3000)] public void XOR() { var p = new T3InOrderProcessor<Word18>(T3Config.T3_18); Ld(p, I(Opcode.MOV, RW, 1), I(Opcode.MOV, RX, -1), R(Opcode.XOR, RW, RW, RX), H()); p.Run(); Assert.AreEqual(0, p.Registers[0].ToLong()); }
        [TestMethod, Timeout(3000)] public void SHL() { var p = new T3InOrderProcessor<Word18>(T3Config.T3_18); Ld(p, I(Opcode.MOV, RW, 5), I(Opcode.MOV, RX, 2), R(Opcode.SHL, RW, RW, RX), H()); p.Run(); Assert.AreEqual(45, p.Registers[0].ToLong()); }
        [TestMethod, Timeout(3000)] public void SHR() { var p = new T3InOrderProcessor<Word18>(T3Config.T3_18); Ld(p, I(Opcode.MOV, RW, 45), I(Opcode.MOV, RX, 2), R(Opcode.SHR, RW, RW, RX), H()); p.Run(); Assert.AreEqual(5, p.Registers[0].ToLong()); }
        [TestMethod, Timeout(3000)] public void CMP() { var p = new T3InOrderProcessor<Word18>(T3Config.T3_18); Ld(p, I(Opcode.MOV, RW, 10), I(Opcode.MOV, RX, 20), R(Opcode.CMP, RW, RX, RZ), H()); p.Run(); Assert.AreEqual(-1, p.CD); }
        [TestMethod, Timeout(3000)] public void LOAD_STORE() { var p = new T3InOrderProcessor<Word18>(T3Config.T3_18); Ld(p, I(Opcode.MOV, RW, 100), I(Opcode.MOV, RX, 50), R(Opcode.ST, RW, RX, 0), I(Opcode.MOV, RW, 0), R(Opcode.LD, RW, RX, 0), H()); p.Run(); Assert.AreEqual(100, p.Registers[0].ToLong()); }
        [TestMethod, Timeout(3000)] public void PUSH_POP() { var p = new T3InOrderProcessor<Word18>(T3Config.T3_18); Ld(p, I(Opcode.MOV, RW, 10), R(Opcode.PUSH, RW, 0, 0), I(Opcode.MOV, RW, 20), R(Opcode.POP, RW, 0, 0), H()); p.Run(); Assert.AreEqual(10, p.Registers[0].ToLong()); }
        [TestMethod, Timeout(3000)] public void ADDI() { var p = new T3InOrderProcessor<Word18>(T3Config.T3_18); Ld(p, I(Opcode.MOV, RW, 10), I(Opcode.ADD, RW, 5), H()); p.Run(); Assert.AreEqual(15, p.Registers[0].ToLong()); }
        [TestMethod, Timeout(3000)] public void CALL_RET() { var p = new T3InOrderProcessor<Word18>(T3Config.T3_18); Ld(p, I(Opcode.MOV, RW, 10), I(Opcode.MOV, RX, 4), J(Opcode.CALL, RX), H(), I(Opcode.MOV, R1, 1), R(Opcode.ADD, RW, RW, R1), R(Opcode.RET, 0, 0, 0), H()); p.Run(); Assert.AreEqual(11, p.Registers[0].ToLong()); }
        [TestMethod, Timeout(3000)] public void Predication() { var p = new T3InOrderProcessor<Word18>(T3Config.T3_18); p.PR = Word18.FromLong(1); p.Registers[1] = Word18.FromLong(10); p.Registers[2] = Word18.FromLong(20); Ld(p, Word18.FromLong(InstructionEncoder.EncodeR(1,0,0,(int)Opcode.ADD, RW, RX, RY)), H()); p.Run(); Assert.AreEqual(0, p.Registers[0].ToLong()); }
        [TestMethod, Timeout(3000)] public void Quadratic() { var p = new T3InOrderProcessor<Word18>(T3Config.T3_18); Ld(p, I(Opcode.MOV, RW, 1), I(Opcode.MOV, RX, -3), I(Opcode.MOV, RY, 2), R(Opcode.MUL, RZ, RX, RX), R(Opcode.MUL, R0, RW, RY), I(Opcode.MOV, R1, 4), R(Opcode.MUL, R0, R0, R1), R(Opcode.SUB, RZ, RZ, R0), H()); p.Run(); Assert.AreEqual(1, p.Registers[3].ToLong()); }
        [TestMethod, Timeout(3000)] public void Branch() { var p = new T3InOrderProcessor<Word18>(T3Config.T3_18); Ld(p, I(Opcode.MOV, R3, 5), I(Opcode.MOV, RW, 10), I(Opcode.MOV, RX, 20), R(Opcode.CMP, RW, RX, RZ), J(Opcode.JG, R3), I(Opcode.MOV, RW, 1), H()); p.Run(); Assert.AreEqual(1, p.Registers[0].ToLong()); }
        [TestMethod, Timeout(3000)] public void LoopSum() { var p = new T3InOrderProcessor<Word18>(T3Config.T3_18); Ld(p, I(Opcode.MOV, RW, 0), I(Opcode.MOV, RX, 1), I(Opcode.MOV, RY, 6), I(Opcode.MOV, R1, 1), I(Opcode.MOV, R0, 4), R(Opcode.ADD, RW, RW, RX), R(Opcode.ADD, RX, RX, R1), R(Opcode.CMP, RX, RY, RZ), J(Opcode.JNE, R0), H()); p.Run(); Assert.AreEqual(15, p.Registers[0].ToLong()); }
        [TestMethod, Timeout(3000)] public void DoubleLoop() { var p = new T3InOrderProcessor<Word18>(T3Config.T3_18); Ld(p, I(Opcode.MOV, RW, 0), I(Opcode.MOV, RX, 1), I(Opcode.MOV, RY, 4), I(Opcode.MOV, R1, 1), I(Opcode.MOV, R0, 4), R(Opcode.ADD, RW, RW, RX), R(Opcode.ADD, RX, RX, R1), R(Opcode.CMP, RX, RY, RZ), J(Opcode.JL, R0), H()); p.Run(); Assert.AreEqual(6, p.Registers[0].ToLong()); }
        [TestMethod, Timeout(3000)] public void Fibonacci() { var p = new T3InOrderProcessor<Word18>(T3Config.T3_18); Ld(p, I(Opcode.MOV, RW, 0), I(Opcode.MOV, RX, 1), I(Opcode.MOV, RY, 5), I(Opcode.MOV, R0, 4), R(Opcode.ADD, R3, RW, RX), R(Opcode.MOV, RW, RX, 0), R(Opcode.MOV, RX, R3, 0), I(Opcode.MOV, R1, 1), R(Opcode.ADD, RZ, RZ, R1), R(Opcode.CMP, RZ, RY, RZ), J(Opcode.JL, R0), H()); p.Run(); Assert.AreEqual(5, p.Registers[0].ToLong()); }
    }
}