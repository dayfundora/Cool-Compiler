using System;
using CoolMIPS_Compiler_DW.Semantic;
using CoolMIPS_Compiler_DW.AST;
using CoolMIPS_Compiler_DW.CodeGenration;
using CoolMIPS_Compiler_DW.Parsing;

namespace CoolMIPS_Compiler_DW
{

    class Program
    {
        static void Main(string[] args)
        {
            //args = new string[1];
            //args[0] = "C:/Users/DAYRENE/source/repos/CoolMIPS_Compiler_DW/Test/hello_world.cl";

            Utils.Welcome();
            Utils.OpenFile(args);

            var ast = Parsing.Parsing.AST(args[0]);
            var scope = new Scope();
            ProgramNode programNode= ast as ProgramNode;
            Semantic.Semantic.Check(programNode, scope);
            GenerateCode.Generate(programNode, Utils.GetOutputPath(args[0]), scope);

            Environment.Exit(0);



        }
    }

}
