using System;
using System.Text;
using System.Linq;

namespace T3Simulator.Common
{
    public static class T3StateDumper
    {
        public static string Dump(ProcessorState<long> state)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--- T3 Processor State ---");
            sb.AppendLine($"PC: {state.PC}");
            sb.AppendLine($"SP: {state.SP}");
            sb.AppendLine($"WD: {state.WD}");
            sb.AppendLine($"CD: {state.CD}");
            sb.AppendLine($"Cycles: {state.CycleCount}");
            sb.AppendLine($"Instructions: {state.InstructionCount}");
            sb.AppendLine($"Stalls: {state.StallCount}");
            sb.AppendLine();
            
            sb.AppendLine("Registers:");
            for (int i = 0; i < state.Registers.Length; i++)
            {
                string regName = GetRegName(i);
                sb.AppendLine($"  {regName} (R{i}): {state.Registers[i]}");
            }
            
            sb.AppendLine();
            sb.AppendLine($"PR: {state.PR}");
            sb.AppendLine("-------------------------");
            
            return sb.ToString();
        }

        public static string Dump<TWord>(ProcessorState<TWord> state)
        {
            // Basic implementation for other types, casting to long if possible
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--- T3 Processor State (Generic) ---");
            sb.AppendLine($"PC: {state.PC}");
            sb.AppendLine($"SP: {state.SP}");
            sb.AppendLine($"WD: {state.WD}");
            sb.AppendLine($"CD: {state.CD}");
            sb.AppendLine($"Cycles: {state.CycleCount}");
            sb.AppendLine($"Instructions: {state.InstructionCount}");
            sb.AppendLine($"Stalls: {state.StallCount}");
            sb.AppendLine();
            
            sb.AppendLine("Registers:");
            for (int i = 0; i < state.Registers.Length; i++)
            {
                string regName = GetRegName(i);
                sb.AppendLine($"  {regName} (R{i}): {state.Registers[i]}");
            }
            
            sb.AppendLine();
            sb.AppendLine($"PR: {state.PR}");
            sb.AppendLine("-------------------------");
            
            return sb.ToString();
        }

        private static string GetRegName(int index)
        {
            if (index >= 0 && index < 9)
            {
                return "ABCDEFGHI"[index].ToString();
            }
            return "Unknown";
        }
    }
}