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
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control that can be used to select an interaction component from a particular pre-configured interaction channel.
    /// </summary>
    public class InteractionComponentPicker : RockDropDownList, IInteractionComponentPicker
    {
        /// <summary>
        /// Gets or sets the interaction component identifier.
        /// </summary>
        public int? InteractionChannelId
        {
            get
            {
                return _interactionChannelId;
            }

            set
            {
                _interactionChannelId = value;
                LoadDropDownItems( this, true );
            }
        }

        /// <summary>
        /// The interaction channel identifier
        /// </summary>
        private int? _interactionChannelId;

        /// <summary>
        /// Loads the drop down items.
        /// </summary>
        /// <param name="picker">The picker.</param>
        /// <param name="includeEmptyOption">if set to <c>true</c> [include empty option].</param>
        internal static void LoadDropDownItems( IInteractionComponentPicker picker, bool includeEmptyOption )
        {
            var selectedItems = picker.Items.Cast<ListItem>()
                .Where( i => i.Selected )
                .Select( i => i.Value )
                .AsIntegerList();

            picker.Items.Clear();

            if ( !picker.InteractionChannelId.HasValue )
            {
                return;
            }

            if ( includeEmptyOption )
            {
                // add Empty option first
                picker.Items.Add( new ListItem() );
            }

            var rockContext = new RockContext();
            var interactionComponentService = new InteractionComponentService( rockContext );
            var components = interactionComponentService.Queryable().AsNoTracking()
                .Where( ic => ic.InteractionChannelId == picker.InteractionChannelId.Value )
                .OrderBy( ic => ic.Name )
                .ToList();

            foreach ( var component in components )
            {
                var li = new ListItem( component.Name, component.Id.ToString() );
                li.Selected = selectedItems.Contains( component.Id );
                picker.Items.Add( li );
            }
        }
    }

    /// <summary>
    /// Interface used by defined value pickers
    /// </summary>
    public interface IInteractionComponentPicker
    {
        /// <summary>
        /// Gets or sets the interaction component identifier.
        /// </summary>
        int? InteractionChannelId { get; set; }

        /// <summary>
        /// Gets the items.
        /// </summary>
        ListItemCollection Items { get; }
    }
}