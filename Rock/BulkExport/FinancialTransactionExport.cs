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
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.BulkExport
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.BulkExport.ModelExport" />
    [RockClientInclude( "Export result from ~/api/FinancialTransactions/Export" )]
    public class FinancialTransactionExport : ModelExport
    {
        private FinancialTransaction _financialTransaction;

        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionExport"/> class.
        /// </summary>
        /// <param name="financialTransaction">The financial transaction.</param>
        public FinancialTransactionExport( FinancialTransaction financialTransaction )
            : base( financialTransaction )
        {
            _financialTransaction = financialTransaction;
        }

        /// <summary>
        /// Gets the transaction details.
        /// </summary>
        /// <value>
        /// The transaction details.
        /// </value>
        [DataMember]
        public List<FinancialTransactionDetailExport> TransactionDetails
        {
            get
            {
                return _financialTransaction.TransactionDetails.Select( d => new FinancialTransactionDetailExport( d ) ).ToList();
            }
        }

        /// <summary>
        /// Gets the authorized person identifier.
        /// </summary>
        /// <value>
        /// The authorized person identifier.
        /// </value>
        [DataMember]
        public int? AuthorizedPersonId => _financialTransaction.AuthorizedPersonAlias?.PersonId;

        /// <summary>
        /// Gets the authorized person alias identifier.
        /// </summary>
        /// <value>
        /// The authorized person alias identifier.
        /// </value>
        [DataMember]
        public int? AuthorizedPersonAliasId => _financialTransaction.AuthorizedPersonAliasId;

        /// <summary>
        /// Gets the transaction date time.
        /// </summary>
        /// <value>
        /// The transaction date time.
        /// </value>
        [DataMember]
        public DateTime TransactionDateTime => _financialTransaction.TransactionDateTime ?? DateTime.MinValue;

        /// <summary>
        /// Gets a value indicating whether [show as anonymous].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show as anonymous]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ShowAsAnonymous => _financialTransaction.ShowAsAnonymous;

        /// <summary>
        /// Gets the batch identifier.
        /// </summary>
        /// <value>
        /// The batch identifier.
        /// </value>
        [DataMember]
        public int? BatchId => _financialTransaction.BatchId;

        /// <summary>
        /// Gets the financial gateway identifier.
        /// </summary>
        /// <value>
        /// The financial gateway identifier.
        /// </value>
        [DataMember]
        public int? FinancialGatewayId => _financialTransaction.FinancialGatewayId;

        /// <summary>
        /// Gets the transaction code.
        /// </summary>
        /// <value>
        /// The transaction code.
        /// </value>
        [DataMember]
        public string TransactionCode => _financialTransaction.TransactionCode;

        /// <summary>
        /// Gets the summary.
        /// </summary>
        /// <value>
        /// The summary.
        /// </value>
        [DataMember]
        public string Summary => _financialTransaction.Summary;

        /// <summary>
        /// Gets the source type value identifier.
        /// </summary>
        /// <value>
        /// The source type value identifier.
        /// </value>
        [DataMember]
        public int? SourceTypeValueId => _financialTransaction.SourceTypeValueId;

        /// <summary>
        /// Gets the type of the source.
        /// </summary>
        /// <value>
        /// The type of the source.
        /// </value>
        [DataMember]
        public string SourceType
        {
            get
            {
                if ( SourceTypeValueId.HasValue )
                {
                    return DefinedValueCache.Get( SourceTypeValueId.Value )?.Value;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the scheduled transaction identifier.
        /// </summary>
        /// <value>
        /// The scheduled transaction identifier.
        /// </value>
        [DataMember]
        public int? ScheduledTransactionId => _financialTransaction.ScheduledTransactionId;

        /// <summary>
        /// Gets the processed date time.
        /// </summary>
        /// <value>
        /// The processed date time.
        /// </value>
        [DataMember]
        public DateTime? ProcessedDateTime => _financialTransaction.ProcessedDateTime;

        /// <summary>
        /// Gets the is settled.
        /// </summary>
        /// <value>
        /// The is settled.
        /// </value>
        [DataMember]
        public bool? IsSettled => _financialTransaction.IsSettled;

        /// <summary>
        /// Gets the settled group identifier.
        /// </summary>
        /// <value>
        /// The settled group identifier.
        /// </value>
        [DataMember]
        public string SettledGroupId => _financialTransaction.SettledGroupId;

        /// <summary>
        /// Gets the settled date.
        /// </summary>
        /// <value>
        /// The settled date.
        /// </value>
        [DataMember]
        public DateTime? SettledDate => _financialTransaction.SettledDate;

        /// <summary>
        /// Gets the is reconciled.
        /// </summary>
        /// <value>
        /// The is reconciled.
        /// </value>
        [DataMember]
        public bool? IsReconciled => _financialTransaction.IsReconciled;

        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [DataMember]
        public string Status => _financialTransaction.Status;

        /// <summary>
        /// Gets the status message.
        /// </summary>
        /// <value>
        /// The status message.
        /// </value>
        [DataMember]
        public string StatusMessage => _financialTransaction.StatusMessage;

        /// <summary>
        /// Gets the currency type value identifier.
        /// </summary>
        /// <value>
        /// The currency type value identifier.
        /// </value>
        [DataMember]
        public int? CurrencyTypeValueId => _financialTransaction.FinancialPaymentDetail?.CurrencyTypeValueId;

        /// <summary>
        /// Gets the type of the currency.
        /// </summary>
        /// <value>
        /// The type of the currency.
        /// </value>
        [DataMember]
        public string CurrencyType
        {
            get
            {
                if ( CurrencyTypeValueId.HasValue )
                {
                    return DefinedValueCache.Get( CurrencyTypeValueId.Value )?.Value;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the credit card type value identifier.
        /// </summary>
        /// <value>
        /// The credit card type value identifier.
        /// </value>
        [DataMember]
        public int? CreditCardTypeValueId => _financialTransaction.FinancialPaymentDetail?.CreditCardTypeValueId;

        /// <summary>
        /// Gets the type of the credit card.
        /// </summary>
        /// <value>
        /// The type of the credit card.
        /// </value>
        [DataMember]
        public string CreditCardType
        {
            get
            {
                if ( CreditCardTypeValueId.HasValue )
                {
                    return DefinedValueCache.Get( CreditCardTypeValueId.Value )?.Value;
                }

                return null;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [RockClientInclude( "Export result from ~/api/FinancialTransactions/Export" )]
    public class FinancialTransactionDetailExport
    {
        /// <summary>
        /// The financial transaction detail
        /// </summary>
        private FinancialTransactionDetail _financialTransactionDetail;

        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionDetailExport"/> class.
        /// </summary>
        /// <param name="financialTransactionDetail">The financial transaction detail.</param>
        public FinancialTransactionDetailExport( FinancialTransactionDetail financialTransactionDetail )
        {
            _financialTransactionDetail = financialTransactionDetail;
        }

        /// <summary>
        /// Gets the account identifier.
        /// </summary>
        /// <value>
        /// The account identifier.
        /// </value>
        [DataMember]
        public int AccountId => _financialTransactionDetail.AccountId;

        /// <summary>
        /// Gets the name of the account.
        /// </summary>
        /// <value>
        /// The name of the account.
        /// </value>
        [DataMember]
        public string AccountName => _financialTransactionDetail.Account.Name;

        /// <summary>
        /// Gets the CampusId for the Account
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        [DataMember]
        public int? CampusId => _financialTransactionDetail.Account.CampusId;

        /// <summary>
        /// Gets the Campus Name for the Account
        /// </summary>
        /// <value>
        /// The name of the campus.
        /// </value>
        [DataMember]
        public string CampusName => _financialTransactionDetail.Account.CampusId.HasValue ? CampusCache.Get( _financialTransactionDetail.Account.CampusId.Value ).Name : null;

        /// <summary>
        /// Gets the amount.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        [DataMember]
        public decimal Amount => _financialTransactionDetail.Amount;
    }
}
