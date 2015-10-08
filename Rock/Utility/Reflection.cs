// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
        /// Finds the first matching type in Rock or any of the assemblies that reference Rock
        /// </summary>
        /// <param name="baseType">Type of the base.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <returns></returns>
        public static Type FindType( Type baseType, string typeName )
        {
            return FindTypes( baseType, typeName ).Select( a => a.Value ).FirstOrDefault();
        }

        /// <summary>
        /// Finds the all the types that implement or inherit from the baseType.  The baseType
        /// will not be included in the result
        /// </summary>
        /// <param name="baseType">base type.</param>
        /// <param name="typeName">typeName can be specified to filter it to a specific type name</param>
        /// <returns></returns>
        public static SortedDictionary<string, Type> FindTypes( Type baseType, string typeName = null )
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

            // Add any dll's in the Plugins folder
            var httpContext = System.Web.HttpContext.Current;
            if ( httpContext != null )
            {
                var pluginsDir = new DirectoryInfo( httpContext.Server.MapPath( "~/Plugins" ) );
                if ( pluginsDir.Exists )
                {
                    foreach ( var file in pluginsDir.GetFiles( "*.dll", SearchOption.AllDirectories ) )
                    {
                        var assembly = Assembly.LoadFrom( file.FullName );
                        if ( !assemblies.Keys.Contains( assembly.FullName.ToLower() ) )
                        {
                            assemblies.Add( assembly.FullName.ToLower(), assembly );
                        }
                    }
                }
            }

            foreach ( KeyValuePair<string, Assembly> assemblyEntry in assemblies )
            {
                var typeEntries = SearchAssembly( assemblyEntry.Value, baseType );
                foreach ( KeyValuePair<string, Type> typeEntry in typeEntries )
                {
                    if ( string.IsNullOrWhiteSpace( typeName ) || typeEntry.Key == typeName )
                    {
                        types.AddOrIgnore( typeEntry.Key, typeEntry.Value );
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
        /// Returns the DisplayName Attribute value for a given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetDisplayName( Type type )
        {
            foreach ( var nameAttribute in type.GetCustomAttributes( typeof( DisplayNameAttribute ), true ) )
            {
                return ( (DisplayNameAttribute)nameAttribute ).DisplayName;
            }
            return null;
        }

        /// <summary>
        /// Returns the Category Attribute value for a given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetCategory( Type type )
        {
            foreach ( var categoryAttribute in type.GetCustomAttributes( typeof( CategoryAttribute ), true ) )
            {
                return ( (CategoryAttribute)categoryAttribute ).Category;
            }
            return null;
        }
        /// <summary>
        /// Returns the Description Attribute value for a given type
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

        /// <summary>
        /// Gets the appropriate DbContext based on the entity type
        /// </summary>
        /// <param name="entityType">Type of the Entity.</param>
        /// <returns></returns>
        public static System.Data.Entity.DbContext GetDbContextForEntityType( Type entityType )
        {
            Type contextType = typeof( Rock.Data.RockContext );
            if ( entityType.Assembly != contextType.Assembly )
            {
                var contextTypeLookup = Reflection.SearchAssembly( entityType.Assembly, typeof( System.Data.Entity.DbContext ) );

                if ( contextTypeLookup.Any() )
                {
                    contextType = contextTypeLookup.First().Value;
                }
            }

            System.Data.Entity.DbContext dbContext = Activator.CreateInstance( contextType ) as System.Data.Entity.DbContext;
            return dbContext;
        }

        /// <summary>
        /// Gets the appropriate Rock.Data.IService based on the entity type
        /// </summary>
        /// <param name="entityType">Type of the Entity.</param>
        /// <param name="dbContext">The database context.</param>
        /// <returns></returns>
        public static Rock.Data.IService GetServiceForEntityType( Type entityType, System.Data.Entity.DbContext dbContext )
        {
            Type serviceType = typeof( Rock.Data.Service<> );
            if ( entityType.Assembly != serviceType.Assembly )
            {
                var serviceTypeLookup = Reflection.SearchAssembly( entityType.Assembly, serviceType );
                if ( serviceTypeLookup.Any() )
                {
                    serviceType = serviceTypeLookup.First().Value;
                }
            }

            Type service = serviceType.MakeGenericType( new Type[] { entityType } );
            Rock.Data.IService serviceInstance = Activator.CreateInstance( service, dbContext ) as Rock.Data.IService;
            return serviceInstance;
        }
    }
}