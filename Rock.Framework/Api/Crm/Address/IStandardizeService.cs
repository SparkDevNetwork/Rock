using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.Api.Crm.Address
{
    public interface IStandardizeService : Rock.Attribute.IHasAttributes
    {
        int Order { get; }
        bool Standardize( AddressStub address );
    }

    public interface IStandardizeServiceData
    {
        string ServiceName { get; }
    }
}
