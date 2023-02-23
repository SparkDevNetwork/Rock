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
using Rock.ViewModels.Entities;

namespace Rock.ViewModels.Blocks.Engagement.SignUp.SignUpRegister
{
    /// <summary>
    /// Information about a sign-up registrant.
    /// </summary>
    public class SignUpRegistrantBag
    {
        /// <summary>
        /// Gets or sets the registrant's person identifier.
        /// </summary>
        /// <value>
        /// The registrant's person identifier.
        /// </value>
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the registrant's first name.
        /// </summary>
        /// <value>
        /// The registrant's first name.
        /// </value>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the registrant's last name.
        /// </summary>
        /// <value>
        /// The last registrant's name.
        /// </value>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the registrant's communication preference.
        /// </summary>
        /// <value>
        /// The registrant's communication preference.
        /// </value>
        public int CommunicationPreference { get; set; }

        /// <summary>
        /// Gets or sets the registrant's mobile phone.
        /// </summary>
        /// <value>
        /// The registrant's mobile phone.
        /// </value>
        public PhoneNumberBag MobilePhone { get; set; }

        /// <summary>
        /// Gets or sets the registrant's email.
        /// </summary>
        /// <value>
        /// The registrant's email.
        /// </value>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets whether the registrant will attend.
        /// </summary>
        /// <value>
        /// Whether the registrant will attend.
        /// </value>
        public bool WillAttend { get; set; }

        /// <summary>
        /// Gets or sets the registrant's unmet group requirements, if any.
        /// </summary>
        /// <value>
        /// The registrant's unmet group requirements, if any.
        /// </value>
        public List<string> UnmetGroupRequirements { get; set; }
    }
}
