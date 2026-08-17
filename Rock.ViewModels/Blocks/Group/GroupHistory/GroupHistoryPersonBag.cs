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

namespace Rock.ViewModels.Blocks.Group.GroupHistory
{
    /// <summary>
    /// A person referenced by a Group History timeline event, such as a member
    /// that was added to or removed from the group.
    /// </summary>
    public class GroupHistoryPersonBag
    {
        /// <summary>
        /// Gets or sets the person's full name.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the URL of the person's photo, sized for an avatar.
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the IdKey of the person.
        /// </summary>
        public string PersonIdKey { get; set; }

        /// <summary>
        /// Gets or sets the URL of the page that shows this member's history
        /// within the group. Empty when the group member history page is not
        /// configured.
        /// </summary>
        public string MemberHistoryUrl { get; set; }
    }
}
