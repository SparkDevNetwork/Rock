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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
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

        /*
            11/13/26 - CLAUDE

            The cache used to be a plain Dictionary keyed by a hash of the
            field list. When several requests asked for the same uncached
            type at once, they all tried to define a type with the same name,
            all but one threw, and those callers got null. The Lazy makes
            sure each field list is built exactly once without a lock().
            The key is the full field list so two different field lists can
            never share a type, and type names come from a counter so they
            can't collide either.

            Reason: Reports failed to load when several people ran them at once.
        */

        /// <summary>
        /// The types already built, keyed by their full field list.
        /// </summary>
        private static readonly ConcurrentDictionary<string, Lazy<Type>> builtTypes = new ConcurrentDictionary<string, Lazy<Type>>();

        /// <summary>
        /// The number of types built so far, used to give each one a unique name.
        /// </summary>
        private static int typeCounter = 0;

        /// <summary>
        /// Initializes the <see cref="LinqRuntimeTypeBuilder"/> class.
        /// </summary>
        static LinqRuntimeTypeBuilder()
        {
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly( assemblyName, AssemblyBuilderAccess.Run );
            moduleBuilder = assemblyBuilder.DefineDynamicModule( assemblyName.Name );
        }

        /// <summary>
        /// Gets the key for the field list, used to look in our cache of builtTypes.
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns>A string that is unique to the field names and types.</returns>
        private static string GetTypeKey( Dictionary<string, Type> fields )
        {
            var key = new StringBuilder();
            foreach ( var field in fields )
            {
                key.Append( field.Key ).Append( ';' ).Append( field.Value.FullName ).Append( ';' );
            }

            return key.ToString();
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

            string key = null;
            Lazy<Type> lazyType = null;

            try
            {
                key = GetTypeKey( fields );
                lazyType = builtTypes.GetOrAdd( key, k => new Lazy<Type>( () => BuildType( fields ), LazyThreadSafetyMode.ExecutionAndPublication ) );

                return lazyType.Value;
            }
            catch
            {
                if ( lazyType == null )
                {
                    return null;
                }

                // Lazy caches the exception, so remove this entry and let the
                // next call try again. The failure may not depend on the field
                // list, for example a ThreadAbortException from a request
                // timeout. Removing by key and value leaves alone any newer
                // entry another thread may have added for the same key.
                ( ( ICollection<KeyValuePair<string, Lazy<Type>>> ) builtTypes ).Remove( new KeyValuePair<string, Lazy<Type>>( key, lazyType ) );

                return null;
            }
        }

        /// <summary>
        /// Builds a new Type with a public field and a matching read-only
        /// property for each entry in <paramref name="fields"/>.
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns>The new Type.</returns>
        private static Type BuildType( Dictionary<string, Type> fields )
        {
            var className = "LinqRuntimeType" + Interlocked.Increment( ref typeCounter );

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

            return typeBuilder.CreateType();
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
