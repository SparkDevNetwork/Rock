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
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;

using Rock;

/// <summary>
/// MEF Directory Catalog that will handle outdated MEF Components
/// </summary>
public class SafeDirectoryCatalog : ComposablePartCatalog
{
    private readonly AggregateCatalog _catalog;

    /// <summary>
    /// Initializes a new instance of the <see cref="SafeDirectoryCatalog" /> class.
    /// </summary>
    /// <param name="directory">The directory.</param>
    /// <param name="baseType">Type of the base.</param>
    [RockObsolete( "1.8" )]
    [Obsolete( "Use SafeDirectoryCatalog(baseType) instead", true )]
    public SafeDirectoryCatalog( string directory, Type baseType )
    : this( baseType )
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SafeDirectoryCatalog"/> class.
    /// </summary>
    /// <param name="baseType">Type of the base.</param>
    public SafeDirectoryCatalog( Type baseType )
    {
        var assemblies = Reflection.GetPluginAssemblies();

        // Add Rock.dll
        assemblies.Add( typeof( SafeDirectoryCatalog ).Assembly );

        string baseTypeAssemblyName = baseType.Assembly.GetName().Name;

        _catalog = new AggregateCatalog();

        foreach ( var assembly in assemblies.ToList() )
        {
            try
            {
                // only attempt to load the catalog if the assembly is or references the basetype assembly
                if ( assembly == baseType.Assembly || assembly.GetReferencedAssemblies().Any( a => a.Name.Equals( baseTypeAssemblyName, StringComparison.OrdinalIgnoreCase ) ) )
                {
                    AssemblyCatalog assemblyCatalog = new AssemblyCatalog( assembly );

                    // Force MEF to load the plugin and figure out if there are any exports
                    // good assemblies will not throw the RTLE exception and can be added to the catalog
                    if ( assemblyCatalog.Parts.ToList().Count > 0 )
                    {
                        _catalog.Catalogs.Add( assemblyCatalog );
                    }
                }
            }
            catch ( ReflectionTypeLoadException e )
            {
                foreach ( var loaderException in e.LoaderExceptions )
                {
                    Rock.Model.ExceptionLogService.LogException( new Exception( "Unable to load MEF from " + assembly.FullName, loaderException ) );
                }

                string msg = e.Message;
            }
        }
    }

    /// <summary>
    /// Gets the part definitions that are contained in the catalog.
    /// </summary>
    /// <returns>The <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartDefinition" /> contained in the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartCatalog" />.</returns>
    public override IQueryable<ComposablePartDefinition> Parts
    {
        get { return _catalog.Parts; }
    }
}