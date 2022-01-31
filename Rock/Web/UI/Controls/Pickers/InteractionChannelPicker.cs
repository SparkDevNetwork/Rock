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
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control that can be used to select an interaction channel.
    /// </summary>
    public class InteractionChannelPicker : RockDropDownList, IInteractionChannelPicker
    {
        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack && Items.Count == 0 )
            {
                LoadDropDownItems( this, true );
            }
        }

        /// <summary>
        /// Loads the drop down items.
        /// </summary>
        /// <param name="picker">The picker.</param>
        /// <param name="includeEmptyOption">if set to <c>true</c> [include empty option].</param>
        public static void LoadDropDownItems( IInteractionChannelPicker picker, bool includeEmptyOption )
        {
            var selectedItems = picker.Items.Cast<ListItem>()
                .Where( i => i.Selected )
                .Select( i => i.Value )
                .AsIntegerList();

            picker.Items.Clear();

            if ( includeEmptyOption )
            {
                // add Empty option first
                picker.Items.Add( new ListItem() );
            }

            var rockContext = new RockContext();
            var interactionChannelService = new InteractionChannelService( rockContext );
            var channels = interactionChannelService.Queryable().AsNoTracking()
                .Include( "ChannelTypeMediumValue" )
                .Where( ic => ic.IsActive )
                .OrderBy( ic => ic.Name )
                .ToList();

            foreach ( var channel in channels )
            {
                var li = new ListItem( $"{channel.Name} ({(channel.ChannelTypeMediumValue != null ? channel.ChannelTypeMediumValue.Value : string.Empty )})", channel.Id.ToString() );
                li.Selected = selectedItems.Contains( channel.Id );
                picker.Items.Add( li );
            }
        }
    }

    /// <summary>
    /// Interface used by defined value pickers
    /// </summary>
    public interface IInteractionChannelPicker
    {
        /// <summary>
        /// Gets the items.
        /// </summary>
        ListItemCollection Items { get; }
    }
}