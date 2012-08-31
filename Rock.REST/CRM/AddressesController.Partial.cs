//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Net;
using System.Web.Http;

using Rock.Crm;

namespace Rock.Rest.Crm
{
	/// <summary>
	/// Addresses REST API
	/// </summary>
	public partial class AddressesController 
		: Rock.Rest.ApiController<Rock.Crm.Address, Rock.Crm.AddressDto>, IHasCustomRoutes
	{
		/// <summary>
		/// Add custom routes needed for geocoding and standardization
		/// </summary>
		/// <param name="routes"></param>
		public void AddRoutes( System.Web.Routing.RouteCollection routes )
		{
			routes.MapHttpRoute(
				name: "AddressGeocode",
				routeTemplate: "api/addresses/geocode",
				defaults: new
				{
					controller = "addresses",
					action = "geocode"
				} );

			routes.MapHttpRoute(
				name: "AddressStandardize",
				routeTemplate: "api/addresses/standardize",
				defaults: new
				{
					controller = "addresses",
					action = "standardize"
				} );
		}

		/// <summary>
		/// Geocode an address
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		[HttpPut]
		public AddressDto Geocode( AddressDto address )
		{
			var user = CurrentUser();
			if ( user != null )
			{
				if ( address != null )
				{
					var addressService = new AddressService();
					return new AddressDto( addressService.Geocode( address, user.PersonId ) );
				}
				throw new HttpResponseException( HttpStatusCode.BadRequest );
			}
			throw new HttpResponseException( HttpStatusCode.Unauthorized );
		}

		/// <summary>
		/// Standardize an address
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		[HttpPut]
		public AddressDto Standardize( AddressDto address )
		{
			var user = CurrentUser();
			if ( user != null )
			{
				if ( address != null )
				{
					var addressService = new AddressService();
					return new AddressDto( addressService.Standardize( address, user.PersonId ) );
				}
				throw new HttpResponseException( HttpStatusCode.BadRequest );
			}
			throw new HttpResponseException( HttpStatusCode.Unauthorized );
		}
	}
}
