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

namespace Rock.ViewModels.Blocks.Security.AccountEntry
{
    /// <summary>
    /// A bag that contains the required information to render an account entry block's phone number control.
    /// </summary>
    public class AccountEntryPhoneNumberBag
    {
        /// <summary>
        /// Gets or sets the phone's defined value guid.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Indicates whether this phone number is hidden.
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Indicates whether this phone number is required.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Indicates whether SMS is enabled for this phone number.
        /// </summary>
        public bool IsSmsEnabled { get; set; }

        /// <summary>
        /// Indicates whether this phone number is unlisted.
        /// </summary>
        public bool IsUnlisted { get; set; }

        /// <summary>
        /// Gets or sets the label for this phone number.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the country code.
        /// </summary>
        public string CountryCode { get; set; }
    }
}
