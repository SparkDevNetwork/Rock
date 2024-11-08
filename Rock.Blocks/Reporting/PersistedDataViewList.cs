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
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Reporting.PersistedDataViewList;
using Rock.Web.Cache;

namespace Rock.Blocks.Reporting
{
    /// <summary>
    /// Displays a list of data views.
    /// </summary>
    [DisplayName( "Persisted Data View List" )]
    [Category( "Reporting" )]
    [Description( "Shows a list of Data Views that have persistence enabled." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the data view details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "e1c5fbeb-7e0a-496f-97b2-38e6ec8d5b84" )]
    [Rock.SystemGuid.BlockTypeGuid( "1a46cc61-6110-4022-8ace-efe188a6ab5a" )]
    [CustomizedGrid]
    public class PersistedDataViewList : RockEntityListBlockType<DataView>
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

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<PersistedDataViewListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = false;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private PersistedDataViewListOptionsBag GetBoxOptions()
        {
            var options = new PersistedDataViewListOptionsBag();

            return options;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "PersistedDataViewId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<DataView> GetListQueryable( RockContext rockContext )
        {
            var dataViewService = new DataViewService( rockContext );
            return dataViewService.Queryable().Where( a => a.PersistedScheduleIntervalMinutes.HasValue && a.PersistedLastRefreshDateTime.HasValue ).AsNoTracking();
        }

        /// <inheritdoc/>
        protected override IQueryable<DataView> GetOrderedListQueryable( IQueryable<DataView> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( d => d.Name );
        }

        /// <inheritdoc/>
        protected override GridBuilder<DataView> GetGridBuilder()
        {
            return new GridBuilder<DataView>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "persistedInterval", a => FormatFriendlyMinutesDuration( a.PersistedScheduleIntervalMinutes ) )
                .AddField( "persistedLastRunDurationMilliseconds", a => a.PersistedLastRunDurationMilliseconds )
                .AddField( "runCount", a => a.RunCount )
                .AddDateTimeField( "lastRunDateTime", a => a.LastRunDateTime )
                .AddDateTimeField( "persistedLastRefreshDateTime", a => a.PersistedLastRefreshDateTime );
        }

        /// <summary>
        /// Formats the value as a human friendly duration.
        /// </summary>
        /// <param name="minutesValue">The interval value to convert.</param>
        /// <returns>a human readable value</returns>
        private string FormatFriendlyMinutesDuration( int? minutesValue )
        {
            if ( !minutesValue.HasValue )
            {
                return string.Empty;
            }

            return ( minutesValue * 60 ).ToFriendlyDuration();
        }

        #endregion
    }
}
