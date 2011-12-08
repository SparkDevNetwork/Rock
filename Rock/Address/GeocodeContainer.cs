using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;

namespace Rock.Address
{
    /// <summary>
    /// Singleton class that uses MEF to load and cache all of the GeocodeService classes
    /// </summary>
    public class GeocodeContainer
    {
        private static GeocodeContainer instance;

        /// <summary>
        /// Gets the services.
        /// </summary>
        public Dictionary<int, Lazy<GeocodeService, IGeocodeServiceData>> Services { get; private set; }

        // MEF Container
        private CompositionContainer container;

        // MEF Import Definition
#pragma warning disable 
        [ImportMany( typeof( GeocodeService ) )]
        IEnumerable<Lazy<GeocodeService, IGeocodeServiceData>> geocodingServices;
#pragma warning restore

        private GeocodeContainer() 
        {
            Refresh();
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static GeocodeContainer Instance
        {
            get
            {
                if ( instance == null )
                    instance = new GeocodeContainer();
                return instance;
            }
        }

        /// <summary>
        /// Forces a reloading of all the GeocodeService classes
        /// </summary>
        public void Refresh()
        {
            Services = new Dictionary<int, Lazy<GeocodeService, IGeocodeServiceData>>();

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

            // Compose the MEF container with any classes that export the same definition
            container.ComposeParts( this );

            // Create a temporary sorted dictionary of the classes so that they can be executed in a specific order
            var services = new SortedDictionary<int, List<Lazy<GeocodeService, IGeocodeServiceData>>>();
            foreach ( Lazy<GeocodeService, IGeocodeServiceData> i in geocodingServices )
            {
                if ( !services.ContainsKey( i.Value.Order ) )
                    services.Add( i.Value.Order, new List<Lazy<GeocodeService, IGeocodeServiceData>>() );
                services[i.Value.Order].Add( i );
            }

            // Add each class found through MEF into the Services property value in the correct order
            int id = 0;
            foreach ( KeyValuePair<int, List<Lazy<GeocodeService, IGeocodeServiceData>>> entry in services )
                foreach ( Lazy<GeocodeService, IGeocodeServiceData> service in entry.Value )
                    Services.Add(id++, service );
        }
    }
}