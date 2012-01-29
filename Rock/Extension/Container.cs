//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;

namespace Rock.Extension
{
    /// <summary>
    /// Singleton generic class that uses MEF to load and cache all of the component classes
    /// </summary>
    public abstract class Container<T, TData>
        where T : Component
        where TData : IComponentData
    {
        /// <summary>
        /// Gets the services.
        /// </summary>
        public Dictionary<int, Lazy<T, TData>> Services { get; private set; }

        // MEF Container
        private CompositionContainer container;

        /// <summary>
        /// Gets or sets the components.
        /// </summary>
        /// <value>
        /// The components.
        /// </value>
        protected abstract IEnumerable<Lazy<T, TData>> Components { get; set; }

        /// <summary>
        /// Forces a reloading of all the GeocodeService classes
        /// </summary>
        public void Refresh()
        {
            Services = new Dictionary<int, Lazy<T, TData>>();

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
            var services = new SortedDictionary<int, List<Lazy<T, TData>>>();
            foreach ( Lazy<T, TData> i in Components )
            {
                if ( !services.ContainsKey( i.Value.Order ) )
                    services.Add( i.Value.Order, new List<Lazy<T, TData>>() );
                services[i.Value.Order].Add( i );
            }

            // Add each class found through MEF into the Services property value in the correct order
            int id = 0;
            foreach ( KeyValuePair<int, List<Lazy<T, TData>>> entry in services )
                foreach ( Lazy<T, TData> service in entry.Value )
                    Services.Add(id++, service );
        }
    }
}