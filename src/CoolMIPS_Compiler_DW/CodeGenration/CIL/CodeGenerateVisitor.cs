using System;
using System.Collections.Generic;
using System.Text;
using CoolMIPS_Compiler_DW.Interfaces;
using CoolMIPS_Compiler_DW.AST;

namespace CoolMIPS_Compiler_DW.CodeGenration.CIL
{
    class CodeGeneratorVisitor : IVisitor
    {
        List<CodeLine> Code;
        ClassManager ClassManager;
        IScope Scope;
        VariableManager VariableManager;
        bool object_return_type = false;
        static int return_type_variable = 1;

        public List<CodeLine> GetIntermediateCode(ProgramNode node, IScope scope)
        {
            Scope = scope;



            Code = new List<CodeLine>();
            VariableManager = new VariableManager();
            ClassManager = new ClassManager(scope);

            VariableManager.PushVariableCounter();
            InitCode();
            VariableManager.PopVariableCounter();

            node.Accept(this);

            VariableManager.PushVariableCounter();
            StepOne();
            VariableManager.PopVariableCounter();

            return Code;
        }

        public void Visit(ProgramNode node)
        {
            List<ClassNode> sorted = new List<ClassNode>();
            sorted.AddRange(node.Classes);
            sorted.Sort((x, y) => (Scope.GetType(x.TypeClass.Text) <= Scope.GetType(y.TypeClass.Text) ? 1 : -1));

            foreach (var c in sorted)
            {
                ClassManager.DefineClass(c.TypeClass.Text);

                List<AttributeNode> attributes = new List<AttributeNode>();
                List<MethodNode> methods = new List<MethodNode>();

                foreach (var f in c.FeatureNodes)
                    if (f is AttributeNode)
                    {
                        attributes.Add((AttributeNode)f);
                    }
                    else
                    {
                        methods.Add((MethodNode)f);
                    }

                foreach (var method in methods)
                {
                    List<string> params_type = new List<string>();
                    foreach (var x in method.Arguments)
                        params_type.Add(x.Type.Text);

                    ClassManager.DefineMethod(c.TypeClass.Text, method.Id.Text, params_type);
                }

                foreach (var attr in attributes)
                    ClassManager.DefineAttribute(c.TypeClass.Text, attr.Formal.Id.Text, attr.Formal.Type.Text);
            }

            foreach (var c in sorted)
                c.Accept(this);
        }


