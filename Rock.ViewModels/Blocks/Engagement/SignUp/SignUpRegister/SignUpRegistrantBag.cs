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
using System.Diagnostics;

namespace Rock.ViewModels.Blocks.Engagement.SignUp.SignUpRegister
{
    /// <summary>
    /// Information about a sign-up registrant.
    /// </summary>
    [DebuggerDisplay( "{FullName}, Will Attend: {WillAttend}" )]
    public class SignUpRegistrantBag
    {
        /// <summary>
        /// Gets or sets the registrant's hashed person identifier key.
        /// </summary>
        /// <value>
        /// The registrant's hashed person identifier key.
        /// </value>
        public string PersonIdKey { get; set; }

        /// <summary>
        /// Gets or sets whether this <see cref="SignUpRegistrantBag"/> instance represents the registrar.
        /// </summary>
        /// <value>
        /// Whether this <see cref="SignUpRegistrantBag"/> instance represents the registrar.
        /// </value>
        public bool IsRegistrar { get; set; }

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
        /// Gets or sets the registrant's full name.
        /// </summary>
        /// <value>
        /// The registrant's full name.
        /// </value>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets whether this <see cref="SignUpRegistrantBag"/> instance represents a child.
        /// </summary>
        /// <value>
        /// Whether this <see cref="SignUpRegistrantBag"/> instance represents a child.
        /// </value>
        public bool IsChild { get; set; }

        /// <summary>
        /// Gets or sets the registrant's communication preference.
        /// </summary>
        /// <value>
        /// The registrant's communication preference.
        /// </value>
        public int CommunicationPreference { get; set; }

        /// <summary>
        /// Gets or sets the registrant's email.
        /// </summary>
        /// <value>
        /// The registrant's email.
        /// </value>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the registrant's mobile phone number.
        /// </summary>
        /// <value>
        /// The registrant's mobile phone number.
        /// </value>
        public string MobilePhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the registrant's formatted mobile phone number.
        /// </summary>
        /// <value>
        /// The registrant's formatted mobile phone number.
        /// </value>
        public string MobilePhoneNumberFormatted { get; set; }

        /// <summary>
        /// Gets or sets the registrant's mobile phone country code.
        /// </summary>
        /// <value>
        /// The registrant's mobile phone country code.
        /// </value>
        public string MobilePhoneCountryCode { get; set; }

        /// <summary>
        /// Gets or sets whether to allow SMS messages to be sent to the registrant's mobile phone.
        /// </summary>
        /// <value>
        /// Whether to allow SMS messages to be sent to the registrant's mobile phone.
        /// </value>
        public bool AllowSms { get; set; }

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
        public List<string> UnmetGroupRequirements { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the registrant's member attribute values.
        /// </summary>
        /// <value>
        /// The registrant's member attribute values.
        /// </value>
        public Dictionary<string, string> MemberAttributeValues { get; set; }

        /// <summary>
        /// Gets or sets the registrant's member opportunity attribute values.
        /// </summary>
        /// <value>
        /// The registrant's member opportunity attribute values.
        /// </value>
        public Dictionary<string, string> MemberOpportunityAttributeValues { get; set; }
    }
}
