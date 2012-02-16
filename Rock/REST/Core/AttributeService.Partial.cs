//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;
using System.ServiceModel.Web;

namespace Rock.REST.Core
{
    public partial class AttributeService 
    {
        /// <summary>
        /// Flushes the specified attribute from cache.
        /// </summary>
        /// <param name="id">The id.</param>
        [WebInvoke( Method = "PUT", UriTemplate = "Flush/{id}" )]
        public void Flush( string id )
        {
            int attributeId = 0;
            if (Int32.TryParse(id, out attributeId))
                Rock.Web.Cache.Attribute.Flush( attributeId );
        }

        /// <summary>
        /// Flushes the specified attribute from cache.
        /// </summary>
        /// <param name="id">The id.</param> 
        /// <param name="apiKey">The API key.</param>
        [WebInvoke( Method = "PUT", UriTemplate = "Flush/{id}/{apiKey}" )]
        public void ApiFlush( string id, string apiKey )
        {
            using ( Rock.Data.UnitOfWorkScope uow = new Rock.Data.UnitOfWorkScope() )
            {
				Rock.CMS.UserService userService = new Rock.CMS.UserService();
                Rock.CMS.User user = userService.Queryable().Where( u => u.ApiKey == apiKey ).FirstOrDefault();

                if ( user != null )
                    Flush(id);
                else
                    throw new WebFaultException<string>( "Invalid API Key", System.Net.HttpStatusCode.Forbidden );
            }
        }

        /// <summary>
        /// Flushes the global attributes from cache.
        /// </summary>
        [WebInvoke( Method = "PUT", UriTemplate = "FlushGlobal" )]
        public void FlushGlobal( )
        {
            Rock.Web.Cache.GlobalAttributes.Flush();
        }

        /// <summary>
        /// Flushes the global attributes from cache.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        [WebInvoke( Method = "PUT", UriTemplate = "FlushGlobal/{apiKey}" )]
        public void ApiFlushGlobal( string apiKey )
        {
            using ( Rock.Data.UnitOfWorkScope uow = new Rock.Data.UnitOfWorkScope() )
            {
                Rock.CMS.UserService userService = new Rock.CMS.UserService();
                Rock.CMS.User user = userService.Queryable().Where( u => u.ApiKey == apiKey ).FirstOrDefault();

                if ( user != null )
                    FlushGlobal();
                else
                    throw new WebFaultException<string>( "Invalid API Key", System.Net.HttpStatusCode.Forbidden );
            }
        }
    }
}