        void InitCode()
        {
            int self = VariableManager.PeekVariableCounter();
            (string, string) label;
            List<string> obj = new List<string> { "abort", "type_name", "copy" };

            Code.Add(new CallLabelCodeLine(new LabelCodeLine("start")));

            Code.Add(new LabelCodeLine("Object", "constructor"));
            Code.Add(new ParamCodeLine(self));
            foreach (var f in ClassManager.Object)
            {
                label = ClassManager.GetDefinition("Object", f);
                Code.Add(new CommentCodeLine("set method: " + label.Item1 + "." + label.Item2));
                Code.Add(new AssignmentLabelToMemoryCodeLine(self, new LabelCodeLine(label.Item1, label.Item2), ClassManager.GetOffset("Object", f)));
            }

            Code.Add(new CommentCodeLine("set class name: Object"));
            Code.Add(new AssignmentStringToMemoryCodeLine(0, "Object", 0));
            Code.Add(new CommentCodeLine("set class size: " + ClassManager.GetSizeClass("Object") + " words"));
            Code.Add(new AssignmentConstantToMemoryCodeLine(0, ClassManager.GetSizeClass("Object"), 1));


            Code.Add(new ReturnCodeLine());


            Code.Add(new LabelCodeLine("IO", "constructor"));

            Code.Add(new ParamCodeLine(self));
            Code.Add(new PushParamCodeLine(self));
            Code.Add(new CallLabelCodeLine(new LabelCodeLine("Object", "constructor")));
            Code.Add(new PopParamCodeLine(1));

            foreach (var f in ClassManager.IO)
            {
                label = ClassManager.GetDefinition("IO", f);
                Code.Add(new CommentCodeLine("set method: " + label.Item1 + "." + label.Item2));
                Code.Add(new AssignmentLabelToMemoryCodeLine(self, new LabelCodeLine(label.Item1, label.Item2), ClassManager.GetOffset("IO", f)));
            }

            Code.Add(new CommentCodeLine("set class name: Object"));
            Code.Add(new AssignmentStringToMemoryCodeLine(0, "IO", 0));
            Code.Add(new CommentCodeLine("set class size: " + ClassManager.GetSizeClass("IO") + " words"));
            Code.Add(new AssignmentConstantToMemoryCodeLine(0, ClassManager.GetSizeClass("IO"), 1));
            Code.Add(new CommentCodeLine("set class generation label"));
            Code.Add(new AssignmentLabelToMemoryCodeLine(0, new LabelCodeLine("_class", "IO"), 2));

            Code.Add(new ReturnCodeLine());


            Code.Add(new InheritCodeLine("IO", "Object"));
            Code.Add(new InheritCodeLine("Int", "Object"));
            Code.Add(new InheritCodeLine("Bool", "Object"));
            Code.Add(new InheritCodeLine("String", "Object"));

            //Int wrapper for runtime check typing
            Code.Add(new LabelCodeLine("_wrapper", "Int"));
            Code.Add(new ParamCodeLine(self));
            Code.Add(new AllocateCodeLine(self + 1, ClassManager.GetSizeClass("Int") + 1));
            Code.Add(new PushParamCodeLine(self + 1));
            Code.Add(new CallLabelCodeLine(new LabelCodeLine("Object", "constructor")));
            Code.Add(new PopParamCodeLine(1));
            Code.Add(new AssignmentStringToMemoryCodeLine(self + 1, "Int", 0));
            Code.Add(new AssignmentVariableToMemoryCodeLine(self + 1, self, ClassManager.GetSizeClass("Int")));
            Code.Add(new AssignmentLabelToMemoryCodeLine(self + 1, new LabelCodeLine("_class", "Int"), 2));
            Code.Add(new ReturnCodeLine(self + 1));

            //Bool wrapper for runtime check typing
            Code.Add(new LabelCodeLine("_wrapper", "Bool"));
            Code.Add(new ParamCodeLine(self));
            Code.Add(new AllocateCodeLine(self + 1, ClassManager.GetSizeClass("Bool") + 1));
            Code.Add(new PushParamCodeLine(self + 1));
            Code.Add(new CallLabelCodeLine(new LabelCodeLine("Object", "constructor")));
            Code.Add(new PopParamCodeLine(1));
            Code.Add(new AssignmentStringToMemoryCodeLine(self + 1, "Bool", 0));
            Code.Add(new AssignmentVariableToMemoryCodeLine(self + 1, self, ClassManager.GetSizeClass("Bool")));
            Code.Add(new AssignmentLabelToMemoryCodeLine(self + 1, new LabelCodeLine("_class", "Bool"), 2));
            Code.Add(new ReturnCodeLine(self + 1));

            //String wrapper for runtime check typing
            Code.Add(new LabelCodeLine("_wrapper", "String"));
            Code.Add(new ParamCodeLine(self));
            Code.Add(new AllocateCodeLine(self + 1, ClassManager.GetSizeClass("String") + 1));
            Code.Add(new PushParamCodeLine(self + 1));
            Code.Add(new CallLabelCodeLine(new LabelCodeLine("Object", "constructor")));
            Code.Add(new PopParamCodeLine(1));
            Code.Add(new AssignmentStringToMemoryCodeLine(self + 1, "String", 0));
            Code.Add(new AssignmentVariableToMemoryCodeLine(self + 1, self, ClassManager.GetSizeClass("String")));
            Code.Add(new AssignmentLabelToMemoryCodeLine(self + 1, new LabelCodeLine("_class", "String"), 2));
            Code.Add(new ReturnCodeLine(self + 1));


            //abort, typename, copy
            Code.Add(new LabelCodeLine("Object", "abort"));
            Code.Add(new GotoJumpCodeLine(new LabelCodeLine("_abort")));

            Code.Add(new LabelCodeLine("Object", "type_name"));
            Code.Add(new ParamCodeLine(0));
            Code.Add(new AssignmentMemoryToVariableCodeLine(0, 0, 0));
            Code.Add(new ReturnCodeLine(0));


            Code.Add(new LabelCodeLine("Object", "copy"));
            Code.Add(new ParamCodeLine(0));
            Code.Add(new AssignmentMemoryToVariableCodeLine(1, 0, 1));
            Code.Add(new AssignmentConstantToVariableCodeLine(2, 4));
            Code.Add(new BinaryOperationCodeLine(1, 1, 2, "*"));
            Code.Add(new PushParamCodeLine(0));
            Code.Add(new PushParamCodeLine(1));
            Code.Add(new CallLabelCodeLine(new LabelCodeLine("_copy"), 0));
            Code.Add(new PopParamCodeLine(2));

            Code.Add(new ReturnCodeLine(0));


            //io: in_string, out_string, in_int, out_int
            Code.Add(new LabelCodeLine("IO", "out_string"));
            Code.Add(new ParamCodeLine(0));
            Code.Add(new ParamCodeLine(1));
            Code.Add(new PushParamCodeLine(1));
            Code.Add(new CallLabelCodeLine(new LabelCodeLine("_out_string"), 0));
            Code.Add(new PopParamCodeLine(1));
            Code.Add(new ReturnCodeLine(0));

            Code.Add(new LabelCodeLine("IO", "out_int"));
            Code.Add(new ParamCodeLine(0));
            Code.Add(new ParamCodeLine(1));
            Code.Add(new PushParamCodeLine(1));
            Code.Add(new CallLabelCodeLine(new LabelCodeLine("_out_int"), 0));
            Code.Add(new PopParamCodeLine(1));
            Code.Add(new ReturnCodeLine(0));


            Code.Add(new LabelCodeLine("IO", "in_string"));
            Code.Add(new ParamCodeLine(0));
            Code.Add(new CallLabelCodeLine(new LabelCodeLine("_in_string"), 0));
            Code.Add(new ReturnCodeLine(0));


            Code.Add(new LabelCodeLine("IO", "in_int"));
            Code.Add(new ParamCodeLine(0));
            Code.Add(new CallLabelCodeLine(new LabelCodeLine("_in_int"), 0));
            Code.Add(new ReturnCodeLine(0));

            //string: substr, concat, length
            Code.Add(new LabelCodeLine("String", "length"));
            Code.Add(new ParamCodeLine(0));
            Code.Add(new PushParamCodeLine(0));
            Code.Add(new CallLabelCodeLine(new LabelCodeLine("_stringlength"), 0));
            Code.Add(new PopParamCodeLine(1));
            Code.Add(new ReturnCodeLine(0));


            Code.Add(new LabelCodeLine("String", "concat"));
            Code.Add(new ParamCodeLine(0));
            Code.Add(new ParamCodeLine(1));
            Code.Add(new PushParamCodeLine(0));
            Code.Add(new PushParamCodeLine(1));
            Code.Add(new CallLabelCodeLine(new LabelCodeLine("_stringconcat"), 0));
            Code.Add(new PopParamCodeLine(2));
            Code.Add(new ReturnCodeLine(0));


            Code.Add(new LabelCodeLine("String", "substr"));
            Code.Add(new ParamCodeLine(0));
            Code.Add(new ParamCodeLine(1));
            Code.Add(new ParamCodeLine(2));
            Code.Add(new PushParamCodeLine(0));
            Code.Add(new PushParamCodeLine(1));
            Code.Add(new PushParamCodeLine(2));
            Code.Add(new CallLabelCodeLine(new LabelCodeLine("_stringsubstr"), 0));
            Code.Add(new PopParamCodeLine(3));
            Code.Add(new ReturnCodeLine(0));
        }

