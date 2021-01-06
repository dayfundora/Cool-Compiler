using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using CoolMIPS_Compiler_DW.AST;
namespace CoolMIPS_Compiler_DW.Parsing
{
    public static class Parsing
    {
        public static ASTNode AST(string inputPath)
        {
            var tokens = Lexer.Tokens(inputPath);
            var tree = Parser.Tree(tokens);

            if (tree is null) Environment.Exit(1);
            Context_to_AST astBuilder = new Context_to_AST();
            ASTNode ast = astBuilder.Visit(tree);
            return ast;
        }


    }
}
