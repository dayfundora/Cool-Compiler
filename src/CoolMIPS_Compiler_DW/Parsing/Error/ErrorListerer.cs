using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using System.IO;

namespace CoolMIPS_Compiler_DW.Parsing
{
    public class LexerErrorListener :IAntlrErrorListener<int>
    {
        List<string> _errors;


        public LexerErrorListener(List<string> errors)
        {
            _errors = errors;
        }

        public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            _errors.Add($"({line}, {charPositionInLine + 1}) - Lexer Error: {msg}");
        }
    }

    public class ParserErrorListener : BaseErrorListener
    {
        public List<string> _errors;

        public ParserErrorListener(List<string> errors)
        {
            _errors = errors;
        }

        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            _errors.Add($"({line}, {charPositionInLine + 1}) - Parser Error: {msg}");
        }

    }
}
