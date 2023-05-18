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

namespace Rock.ViewModels.Blocks.Mobile.Communication.SmsConversationList
{
    /// <summary>
    /// A single phone number provided by the server that can be selected
    /// by the individual.
    /// </summary>
    public class PhoneNumberBag
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>The unique identifier.</value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the contact key.
        /// </summary>
        /// <value>The contact key.</value>
        public string ContactKey { get; set; }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        /// <value>The phone number.</value>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the text that describes the phone number.
        /// </summary>
        /// <value>The text that describes the phone number.</value>
        public string Description { get; set; }
    }
}
