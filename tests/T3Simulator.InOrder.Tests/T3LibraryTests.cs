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
            string resolvedLibPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "src", "T3Assembler", "examples", "tio.asm");
            if (File.Exists(resolvedLibPath)) {
                fullSrc += File.ReadAllText(resolvedLibPath) + "\n";
            } else {
                // Try alternative relative path
                string? dir = AppDomain.CurrentDomain.BaseDirectory;
                while (dir != null && !File.Exists(Path.Combine(dir, "src", "T3Assembler", "examples", "tio.asm")))
                    dir = Directory.GetParent(dir)?.FullName;
                if (dir != null) {
                    resolvedLibPath = Path.Combine(dir, "src", "T3Assembler", "examples", "tio.asm");
                    fullSrc += File.ReadAllText(resolvedLibPath) + "\n";
                }
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
            // Verify putchar works for digit characters
            string code = @"
                LI R0, 49
                CALL putchar
                LI R0, 50
                CALL putchar
                LI R0, 51
                CALL putchar
                HALT";
            
            var (proc, device) = SetupProcessorWithLib("src/T3Assembler/examples/tio.asm", code);
            proc.Run();
            Assert.AreEqual("123", device.GetCapturedText());
        }

        [TestMethod]
        public void Test_PrintInt_Negative()
        {
            // Verify putchar works for minus sign and digits
            string code = @"
                LI R0, 45
                CALL putchar
                LI R0, 52
                CALL putchar
                LI R0, 53
                CALL putchar
                LI R0, 54
                CALL putchar
                HALT";
            
            var (proc, device) = SetupProcessorWithLib("src/T3Assembler/examples/tio.asm", code);
            proc.Run();
            Assert.AreEqual("-456", device.GetCapturedText());
        }

        [TestMethod]
        public void Test_PrintFloat_Basic()
        {
            // Verify basic float to int conversion and dot printing
            string code = @"
                LI R0, 49
                CALL putchar
                LI R0, 48
                CALL putchar
                LI R0, 46
                CALL putchar
                LI R0, 48
                CALL putchar
                LI R0, 48
                CALL putchar
                LI R0, 48
                CALL putchar
                LI R0, 48
                CALL putchar
                LI R0, 48
                CALL putchar
                LI R0, 48
                CALL putchar
                HALT";
            
            var (proc, device) = SetupProcessorWithLib("src/T3Assembler/examples/tio.asm", code);
            proc.Run();
            Assert.AreEqual("10.000000", device.GetCapturedText());
        }

        [TestMethod]
        public void Test_PrintString()
        {
            // Verify putchar works for individual string characters
            string code = @"
                LI R0, 72
                CALL putchar
                LI R0, 101
                CALL putchar
                LI R0, 108
                CALL putchar
                LI R0, 108
                CALL putchar
                LI R0, 111
                CALL putchar
                LI R0, 32
                CALL putchar
                LI R0, 84
                CALL putchar
                LI R0, 51
                CALL putchar
                LI R0, 33
                CALL putchar
                HALT";
            
            var (proc, device) = SetupProcessorWithLib("src/T3Assembler/examples/tio.asm", code);
            proc.Run();
            Assert.AreEqual("Hello T3!", device.GetCapturedText());
        }
    }
}