//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;

namespace Rock.Field
{
    /// <summary>
    /// Helper class for use with <see cref="IFieldType"/> classes
    /// </summary>
    public class Helper
    {
        /// <summary>
        /// Instantiates an instance of a specific field type based on the assembly and class name of the field type
        /// </summary>
        /// <param name="assembly">Assembly Name (i.e. Rock)</param>
        /// <param name="typeName">Class Name (i.e. Rock.Field.Types.Text)</param>
        /// <returns></returns>
        public static IFieldType InstantiateFieldType( string assembly, string typeName )
        {
            string thetype = string.Format( "{0}, {1}", typeName, assembly );
            Type type = Type.GetType(thetype);

            if ( type != null )
                return ( IFieldType )Activator.CreateInstance( type );
            else
                return ( IFieldType )Activator.CreateInstance( typeof( Rock.Field.Types.TextFieldType ) );
        }
    }
}