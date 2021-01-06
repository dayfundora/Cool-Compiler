using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;


namespace CoolMIPS_Compiler_DW
{
    public static class Utils
    {
        public static void Welcome()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Cool Language Compiler Version 1.0");
            Console.WriteLine("Copyright (c) 2019 by Dayrene Fundora Gonzalez and Wendy Diaz Ramirez");
            Console.ForegroundColor = ConsoleColor.Gray;

        }
        public static void OpenFile(string[] args)
        {
            if (args.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: No arguments given ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Environment.Exit(1);
                return;
            }
            Console.WriteLine("Program: {0,40}\t", args[0]);
            AntlrFileStream input_file;
            try
            {
                input_file = new AntlrFileStream(args[0]);
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: Opening file ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Environment.Exit(1);
                return;
            }

        }
        public static string GetOutputPath(string path)
        {
            string resp = path.Replace(".cl", ".s");

            return resp;

        }

    }
}
