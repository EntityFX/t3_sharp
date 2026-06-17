using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Simulator.Common;using T3Simulator.InOrder;using System.Collections.Generic;using TritTypes;

namespace T3Simulator.InOrder.Tests
{
    [TestClass]
    public class InOrderInstructionTests
    {
        // Phys: RW=0 RX=1 RY=2 RZ=3 R0=4 R1=5 R2=6 R3=7 R4=8
        // Asm R0-R4→phys4-8. Tests encode phys indices directly.
        static string BT(long v,int d)=>BalancedTernary.ToTernaryString(v).PadLeft(d,'0')[^d..];
        static Word18 R(Opcode o,int r1,int r2,int r3=0)=>Word18.FromLong(BalancedTernary.ParseToLong("000"+BT((int)o,6)+BT(r1,3)+BT(r2,3)+BT(r3,3)));
        static Word18 I(Opcode o,int r,long imm)=>Word18.FromLong(BalancedTernary.ParseToLong("000"+BT((int)o,6)+BT(r,3)+BT(imm,6)));
        static Word18 J(Opcode o,int r)=>Word18.FromLong(BalancedTernary.ParseToLong("000"+BT((int)o,6)+BT(r,3)+"000000"));
        static Word18 H()=>Word18.FromLong(0);
        static T3InOrderProcessor<Word18> P()=>new(T3Config.T3_18);

