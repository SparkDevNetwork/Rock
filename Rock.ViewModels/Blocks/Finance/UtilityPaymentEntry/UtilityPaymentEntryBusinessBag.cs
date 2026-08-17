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

using Rock.ViewModels.Controls;

namespace Rock.ViewModels.Blocks.Finance.UtilityPaymentEntry
{
    /// <summary>
    /// A business the signed-in giver may give on behalf of, offered in the Give As Business list. Carries
    /// the prefill values shown when the giver selects it.
    /// </summary>
    public class UtilityPaymentEntryBusinessBag
    {
        /// <summary>
        /// Gets or sets the Guid of the business.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the business name, shown in the list and prefilled into the business name field.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the business email, prefilled when the business is selected.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the business phone number, prefilled when the business is selected. Empty when no
        /// number is on file.
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets the country code for the business phone number.
        /// </summary>
        public string PhoneCountryCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the SMS opt-in checkbox starts checked for the
        /// business's number.
        /// </summary>
        public bool IsSmsOptInChecked { get; set; }

        /// <summary>
        /// Gets or sets the business address, prefilled when the business is selected. Null when no address
        /// is on file.
        /// </summary>
        public AddressControlBag Address { get; set; }
    }
}