        void StepOne()
        {
            Code.Add(new LabelCodeLine("start"));
            New("Main");
            Code.Add(new PushParamCodeLine(VariableManager.PeekVariableCounter()));
            Code.Add(new CallLabelCodeLine(new LabelCodeLine("Main", "main")));
            Code.Add(new PopParamCodeLine(1));

        }

        public void Visit(ClassNode node)
        {
            string cclass;
            cclass = VariableManager.CurrentClass = node.TypeClass.Text;
            Code.Add(new InheritCodeLine(node.TypeClass.Text, Scope.GetType(node.TypeClass.Text).Parent.Text));

            //VirtualTable.DefineClass(VariableManager.CurrentClass);
            int self = VariableManager.VariableCounter = 0;
            VariableManager.IncrementVariableCounter();
            VariableManager.PushVariableCounter();

            List<AttributeNode> attributes = new List<AttributeNode>();
            List<MethodNode> methods = new List<MethodNode>();

            foreach (var f in node.FeatureNodes)
                if (f is AttributeNode)
                    attributes.Add((AttributeNode)f);
                else
                    methods.Add((MethodNode)f);


            foreach (var method in methods)
            {
                method.Accept(this);
            }


            //begin constructor function

            Code.Add(new LabelCodeLine(VariableManager.CurrentClass, "constructor"));
            Code.Add(new ParamCodeLine(self));

            //calling first the parent constructor method
            if (VariableManager.CurrentClass != "Object")
            {
                Code.Add(new PushParamCodeLine(self));
                LabelCodeLine label = new LabelCodeLine(node.TypeInherit.Text, "constructor");
                Code.Add(new CallLabelCodeLine(label));
                Code.Add(new PopParamCodeLine(1));
            }


            foreach (var method in methods)
            {
                (string, string) label = ClassManager.GetDefinition(node.TypeClass.Text, method.Id.Text);
                Code.Add(new CommentCodeLine("set method: " + label.Item1 + "." + label.Item2));
                Code.Add(new AssignmentLabelToMemoryCodeLine(self, new LabelCodeLine(label.Item1, label.Item2), ClassManager.GetOffset(node.TypeClass.Text, method.Id.Text)));

            }


            foreach (var attr in attributes)
            {
                VariableManager.PushVariableCounter();
                attr.Accept(this);
                VariableManager.PopVariableCounter();
                Code.Add(new CommentCodeLine("set attribute: " + attr.Formal.Id.Text));
                Code.Add(new AssignmentVariableToMemoryCodeLine(self, VariableManager.PeekVariableCounter(), ClassManager.GetOffset(node.TypeClass.Text, attr.Formal.Id.Text)));
            }


            Code.Add(new CommentCodeLine("set class name: " + node.TypeClass.Text));
            Code.Add(new AssignmentStringToMemoryCodeLine(0, node.TypeClass.Text, 0));
            Code.Add(new CommentCodeLine("set class size: " + ClassManager.GetSizeClass(node.TypeClass.Text) + " words"));
            Code.Add(new AssignmentConstantToMemoryCodeLine(0, ClassManager.GetSizeClass(node.TypeClass.Text), 1));
            Code.Add(new CommentCodeLine("set class generation label"));
            Code.Add(new AssignmentLabelToMemoryCodeLine(0, new LabelCodeLine("_class", node.TypeClass.Text), 2));

            Code.Add(new ReturnCodeLine(-1));

            VariableManager.PopVariableCounter();
        }

