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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Rock.Data
{
    /// <summary>
    /// Helps create a Type at runtime that can be used when building a dynamic Linq select statement
    /// From http://stackoverflow.com/questions/606104/how-to-create-linq-expression-tree-with-anonymous-type-in-it
    /// see answer http://stackoverflow.com/a/723018/1755417
    /// The stackoverflow version only defined fields, but our implementation adds Properties
    /// </summary>
    public static class LinqRuntimeTypeBuilder
    {
        /// <summary>
        /// Creates a new 'in-memory' assembly at runtime where the new type can be generated and stored
        /// </summary>
        private static AssemblyName assemblyName = new AssemblyName() { Name = "DynamicLinqTypes" };

        /// <summary>
        /// The ModuleBuilder within our DynamicLinqTypes assembly where the new Type can be defined
        /// </summary>
        private static ModuleBuilder moduleBuilder = null;

        /// <summary>
        /// A place to store the types have already created so we don't have to recreate a type that has already been created
        /// </summary>
        private static Dictionary<string, Type> builtTypes = new Dictionary<string, Type>();

        /// <summary>
        /// Initializes the <see cref="LinqRuntimeTypeBuilder"/> class.
        /// </summary>
        static LinqRuntimeTypeBuilder()
        {
            moduleBuilder = Thread.GetDomain().DefineDynamicAssembly( assemblyName, AssemblyBuilderAccess.Run ).DefineDynamicModule( assemblyName.Name );
        }

        /// <summary>
        /// Gets the type key which can be used to look in our cache of builtTypes
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns></returns>
        private static string GetTypeKey( Dictionary<string, Type> fields )
        {
            // class name must be unique, but can't be more than 1024 chars long.  Show generate a hash using the Fields
            string key = string.Empty;
            foreach ( var field in fields )
            {
                key += field.Key + ";" + field.Value.Name + ";";
            }
            
            return "LinqRuntimeType" + key.GetHashCode();
        }

        /// <summary>
        /// Creates a Type from a list of fields and their type then returns that Type 
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">fields</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">fields;fields must have at least 1 field definition</exception>
        public static Type GetDynamicType( Dictionary<string, Type> fields )
        {
            if ( null == fields )
            {
                throw new ArgumentNullException( "fields" );
            }

            if ( 0 == fields.Count )
            {
                throw new ArgumentOutOfRangeException( "fields", "fields must have at least 1 field definition" );
            }

            try
            {
                string className = GetTypeKey( fields );

                if ( builtTypes.ContainsKey( className ) )
                {
                    return builtTypes[className];
                }

                TypeBuilder typeBuilder = moduleBuilder.DefineType( className, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Serializable );

                foreach ( var field in fields )
                {
                    // make firstchar of fieldName lowercase
                    string fieldName = field.Key.ToLower()[0] + field.Key.Substring( 1 );
                    if ( fieldName == field.Key )
                    {
                        throw new Exception( "Field names must start with an uppercase character" );
                    }

                    var fieldBuilder = typeBuilder.DefineField( fieldName, field.Value, FieldAttributes.Public );

                    // create a Property for each Field so that DataGrids can bind to the object
                    var propertyBuilder = typeBuilder.DefineProperty( field.Key, PropertyAttributes.HasDefault, field.Value, null );

                    // The property set and property get methods require a special set of attributes.
                    MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

                    // Define the "get" accessor method for the property and gen IL code so its value comes from the field
                    MethodBuilder getPropMthdBldr = typeBuilder.DefineMethod( "get_" + field.Key, getSetAttr, field.Value, Type.EmptyTypes );

                    ILGenerator fieldGetIL = getPropMthdBldr.GetILGenerator();

                    fieldGetIL.Emit( OpCodes.Ldarg_0 );
                    fieldGetIL.Emit( OpCodes.Ldfld, fieldBuilder );
                    fieldGetIL.Emit( OpCodes.Ret );

                    propertyBuilder.SetGetMethod( getPropMthdBldr );
                }

                builtTypes[className] = typeBuilder.CreateType();

                return builtTypes[className];
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the type key which can be used to look in our cache of builtTypes
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns></returns>
        private static string GetTypeKey( IEnumerable<PropertyInfo> fields )
        {
            return GetTypeKey( fields.ToDictionary( f => f.Name, f => f.PropertyType ) );
        }

        /// <summary>
        /// Creates a Type from a list of field PropertyInfos then returns that type
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns></returns>
        public static Type GetDynamicType( IEnumerable<PropertyInfo> fields )
        {
            return GetDynamicType( fields.ToDictionary( f => f.Name, f => f.PropertyType ) );
        }
    }
}
