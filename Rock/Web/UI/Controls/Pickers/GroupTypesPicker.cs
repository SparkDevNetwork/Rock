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
using System.Linq;
using System.Web.UI.WebControls;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Select multiple group types 
    /// NOTE: GroupTypes must be set first (it doesn't automatically load group types)
    /// </summary>
    public class GroupTypesPicker : RockCheckBoxList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupTypesPicker"/> class.
        /// </summary>
        public GroupTypesPicker()
            : base()
        {
            Label = "Group Types";
        }

        /// <summary>
        /// Gets or sets a value indicating whether to Group.Guid instead of Group.Id for the listitem values (default false)
        /// NOTE: Make sure you set this before setting .GroupTypes
        /// </summary>
        /// <value>
        /// <c>true</c> if [use unique identifier as value]; otherwise, <c>false</c>.
        /// </value>
        public bool UseGuidAsValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether listitem to sorted by name Or by Order then by name (default false)
        /// NOTE: Make sure you set this before setting .GroupTypes
        /// </summary>
        /// <value>
        /// <c>true</c> if [use unique identifier as value]; otherwise, <c>false</c>.
        /// </value>
        public bool IsSortedByName { get; set; }

        /// <summary>
        /// The default number of columns to display in the <see cref="RockCheckBoxList" /> control.
        /// </summary>
        public static readonly int DefaultRepeatColumns = 2;

        /// <summary>
        /// Sets the group types. Note: Use SetGroupTypes instead to set this using List&lt;GroupTypeCache&gt;
        /// </summary>
        /// <value>
        /// The group types.
        /// </value>
        public List<GroupType> GroupTypes
        {
            set
            {
                SetGroupTypes( value.Select( a => GroupTypeCache.Get( a.Id ) ).ToList() );
            }
        }

        /// <summary>
        /// Sets the group types.
        /// </summary>
        /// <param name="groupTypes">The group types.</param>
        public void SetGroupTypes( IEnumerable<GroupTypeCache> groupTypes )
        {
            this.Items.Clear();
            var orderedGroupTypes = IsSortedByName ? groupTypes.OrderBy( a => a.Name ) : groupTypes.OrderBy( a => a.Order ).ThenBy( a => a.Name );
            foreach ( GroupTypeCache groupType in orderedGroupTypes )
            {
                this.Items.Add( new ListItem( groupType.Name, UseGuidAsValue ? groupType.Guid.ToString() : groupType.Id.ToString() ) );
            }
        }

        /// <summary>
        /// Gets the available group type ids.
        /// </summary>
        /// <value>
        /// The available group type ids.
        /// </value>
        public List<int> AvailableGroupTypeIds
        {
            get
            {
                return this.Items.OfType<ListItem>().Select( a => a.Value ).AsIntegerList();
            }
        }

        /// <summary>
        /// Gets or sets the selected group type Guid.
        /// </summary>
        /// <value>The selected group type guids.</value>
        public List<Guid> SelectedGroupTypeGuids
        {
            get
            {
                return this.Items.OfType<ListItem>().Where( l => l.Selected ).Select( a => a.Value ).AsGuidList();
            }

            set
            {
                foreach ( ListItem groupTypeItem in this.Items )
                {
                    groupTypeItem.Selected = value.Exists( a => a.Equals( groupTypeItem.Value.AsGuid() ) );
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected group type ids.
        /// </summary>
        /// <value>The selected group type ids.</value>
        public List<int> SelectedGroupTypeIds
        {
            get
            {
                return this.Items.OfType<ListItem>().Where( l => l.Selected ).Select( a => a.Value ).AsIntegerList();
            }

            set
            {
                foreach ( ListItem groupTypeItem in this.Items )
                {
                    groupTypeItem.Selected = value.Exists( a => a.Equals( groupTypeItem.Value.AsInteger() ) );
                }
            }
        }

        /// <inheritdoc/>
        public override int RepeatColumns
        {
            get
            {
                if ( this.RepeatDirection == RepeatDirection.Horizontal )
                {
                    return _repeatColumns ?? DefaultRepeatColumns;
                }
                else
                {
                    return _repeatColumns ?? 0;
                }
            }

            set
            {
                _repeatColumns = value;
            }
        }

        private int? _repeatColumns;
    }
}