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
namespace Rock.ViewModels.Blocks.Crm.PersonDetail.GroupMemberNavigation
{
    /// <summary>
    /// A single group member entry in the Family Navigation dropdown.
    /// </summary>
    public class GroupMemberNavigationItemBag
    {
        /// <summary>
        /// Gets or sets the full name of the group member.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the avatar URL of the group member.
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the URL of the group member's profile page, preserving
        /// the subpage the viewer is currently on.
        /// </summary>
        public string PersonProfileUrl { get; set; }
    }
}
