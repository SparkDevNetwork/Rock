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
    public class StandardizeContainer
    {
        private static StandardizeContainer instance;

        public List<IStandardizeService> Services { get; private set; }

        private CompositionContainer container;

        [ImportMany( typeof( IStandardizeService ) )]
        IEnumerable<Lazy<IStandardizeService, IStandardizeServiceData>> geocodingServices;

        private StandardizeContainer() 
        {
            Refresh();
        }

        public static StandardizeContainer Instance
        {
            get
            {
                if ( instance == null )
                    instance = new StandardizeContainer();
                return instance;
            }
        }

        public void Refresh()
        {
            Services = new List<IStandardizeService>();

            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add( new AssemblyCatalog( typeof( ServiceHelper ).Assembly ) );

            string extensionFolder = Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ), "Extensions" );
            if ( Directory.Exists( extensionFolder ) )
                catalog.Catalogs.Add( new DirectoryCatalog( extensionFolder ) );

            container = new CompositionContainer( catalog );

            try
            {
                container.ComposeParts( this );

                var services = new SortedDictionary<int, List<Lazy<IStandardizeService, IStandardizeServiceData>>>();
                foreach ( Lazy<IStandardizeService, IStandardizeServiceData> i in geocodingServices )
                {
                    if ( !services.ContainsKey( i.Value.Order ) )
                        services.Add( i.Value.Order, new List<Lazy<IStandardizeService, IStandardizeServiceData>>() );
                    services[i.Value.Order].Add( i );
                }

                foreach ( KeyValuePair<int, List<Lazy<IStandardizeService, IStandardizeServiceData>>> entry in services )
                    foreach ( Lazy<IStandardizeService, IStandardizeServiceData> service in entry.Value )
                        Services.Add( service.Value );
            }
            catch ( CompositionException ex )
            {
            }
        }
    }
}