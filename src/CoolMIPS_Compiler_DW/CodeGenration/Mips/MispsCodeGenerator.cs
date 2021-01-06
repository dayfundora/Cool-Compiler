using System;
using System.Collections.Generic;
using System.Text;
using CoolMIPS_Compiler_DW.Interfaces;
using CoolMIPS_Compiler_DW.CodeGenration.CIL;



namespace CoolMIPS_Compiler_DW.CodeGenration.Mips
{
    class MIPSCodeGenerator : ICodeVisitor
    {
        List<string> MIPSCode;
        List<string> ProgramData;
        string current_function;
        int size;
        int param_count;
        CILPreprocessor Preprocessor;

        public string GenerateCode(List<CodeLine> lines)
        {
            MIPSCode = new List<string>();
            ProgramData = new List<string>();
            param_count = 0;
            Preprocessor = new CILPreprocessor(lines);

            foreach (var str in Preprocessor.StringsCounter)
                ProgramData.Add($"str{str.Value}: .asciiz \"{str.Key}\"");

            foreach (var x in Preprocessor.Inherit)
            {
                string s = $"_class.{ x.Key}: .word str{Preprocessor.StringsCounter[x.Key]}, ";

                string p = x.Value;
                while (p != "Object")
                {
                    s += $"str{Preprocessor.StringsCounter[p]}, ";
                    p = Preprocessor.Inherit[p];
                }

                s += $"str{Preprocessor.StringsCounter["Object"]}, 0";
                ProgramData.Add(s);
            }

            for (int i = 0; i < lines.Count; ++i)
            {
                MIPSCode.Add($"# {lines[i]}");
                lines[i].Accept(this);
            }

            List<string> t = new List<string>();
            foreach (var s in MIPSCode)
            {
                t.Add(s);
            }

            MIPSCode = t;

            string gen = "";

            gen += ".data\n";
            gen += "buffer: .space 65536\n";
            gen += "strsubstrexception: .asciiz \"Substring index exception\n\"\n";


            foreach (string s in ProgramData)
                gen += s + "\n";

            gen += "\n.globl main\n";
            gen += ".text\n";

            gen += "_inherit:\n";
            gen += "lw $a0, 8($a0)\n";
            gen += "_inherit.loop:\n";
            gen += "lw $a2, 0($a0)\n";
            gen += "beq $a1, $a2, _inherit_true\n";
            gen += "beq $a2, $zero, _inherit_false\n";
            gen += "addiu $a0, $a0, 4\n";
            gen += "j _inherit.loop\n";
            gen += "_inherit_false:\n";
            gen += "li $v0, 0\n";
            gen += "jr $ra\n";
            gen += "_inherit_true:\n";
            gen += "li $v0, 1\n";
            gen += "jr $ra\n";
            gen += "\n";

            gen += "_copy:\n";
            gen += "lw $a1, 0($sp)\n";
            gen += "lw $a0, -4($sp)\n";
            gen += "li $v0, 9\n";
            gen += "syscall\n";
            gen += "lw $a1, 0($sp)\n";
            gen += "lw $a0, 4($a1)\n";
            gen += "move $a3, $v0\n";
            gen += "_copy.loop:\n";
            gen += "lw $a2, 0($a1)\n";
            gen += "sw $a2, 0($a3)\n";
            gen += "addiu $a0, $a0, -1\n";
            gen += "addiu $a1, $a1, 4\n";
            gen += "addiu $a3, $a3, 4\n";
            gen += "beq $a0, $zero, _copy.end\n";
            gen += "j _copy.loop\n";
            gen += "_copy.end:\n";
            gen += "jr $ra\n";
            gen += "\n";

            gen += "_abort:\n";
            gen += "li $v0, 10\n";
            gen += "syscall\n";
            gen += "\n";

            gen += "_out_string:\n";
            gen += "li $v0, 4\n";
            gen += "lw $a0, 0($sp)\n";
            gen += "syscall\n";
            gen += "jr $ra\n";
            gen += "\n";

            gen += "_out_int:\n";
            gen += "li $v0, 1\n";
            gen += "lw $a0, 0($sp)\n";
            gen += "syscall\n";
            gen += "jr $ra\n";
            gen += "\n";

            gen += "_in_string:\n";
            gen += "move $a3, $ra\n";
            gen += "la $a0, buffer\n";
            gen += "li $a1, 65536\n";
            gen += "li $v0, 8\n";
            gen += "syscall\n";
            gen += "addiu $sp, $sp, -4\n";
            gen += "sw $a0, 0($sp)\n";
            gen += "jal String.length\n";
            gen += "addiu $sp, $sp, 4\n";
            gen += "move $a2, $v0\n";
            gen += "addiu $a2, $a2, -1\n";
            gen += "move $a0, $v0\n";
            gen += "li $v0, 9\n";
            gen += "syscall\n";
            gen += "move $v1, $v0\n";
            gen += "la $a0, buffer\n";
            gen += "_in_string.loop:\n";
            gen += "beqz $a2, _in_string.end\n";
            gen += "lb $a1, 0($a0)\n";
            gen += "sb $a1, 0($v1)\n";
            gen += "addiu $a0, $a0, 1\n";
            gen += "addiu $v1, $v1, 1\n";
            gen += "addiu $a2, $a2, -1\n";
            gen += "j _in_string.loop\n";
            gen += "_in_string.end:\n";
            gen += "sb $zero, 0($v1)\n";
            gen += "move $ra, $a3\n";
            gen += "jr $ra\n";
            gen += "\n";

            gen += "_in_int:\n";
            gen += "li $v0, 5\n";
            gen += "syscall\n";
            gen += "jr $ra\n";
            gen += "\n";

            gen += "_stringlength:\n";
            gen += "lw $a0, 0($sp)\n";
            gen += "_stringlength.loop:\n";
            gen += "lb $a1, 0($a0)\n";
            gen += "beqz $a1, _stringlength.end\n";
            gen += "addiu $a0, $a0, 1\n";
            gen += "j _stringlength.loop\n";
            gen += "_stringlength.end:\n";
            gen += "lw $a1, 0($sp)\n";
            gen += "subu $v0, $a0, $a1\n";
            gen += "jr $ra\n";
            gen += "\n";

            gen += "_stringconcat:\n";
            gen += "move $a2, $ra\n";
            gen += "jal _stringlength\n";
            gen += "move $v1, $v0\n";
            gen += "addiu $sp, $sp, -4\n";
            gen += "jal _stringlength\n";
            gen += "addiu $sp, $sp, 4\n";
            gen += "add $v1, $v1, $v0\n";
            gen += "addi $v1, $v1, 1\n";
            gen += "li $v0, 9\n";
            gen += "move $a0, $v1\n";
            gen += "syscall\n";
            gen += "move $v1, $v0\n";
            gen += "lw $a0, 0($sp)\n";
            gen += "_stringconcat.loop1:\n";
            gen += "lb $a1, 0($a0)\n";
            gen += "beqz $a1, _stringconcat.end1\n";
            gen += "sb $a1, 0($v1)\n";
            gen += "addiu $a0, $a0, 1\n";
            gen += "addiu $v1, $v1, 1\n";
            gen += "j _stringconcat.loop1\n";
            gen += "_stringconcat.end1:\n";
            gen += "lw $a0, -4($sp)\n";
            gen += "_stringconcat.loop2:\n";
            gen += "lb $a1, 0($a0)\n";
            gen += "beqz $a1, _stringconcat.end2\n";
            gen += "sb $a1, 0($v1)\n";
            gen += "addiu $a0, $a0, 1\n";
            gen += "addiu $v1, $v1, 1\n";
            gen += "j _stringconcat.loop2\n";
            gen += "_stringconcat.end2:\n";
            gen += "sb $zero, 0($v1)\n";
            gen += "move $ra, $a2\n";
            gen += "jr $ra\n";
            gen += "\n";

            gen += "_stringsubstr:\n";
            gen += "lw $a0, -8($sp)\n";
            gen += "addiu $a0, $a0, 1\n";
            gen += "li $v0, 9\n";
            gen += "syscall\n";
            gen += "move $v1, $v0\n";
            gen += "lw $a0, 0($sp)\n";
            gen += "lw $a1, -4($sp)\n";
            gen += "add $a0, $a0, $a1\n";
            gen += "lw $a2, -8($sp)\n";
            gen += "_stringsubstr.loop:\n";
            gen += "beqz $a2, _stringsubstr.end\n";
            gen += "lb $a1, 0($a0)\n";
            gen += "beqz $a1, _substrexception\n";
            gen += "sb $a1, 0($v1)\n";
            gen += "addiu $a0, $a0, 1\n";
            gen += "addiu $v1, $v1, 1\n";
            gen += "addiu $a2, $a2, -1\n";
            gen += "j _stringsubstr.loop\n";
            gen += "_stringsubstr.end:\n";
            gen += "sb $zero, 0($v1)\n";
            gen += "jr $ra\n";
            gen += "\n";

            gen += "_substrexception:\n";
            gen += "la $a0, strsubstrexception\n";
            gen += "li $v0, 4\n";
            gen += "syscall\n";
            gen += "li $v0, 10\n";
            gen += "syscall\n";
            gen += "\n";

            gen += "_stringcmp:\n";
            gen += "li $v0, 1\n";
            gen += "_stringcmp.loop:\n";
            gen += "lb $a2, 0($a0)\n";
            gen += "lb $a3, 0($a1)\n";
            gen += "beqz $a2, _stringcmp.end\n";
            gen += "beq $a2, $zero, _stringcmp.end\n";
            gen += "beq $a3, $zero, _stringcmp.end\n";
            gen += "bne $a2, $a3, _stringcmp.differents\n";
            gen += "addiu $a0, $a0, 1\n";
            gen += "addiu $a1, $a1, 1\n";
            gen += "j _stringcmp.loop\n";
            gen += "_stringcmp.end:\n";
            gen += "beq $a2, $a3, _stringcmp.equals\n";
            gen += "_stringcmp.differents:\n";
            gen += "li $v0, 0\n";
            gen += "jr $ra\n";
            gen += "_stringcmp.equals:\n";
            gen += "li $v0, 1\n";
            gen += "jr $ra\n";
            gen += "\n";

            gen += "\nmain:\n";

            foreach (string s in MIPSCode)
                gen += s + "\n";

            gen += "li $v0, 10\n";
            gen += "syscall\n";

            return gen;
        }