        public void Visit(AttributeNode node)
        {
            node.AssignExp.Accept(this);

            if ((node.AssignExp.StaticType.Text == "Int" ||
                node.AssignExp.StaticType.Text == "Bool" ||
                node.AssignExp.StaticType.Text == "String") &&
                node.Formal.Type.Text == "Object")
            {
                Code.Add(new PushParamCodeLine(VariableManager.PeekVariableCounter()));
                Code.Add(new CallLabelCodeLine(new LabelCodeLine("_wrapper", node.AssignExp.StaticType.Text), VariableManager.PeekVariableCounter()));
                Code.Add(new PopParamCodeLine(1));
            }
        }

        public void Visit(MethodNode node)
        {
            Code.Add(new LabelCodeLine(VariableManager.CurrentClass, node.Id.Text));

            object_return_type = node.TypeReturn.Text == "Object";

            int self = VariableManager.VariableCounter = 0;
            Code.Add(new ParamCodeLine(self));

            //if return type is object, annotation type is needed
            if (object_return_type)
                VariableManager.IncrementVariableCounter();

            VariableManager.IncrementVariableCounter();

            foreach (var formal in node.Arguments)
            {
                Code.Add(new ParamCodeLine(VariableManager.VariableCounter));
                VariableManager.PushVariable(formal.Id.Text, formal.Type.Text);
                VariableManager.IncrementVariableCounter();
            }

            VariableManager.PushVariableCounter();
            node.Body.Accept(this);

            if (object_return_type)
                ReturnObjectWrapping();

            Code.Add(new ReturnCodeLine(VariableManager.PeekVariableCounter()));


            VariableManager.PopVariableCounter();

            foreach (var formal in node.Arguments)
            {
                VariableManager.PopVariable(formal.Id.Text);
            }

            object_return_type = false;
        }

