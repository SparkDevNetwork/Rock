using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.Api.Crm.Address
{
    public interface IGeocodeService : Rock.Attribute.IHasAttributes
    {
        int Order { get; }
        bool Geocode( AddressStub address );
    }

    public interface IGeocodeServiceData
    {
        string ServiceName { get; }
    }
}
