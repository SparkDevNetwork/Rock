using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Compilation;

namespace Rock.Helpers
{
    public static class Reflection
    {
        public static SortedDictionary<string, Type> FindTypes ( Type baseType )
        {
            return FindTypes( baseType, false );
        }

        public static SortedDictionary<string, Type> FindTypes( Type baseType, bool includeBaseType )
        {
            SortedDictionary<string, Type> types = new SortedDictionary<string, Type>();

            if ( includeBaseType )
                types.Add( ClassName( baseType ), baseType );

            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            FileInfo executingFile = new FileInfo( executingAssembly.Location );

            Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();
            assemblies.Add( executingAssembly.Location, executingAssembly );

            foreach ( Assembly assembly in AppDomain.CurrentDomain.GetAssemblies() )
                if ( assembly.FullName.StartsWith( "Rock" ) && !assemblies.Keys.Contains( assembly.Location  ) )
                    assemblies.Add( assembly.FullName, assembly );

            //foreach (Assembly assembly in BuildManager.GetReferencedAssemblies())
            //    if ( assembly.FullName.StartsWith( "Rock" ) && !assemblies.Keys.Contains( assembly.Location ) )
            //        assemblies.Add( assembly.FullName, assembly );

            foreach ( FileInfo fileInfo in executingFile.Directory.GetFiles( "rock.*.dll" ) )
                if ( !assemblies.Keys.Contains( fileInfo.FullName ) )
                {
                    Assembly fileAssembly = Assembly.LoadFrom( fileInfo.FullName );
                    assemblies.Add( fileInfo.FullName, fileAssembly );
                }


            foreach ( KeyValuePair<string, Assembly> assemblyEntry in assemblies )
                foreach ( KeyValuePair<string, Type> typeEntry in SearchAssembly( assemblyEntry.Value, baseType ) )
                    types.Add( typeEntry.Key, typeEntry.Value );

            return types;
        }

        private static Dictionary<string, Type> SearchAssembly( Assembly assembly, Type baseType )
        {
            Dictionary<string, Type> types = new Dictionary<string, Type>();

            try
            {
                foreach ( Type type in assembly.GetTypes() )
                {
                    if ( baseType.IsInterface )
                    {
                        foreach ( Type typeInterface in type.GetInterfaces() )
                            if ( typeInterface == baseType )
                            {
                                types.Add( ClassName( type ), type );
                                break;
                            }
                    }
                    else
                    {
                        Type parentType = type.BaseType;
                        while ( parentType != null )
                        {
                            if ( parentType == baseType )
                            {
                                types.Add( ClassName( type ), type );
                                break;
                            }
                            parentType = parentType.BaseType;
                        }
                    }
                }
            }
            catch ( ReflectionTypeLoadException ex )
            {
                string dd = ex.Message;
            }

            return types;
        }

        public static string ClassName(Type type)
        {
            object[] attributes = type.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), true);
            if (attributes.Length > 0)
                return ((System.ComponentModel.DescriptionAttribute)attributes[0]).Description;

            return type.ToString();
        }
    }
}