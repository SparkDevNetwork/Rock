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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Rock
{
    /// <summary>
    /// Static helper methods for accessing the .NET type system.
    /// </summary>
    public static class SystemTypeHelper
    {
        /// <summary>
        /// Finds the first matching type in Rock or any of the assemblies that reference Rock
        /// </summary>
        /// <param name="baseType">Type of the base.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <returns></returns>
        /// <remarks>
        /// This code is taken from the Rock.Reflection class, with some data-model-related functions removed.
        /// </remarks>
        public static Type FindType( Type baseType, string typeName )
        {
            return FindTypes( baseType, typeName ).Select( a => a.Value ).FirstOrDefault();
        }

        /// <summary>
        /// Finds the all the types that implement or inherit from the baseType. NOTE: It will only search the Rock.dll and also in assemblies that reference Rock.dll. The baseType
        /// will not be included in the result
        /// </summary>
        /// <param name="baseType">base type.</param>
        /// <param name="typeName">typeName can be specified to filter it to a specific type name</param>
        /// <returns></returns>
        public static SortedDictionary<string, Type> FindTypes( Type baseType, string typeName = null )
        {
            SortedDictionary<string, Type> types = new SortedDictionary<string, Type>();

            var assemblies = GetPluginAssemblies();

            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            assemblies.Add( executingAssembly );

            foreach ( var assemblyEntry in assemblies )
            {
                var typeEntries = SearchAssembly( assemblyEntry, baseType );
                foreach ( KeyValuePair<string, Type> typeEntry in typeEntries )
                {
                    if ( string.IsNullOrWhiteSpace( typeName ) || typeEntry.Key == typeName )
                    {
                        types.TryAdd( typeEntry.Key, typeEntry.Value );
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
            return type.GetCustomAttribute<DisplayNameAttribute>( true )?.DisplayName;
        }

        /// <summary>
        /// Returns the Category Attribute value for a given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetCategory( Type type )
        {
            return type.GetCustomAttribute<CategoryAttribute>( true )?.Category;
        }

        /// <summary>
        /// Returns the Description Attribute value for a given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetDescription( Type type )
        {
            return type.GetCustomAttribute<DescriptionAttribute>( true )?.Description;
        }

        /// <summary>
        /// Determines whether the specified property of an IEntity is mapped to a real database field
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns>
        ///   <c>true</c> if [is mapped database property] [the specified property information]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsMappedDatabaseProperty( PropertyInfo propertyInfo )
        {
            // if marked as NotMapped, it isn't a database property
            var notMapped = propertyInfo.GetCustomAttribute<NotMappedAttribute>() != null;

            if ( notMapped )
            {
                return false;
            }

            // if the property is marked virtual (unless it is 'virtual final'), don't include it since it isn't a real database field
            var getter = propertyInfo.GetGetMethod();
            var isVirtual = getter?.IsVirtual == true;
            if ( isVirtual )
            {
                // NOTE: Properties that implement interface members (for example Rock.Data.IOrder) will also be marked as 'virtual final' by the compiler, so check IsFinal to determine if it was the compiler that did it.
                // See https://docs.microsoft.com/en-us/dotnet/api/system.reflection.methodbase.isfinal?redirectedfrom=MSDN&view=netframework-4.7.2#System_Reflection_MethodBase_IsFinal
                bool isVirtualDueToInterface = getter?.IsFinal == true;
                if ( !isVirtualDueToInterface )
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The Plugin assemblies
        /// </summary>
        private static List<Assembly> _pluginAssemblies = null;


        /// <summary>
        /// The RockWeb app_code assembly
        /// </summary>
        private static Assembly _appCodeAssembly = null;

        /// <summary>
        /// Sets the RockWeb.App_Code assembly so that the Reflection methods can search for types in it
        /// </summary>
        /// <param name="appCodeAssembly">The application code assembly.</param>
        public static void SetAppCodeAssembly( Assembly appCodeAssembly )
        {
            _appCodeAssembly = appCodeAssembly;
            if ( _pluginAssemblies != null && _appCodeAssembly != null )
            {
                _pluginAssemblies.Add( _appCodeAssembly );
            }
        }

        /// <summary>
        /// Gets a list of Assemblies in the ~/Bin and ~/Plugins folders as well as the RockWeb.App_Code assembly that are assemblies that might have plugins
        /// </summary>
        /// <returns></returns>
        public static List<Assembly> GetPluginAssemblies()
        {
            if ( _pluginAssemblies != null )
            {
                return _pluginAssemblies.ToList();
            }

            // Add executing assembly's directory
            string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder( codeBase );
            string path = Uri.UnescapeDataString( uri.Path );
            string binDirectory = Path.GetDirectoryName( path );

            // Add all the assemblies in the 'Plugins' subdirectory
            string pluginsFolder = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "Plugins" );

            // blacklist of files that would never have Rock MEF components or Rock types
            string[] ignoredFileStart = { "Lucene.", "Microsoft.", "msvcr100.", "System.", "JavaScriptEngineSwitcher.", "React.", "CacheManager." };

            // get all *.dll in the bin and plugin directories except for blacklisted ones
            var assemblyFileNames = Directory.EnumerateFiles( binDirectory, "*.dll", SearchOption.AllDirectories ).ToList();

            if ( Directory.Exists( pluginsFolder ) )
            {
                assemblyFileNames.AddRange( Directory.EnumerateFiles( pluginsFolder, "*.dll", SearchOption.AllDirectories ) );
            }

            assemblyFileNames = assemblyFileNames.Where( a => !a.EndsWith( ".resources.dll", StringComparison.OrdinalIgnoreCase )
                                        && !ignoredFileStart.Any( i => Path.GetFileName( a ).StartsWith( i, StringComparison.OrdinalIgnoreCase ) ) ).ToList();

            // get a lookup of already loaded assemblies so that we don't have to load it unnecessarily
            var loadedAssembliesDictionary = AppDomain.CurrentDomain.GetAssemblies().Where( a => !a.IsDynamic && !a.GlobalAssemblyCache && !string.IsNullOrWhiteSpace( a.Location ) )
                .GroupBy( k => new Uri( k.CodeBase ).LocalPath ).Select( x => x.First() )
                //.DistinctBy( k => new Uri( k.CodeBase ).LocalPath )
                .ToDictionary( k => new Uri( k.CodeBase ).LocalPath, v => v, StringComparer.OrdinalIgnoreCase );

            List<Assembly> pluginAssemblies = new List<Assembly>();
            if ( _appCodeAssembly != null )
            {
                pluginAssemblies.Add( _appCodeAssembly );
            }

            foreach ( var assemblyFileName in assemblyFileNames )
            {
                Assembly assembly = loadedAssembliesDictionary.GetValueOrNull( assemblyFileName );
                if ( assembly == null )
                {
                    try
                    {
                        // if an assembly is found that isn't loaded yet, load it into the CurrentDomain
                        AssemblyName assemblyName = AssemblyName.GetAssemblyName( assemblyFileName );
                        assembly = AppDomain.CurrentDomain.Load( assemblyName );
                    }
                    catch ( BadImageFormatException )
                    {
                        // BadImageFormatException means the dll isn't a managed dll (not a .NET dll), so we can safely ignore
                    }
                    catch ( Exception ex )
                    {
                        throw ex;
                        //Rock.Model.ExceptionLogService.LogException( new Exception( $"Unable to load assembly from {assemblyFileName}", ex ) );
                    }
                }

                if ( assembly != null )
                {
                    bool isRockAssembly = false;

                    // only search inside dlls that are Rock.dll or reference Rock.dll
                    if ( assemblyFileName.Equals( "Rock.dll", StringComparison.OrdinalIgnoreCase ) )
                    {
                        isRockAssembly = true;
                    }
                    else
                    {
                        List<AssemblyName> referencedAssemblies = assembly.GetReferencedAssemblies().ToList();

                        if ( referencedAssemblies.Any( a => a.Name.Equals( "Rock", StringComparison.OrdinalIgnoreCase ) ) )
                        {
                            isRockAssembly = true;
                        }
                    }

                    if ( isRockAssembly )
                    {
                        pluginAssemblies.Add( assembly );
                    }
                }
            }

            _pluginAssemblies = pluginAssemblies;

            return _pluginAssemblies.ToList();
        }
    }
}