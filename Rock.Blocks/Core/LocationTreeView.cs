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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.LocationTreeView;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a navigation tree for named locations. Selecting a node navigates to the
    /// configured Detail Page (or reloads the current page) with the selection and expanded
    /// nodes on the query string so sibling blocks read them as page parameters.
    /// </summary>
    [DisplayName( "Location Tree View" )]
    [Category( "Core" )]
    [Description( "Creates a navigation tree for named locations." )]
    [IconCssClass( "ti ti-list-tree" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        IsRequired = false,
        Order = 1 )]

    [TextField(
        "Treeview Title",
        Key = AttributeKey.TreeviewTitle,
        Description = "Location Tree View",
        IsRequired = false,
        Order = 2 )]

    #endregion Block Attributes

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Navigation )]
    [Rock.SystemGuid.EntityTypeGuid( "BBFBB62D-C91A-446C-9E6A-7A36DCF1CB47" )]
    [Rock.SystemGuid.BlockTypeGuid( "468B99CE-D276-4D30-84A9-7842933BDBCD" )]
    public class LocationTreeView : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string TreeviewTitle = "TreeviewTitle";
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKey
        {
            public const string LocationId = "LocationId";
            public const string ExpandedIds = "ExpandedIds";
            public const string ParentLocationId = "ParentLocationId";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<LocationTreeViewBag, LocationTreeViewOptionsBag>
            {
                Options = GetBoxOptions(),
                NavigationUrls = GetBoxNavigationUrls()
            };

            box.Bag = GetBag();

            return box;
        }

        /// <summary>
        /// Builds the block's configured settings for the client.
        /// </summary>
        /// <returns>The options bag describing how the tree should be displayed.</returns>
        private LocationTreeViewOptionsBag GetBoxOptions()
        {
            return new LocationTreeViewOptionsBag
            {
                BlockProperties = new LocationTreeViewBlockAttributesBag
                {
                    PanelTitle = GetAttributeValue( AttributeKey.TreeviewTitle )
                }
            };
        }

        /// <summary>
        /// Builds the navigation URLs for the configured linked pages.
        /// </summary>
        /// <returns>A map of navigation key to URL.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage )
            };
        }

        /// <summary>
        /// Builds the runtime data for the client: selection, expansion, auth, and auto-select URL.
        /// </summary>
        /// <returns>The populated runtime bag.</returns>
        private LocationTreeViewBag GetBag()
        {
            var canEditBlock = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            var bag = new LocationTreeViewBag
            {
                SelectedLocationGuids = new List<Guid>(),
                ExpandedLocationGuids = new List<Guid>(),
                IsAddLocationVisible = canEditBlock,
                IsAddRootEnabled = canEditBlock,
                IsAddChildEnabled = false
            };

            var locationKey = RequestContext.GetPageParameter( PageParameterKey.LocationId );
            NamedLocationCache selectedLocation = null;

            if ( locationKey.IsNotNullOrWhiteSpace() && locationKey != "0" )
            {
                selectedLocation = NamedLocationCache.Get( locationKey, !PageCache.Layout.Site.DisablePredictableIds );
            }

            if ( selectedLocation != null )
            {
                bag.SelectedLocationGuids.Add( selectedLocation.Guid );
                AddAncestorLocationGuids( selectedLocation, bag.ExpandedLocationGuids );

                bag.IsAddChildEnabled = canEditBlock;

                /*
                    7/22/26 - NA

                    Location entities support per-entity security. However, this block currently 
                    mirrors the WebForms block.  That means the '(+) Add' (new location) button
                    is only based on the block's CMS EDIT authorization only. Therefore a person with
                    entity EDIT on the selected location but no block EDIT will not see the '(+) Add'
                    button.

                    If we need to change this to support entity authorization checking, we can change:
                       if !canEditBlock and selectedLocation.IsAuthorized( EDIT, CurrentPerson )
                    is true, set IsAddChildEnabled = true and IsAddLocationVisible = true.

                    GroupTreeView.ApplyAddChildAuthorization is the reference pattern (it
                    also falls back to child-GroupType EDIT); CategoryTreeView does the
                    same elevation at the tail of GetBoxBag.

                    Reason: Deferred until after the WebForms A/B rollout to preserve parity.
                */
            }

            var expandedValue = RequestContext.GetPageParameter( PageParameterKey.ExpandedIds );
            if ( expandedValue.IsNotNullOrWhiteSpace() )
            {
                foreach ( var key in expandedValue.SplitDelimitedValues() )
                {
                    var expandedLocation = NamedLocationCache.Get( key, !PageCache.Layout.Site.DisablePredictableIds );
                    if ( expandedLocation != null && !bag.ExpandedLocationGuids.Contains( expandedLocation.Guid ) )
                    {
                        bag.ExpandedLocationGuids.Add( expandedLocation.Guid );
                    }
                }
            }

            // Auto-select the first named top-level location only when the page has no LocationId.
            // Server-side redirect so the page loads once with the first location selected instead of
            // rendering the unselected tree and then doing a second client-side navigation.
            if ( selectedLocation == null && locationKey.IsNullOrWhiteSpace() )
            {
                var firstLocation = FindFirstLocation();
                if ( firstLocation != null )
                {
                    var autoSelectUrl = GetNavigationUrl( firstLocation.Guid, Guid.Empty, bag.ExpandedLocationGuids, out _, forceCurrentPage: true );
                    if ( autoSelectUrl.IsNotNullOrWhiteSpace() )
                    {
                        RequestContext.Response.RedirectToUrl( autoSelectUrl );
                        bag.AutoSelectUrl = autoSelectUrl;
                    }
                }
            }

            return bag;
        }

        /// <summary>
        /// Builds the navigate-mode URL for a selected (or to-be-added) location.
        /// </summary>
        /// <param name="locationGuid">The selected location, or an empty Guid when adding.</param>
        /// <param name="parentGuid">The parent location, used when adding a child.</param>
        /// <param name="expandedGuids">The locations currently expanded in the tree.</param>
        /// <param name="error">When this returns, indicates whether the URL could not be built.</param>
        /// <param name="forceCurrentPage">
        /// When <c>true</c>, always builds the current-page URL and ignores the Detail Page.
        /// Used for auto-select so arriving at the tree page never redirects to a separate
        /// Detail Page on load; only a deliberate selection navigates there.
        /// </param>
        /// <returns>The Detail Page URL (or current-page URL) with the page parameters applied.</returns>
        private string GetNavigationUrl( Guid locationGuid, Guid parentGuid, List<Guid> expandedGuids, out ErrorPouch error, bool forceCurrentPage = false )
        {
            error = new ErrorPouch();
            expandedGuids = expandedGuids ?? new List<Guid>();

            var qryParams = new Dictionary<string, string>();

            if ( locationGuid == Guid.Empty )
            {
                // An add action targets a new location; the Detail Page treats LocationId=0 as "new".
                qryParams[PageParameterKey.LocationId] = "0";
            }
            else
            {
                var location = NamedLocationCache.Get( locationGuid );
                if ( location == null )
                {
                    error = new ErrorPouch { IsError = true, Message = "The selected location could not be found." };
                    return string.Empty;
                }

                qryParams[PageParameterKey.LocationId] = location.IdKey;
            }

            if ( parentGuid != Guid.Empty )
            {
                var parentLocation = NamedLocationCache.Get( parentGuid );
                if ( parentLocation != null )
                {
                    qryParams[PageParameterKey.ParentLocationId] = parentLocation.IdKey;
                }
            }
            else if ( locationGuid == Guid.Empty )
            {
                // Add Top-Level parents at the root; the Detail Page treats ParentLocationId=0 as "no parent".
                qryParams[PageParameterKey.ParentLocationId] = "0";
            }

            var expandedIds = expandedGuids
                .Select( guid => NamedLocationCache.Get( guid )?.IdKey )
                .Where( idKey => idKey.IsNotNullOrWhiteSpace() )
                .Distinct()
                .ToList();

            if ( expandedIds.Any() )
            {
                qryParams[PageParameterKey.ExpandedIds] = string.Join( ",", expandedIds );
            }

            var detailPageReference = new PageReference( GetAttributeValue( AttributeKey.DetailPage ) );
            if ( forceCurrentPage || detailPageReference.PageId <= 0 || detailPageReference.PageId == PageCache.Id )
            {
                return this.GetCurrentPageUrl( qryParams );
            }

            return this.GetLinkedPageUrl( AttributeKey.DetailPage, qryParams );
        }

        /// <summary>
        /// Finds the first named top-level location the current person can view, ordered by name.
        /// </summary>
        /// <returns>The first authorized location, or null when none is found.</returns>
        private Rock.Model.Location FindFirstLocation()
        {
            var locationQuery = new LocationService( RockContext ).Queryable()
                .Where( l =>
                    l.Name != null &&
                    l.Name != string.Empty &&
                    !l.ParentLocationId.HasValue )
                .OrderBy( l => l.Name );

            foreach ( var location in locationQuery )
            {
                if ( location.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                {
                    return location;
                }
            }

            return null;
        }

        /// <summary>
        /// Walks the ancestor chain of the given location and adds each ancestor's Guid to the
        /// expanded set so a deep-link selection opens the tree far enough to reveal it. Guards
        /// against recursive parent loops.
        /// </summary>
        /// <param name="location">The selected location whose ancestors should be expanded.</param>
        /// <param name="expandedGuids">The expanded-location set to add the ancestors to.</param>
        private static void AddAncestorLocationGuids( NamedLocationCache location, List<Guid> expandedGuids )
        {
            if ( location == null )
            {
                return;
            }

            var visited = new HashSet<Guid>();
            var ancestor = location.ParentLocation;

            while ( ancestor != null && visited.Add( ancestor.Guid ) )
            {
                if ( !expandedGuids.Contains( ancestor.Guid ) )
                {
                    expandedGuids.Add( ancestor.Guid );
                }

                ancestor = ancestor.ParentLocation;
            }
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Builds the navigate-mode URL for a location selection or add action.
        /// </summary>
        /// <param name="locationGuid">The selected location, or an empty Guid when adding.</param>
        /// <param name="parentGuid">The parent location, used when adding a child.</param>
        /// <param name="expandedGuids">The locations currently expanded in the tree.</param>
        /// <returns>The navigation URL, or a bad-request result when it could not be built.</returns>
        [BlockAction]
        public BlockActionResult GetNavigationUrl( Guid locationGuid, Guid parentGuid, List<Guid> expandedGuids )
        {
            var url = GetNavigationUrl( locationGuid, parentGuid, expandedGuids, out var error );

            if ( url.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( error.IsError ? error.Message : "Could not determine the navigation URL for the provided location." );
            }

            return ActionOk( url );
        }

        #endregion Block Actions

        #region Helper Classes

        private class ErrorPouch
        {
            public bool IsError { get; set; } = false;

            public string Message { get; set; } = string.Empty;
        }

        #endregion Helper Classes
    }
}
