using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Web;

namespace Rock.Api
{
    public class RockServiceBehavior : IServiceBehavior
    {
        public void AddBindingParameters( 
            ServiceDescription serviceDescription, 
            ServiceHostBase serviceHostBase, 
            System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, 
            System.ServiceModel.Channels.BindingParameterCollection bindingParameters )
        {
            throw new NotImplementedException();
        }

        public void ApplyDispatchBehavior( ServiceDescription serviceDescription, ServiceHostBase serviceHostBase )
        {
            throw new NotImplementedException();
        }

        public void Validate( ServiceDescription serviceDescription, ServiceHostBase serviceHostBase )
        {
            throw new NotImplementedException();
        }
    }
}