using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rock.Data
{
    public static class LinqRuntimeTypeBuilder
    {
        private static AssemblyName assemblyName = new AssemblyName() { Name = "DynamicLinqTypes" };
        private static ModuleBuilder moduleBuilder = null;
        private static Dictionary<string, Type> builtTypes = new Dictionary<string, Type>();

        static LinqRuntimeTypeBuilder()
        {
            moduleBuilder = Thread.GetDomain().DefineDynamicAssembly( assemblyName, AssemblyBuilderAccess.Run ).DefineDynamicModule( assemblyName.Name );
        }

        private static string GetTypeKey( Dictionary<string, Type> fields )
        {
            //TODO: optimize the type caching -- if fields are simply reordered, that doesn't mean that they're actually different types, so this needs to be smarter
            string key = string.Empty;
            foreach ( var field in fields )
                key += field.Key + ";" + field.Value.Name + ";";

            return key;
        }

        public static Type GetDynamicType( Dictionary<string, Type> fields )
        {
            if ( null == fields )
                throw new ArgumentNullException( "fields" );
            if ( 0 == fields.Count )
                throw new ArgumentOutOfRangeException( "fields", "fields must have at least 1 field definition" );

            try
            {
                string className = GetTypeKey( fields );

                if ( builtTypes.ContainsKey( className ) )
                    return builtTypes[className];

                TypeBuilder typeBuilder = moduleBuilder.DefineType( className, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Serializable );

                foreach ( var field in fields )
                    typeBuilder.DefineField( field.Key, field.Value, FieldAttributes.Public );

                builtTypes[className] = typeBuilder.CreateType();

                return builtTypes[className];
            }
            catch
            {
                return null;
            }

        }


        private static string GetTypeKey( IEnumerable<PropertyInfo> fields )
        {
            return GetTypeKey( fields.ToDictionary( f => f.Name, f => f.PropertyType ) );
        }

        public static Type GetDynamicType( IEnumerable<PropertyInfo> fields )
        {
            return GetDynamicType( fields.ToDictionary( f => f.Name, f => f.PropertyType ) );
        }
    }
}
