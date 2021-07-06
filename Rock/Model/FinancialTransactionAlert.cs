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
    /// 
    /// </summary>
    [RockDomain( "Finance" )]
    [Table( "FinancialTransactionAlert" )]
    [DataContract]
    public class FinancialTransactionAlert : Model<FinancialTransactionAlert>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// </value>
        [DataMember]
        public int? TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the person <see cref="Rock.Model.Person"/> who is associated with the transaction.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PersonAliasId of <see cref="Rock.Model.PersonAlias"/> associated with the transaction.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the giving identifier.
        /// </summary>
        /// <value>
        /// The giving identifier.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string GivingId { get; set; }

        /// <summary>
        /// Gets or sets the financial transaction alert type identifier.
        /// </summary>
        /// <value>
        /// The financial transaction alert type identifier.
        /// </value>
        [DataMember]
        public int AlertTypeId { get; set; }

        /// <summary>
        /// Gets or sets the amount of financial transaction
        /// </summary>
        /// <value>
        /// The amount of financial transaction.
        /// </value>
        [DataMember]
        [DecimalPrecision( 18, 2 )]
        public decimal? Amount { get; set; }

        /// <summary>
        /// Gets or sets the amount current median
        /// </summary>
        /// <value>
        /// The amount current median
        /// </value>
        [DataMember]
        [DecimalPrecision( 18, 2 )]
        public decimal? AmountCurrentMedian { get; set; }

        /// <summary>
        /// Gets or sets the amount current interquartile range.
        /// </summary>
        /// <value>
        /// The amount current interquartile range.
        /// </value>
        [DataMember]
        [DecimalPrecision( 18, 2 )]
        public decimal? AmountCurrentIqr { get; set; }

        /// <summary>
        /// Gets or sets the amount interquartile range multiplier.
        /// </summary>
        /// <value>
        /// The amount interquartile range multiplier.
        /// </value>
        [DataMember]
        [DecimalPrecision( 6, 1 )]
        public decimal? AmountIqrMultiplier { get; set; }

        /// <summary>
        /// Gets or sets the frequency current mean.
        /// </summary>
        /// <value>
        /// The frequency current mean.
        /// </value>
        [DataMember]
        [DecimalPrecision( 6, 1 )]
        public decimal? FrequencyCurrentMean { get; set; }

        /// <summary>
        /// Gets or sets the frequency current standard deviation.
        /// </summary>
        /// <value>
        /// The frequency current standard deviation.
        /// </value>
        [DataMember]
        [DecimalPrecision( 6, 1 )]
        public decimal? FrequencyCurrentStandardDeviation { get; set; }

        /// <summary>
        /// Gets or sets the frequency difference from mean.
        /// </summary>
        /// <value>
        /// The frequency difference from mean.
        /// </value>
        [DataMember]
        [DecimalPrecision( 6, 1 )]
        public decimal? FrequencyDifferenceFromMean { get; set; }

        /// <summary>
        /// Gets or sets the frequency Z score.
        /// </summary>
        /// <value>
        /// The frequency Z score.
        /// </value>
        [DataMember]
        [DecimalPrecision( 6, 1 )]
        public decimal? FrequencyZScore { get; set; }

        /// <summary>
        /// Gets or sets the reason key.
        /// </summary>
        /// <value>
        /// The reason key.
        /// </value>
        [DataMember]
        [MaxLength( 2500 )]
        public string ReasonsKey { get; set; }

        /// <summary>
        /// Gets or sets the alert date time.
        /// </summary>
        /// <value>
        /// The alert date time.
        /// </value>
        [DataMember]
        [Index]
        public DateTime AlertDateTime { get; set; }

        /// <summary>
        /// Gets or sets the alert date key.
        /// </summary>
        /// <value>
        /// The alert date key.
        /// </value>
        [DataMember]
        public int AlertDateKey { get; set; }

        #endregion Entity Properties

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the transaction that this financial transaction alert is associated with.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Campus"/> that the transaction that this financial transaction alert is associated with.
        /// </value>
        [DataMember]
        public virtual FinancialTransaction FinancialTransaction { get; set; }

        /// <summary>
        /// Gets or sets the person alias <see cref="Rock.Model.Person"/> associated with the financial transaction alert.
        /// </summary>
        /// <value>
        /// A person alias <see cref="Rock.Model.PersonAlias"/> asssociated to the financial transaction alert.
        /// </value>
        [DataMember]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the type of the financial transaction alert.
        /// </summary>
        /// <value>
        /// The type of the financial transaction alert.
        /// </value>
        [DataMember]
        public virtual FinancialTransactionAlertType FinancialTransactionAlertType { get; set; }

        #endregion Virtual Properties

        #region Methods

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if ( ( FinancialTransactionAlertType?.Name ).IsNullOrWhiteSpace() )
            {
                return base.ToString();
            }

            if ( ( PersonAlias?.Person?.FullName ).IsNullOrWhiteSpace() )
            {
                return base.ToString();
            }

            return $"{FinancialTransactionAlertType.Name}: {PersonAlias.Person.FullName}";
        }

        #endregion Methods
    }

    #region Entity Configuration

    /// <summary>
    /// FinancialTransactionAlert Configuration class.
    /// </summary>
    public partial class FinancialTransactionAlertConfiguration : EntityTypeConfiguration<FinancialTransactionAlert>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionAlertConfiguration"/> class.
        /// </summary>
        public FinancialTransactionAlertConfiguration()
        {
            this.HasOptional( t => t.FinancialTransaction ).WithMany().HasForeignKey( t => t.TransactionId ).WillCascadeOnDelete( false );
            this.HasRequired( t => t.PersonAlias ).WithMany().HasForeignKey( t => t.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasRequired( t => t.FinancialTransactionAlertType ).WithMany( t => t.FinancialTransactionAlerts ).HasForeignKey( t => t.AlertTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}