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
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Enums;
using Rock.Enums.WebFarm;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.WebFarm.WebFarmNodeLogList;

namespace Rock.Blocks.WebFarm
{
    /// <summary>
    /// Displays a list of web farm node logs.
    /// </summary>

    [DisplayName( "Web Farm Node Log List" )]
    [Category( "WebFarm" )]
    [Description( "Displays a list of web farm node logs." )]
    [IconCssClass( "fa fa-list" )]
    //[SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "57e8356d-6e59-4f5b-8db9-a274b7a0efd8" )]
    [Rock.SystemGuid.BlockTypeGuid( "6c824483-6624-460b-9dd8-e127b25ca65d" )]
    [CustomizedGrid]
    public class WebFarmNodeLogList : RockEntityListBlockType<WebFarmNodeLog>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string WebFarmNodeId = "WebFarmNodeId";
        }

        private static class PreferenceKey
        {
            public const string FilterDateRange = "filter-date-range";
        }

        #endregion Keys

        #region Properties

        protected string FilterDateRange => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterDateRange );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<WebFarmNodeLogListOptionsBag>();
            var builder = GetGridBuilder();

            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private WebFarmNodeLogListOptionsBag GetBoxOptions()
        {
            var options = new WebFarmNodeLogListOptionsBag()
            {
                SeverityTypes = typeof( WebFarmNodeLog.SeverityLevel ).ToEnumListItemBag(),
                EventTypes = typeof( LogType ).ToEnumListItemBag(),
            };

            return options;
        }

        /// <inheritdoc/>
        protected override IQueryable<WebFarmNodeLog> GetListQueryable( RockContext rockContext )
        {
            var query = base.GetListQueryable( rockContext );

            // Filter by the page parameter
            var nodeId = PageParameter( PageParameterKey.WebFarmNodeId ).AsIntegerOrNull();

            if ( nodeId.HasValue )
            {
                query = query.Where( l => l.WebFarmNodeId == nodeId.Value );
            }

            // Filter by Date Range
            var dateRange = RockDateTimeHelper.CalculateDateRangeFromDelimitedValues( FilterDateRange, RockDateTime.Now );

            if ( dateRange.Start.HasValue )
            {
                query = query.Where( l => l.EventDateTime >= dateRange.Start.Value );
            }

            if ( dateRange.End.HasValue )
            {
                query = query.Where( l => l.EventDateTime < dateRange.End.Value );
            }

            return query.OrderByDescending( l => l.EventDateTime );
        }

        /// <inheritdoc/>
        protected override GridBuilder<WebFarmNodeLog> GetGridBuilder()
        {
            return new GridBuilder<WebFarmNodeLog>()
                .WithBlock( this )
                .AddTextField( "id", a => a.Id.ToString() )
                .AddTextField( "idKey", a => a.IdKey )
                .AddDateTimeField( "logDate", a => a.EventDateTime )
                .AddTextField( "writerNodeName", a => a.WriterWebFarmNode.NodeName )
                .AddTextField( "nodeName", a => a.WebFarmNode.NodeName )
                .AddField( "severity", a => a.Severity )
                .AddField( "type", a => a.EventType )
                .AddTextField( "details", a => a.Message )
                .AddAttributeFields( GetGridAttributes() );
        }

        #endregion
    }
}
