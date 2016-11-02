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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents the fact record for an Analytic Fact Financial Transaction in Rock.
    /// Note: that this respresents a combination of the FinancialTransaction and the FinancialTransactionDetail, 
    /// so if a person contributed to multiple accounts in a transaction, there will be multiple AnalyticFactFinancialRecords. 
    /// </summary>
    [Table( "AnalyticsFactFinancialTransaction" )]
    [DataContract]
    public class AnalyticsFactFinancialTransaction : Rock.Data.Entity<AnalyticsFactFinancialTransaction>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the transaction key.
        /// </summary>
        /// <value>
        /// The transaction key.
        /// </value>
        [DataMember]
        [Index( "IX_TransactionKey", IsUnique = true )]
        public string TransactionKey { get; set; }

        /// <summary>
        /// Gets or sets the transaction date key.
        /// </summary>
        /// <value>
        /// The transaction date key.
        /// </value>
        [DataMember]
        public string TransactionDateKey { get; set; }

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

        [DataMember]
        [DefinedValue]
        public string TransactionType { get; set; }

        [DataMember]
        public string TransactionSource { get; set; }

        [DataMember]
        public string ScheduleType { get; set; }

        [DataMember]
        public string AuthorizedPersonKey { get; set; }

        [DataMember]
        public int AuthorizedCurrentPersonId { get; set; }

        [DataMember]
        public string ProcessedByPersonKey { get; set; }

        /// <summary>
        /// Gets or sets the processed date time.
        /// </summary>
        /// <value>
        /// The processed date time.
        /// </value>
        [DataMember]
        public DateTime? ProcessedDateTime { get; set; }

        [DataMember]
        public string GivingUnitKey { get; set; }

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
        public string FinancialGateway { get; set; }

        /// <summary>
        /// Gets or sets the entity type identifier.
        /// </summary>
        /// <value>
        /// The entity type identifier.
        /// </value>
        [DataMember]
        public string EntityTypeName { get; set; }

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
        
        [DataMember]
        public string CurrencyType { get; set; }

        [DataMember]
        public string CreditCardType { get; set; }

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
        /// </summary>
        /// <value>
        /// The authorized family identifier.
        /// </value>
        [DataMember]
        public int? AuthorizedFamilyId { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        [DataMember]
        [BoundFieldTypeAttribute( typeof( Rock.Web.UI.Controls.CurrencyField ) )]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the modified date time.
        /// </summary>
        /// <value>
        /// The modified date time.
        /// </value>
        [DataMember]
        public DateTime? ModifiedDateTime { get; set; }

        #endregion

        #region Virtual Properties
        // TODO
        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Person Configuration class.
    /// </summary>
    public partial class AnalyticsFactFinancialTransactionConfiguration : EntityTypeConfiguration<AnalyticsFactFinancialTransaction>
    {
        public AnalyticsFactFinancialTransactionConfiguration()
        {
  
        }
    }

    #endregion
}

