using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using System.Linq;

namespace CoolMIPS_Compiler_DW.Parsing
{
   public static class Lexer
    {
        public static CommonTokenStream Tokens(string inputPath)
        {
            var code = new AntlrFileStream(inputPath);
            var lexer = new CoolLexer(code);

            var errors = new List<string>();
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(new LexerErrorListener(errors));

            if (errors.Any())
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var item in errors)
                    Console.WriteLine(item);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
                return null;
            }
            var tokens = new CommonTokenStream(lexer);
            return tokens;






        }
    }
}
