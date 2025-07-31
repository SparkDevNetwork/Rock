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

using Rock.ViewModels.Controls;

namespace Rock.ViewModels.Blocks.Connection.ConnectionOpportunitySignup
{
    /// <summary>
    /// A bag containing the signup request details.
    /// </summary>
    public class ConnectionOpportunitySignupRequestBag
    {
        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the pre-filled home phone.
        /// </summary>
        public PhoneNumberBoxWithSmsControlBag HomePhone { get; set; }

        /// <summary>
        /// Gets or sets the pre-filled mobile phone.
        /// </summary>
        public PhoneNumberBoxWithSmsControlBag MobilePhone { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Gets or sets the attribute values.
        /// </summary>
        public Dictionary<string, string> AttributeValues { get; set; }
    }
}
