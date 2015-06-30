// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Web.UI.WebControls;

using Rock.Model;

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
        /// Gets or sets the group types.
        /// </summary>
        /// <value>
        /// The group types.
        /// </value>
        public List<GroupType> GroupTypes
        {
            set
            {
                this.Items.Clear();
                this.Items.Add( new ListItem() );
                foreach ( GroupType groupType in value )
                {
                    this.Items.Add( new ListItem( groupType.Name, UseGuidAsValue ? groupType.Guid.ToString() : groupType.Id.ToString() ) );
                }
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
            }
        }
    }
}