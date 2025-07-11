﻿// <copyright>
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

namespace Rock.ViewModels.Finance
{
    /// <summary>
    /// A bag used to add a saved account using a payment method token.
    /// </summary>
    public class SavedAccountTokenBag
    {
        /// <summary>
        /// The token for the payment method.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// The currency type value identifier.
        /// </summary>
        public string CurrencyTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the first street line.
        /// </summary>
        /// <value>
        /// The first street line.
        /// </value>
        public string Street1 { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>
        /// The city.
        /// </value>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the postal code.
        /// </summary>
        /// <value>
        /// The postal code.
        /// </value>
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        /// <value>
        /// The country.
        /// </value>
        public string Country { get; set; }

        /// <summary>
        /// The additional parameters to include on the payment request.
        /// </summary>
        public Dictionary<string, string> AdditionalParameters { get; set; }
    }
}
