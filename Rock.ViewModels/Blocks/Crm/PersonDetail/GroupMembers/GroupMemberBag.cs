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

namespace Rock.ViewModels.Blocks.Crm.PersonDetail.GroupMembers
{
    /// <summary>
    /// Describes one group member tile displayed by the Group Members block.
    /// </summary>
    public class GroupMemberBag
    {
        /// <summary>
        /// Gets or sets the IdKey identifier of the member's person record,
        /// used to build the person profile link.
        /// </summary>
        public string PersonIdKey { get; set; }

        /// <summary>
        /// Gets or sets the display name of the member.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the age of the member in years.
        /// </summary>
        public int? Age { get; set; }

        /// <summary>
        /// Gets or sets the URL of the member's photo.
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the member is deceased.
        /// </summary>
        public bool IsDeceased { get; set; }
    }
}
