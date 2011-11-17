using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Rock.Api
{
    public class ServiceRoutesConfiguration : ConfigurationElementCollection
    {
        public ServiceRouteConfiguration this[int index]
        {
            get
            {
                return base.BaseGet( index ) as ServiceRouteConfiguration;
            }

            set
            {
                if ( base.BaseGet( index ) != null )
                    base.BaseRemoveAt( index );
                base.BaseAdd( index, value );
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ServiceRouteConfiguration();
        }

        protected override object GetElementKey( ConfigurationElement element )
        {
            return ( ( ServiceRouteConfiguration )element ).Route;
        }
    }
}