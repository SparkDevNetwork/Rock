using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

using Rock.Api.Crm.Address;

using Rock.Cms.Security;

namespace Rock.Api.Crm
{
    [Export( typeof( IService ) )]
    [ExportMetadata( "RouteName", "api/Crm/Address" )]
    [AspNetCompatibilityRequirements( RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed )]
    public partial class AddressService : IAddressService, IService
    {
        private CompositionContainer container;

        [ImportMany( typeof( IGeocodeService ) )]
        IEnumerable<Lazy<IGeocodeService, IGeocodeServiceData>> geocodingServices;

        [ImportMany( typeof( IStandardizeService ) )]
        IEnumerable<Lazy<IStandardizeService, IStandardizeServiceData>> standardizeServices;

        [WebGet( UriTemplate = "{id}" )]
        public AddressStub Get( string id )
        {
            AddressStub address = new AddressStub();
            address.Street1 = "Test Address";
            return address;
        }

        [WebInvoke( Method = "PUT", UriTemplate = "Geocode" )]
        public AddressStub Geocode( AddressStub address )
        {
            try
            {
                if ( address != null )
                {
                    container.ComposeParts( this );

                    var services = new SortedDictionary<int, List<Lazy<IGeocodeService, IGeocodeServiceData>>>();
                    foreach ( Lazy<IGeocodeService, IGeocodeServiceData> i in geocodingServices )
                    {
                        if ( !services.ContainsKey( i.Value.Order ) )
                            services.Add( i.Value.Order, new List<Lazy<IGeocodeService, IGeocodeServiceData>>() );
                        services[i.Value.Order].Add( i );
                    }

                    foreach(KeyValuePair<int, List<Lazy<IGeocodeService, IGeocodeServiceData>>> entry in services)
                        foreach(Lazy<IGeocodeService, IGeocodeServiceData> service in entry.Value)
                            if ( service.Value.Geocode( address ) )
                                return address;

                    address.ResultCode = "No Match";
                    return address;
                }
                else
                    throw new FaultException( "Invalid Address" );
            }
            catch ( CompositionException ex )
            {
                throw new FaultException( "No Geocoding Service Found" );
            }
        }

        [WebInvoke( Method = "PUT", UriTemplate = "Standardize" )]
        public AddressStub Standardize( AddressStub address )
        {
            try
            {
                container.ComposeParts( this );

                foreach ( Lazy<IStandardizeService, IStandardizeServiceData> i in standardizeServices )
                    if ( i.Value.Standardize( address ) )
                        return address;
            }
            catch ( CompositionException ex )
            {
                address.ResultCode = "No Standardization Service Found";
            }

            return address;
        }

        public AddressService()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add( new AssemblyCatalog( typeof( ServiceHelper ).Assembly ) );

            string extensionFolder = Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ), "Extensions" );
            if ( Directory.Exists( extensionFolder ) )
                catalog.Catalogs.Add( new DirectoryCatalog( extensionFolder ) );

            container = new CompositionContainer( catalog );
        }
    }
}
