using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.FieldTypes
{
    /// <summary>
    /// Helper class for use with <see cref="IFieldType"/> classes
    /// </summary>
    public class FieldHelper
    {
        /// <summary>
        /// Instantiates an instance of a specific field type based on the assembly and class name of the field type
        /// </summary>
        /// <param name="assembly">Assembly Name (i.e. Rock.Framework)</param>
        /// <param name="typeName">Class Name (i.e. Rock.FieldTypes.Text)</param>
        /// <returns></returns>
        public static IFieldType InstantiateFieldType( string assembly, string typeName )
        {
            string thetype = string.Format( "{0}, {1}", typeName, assembly );
            Type type = Type.GetType(thetype);
            if (type != null)
                return (IFieldType)Activator.CreateInstance(type);

            return null;
        }
    }
}