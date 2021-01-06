using System;
using System.Collections.Generic;
using System.Text;
using CoolMIPS_Compiler_DW.Interfaces;
using CoolMIPS_Compiler_DW.CodeGenration.CIL;

namespace CoolMIPS_Compiler_DW.CodeGenration.Mips
{
    public class CILPreprocessor : ICodeVisitor
    {
        public Dictionary<string, int> FunctionVarsSize;
        public Dictionary<string, (int, int)> FunctionLimits;
        public Dictionary<string, int> FunctionParamsCount;
        public Dictionary<string, int> StringsCounter;
        public Dictionary<string, string> Inherit;

        int current_line;
        string current_function;
        int string_counter;

        public CILPreprocessor(List<CodeLine> lines)
        {
            FunctionVarsSize = new Dictionary<string, int>();
            FunctionLimits = new Dictionary<string, (int, int)>();
            FunctionParamsCount = new Dictionary<string, int>();
            StringsCounter = new Dictionary<string, int>();
            Inherit = new Dictionary<string, string>();
            string_counter = 0;

            for (current_line = 0; current_line < lines.Count; ++current_line)
            {
                lines[current_line].Accept(this);
            }
        }

        public void Visit(LabelCodeLine line)
        {
            if (line.Head[0] != '_')
            {
                current_function = line.Label;
                FunctionVarsSize[current_function] = 0;
                FunctionLimits[current_function] = (current_line, -1);
                FunctionParamsCount[current_function] = 0;
            }
        }

        public void Visit(AllocateCodeLine line)
        {
            FunctionVarsSize[current_function] = Math.Max(FunctionVarsSize[current_function], line.Variable + 1);
        }

        public void Visit(AssignmentVariableToVariableCodeLine line)
        {
            FunctionVarsSize[current_function] = Math.Max(FunctionVarsSize[current_function], line.Left + 1);
        }

        public void Visit(AssignmentMemoryToVariableCodeLine line)
        {
            FunctionVarsSize[current_function] = Math.Max(FunctionVarsSize[current_function], line.Left + 1);
        }

        public void Visit(AssignmentConstantToVariableCodeLine line)
        {
            FunctionVarsSize[current_function] = Math.Max(FunctionVarsSize[current_function], line.Left + 1);
        }

        public void Visit(AssignmentStringToVariableCodeLine line)
        {
            FunctionVarsSize[current_function] = Math.Max(FunctionVarsSize[current_function], line.Left + 1);
            if (!StringsCounter.ContainsKey(line.Right))
            {
                StringsCounter[line.Right] = string_counter++;
            }

        }

        public void Visit(AssignmentLabelToVariableCodeLine line)
        {
            FunctionVarsSize[current_function] = Math.Max(FunctionVarsSize[current_function], line.Left + 1);
        }

        public void Visit(BinaryOperationCodeLine line)
        {
            FunctionVarsSize[current_function] = Math.Max(FunctionVarsSize[current_function], line.AssignVariable + 1);
        }

        public void Visit(UnaryOperationCodeLine line)
        {
            FunctionVarsSize[current_function] = Math.Max(FunctionVarsSize[current_function], line.AssignVariable + 1);
        }

        public void Visit(ParamCodeLine line)
        {
            FunctionVarsSize[current_function] = Math.Max(FunctionVarsSize[current_function], line.VariableCounter + 1);
            ++FunctionParamsCount[current_function];
        }

        public void Visit(ReturnCodeLine line)
        {
            FunctionLimits[current_function] = (FunctionLimits[current_function].Item1, current_line);
        }

        public void Visit(AssignmentStringToMemoryCodeLine line)
        {
            if (!StringsCounter.ContainsKey(line.Right))
            {
                StringsCounter[line.Right] = string_counter++;
            }
        }

        public void Visit(InheritCodeLine line)
        {
            Inherit[line.Child] = line.Parent;

            if (!StringsCounter.ContainsKey(line.Child))
                StringsCounter[line.Child] = string_counter++;
            if (!StringsCounter.ContainsKey(line.Parent))
                StringsCounter[line.Parent] = string_counter++;
        }

        public void Visit(AssignmentVariableToMemoryCodeLine line)
        {
            return;
            throw new NotImplementedException();
        }

        public void Visit(AssignmentConstantToMemoryCodeLine line)
        {
            return;
            throw new NotImplementedException();
        }


        public void Visit(AssignmentLabelToMemoryCodeLine line)
        {
            return;
            throw new NotImplementedException();
        }

        public void Visit(CallLabelCodeLine line)
        {
            return;
            throw new NotImplementedException();
        }

        public void Visit(CallAddressCodeLine line)
        {
            return;
            throw new NotImplementedException();
        }

        public void Visit(CommentCodeLine line)
        {
            return;
            throw new NotImplementedException();
        }

        public void Visit(GotoJumpCodeLine line)
        {
            return;
            throw new NotImplementedException();
        }

        public void Visit(ConditionalJumpCodeLine line)
        {
            return;
            throw new NotImplementedException();
        }


        public void Visit(AssignmentNullToVariableCodeLine line)
        {
            return;
            throw new NotImplementedException();
        }

        public void Visit(PopParamCodeLine line)
        {
            return;
            throw new NotImplementedException();
        }

        public void Visit(PushParamCodeLine line)
        {
            return;
            throw new NotImplementedException();
        }

    }
}
