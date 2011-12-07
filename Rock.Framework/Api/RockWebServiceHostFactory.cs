using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Web;

namespace Rock.Api
{
    public class RockWebServiceHostFactory: WebServiceHostFactory
    {
        protected override System.ServiceModel.ServiceHost CreateServiceHost( Type serviceType, Uri[] baseAddresses )
        {
            ServiceHost serviceHost = new ServiceHost( serviceType, baseAddresses );

            foreach ( Uri address in baseAddresses )
            {
                WSHttpBinding b = new WSHttpBinding( SecurityMode.Message );
                b.Name = serviceType.Name;
                serviceHost.AddServiceEndpoint( serviceType.GetInterfaces()[0], b, address );
            }

            return serviceHost;
        }
    }
}