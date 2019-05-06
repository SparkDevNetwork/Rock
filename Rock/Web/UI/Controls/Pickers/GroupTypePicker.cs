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
    /// 
    /// </summary>
    public class GroupTypePicker : RockDropDownList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupTypePicker" /> class.
        /// </summary>
        public GroupTypePicker()
            : base()
        {
            Label = "Group Type";
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
            this.Items.Add( new ListItem() );
            foreach ( GroupTypeCache groupType in groupTypes )
            {
                this.Items.Add( new ListItem( groupType.Name, UseGuidAsValue ? groupType.Guid.ToString() : groupType.Id.ToString() ) );
            }
        }

        /// <summary>
        /// Gets or sets the selected groupType Guid (only works when UseGuidAsValue = true) 
        /// </summary>
        /// <value>
        /// The selected groupType guids.
        /// </value>
        public Guid? SelectedGroupTypeGuid
        {
            get
            {
                return this.SelectedValue.AsGuidOrNull();
            }

            set
            {
                string itemValue = value.HasValue ? value.ToString() : string.Empty;
                var li = this.Items.FindByValue( itemValue );
                if ( li != null )
                {
                    li.Selected = true;
                    this.SelectedValue = itemValue;
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected groupType ids  (only works when UseGuidAsValue = false)
        /// </summary>
        /// <value>
        /// The selected groupType ids.
        /// </value>
        public int? SelectedGroupTypeId
        {
            get
            {
                return this.SelectedValueAsId();
            }

            set
            {
                int id = value.HasValue ? value.Value : 0;
                var li = this.Items.FindByValue( id.ToString() );
                if ( li != null )
                {
                    li.Selected = true;
                    this.SelectedValue = id.ToString();
                }
                else
                {
                    // if setting GroupTypeId to NULL or 0, just default to the first item in the list (which should be nothing)
                    if ( this.Items.Count > 0 )
                    {
                        this.SelectedIndex = 0;
                    }
                }
            }
        }
    }
}