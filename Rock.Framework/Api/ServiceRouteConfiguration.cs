using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Rock.Api
{
    public class ServiceRouteConfiguration : ConfigurationElement
    {
        [ConfigurationProperty("route", IsRequired = true)]
        public string Route
        {
            get
            {
                return this["route"] as string;
            }
        }

        [ConfigurationProperty( "type", IsRequired = true )]
        public string Type
        {
            get
            {
                return this["type"] as string;
            }
        }

    }
}