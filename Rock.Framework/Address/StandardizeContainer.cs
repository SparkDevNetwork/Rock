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
    public class StandardizeContainer
    {
        private static StandardizeContainer instance;

        public Dictionary<int, Lazy<StandardizeService, IStandardizeServiceData>> Services { get; private set; }

        private CompositionContainer container;

        [ImportMany( typeof( StandardizeService ) )]
        IEnumerable<Lazy<StandardizeService, IStandardizeServiceData>> geocodingServices;

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
            Services = new Dictionary<int, Lazy<StandardizeService, IStandardizeServiceData>>();

            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add( new AssemblyCatalog( this.GetType().Assembly ) );

            string extensionFolder = Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ), "Extensions" );
            if ( Directory.Exists( extensionFolder ) )
                catalog.Catalogs.Add( new DirectoryCatalog( extensionFolder ) );

            container = new CompositionContainer( catalog );

            try
            {
                container.ComposeParts( this );

                var services = new SortedDictionary<int, List<Lazy<StandardizeService, IStandardizeServiceData>>>();
                foreach ( Lazy<StandardizeService, IStandardizeServiceData> i in geocodingServices )
                {
                    if ( !services.ContainsKey( i.Value.Order ) )
                        services.Add( i.Value.Order, new List<Lazy<StandardizeService, IStandardizeServiceData>>() );
                    services[i.Value.Order].Add( i );
                }

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