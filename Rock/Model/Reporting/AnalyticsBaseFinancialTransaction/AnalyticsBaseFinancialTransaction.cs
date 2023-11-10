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
using Rock.Data;
using Rock.Web.UI.Controls;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// AnalyticSourceFinancialTransaction is a real table, and AnalyticsFactFinancialTransation is a VIEW off of AnalyticSourceFinancialTransaction, so they share lots of columns
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Rock.Data.Entity{T}" />
    [RockDomain( "Reporting" )]
    public abstract class AnalyticsBaseFinancialTransaction<T> : Entity<T>
        where T : AnalyticsBaseFinancialTransaction<T>, new()
    {
        #region Entity Properties Specific to Analytics

        /// <summary>
        /// Gets or sets the transaction key in the form of "{Transaction.Id}_{TransactionDetail.Id}"
        /// NOTE: Length of 40 is big enough for each to have int64.MaxValue "9223372036854775807_9223372036854775807"
        /// </summary>
        /// <value>
        /// The transaction key.
        /// </value>
        [DataMember( IsRequired = true )]
        [MaxLength( 40 )]
#if REVIEW_WEBFORMS
        [Index( "IX_TransactionKey", IsUnique = true )]
#endif
        public string TransactionKey { get; set; }

        /// <summary>
        /// Gets or sets the transaction date key which is the form YYYYMMDD
        /// </summary>
        /// <value>
        /// The transaction date key.
        /// </value>
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.DATE )]
        public int TransactionDateKey { get; set; }

        /// <summary>
        /// Gets or sets the authorized person key for the person's record at the time of the transaction
        /// </summary>
        /// <value>
        /// The authorized person key.
        /// </value>
        [DataMember]
        public int? AuthorizedPersonKey { get; set; }

        /// <summary>
        /// Gets or sets the authorized person key for the person's current record
        /// </summary>
        /// <value>
        /// The authorized person key.
        /// </value>
        [DataMember]
        public int? AuthorizedCurrentPersonKey { get; set; }

        /// <summary>
        /// Number of Days since the last time this giving unit did a TransactionType that is the same as this TransactionType
        /// If IsFirstTransactionOfType is TRUE, DaysSinceLastTransactionOfType will be null 
        /// </summary>
        /// <value>
        /// The type of the days since last transaction of.
        /// </value>
        [DataMember]
        public int? DaysSinceLastTransactionOfType { get; set; }

        /// <summary>
        /// This is true if this is the first time this giving unit did a transaction with this TransactionType 
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is first transaction of type; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsFirstTransactionOfType { get; set; }

        /// <summary>
        /// This is the GroupId of the family of the Authorized Person that did this transaction
        /// Note that this is the current family that the person is in. 
        /// To see what GivingGroup they were part of when the Transaction occurred, see GivingUnitId
        /// </summary>
        /// <value>
        /// The authorized family identifier.
        /// </value>
        [DataMember]
        public int? AuthorizedFamilyId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is scheduled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is scheduled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsScheduled { get; set; }

        /// <summary>
        /// If this is from a Schedule Transaction, what is the Scheduled Transaction Frequency
        /// </summary>
        /// <value>
        /// The transaction frequency.
        /// </value>
        [DataMember]
        [MaxLength( 250 )]
        public string TransactionFrequency { get; set; }

        /// <summary>
        /// Gets or sets the giving group id of the person at the time of the transaction.  If an individual would like their giving to be grouped with the rest of their family,
        /// this will be the id of their family group.  If they elect to contribute on their own, this value will be null.
        /// </summary>
        /// <value>
        /// The giving group id.
        /// </value>
        [DataMember]
        public int? GivingGroupId { get; set; }

        /// <summary>
        /// The computed giver identifier in the format G{GivingGroupId} if they are part of a GivingGroup, or P{Personid} if they give individually
        /// Length of 20 is big enough for each to have G/P prefix + int64.MaxValue "G9223372036854775807"
        /// NOTE: this is the Person's GivingId at the time of the transaction
        /// </summary>
        /// <value>
        /// The giving identifier.
        /// </value>
        [DataMember]
        [MaxLength( 20 )]
        public string GivingId { get; set; }

        /// <summary>
        /// Gets or sets the count.
        /// NOTE:  This always has a (hard-coded) value of 1. It is stored in the table to assist with analytics calculations.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        [DataMember]
        public int Count { get; set; } = 1;

        #endregion Entity Properties Specific to Analytics

        #region Entity Properties

        /// <summary>
        /// Gets or sets date and time that the transaction occurred. This is the local server time.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the time that the transaction occurred. This is the local server time.
        /// </value>
        [DataMember]
        public DateTime TransactionDateTime { get; set; }

        /// <summary>
        /// For Credit Card transactions, this is the response code that the gateway returns 
        /// For Scanned Checks, this is the check number
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the transaction code of the transaction.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string TransactionCode { get; set; }

        /// <summary>
        /// Gets or sets a summary of the transaction.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing a summary of the transaction.
        /// </value>
        [DataMember]
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the DefinedValueId of the TransactionType <see cref="Rock.Model.DefinedValue"/> indicating
        /// the type of the transaction.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the DefinedValueId of the TransactionType <see cref="Rock.Model.DefinedValue"/> for this transaction.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE )]
        public int TransactionTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the DefinedValueId of the source type <see cref="Rock.Model.DefinedValue"/> for this transaction. Representing the source (method) of this transaction.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the DefinedValueId of the source type <see cref="Rock.Model.DefinedValue"/> for this transaction.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE )]
        public int? SourceTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the authorized person identifier.
        /// </summary>
        /// <value>
        /// The authorized person identifier.
        /// </value>
        [DataMember]
        public int? AuthorizedPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the processed by person alias identifier.
        /// </summary>
        /// <value>
        /// The processed by person alias identifier.
        /// </value>
        [DataMember]
        public int? ProcessedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the processed date time.
        /// </summary>
        /// <value>
        /// The processed date time.
        /// </value>
        [DataMember]
        public DateTime? ProcessedDateTime { get; set; }

        /// <summary>
        /// Gets or sets BatchId of the <see cref="Rock.Model.FinancialBatch"/> that contains this transaction.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the BatchId of the <see cref="Rock.Model.FinancialBatch"/> that contains the transaction.
        /// </value>
        [DataMember]
        [IgnoreCanDelete]
        public int? BatchId { get; set; }

        /// <summary>
        /// Gets or sets the gateway identifier.
        /// </summary>
        /// <value>
        /// The gateway identifier.
        /// </value>
        [DataMember]
        public int? FinancialGatewayId { get; set; }

        /// <summary>
        /// Gets or sets the entity type identifier.
        /// </summary>
        /// <value>
        /// The entity type identifier.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        [DataMember]
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// </value>
        [DataMember]
        public int TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the transaction detail identifier.
        /// </summary>
        /// <value>
        /// The transaction detail identifier.
        /// </value>
        [DataMember]
        public int TransactionDetailId { get; set; }

        /// <summary>
        /// Gets or sets the account identifier.
        /// </summary>
        /// <value>
        /// The account identifier.
        /// </value>
        [DataMember]
        public int? AccountId { get; set; }

        /// <summary>
        /// Gets or sets the currency type value identifier.
        /// </summary>
        /// <value>
        /// The currency type value identifier.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE )]
        public int? CurrencyTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the DefinedValueId of the credit card type <see cref="Rock.Model.DefinedValue"/> indicating the credit card brand/type that was used
        /// to make this transaction. This value will be null for transactions that were not made by credit card.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the DefinedValueId of the credit card type <see cref="Rock.Model.DefinedValue"/> that was used to make this transaction.
        /// This value value will be null for transactions that were not made by credit card.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.FINANCIAL_CREDIT_CARD_TYPE )]
        public int? CreditCardTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        [DataMember]
#if REVIEW_WEBFORMS
        [BoundFieldType( typeof( CurrencyField ) )]
#endif
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the modified date time.
        /// </summary>
        /// <value>
        /// The modified date time.
        /// </value>
        [DataMember]
        public DateTime? ModifiedDateTime { get; set; }

        #endregion Entity Properties

        #region Navigation Properties (must be set by inheriting class's configuration class)

        /// <summary>
        /// Gets or sets the transaction date.
        /// </summary>
        /// <value>
        /// The transaction date.
        /// </value>
        [DataMember]
        public virtual AnalyticsSourceDate TransactionDate { get; set; }

        /// <summary>
        /// Gets or sets the batch.
        /// </summary>
        /// <value>
        /// The batch.
        /// </value>
        [DataMember]
        public virtual AnalyticsDimFinancialBatch Batch { get; set; }

        /// <summary>
        /// Gets or sets the account.
        /// </summary>
        /// <value>
        /// The account.
        /// </value>
        [DataMember]
        public virtual AnalyticsDimFinancialAccount Account { get; set; }

        #endregion Navigation Properties (must be set by inheriting class's configuration class)
    }
}