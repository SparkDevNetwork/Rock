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
        /// <param name="directories">The directories.</param>
        /// <returns></returns>
        public static SortedDictionary<string, Type> FindTypes( Type baseType, DirectoryInfo[] directories = null )
        {
            return FindTypes( baseType, false, directories );
        }

        /// <summary>
        /// Finds the all the types that implement or inherit from the baseType.
        /// </summary>
        /// <param name="baseType">base type.</param>
        /// <param name="includeBaseType">if set to <c>true</c> the base type will be included in the result</param>
        /// <param name="directories">The directories.</param>
        /// <returns></returns>
        public static SortedDictionary<string, Type> FindTypes( Type baseType, bool includeBaseType, DirectoryInfo[] directories = null )
        {
            SortedDictionary<string, Type> types = new SortedDictionary<string, Type>();

            if ( includeBaseType )
                types.Add( ClassName( baseType ), baseType );

            Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            assemblies.Add( executingAssembly.FullName.ToLower(), executingAssembly );

            foreach ( Assembly assembly in AppDomain.CurrentDomain.GetAssemblies() )
                if ( assembly.FullName.StartsWith( "Rock" ) && !assemblies.Keys.Contains( assembly.FullName.ToLower() ) )
                    assemblies.Add( assembly.FullName.ToLower(), assembly );

            if ( directories != null )
            {
                foreach ( var directory in directories )
                {
                    foreach ( FileInfo fileInfo in directory.GetFiles( "rock.*.dll" ) )
                    {
                        Assembly fileAssembly = Assembly.LoadFrom( fileInfo.FullName );
                        if ( !assemblies.Keys.Contains( fileAssembly.FullName.ToLower() ) )
                            assemblies.Add( fileAssembly.FullName.ToLower(), fileAssembly );
                    }
                }
            }

            foreach ( KeyValuePair<string, Assembly> assemblyEntry in assemblies )
                foreach ( KeyValuePair<string, Type> typeEntry in SearchAssembly( assemblyEntry.Value, baseType ) )
                    if ( !types.Keys.Contains( typeEntry.Key ) )
                        types.Add( typeEntry.Key, typeEntry.Value );

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

        /// <summary>
        /// Returns the name of the type.  If a <see cref="System.ComponentModel.DescriptionAttribute"/> is 
        /// present for the type, it's value will be returned, otherwise the type name will be returned
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static string ClassName( Type type )
        {
            object[] attributes = type.GetCustomAttributes( typeof( System.ComponentModel.DescriptionAttribute ), true );
            if ( attributes.Length > 0 )
                return ( (System.ComponentModel.DescriptionAttribute)attributes[0] ).Description;

            return type.ToString();
        }

        /// <summary>
        /// Returnes the Description Attribute value for a given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetDescription( Type type )
        {
            foreach ( var descriptionAttribute in type.GetCustomAttributes( typeof( DescriptionAttribute ), true ) )
                return ( (DescriptionAttribute)descriptionAttribute ).Description;
            return null;
        }
    }
}