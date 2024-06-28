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

using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
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
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "415d1413-b210-496a-826e-1a4aee81f78b" )]
    [Rock.SystemGuid.BlockTypeGuid( "74f85b71-f5d3-47d8-8d0f-a0dc858fe870" )]
    [CustomizedGrid]
    public class WebFarmNodeLogList : RockEntityListBlockType<WebFarmNodeLog>
    {
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
            var options = new WebFarmNodeLogListOptionsBag();

            return options;
        }

        /// <inheritdoc/>
        protected override IQueryable<WebFarmNodeLog> GetListQueryable( RockContext rockContext )
        {
            return base.GetListQueryable( rockContext ).OrderByDescending( l => l.EventDateTime );
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
