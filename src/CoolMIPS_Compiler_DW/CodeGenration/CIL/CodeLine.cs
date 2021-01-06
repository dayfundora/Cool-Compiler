using System;
using System.Collections.Generic;
using System.Text;
using CoolMIPS_Compiler_DW.Interfaces;

namespace CoolMIPS_Compiler_DW.CodeGenration.CIL
{
    public abstract class CodeLine
    {
        public abstract void Accept(ICodeVisitor visitor);
    }
    #region Operation Line
    public class BinaryOperationCodeLine : CodeLine
    {
        public int AssignVariable { get; }
        public int LeftOperandVariable { get; }
        public int RightOperandVariable { get; }
        public string Symbol { get; }

        public BinaryOperationCodeLine(int assign_variable, int left_operand, int right_operand, string symbol)
        {
            AssignVariable = assign_variable;
            LeftOperandVariable = left_operand;
            RightOperandVariable = right_operand;
            Symbol = symbol;
        }

        public override void Accept(ICodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"t{AssignVariable} = t{LeftOperandVariable} {Symbol} t{RightOperandVariable}";
        }
    }

    public class UnaryOperationCodeLine : CodeLine
    {
        public int AssignVariable { get; }
        public int OperandVariable { get; }
        public string Symbol { get; }

        public UnaryOperationCodeLine(int assign_variable, int operand, string symbol)
        {
            AssignVariable = assign_variable;
            OperandVariable = operand;
            Symbol = symbol;
        }

        public override void Accept(ICodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"t{AssignVariable} = {Symbol} t{OperandVariable}";
        }
    }
    #endregion
    #region Param Line
    public class ParamCodeLine : CodeLine
    {
        public int VariableCounter;
        public ParamCodeLine(int variable_counter)
        {
            VariableCounter = variable_counter;
        }

        public override void Accept(ICodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return "PARAM t" + VariableCounter + ";";
        }
    }
    public class PopParamCodeLine : CodeLine
    {
        int Times;
        public PopParamCodeLine(int times)
        {
            Times = times;
        }

        public override void Accept(ICodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"PopParam {Times};";
        }
    }
    public class PushParamCodeLine : CodeLine
    {
        public int Variable;
        public PushParamCodeLine(int variable)
        {
            Variable = variable;
        }

        public override void Accept(ICodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return "PushParam t" + Variable + ";";
        }
    }
    #endregion
    #region Label Line
    public class Label
    {
        public string Head { get; }
        public string Tag { get; }

        public Label(string head, string tag = "")
        {
            Head = head;
            Tag = tag;
        }


    }

    public class LabelCodeLine : CodeLine
    {
        public string Head { get; }
        public string Tag { get; }

        public string Label
        {
            get
            {
                if (Tag != "")
                    return Head + "." + Tag;
                else
                    return Head;
            }
        }

        public LabelCodeLine(string head, string tag = "")
        {
            Head = head;
            Tag = tag;
        }

        public override void Accept(ICodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return Label + ":";
        }

    
    }
    #endregion
    #region Jump Line
    public class GotoJumpCodeLine : CodeLine
    {
        public LabelCodeLine Label;

        public GotoJumpCodeLine(LabelCodeLine label)
        {
            Label = label;
        }

        public override void Accept(ICodeVisitor visitor)
        {
            visitor.Visit(this);
        }


        public override string ToString()
        {
            return $"Goto {Label.Label}";
        }
    }

    public class ConditionalJumpCodeLine : CodeLine
    {
        public LabelCodeLine Label;
        public int ConditionalVar;
        public ConditionalJumpCodeLine(int conditional_var, LabelCodeLine label)
        {
            Label = label;
            ConditionalVar = conditional_var;
        }

        public override void Accept(ICodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"IfZ t{ConditionalVar} Goto {Label.Label}";
        }
    }
    #endregion
    #region Call Line
    public class CallLabelCodeLine : CodeLine
    {
        public LabelCodeLine Method { get; }
        public int Result { get; }
        public CallLabelCodeLine(LabelCodeLine method, int result_variable = -1)
        {
            Method = method;
            Result = result_variable;
        }

        public override string ToString()
        {
            if (Result == -1)
                return $"Call {Method.Label};";
            else
                return $"t{Result} = Call {Method.Label};";
        }

