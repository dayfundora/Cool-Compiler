using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System.IO;
using CoolMIPS_Compiler_DW.AST;
using CoolMIPS_Compiler_DW.CodeGenration.CIL;
using CoolMIPS_Compiler_DW.CodeGenration.Mips;
using CoolMIPS_Compiler_DW.Semantic;

namespace CoolMIPS_Compiler_DW.CodeGenration
{
    public class GenerateCode
    {
        public static void Generate(ProgramNode root, string outputPath, Scope scope)
        {

            List<CodeLine> g = (new CodeGeneratorVisitor()).GetIntermediateCode(root, scope);
            /*Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("InterneduateCode OK!!!");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine();
            Console.WriteLine("CODE");
            Console.WriteLine("------------------------------------------");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;*/
            string code = (new MIPSCodeGenerator()).GenerateCode(g);
            //Console.WriteLine(code);

            File.WriteAllText(outputPath, code);
        }
    }
}