        public void Visit(LabelCodeLine line)
        {
            if (line.Head[0] != '_')
            {
                current_function = line.Label;
                size = Preprocessor.FunctionVarsSize[current_function];
            }
            MIPSCode.Add($"\n");
            MIPSCode.Add($"{line.Label}:");
            MIPSCode.Add($"li $t9, 0");
        }

        public void Visit(AllocateCodeLine line)
        {
            MIPSCode.Add($"# Begin Allocate");
            MIPSCode.Add($"li $v0, 9");
            MIPSCode.Add($"li $a0, {4 * line.Size}");
            MIPSCode.Add($"syscall");
            MIPSCode.Add($"sw $v0, {-4 * line.Variable}($sp)");
            MIPSCode.Add($"# End Allocate");
        }

        public void Visit(GotoJumpCodeLine line)
        {
            MIPSCode.Add($"j {line.Label.Label}");
        }

        public void Visit(CommentCodeLine line)
        {
            return;
        }

        public void Visit(AssignmentVariableToMemoryCodeLine line)
        {
            MIPSCode.Add($"lw $a0, {-line.Right * 4}($sp)");
            MIPSCode.Add($"lw $a1, {-line.Left * 4}($sp)");
            MIPSCode.Add($"sw $a0, {line.Offset * 4}($a1)");
        }

