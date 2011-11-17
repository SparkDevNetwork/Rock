using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Rock.Api
{
    public class CustomServiceRoutesSection : ConfigurationSection
    {
        [ConfigurationProperty("ServiceRoutes")]
        public ServiceRoutesConfiguration ServiceRoutes
        {
            get
            {
                return this["ServiceRoutes"] as ServiceRoutesConfiguration;
            }
        }
    }
}