        [TestMethod] public void Test_ADD(){var p=P();p.LoadProgram(new List<Word18>{I(Opcode.LI,0,10),I(Opcode.LI,1,20),R(Opcode.ADD,0,0,1),H()});p.Run();Assert.AreEqual(30,p.Registers[0].ToLong());}
        [TestMethod] public void Test_SUB(){var p=P();p.LoadProgram(new List<Word18>{I(Opcode.LI,0,50),I(Opcode.LI,1,20),R(Opcode.SUB,0,0,1),H()});p.Run();Assert.AreEqual(30,p.Registers[0].ToLong());}
        [TestMethod] public void Test_MUL(){var p=P();p.LoadProgram(new List<Word18>{I(Opcode.LI,0,5),I(Opcode.LI,1,6),R(Opcode.MUL,0,0,1),H()});p.Run();Assert.AreEqual(30,p.Registers[0].ToLong());}
        [TestMethod] public void Test_DIV(){var p=P();p.LoadProgram(new List<Word18>{I(Opcode.LI,0,30),I(Opcode.LI,1,4),R(Opcode.DIV,0,0,1),H()});p.Run();Assert.AreEqual(7,p.Registers[0].ToLong());}
        [TestMethod] public void Test_MOD(){var p=P();p.LoadProgram(new List<Word18>{I(Opcode.LI,0,30),I(Opcode.LI,1,4),R(Opcode.MOD,0,0,1),H()});p.Run();Assert.AreEqual(2,p.Registers[0].ToLong());}
        [TestMethod] public void Test_NEG(){var p=P();p.LoadProgram(new List<Word18>{I(Opcode.LI,0,15),R(Opcode.NEG,0,0),H()});p.Run();Assert.AreEqual(-15,p.Registers[0].ToLong());}
        [TestMethod] public void Test_AND(){var p=P();p.LoadProgram(new List<Word18>{I(Opcode.LI,0,1),I(Opcode.LI,1,0),R(Opcode.AND,0,0,1),H()});p.Run();Assert.AreEqual(0,p.Registers[0].ToLong());}
        [TestMethod] public void Test_OR(){var p=P();p.LoadProgram(new List<Word18>{I(Opcode.LI,0,1),I(Opcode.LI,1,0),R(Opcode.OR,0,0,1),H()});p.Run();Assert.AreEqual(1,p.Registers[0].ToLong());}
        [TestMethod] public void Test_XOR(){var p=P();p.LoadProgram(new List<Word18>{I(Opcode.LI,0,1),I(Opcode.LI,1,1),R(Opcode.XOR,0,0,1),H()});p.Run();Assert.AreEqual(-1,p.Registers[0].ToLong());}
        [TestMethod] public void Test_SHL(){var p=P();p.LoadProgram(new List<Word18>{I(Opcode.LI,0,2),I(Opcode.LI,1,2),R(Opcode.SHL,0,0,1),H()});p.Run();Assert.AreEqual(18,p.Registers[0].ToLong());}
        [TestMethod] public void Test_SHR(){var p=P();p.LoadProgram(new List<Word18>{I(Opcode.LI,0,18),I(Opcode.LI,1,2),R(Opcode.SHR,0,0,1),H()});p.Run();Assert.AreEqual(2,p.Registers[0].ToLong());}
        [TestMethod] public void Test_CMP_And_Branches(){var p=P();p.LoadProgram(new List<Word18>{I(Opcode.LI,6,6),I(Opcode.LI,0,10),I(Opcode.LI,1,20),R(Opcode.CMP,0,1),J(Opcode.JG,6),I(Opcode.LI,0,1),H()});p.Run();Assert.AreEqual(1,p.Registers[0].ToLong());}
        [TestMethod] public void Test_Loop_Sum(){var p=P();p.LoadProgram(new List<Word18>{I(Opcode.LI,0,0),I(Opcode.LI,1,1),I(Opcode.LI,6,6),I(Opcode.LI,7,1),I(Opcode.LI,5,6),I(Opcode.LI,4,11),R(Opcode.ADD,0,0,1),R(Opcode.ADD,1,1,7),R(Opcode.CMP,1,6),J(Opcode.JE,4),J(Opcode.JMP,5),H()});p.Run();Assert.AreEqual(15,p.Registers[0].ToLong());}
        [TestMethod] public void Test_Call_Ret(){var p=P();p.LoadProgram(new List<Word18>{I(Opcode.LI,0,10),I(Opcode.LI,5,4),J(Opcode.CALL,5),H(),I(Opcode.LI,1,1),R(Opcode.ADD,0,0,1),R(Opcode.RET,0,0)});p.Run();Assert.AreEqual(11,p.Registers[0].ToLong());}
        [TestMethod] public void Test_Push_Pop(){var p=P();p.LoadProgram(new List<Word18>{I(Opcode.LI,0,10),R(Opcode.PUSH,0,0),I(Opcode.LI,0,20),R(Opcode.POP,0,0),H()});p.Run();Assert.AreEqual(10,p.Registers[0].ToLong());}
        [TestMethod] public void Test_LIMM(){var p=P();p.LoadProgram(new List<Word18>{I(Opcode.LIMM,0,0),Word18.FromLong(12345),H()});p.Run();Assert.AreEqual(12345,p.Registers[0].ToLong());}
        [TestMethod] public void Test_LOAD_STORE(){var p=P();p.LoadProgram(new List<Word18>{I(Opcode.LI,0,100),I(Opcode.LI,1,50),R(Opcode.STORE,0,1),I(Opcode.LI,0,0),R(Opcode.LOAD,0,1),H()});p.Run();Assert.AreEqual(100,p.Registers[0].ToLong());}
        [TestMethod] public void Test_IO_Basic(){var p=P();p.LoadProgram(new List<Word18>{I(Opcode.LI,0,5),I(Opcode.LI,1,42),R(Opcode.OUT,1,0),H()});var d=new MD(42);p.SetOutputDevice(5,d);p.Run();Assert.AreEqual(42,d.V);}
        [TestMethod] public void Test_INI_OUTI(){var p=P();p.LoadProgram(new List<Word18>{I(Opcode.INI,0,7),I(Opcode.OUTI,0,8),H()});var i=new MD(99);var o=new MD(0);p.SetInputDevice(7,i);p.SetOutputDevice(8,o);p.Run();Assert.AreEqual(99,o.V);}
        [TestMethod] public void Test_JNE_JM(){var p=P();p.LoadProgram(new List<Word18>{I(Opcode.LI,6,6),I(Opcode.LI,0,10),I(Opcode.LI,1,20),R(Opcode.CMP,0,1),J(Opcode.JNE,6),I(Opcode.LI,0,-1),I(Opcode.LI,0,1),H()});p.Run();Assert.AreEqual(1,p.Registers[0].ToLong());}
        [TestMethod] public void Test_T3_54_Int128(){var p=new T3InOrderProcessor<Word54>(T3Config.T3_54);p.LoadProgram(new List<Word54>{I54(Opcode.LI,4,100),I54(Opcode.LI,5,2),R54(Opcode.MUL,4,4,5),H54()});p.Run();Assert.AreEqual((Int128)200,p.Registers[4].ToInt128());}
        static Word54 I54(Opcode o,int r,Int128 v)=>Word54.FromInt128(BalancedTernary.ParseToInt128((BT(0,3)+BT((int)o,6)+BT(r,3)+BT((long)v,6)).PadLeft(54,'0')));
        static Word54 R54(Opcode o,int r1,int r2,int r3)=>Word54.FromInt128(BalancedTernary.ParseToInt128((BT(0,3)+BT((int)o,6)+BT(r1,3)+BT(r2,3)+BT(r3,3)).PadLeft(54,'0')));
        static Word54 H54()=>Word54.FromInt128(0);
        class MD:IDevice<Word18>{public long V{get;private set;}long i;public MD(long iv){i=iv;}public Word18 Read()=>Word18.FromLong(i);public void Write(Word18 v)=>V=v.ToLong();public bool DataReady=>true;}
    }
}