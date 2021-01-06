using System;
using System.Collections.Generic;
using System.Text;
using CoolMIPS_Compiler_DW.Interfaces;
using CoolMIPS_Compiler_DW.AST;

namespace CoolMIPS_Compiler_DW.Semantic
{
    public class Scope : IScope
    {
        
        Dictionary<string, TypeInfo> variables = new Dictionary<string, TypeInfo>();

        Dictionary<string, (TypeInfo[] Args, TypeInfo ReturnType)> functions = new Dictionary<string, (TypeInfo[], TypeInfo)>();

        static Dictionary<string, TypeInfo> declaredTypes = new Dictionary<string, TypeInfo>();

        public IScope Parent { get; set; }
        public TypeInfo Type { get; set; }

        public Scope()
        {

        }

        public Scope(IScope parent, TypeInfo type)
        {
            Parent = parent;
            Type = type;
        }

        static Scope()
        {
            declaredTypes.Add("Object", TypeInfo.OBJECT);
            declaredTypes["Object"].ClassReference = new ClassNode(-1, -1, "Object", "NULL");
            declaredTypes["Object"].ClassReference.Scope = new Scope(NULL, declaredTypes["Object"]);

            declaredTypes.Add("Bool", new TypeInfo { Text = "Bool", Parent = declaredTypes["Object"], Level = 1, ClassReference = new ClassNode(-1, -1, "Bool", "Object") });
            declaredTypes.Add("Int", new TypeInfo { Text = "Int", Parent = declaredTypes["Object"], Level = 1, ClassReference = new ClassNode(-1, -1, "Int", "Object") });
            declaredTypes.Add("String", new TypeInfo { Text = "String", Parent = declaredTypes["Object"], Level = 1, ClassReference = new ClassNode(-1, -1, "String", "Object") });
            declaredTypes.Add("IO", new TypeInfo { Text = "IO", Parent = declaredTypes["Object"], Level = 1, ClassReference = new ClassNode(-1, -1, "IO", "Object") });

            declaredTypes["Bool"].ClassReference.Scope = new Scope(declaredTypes["Object"].ClassReference.Scope, declaredTypes["Bool"]);
            declaredTypes["Int"].ClassReference.Scope = new Scope(declaredTypes["Object"].ClassReference.Scope, declaredTypes["Int"]);
            declaredTypes["String"].ClassReference.Scope = new Scope(declaredTypes["Object"].ClassReference.Scope, declaredTypes["String"]);
            declaredTypes["IO"].ClassReference.Scope = new Scope(declaredTypes["Object"].ClassReference.Scope, declaredTypes["IO"]);

            declaredTypes["Object"].ClassReference.Scope.Define("abort", new TypeInfo[0], declaredTypes["Object"]);
            declaredTypes["Object"].ClassReference.Scope.Define("type_name", new TypeInfo[0], declaredTypes["String"]);
            declaredTypes["Object"].ClassReference.Scope.Define("copy", new TypeInfo[0], declaredTypes["Object"]);

            declaredTypes["String"].ClassReference.Scope.Define("length", new TypeInfo[0], declaredTypes["Int"]);
            declaredTypes["String"].ClassReference.Scope.Define("concat", new TypeInfo[1] { declaredTypes["String"] }, declaredTypes["String"]);
            declaredTypes["String"].ClassReference.Scope.Define("substr", new TypeInfo[2] { declaredTypes["Int"], declaredTypes["Int"] }, declaredTypes["String"]);

            declaredTypes["IO"].ClassReference.Scope.Define("out_string", new TypeInfo[1] { declaredTypes["String"] }, declaredTypes["IO"]);
            declaredTypes["IO"].ClassReference.Scope.Define("out_int", new TypeInfo[1] { declaredTypes["Int"] }, declaredTypes["IO"]);
            declaredTypes["IO"].ClassReference.Scope.Define("in_string", new TypeInfo[0], declaredTypes["String"]);
            declaredTypes["IO"].ClassReference.Scope.Define("in_int", new TypeInfo[0], declaredTypes["Int"]);

        }

        public static void Clear()
        {
            var tmp = new Dictionary<string, TypeInfo>();
            HashSet<string> builtin = new HashSet<string> { "Object", "Bool", "Int", "String", "IO" };
            foreach (var item in declaredTypes)
                if (builtin.Contains(item.Key))
                    tmp.Add(item.Key, item.Value);
            declaredTypes = tmp;
        }

        public bool IsDefined(string name, out TypeInfo type)
        {
            if (variables.TryGetValue(name, out type) || Parent.IsDefined(name, out type))
                return true;

            type = TypeInfo.OBJECT;
            return false;
        }

