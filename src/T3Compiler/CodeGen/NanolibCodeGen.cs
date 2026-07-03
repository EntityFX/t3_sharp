using System.Text;

namespace T3Compiler.CodeGen
{
    public static class NanolibCodeGen
    {
        public static void EmitStdLib(StringBuilder output)
        {
            Emit(output, "\n; --- StdLib ---");

            // strlen(str) — returns length of null-terminated string (arg in RW)
            Emit(output, "strlen:");
            Emit(output, "    PUSH RZ"); Emit(output, "    PUSH R3"); Emit(output, "    PUSH R4");
            Emit(output, "    MOV R2, 0");
            Emit(output, "strlen_loop:");
            Emit(output, "    LD R1, RW, 0");
            Emit(output, "    MOV R0, 0"); Emit(output, "    CMP R1, R0");
            Emit(output, "    JE strlen_end");
            Emit(output, "    ADD R2, R2, 1");
            Emit(output, "    ADD RW, RW, 1");
            Emit(output, "    LIMM R0, strlen_loop"); Emit(output, "    JMP R0");
            Emit(output, "strlen_end:");
            Emit(output, "    POP R4"); Emit(output, "    POP R3"); Emit(output, "    POP RZ");
            Emit(output, "    RET");

            // putchar(c) — write char to port 0 (arg in RW)
            Emit(output, "putchar:");
            Emit(output, "    PUSH RZ"); Emit(output, "    PUSH R3"); Emit(output, "    PUSH R4");
            Emit(output, "    MOV R0, 0"); Emit(output, "    OUT RW, R0");  // OUT reg, portReg
            Emit(output, "    POP R4"); Emit(output, "    POP R3"); Emit(output, "    POP RZ");
            Emit(output, "    RET");

            // getchar() — read char from port 0 (result in R2)
            Emit(output, "getchar:");
            Emit(output, "    PUSH RZ"); Emit(output, "    PUSH R3"); Emit(output, "    PUSH R4");
            Emit(output, "    MOV R0, 0"); Emit(output, "    IN R2, R0");  // IN dest, portReg
            Emit(output, "    POP R4"); Emit(output, "    POP R3"); Emit(output, "    POP RZ");
            Emit(output, "    RET");

            // print_int(value) — print integer to stdout (arg in RW)
            Emit(output, "print_int:");
            Emit(output, "    PUSH RZ"); Emit(output, "    PUSH R3"); Emit(output, "    PUSH R4");
            Emit(output, "    MOV R0, 0"); Emit(output, "    CMP RW, R0"); Emit(output, "    JGE pi_pos");
            Emit(output, "    MOV R1, 45"); Emit(output, "    MOV R0, 0"); Emit(output, "    OUT R1, R0"); Emit(output, "    NEG RW, RW");
            Emit(output, "pi_pos:");
            Emit(output, "    MOV R2, 10");
            Emit(output, "    MOV R3, 0");
            Emit(output, "pi_loop:");
            Emit(output, "    MOD R4, RW, R2"); Emit(output, "    PUSH R4");
            Emit(output, "    ADD R3, R3, 1");
            Emit(output, "    DIV RW, RW, R2");
            Emit(output, "    MOV R0, 0"); Emit(output, "    CMP RW, R0"); Emit(output, "    JNE pi_loop");
            Emit(output, "pi_emit:");
            Emit(output, "    POP R4"); Emit(output, "    ADD R4, R4, 48");
            Emit(output, "    MOV R0, 0"); Emit(output, "    OUT R4, R0");
            Emit(output, "    SUB R3, R3, 1");
            Emit(output, "    MOV R0, 0"); Emit(output, "    CMP R3, R0"); Emit(output, "    JNE pi_emit");
            Emit(output, "    POP R4"); Emit(output, "    POP R3"); Emit(output, "    POP RZ");
            Emit(output, "    RET");

            // print_str(str) — print null-terminated string (arg in RW)
            Emit(output, "print_str:");
            Emit(output, "    PUSH RZ"); Emit(output, "    PUSH R3"); Emit(output, "    PUSH R4");
            Emit(output, "ps_loop:");
            Emit(output, "    LD R3, RW, 0"); Emit(output, "    MOV R0, 0"); Emit(output, "    CMP R3, R0");
            Emit(output, "    JE ps_end"); Emit(output, "    MOV R0, 0"); Emit(output, "    OUT R3, R0");
            Emit(output, "    ADD RW, RW, 1"); Emit(output, "    LIMM R0, ps_loop"); Emit(output, "    JMP R0");
            Emit(output, "ps_end:");
            Emit(output, "    POP R4"); Emit(output, "    POP R3"); Emit(output, "    POP RZ");
            Emit(output, "    RET");

            // puts(str) — print string with newline (arg in RW)
            Emit(output, "puts:");
            Emit(output, "    PUSH RZ"); Emit(output, "    PUSH R3"); Emit(output, "    PUSH R4");
            Emit(output, "    LIMM R1, print_str"); Emit(output, "    CALL R1");
            Emit(output, "    MOV R1, 10"); Emit(output, "    MOV R0, 0"); Emit(output, "    OUT R1, R0");
            Emit(output, "    POP R4"); Emit(output, "    POP R3"); Emit(output, "    POP RZ");
            Emit(output, "    RET");

            // abs(x) — absolute value (arg in RW)
            Emit(output, "abs:");
            Emit(output, "    PUSH RZ"); Emit(output, "    PUSH R3"); Emit(output, "    PUSH R4");
            Emit(output, "    MOV R0, 0"); Emit(output, "    CMP RW, R0"); Emit(output, "    JGE abs_end");
            Emit(output, "    NEG RW, RW");
            Emit(output, "abs_end:");
            Emit(output, "    MOV R0, 0"); Emit(output, "    MOV R2, RW");
            Emit(output, "    POP R4"); Emit(output, "    POP R3"); Emit(output, "    POP RZ");
            Emit(output, "    RET");

            // min(a,b) — minimum of two values (args in RW, RX)
            Emit(output, "min:");
            Emit(output, "    PUSH RZ"); Emit(output, "    PUSH R3"); Emit(output, "    PUSH R4");
            Emit(output, "    CMP RW, RX"); Emit(output, "    JLE min_rw");
            Emit(output, "    MOV R2, RX"); Emit(output, "    LIMM R0, min_end"); Emit(output, "    JMP R0");
            Emit(output, "min_rw:"); Emit(output, "    MOV R2, RW");
            Emit(output, "min_end:");
            Emit(output, "    POP R4"); Emit(output, "    POP R3"); Emit(output, "    POP RZ");
            Emit(output, "    RET");

            // max(a,b) — maximum of two values (args in RW, RX)
            Emit(output, "max:");
            Emit(output, "    PUSH RZ"); Emit(output, "    PUSH R3"); Emit(output, "    PUSH R4");
            Emit(output, "    CMP RW, RX"); Emit(output, "    JGE max_rw");
            Emit(output, "    MOV R2, RX"); Emit(output, "    LIMM R0, max_end"); Emit(output, "    JMP R0");
            Emit(output, "max_rw:"); Emit(output, "    MOV R2, RW");
            Emit(output, "max_end:");
            Emit(output, "    POP R4"); Emit(output, "    POP R3"); Emit(output, "    POP RZ");
            Emit(output, "    RET");

            // t_strcmp(a,b) — compare two strings (args in RW, RX)
            Emit(output, "t_strcmp:");
            Emit(output, "    PUSH RZ"); Emit(output, "    PUSH R3"); Emit(output, "    PUSH R4");
            Emit(output, "tsc_loop:");
            Emit(output, "    LD R3, RW, 0"); Emit(output, "    LD R4, RX, 0");
            Emit(output, "    CMP R3, R4"); Emit(output, "    JNE tsc_diff");
            Emit(output, "    MOV R0, 0"); Emit(output, "    CMP R3, R0"); Emit(output, "    JE tsc_eq");
            Emit(output, "    ADD RW, RW, 1"); Emit(output, "    ADD RX, RX, 1");
            Emit(output, "    LIMM R0, tsc_loop"); Emit(output, "    JMP R0");
            Emit(output, "tsc_diff:"); Emit(output, "    CMP R3, R4"); Emit(output, "    JL tsc_lt");
            Emit(output, "    MOV R2, 1"); Emit(output, "    LIMM R0, tsc_end"); Emit(output, "    JMP R0");
            Emit(output, "tsc_lt:"); Emit(output, "    MOV R2, -1"); Emit(output, "    LIMM R0, tsc_end"); Emit(output, "    JMP R0");
            Emit(output, "tsc_eq:"); Emit(output, "    MOV R2, 0");
            Emit(output, "tsc_end:");
            Emit(output, "    POP R4"); Emit(output, "    POP R3"); Emit(output, "    POP RZ");
            Emit(output, "    RET");

            // t_strcpy(dest, src) — copy string (dest in RW, src in RX)
            Emit(output, "t_strcpy:");
            Emit(output, "    PUSH RZ"); Emit(output, "    PUSH R3"); Emit(output, "    PUSH R4");
            Emit(output, "tcp_loop:");
            Emit(output, "    LD R3, RX, 0"); Emit(output, "    ST R3, RW, 0");
            Emit(output, "    MOV R0, 0"); Emit(output, "    CMP R3, R0"); Emit(output, "    JE tcp_end");
            Emit(output, "    ADD RW, RW, 1"); Emit(output, "    ADD RX, RX, 1");
            Emit(output, "    LIMM R0, tcp_loop"); Emit(output, "    JMP R0");
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