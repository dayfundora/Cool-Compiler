using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using CoolMIPS_Compiler_DW.AST;
using CoolMIPS_Compiler_DW.Interfaces;

namespace CoolMIPS_Compiler_DW.Semantic
{
    public static class Semantic
    {
        public static void Check(ProgramNode root, Scope scope)
        {
            var errors = new List<string>();

            Console.ForegroundColor = ConsoleColor.Red;

            var programNode = new FirstSemanticVisit().CheckSemantic(root, scope, errors);
            errors.ForEach(e => Console.WriteLine(e));
            
            programNode = new SecondSemanticVisit().CheckSemantic(programNode, scope, errors);
            errors.ForEach(e => Console.WriteLine(e));

            Console.ForegroundColor = ConsoleColor.Gray;
            if (errors.Count > 0) Environment.Exit(1);

        }
    }
    public class FirstSemanticVisit : IVisitor
    {
        IScope scope;
        ICollection<string> errors;
        public ProgramNode CheckSemantic(ProgramNode node, IScope scope, ICollection<string> errors)
        {
            this.scope = scope;
            this.errors = errors;
            node.Accept(this);
            return node;
        }

        public void Visit(ProgramNode node)
        {
            if (!SemanticAlgorithm.TopologicalSort(node.Classes, errors))
                return;

            node.Classes.ForEach(cclass => scope.AddType(cclass.TypeClass.Text, new TypeInfo(cclass.TypeClass.Text, scope.GetType(cclass.TypeInherit.Text), cclass)));

            int idMain = -1;
            for (int i = 0; i < node.Classes.Count; ++i)
                if (node.Classes[i].TypeClass.Text == "Main")
                    idMain = i;

            if (idMain == -1)
            {
                errors.Add(SemanticError.NotFoundClassMain());
                return;
            }

            bool mainOK = false;
            foreach (var item in node.Classes[idMain].FeatureNodes)
            {
                if (item is MethodNode)
                {
                    var method = item as MethodNode;
                    if (method.Id.Text == "main" && method.Arguments.Count == 0)
                        mainOK = true;
                }
            }

            if (!mainOK)
                errors.Add(SemanticError.NotFoundMethodmain(node.Classes[idMain]));

            foreach (var cclass in node.Classes)
            {
                if (!scope.IsDefinedType(cclass.TypeInherit.Text, out TypeInfo type))
                {
                    errors.Add(SemanticError.NotDeclaredType(cclass.TypeInherit));
                    return;
                }
                if (new List<string> { "Bool", "Int", "String" }.Contains(type.Text))
                {
                    errors.Add(SemanticError.NotInheritsOf(cclass, type));
                    return;
                }
                cclass.Accept(this);
            }
        }

        public void Visit(ClassNode node)
        {
            FirstSemanticVisit tour = new FirstSemanticVisit();
            tour.scope = new Scope
            {
                Type = scope.GetType(node.TypeClass.Text),
                Parent = scope.GetType(node.TypeInherit.Text).ClassReference.Scope
            };
            tour.errors = errors;
            node.Scope = tour.scope;

            node.FeatureNodes.ForEach(feature => feature.Accept(tour));
        }

        public void Visit(AttributeNode node)
        {
            if (!scope.IsDefinedType(node.Formal.Type.Text, out TypeInfo type))
                errors.Add(SemanticError.NotDeclaredType(node.Formal.Type));

            if (scope.IsDefined(node.Formal.Id.Text, out TypeInfo t))
                errors.Add(SemanticError.RepeatedVariable(node.Formal.Id));

            scope.Define(node.Formal.Id.Text, type);
        }

        public void Visit(MethodNode node)
        {
            if (!scope.IsDefinedType(node.TypeReturn.Text, out TypeInfo typeReturn))
                errors.Add(SemanticError.NotDeclaredType(node.TypeReturn));

            node.TypeReturn = new TypeNode(node.TypeReturn.Line, node.TypeReturn.Column, typeReturn.Text);

            TypeInfo[] typeArgs = new TypeInfo[node.Arguments.Count];
            for (int i = 0; i < node.Arguments.Count; ++i)
                if (!scope.IsDefinedType(node.Arguments[i].Type.Text, out typeArgs[i]))
                    errors.Add(SemanticError.NotDeclaredType(node.Arguments[i].Type));

            scope.Define(node.Id.Text, typeArgs, typeReturn);
        }

