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
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

using Rock.Constants;
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

                if ( !Required )
                {
                    this.Items.Add( new ListItem( string.Empty, string.Empty ) );
                }

                foreach ( GroupType groupType in value )
                {
                    ListItem groupTypeItem = new ListItem();
                    groupTypeItem.Value = groupType.Id.ToString();
                    groupTypeItem.Text = groupType.Name;
                    this.Items.Add( groupTypeItem );
                }

            }
        }

        /// <summary>
        /// Gets the selected groupType ids.
        /// </summary>
        /// <value>
        /// The selected groupType ids.
        /// </value>
        public int? SelectedGroupTypeId
        {
            get
            {
                return this.SelectedValueAsInt();
            }
            set
            {
                int id = value.HasValue ? value.Value : 0;
                var li = this.Items.FindByValue( id.ToString() );
                if ( li != null )
                {
                    li.Selected = true;
                }
            }
        }

    }
}