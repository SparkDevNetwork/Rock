// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;

using Microsoft.AspNet.OData;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Locations REST API
    /// </summary>
    public partial class LocationsController
    {
        /// <summary>
        /// Geocode an location
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpPut]
        [System.Web.Http.Route( "api/locations/verify" )]
        [Rock.SystemGuid.RestActionGuid( "C88F7B3F-6B35-48CA-A3AE-E12F4928A86F" )]
        public Location Verify( Location location )
        {
            if ( location != null )
            {
                ( ( LocationService ) Service ).Verify( location, false );
                return location;
            }

            throw new HttpResponseException( HttpStatusCode.BadRequest );
        }

        /// <summary>
        /// Gets the mapCoordinate from postalcode.
        /// </summary>
        /// <param name="postalCode">The postalcode.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/locations/postalcodetomapcoordinate/{postalCode}" )]
        [Rock.SystemGuid.RestActionGuid( "F83113C8-EBC3-44F2-8DB3-26B035DA4E2F" )]
        public MapCoordinate GetMapCoordinateFromPostalCode( string postalCode )
        {
            return ( ( LocationService ) Service ).GetMapCoordinateFromPostalCode( postalCode );
        }

        /// <summary>
        /// Gets the specified street.
        /// </summary>
        /// <param name="street">The street.</param>
        /// <param name="city">The city.</param>
        /// <param name="state">The state.</param>
        /// <param name="postalCode">The postal code.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [Authenticate, Secured]
        [RockEnableQuery]
        [HttpGet]
        [System.Web.Http.Route( "api/locations/{street}/{city}/{state}/{postalCode}" )]
        [Rock.SystemGuid.RestActionGuid( "94A50E83-5801-42ED-A3DB-0133353E4451" )]
        public Location Get( string street, string city, string state, string postalCode )
        {
            string street2 = string.Empty;
            string country = GlobalAttributesCache.Get().OrganizationCountry;

            // Get a new location record for the address
            var location = ( ( LocationService ) Service ).Get( street, street2, city, state, postalCode, country );
            if ( location == null )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            return location;
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <param name="rootLocationId">The root location unique identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/locations/getchildren/{id}/{rootLocationId}" )]
        [Rock.SystemGuid.RestActionGuid( "179B09BE-63C4-4487-98A5-83EDF94D083E" )]
        public IQueryable<TreeViewItem> GetChildren( int id, int rootLocationId )
        {
            IQueryable<Location> qry;
            if ( id == 0 )
            {
                if ( rootLocationId != 0 )
                {
                    // ignore ParentLocationId when RootLocationId is supplied.
                    qry = Get().Where( a => a.Id == rootLocationId );
                }
                else
                {
                    // continue looking for top-level named locations when RootLocationId is not supplied.
                    qry = Get().Where( a => a.ParentLocationId == null );
                }
            }
            else
            {
                // look for child locations when a LocationId has been supplied.
                qry = Get().Where( a => a.ParentLocationId == id );
            }

            // limit to only Named Locations (don't show home addresses, etc)
            qry = qry.Where( a => a.Name != null && a.Name != string.Empty );

            List<Location> locationList = new List<Location>();
            List<TreeViewItem> locationNameList = new List<TreeViewItem>();

            var person = GetPerson();

            foreach ( var location in qry.OrderBy( l => l.Name ) )
            {
                if ( location.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
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

            var qryHasChildren = Get()
                .Where( l =>
                    l.ParentLocationId.HasValue &&
                    resultIds.Contains( l.ParentLocationId.Value ) )
                .Select( l => l.ParentLocationId.Value )
                .Distinct()
                .ToList();

            var qryHasChildrenList = qryHasChildren.ToList();

            foreach ( var item in locationNameList )
            {
                int locationId = int.Parse( item.Id );
                item.HasChildren = qryHasChildrenList.Any( a => a == locationId );
            }

            return locationNameList.AsQueryable();
        }

        /// <summary>
        /// Gets the children, excluding inactive items.
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <param name="rootLocationId">The root location unique identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/locations/getactivechildren/{id}/{rootLocationId}" )]
        [Rock.SystemGuid.RestActionGuid( "CBB50308-EE20-4540-9492-A27B45BB9610" )]
        public IQueryable<TreeViewItem> GetActiveChildren( int id, int rootLocationId )
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

            // limit to only active locations.
            qry = qry.Where( a => a.IsActive );

            // limit to only Named Locations (don't show home addresses, etc)
            qry = qry.Where( a => a.Name != null && a.Name != string.Empty );

            List<Location> locationList = new List<Location>();
            List<TreeViewItem> locationNameList = new List<TreeViewItem>();

            var person = GetPerson();

            foreach ( var location in qry.OrderBy( l => l.Name ) )
            {
                if ( location.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
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

            var qryHasChildren = Get()
                .Where( l =>
                    l.ParentLocationId.HasValue &&
                    resultIds.Contains( l.ParentLocationId.Value ) &&
                    l.IsActive
                )
                .Select( l => l.ParentLocationId.Value )
                .Distinct()
                .ToList();

            var qryHasChildrenList = qryHasChildren.ToList();

            foreach ( var item in locationNameList )
            {
                int locationId = int.Parse( item.Id );
                item.HasChildren = qryHasChildrenList.Any( a => a == locationId );
            }

            return locationNameList.AsQueryable();
        }
    }
}