        #region NOT IMPLEMENTATION
        public void Visit(AssignmentNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(ArithmeticOperationNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(SequenceNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(BoolNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(CaseNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(DispatchExplicitNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(DispatchImplicitNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(EqualNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(ComparisonOperation node)
        {
            throw new NotImplementedException();
        }

        public void Visit(FormalNode formalNode)
        {
            throw new NotImplementedException();
        }

        public void Visit(IfNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(IntNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(IsVoidNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(LetNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(NegNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(NewNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(NotNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(StringNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(IdentifierNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(WhileNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(VoidNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(SelfNode node)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
    public class SecondSemanticVisit : IVisitor
    {
        IScope scope;
        ICollection<string> errors;

        public SecondSemanticVisit() { }
        public SecondSemanticVisit(IScope scope, ICollection<string> errors)
        {
            this.scope = scope;
            this.errors = errors;
        }

        public ProgramNode CheckSemantic(ProgramNode node, IScope scope, ICollection<string> errors)
        {
            this.scope = scope;
            this.errors = errors;
            node.Accept(this);
            return node;
        }

        #region Program and Class
        public void Visit(ProgramNode node)
        {
            node.Classes.ForEach(cclass => cclass.Accept(new SecondSemanticVisit(cclass.Scope, errors)));
        }

        public void Visit(ClassNode node)
        {
            node.FeatureNodes.ForEach(feature => feature.Accept(this));
        }
        #endregion

        #region Feature
        public void Visit(AttributeNode node)
        {
            node.AssignExp.Accept(this);
            var typeAssignExp = node.AssignExp.StaticType;

            if (!scope.IsDefinedType(node.Formal.Type.Text, out TypeInfo typeDeclared))
                errors.Add(SemanticError.NotDeclaredType(node.Formal.Type));

            if (!(typeAssignExp <= typeDeclared))
                errors.Add(SemanticError.CannotConvert(node.Formal.Type, typeAssignExp, typeDeclared));

            scope.Define(node.Formal.Id.Text, typeDeclared);
        }

        public void Visit(MethodNode node)
        {
            var scopeMethod = scope.CreateChild();
            foreach (var arg in node.Arguments)
            {
                if (!scope.IsDefinedType(arg.Type.Text, out TypeInfo typeArg))
                    errors.Add(SemanticError.NotDeclaredType(arg.Type));
                scopeMethod.Define(arg.Id.Text, typeArg);
            }

            if (!scope.IsDefinedType(node.TypeReturn.Text, out TypeInfo typeReturn))
                errors.Add(SemanticError.NotDeclaredType(node.TypeReturn));

            scope.Define(node.Id.Text, node.Arguments.Select(x => scope.GetType(x.Type.Text)).ToArray(), typeReturn);

            node.Body.Accept(new SecondSemanticVisit(scopeMethod, errors));

            if (!(node.Body.StaticType <= typeReturn))
                errors.Add(SemanticError.CannotConvert(node.Body, node.Body.StaticType, typeReturn));

            node.TypeReturn = new TypeNode(node.Body.Line, node.Body.Column, typeReturn.Text);
        }
        #endregion

        #region Unary Operation
        public void Visit(IsVoidNode node)
        {
            node.Operand.Accept(this);

            if (!scope.IsDefinedType("Bool", out node.StaticType))
                errors.Add(SemanticError.NotDeclaredType(new TypeNode(node.Line, node.Column, "Bool")));
        }

        public void Visit(NotNode node)
        {
            node.Operand.Accept(this);

            if (node.Operand.StaticType.Text != "Bool")
                errors.Add(SemanticError.InvalidUseOfOperator(node, node.Operand.StaticType));

            if (!scope.IsDefinedType("Bool", out node.StaticType))
                errors.Add(SemanticError.NotDeclaredType(new TypeNode(node.Line, node.Column, "Bool")));
        }

        public void Visit(NegNode node)
        {
            node.Operand.Accept(this);

            if (node.Operand.StaticType.Text != "Int")
                errors.Add(SemanticError.InvalidUseOfOperator(node, node.Operand.StaticType));

            if (!scope.IsDefinedType("Int", out node.StaticType))
                errors.Add(SemanticError.NotDeclaredType(new TypeNode(node.Line, node.Column, "Int")));
        }
        #endregion

        #region Binary Operation
        public void Visit(ArithmeticOperationNode node)
        {
            node.LeftOperand.Accept(this);
            node.RightOperand.Accept(this);

            if (node.LeftOperand.StaticType.Text != node.RightOperand.StaticType.Text)
                errors.Add(SemanticError.InvalidUseOfOperator(node, node.LeftOperand.StaticType, node.RightOperand.StaticType));

            else if (node.LeftOperand.StaticType.Text != "Int" || node.RightOperand.StaticType.Text != "Int")
                errors.Add(SemanticError.InvalidUseOfOperator(node));

            else if (!scope.IsDefinedType("Int", out node.StaticType))
                errors.Add(SemanticError.NotDeclaredType(new TypeNode(node.Line, node.Column, "Int")));
        }

        public void Visit(ComparisonOperation node)
        {
            node.LeftOperand.Accept(this);
            node.RightOperand.Accept(this);

            if (node.LeftOperand.StaticType.Text != "Int" || node.RightOperand.StaticType.Text != "Int")
                errors.Add(SemanticError.InvalidUseOfOperator(node, node.LeftOperand.StaticType, node.RightOperand.StaticType));

            if (!scope.IsDefinedType("Bool", out node.StaticType))
                errors.Add(SemanticError.NotDeclaredType(new TypeNode(node.Line, node.Column, "Bool")));
        }

        public void Visit(EqualNode node)
        {
            node.LeftOperand.Accept(this);
            node.RightOperand.Accept(this);

            if (node.LeftOperand.StaticType.Text != node.RightOperand.StaticType.Text || !(new string[3] { "Bool", "Int", "String" }.Contains(node.LeftOperand.StaticType.Text)))
                errors.Add(SemanticError.InvalidUseOfOperator(node, node.LeftOperand.StaticType, node.RightOperand.StaticType));

            if (!scope.IsDefinedType("Bool", out node.StaticType))
                errors.Add(SemanticError.NotDeclaredType(new TypeNode(node.Line, node.Column, "Bool")));
        }
        #endregion

        #region Block and Assignment
        public void Visit(SequenceNode node)
        {
            node.Sequence.ForEach(exp => exp.Accept(this));

            var last = node.Sequence[node.Sequence.Count - 1];

            if (!scope.IsDefinedType(last.StaticType.Text, out node.StaticType))
                errors.Add(SemanticError.NotDeclaredType(new TypeNode(last.Line, last.Column, last.StaticType.Text)));
        }

        public void Visit(AssignmentNode node)
        {
            node.ExpressionRight.Accept(this);

            if (!scope.IsDefined(node.ID.Text, out TypeInfo type))
                errors.Add(SemanticError.NotDeclaredVariable(node.ID));

            if (!(node.ExpressionRight.StaticType <= type))
                errors.Add(SemanticError.CannotConvert(node, node.ExpressionRight.StaticType, type));

            node.StaticType = node.ExpressionRight.StaticType;
        }

        public void Visit(VoidNode node)
        {
            node.StaticType = scope.GetType(node.GetStaticType);
        }
        #endregion

        #region Dispatch
        public void Visit(DispatchExplicitNode node)
        {
            node.Expression.Accept(this);
            if (node.IdType.Text == "Object")
                node.IdType = new TypeNode(node.Expression.Line, node.Expression.Column, node.Expression.StaticType.Text);

            if (!scope.IsDefinedType(node.IdType.Text, out TypeInfo typeSuperClass))
                errors.Add(SemanticError.NotDeclaredType(node.IdType));

            if (!(node.Expression.StaticType <= typeSuperClass))
                errors.Add(SemanticError.CannotConvert(node, node.Expression.StaticType, typeSuperClass));

            node.Arguments.ForEach(x => x.Accept(this));

            var scopeSuperClass = typeSuperClass.ClassReference.Scope;
            if (!(scopeSuperClass.IsDefined(node.IdMethod.Text, node.Arguments.Select(x => x.StaticType).ToArray(), out node.StaticType)))
                errors.Add(SemanticError.NotDeclareFunction(node, node.IdMethod.Text));
        }

        public void Visit(DispatchImplicitNode node)
        {
            node.Arguments.ForEach(expArg => expArg.Accept(this));

            if (!scope.IsDefined(node.IdMethod.Text, node.Arguments.Select(x => x.StaticType).ToArray(), out node.StaticType))
                errors.Add(SemanticError.NotDeclareFunction(node, node.IdMethod.Text));
        }
        #endregion

        #region Atom
        public void Visit(IntNode node)
        {
            if (!scope.IsDefinedType("Int", out node.StaticType))
                errors.Add(SemanticError.NotDeclaredType(new TypeNode(node.Line, node.Column, "Int")));
        }

        public void Visit(BoolNode node)
        {
            if (!scope.IsDefinedType("Bool", out node.StaticType))
                errors.Add(SemanticError.NotDeclaredType(new TypeNode(node.Line, node.Column, "Int")));
        }

        public void Visit(StringNode node)
        {
            if (!scope.IsDefinedType("String", out node.StaticType))
                errors.Add(SemanticError.NotDeclaredType(new TypeNode(node.Line, node.Column, "Int")));
        }
        #endregion

        #region Identifier, Formal and Self
        public void Visit(IdentifierNode node)
        {
            if (!scope.IsDefined(node.Text, out node.StaticType))
                errors.Add(SemanticError.NotDeclaredVariable(node));
        }

        public void Visit(FormalNode node)
        {
            if (!scope.IsDefinedType(node.Type.Text, out node.StaticType))
                errors.Add(SemanticError.NotDeclaredType(node.Type));
        }

        public void Visit(SelfNode node)
        {
            node.StaticType = scope.Type;
        }
        #endregion

        #region Keywords
        public void Visit(CaseNode node)
        {
            node.ExpressionCase.Accept(this);

            int branchSelected = -1;
            var typeExp0 = node.ExpressionCase.StaticType;
            var typeExpK = scope.GetType(node.Branches[0].Formal.Type.Text);

            for (int i = 0; i < node.Branches.Count; ++i)
            {
                if (!scope.IsDefinedType(node.Branches[i].Formal.Type.Text, out TypeInfo type))
                    errors.Add(SemanticError.NotDeclaredType(node.Branches[i].Formal.Type));

                var typeK = scope.GetType(node.Branches[i].Formal.Type.Text);

                var scopeBranch = scope.CreateChild();
                scopeBranch.Define(node.Branches[i].Formal.Id.Text, typeK);

                node.Branches[i].Expression.Accept(new SecondSemanticVisit(scopeBranch, errors));

                typeExpK = node.Branches[i].Expression.StaticType;

                if (branchSelected == -1 && typeExp0 <= typeK)
                    branchSelected = i;

                if (i == 0)
                    node.StaticType = node.Branches[0].Expression.StaticType;
                node.StaticType = SemanticAlgorithm.LowerCommonAncestor(node.StaticType, typeExpK);
            }
            node.BranchSelected = branchSelected;

            if (node.BranchSelected == -1)
                errors.Add(SemanticError.NotMatchedBranch(node));
        }

        public void Visit(IfNode node)
        {
            node.Condition.Accept(this);
            node.Body.Accept(this);
            node.ElseBody.Accept(this);

            if (node.Condition.StaticType.Text != "Bool")
                errors.Add(SemanticError.CannotConvert(node.Condition, node.Condition.StaticType, scope.GetType("Bool")));

            node.StaticType = SemanticAlgorithm.LowerCommonAncestor(node.Body.StaticType, node.ElseBody.StaticType);
        }

        public void Visit(LetNode node)
        {
            var scopeLet = scope.CreateChild();

            foreach (var expInit in node.Initialization)
            {
                expInit.AssignExp.Accept(new SecondSemanticVisit(scopeLet, errors));
                var typeAssignExp = expInit.AssignExp.StaticType;

                if (!scopeLet.IsDefinedType(expInit.Formal.Type.Text, out TypeInfo typeDeclared))
                    errors.Add(SemanticError.NotDeclaredType(expInit.Formal.Type));

                if (!(typeAssignExp <= typeDeclared))
                    errors.Add(SemanticError.CannotConvert(expInit.Formal.Type, typeAssignExp, typeDeclared));

                if (scopeLet.IsDefined(expInit.Formal.Id.Text, out TypeInfo typeOld))
                    scopeLet.UpdateType(expInit.Formal.Id.Text, typeDeclared);
                else
                    scopeLet.Define(expInit.Formal.Id.Text, typeDeclared);
            }

            node.ExpressionBody.Accept(new SecondSemanticVisit(scopeLet, errors));
            node.StaticType = node.ExpressionBody.StaticType;
        }

        public void Visit(NewNode node)
        {
            if (!scope.IsDefinedType(node.TypeId.Text, out node.StaticType))
                errors.Add(SemanticError.NotDeclaredType(node.TypeId));
        }

        public void Visit(WhileNode node)
        {
            node.Condition.Accept(this);
            node.Body.Accept(this);

            if (node.Condition.StaticType.Text != "Bool")
                errors.Add(SemanticError.CannotConvert(node.Condition, node.Condition.StaticType, scope.GetType("Bool")));

            if (!scope.IsDefinedType("Object", out node.StaticType))
                errors.Add(SemanticError.NotDeclaredType(new TypeNode(node.Line, node.Column, "Object")));
        }
        #endregion
    }
    public static class SemanticAlgorithm
    {
        enum Color { White, Gray, Black };

        static private Dictionary<string, int> _id;
        static private Color[] _mk;
        static List<int>[] g;
        static List<int> tp;

        private static int idA, idB;

        private static void Init(int n)
        {
            _id = new Dictionary<string, int>();
            _mk = new Color[n];
            tp = new List<int>();
            g = new List<int>[n];
            for (int i = 0; i < n; ++i)
                g[i] = new List<int>();
        }

        public static bool TopologicalSort(List<ClassNode> classNodes, ICollection<string> errors)
        {
            int n = classNodes.Count;
            Init(n);

            for (int i = 0; i < n; ++i)
            {
                if (_id.ContainsKey(classNodes[i].TypeClass.Text))
                {
                    errors.Add(SemanticError.RepeatedClass(classNodes[i].TypeClass));
                    return false;
                }
                else
                {
                    _id.Add(classNodes[i].TypeClass.Text, i);
                }
            }

            foreach (var item in classNodes)
            {
                int u = Hash(item.TypeClass);
                int v = Hash(item.TypeInherit);
                if (v != -1)
                    g[u].Add(v);
            }

            for (int u = 0; u < n; ++u)
            {
                if (_mk[u] == Color.White && !Dfs(u))
                {
                    errors.Add(SemanticError.InvalidClassDependency(classNodes[idA].TypeClass, classNodes[idB].TypeClass));
                    return false;
                }
            }

            ClassNode[] ans = new ClassNode[n];
            for (int i = 0; i < n; ++i) ans[i] = classNodes[tp[i]];
            for (int i = 0; i < n; ++i) classNodes[i] = ans[i];

            return true;
        }

        private static bool Dfs(int u)
        {
            _mk[u] = Color.Gray;
            foreach (var v in g[u])
            {
                if (_mk[v] == Color.Gray)
                {
                    idA = u;
                    idB = v;
                    return false;
                }
                if (_mk[v] == Color.White && !Dfs(v))
                    return false;
            }
            tp.Add(u);
            _mk[u] = Color.Black;
            return true;
        }

        private static int Hash(TypeNode type)
        {
            return _id.ContainsKey(type.Text) ? _id[type.Text] : -1;
        }

        public static TypeInfo LowerCommonAncestor(TypeInfo type1, TypeInfo type2)
        {
            while (type1.Level < type2.Level) type2 = type2.Parent;
            while (type2.Level < type1.Level) type1 = type1.Parent;
            while (type1 != type2) { type1 = type1.Parent; type2 = type2.Parent; }
            return type1;
        }

    }
}
