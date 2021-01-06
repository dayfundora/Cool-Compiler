using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using CoolMIPS_Compiler_DW.Interfaces;
using CoolMIPS_Compiler_DW.Semantic;

namespace CoolMIPS_Compiler_DW.AST
{
    public abstract class ASTNode
    {
        public int Line { get; }

        public int Column { get; }

        public Dictionary<string, dynamic> Attributes { get; }

        public ASTNode(ParserRuleContext context)
        {
            Line = context.Start.Line;
            Column = context.Start.Column + 1;
            Attributes = new Dictionary<string, dynamic>();
        }

        public ASTNode(int line, int column)
        {
            Line = line;
            Column = column;
        }

    }
    public class ProgramNode : ASTNode, IVisit
    {
        public List<ClassNode> Classes { get; set; }

        public ProgramNode(ParserRuleContext context) : base(context)
        {
        }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public class ClassNode : ASTNode, IVisit
    {
        public TypeNode TypeClass { get; set; }

        public TypeNode TypeInherit { get; set; }

        public List<FeatureNode> FeatureNodes { get; set; }

        public IScope Scope { get; set; }

        public ClassNode(ParserRuleContext context) : base(context)
        {
        }

        public ClassNode(int line, int column, string className, string classInherit) : base(line, column)
        {
            TypeClass = new TypeNode(line, column, className);
            TypeInherit = new TypeNode(line, column, classInherit);
        }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public abstract class FeatureNode : ASTNode, IVisit
    {
        public FeatureNode(ParserRuleContext context) : base(context) { }

        public abstract void Accept(IVisitor visitor);
    }
    public class MethodNode : FeatureNode
    {
        public IdNode Id { get; set; }
        public List<FormalNode> Arguments { get; set; }
        public TypeNode TypeReturn { get; set; }
        public ExpressionNode Body { get; set; }

        public MethodNode(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public class AttributeNode : FeatureNode
    {
        public FormalNode Formal { get; set; }
        public ExpressionNode AssignExp { get; set; }

        public AttributeNode(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public abstract class ExpressionNode : ASTNode, IVisit
    {
        public TypeInfo StaticType = TypeInfo.OBJECT;

        public ExpressionNode(ParserRuleContext context) : base(context) { }

        public ExpressionNode(int line, int column) : base(line, column) { }

        public abstract void Accept(IVisitor visitor);

    }
    public class SequenceNode : ExpressionNode
    {
        public List<ExpressionNode> Sequence { get; set; }

        public SequenceNode(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public abstract class AuxiliaryNode : ASTNode
    {
        public AuxiliaryNode(ParserRuleContext context) : base(context) { }

        public AuxiliaryNode(int line, int column) : base(line, column) { }


    }
    public class FormalNode : ExpressionNode
    {
        public IdentifierNode Id { get; set; }
        public TypeNode Type { get; set; }

        public FormalNode(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public class IdNode : AuxiliaryNode
    {
        public string Text { get; set; }

        public IdNode(ParserRuleContext context, string text) : base(context)
        {
            Text = text;
        }

        public IdNode(int line, int column, string text) : base(line, column)
        {
            Text = text;
        }

        public override string ToString()
        {
            return Text;
        }

        #region NULL
        static readonly NullId nullId = new NullId();

        public static NullId NULL => nullId;

        public class NullId : IdNode
        {
            public NullId(int line = 0, int column = 0, string name = "Null-Id-Object") : base(line, column, name) { }
        }
        #endregion

    }
    public class TypeNode : AuxiliaryNode
    {
        public string Text { get; set; }

        public TypeNode(ParserRuleContext context, string text) : base(context)
        {
            Text = text;
        }

        public TypeNode(int line, int column, string text) : base(line, column)
        {
            Text = text;
        }

        public override string ToString()
        {
            return Text;
        }

        #region OBJECT
        private static readonly ObjectType objectType = new ObjectType();

        public static ObjectType OBJECT => objectType;

        public class ObjectType : TypeNode
        {
            public ObjectType(int line = 0, int column = 0, string name = "Object") : base(line, column, name) { }
        }
        #endregion

    }

    #region AtomNodes
    public abstract class AtomNode : ExpressionNode
    {
        public AtomNode(ParserRuleContext context) : base(context) { }

        public AtomNode(int line, int column) : base(line, column) { }

    }
    public class VoidNode : AtomNode
    {
        public string GetStaticType { get; }
        public VoidNode(string type, int line = 0, int column = 0) : base(line, column)
        {
            GetStaticType = type;
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public class StringNode : AtomNode
    {
        public string Text { get; set; }

        public StringNode(ParserRuleContext context, string text) : base(context)
        {
            Text = "";

            // 0 to Length - 1 in order to remove the (Antlr delivered) ""
            for (int i = 1; i < text.Length - 1; ++i)
            {
                Text += text[i];
            }

            //Text = text;
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public class SelfNode : AtomNode
    {
        public SelfNode(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public class IntNode : AtomNode
    {
        public int Value { get; set; }

        public IntNode(ParserRuleContext context, string text) : base(context)
        {
            Value = int.Parse(text);
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public class IdentifierNode : AtomNode
    {
        public string Text { get; set; }

        public IdentifierNode(ParserRuleContext context, string text) : base(context)
        {
            Text = text;
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public class BoolNode : AtomNode
    {
        public bool Value { get; set; }

        public BoolNode(ParserRuleContext context, string text) : base(context)
        {
            Value = bool.Parse(text);
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    #endregion

    #region DispatchNode
    public abstract class DispatchNode : ExpressionNode
    {
        public IdNode IdMethod { get; set; }

        public List<ExpressionNode> Arguments { get; set; }

        public DispatchNode(ParserRuleContext context) : base(context)
        {

        }

    }
    public class DispatchImplicitNode : DispatchNode
    {
        public DispatchImplicitNode(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public class DispatchExplicitNode : DispatchNode
    {
        public ExpressionNode Expression { get; set; }
        public TypeNode IdType { get; set; }

        public DispatchExplicitNode(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    #endregion

    #region KeywordNode
    public abstract class KeywordNode : ExpressionNode
    {
        public KeywordNode(ParserRuleContext context) : base(context)
        {
        }
    }
    public class AssignmentNode : ExpressionNode
    {
        public IdentifierNode ID { get; set; }
        public ExpressionNode ExpressionRight { get; set; }

        public AssignmentNode(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

    }
    public class LetNode : KeywordNode
    {
        public List<AttributeNode> Initialization { get; set; }
        public ExpressionNode ExpressionBody { get; set; }

        public LetNode(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public class NewNode : KeywordNode
    {
        public TypeNode TypeId { get; set; }

        public NewNode(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    //Flow Control
    public class CaseNode : KeywordNode
    {
        public ExpressionNode ExpressionCase { get; set; }
        public List<(FormalNode Formal, ExpressionNode Expression)> Branches { get; set; }
        public int BranchSelected { get; set; }

        public CaseNode(ParserRuleContext context) : base(context)
        {
            Branches = new List<(FormalNode, ExpressionNode)>();
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    //Flow Control
    public class IfNode : KeywordNode
    {
        public ExpressionNode Condition { get; set; }
        public ExpressionNode Body { get; set; }
        public ExpressionNode ElseBody { get; set; }

        public IfNode(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    //Flow Control
    public class WhileNode : KeywordNode
    {
        public ExpressionNode Condition { get; set; }
        public ExpressionNode Body { get; set; }

        public WhileNode(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    #endregion

    #region UnaryOperationNode
    public abstract class UnaryOperationNode : ExpressionNode
    {
        public ExpressionNode Operand { get; set; }

        public abstract string Symbol { get; }

        public UnaryOperationNode(ParserRuleContext context) : base(context)
        {
        }
    }
    public class NotNode : UnaryOperationNode
    {
        public override string Symbol => "not";

        public NotNode(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

    }
    public class NegNode : UnaryOperationNode
    {
        public override string Symbol => "~";

        public NegNode(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public class IsVoidNode : UnaryOperationNode
    {
        public override string Symbol => "isvoid";

        public IsVoidNode(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

    }

    #endregion

    #region BinaryOperationNode
    public abstract class BinaryOperationNode : ExpressionNode
    {
        public ExpressionNode LeftOperand { get; set; }
        public ExpressionNode RightOperand { get; set; }

        public BinaryOperationNode(ParserRuleContext context) : base(context)
        {
        }

        public abstract string Symbol { get; }

    }
    public abstract class ComparisonOperation : BinaryOperationNode
    {
        public ComparisonOperation(ParserRuleContext context) : base(context)
        {
        }

    }
    public class LessEqual : ComparisonOperation
    {
        public override string Symbol => "<=";

        public LessEqual(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public class Less : ComparisonOperation
    {
        public override string Symbol => "<";

        public Less(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public class EqualNode : ComparisonOperation
    {
        public override string Symbol => "=";

        public EqualNode(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }


    public abstract class ArithmeticOperationNode : BinaryOperationNode
    {
        public ArithmeticOperationNode(ParserRuleContext context) : base(context)
        {
        }
    }
    class AddNode : ArithmeticOperationNode
    {
        public override string Symbol => "+";

        public AddNode(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    class SubNode : ArithmeticOperationNode
    {
        public override string Symbol => "-";

        public SubNode(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    class MulNode : ArithmeticOperationNode
    {
        public override string Symbol => "*";

        public MulNode(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    class DivNode : ArithmeticOperationNode
    {
        public override string Symbol => "/";

        public DivNode(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    #endregion


}


