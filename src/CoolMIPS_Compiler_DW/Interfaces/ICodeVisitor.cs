using System;
using System.Collections.Generic;
using System.Text;
using CoolMIPS_Compiler_DW.CodeGenration.CIL;

namespace CoolMIPS_Compiler_DW.Interfaces
{
    public interface ICodeVisitor
    {
        void Visit(AllocateCodeLine line);

        void Visit(AssignmentVariableToMemoryCodeLine line);

        void Visit(AssignmentVariableToVariableCodeLine line);

        void Visit(AssignmentConstantToMemoryCodeLine line);

        void Visit(AssignmentMemoryToVariableCodeLine line);

        void Visit(AssignmentConstantToVariableCodeLine line);

        void Visit(AssignmentStringToVariableCodeLine line);

        void Visit(AssignmentStringToMemoryCodeLine line);

        void Visit(AssignmentLabelToVariableCodeLine line);

        void Visit(AssignmentLabelToMemoryCodeLine line);

        void Visit(CallLabelCodeLine line);

        void Visit(CallAddressCodeLine line);

        void Visit(CommentCodeLine line);

        void Visit(GotoJumpCodeLine line);

        void Visit(ConditionalJumpCodeLine line);

        void Visit(LabelCodeLine line);

        void Visit(AssignmentNullToVariableCodeLine line);

        void Visit(BinaryOperationCodeLine line);

        void Visit(UnaryOperationCodeLine line);

        void Visit(ParamCodeLine line);

        void Visit(PopParamCodeLine line);

        void Visit(PushParamCodeLine line);

        void Visit(ReturnCodeLine line);
        void Visit(InheritCodeLine line);
    }
}
