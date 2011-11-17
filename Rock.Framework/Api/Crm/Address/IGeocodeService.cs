using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.Api.Crm.Address
{
    interface IGeocodeService
    {
        int Order { get; }
        bool Geocode( AddressStub address );
    }

    public interface IGeocodeServiceData
    {
        string ServiceName { get; }
    }
}
