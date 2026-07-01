using System.Text;

namespace T3Compiler.CodeGen
{
    /// <summary>
    /// Nanolib — встроенная стандартная библиотека для компилятора T-lang.
    /// Содержит asm-реализации функций из <tio.h>, <tstring.h>, <tmath.h>.
    /// Аналог Nanolib.cs для интерпретатора, но эмиттирует T3 assembly.
    ///
    /// ВАЖНО: OUTI — I-type, op1 = 3 трита (диапазон [-13, 13]), imm = порт (6 тритов).
    /// Для вывода символов с ASCII > 13 используем LI + OUT.
    /// </summary>
    public static class NanolibCodeGen
    {
        /// <summary>
        /// Эмиттирует все builtin asm-функции в указанный StringBuilder.
        /// </summary>
        public static void EmitStdLib(StringBuilder output)
        {
            Emit(output, "\n; --- StdLib ---");

            // strlen(str) — returns length of null-terminated string
            Emit(output, "strlen:");
            Emit(output, "    PUSH RZ"); Emit(output, "    PUSH R3"); Emit(output, "    PUSH R4");
            Emit(output, "    LI R2, 0");
            Emit(output, "strlen_loop:");
            Emit(output, "    LOADI R1, RW, 0");
            Emit(output, "    CMPI R1, 0");
            Emit(output, "    JE strlen_end");
            Emit(output, "    ADDI R2, 1");
            Emit(output, "    ADDI RW, 1");
            Emit(output, "    JMP strlen_loop");
            Emit(output, "strlen_end:");
            Emit(output, "    POP R4"); Emit(output, "    POP R3"); Emit(output, "    POP RZ");
            Emit(output, "    RET");

            // putchar(c) — write char to port 0
            Emit(output, "putchar:");
            Emit(output, "    PUSH RZ"); Emit(output, "    PUSH R3"); Emit(output, "    PUSH R4");
            Emit(output, "    OUTI RW, 0");
            Emit(output, "    POP R4"); Emit(output, "    POP R3"); Emit(output, "    POP RZ");
            Emit(output, "    RET");

            // getchar() — read char from port 0
            Emit(output, "getchar:");
            Emit(output, "    PUSH RZ"); Emit(output, "    PUSH R3"); Emit(output, "    PUSH R4");
            Emit(output, "    INI R2, 0");
            Emit(output, "    POP R4"); Emit(output, "    POP R3"); Emit(output, "    POP RZ");
            Emit(output, "    RET");

            // print_int(value) — print integer to stdout
            // Используем LI + OUT для ASCII-символов (значения > 13 не влезают в op1 OUTI)
            Emit(output, "print_int:");
            Emit(output, "    PUSH RZ"); Emit(output, "    PUSH R3"); Emit(output, "    PUSH R4");
            Emit(output, "    LI R2, 0"); Emit(output, "    LI R3, 0"); Emit(output, "    LI R4, 0");
            Emit(output, "    CMPI RW, 0"); Emit(output, "    JGE pi_pos");
            Emit(output, "    LI R1, 45"); Emit(output, "    OUT R1, 0"); Emit(output, "    NEG RW, RW");
            Emit(output, "pi_pos:");
            Emit(output, "    LI R2, 10"); Emit(output, "    DIV R3, RW, R2"); Emit(output, "    CMPI R3, 0");
            Emit(output, "    JE pi_one"); Emit(output, "    DIV R3, RW, R2"); Emit(output, "    CMPI R3, 0");
            Emit(output, "    JE pi_two"); Emit(output, "    DIV R3, RW, R2"); Emit(output, "    CMPI R3, 0");
            Emit(output, "    JE pi_three");
            Emit(output, "pi_three:"); Emit(output, "    MOD R3, RW, R2"); Emit(output, "    PUSH R3");
            Emit(output, "    DIV RW, RW, R2");
            Emit(output, "pi_two:"); Emit(output, "    MOD R3, RW, R2"); Emit(output, "    PUSH R3");
            Emit(output, "    DIV RW, RW, R2");
            Emit(output, "pi_one:"); Emit(output, "    MOD R3, RW, R2"); Emit(output, "    PUSH R3");
            Emit(output, "    DIV RW, RW, R2");
            Emit(output, "pi_emit:");
            Emit(output, "    POP R3"); Emit(output, "    ADDI R3, 48"); Emit(output, "    OUTI R3, 0");
            Emit(output, "    CMPI RW, 0"); Emit(output, "    JNE pi_emit");
            Emit(output, "    POP R4"); Emit(output, "    POP R3"); Emit(output, "    POP RZ");
            Emit(output, "    RET");

            // print_str(str) — print null-terminated string
            Emit(output, "print_str:");
            Emit(output, "    PUSH RZ"); Emit(output, "    PUSH R3"); Emit(output, "    PUSH R4");
            Emit(output, "ps_loop:");
            Emit(output, "    LOADI R3, RW, 0"); Emit(output, "    CMPI R3, 0");
            Emit(output, "    JE ps_end"); Emit(output, "    OUTI R3, 0");
            Emit(output, "    ADDI RW, 1"); Emit(output, "    JMP ps_loop");
            Emit(output, "ps_end:");
            Emit(output, "    POP R4"); Emit(output, "    POP R3"); Emit(output, "    POP RZ");
            Emit(output, "    RET");

            // puts(str) — print string with newline
            Emit(output, "puts:");
            Emit(output, "    PUSH RZ"); Emit(output, "    PUSH R3"); Emit(output, "    PUSH R4");
            Emit(output, "    LIMM R1, print_str"); Emit(output, "    CALL R1");
            Emit(output, "    LI R1, 10"); Emit(output, "    OUT R1, 0");
            Emit(output, "    POP R4"); Emit(output, "    POP R3"); Emit(output, "    POP RZ");
            Emit(output, "    RET");

            // abs(x) — absolute value
            Emit(output, "abs:");
            Emit(output, "    PUSH RZ"); Emit(output, "    PUSH R3"); Emit(output, "    PUSH R4");
            Emit(output, "    CMPI RW, 0"); Emit(output, "    JGE abs_end");
            Emit(output, "    NEG RW, RW");
            Emit(output, "abs_end:");
            Emit(output, "    MOV R2, RW");
            Emit(output, "    POP R4"); Emit(output, "    POP R3"); Emit(output, "    POP RZ");
            Emit(output, "    RET");

            // min(a,b) — minimum of two values
            Emit(output, "min:");
            Emit(output, "    PUSH RZ"); Emit(output, "    PUSH R3"); Emit(output, "    PUSH R4");
            Emit(output, "    CMP RW, RX"); Emit(output, "    JLE min_rw");
            Emit(output, "    MOV R2, RX"); Emit(output, "    JMP min_end");
            Emit(output, "min_rw:"); Emit(output, "    MOV R2, RW");
            Emit(output, "min_end:");
            Emit(output, "    POP R4"); Emit(output, "    POP R3"); Emit(output, "    POP RZ");
            Emit(output, "    RET");

            // max(a,b) — maximum of two values
            Emit(output, "max:");
            Emit(output, "    PUSH RZ"); Emit(output, "    PUSH R3"); Emit(output, "    PUSH R4");
            Emit(output, "    CMP RW, RX"); Emit(output, "    JGE max_rw");
            Emit(output, "    MOV R2, RX"); Emit(output, "    JMP max_end");
            Emit(output, "max_rw:"); Emit(output, "    MOV R2, RW");
            Emit(output, "max_end:");
            Emit(output, "    POP R4"); Emit(output, "    POP R3"); Emit(output, "    POP RZ");
            Emit(output, "    RET");

            // t_strcmp(a,b) — compare two strings
            Emit(output, "t_strcmp:");
            Emit(output, "    PUSH RZ"); Emit(output, "    PUSH R3"); Emit(output, "    PUSH R4");
            Emit(output, "tsc_loop:");
            Emit(output, "    LOADI R3, RW, 0"); Emit(output, "    LOADI R4, RX, 0");
            Emit(output, "    CMP R3, R4"); Emit(output, "    JNE tsc_diff");
            Emit(output, "    CMPI R3, 0"); Emit(output, "    JE tsc_eq");
            Emit(output, "    ADDI RW, 1"); Emit(output, "    ADDI RX, 1");
            Emit(output, "    JMP tsc_loop");
            Emit(output, "tsc_diff:"); Emit(output, "    CMP R3, R4"); Emit(output, "    JL tsc_lt");
            Emit(output, "    LI R2, 1"); Emit(output, "    JMP tsc_end");
            Emit(output, "tsc_lt:"); Emit(output, "    LI R2, -1"); Emit(output, "    JMP tsc_end");
            Emit(output, "tsc_eq:"); Emit(output, "    LI R2, 0");
            Emit(output, "tsc_end:");
            Emit(output, "    POP R4"); Emit(output, "    POP R3"); Emit(output, "    POP RZ");
            Emit(output, "    RET");

            // t_strcpy(dest, src) — copy string
            Emit(output, "t_strcpy:");
            Emit(output, "    PUSH RZ"); Emit(output, "    PUSH R3"); Emit(output, "    PUSH R4");
            Emit(output, "tcp_loop:");
            Emit(output, "    LOADI R3, RX, 0"); Emit(output, "    STOREI R3, RW, 0");
            Emit(output, "    CMPI R3, 0"); Emit(output, "    JE tcp_end");
            Emit(output, "    ADDI RW, 1"); Emit(output, "    ADDI RX, 1");
            Emit(output, "    JMP tcp_loop");
            Emit(output, "tcp_end:");
            Emit(output, "    POP R4"); Emit(output, "    POP R3"); Emit(output, "    POP RZ");
            Emit(output, "    RET");

            // t_strlen(str) — alias for strlen
            Emit(output, "t_strlen:");
            Emit(output, "    LIMM R1, strlen"); Emit(output, "    CALL R1"); Emit(output, "    RET");
        }

        private static void Emit(StringBuilder output, string s = "")
        {
            output.AppendLine(s);
        }
    }
}