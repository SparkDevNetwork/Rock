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

using Rock.ClientService.Core.Category;
using Rock.ClientService.Core.Category.Options;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Rest.v2.Options;
using Rock.Security;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http;

namespace Rock.Rest.v2
{
    /// <summary>
    /// Provides API endpoints for the Controls controller.
    /// </summary>
    [RoutePrefix( "api/v2/Controls" )]
    [RockGuid( "815b51f0-b552-47fd-8915-c653eedd5b67" )]
    public class ControlsController : ApiControllerBase
    {
        #region Category Picker

        private static readonly Regex QualifierValueLookupRegex = new Regex( "^{EL:((?:[a-f\\d]{8})-(?:[a-f\\d]{4})-(?:[a-f\\d]{4})-(?:[a-f\\d]{4})-(?:[a-f\\d]{12})):((?:[a-f\\d]{8})-(?:[a-f\\d]{4})-(?:[a-f\\d]{4})-(?:[a-f\\d]{4})-(?:[a-f\\d]{12}))}$", RegexOptions.IgnoreCase );

        /// <summary>
        /// Gets the child items that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A collection of view models that represent the tree items.</returns>
        [HttpPost]
        [System.Web.Http.Route( "CategoryPickerChildTreeItems" )]
        [Authenticate]
        [RockGuid( "a1d07211-6c50-463b-98ed-1622dc4d73dd" )]
        public IHttpActionResult CategoryPickerChildTreeItems( [FromBody] CategoryPickerChildTreeItemsOptions options )
        {
            using ( var rockContext = new RockContext() )
            {
                var clientService = new CategoryClientService( rockContext, GetPerson( rockContext ) );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                var items = clientService.GetCategorizedTreeItems( new CategoryItemTreeOptions
                {
                    ParentGuid = options.ParentGuid,
                    GetCategorizedItems = options.GetCategorizedItems,
                    EntityTypeGuid = options.EntityTypeGuid,
                    EntityTypeQualifierColumn = options.EntityTypeQualifierColumn,
                    EntityTypeQualifierValue = GetQualifierValueLookupResult( options.EntityTypeQualifierValue, rockContext ),
                    IncludeUnnamedEntityItems = options.IncludeUnnamedEntityItems,
                    IncludeCategoriesWithoutChildren = options.IncludeCategoriesWithoutChildren,
                    DefaultIconCssClass = options.DefaultIconCssClass,
                    IncludeInactiveItems = options.IncludeInactiveItems,
                    LazyLoad = options.LazyLoad,
                    SecurityGrant = grant
                } );

                return Ok( items );
            }
        }

        /// <summary>
        /// Checks if the qualifier value is a lookup and if so translate it to the
        /// identifier from the unique identifier. Otherwise returns the original
        /// value.
        /// </summary>
        /// <remarks>
        /// At some point this needs to be moved into a ClientService layer, but
        /// I'm not sure where since it isn't related to any one service.
        /// </remarks>
        /// <param name="value">The value to be translated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The qualifier value to use.</returns>
        private static string GetQualifierValueLookupResult( string value, RockContext rockContext )
        {
            if ( value == null )
            {
                return null;
            }

            var m = QualifierValueLookupRegex.Match( value );

            if ( m.Success )
            {
                int? id = null;

                if ( Guid.TryParse( m.Groups[1].Value, out var g1 ) && Guid.TryParse( m.Groups[2].Value, out var g2 ) )
                {
                    id = Rock.Reflection.GetEntityIdForEntityType( g1, g2, rockContext );
                }

                return id?.ToString() ?? "0";
            }
            else
            {
                return value;
            }
        }

        #endregion

        #region Defined Value Picker

        /// <summary>
        /// Gets the child items that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="options">The options that describe which defined values to load.</param>
        /// <returns>A collection of view models that represent the defined values.</returns>
        [HttpPost]
        [System.Web.Http.Route( "DefinedValuePickerGetDefinedValues" )]
        [Authenticate]
        [RockGuid( "1e4a1812-8a2c-4266-8f39-3004c1debc9f" )]
        public IHttpActionResult DefinedValuePickerGetDefinedValues( DefinedValuePickerGetDefinedValuesOptions options )
        {
            using ( var rockContext = new RockContext() )
            {
                var definedType = DefinedTypeCache.Get( options.DefinedTypeGuid );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                if ( definedType == null || !definedType.IsAuthorized( Rock.Security.Authorization.VIEW, RockRequestContext.CurrentPerson ) )
                {
                    return NotFound();
                }

                var definedValues = definedType.DefinedValues
                    .Where( v => ( v.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) || grant?.IsAccessGranted( v, Authorization.VIEW ) == true )
                        && ( options.IncludeInactive || v.IsActive ) )
                    .OrderBy( v => v.Order )
                    .ThenBy( v => v.Value )
                    .Select( v => new ListItemBag
                    {
                        Value = v.Guid.ToString(),
                        Text = v.Value
                    } )
                    .ToList();

                return Ok( definedValues );
            }
        }

