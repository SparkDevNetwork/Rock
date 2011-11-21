using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Rock.MEF
{
    public class Container<T, TMetadata> where T : IClass
    {
        private static Container<T, TMetadata> instance;

        public List<T> Services { get; private set; }

        private CompositionContainer container;

        [ImportMany( typeof( T ) )]
        IEnumerable<Lazy<T, TMetadata>> classes;

        private Container() 
        {
            Refresh();
        }

        public static Container<T, TMetadata> Instance
        {
            get
            {
                if ( instance == null )
                    instance = new Container<T, TMetadata>();
                return instance;
            }
        }

        public void Refresh()
        {
            Services = new List<T>();

            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add( new AssemblyCatalog( this.GetType().Assembly ) );

            string extensionFolder = Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ), "Extensions" );
            if ( Directory.Exists( extensionFolder ) )
                catalog.Catalogs.Add( new DirectoryCatalog( extensionFolder ) );

            container = new CompositionContainer( catalog );

            try
            {
                container.ComposeParts( this );

                var services = new SortedDictionary<int, List<Lazy<T, TMetadata>>>();
                foreach ( Lazy<T, TMetadata> i in classes )
                {
                    if ( !services.ContainsKey( i.Value.Order ) )
                        services.Add( i.Value.Order, new List<Lazy<T, TMetadata>>() );
                    services[i.Value.Order].Add( i );
                }

                foreach ( KeyValuePair<int, List<Lazy<T, TMetadata>>> entry in services )
                    foreach ( Lazy<T, TMetadata> service in entry.Value )
                        Services.Add( service.Value );
            }
            catch ( CompositionException ex )
            {
            }
        }
    }
}