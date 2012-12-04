//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Net;
using System.Web.Http;

using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Locations REST API
    /// </summary>
    public partial class LocationsController : IHasCustomRoutes
    {
        /// <summary>
        /// Add custom routes needed for geocoding and standardization
        /// </summary>
        /// <param name="routes"></param>
        public void AddRoutes( System.Web.Routing.RouteCollection routes )
        {
            routes.MapHttpRoute(
                name: "LocationGeocode",
                routeTemplate: "api/locations/geocode",
                defaults: new
                {
                    controller = "locations",
                    action = "geocode"
                } );

            routes.MapHttpRoute(
                name: "LocationStandardize",
                routeTemplate: "api/locations/standardize",
                defaults: new
                {
                    controller = "locations",
                    action = "standardize"
                } );
        }

        /// <summary>
        /// Geocode an location
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        [HttpPut]
        [Authenticate]
        public LocationDto Geocode( LocationDto location )
        {
            var user = CurrentUser();
            if ( user != null )
            {
                if ( location != null )
                {
                    var locationService = new LocationService();
                    return new LocationDto( locationService.Geocode( location, user.PersonId ) );
                }
                throw new HttpResponseException( HttpStatusCode.BadRequest );
            }
            throw new HttpResponseException( HttpStatusCode.Unauthorized );
        }

        /// <summary>
        /// Standardize an location
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        [HttpPut]
        [Authenticate]
        public LocationDto Standardize( LocationDto location )
        {
            var user = CurrentUser();
            if ( user != null )
            {
                if ( location != null )
                {
                    var locationService = new LocationService();
                    return new LocationDto( locationService.Standardize( location, user.PersonId ) );
                }
                throw new HttpResponseException( HttpStatusCode.BadRequest );
            }
            throw new HttpResponseException( HttpStatusCode.Unauthorized );
        }
    }
}
