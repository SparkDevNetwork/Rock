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
using Rock.Model;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Security.AccountEntry
{
    /// <summary>
    /// A bag that contains the required information to render an account entry block's person info section.
    /// </summary>
    public class AccountEntryPersonInfoBag
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
        /// Gets or sets the gender.
        /// </summary>
        public Gender? Gender { get; set; }

        /// <summary>
        /// Gets or sets the birthday.
        /// </summary>
        public BirthdayPickerBag Birthday { get; set; }

        /// <summary>
        /// Gets or sets the phone numbers.
        /// </summary>
        public List<AccountEntryPhoneNumberBag> PhoneNumbers { get; set; }

        /// <summary>
        /// Gets or sets the campus.
        /// </summary>
        public Guid? Campus { get; set; }

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        public AddressControlBag Address { get; set; }

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the attribute values.
        /// </summary>
        public Dictionary<string, string> AttributeValues { get; set; }
    }
}
