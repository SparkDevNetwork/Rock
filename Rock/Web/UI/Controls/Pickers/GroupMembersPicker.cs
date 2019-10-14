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
namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class GroupMembersPicker : RockCheckBoxList, IGroupMemberPicker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMembersPicker"/> class.
        /// </summary>
        public GroupMembersPicker()
            : base()
        {
            this.RepeatDirection = System.Web.UI.WebControls.RepeatDirection.Horizontal;
            this.AddCssClass( "checkboxlist-group" );
        }

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
                GroupMemberPicker.LoadDropDownItems( this, false );
            }
        }

        /// <summary>
        /// The group identifier
        /// </summary>
        private int? _groupId;
    }
}