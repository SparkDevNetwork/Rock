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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Security.PhoneNumberIdentification
{
    /// <summary>
    /// The result of verifying a code that was sent to a phone number.
    /// </summary>
    public class PhoneNumberIdentificationVerifyResponseBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the individual must select themselves from <see cref="People"/>
        /// because the phone number matched more than one person.
        /// </summary>
        public bool IsPersonSelectionRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the verified phone number did not match anyone in Rock.
        /// </summary>
        public bool IsPhoneNumberNotFound { get; set; }

        /// <summary>
        /// Gets or sets the people that matched the phone number when more than one match was found.
        /// Each value is the person's identifier key and each text is the person's full name.
        /// </summary>
        public List<ListItemBag> People { get; set; }

        /// <summary>
        /// Gets or sets the URL to redirect to when a single person was matched and authenticated.
        /// </summary>
        public string RedirectUrl { get; set; }
    }
}