        void ReturnObjectWrapping()
        {
            int t;
            int result = VariableManager.PeekVariableCounter();
            string tag = Code.Count.ToString();

            VariableManager.PushVariableCounter();
            VariableManager.IncrementVariableCounter();
            t = VariableManager.VariableCounter;
            Code.Add(new AssignmentStringToVariableCodeLine(t, "Int"));
            Code.Add(new BinaryOperationCodeLine(t, return_type_variable, t, "="));
            Code.Add(new ConditionalJumpCodeLine(t, new LabelCodeLine("_attempt_bool", tag)));
            Code.Add(new PushParamCodeLine(result));
            Code.Add(new CallLabelCodeLine(new LabelCodeLine("_wrapper", "Int"), result));
            Code.Add(new PopParamCodeLine(1));
            Code.Add(new GotoJumpCodeLine(new LabelCodeLine("_not_more_attempt", tag)));
            VariableManager.PopVariableCounter();

            Code.Add(new LabelCodeLine("_attempt_bool", tag));
            VariableManager.PushVariableCounter();
            VariableManager.IncrementVariableCounter();
            t = VariableManager.VariableCounter;
            Code.Add(new AssignmentStringToVariableCodeLine(t, "Bool"));
            Code.Add(new BinaryOperationCodeLine(t, return_type_variable, t, "="));
            Code.Add(new ConditionalJumpCodeLine(t, new LabelCodeLine("_attempt_string", tag)));
            Code.Add(new PushParamCodeLine(result));
            Code.Add(new CallLabelCodeLine(new LabelCodeLine("_wrapper", "Bool"), result));
            Code.Add(new PopParamCodeLine(1));
            Code.Add(new GotoJumpCodeLine(new LabelCodeLine("_not_more_attempt", tag)));
            VariableManager.PopVariableCounter();

            Code.Add(new LabelCodeLine("_attempt_string", tag));
            VariableManager.PushVariableCounter();
            VariableManager.IncrementVariableCounter();
            t = VariableManager.VariableCounter;
            Code.Add(new AssignmentStringToVariableCodeLine(t, "String"));
            Code.Add(new BinaryOperationCodeLine(t, return_type_variable, t, "="));
            Code.Add(new ConditionalJumpCodeLine(t, new LabelCodeLine("_not_more_attempt", tag)));
            Code.Add(new PushParamCodeLine(result));
            Code.Add(new CallLabelCodeLine(new LabelCodeLine("_wrapper", "String"), result));
            Code.Add(new PopParamCodeLine(1));
            VariableManager.PopVariableCounter();

            Code.Add(new LabelCodeLine("_not_more_attempt", tag));
        }


        public void Visit(CaseNode node)
        {
            string static_type = node.ExpressionCase.StaticType.Text;

            int result = VariableManager.PeekVariableCounter();
            int expr = VariableManager.IncrementVariableCounter();

            VariableManager.PushVariableCounter();
            node.ExpressionCase.Accept(this);
            VariableManager.PopVariableCounter();


            if (static_type == "String" ||
                static_type == "Int" ||
                static_type == "Bool")
            {
                int index = node.Branches.FindIndex((x) => x.Formal.Type.Text == static_type);
                string v = node.Branches[index].Formal.Id.Text;

                VariableManager.PushVariable(v, node.Branches[index].Formal.Type.Text);

                int t = VariableManager.IncrementVariableCounter();
                VariableManager.PushVariableCounter();

                node.Branches[index].Expression.Accept(this);

                VariableManager.PopVariableCounter();

                VariableManager.PopVariable(v);

                Code.Add(new AssignmentVariableToVariableCodeLine(VariableManager.PeekVariableCounter(), t));
            }
            else
            {
                string tag = Code.Count.ToString();

                List<(FormalNode Formal, ExpressionNode Expression)> sorted = new List<(FormalNode Formal, ExpressionNode Expression)>();
                sorted.AddRange(node.Branches);
                sorted.Sort((x, y) => (Scope.GetType(x.Formal.Type.Text) <= Scope.GetType(y.Formal.Type.Text) ? -1 : 1));

                for (int i = 0; i < sorted.Count; ++i)
                {
                    //same that expr integer
                    VariableManager.PushVariable(sorted[i].Formal.Id.Text, sorted[i].Formal.Type.Text);

                    string branch_type = sorted[i].Formal.Type.Text;
                    VariableManager.PushVariableCounter();
                    VariableManager.IncrementVariableCounter();

                    Code.Add(new LabelCodeLine("_case", tag + "." + i));
                    Code.Add(new AssignmentStringToVariableCodeLine(VariableManager.VariableCounter, branch_type));
                    Code.Add(new BinaryOperationCodeLine(VariableManager.VariableCounter, expr, VariableManager.VariableCounter, "inherit"));
                    Code.Add(new ConditionalJumpCodeLine(VariableManager.VariableCounter, new LabelCodeLine("_case", tag + "." + (i + 1))));


                    if ((branch_type == "Int" ||
                        branch_type == "Bool" ||
                        branch_type == "String"))
                    {
                        if (static_type == "Object")
                        {

                            Code.Add(new AssignmentMemoryToVariableCodeLine(expr, expr, ClassManager.GetSizeClass(branch_type)));

                            VariableManager.PushVariableCounter();
                            sorted[i].Expression.Accept(this);
                            VariableManager.PopVariableCounter();

                            Code.Add(new AssignmentVariableToVariableCodeLine(result, VariableManager.PeekVariableCounter()));
                            Code.Add(new GotoJumpCodeLine(new LabelCodeLine("_endcase", tag)));
                        }
                    }
                    else
                    {
                        VariableManager.PushVariableCounter();
                        sorted[i].Expression.Accept(this);
                        VariableManager.PopVariableCounter();

                        Code.Add(new AssignmentVariableToVariableCodeLine(result, VariableManager.PeekVariableCounter()));
                        Code.Add(new GotoJumpCodeLine(new LabelCodeLine("_endcase", tag)));
                    }



                    VariableManager.PopVariableCounter();

                    VariableManager.PopVariable(sorted[i].Formal.Id.Text);
                }

                Code.Add(new LabelCodeLine("_case", tag + "." + sorted.Count));
                Code.Add(new GotoJumpCodeLine(new LabelCodeLine("_caseselectionexception")));

                Code.Add(new LabelCodeLine("_endcase", tag));
            }
        }

