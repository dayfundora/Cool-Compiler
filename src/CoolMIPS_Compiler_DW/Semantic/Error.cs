using System;
using System.Collections.Generic;
using System.Text;
using CoolMIPS_Compiler_DW.AST;

namespace CoolMIPS_Compiler_DW.Semantic
{
    public class SemanticError
    {

        public static string InvalidClassDependency(TypeNode confilctClassA, TypeNode confilctClassB)
        {


            return $"({confilctClassA.Line}, {confilctClassA.Column}) - Semantic Error: " + "Circular base class dependency involving " +
                   $" '{confilctClassA.Text}' and '{confilctClassB.Text}' in (line:{confilctClassB.Line}, column:{confilctClassB.Column})"
                   ;
        }

        public static string RepeatedClass(TypeNode node)
        {
            return $"({node.Line}, {node.Column})" +
                    $" - Semantic Error: The program already contains a definition for '{node.Text}'."
                    ;
        }

        public static string NotFoundClassMain()
        {
            return $"(0, 0) - Semantic Error: Couldn't found the class 'Main'.";
        }

        public static string NotFoundMethodmain(ClassNode node)
        {
            return $"({node.Line}, {node.Column})" +
                   $" - Semantic Error: The class '{node.TypeClass.Text}' has not a method 'main' without parameters."
                    ;
        }

        public static string NotDeclaredVariable(IdentifierNode node)
        {
            return $"({node.Line}, {node.Column}) - Semantic Error: The name " +
                    $"'{node.Text}' does not exist in the current context."
                    ;
        }

        public static string NotDeclaredType(TypeNode node)
        {
            return $"({node.Line}, {node.Column}) - Semantic Error:" +
                    $" The type '{node.Text}' could not be found."
                    ;
        }

        public static string NotInheritsOf(ClassNode node, TypeInfo type)
        {
            return $"({node.Line}, {node.Column}) - Semantic Error:" +
                   $" Is not allowed inherit of the type '{type.Text}'"
                   ;
        }

        public static string RepeatedVariable(IdentifierNode node)
        {
            return $"({node.Line}, {node.Column}) - Semantic Error" +
                    $" The variable '{node.Text}' is already defined in this scope."
                    ;
        }

        public static string CannotConvert(ASTNode node, TypeInfo first, TypeInfo second)
        {
            return $"({node.Line}, {node.Column}) - Semantic Error:" +
                    $"  Cannot convert from '{first.Text}' to '{second.Text}'."
                    ;
        }

        public static string InvalidUseOfOperator(UnaryOperationNode node, TypeInfo operand)
        {
            return $"({node.Line}, {node.Column}) - Semantic Error:" +
                    $" Operator '{node.Symbol}' cannot be applied to operator'{operand.Text}'."
                    ;
        }

        public static string InvalidUseOfOperator(ArithmeticOperationNode node)
        {
            return $"({node.Line}, {node.Column}) - Semantic Error" +
                    $" Operator '{node.Symbol}' must be applied to types 'Int'."
                    ;
        }

        public static string InvalidUseOfOperator(BinaryOperationNode node, TypeInfo leftOperand, TypeInfo rightOperand)
        {
            return $"({node.Line}, {node.Column}) - Semantic Error:" +
                    $" El operador '{node.Symbol}' no se puede aplicar a tipos '{leftOperand.Text}' y '{rightOperand.Text}'."
                    ;
        }

        public static string NotDeclareFunction(DispatchNode node, string name)
        {
            return $"({node.Line}, {node.Column}) - Semantic Error:" +
                    $" The name '{name}' does not exist in the current context."
                    ;
        }

        public static string NotMatchedBranch(CaseNode node)
        {
            return $"({node.Line}, {node.Column}) - Semantic Error:" +
                    $" At least one branch must be matched."
                    ;
        }


    }
}

