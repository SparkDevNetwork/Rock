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

using System.Collections.Generic;

using Rock.Model;

namespace Rock.ViewModels.Blocks.Group.GroupPlacement
{
    /// <summary>
    /// Represents a person involved in a placement process, including basic identity details,
    /// photo, and associated registrants or source group memberships.
    /// </summary>
    public class PersonBag
    {
        /// <summary>
        /// The encrypted key for the person’s unique identifier.
        /// </summary>
        public string PersonIdKey { get; set; }

        /// <summary>
        /// The person’s first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The person’s preferred name or nickname.
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// The person’s last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// The gender of the person.
        /// </summary>
        public Gender Gender { get; set; }

        /// <summary>
        /// The URL of the person’s profile photo.
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// A list of registrant records associated with the person, typically used in registration-based placement.
        /// </summary>
        public List<RegistrantBag> Registrants { get; set; }

        /// <summary>
        /// A list of group membership records representing the person’s involvement in source groups.
        /// </summary>
        public List<GroupMemberBag> SourceGroupMembers { get; set; }
    }

}
