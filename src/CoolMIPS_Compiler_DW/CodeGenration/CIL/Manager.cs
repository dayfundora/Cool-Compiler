using System;
using System.Collections.Generic;
using System.Text;
using CoolMIPS_Compiler_DW.Interfaces;

namespace CoolMIPS_Compiler_DW.CodeGenration.CIL
{
    public class VariableManager
    {

        Stack<int> variable_counter_stack;
        public int VariableCounter { set; get; }

        public string CurrentClass { set; get; }

        Dictionary<string, Stack<(int, string)>> VariableLink;

        public VariableManager()
        {
            VariableCounter = 0;
            VariableLink = new Dictionary<string, Stack<(int, string)>>();
            variable_counter_stack = new Stack<int>();
        }

        public void PushVariable(string name, string type)
        {
            if (!VariableLink.ContainsKey(name))
                VariableLink[name] = new Stack<(int, string)>();

            VariableLink[name].Push((VariableCounter, type));
        }

        public void PopVariable(string name)
        {
            if (VariableLink.ContainsKey(name) && VariableLink[name].Count > 0)
                VariableLink[name].Pop();
        }

        public (int, string) GetVariable(string name)
        {
            if (VariableLink.ContainsKey(name) && VariableLink[name].Count > 0)
                return VariableLink[name].Peek();
            else
                return (-1, "");
        }

        public void PushVariableCounter()
        {
            variable_counter_stack.Push(VariableCounter);
        }

        public int PeekVariableCounter()
        {
            return variable_counter_stack.Peek();
        }

        public void PopVariableCounter()
        {
            VariableCounter = variable_counter_stack.Pop();
        }

        public int IncrementVariableCounter()
        {
            ++VariableCounter;
            return VariableCounter;
        }
    }

    public class ClassManager
    {
        IScope Scope;
        Dictionary<string, List<(string, string)>> DefinedClasses;
        Dictionary<(string, string), List<string>> MethodParametersTypes;
        Dictionary<(string, string), string> PropertyType;

        public static List<string> Object = new List<string> { "abort", "type_name", "copy" };
        public static List<string> IO = new List<string> { "out_string", "out_int", "in_string", "in_int" };

        public ClassManager(IScope scope)
        {
            Scope = scope;
            DefinedClasses = new Dictionary<string, List<(string, string)>>();
            MethodParametersTypes = new Dictionary<(string, string), List<string>>();
            PropertyType = new Dictionary<(string, string), string>();

            DefineClass("Object");
            foreach (var f in Object)
                DefineMethod("Object", f, new List<string>());

            DefineClass("IO");
            DefineMethod("IO", "out_string", new List<string>() { "String" });
            DefineMethod("IO", "out_int", new List<string>() { "Int" });
            DefineMethod("IO", "in_string", new List<string>());
            DefineMethod("IO", "in_int", new List<string>());

            DefineClass("String");
            DefineMethod("String", "length", new List<string>());
            DefineMethod("String", "concat", new List<string>() { "String" });
            DefineMethod("String", "substr", new List<string>() { "Int", "Int" });

            DefineClass("Int");
            DefineClass("Bool");
        }

        public void DefineClass(string cclass)
        {
            DefinedClasses[cclass] = new List<(string, string)>();

            if (cclass != "Object")
            {
                string parent = Scope.GetType(cclass).Parent.Text;
                DefinedClasses[parent].ForEach(m => DefinedClasses[cclass].Add(m));
            }
        }

        public void DefineMethod(string cclass, string method, List<string> args_types)
        {
            MethodParametersTypes[(cclass, method)] = args_types;

            string label = cclass + "." + method;
            if (cclass != "Object")
            {
                string parent = Scope.GetType(cclass).Parent.Text;
                int i = DefinedClasses[parent].FindIndex((x) => x.Item2 == method);
                //keep with the same parent address for that method (use in override)
                if (i != -1)
                {
                    DefinedClasses[cclass][i] = (cclass, method);
                    return;
                }
            }

            DefinedClasses[cclass].Add((cclass, method));
        }

        public int GetOffset(string cclass, string item)
        {
            return DefinedClasses[cclass].FindIndex((x) => x.Item2 == item) + 3;
        }

        public (string, string) GetDefinition(string cclass, string item)
        {
            return DefinedClasses[cclass].Find((x) => x.Item2 == item);
        }

        public List<string> GetParametersTypes(string cclass, string method)
        {
            return MethodParametersTypes[GetDefinition(cclass, method)];
        }

        public void DefineAttribute(string cclass, string attr, string type)
        {
            PropertyType[(cclass, attr)] = type;

            if (cclass != "Object")
            {
                string parent = Scope.GetType(cclass).Parent.Text;
                int i = DefinedClasses[parent].FindIndex((x) => x.Item2 == attr);
                //keep with the same parent address
                if (i != -1)
                    return;
            }

            DefinedClasses[cclass].Add((cclass, attr));
        }

        public string GetAttributeType(string cclass, string attr)
        {
            return PropertyType[GetDefinition(cclass, attr)];
        }

        public int GetSizeClass(string cclass)
        {
            return (DefinedClasses[cclass].Count + 3);
        }

    }

}