        public void Visit(AssignmentVariableToVariableCodeLine line)
        {
            MIPSCode.Add($"lw $a0, {-line.Right * 4}($sp)");
            MIPSCode.Add($"sw $a0, {-line.Left * 4}($sp)");
        }

        public void Visit(AssignmentConstantToMemoryCodeLine line)
        {
            MIPSCode.Add($"lw $a0, {-line.Left * 4}($sp)");
            MIPSCode.Add($"li $a1, {line.Right}");
            MIPSCode.Add($"sw $a1, {line.Offset * 4}($a0)");
        }

        public void Visit(AssignmentMemoryToVariableCodeLine line)
        {
            MIPSCode.Add($"lw $a0, {-line.Right * 4}($sp)");
            MIPSCode.Add($"lw $a1, {line.Offset * 4}($a0)");
            MIPSCode.Add($"sw $a1, {-line.Left * 4}($sp)");
        }

        public void Visit(AssignmentConstantToVariableCodeLine line)
        {
            MIPSCode.Add($"li $a0, {line.Right}");
            MIPSCode.Add($"sw $a0, {-line.Left * 4}($sp)");
        }

        public void Visit(AssignmentStringToVariableCodeLine line)
        {
            MIPSCode.Add($"la $a0, str{Preprocessor.StringsCounter[line.Right]}");
            MIPSCode.Add($"sw $a0, {-line.Left * 4}($sp)");
        }

        public void Visit(AssignmentStringToMemoryCodeLine line)
        {
            MIPSCode.Add($"la $a0, str{Preprocessor.StringsCounter[line.Right]}");
            MIPSCode.Add($"lw $a1, {-line.Left * 4}($sp)");
            MIPSCode.Add($"sw $a0, {line.Offset * 4}($a1)");
        }


        public void Visit(AssignmentLabelToVariableCodeLine line)
        {
            MIPSCode.Add($"la $a0, {line.Right.Label}");
            MIPSCode.Add($"sw $a0, {-line.Left * 4}($sp)");
        }

        public void Visit(AssignmentLabelToMemoryCodeLine line)
        {
            MIPSCode.Add($"la $a0, {line.Right.Label}");
            MIPSCode.Add($"lw $a1, {-line.Left * 4}($sp)");
            MIPSCode.Add($"sw $a0, {line.Offset * 4}($a1)");
        }
        public void Visit(AssignmentNullToVariableCodeLine line)
        {
            MIPSCode.Add($"sw $zero, {-line.Variable * 4}($sp)");
        }

