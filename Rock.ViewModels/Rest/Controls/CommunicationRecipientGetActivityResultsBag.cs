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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// A bag that contains activity information for a communication recipient.
    /// </summary>
    public class CommunicationRecipientGetActivityResultsBag
    {
        /// <summary>
        /// Gets or sets the recipient person's hashed identifier key.
        /// </summary>
        public string PersonIdKey { get; set; }

        /// <summary>
        /// Gets or sets the recipient person's nick name.
        /// </summary>
        public string PersonNickName { get; set; }

        /// <summary>
        /// Gets or sets the recipient person's last name.
        /// </summary>
        public string PersonLastName { get; set; }

        /// <summary>
        /// Gets or sets the recipient person's photo URL.
        /// </summary>
        public string PersonPhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the recipient person's email address.
        /// </summary>
        public string PersonEmail { get; set; }

        /// <summary>
        /// Gets or sets the recipient person's primary campus name.
        /// </summary>
        public string PersonCampusName { get; set; }

        /// <summary>
        /// Gets or sets the recipient person's age.
        /// </summary>
        public int? PersonAge { get; set; }

        /// <summary>
        /// Gets or sets the recipient person's connection status.
        /// </summary>
        public string PersonConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets the recipient person's marital status.
        /// </summary>
        public string PersonMaritalStatus { get; set; }

        /// <summary>
        /// Gets or sets the recipient person's phone number.
        /// </summary>
        public string PersonPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the list of activities for this recipient.
        /// </summary>
        public List<CommunicationRecipientActivityBag> Activities { get; set; }
    }
}
