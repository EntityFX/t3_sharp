using System;
using System.Collections.Generic;

namespace T3Simulator.Common
{
    public static class OpcodeExtensions
    {
        /// <summary>
        /// Checks if the given opcode writes to its first operand register.
        /// </summary>
        public static bool WritesToRegister(this Opcode opcode)
        {
            return opcode switch
            {
                Opcode.LOAD => true,
                Opcode.MOV => true,
                Opcode.LI => true,
                Opcode.LIMM => true,
                Opcode.ADD => true,
                Opcode.SUB => true,
                Opcode.MUL => true,
                Opcode.DIV => true,
                Opcode.MOD => true,
                Opcode.NEG => true,
                Opcode.POP => true,
                Opcode.IN => true,
                Opcode.INI => true,
                _ => false
            };
        }

        /// <summary>
        /// Checks if the given opcode is a memory access operation.
        /// </summary>
        public static bool IsMemoryOp(this Opcode opcode)
        {
            return opcode switch
            {
                Opcode.LOAD => true,
                Opcode.STORE => true,
                Opcode.LIMM => true,
                Opcode.PUSH => true,
                Opcode.POP => true,
                _ => false
            };
        }

        /// <summary>
        /// Checks if the given opcode is a branch/control flow operation.
        /// </summary>
        public static bool IsBranchOp(this Opcode opcode)
        {
            return opcode switch
            {
                Opcode.JMP => true,
                Opcode.JE => true,
                Opcode.JNE => true,
                Opcode.JL => true,
                Opcode.JG => true,
                Opcode.JM => true,
                Opcode.CALL => true,
                Opcode.RET => true,
                _ => false
            };
        }
    }
}