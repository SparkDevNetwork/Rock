// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
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