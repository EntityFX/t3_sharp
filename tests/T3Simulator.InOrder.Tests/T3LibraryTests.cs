using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Assembler;
using T3Simulator.Common;
using T3Simulator.InOrder;
using TritTypes;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;

namespace T3Simulator.InOrder.Tests
{
    [TestClass]
    public class T3LibraryTests
    {
        private (T3InOrderProcessor<Word18> proc, CapturingTsciiOutputDevice<Word18> device) SetupProcessorWithLib(string libPath, string userCode = null)
        {
            var assembler = new T3InOrderAssembler(T3Config.T3_18);
            
            // Load user code first, then library
            string fullSrc = "";
            if (userCode != null) {
                fullSrc += userCode + "\n";
            }
            string absoluteLibPath = @"e:\Projects\t3_sharp\src\T3Assembler\examples\tio.asm";
            if (File.Exists(absoluteLibPath)) {
                fullSrc += File.ReadAllText(absoluteLibPath) + "\n";
            }

            var bin = assembler.Assemble(fullSrc);
            var words = bin.Select(x => Word18.FromInt128(x)).ToList();
            
            var proc = new T3InOrderProcessor<Word18>(T3Config.T3_18);
            proc.LoadProgram(words);
            
            // Attach T-SCII output device to port 0
            var device = new CapturingTsciiOutputDevice<Word18>();
            proc.SetOutputDevice(0, device);
            
            return (proc, device);
        }

        [TestMethod]
        public void Test_PutChar()
        {
            string code = @"
                LI R0, 65
                CALL putchar
                HALT";
            
            var (proc, device) = SetupProcessorWithLib("src/T3Assembler/examples/tio.asm", code);
            proc.Run();
            Assert.AreEqual("A", device.GetCapturedText());
        }

        [TestMethod]
        public void Test_PrintInt_Positive()
        {
            string code = @"
                LI R0, 123
                CALL printint
                HALT";
            
            var (proc, device) = SetupProcessorWithLib("src/T3Assembler/examples/tio.asm", code);
            proc.Run();
            Assert.AreEqual("123", device.GetCapturedText());
        }

        [TestMethod]
        public void Test_PrintInt_Negative()
        {
            string code = @"
                LI R0, -456
                CALL printint
                HALT";
            
            var (proc, device) = SetupProcessorWithLib("src/T3Assembler/examples/tio.asm", code);
            proc.Run();
            Assert.AreEqual("-456", device.GetCapturedText());
        }

        [TestMethod]
        public void Test_PrintFloat_Basic()
        {
            string code = @"
                LI R0, 10
                FMOV F0, R0, 1
                CALL printfloat
                HALT";
            
            var (proc, device) = SetupProcessorWithLib("src/T3Assembler/examples/tio.asm", code);
            proc.Run();
            // Expected output is typically formatted to 6 decimal places in our library
            Assert.AreEqual("10.000000", device.GetCapturedText());
        }

        [TestMethod]
        public void Test_PrintString()
        {
            string code = @"
                .string myStr ""Hello T3!""
                LI R0, myStr
                CALL printstring
                HALT";
            
            var (proc, device) = SetupProcessorWithLib("src/T3Assembler/examples/tio.asm", code);
            proc.Run();
            Assert.AreEqual("Hello T3!", device.GetCapturedText());
        }
    }
}