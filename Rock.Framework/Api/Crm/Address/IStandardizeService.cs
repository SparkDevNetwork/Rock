using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.Api.Crm.Address
{
    interface IStandardizeService
    {
        bool Standardize( AddressStub address );
    }

    public interface IStandardizeServiceData
    {
        string ServiceName { get; }
    }
}
