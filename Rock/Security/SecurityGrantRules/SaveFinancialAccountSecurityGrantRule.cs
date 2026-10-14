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

using Newtonsoft.Json;

namespace Rock.Security.SecurityGrantRules
{
    /// <summary>
    /// Grants permission to save the payment method of a single financial
    /// transaction for future use.
    /// </summary>
    /// <seealso cref="Rock.Security.SecurityGrantRule" />
    [Rock.SystemGuid.SecurityGrantRuleGuid( "090f59da-c68c-4c27-89a9-2545bb36e844" )]
    internal sealed class SaveFinancialAccountSecurityGrantRule : SecurityGrantRule
    {
        #region Properties

        /// <summary>
        /// Gets the unique identifier of the financial gateway that processed
        /// the transaction.
        /// </summary>
        /// <value>The financial gateway unique identifier.</value>
        [JsonProperty( "gw" )]
        public Guid FinancialGatewayGuid { get; private set; }

        /// <summary>
        /// Gets the transaction code of the transaction.
        /// </summary>
        /// <value>The transaction code.</value>
        [JsonProperty( "tc" )]
        public string TransactionCode { get; private set; }

        /// <summary>
        /// Gets the gateway person identifier of the payment method.
        /// </summary>
        /// <value>The gateway person identifier.</value>
        [JsonProperty( "gp" )]
        public string GatewayPersonIdentifier { get; private set; }

        /// <summary>
        /// Gets the unique identifier of the scheduled transaction when the
        /// payment method is saved from a scheduled transaction.
        /// </summary>
        /// <value>The scheduled transaction unique identifier.</value>
        [JsonProperty( "st", DefaultValueHandling = DefaultValueHandling.Ignore )]
        public Guid? ScheduledTransactionGuid { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Prevents a default instance of the <see cref="SaveFinancialAccountSecurityGrantRule"/> class from being created.
        /// </summary>
        private SaveFinancialAccountSecurityGrantRule()
            : base( Authorization.EDIT )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveFinancialAccountSecurityGrantRule"/> class
        /// for granting <see cref="Authorization.EDIT"/> access.
        /// </summary>
        /// <param name="financialGatewayGuid">The unique identifier of the financial gateway that processed the transaction.</param>
        /// <param name="transactionCode">The transaction code of the transaction.</param>
        /// <param name="gatewayPersonIdentifier">The gateway person identifier of the payment method.</param>
        /// <param name="scheduledTransactionGuid">The unique identifier of the scheduled transaction, if the payment method is saved from one.</param>
        public SaveFinancialAccountSecurityGrantRule( Guid financialGatewayGuid, string transactionCode, string gatewayPersonIdentifier, Guid? scheduledTransactionGuid = null )
            : base( Authorization.EDIT )
        {
            FinancialGatewayGuid = financialGatewayGuid;
            TransactionCode = transactionCode;
            GatewayPersonIdentifier = gatewayPersonIdentifier;
            ScheduledTransactionGuid = scheduledTransactionGuid;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override bool IsAccessGranted( object obj, string action )
        {
            if ( !( obj is SaveFinancialAccountAccess access ) )
            {
                return false;
            }

            if ( FinancialGatewayGuid == Guid.Empty || TransactionCode.IsNullOrWhiteSpace() )
            {
                return false;
            }

            // Some gateways do not provide a gateway person identifier, so
            // an empty value only matches another empty value.
            return access.FinancialGatewayGuid == FinancialGatewayGuid
                && string.Equals( access.TransactionCode, TransactionCode, StringComparison.Ordinal )
                && string.Equals( access.GatewayPersonIdentifier ?? string.Empty, GatewayPersonIdentifier ?? string.Empty, StringComparison.Ordinal )
                && access.ScheduledTransactionGuid == ScheduledTransactionGuid;
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// The object that is checked for permission when the payment method
        /// of a financial transaction is saved.
        /// </summary>
        internal sealed class SaveFinancialAccountAccess
        {
            /// <summary>
            /// Gets the unique identifier of the financial gateway.
            /// </summary>
            /// <value>The financial gateway unique identifier.</value>
            public Guid FinancialGatewayGuid { get; }

            /// <summary>
            /// Gets the transaction code.
            /// </summary>
            /// <value>The transaction code.</value>
            public string TransactionCode { get; }

            /// <summary>
            /// Gets the gateway person identifier.
            /// </summary>
            /// <value>The gateway person identifier.</value>
            public string GatewayPersonIdentifier { get; }

            /// <summary>
            /// Gets the unique identifier of the scheduled transaction.
            /// </summary>
            /// <value>The scheduled transaction unique identifier.</value>
            public Guid? ScheduledTransactionGuid { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="SaveFinancialAccountAccess"/> class.
            /// </summary>
            /// <param name="financialGatewayGuid">The unique identifier of the financial gateway.</param>
            /// <param name="transactionCode">The transaction code.</param>
            /// <param name="gatewayPersonIdentifier">The gateway person identifier.</param>
            /// <param name="scheduledTransactionGuid">The unique identifier of the scheduled transaction, if any.</param>
            public SaveFinancialAccountAccess( Guid financialGatewayGuid, string transactionCode, string gatewayPersonIdentifier, Guid? scheduledTransactionGuid = null )
            {
                FinancialGatewayGuid = financialGatewayGuid;
                TransactionCode = transactionCode;
                GatewayPersonIdentifier = gatewayPersonIdentifier;
                ScheduledTransactionGuid = scheduledTransactionGuid;
            }
        }

        #endregion
    }
}
