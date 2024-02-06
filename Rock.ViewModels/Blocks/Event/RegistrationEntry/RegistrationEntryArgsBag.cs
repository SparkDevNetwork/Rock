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

namespace Rock.ViewModels.Blocks.Event.RegistrationEntry
{
    /// <summary>
    /// RegistrationEntryBlockArgs Interface
    /// </summary>
    public interface IRegistrationEntryBlockArgs
    {
        /// <summary>
        /// Gets or sets the registration unique identifier.
        /// </summary>
        /// <value>
        /// The registration unique identifier.
        /// </value>
        Guid? RegistrationGuid { get; set; }

        /// <summary>
        /// Gets or sets the registrants.
        /// </summary>
        /// <value>
        /// The registrants.
        /// </value>
        List<RegistrantBag> Registrants { get; set; }

        /// <summary>
        /// Gets or sets the field values.
        /// </summary>
        /// <value>
        /// The field values.
        /// </value>
        Dictionary<Guid, object> FieldValues { get; set; }

        /// <summary>
        /// Gets or sets the registrar.
        /// </summary>
        /// <value>
        /// The registrar.
        /// </value>
        RegistrarBag Registrar { get; set; }

        /// <summary>
        /// Gets or sets the gateway token.
        /// </summary>
        /// <value>
        /// The gateway token.
        /// </value>
        string GatewayToken { get; set; }

        /// <summary>
        /// Gets or sets the discount code.
        /// </summary>
        /// <value>
        /// The discount code.
        /// </value>
        string DiscountCode { get; set; }

        /// <summary>
        /// Gets or sets the amount to pay now.
        /// </summary>
        /// <value>
        /// The amount to pay now.
        /// </value>
        decimal AmountToPayNow { get; set; }
    }

    /// <summary>
    /// RegistrationEntryBlockArgs
    /// </summary>
    public sealed class RegistrationEntryArgsBag : IRegistrationEntryBlockArgs
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
        public List<RegistrantBag> Registrants { get; set; }

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
        public RegistrarBag Registrar { get; set; }

        /// <summary>
        /// Gets or sets the gateway token.
        /// </summary>
        /// <value>
        /// The gateway token.
        /// </value>
        public string GatewayToken { get; set; }

        /// <summary>
        /// Gets or sets the saved account unique identifier.
        /// </summary>
        /// <value>
        /// The saved account unique identifier.
        /// </value>
        public Guid? SavedAccountGuid { get; set; }

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
    }
}
