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
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;

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
    public SafeDirectoryCatalog( string directory )
    {
        var files = Directory.EnumerateFiles( directory, "*.dll", SearchOption.AllDirectories );

        _catalog = new AggregateCatalog();

        foreach ( var file in files )
        {
            try
            {
                var asmCat = new AssemblyCatalog( file );

                //Force MEF to load the plugin and figure out if there are any exports
                // good assemblies will not throw the RTLE exception and can be added to the catalog
                if ( asmCat.Parts.ToList().Count > 0 )
                    _catalog.Catalogs.Add( asmCat );
            }

            catch ( ReflectionTypeLoadException e)
            {
                //TODO: Add error logging
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