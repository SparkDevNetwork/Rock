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

using System;
using System.Collections.Generic;

namespace Rock.ViewModel.Blocks
{
    /// <summary>
    /// RegistrationEntryBlockArgs Interface
    /// </summary>
    public interface IRegistrationEntryBlockArgs : IViewModel
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
        List<RegistrantInfo> Registrants { get; set; }

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
        RegistrarInfo Registrar { get; set; }

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
    public sealed class RegistrationEntryBlockArgs : IRegistrationEntryBlockArgs
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
    }

    /// <summary>
    /// Registrant Info
    /// </summary>
    public sealed class RegistrantInfo
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is on wait list.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is on wait list; otherwise, <c>false</c>.
        /// </value>
        public bool IsOnWaitList { get; set; }

        /// <summary>
        /// Gets or sets the family unique identifier.
        /// </summary>
        /// <value>
        /// The family unique identifier.
        /// </value>
        public Guid? FamilyGuid { get; set; }

        /// <summary>
        /// Gets or sets the person unique identifier.
        /// </summary>
        /// <value>
        /// The person unique identifier.
        /// </value>
        public Guid? PersonGuid { get; set; }

        /// <summary>
        /// Gets or sets the field values.
        /// </summary>
        /// <value>
        /// The field values.
        /// </value>
        public Dictionary<Guid, object> FieldValues { get; set; }

        /// <summary>
        /// Gets or sets the fee item quantities.
        /// </summary>
        /// <value>
        /// The fee item quantities.
        /// </value>
        public Dictionary<Guid, int> FeeItemQuantities { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid { get; set; }
    }

    /// <summary>
    /// Registrar Info
    /// </summary>
    public sealed class RegistrarInfo
    {
        /// <summary>
        /// Gets or sets the name of the nick.
        /// </summary>
        /// <value>
        /// The name of the nick.
        /// </value>
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [update email].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [update email]; otherwise, <c>false</c>.
        /// </value>
        public bool UpdateEmail { get; set; }

        /// <summary>
        /// Gets or sets the family unique identifier.
        /// </summary>
        /// <value>
        /// The family unique identifier.
        /// </value>
        public Guid? FamilyGuid { get; set; }
    }
}
