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

namespace Rock.ViewModels.Blocks.Security.AccountEntry
{
    /// <summary>
    /// A box that contains the required information to render an account entry block.
    /// </summary>
    public class AccountEntryInitializationBox
    {
        /// <summary>
        /// Gets or sets a value indicating whether phone numbers shown.
        /// </summary>
        public bool ArePhoneNumbersShown { get; set; }

        /// <summary>
        /// The email address of the registering user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether account information is hidden.
        /// </summary>
        public bool IsAccountInfoHidden { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether address info is required.
        /// </summary>
        public bool IsAddressRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the address fields are shown.
        /// </summary>
        public bool IsAddressShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the email field is hidden.
        /// </summary>
        public bool IsEmailHidden { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether username must be an email.
        /// </summary>
        public bool IsEmailRequiredForUsername { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the mobile number is hidden.
        /// </summary>
        public bool IsMobileNumberHidden { get; set; }

        /// <summary>
        /// Gets or sets the minimum age.
        /// </summary>
        public int MinimumAge { get; set; }

        /// <summary>
        /// Gets or sets the phone numbers.
        /// </summary>
        /// <value>
        /// The phone numbers.
        /// </value>
        public List<AccountEntryPhoneNumberBag> PhoneNumbers { get; set; }

        /// <summary>
        /// Gets or sets the username field label.
        /// </summary>
        public string UsernameFieldLabel { get; set; }

        /// <summary>
        /// Gets or sets the campus picker label.
        /// </summary>
        public string CampusPickerLabel { get; set; }

        /// <summary>
        /// Indicating whether the campus picker is shown.
        /// </summary>
        public bool IsCampusPickerShown { get; set; }

        /// <summary>
        /// The registration state.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// The caption when an existing account is found.
        /// </summary>
        public string ExistingAccountCaption { get; set; }

        /// <summary>
        /// The caption when the username is sent via email.
        /// </summary>
        public string SentLoginCaption { get; set; }

        /// <summary>
        /// The login page URL.
        /// </summary>
        public string LoginPageUrl { get; set; }

        /// <summary>
        /// The caption when a confirmation email was sent to a selected, duplicate person.
        /// </summary>
        public string ConfirmationSentCaption { get; set; }

        /// <summary>
        /// The success caption.
        /// </summary>
        public string SuccessCaption { get; set; }

        /// <summary>
        /// The regular expression used to determine that the username is in a valid format.
        /// </summary>
        public string UsernameRegex { get; set; }

        /// <summary>
        /// The friendly description of the regular expression used to determine username validity.
        /// </summary>
        public string UsernameRegexDescription { get; set; }

        /// <summary>
        /// Indicates whether username availability checking is disabled.
        /// </summary>
        public bool IsUsernameAvailabilityCheckDisabled { get; set; }

        /// <summary>
        /// Gets or sets the account entry step box.
        /// </summary>
        /// <value>
        /// The  account entry step box.
        /// </value>
        public AccountEntryRegisterResponseBox AccountEntryRegisterStepBox { get; set; }

        /// <summary>
        /// Indicating whether the Gender dropdown is shown.
        /// </summary>
        public bool IsGenderPickerShown { get; set; }

        /// <summary>
        /// Gets or sets the person details if there is an identified user.
        /// </summary>
        /// <value>
        /// The account entry person information bag.
        /// </value>
        public AccountEntryPersonInfoBag AccountEntryPersonInfoBag { get; set; }

        /// <summary>
        /// If set to true if the Captcha verification step should not be performed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if Captcha is disabled; otherwise, <c>false</c>.
        /// </value>
        public bool DisableCaptchaSupport { get; set; }
    }
}
