using System;
using System.Collections.Generic;

namespace Rock.CodeGeneration.Utility
{
    public class PropertyDeclaration
    {
        public IList<string> RequiredUsings { get; }

        public string TypeName { get; }

        public PropertyDeclaration( string typeName, IList<string> requiredUsings )
        {
            RequiredUsings = requiredUsings;
            TypeName = typeName;
        }

        public PropertyDeclaration( string typeName )
        {
            RequiredUsings = Array.Empty<string>();
            TypeName = typeName;
        }
    }
}
