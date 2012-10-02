//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Net;
using System.Web.Http;

using Rock.Core;

namespace Rock.Rest.Core
{
	/// <summary>
	/// Attributes REST API
	/// </summary>
	public partial class AttributesController : IHasCustomRoutes
	{
		/// <summary>
		/// Add Custom route for flushing cached attributes
		/// </summary>
		/// <param name="routes"></param>
		public void AddRoutes( System.Web.Routing.RouteCollection routes )
		{
			routes.MapHttpRoute(
				name: "AttributeFlush",
				routeTemplate: "api/attributes/flush/{id}",
				defaults: new
				{
					controller = "attributes",
					action = "flush",
					id = System.Web.Http.RouteParameter.Optional
				} );
		}

		/// <summary>
		/// Flushes an attributes from cache.
		/// </summary>
		[HttpPut]
		public void Flush( int id )
		{
			Rock.Web.Cache.AttributeCache.Flush( id );
		}

		/// <summary>
		/// Flushes all global attributes from cache.
		/// </summary>
		[HttpPut]
		public void Flush()
		{
			Rock.Web.Cache.GlobalAttributesCache.Flush();
		}
	}
}
