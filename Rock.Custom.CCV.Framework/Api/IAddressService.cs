using System.ServiceModel;

namespace Rock.Custom.CCV.Api
{
    [ServiceContract]
    public interface IAddressService
    {
        [OperationContract]
        string Geocode( string address );
    }
}
