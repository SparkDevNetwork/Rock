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
using System.Linq;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control that can be used to select a group member from a particular pre-configured group.
    /// </summary>
    public class GroupMemberPicker : RockDropDownList, IGroupMemberPicker
    {
        /// <summary>
        /// Gets or sets the group identifier ( Required )
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        public int? GroupId
        {
            get
            {
                return _groupId;
            }

            set
            {
                _groupId = value;
                GroupMemberPicker.LoadDropDownItems( this, true );
            }
        }

        /// <summary>
        /// The group identifier
        /// </summary>
        private int? _groupId;

        /// <summary>
        /// Loads the drop down items.
        /// </summary>
        /// <param name="picker">The picker.</param>
        /// <param name="includeEmptyOption">if set to <c>true</c> [include empty option].</param>
        internal static void LoadDropDownItems( IGroupMemberPicker picker, bool includeEmptyOption )
        {
            var selectedItems = picker.Items.Cast<ListItem>()
                .Where( i => i.Selected )
                .Select( i => i.Value ).AsIntegerList();

            picker.Items.Clear();

            if ( picker.GroupId.HasValue )
            {
                if ( includeEmptyOption )
                {
                    // add Empty option first
                    picker.Items.Add( new ListItem() );
                }

                var group = new GroupService( new RockContext() ).Get( picker.GroupId.Value );
                if ( group != null && group.Members.Any() )
                {
                    foreach ( var groupMember in group.Members.OrderBy( m => m.Person.FullName ) )
                    {
                        var li = new ListItem( groupMember.Person.FullName, groupMember.Id.ToString() );
                        li.Selected = selectedItems.Contains( groupMember.Id );
                        picker.Items.Add( li );
                    }
                }
            }
        }
    }

    /// <summary>
    /// Interface used by defined value pickers
    /// </summary>
    public interface IGroupMemberPicker
    {
        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        int? GroupId { get; set; }

        /// <summary>
        /// Gets the items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        ListItemCollection Items { get; }
    }
}