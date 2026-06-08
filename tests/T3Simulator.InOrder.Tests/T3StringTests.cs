using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using TritTypes;
using T3Simulator.Common;
using T3Simulator.InOrder;
using T3Assembler;

namespace T3Simulator.InOrder.Tests
{
    /// <summary>
    /// Mock device to capture output for verification.
    /// </summary>
    public class MockOutputDevice<TWord> : IDevice<TWord> where TWord : IT3Word<TWord>
    {
        public List<TWord> WrittenValues = new List<TWord>();
        public bool DataReady => true;
        public TWord Read() => throw new NotSupportedException();
        public void Write(TWord value) => WrittenValues.Add(value);
    }

    [TestClass]
    public class T3StringTests
    {
        [TestMethod]
        public void StringOutput_IntegrationTest()
        {
            // 1. Setup Assembler and Program
            var config = T3Config.T3_18;
            var assembler = new T3InOrderAssembler(config);
            
            // Program that prints a string from memory
            // We place the code at the start and the string at the end to avoid executing data
            string asm = @"
                start:
                    LI A, msg
                    LI C, 1
                loop:
                    LOAD B, A
                    CMP B, 0
                    JE end
                    OUT 0, B
                    ADD A, C
                    JMP loop
                end:
                    OUT 0, B
                    HALT
                msg: ""Hello T3!""
            ";

            List<Int128> binaryInt128 = assembler.Assemble(asm);
            List<Word18> binary = binaryInt128.Select(v => Word18.FromInt128(v)).ToList();

            // 2. Setup Processor
            var processor = new T3InOrderProcessor<Word18>(config);
            var mockDevice = new MockOutputDevice<Word18>();
            processor.SetOutputDevice(0, mockDevice);
            
            processor.LoadProgram(binary);
            
            // 3. Run
            processor.Run();

            // 4. Verify
            // Expected values are the T-SCII values of "Hello T3!"
            // We exclude the null terminator since the loop stops when B == 0
            // But actually, the loop checks B == 0 AFTER the OUT.
            // So it will print the null terminator too.
            
            string expectedText = "Hello T3!";
            List<Int128> expectedValues = new List<Int128>();
            foreach (char c in expectedText)
            {
                expectedValues.Add(TScii.FromChar(c));
            }
            expectedValues.Add(0); // Null terminator

            Assert.AreEqual(expectedValues.Count, mockDevice.WrittenValues.Count, "Output length mismatch");
            
            for (int i = 0; i < expectedValues.Count; i++)
            {
                Assert.AreEqual((long)expectedValues[i], (long)mockDevice.WrittenValues[i].ToInt128(), $"Character at index {i} mismatch");
            }
        }
    }
}