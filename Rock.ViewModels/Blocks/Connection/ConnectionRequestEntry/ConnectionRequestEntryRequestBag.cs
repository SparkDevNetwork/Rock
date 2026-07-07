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

using System;
using System.Collections.Generic;

using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Connection.ConnectionRequestEntry
{
    /// <summary>
    /// The values a visitor submits from the Connection Request Entry form.
    /// </summary>
    public class ConnectionRequestEntryRequestBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the selected campus.
        /// </summary>
        public Guid? CampusGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the visitor identified as a first time guest.
        /// </summary>
        public bool IsFirstTimeGuest { get; set; }

        /// <summary>
        /// Gets or sets the selected title (a Person Title defined value).
        /// </summary>
        public ListItemBag Title { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the selected suffix (a Person Suffix defined value).
        /// </summary>
        public ListItemBag Suffix { get; set; }

        /// <summary>
        /// Gets or sets the birth date in ISO 8601 form.
        /// </summary>
        public string BirthDate { get; set; }

        /// <summary>
        /// Gets or sets the gender value.
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// Gets or sets the uploaded profile photo binary file unique identifier.
        /// </summary>
        public string PhotoGuid { get; set; }

        /// <summary>
        /// Gets or sets the selected marital status (a Marital Status defined value).
        /// </summary>
        public ListItemBag MaritalStatus { get; set; }

        /// <summary>
        /// Gets or sets the spouse first name. Only used when Marital Status is Married.
        /// </summary>
        public string SpouseFirstName { get; set; }

        /// <summary>
        /// Gets or sets the spouse last name. Only used when Marital Status is Married.
        /// </summary>
        public string SpouseLastName { get; set; }

        /// <summary>
        /// Gets or sets the spouse gender value. Only used when Marital Status is Married.
        /// </summary>
        public string SpouseGender { get; set; }

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the spouse email address. Only used when Marital Status is Married.
        /// </summary>
        public string SpouseEmail { get; set; }

        /// <summary>
        /// Gets or sets the mobile phone, including the SMS-consent flag.
        /// </summary>
        public PhoneNumberBoxWithSmsControlBag MobilePhone { get; set; }

        /// <summary>
        /// Gets or sets the spouse mobile phone. Only used when Marital Status is Married.
        /// </summary>
        public PhoneNumberBoxWithSmsControlBag SpouseMobilePhone { get; set; }

        /// <summary>
        /// Gets or sets the home address.
        /// </summary>
        public AddressControlBag Address { get; set; }

        /// <summary>
        /// Gets or sets the selected preferred service time (a Schedule).
        /// </summary>
        public ListItemBag PreferredServiceTime { get; set; }

        /// <summary>
        /// Gets or sets the additional comments.
        /// </summary>
        public string AdditionalComments { get; set; }

        /// <summary>
        /// Gets or sets the person attribute values entered in the Additional Information section.
        /// </summary>
        public Dictionary<string, string> PersonAttributeValues { get; set; }

        /// <summary>
        /// Gets or sets the opportunities the visitor selected, with their per-opportunity attribute values.
        /// </summary>
        public List<ConnectionRequestEntrySelectedOpportunityBag> SelectedOpportunities { get; set; }

        /// <summary>
        /// Gets or sets the CAPTCHA token to validate when CAPTCHA is enabled.
        /// </summary>
        public string CaptchaToken { get; set; }
    }
}
