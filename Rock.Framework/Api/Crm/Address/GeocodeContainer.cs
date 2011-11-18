using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Rock.Api.Crm.Address
{
    public class GeocodeContainer
    {
        private static GeocodeContainer instance;

        public List<IGeocodeService> Services { get; private set; }

        private CompositionContainer container;

        [ImportMany( typeof( IGeocodeService ) )]
        IEnumerable<Lazy<IGeocodeService, IGeocodeServiceData>> geocodingServices;

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
            Services = new List<IGeocodeService>();

            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add( new AssemblyCatalog( typeof( ServiceHelper ).Assembly ) );

            string extensionFolder = Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ), "Extensions" );
            if ( Directory.Exists( extensionFolder ) )
                catalog.Catalogs.Add( new DirectoryCatalog( extensionFolder ) );

            container = new CompositionContainer( catalog );

            try
            {
                container.ComposeParts( this );

                var services = new SortedDictionary<int, List<Lazy<IGeocodeService, IGeocodeServiceData>>>();
                foreach ( Lazy<IGeocodeService, IGeocodeServiceData> i in geocodingServices )
                {
                    if ( !services.ContainsKey( i.Value.Order ) )
                        services.Add( i.Value.Order, new List<Lazy<IGeocodeService, IGeocodeServiceData>>() );
                    services[i.Value.Order].Add( i );
                }

                foreach ( KeyValuePair<int, List<Lazy<IGeocodeService, IGeocodeServiceData>>> entry in services )
                    foreach ( Lazy<IGeocodeService, IGeocodeServiceData> service in entry.Value )
                        Services.Add( service.Value );
            }
            catch ( CompositionException ex )
            {
            }
        }
    }
}