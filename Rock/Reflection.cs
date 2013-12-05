//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Rock
{
    /// <summary>
    /// Static helper methods for using Reflection
    /// </summary>
    public static class Reflection
    {
        /// <summary>
        /// Finds the all the types that implement or inherit from the baseType.  The baseType
        /// will not be included in the result
        /// </summary>
        /// <param name="baseType">base type.</param>
        /// <returns></returns>
        public static SortedDictionary<string, Type> FindTypes( Type baseType )
        {
            SortedDictionary<string, Type> types = new SortedDictionary<string, Type>();

            Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            assemblies.Add( executingAssembly.FullName.ToLower(), executingAssembly );

            foreach ( Assembly assembly in AppDomain.CurrentDomain.GetAssemblies() )
            {
                if ( assembly.GlobalAssemblyCache || assembly.IsDynamic )
                {
                    continue;
                }

                bool searchAssembly = false;

                string fileName = Path.GetFileName( assembly.CodeBase );

                // only search inside dlls that are Rock.dll or reference Rock.dll
                if ( fileName.Equals( "Rock.dll", StringComparison.OrdinalIgnoreCase ) )
                {
                    searchAssembly = true;
                }
                else
                {
                    List<AssemblyName> referencedAssemblies = assembly.GetReferencedAssemblies().ToList();

                    if ( referencedAssemblies.Any( a => a.Name.Equals( "Rock", StringComparison.OrdinalIgnoreCase ) ) )
                    {
                        searchAssembly = true;
                    }
                }

                if ( searchAssembly )
                {
                    if ( !assemblies.Keys.Contains( assembly.FullName.ToLower() ) )
                    {
                        assemblies.Add( assembly.FullName.ToLower(), assembly );
                    }
                }
            }

            foreach ( KeyValuePair<string, Assembly> assemblyEntry in assemblies )
            {
                var typeEntries = SearchAssembly( assemblyEntry.Value, baseType );

                foreach ( KeyValuePair<string, Type> typeEntry in typeEntries )
                {
                    if ( !types.Keys.Contains( typeEntry.Key ) )
                    {
                        types.Add( typeEntry.Key, typeEntry.Value );
                    }
                }
            }

            return types;
        }

        /// <summary>
        /// Searches the assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="baseType">Type of the base.</param>
        /// <returns></returns>
        public static Dictionary<string, Type> SearchAssembly( Assembly assembly, Type baseType )
        {
            Dictionary<string, Type> types = new Dictionary<string, Type>();

            try
            {
                foreach ( Type type in assembly.GetTypes() )
                {
                    if ( !type.IsAbstract )
                    {
                        if ( baseType.IsInterface )
                        {
                            foreach ( Type typeInterface in type.GetInterfaces() )
                            {
                                if ( typeInterface == baseType )
                                {
                                    types.Add( type.FullName, type );
                                    break;
                                }
                            }
                        }
                        else
                        {
                            Type parentType = type.BaseType;
                            while ( parentType != null )
                            {
                                if ( parentType == baseType )
                                {
                                    types.Add( type.FullName, type );
                                    break;
                                }
                                parentType = parentType.BaseType;
                            }
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

        /// <summary>
        /// Returnes the Description Attribute value for a given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetDescription( Type type )
        {
            foreach ( var descriptionAttribute in type.GetCustomAttributes( typeof( DescriptionAttribute ), true ) )
            {
                return ( (DescriptionAttribute)descriptionAttribute ).Description;
            }
            return null;
        }
    }
}