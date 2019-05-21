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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// AnalyticsFactFinancialTransaction is SQL View based on AnalyticsSourceFinancialTransaction
    /// and represents the fact record for an Analytic Fact Financial Transaction in Rock.
    /// </summary>
    [RockDomain( "Reporting" )]
    [Table( "AnalyticsFactFinancialTransaction" )]
    [DataContract]
    public class AnalyticsFactFinancialTransaction : AnalyticsBaseFinancialTransaction<AnalyticsFactFinancialTransaction>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the type of the transaction.
        /// </summary>
        /// <value>
        /// The type of the transaction.
        /// </value>
        [DataMember]
        [DefinedValue]
        public string TransactionType { get; set; }

        /// <summary>
        /// Gets or sets the transaction source.
        /// </summary>
        /// <value>
        /// The transaction source.
        /// </value>
        [DataMember]
        public string TransactionSource { get; set; }

        /// <summary>
        /// Gets or sets the type of the schedule. (Scheduled or Non-Scheduled)
        /// </summary>
        /// <value>
        /// The type of the schedule.
        /// </value>
        [DataMember]
        public string ScheduleType { get; set; }

        /// <summary>
        /// Gets or sets the processed by person key.
        /// </summary>
        /// <value>
        /// The processed by person key.
        /// </value>
        [DataMember]
        public int? ProcessedByPersonKey { get; set; }

        /// <summary>
        /// This is the FamilyKey (AnalyticsDimFamilyCurrent.Id) of the family of the Authorized Person that did this transaction
        /// Note that this is the family that the person was in at the time of the transaction
        /// To see what GivingGroup they were part of when the Transaction occurred, see GivingUnitKey
        /// </summary>
        /// <value>
        /// The authorized family key.
        /// </value>
        [DataMember]
        public int? AuthorizedFamilyKey { get; set; }

        /// <summary>
        /// This is the FamilyKey (AnalyticsDimFamilyCurrent.Id) of the family of the Authorized Person that did this transaction
        /// Note that this is the family that the person is in now
        /// To see what GivingGroup they were part of when the Transaction occurred, see GivingUnitKey
        /// </summary>
        /// <value>
        /// The authorized family key.
        /// </value>
        [DataMember]
        public int? AuthorizedCurrentFamilyKey { get; set; }

        /// <summary>
        /// Gets or sets the giving unit key, which is the GivingGroup the person was in at the time of the transaction
        /// </summary>
        /// <value>
        /// The giving unit key.
        /// </value>
        [DataMember]
        public int? GivingUnitKey { get; set; }

        /// <summary>
        /// Gets or sets the current giving unit key, which is the GivingGroup the person is in now
        /// </summary>
        /// <value>
        /// The giving unit key.
        /// </value>
        [DataMember]
        public int? GivingUnitCurrentKey { get; set; }

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
        /// Gets or sets the type of the currency.
        /// </summary>
        /// <value>
        /// The type of the currency.
        /// </value>
        [DataMember]
        public string CurrencyType { get; set; }

        /// <summary>
        /// Gets or sets the type of the credit card.
        /// </summary>
        /// <value>
        /// The type of the credit card.
        /// </value>
        [DataMember]
        public string CreditCardType { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class AnalyticsFactFinancialTransactionConfiguration : EntityTypeConfiguration<AnalyticsFactFinancialTransaction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsFactFinancialTransactionConfiguration"/> class.
        /// </summary>
        public AnalyticsFactFinancialTransactionConfiguration()
        {
            // NOTE: When creating a migration for this, don't create the actual FK's in the database for this just in case there are outlier TransactionDates that aren't in the AnalyticsSourceDate table
            // and so that the AnalyticsSourceDate can be rebuilt from scratch as needed
            this.HasRequired( t => t.TransactionDate ).WithMany().HasForeignKey( t => t.TransactionDateKey ).WillCascadeOnDelete( false );

            // NOTE: When creating a migration for this, don't create the actual FK's in the database for any of these since they are views
            this.HasOptional( t => t.Batch ).WithMany().HasForeignKey( t => t.BatchId ).WillCascadeOnDelete( false );
            this.HasRequired( t => t.Account ).WithMany().HasForeignKey( t => t.AccountId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}