        public bool IsDefined(string name, TypeInfo[] args, out TypeInfo type)
        {
            type = TypeInfo.OBJECT;
            if (functions.ContainsKey(name) && functions[name].Args.Length == args.Length)
            {
                bool ok = true;
                for (int i = 0; i < args.Length; ++i)
                    if (!(args[i] <= functions[name].Args[i]))
                        ok = false;
                if (ok)
                {
                    type = functions[name].ReturnType;
                    return true;
                }
            }

            if (Parent.IsDefined(name, args, out type))
                return true;

            type = TypeInfo.OBJECT;
            return false;
        }

        public bool IsDefinedType(string name, out TypeInfo type)
        {
            if (declaredTypes.TryGetValue(name, out type))
                return true;
            type = TypeInfo.OBJECT;
            return false;
        }

        public bool Define(string name, TypeInfo type)
        {
            if (variables.ContainsKey(name))
                return false;
            variables.Add(name, type);
            return true;
        }

        public bool Define(string name, TypeInfo[] args, TypeInfo type)
        {
            if (functions.ContainsKey(name))
                return false;
            functions[name] = (args, type);
            return true;
        }

        public bool UpdateType(string name, TypeInfo type)
        {
            if (!variables.ContainsKey(name))
                variables.Add(name, type);
            variables[name] = type;
            return true;
        }

        public IScope CreateChild()
        {
            return new Scope()
            {
                Parent = this,
                Type = this.Type
            };
        }

        public bool AddType(string name, TypeInfo type)
        {
            declaredTypes.Add(name, type);
            return true;
        }

        public TypeInfo GetType(string name)
        {
            if (declaredTypes.TryGetValue(name, out TypeInfo type))
                return type;
            return TypeInfo.OBJECT;
        }

        #region
        private static NullScope nullScope = new NullScope();

        public static NullScope NULL => nullScope;

        public class NullScope : IScope
        {

            public IScope Parent { get; set; }
            public TypeInfo Type { get; set; }

            public bool AddType(string name, TypeInfo type)
            {
                return false;
            }

            public bool UpdateType(string name, TypeInfo type)
            {
                return false;
            }

            public IScope CreateChild()
            {
                return new Scope()
                {
                    Parent = NULL,
                    Type = null
                };
            }

            public bool Define(string name, TypeInfo type)
            {
                return false;
            }

            public bool Define(string name, TypeInfo[] args, TypeInfo type)
            {
                return false;
            }

            public TypeInfo GetType(string name)
            {
                return TypeInfo.OBJECT;
            }

            public bool IsDefined(string name, out TypeInfo type)
            {
                type = TypeInfo.OBJECT;
                return false;
            }

            public bool IsDefined(string name, TypeInfo[] args, out TypeInfo type)
            {
                type = TypeInfo.OBJECT;
                return false;
            }

            public bool IsDefinedType(string name, out TypeInfo type)
            {
                type = TypeInfo.OBJECT;
                return false;
            }
        }
        #endregion

    }
    public class TypeInfo
    {
        public string Text { get; set; }
        public TypeInfo Parent { get; set; }
        public ClassNode ClassReference { get; set; }
        public int Level { get; set; }

        public TypeInfo()
        {
            Text = "Object";
            Parent = null;
            ClassReference = null;
            Level = 0;
        }

        public TypeInfo(string text, TypeInfo parent, ClassNode classReference)
        {
            Text = text;
            Parent = parent;
            ClassReference = classReference;
            Level = parent.Level + 1;
        }

        /// Check if a type inherit of other type in the hierarchy of the program.
        public virtual bool Inherit(TypeInfo other)
        {
            if (this == other) return true;
            return Parent.Inherit(other);
        }

        public static bool operator <=(TypeInfo a, TypeInfo b)
        {
            return a.Inherit(b);
        }

        public static bool operator >=(TypeInfo a, TypeInfo b)
        {
            return b.Inherit(a);
        }

        public static bool operator ==(TypeInfo a, TypeInfo b)
        {
            return a.Text == b.Text;
        }

        public static bool operator !=(TypeInfo a, TypeInfo b)
        {
            return !(a == b);
        }

        #region OBJECT
        private static ObjectTypeInfo objectType = new ObjectTypeInfo();

        public static ObjectTypeInfo OBJECT => objectType;

        public class ObjectTypeInfo : TypeInfo
        {

            public override bool Inherit(TypeInfo other)
            {
                return this == other;
            }
        }
        #endregion

        public override string ToString()
        {
            return Text + " : " + Parent.Text;
        }
    }
}