        public void Visit(IntNode node)
        {
            Code.Add(new AssignmentConstantToVariableCodeLine(VariableManager.PeekVariableCounter(), node.Value));
            if (object_return_type)
                SetReturnType("Int");
        }

        public void Visit(BoolNode node)
        {
            Code.Add(new AssignmentConstantToVariableCodeLine(VariableManager.PeekVariableCounter(), node.Value ? 1 : 0));
            if (object_return_type)
                SetReturnType("Bool");
        }

        public void Visit(ArithmeticOperationNode node)
        {
            if (node.Attributes.ContainsKey("integer_constant_value"))
                Code.Add(new AssignmentConstantToVariableCodeLine(VariableManager.PeekVariableCounter(), node.Attributes["integer_constant_value"]));
            else
                BinaryOperationVisit(node);
            if (object_return_type)
                SetReturnType("Int");
        }

        public void Visit(AssignmentNode node)
        {

            node.ExpressionRight.Accept(this);
            var (t, type) = VariableManager.GetVariable(node.ID.Text);

            if (type == "")
                type = ClassManager.GetAttributeType(VariableManager.CurrentClass, node.ID.Text);


            if ((node.ExpressionRight.StaticType.Text == "Int" ||
                node.ExpressionRight.StaticType.Text == "Bool" ||
                node.ExpressionRight.StaticType.Text == "String") &&
                type == "Object")
            {
                Code.Add(new PushParamCodeLine(VariableManager.PeekVariableCounter()));
                Code.Add(new CallLabelCodeLine(new LabelCodeLine("_wrapper", node.ExpressionRight.StaticType.Text), VariableManager.PeekVariableCounter()));
                Code.Add(new PopParamCodeLine(1));
            }

            if (t != -1)
            {

                Code.Add(new AssignmentVariableToVariableCodeLine(t, VariableManager.PeekVariableCounter()));
            }
            else
            {
                int offset = ClassManager.GetOffset(VariableManager.CurrentClass, node.ID.Text);
                Code.Add(new AssignmentVariableToMemoryCodeLine(0, VariableManager.PeekVariableCounter(), offset));
            }


        }

        public void Visit(SequenceNode node)
        {
            foreach (var s in node.Sequence)
            {
                s.Accept(this);
            }
        }

        public void Visit(IdentifierNode node)
        {
            var (t, type) = VariableManager.GetVariable(node.Text);
            if (t != -1)
            {
                Code.Add(new CommentCodeLine("get veriable: " + node.Text));
                Code.Add(new AssignmentVariableToVariableCodeLine(VariableManager.PeekVariableCounter(), t));
            }
            else
            {
                Code.Add(new CommentCodeLine("get attribute: " + VariableManager.CurrentClass + "." + node.Text));
                Code.Add(new AssignmentMemoryToVariableCodeLine(VariableManager.PeekVariableCounter(), 0, ClassManager.GetOffset(VariableManager.CurrentClass, node.Text)));
            }

            if (object_return_type)
                SetReturnType(type);
        }


        public void Visit(ComparisonOperation node)
        {
            BinaryOperationVisit(node);
            if (object_return_type)
            {
                Code.Add(new CommentCodeLine($"set bool as return type"));
                Code.Add(new AssignmentStringToVariableCodeLine(return_type_variable, "Bool"));
            }
        }

        public void Visit(DispatchExplicitNode node)
        {
            string cclass = node.IdType.Text;
            node.Expression.Accept(this);
            DispatchVisit(node, cclass);
        }

        public void Visit(DispatchImplicitNode node)
        {
            string cclass = VariableManager.CurrentClass;
            Code.Add(new AssignmentVariableToVariableCodeLine(VariableManager.PeekVariableCounter(), 0));
            DispatchVisit(node, cclass);
        }

