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
using com_rocksolidchurchdemo.PageDebug.Model;
using com_rocksolidchurchdemo.PageDebug.ViewModel;
using Rock.Attribute;
using Rock.Blocks;
using Rock.Data;
using Rock.Model;
using Rock.ViewModel;

namespace com_rocksolidchurchdemo.PageDebug.Blocks
{
    /// <summary>
    /// Shows the widgets.
    /// </summary>
    /// <seealso cref="RockObsidianBlockType" />

    [DisplayName( "Widget List" )]
    [Category( "Rock Solid Church Demo > Page Debug" )]
    [Description( "Shows a list of widgets" )]
    [IconCssClass( "fa fa-fan" )]

    public class WidgetsList : RockObsidianBlockType
    {
        /// <summary>
        /// Gets the block markup file identifier.
        /// </summary>
        /// <value>
        /// The block markup file identifier.
        /// </value>
        public override string BlockFileUrl => $"/Plugins/com_rocksolidchurchdemo/PageDebug/WidgetsList";

        /// <summary>
        /// Gets the widgets.
        /// </summary>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetWidgets( )
        {
            var currentPerson = GetCurrentPerson();

            using ( var rockContext = new RockContext() )
            {
                var service = new WidgetService( rockContext );
                var widgets = service.Queryable().AsNoTracking().ToList();

                var viewModelHelper = new ViewModelHelper<Widget, PluginWidgetViewModel>();
                var viewModels = widgets.Select( w => viewModelHelper.CreateViewModel( w, currentPerson, false ) );

                return new BlockActionResult( System.Net.HttpStatusCode.OK, viewModels );
            }
        }
    }
}