        public void Visit(ReturnCodeLine line)
        {
            MIPSCode.Add($"lw $v0, {-line.Variable * 4}($sp)");
            MIPSCode.Add($"jr $ra");
        }

        public void Visit(ParamCodeLine line)
        {
            return;
        }

        public void Visit(PopParamCodeLine line)
        {
            param_count = 0;
        }

        public void Visit(ConditionalJumpCodeLine line)
        {
            MIPSCode.Add($"lw $a0, {-line.ConditionalVar * 4}($sp)");
            MIPSCode.Add($"beqz $a0, {line.Label.Label}");
        }

        public void Visit(PushParamCodeLine line)
        {
            ++param_count;
            MIPSCode.Add($"lw $a0, {-line.Variable * 4}($sp)");
            MIPSCode.Add($"sw $a0, {-(size + param_count) * 4}($sp)");
        }

        public void Visit(CallLabelCodeLine line)
        {
            MIPSCode.Add($"sw $ra, {-size * 4}($sp)");
            MIPSCode.Add($"addiu $sp, $sp, {-(size + 1) * 4}");
            MIPSCode.Add($"jal {line.Method.Label}");
            MIPSCode.Add($"addiu $sp, $sp, {(size + 1) * 4}");
            MIPSCode.Add($"lw $ra, {-size * 4}($sp)");
            if (line.Result != -1)
                MIPSCode.Add($"sw $v0, {-line.Result * 4}($sp)");
        }

        public void Visit(CallAddressCodeLine line)
        {
            MIPSCode.Add($"sw $ra, {-size * 4}($sp)");
            MIPSCode.Add($"lw $a0, {-line.Address * 4}($sp)");
            MIPSCode.Add($"addiu $sp, $sp, {-(size + 1) * 4}");
            MIPSCode.Add($"jalr $ra, $a0");
            MIPSCode.Add($"addiu $sp, $sp, {(size + 1) * 4}");
            MIPSCode.Add($"lw $ra, {-size * 4}($sp)");
            if (line.Result != -1)
                MIPSCode.Add($"sw $v0, {-line.Result * 4}($sp)");
        }


        public void Visit(BinaryOperationCodeLine line)
        {
            MIPSCode.Add($"lw $a0, {-line.LeftOperandVariable * 4}($sp)");
            MIPSCode.Add($"lw $a1, {-line.RightOperandVariable * 4}($sp)");

            switch (line.Symbol)
            {
                case "+":
                    MIPSCode.Add($"add $a0, $a0, $a1");
                    break;
                case "-":
                    MIPSCode.Add($"sub $a0, $a0, $a1");
                    break;
                case "*":
                    MIPSCode.Add($"mult $a0, $a1");
                    MIPSCode.Add($"mflo $a0");
                    break;
                case "/":
                    MIPSCode.Add($"div $a0, $a1");
                    MIPSCode.Add($"mflo $a0");
                    break;
                case "<":
                    MIPSCode.Add($"sge $a0, $a0, $a1");
                    MIPSCode.Add($"li $a1, 1");
                    MIPSCode.Add($"sub $a0, $a1, $a0");
                    break;
                case "<=":
                    MIPSCode.Add($"sle $a0, $a0, $a1");
                    break;
                case "=":
                    MIPSCode.Add($"seq $a0, $a0, $a1");
                    break;
                case "=:=":
                    MIPSCode.Add($"move $v1, $ra");
                    MIPSCode.Add($"jal _stringcmp");
                    MIPSCode.Add($"move $ra, $v1");
                    MIPSCode.Add($"move $a0, $v0");
                    break;
                case "inherit":
                    MIPSCode.Add($"move $v1, $ra");
                    MIPSCode.Add($"jal _inherit");
                    MIPSCode.Add($"move $ra, $v1");
                    MIPSCode.Add($"move $a0, $v0");
                    break;
                default:
                    throw new NotImplementedException();
            }

            MIPSCode.Add($"sw $a0, {-line.AssignVariable * 4}($sp)");
        }

        public void Visit(UnaryOperationCodeLine line)
        {
            MIPSCode.Add($"lw $a0, {-line.OperandVariable * 4}($sp)");

            switch (line.Symbol)
            {
                case "not":
                    MIPSCode.Add($"li $a1, 1");
                    MIPSCode.Add($"sub $a0, $a1, $a0");
                    break;
                case "isvoid":
                    MIPSCode.Add($"seq $a0, $a0, $zero");
                    break;
                case "~":
                    MIPSCode.Add($"not $a0, $a0");
                    break;
                default:
                    throw new NotImplementedException();
                    
            }

            MIPSCode.Add($"sw $a0, {-line.AssignVariable * 4}($sp)");
        }

        public void Visit(InheritCodeLine line)
        {
        }

    }
}
