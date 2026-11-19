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

namespace Rock.ViewModels.Blocks.Event.RegistrationDetail
{
    /// <summary>
    /// The values used to charge a new payment for a registration through the gateway.
    /// </summary>
    public class ProcessPaymentRequestBag
    {
        /// <summary>
        /// Gets or sets the unique identifier the client created for this payment.
        /// A retry of the same payment must send the same value so it is never
        /// recorded or charged twice.
        /// </summary>
        public Guid TransactionGuid { get; set; }

        /// <summary>
        /// Gets or sets the payment token provided by the gateway control.
        /// </summary>
        public string GatewayToken { get; set; }

        /// <summary>
        /// Gets or sets the amount of the payment.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the person alias of the payer.
        /// When null the registrar is used.
        /// </summary>
        public Guid? PayerPersonAliasGuid { get; set; }

        /// <summary>
        /// Gets or sets the comments about the payment.
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Gets or sets the billing address of the payment method.
        /// </summary>
        public AddressControlBag BillingAddress { get; set; }
    }
}