        public override void Accept(ICodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class CallAddressCodeLine : CodeLine
    {
        public int Address { get; }
        public int Result { get; }
        public CallAddressCodeLine(int address, int result_variable = -1)
        {
            Address = address;
            Result = result_variable;
        }

        public override string ToString()
        {
            if (Result == -1)
                return $"Call t{Address};";
            else
                return $"t{Result} = Call t{Address};";
        }

        public override void Accept(ICodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    #endregion
    #region Assignment Line
    public abstract class AssignmentCodeLine<T> : CodeLine
    {
        public int Left { get; protected set; }
        public T Right { get; protected set; }

    }

    public class AssignmentVariableToMemoryCodeLine : AssignmentCodeLine<int>
    {
        public int Offset { get; }
        public AssignmentVariableToMemoryCodeLine(int left, int right, int offset = 0)
        {
            Left = left;
            Right = right;
            Offset = offset;
        }

        public override void Accept(ICodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"*(t{Left} + {Offset}) = t{Right}";
        }
    }

    public class AssignmentVariableToVariableCodeLine : AssignmentCodeLine<int>
    {
        public AssignmentVariableToVariableCodeLine(int left, int right)
        {
            Left = left;
            Right = right;
        }

        public override void Accept(ICodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"t{Left} = t{Right}";
        }
    }

    public class AssignmentConstantToMemoryCodeLine : AssignmentCodeLine<int>
    {
        public int Offset { get; }
        public AssignmentConstantToMemoryCodeLine(int left, int right, int offset = 0)
        {
            Left = left;
            Right = right;
            Offset = offset;
        }

        public override void Accept(ICodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"*(t{Left} + {Offset}) = {Right}";
        }
    }

    public class AssignmentMemoryToVariableCodeLine : AssignmentCodeLine<int>
    {
        public int Offset { get; }

        public AssignmentMemoryToVariableCodeLine(int left, int right, int offset = 0)
        {
            Left = left;
            Right = right;
            Offset = offset;
        }

        public override void Accept(ICodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"t{Left} = *(t{Right} + {Offset})"; ;
        }
    }


    public class AssignmentConstantToVariableCodeLine : AssignmentCodeLine<int>
    {

        public AssignmentConstantToVariableCodeLine(int left, int right)
        {
            Left = left;
            Right = right;
        }

        public override void Accept(ICodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"t{Left} = {Right}";
        }
    }

    public class AssignmentStringToVariableCodeLine : AssignmentCodeLine<string>
    {

        public AssignmentStringToVariableCodeLine(int left, string right)
        {
            Left = left;
            Right = right;
        }

        public override void Accept(ICodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"t{Left} = \"{Right}\"";
        }
    }

    public class AssignmentStringToMemoryCodeLine : AssignmentCodeLine<string>
    {
        public int Offset { get; }
        public AssignmentStringToMemoryCodeLine(int left, string right, int offset = 0)
        {
            Left = left;
            Right = right;
            Offset = offset;
        }

        public override void Accept(ICodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"*(t{Left} + {Offset}) = \"{Right}\"";
        }
    }


    public class AssignmentLabelToVariableCodeLine : AssignmentCodeLine<LabelCodeLine>
    {
        public AssignmentLabelToVariableCodeLine(int left, LabelCodeLine right)
        {
            Left = left;
            Right = right;
        }

        public override void Accept(ICodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"t{Left} = \"{Right.Label}\"";
        }
    }

    public class AssignmentLabelToMemoryCodeLine : AssignmentCodeLine<LabelCodeLine>
    {
        public int Offset { get; }
        public AssignmentLabelToMemoryCodeLine(int left, LabelCodeLine right, int offset)
        {
            Left = left;
            Right = right;
            Offset = offset;
        }

        public override void Accept(ICodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"*(t{Left} + {Offset}) = Label \"{Right.Label}\"";
        }
    }

    public class AssignmentNullToVariableCodeLine : CodeLine
    {
        public int Variable { get; }

        public AssignmentNullToVariableCodeLine(int variable)
        {
            Variable = variable;
        }

        public override void Accept(ICodeVisitor visitor)
        {
            visitor.Visit(this);
        }


        public override string ToString()
        {
            return $"t{Variable} = NULL;";
        }
    }
    #endregion

    public class AllocateCodeLine : CodeLine
    {
        public int Variable { get; }
        public int Size { get; }

        public AllocateCodeLine(int variable, int size)
        {
            Variable = variable;
            Size = size;
        }

        public override void Accept(ICodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"t{Variable} = Alloc {Size};";
        }
    }
    public class CommentCodeLine : CodeLine
    {
        string Comment { get; }
        public CommentCodeLine(string comment)
        {
            Comment = comment;
        }

        public override void Accept(ICodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return "// " + Comment;
        }
    }
    public class InheritCodeLine : CodeLine
    {
        public string Child;
        public string Parent;


        public InheritCodeLine(string child, string parent)
        {
            Child = child;
            Parent = parent;
        }

        public override void Accept(ICodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"_class.{Child}: _class.{Parent}";
        }
    }
    public class ReturnCodeLine : CodeLine
    {
        public int Variable { get; }

        public ReturnCodeLine(int variable = -1)
        {
            Variable = variable;
        }

        public override void Accept(ICodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return "Return " + (Variable == -1 ? "" : "t" + Variable) + ";\n";
        }
    }

}