        #endregion

        #region Entity Type Picker

        /// <summary>
        /// Gets the entity types that can be displayed in the entity type picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A collection of view models that represent the tree items.</returns>
        [HttpPost]
        [System.Web.Http.Route( "EntityTypePickerGetEntityTypes" )]
        [Authenticate]
        [RockGuid( "afdd3d40-5856-478b-a41a-0539127f0631" )]
        public IHttpActionResult EntityTypePickerGetEntityTypes( [FromBody] EntityTypePickerGetEntityTypesOptions options )
        {
            using ( var rockContext = new RockContext() )
            {
                var items = EntityTypeCache.All( rockContext )
                    .Where( t => t.IsEntity )
                    .OrderByDescending( t => t.IsCommon )
                    .ThenBy( t => t.FriendlyName )
                    .Select( t => new ListItemBag
                    {
                        Value = t.Guid.ToString(),
                        Text = t.FriendlyName,
                        Category = t.IsCommon ? "Common" : "All Entities"
                    } )
                    .ToList();

                return Ok( items );
            }
        }

        #endregion

        #region Location Picker

        /// <summary>
        /// Gets the child locations, excluding inactive items.
        /// </summary>
        /// <param name="options">The options that describe which child locations to retrieve.</param>
        /// <returns>A collection of <see cref="TreeItemBag"/> objects that represent the child locations.</returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "LocationPickerGetActiveChildren" )]
        [RockGuid( "e57312ec-92a7-464c-aa7e-5320ddfaef3d" )]
        public IHttpActionResult LocationPickerGetActiveChildren( [FromBody] LocationPickerGetActiveChildrenOptions options )
        {
            IQueryable<Location> qry;

            using ( var rockContext = new RockContext() )
            {
                var locationService = new LocationService( rockContext );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                if ( options.Guid == Guid.Empty )
                {
                    qry = locationService.Queryable().AsNoTracking().Where( a => a.ParentLocationId == null );
                    if ( options.RootLocationGuid != Guid.Empty )
                    {
                        qry = qry.Where( a => a.Guid == options.RootLocationGuid );
                    }
                }
                else
                {
                    qry = locationService.Queryable().AsNoTracking().Where( a => a.ParentLocation.Guid == options.Guid );
                }

                // limit to only active locations.
                qry = qry.Where( a => a.IsActive );

                // limit to only Named Locations (don't show home addresses, etc)
                qry = qry.Where( a => a.Name != null && a.Name != string.Empty );

                List<Location> locationList = new List<Location>();
                List<TreeItemBag> locationNameList = new List<TreeItemBag>();

                var person = GetPerson();

                foreach ( var location in qry.OrderBy( l => l.Name ) )
                {
                    if ( location.IsAuthorized( Authorization.VIEW, person ) || grant?.IsAccessGranted( location, Authorization.VIEW ) == true )
                    {
                        locationList.Add( location );
                        var treeViewItem = new TreeItemBag();
                        treeViewItem.Value = location.Guid.ToString();
                        treeViewItem.Text = location.Name;
                        locationNameList.Add( treeViewItem );
                    }
                }

                // try to quickly figure out which items have Children
                List<int> resultIds = locationList.Select( a => a.Id ).ToList();

                var qryHasChildren = locationService.Queryable().AsNoTracking()
                    .Where( l =>
                        l.ParentLocationId.HasValue &&
                        resultIds.Contains( l.ParentLocationId.Value ) &&
                        l.IsActive
                    )
                    .Select( l => l.ParentLocation.Guid )
                    .Distinct()
                    .ToList();

                var qryHasChildrenList = qryHasChildren.ToList();

                foreach ( var item in locationNameList )
                {
                    var locationGuid = item.Value.AsGuid();
                    item.IsFolder = qryHasChildrenList.Any( a => a == locationGuid );
                    item.HasChildren = item.IsFolder;
                }

                return Ok( locationNameList );
            }
        }

        #endregion

        #region Person Picker

        /// <summary>
        /// Searches for people that match the given search options and returns
        /// those matches.
        /// </summary>
        /// <param name="options">The options that describe how the search should be performed.</param>
        /// <returns>A collection of <see cref="Rock.Rest.Controllers.PersonSearchResult"/> objects.</returns>
        [Authenticate]
        [Secured]
        [HttpPost]
        [System.Web.Http.Route( "PersonPickerSearch" )]
        [RockGuid( "1947578d-b28f-4956-8666-dcc8c0f2b945" )]
        public IQueryable<Rock.Rest.Controllers.PersonSearchResult> PersonPickerSearch( [FromBody] PersonPickerSearchOptions options )
        {
            var rockContext = new RockContext();

            // Chain to the v1 controller.
            return Rock.Rest.Controllers.PeopleController.SearchForPeople( rockContext, options.Name, options.Address, options.Phone, options.Email, options.IncludeDetails, options.IncludeBusinesses, options.IncludeDeceased, false );
        }

        #endregion
    }
}