        void DispatchVisit(DispatchNode node, string cclass)
        {
            string method = node.IdMethod.Text;

            if (method == "abort" && (cclass == "Int" || cclass == "String" || cclass == "Bool"))
            {
                Code.Add(new CallLabelCodeLine(new LabelCodeLine("Object", "abort")));
                return;
            }

            if (method == "type_name")
            {
                if (cclass == "Int" || cclass == "Bool" || cclass == "String")
                {
                    Code.Add(new AssignmentStringToVariableCodeLine(VariableManager.PeekVariableCounter(), cclass));
                    return;
                }
            }

            //important for define
            if (method == "copy")
            {
                if (cclass == "Int" || cclass == "Bool" || cclass == "String")
                {
                    Code.Add(new PushParamCodeLine(VariableManager.PeekVariableCounter()));
                    Code.Add(new CallLabelCodeLine(new LabelCodeLine("_wrapper", cclass), VariableManager.PeekVariableCounter()));
                    Code.Add(new PopParamCodeLine(1));
                    return;
                }
            }

            VariableManager.PushVariableCounter();


            int function_address = VariableManager.IncrementVariableCounter();

            int offset = ClassManager.GetOffset(cclass, method);

            List<int> parameters = new List<int>();
            List<string> parameters_types = ClassManager.GetParametersTypes(cclass, method);
            for (int i = 0; i < node.Arguments.Count; ++i)
            {
                VariableManager.IncrementVariableCounter();
                VariableManager.PushVariableCounter();
                parameters.Add(VariableManager.VariableCounter);
                node.Arguments[i].Accept(this);

                if (parameters_types[i] == "Object" && (
                    node.Arguments[i].StaticType.Text == "Int" ||
                    node.Arguments[i].StaticType.Text == "Bool" ||
                    node.Arguments[i].StaticType.Text == "String"))
                {
                    Code.Add(new PushParamCodeLine(VariableManager.PeekVariableCounter()));
                    Code.Add(new CallLabelCodeLine(new LabelCodeLine("_wrapper", node.Arguments[i].StaticType.Text), VariableManager.PeekVariableCounter()));
                    Code.Add(new PopParamCodeLine(1));
                }

                VariableManager.PopVariableCounter();
            }

            VariableManager.PopVariableCounter();

            if (cclass != "String")
            {
                Code.Add(new CommentCodeLine("get method: " + cclass + "." + method));
                Code.Add(new AssignmentMemoryToVariableCodeLine(function_address, VariableManager.PeekVariableCounter(), offset));
            }

            Code.Add(new PushParamCodeLine(VariableManager.PeekVariableCounter()));

            foreach (var p in parameters)
            {
                Code.Add(new PushParamCodeLine(p));
            }

            if (cclass != "String")
            {
                Code.Add(new CallAddressCodeLine(function_address, VariableManager.PeekVariableCounter()));
            }
            else
            {
                Code.Add(new CallLabelCodeLine(new LabelCodeLine(cclass, method), VariableManager.PeekVariableCounter()));
            }

            if (object_return_type)
                SetReturnType(node.StaticType.Text);

            Code.Add(new PopParamCodeLine(parameters.Count + 1));
        }

        void SetReturnType(string type)
        {
            if (type == "Int" ||
                type == "Bool" ||
                type == "String")
            {
                Code.Add(new CommentCodeLine($"set {type} as return type"));
                Code.Add(new AssignmentStringToVariableCodeLine(return_type_variable, type));
            }
            else
            {
                Code.Add(new CommentCodeLine($"set object as return type"));
                Code.Add(new AssignmentStringToVariableCodeLine(return_type_variable, "Object"));
            }
        }


        public void Visit(EqualNode node)
        {
            BinaryOperationVisit(node);
            if (object_return_type)
                SetReturnType("Bool");
        }

        void BinaryOperationVisit(BinaryOperationNode node)
        {
            VariableManager.PushVariableCounter();

            int t1 = VariableManager.IncrementVariableCounter();
            VariableManager.PushVariableCounter();
            node.LeftOperand.Accept(this);
            VariableManager.PopVariableCounter();

            int t2 = VariableManager.IncrementVariableCounter();
            VariableManager.PushVariableCounter();
            node.RightOperand.Accept(this);
            VariableManager.PopVariableCounter();

            VariableManager.PopVariableCounter();

            if (node.LeftOperand.StaticType.Text == "String" && node.Symbol == "=")
            {
                Code.Add(new BinaryOperationCodeLine(VariableManager.PeekVariableCounter(), t1, t2, "=:="));
                return;
            }

            Code.Add(new BinaryOperationCodeLine(VariableManager.PeekVariableCounter(), t1, t2, node.Symbol));
        }

        public void Visit(StringNode node)
        {
            Code.Add(new AssignmentStringToVariableCodeLine(VariableManager.PeekVariableCounter(), node.Text));
            if (object_return_type)
                SetReturnType("String");
        }

