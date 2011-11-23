using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Rock.Address
{
    /// <summary>
    /// Singleton class that uses MEF to load and cache all of the StandardizeService classes
    /// </summary>
    public class StandardizeContainer
    {
        private static StandardizeContainer instance;

        /// <summary>
        /// Gets the services.
        /// </summary>
        public Dictionary<int, Lazy<StandardizeService, IStandardizeServiceData>> Services { get; private set; }

        // MEF Container
        private CompositionContainer container;

        // MEF Import Definition
        [ImportMany( typeof( StandardizeService ) )]
        IEnumerable<Lazy<StandardizeService, IStandardizeServiceData>> geocodingServices;

        private StandardizeContainer() 
        {
            Refresh();
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static StandardizeContainer Instance
        {
            get
            {
                if ( instance == null )
                    instance = new StandardizeContainer();
                return instance;
            }
        }

        /// <summary>
        /// Forces a reloading of all the StandardizeService classes
        /// </summary>
        public void Refresh()
        {
            Services = new Dictionary<int, Lazy<StandardizeService, IStandardizeServiceData>>();

            // Create the MEF Catalog
            var catalog = new AggregateCatalog();

            // Add the currently running assembly to the Catalog
            catalog.Catalogs.Add( new AssemblyCatalog( this.GetType().Assembly ) );

            // Add all the assemblies in the 'Extensions' subdirectory
            string extensionFolder = Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ), "Extensions" );
            if ( Directory.Exists( extensionFolder ) )
                catalog.Catalogs.Add( new DirectoryCatalog( extensionFolder ) );

            // Create the container from the catalog
            container = new CompositionContainer( catalog );

            try
            {
                // Compose the MEF container with any classes that export the same definition
                container.ComposeParts( this );

                // Create a temporary sorted dictionary of the classes so that they can be executed in a specific order
                var services = new SortedDictionary<int, List<Lazy<StandardizeService, IStandardizeServiceData>>>();
                foreach ( Lazy<StandardizeService, IStandardizeServiceData> i in geocodingServices )
                {
                    if ( !services.ContainsKey( i.Value.Order ) )
                        services.Add( i.Value.Order, new List<Lazy<StandardizeService, IStandardizeServiceData>>() );
                    services[i.Value.Order].Add( i );
                }

                // Add each class found through MEF into the Services property value in the correct order
                int id = 0;
                foreach ( KeyValuePair<int, List<Lazy<StandardizeService, IStandardizeServiceData>>> entry in services )
                    foreach ( Lazy<StandardizeService, IStandardizeServiceData> service in entry.Value )
                        Services.Add(id++, service );
            }
            catch ( CompositionException ex )
            {
            }
        }
    }
}