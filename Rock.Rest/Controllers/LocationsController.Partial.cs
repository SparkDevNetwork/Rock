//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Http;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.UI.Controls;

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

            routes.MapHttpRoute(
                name: "LocationsGetChildren",
                routeTemplate: "api/locations/getchildren/{id}/{rootLocationId}",
                defaults: new
                {
                    controller = "locations",
                    action = "getchildren"
                } );
        }

        /// <summary>
        /// Geocode an location
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        [HttpPut]
        [Authenticate]
        public Location Geocode( Location location )
        {
            var user = CurrentUser();
            if ( user != null )
            {
                if ( location != null )
                {
                    var locationService = new LocationService();
                    locationService.Geocode( location, user.PersonId );
                    return location;
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
        public Location Standardize( Location location )
        {
            var user = CurrentUser();
            if ( user != null )
            {
                if ( location != null )
                {
                    var locationService = new LocationService();
                    locationService.Standardize( location, user.PersonId );
                    return location;
                }

                throw new HttpResponseException( HttpStatusCode.BadRequest );
            }

            throw new HttpResponseException( HttpStatusCode.Unauthorized );
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <param name="rootLocationId">The root location unique identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [Authenticate]
        public IQueryable<TreeViewItem> GetChildren( int id, int rootLocationId)
        {
            var user = CurrentUser();
            if ( user != null )
            {
                IQueryable<Location> qry;
                if ( id == 0 )
                {
                    qry = Get().Where( a => a.ParentLocationId == null );
                    if ( rootLocationId != 0 )
                    {
                        qry = qry.Where( a => a.Id == rootLocationId );
                    }
                }
                else
                {
                    qry = Get().Where( a => a.ParentLocationId == id );
                }

                // limit to only Named Locations (don't show home addresses, etc)
                qry = qry.Where( a => a.IsLocation );

                List<Location> locationList = new List<Location>();
                List<TreeViewItem> locationNameList = new List<TreeViewItem>();

                foreach ( var location in qry )
                {
                    if ( location.IsAuthorized( "View", user.Person ) )
                    {
                        locationList.Add( location );
                        var treeViewItem = new TreeViewItem();
                        treeViewItem.Id = location.Id.ToString();
                        treeViewItem.Name = System.Web.HttpUtility.HtmlEncode( location.Name );
                        locationNameList.Add( treeViewItem );
                    }
                }

                // try to quickly figure out which items have Children
                List<int> resultIds = locationList.Select( a => a.Id ).ToList();

                var qryHasChildren = from x in Get().Select( a => a.ParentLocationId )
                                     where resultIds.Contains( x.Value )
                                     select x.Value;

                var qryHasChildrenList = qryHasChildren.ToList();

                foreach ( var item in locationNameList )
                {
                    int locationId = int.Parse( item.Id );
                    item.HasChildren = qryHasChildrenList.Any( a => a == locationId );
                }

                return locationNameList.AsQueryable();
            }
            else
            {
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
            }
        }
    }
}
