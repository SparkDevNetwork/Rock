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

namespace Rock.ViewModel.Blocks.Event.RegistrationEntry
{
    /// <summary>
    /// RegistrationEntryBlockSession
    /// </summary>
    public sealed class RegistrationEntryBlockSession : IRegistrationEntryBlockArgs
    {
        /// <summary>
        /// Gets or sets the registration session unique identifier.
        /// </summary>
        /// <value>
        /// The registration session unique identifier.
        /// </value>
        public Guid RegistrationSessionGuid { get; set; }

        /// <summary>
        /// Gets or sets the registration unique identifier.
        /// </summary>
        /// <value>
        /// The registration unique identifier.
        /// </value>
        public Guid? RegistrationGuid { get; set; }

        /// <summary>
        /// Gets or sets the registrants.
        /// </summary>
        /// <value>
        /// The registrants.
        /// </value>
        public List<RegistrantInfo> Registrants { get; set; }

        /// <summary>
        /// Gets or sets the field values.
        /// </summary>
        /// <value>
        /// The field values.
        /// </value>
        public Dictionary<Guid, object> FieldValues { get; set; }

        /// <summary>
        /// Gets or sets the registrar.
        /// </summary>
        /// <value>
        /// The registrar.
        /// </value>
        public RegistrarInfo Registrar { get; set; }

        /// <summary>
        /// Gets or sets the gateway token.
        /// </summary>
        /// <value>
        /// The gateway token.
        /// </value>
        public string GatewayToken { get; set; }

        /// <summary>
        /// Gets or sets the discount code.
        /// </summary>
        /// <value>
        /// The discount code.
        /// </value>
        public string DiscountCode { get; set; }

        /// <summary>
        /// Gets or sets the amount to pay now.
        /// </summary>
        /// <value>
        /// The amount to pay now.
        /// </value>
        public decimal AmountToPayNow { get; set; }

        /// <summary>
        /// Gets or sets the discount.
        /// </summary>
        /// <value>
        /// The discount.
        /// </value>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Gets or sets the discount.
        /// </summary>
        /// <value>
        /// The discount.
        /// </value>
        public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// Gets or sets the previously paid.
        /// </summary>
        /// <value>
        /// The previously paid.
        /// </value>
        public decimal PreviouslyPaid { get; set; }
    }
}
