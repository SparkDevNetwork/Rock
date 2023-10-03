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

using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Finance.BusinessDetail
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class BusinessDetailBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the name of the business.
        /// </summary>
        /// <value>
        /// The name of the business.
        /// </value>
        public string BusinessName { get; set; }

        /// <summary>
        /// Gets or sets the address as HTML.
        /// </summary>
        /// <value>
        /// The address as HTML.
        /// </value>
        public string AddressAsHtml { get; set; }

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        /// <value>
        /// The address.
        /// </value>
        public AddressControlBag Address { get; set; }

        /// <summary>
        /// Gets or sets the previous address.
        /// </summary>
        /// <value>
        /// The previous address.
        /// </value>
        public string PreviousAddress { get; set; }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        /// <value>
        /// The phone number.
        /// </value>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the country code.
        /// </summary>
        /// <value>
        /// The country code.
        /// </value>
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        /// <value>
        /// The email address.
        /// </value>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the record status.
        /// </summary>
        /// <value>
        /// The record status.
        /// </value>
        public ListItemBag RecordStatus { get; set; }

        /// <summary>
        /// Gets or sets the record reason.
        /// </summary>
        /// <value>
        /// The record reason.
        /// </value>
        public ListItemBag RecordStatusReason { get; set; }

        /// <summary>
        /// Gets or sets the campus.
        /// </summary>
        /// <value>
        /// The campus.
        /// </value>
        public ListItemBag Campus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is SMS checked.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is SMS checked; otherwise, <c>false</c>.
        /// </value>
        public bool IsSmsChecked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is unlisted checked.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is unlisted checked; otherwise, <c>false</c>.
        /// </value>
        public bool IsUnlistedChecked { get; set; }

        /// <summary>
        /// Gets or sets the email preference.
        /// </summary>
        /// <value>
        /// The email preference.
        /// </value>
        public string EmailPreference { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [save former address as previous address].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [save former address as previous address]; otherwise, <c>false</c>.
        /// </value>
        public bool SaveFormerAddressAsPreviousAddress { get; set; }

        /// <summary>
        /// Gets or sets the search keys.
        /// </summary>
        /// <value>
        /// The search keys.
        /// </value>
        public List<SearchKeyBag> SearchKeys { get; set; }

        /// <summary>
        /// Gets or sets the custom actions.
        /// </summary>
        /// <value>
        /// The custom actions.
        /// </value>
        public string CustomActions { get; set; }
    }
}
