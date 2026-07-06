using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Simulator.Common;
using T3Simulator.InOrder;
using System.Collections.Generic;
using TritTypes;

namespace T3Simulator.InOrder.Tests
{
    [TestClass]
    public class RegGroupMatrixTests
    {
        // Helpers to create instructions with specific RegGroups
        static Word18 EncI(int rg, Opcode o, int r, long imm) => 
            Word18.FromLong(InstructionEncoder.EncodeI(0, rg, 1, (int)o, r, imm));
        
        static Word18 EncR(int rg, Opcode o, int r1, int r2, int r3) => 
            Word18.FromLong(InstructionEncoder.EncodeR(0, rg, 0, (int)o, r1, r2, r3));

        static void LoadAndRun(T3InOrderProcessor<Word18> p, params Word18[] program)
        {
            p.LoadProgram(new List<Word18>(program));
            p.Run();
        }

        // Register index constants (mapping -4..4 to 0..8)
        const int RW = -4; 
        const int RX = -3;
        const int RY = -2;
        const int RZ = -1;
        const int R0 = 0;
        const int R1 = 1;
        const int R2 = 2;
        const int R3 = 3;
        const int R4 = 4;

        [TestMethod]
        public void Test_GP_Group_Isolation()
        {
            var p = new T3InOrderProcessor<Word18>(T3Config.T3_18);
            // Set GP RW to 100
            LoadAndRun(p, EncI(0, Opcode.MOV, RW, 100), new Word18(0));
            Assert.AreEqual(100, p.Registers[0].ToLong());
        }

        [TestMethod]
        public void Test_FPU_Group_Isolation()
        {
            var p = new T3InOrderProcessor<Word18>(T3Config.T3_18);
            // Set FPU FW to 200 (indices are shared, so RW index in FPU is FW)
            LoadAndRun(p, EncI(-1, Opcode.MOV, RW, 200), new Word18(0));
            
            // GP RW should remain 0, while FPU FW should be 200
            Assert.AreEqual(0, p.Registers[0].ToLong());
            Assert.AreEqual(200, p.FRegisters[0].ToDouble());
        }

        [TestMethod]
        public void Test_Special_Group_Isolation()
        {
            var p = new T3InOrderProcessor<Word18>(T3Config.T3_18);
            // Set Special FP to 300 (index of RW is FP in Special group)
            LoadAndRun(p, EncI(1, Opcode.MOV, RW, 300), new Word18(0));
            
            Assert.AreEqual(0, p.Registers[0].ToLong());
            Assert.AreEqual(0, p.FRegisters[0].ToDouble());
            Assert.AreEqual(300, p.FP);
        }

        [TestMethod]
        public void Test_RegGroup_Cross_Access_Danger()
        {
            var p = new T3InOrderProcessor<Word18>(T3Config.T3_18);
            
            // Scenario: We intend to use GP RX (index -3), but we accidentally set RegGroup to Special (1)
            // Instruction: MOV R1, 100 | RegGroup = 1
            // This should write 100 to Special register index -3 (HP), NOT GP RX.
            LoadAndRun(p, EncI(1, Opcode.MOV, RX, 100), new Word18(0));
            
            Assert.AreEqual(0, p.Registers[1].ToLong(), "GP RX should not have been modified");
            Assert.AreEqual(100, p.HP, "Special HP should have been modified");
        }

        [TestMethod]
        public void Test_RegGroup_Matrix_Full_Cycle()
        {
            var p = new T3InOrderProcessor<Word18>(T3Config.T3_18);
            
            // 1. Write to GP RW
            // 2. Write to FPU FW
            // 3. Write to Special FP
            LoadAndRun(p, 
                EncI(0, Opcode.MOV, RW, 10), 
                EncI(-1, Opcode.MOV, RW, 20), 
                EncI(1, Opcode.MOV, RW, 30), 
                new Word18(0)
            );
            
            Assert.AreEqual(10, p.Registers[0].ToLong());
            Assert.AreEqual(20, p.FRegisters[0].ToDouble());
            Assert.AreEqual(30, p.FP);
        }
    }
}