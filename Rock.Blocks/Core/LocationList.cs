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
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.LocationList;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a list of locations.
    /// </summary>

    [DisplayName( "Location List" )]
    [Category( "Core" )]
    [Description( "Displays a list of locations." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the location details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "62622112-8375-44cf-957b-2b8fb4922c2b" )]
    [Rock.SystemGuid.BlockTypeGuid( "8a5af4f4-32a2-426f-8363-57ac4f02a6f6" )]
    [CustomizedGrid]
    public class LocationList : RockEntityListBlockType<Location>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PreferenceKey
        {
            public const string FilterStreetAddress = "filter-street-address";
            public const string FilterCity = "filter-city";
            public const string FilterNotGeocoded = "filter-not-geocoded";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Get the street address filter to use to filter grid rows.
        /// </summary>
        /// <value>
        /// The value to filter locations on based on their street address
        /// </value>
        protected string FilterStreetAddress => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterStreetAddress ) ?? string.Empty;

        /// <summary>
        /// Get the city filter to use to filter grid rows.
        /// </summary>
        /// <value>
        /// The value to filter locations on based on their city
        /// </value>
        protected string FilterCity => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterCity ) ?? string.Empty;

        /// <summary>
        /// Get whether or not to filter the grid rows by whether or not they
        /// have a geo code location.
        /// </summary>
        /// <value>
        /// Whether or not to filter locations based on whether or not it has a geo code.
        /// </value>
        protected bool FilterNotGeocoded => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterNotGeocoded ).AsBoolean();

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<LocationListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = false;
            box.ExpectedRowCount = 1;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private LocationListOptionsBag GetBoxOptions()
        {
            var options = new LocationListOptionsBag();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            //var entity = new Location();

            //return entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
            return false;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "LocationId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<Location> GetListQueryable( RockContext rockContext )
        {
            bool isFiltered = false;
            var queryable = base.GetListQueryable( rockContext ).Where( l => l.Street1 != null && l.Street1 != string.Empty );

            if ( !string.IsNullOrWhiteSpace( FilterStreetAddress ) )
            {
                queryable = queryable.Where( l => l.Street1.StartsWith( FilterStreetAddress ) );
                isFiltered = true;
            }

            if ( !string.IsNullOrWhiteSpace( FilterCity ) )
            {
                queryable = queryable.Where( l => l.City.StartsWith( FilterCity ) );
                isFiltered = true;
            }

            if ( FilterNotGeocoded )
            {
                queryable = queryable.Where( l => l.GeoPoint == null );
                isFiltered = true;
            }

            if ( !isFiltered )
            {
                return Enumerable.Empty<Location>().AsQueryable();
            }
            else
            {
                return queryable;
            }
        }

        /// <inheritdoc/>
        protected override GridBuilder<Location> GetGridBuilder()
        {
            return new GridBuilder<Location>()
                .WithBlock( this )
                .AddTextField( "id", a => a.Id.ToString() )
                .AddTextField( "street1", a => a.Street1 )
                .AddTextField( "city", a => a.City )
                .AddTextField( "state", a => a.State )
                .AddTextField( "postalCode", a => a.PostalCode )
                .AddTextField( "country", a => a.Country )
                .AddField( "isStandardized", a => a.StandardizedDateTime.HasValue );
        }

        #endregion

        #region Block Actions

        #endregion
    }
}
