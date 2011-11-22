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
    public class GeocodeContainer
    {
        private static GeocodeContainer instance;

        public Dictionary<int, GeocodeService> Services { get; private set; }

        private CompositionContainer container;

        [ImportMany( typeof( GeocodeService ) )]
        IEnumerable<Lazy<GeocodeService, IGeocodeServiceData>> geocodingServices;

        private GeocodeContainer() 
        {
            Refresh();
        }

        public static GeocodeContainer Instance
        {
            get
            {
                if ( instance == null )
                    instance = new GeocodeContainer();
                return instance;
            }
        }

        public void Refresh()
        {
            Services = new Dictionary<int, GeocodeService>();

            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add( new AssemblyCatalog( this.GetType().Assembly ) );

            string extensionFolder = Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ), "Extensions" );
            if ( Directory.Exists( extensionFolder ) )
                catalog.Catalogs.Add( new DirectoryCatalog( extensionFolder ) );

            container = new CompositionContainer( catalog );

            try
            {
                container.ComposeParts( this );

                var services = new SortedDictionary<int, List<Lazy<GeocodeService, IGeocodeServiceData>>>();
                foreach ( Lazy<GeocodeService, IGeocodeServiceData> i in geocodingServices )
                {
                    if ( !services.ContainsKey( i.Value.Order ) )
                        services.Add( i.Value.Order, new List<Lazy<GeocodeService, IGeocodeServiceData>>() );
                    services[i.Value.Order].Add( i );
                }

                int id = 0;
                foreach ( KeyValuePair<int, List<Lazy<GeocodeService, IGeocodeServiceData>>> entry in services )
                    foreach ( Lazy<GeocodeService, IGeocodeServiceData> service in entry.Value )
                        Services.Add(id++, service.Value );
            }
            catch ( CompositionException ex )
            {
            }
        }
    }
}