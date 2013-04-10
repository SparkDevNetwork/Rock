//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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