        public void Visit(LetNode node)
        {
            VariableManager.PushVariableCounter();

            foreach (var attr in node.Initialization)
            {
                VariableManager.IncrementVariableCounter();
                VariableManager.PushVariable(attr.Formal.Id.Text, attr.Formal.Type.Text);
                VariableManager.PushVariableCounter();
                attr.Accept(this);

                VariableManager.PopVariableCounter();
            }
            VariableManager.IncrementVariableCounter();

            node.ExpressionBody.Accept(this);

            foreach (var attr in node.Initialization)
            {
                VariableManager.PopVariable(attr.Formal.Id.Text);
            }
            VariableManager.PopVariableCounter();

            if (object_return_type)
                SetReturnType(node.StaticType.Text);
        }

        public void Visit(NewNode node)
        {
            if (node.TypeId.Text == "Int" ||
                node.TypeId.Text == "Bool" ||
                node.TypeId.Text == "String")
            {
                if (node.TypeId.Text == "Int" || node.TypeId.Text == "Bool")
                    Code.Add(new AssignmentConstantToVariableCodeLine(VariableManager.PeekVariableCounter(), 0));
                else
                    Code.Add(new AssignmentStringToVariableCodeLine(VariableManager.PeekVariableCounter(), ""));
            }
            else
            {
                New(node.TypeId.Text);
            }

            if (object_return_type)
                SetReturnType(node.TypeId.Text);
        }

        public void New(string cclass)
        {
            int size = ClassManager.GetSizeClass(cclass);
            Code.Add(new AllocateCodeLine(VariableManager.PeekVariableCounter(), size));
            Code.Add(new PushParamCodeLine(VariableManager.PeekVariableCounter()));
            Code.Add(new CallLabelCodeLine(new LabelCodeLine(cclass, "constructor")));
            Code.Add(new PopParamCodeLine(1));
        }

        public void Visit(IsVoidNode node)
        {
            //if special types non void;
            if (node.Operand.StaticType.Text == "Int" ||
               node.Operand.StaticType.Text == "String" ||
               node.Operand.StaticType.Text == "Bool")
                Code.Add(new AssignmentConstantToVariableCodeLine(VariableManager.PeekVariableCounter(), 0));
            else
                UnaryOperationVisit(node);

            if (object_return_type)
                SetReturnType("Bool");
        }

        public void Visit(NegNode node)
        {
            UnaryOperationVisit(node);
            if (object_return_type)
                SetReturnType("Int");
        }


        public void Visit(NotNode node)
        {
            UnaryOperationVisit(node);
            if (object_return_type)
                SetReturnType("Bool");
        }

        void UnaryOperationVisit(UnaryOperationNode node)
        {
            VariableManager.PushVariableCounter();

            VariableManager.IncrementVariableCounter();
            int t1 = VariableManager.VariableCounter;
            VariableManager.PushVariableCounter();
            node.Operand.Accept(this);

            VariableManager.PopVariableCounter();

            Code.Add(new UnaryOperationCodeLine(VariableManager.PeekVariableCounter(), t1, node.Symbol));
        }

        public void Visit(IfNode node)
        {
            string tag = Code.Count.ToString();

            node.Condition.Accept(this);

            Code.Add(new ConditionalJumpCodeLine(VariableManager.PeekVariableCounter(), new LabelCodeLine("_else", tag)));

            node.Body.Accept(this);
            Code.Add(new GotoJumpCodeLine(new LabelCodeLine("_endif", tag)));

            Code.Add(new LabelCodeLine("_else", tag));
            node.ElseBody.Accept(this);

            Code.Add(new LabelCodeLine("_endif", tag));

        }


        public void Visit(WhileNode node)
        {
            string tag = Code.Count.ToString();

            Code.Add(new LabelCodeLine("_whilecondition", tag));

            node.Condition.Accept(this);

            Code.Add(new ConditionalJumpCodeLine(VariableManager.PeekVariableCounter(), new LabelCodeLine("_endwhile", tag)));

            node.Body.Accept(this);

            Code.Add(new GotoJumpCodeLine(new LabelCodeLine("_whilecondition", tag)));

            Code.Add(new LabelCodeLine("_endwhile", tag));
        }


        public void Visit(VoidNode node)
        {
            Code.Add(new AssignmentNullToVariableCodeLine(VariableManager.PeekVariableCounter()));
        }

        public void Visit(SelfNode node)
        {
            Code.Add(new AssignmentVariableToVariableCodeLine(VariableManager.PeekVariableCounter(), 0));
        }
        public void Visit(FormalNode node)
        {
            throw new NotImplementedException();
        }

    }
}
