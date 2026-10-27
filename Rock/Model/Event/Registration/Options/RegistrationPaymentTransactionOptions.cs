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
using Rock.Financial;

namespace Rock.Model.Event.Registration.Options
{
    /*
        10/6/2026 - MSE

        Inside this namespace the simple name "Registration" resolves to the
        Rock.Model.Event.Registration namespace, not the entity. Always refer to
        the entity as Rock.Model.Registration in this file.

        Reason: Avoid a namespace/type name collision.
    */

    /// <summary>
    /// Options that control how <see cref="RegistrationService.SaveRegistrationPaymentTransaction"/>
    /// saves a payment made against a <see cref="Rock.Model.Registration"/>.
    /// </summary>
    internal class RegistrationPaymentTransactionOptions
    {
        /// <summary>
        /// Gets or sets the payment information returned by the gateway. When set, the
        /// transaction's payment detail is filled in from it and its amount is used when
        /// <see cref="Amount"/> is <c>null</c>. When <c>null</c>, the payment detail is
        /// left exactly as the caller built it (manual payments).
        /// </summary>
        public PaymentInfo PaymentInfo { get; set; }

        /// <summary>
        /// Gets or sets the gateway component used to fill in the payment detail from
        /// <see cref="PaymentInfo"/>. Only used when <see cref="PaymentInfo"/> is set.
        /// </summary>
        public GatewayComponent GatewayComponent { get; set; }

        /// <summary>
        /// Gets or sets the financial gateway identifier to stamp on the transaction.
        /// The gateway also drives batch selection (settlement batches, batch time offset
        /// and batch day of week). When <c>null</c> the transaction's existing gateway is
        /// left unchanged, so manual payments stay gateway-less.
        /// </summary>
        public int? FinancialGatewayId { get; set; }

        /// <summary>
        /// Gets or sets the amount of the registration transaction detail. When
        /// <c>null</c>, the amount of <see cref="PaymentInfo"/> is used.
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Gets or sets the financial account of the registration transaction detail.
        /// When <c>null</c>, the detail keeps the account it already has.
        /// </summary>
        public int? AccountId { get; set; }

        /// <summary>
        /// Gets or sets the person alias that authorized the payment. When <c>null</c>,
        /// the registrar of the registration is used.
        /// </summary>
        public int? AuthorizedPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the source type defined value identifier. When <c>null</c>, the
        /// transaction's source type is left unchanged.
        /// </summary>
        public int? SourceTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the transaction summary. When <c>null</c>, the registration's
        /// generated summary is used.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the prefix used to find or create the batch the transaction
        /// is placed in.
        /// </summary>
        public string BatchNamePrefix { get; set; }

        /// <summary>
        /// Gets or sets the person alias recorded as the modifier of the batch history.
        /// The batch history is saved on a background task where no request person is
        /// available, so it must be supplied here.
        /// </summary>
        public int? ModifiedByPersonAliasId { get; set; }
    }
}
