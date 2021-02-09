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
using System.Data.Entity;
using System.Linq;
using org_rocksolidchurch.PageDebug.Model;
using Rock.Attribute;
using Rock.Blocks;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.Blocks;

namespace org_rocksolidchurch.PageDebug.Blocks
{
    /// <summary>
    /// Shows the widgets.
    /// </summary>
    /// <seealso cref="Rock.Blocks.ObsidianBlockType" />

    [DisplayName( "Widget List" )]
    [Category( "Rock Solid Church > Page Debug" )]
    [Description( "Shows a list of widgets" )]
    [IconCssClass( "fa fa-fan" )]

    public class WidgetsList : PluginObsidianBlockType
    {
        /// <summary>
        /// Gets a value indicating whether the block file is written in TypeScript.
        /// If false, then JavaScript.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is type script; otherwise, <c>false</c>.
        /// </value>
        public override bool IsTypeScript => true;

        /// <summary>
        /// Gets the block markup file identifier.
        /// </summary>
        /// <value>
        /// The block markup file identifier.
        /// </value>
        public override string BlockFileUrl => $"/ObsidianJs/Generated/Plugins/org_rocksolidchurch/PageDebug/WidgetsList";

        /// <summary>
        /// Gets the widgets.
        /// </summary>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetWidgets( )
        {
            using ( var rockContext = new RockContext() )
            {
                var service = new PluginWidgetService( rockContext );
                var widgets = service.Queryable().AsNoTracking().ToList();
                return new BlockActionResult( System.Net.HttpStatusCode.OK, widgets );
            }
        }
    }
}
