using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using System.Linq;

namespace CoolMIPS_Compiler_DW.Parsing
{
    public static class Parser
    {
        public static CoolParser.ProgramContext Tree(CommonTokenStream tokens)
        {

            var errors = new List<string>();
            var parser = new CoolParser(tokens);

            parser.RemoveErrorListeners();
            parser.AddErrorListener(new ParserErrorListener(errors));

            var tree = parser.program();


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
            return tree;

        }
    }
}
