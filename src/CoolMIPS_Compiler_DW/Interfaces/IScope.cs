using System;
using System.Collections.Generic;
using System.Text;
using CoolMIPS_Compiler_DW.Semantic;

namespace CoolMIPS_Compiler_DW.Interfaces
{
    public interface IScope
    {
        /// Determines if a variable is defined.
        bool IsDefined(string name, out TypeInfo type);

        ///  Determines if a function is defined.
        bool IsDefined(string name, TypeInfo[] args, out TypeInfo typeReturn);

        /// Determines if a type is defined.
        bool IsDefinedType(string name, out TypeInfo type);

        ///True if the variable was define correctly, false otherwise.
        bool Define(string name, TypeInfo type);

        ///True if the function was define correctly, false otherwise
        bool Define(string name, TypeInfo[] args, TypeInfo typeReturn);

        /// Change the type of the variable.
        bool UpdateType(string name, TypeInfo type);

        /// Add a type to this scope.
        bool AddType(string name, TypeInfo type);

        /// Seach a type to this scope.
        TypeInfo GetType(string name);

        /// Create a child of this scope.
        IScope CreateChild();

        /// Property that represent the scope parent of the scope.
        IScope Parent { get; set; }

        /// Property that represent the type own of the scope.
        TypeInfo Type { get; set; }

    }
}
