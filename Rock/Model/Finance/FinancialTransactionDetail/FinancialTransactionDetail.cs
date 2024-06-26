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
using Rock.Financial;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// Represents a transaction detail line item for a <see cref="Rock.Model.FinancialTransaction"/> in Rock.
    /// </summary>
    [RockDomain( "Finance" )]
    [Table( "FinancialTransactionDetail" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.FINANCIAL_TRANSACTION_DETAIL )]
    public partial class FinancialTransactionDetail : Model<FinancialTransactionDetail>, ITransactionDetail
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the TransactionId of the <see cref="Rock.Model.FinancialTransaction"/> that this 
        /// detail item is a part of.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the TransactionDetailId of the <see cref="Rock.Model.FinancialTransaction"/>
        /// that this detail item is a part of.
        /// </value>
        [DataMember]
        public int TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the AccountId of the <see cref="Rock.Model.FinancialAccount"/>/account that the <see cref="Amount"/> of this 
        /// detail line item should be credited towards.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the <see cref="Rock.Model.FinancialAccount"/>/account that is affected by this
        /// transaction detail.
        /// </value>
        [DataMember]
        [HideFromReporting]
        public int AccountId { get; set; }

        /// <summary>
        /// Gets or sets the total amount of the transaction detail. This total amount includes any associated fees.
        /// </summary>
        /// <value>
        /// A <see cref="System.Decimal"/> representing the total amount of the transaction detail.
        /// </value>
        [DataMember]
        [BoundFieldType( typeof( Rock.Web.UI.Controls.CurrencyField ) )]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the summary of the transaction detail.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the summary of the transaction detail.
        /// </value>
        [MaxLength( 500 )]
        [DataMember]
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity id.
        /// </summary>
        /// <value>
        /// The entity id.
        /// </value>
        [DataMember]
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the fee amount of the transaction detail, which is a subset of the Amount.
        /// </summary>
        /// <remarks>
        /// This is the actual fee amount that has been charged by the payment processor after the transaction has settled.
        /// </remarks>
        /// <value>
        /// A <see cref="System.Decimal"/> representing the fee amount of the transaction detail.
        /// </value>
        [DataMember]
        [BoundFieldType( typeof( Rock.Web.UI.Controls.CurrencyField ) )]
        [IncludeAsEntityProperty]
        public decimal? FeeAmount { get; set; }

        /// <summary>
        /// Gets or sets the fee coverage amount.
        /// </summary>
        /// <remarks>
        /// This is an estimated fee amount the contributing individual has agreed to cover.
        /// </remarks>
        /// <value>
        /// The fee coverage amount.
        /// </value>
        [DataMember]
        [BoundFieldType( typeof( Rock.Web.UI.Controls.CurrencyField ) )]
        [DecimalPrecision(18, 2)]
        public decimal? FeeCoverageAmount { get; set; }

        /// <summary>
        /// Gets or sets the foreign currency amount.
        /// </summary>
        /// <value>
        /// The foreign currency amount.
        /// </value>
        /// /// <remarks>
        /// This value will be in the currency specified by the Financial Transaction's Foreign Currency Code which defaults to USD.
        /// </remarks>
        [DataMember]
        [BoundFieldType( typeof( Web.UI.Controls.CurrencyField ) )]
        [DecimalPrecision( 18, 2 )]
        public decimal? ForeignCurrencyAmount { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialTransaction"/> that this detail item belongs to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.FinancialTransaction"/> that this detail item belongs to.
        /// </value>
        [LavaVisible]
        public virtual FinancialTransaction Transaction { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialAccount"/> that is affected by this detail line item.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.FinancialAccount"/> that is affected by this detail line item.
        /// </value>
        [LavaVisible]
        [DataMember]
        public virtual FinancialAccount Account { get; set; }

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the history change list.
        /// </summary>
        /// <value>
        /// The history change list.
        /// </value>
        [NotMapped]
        [RockObsolete( "1.14" )]
        [Obsolete( "Does nothing. No longer needed. We replaced this with a private property under the SaveHook class for this entity.", true )]
        public virtual History.HistoryChangeList HistoryChangeList { get; set; }

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// TransactionDetail Configuration class
    /// </summary>
    public partial class FinancialTransactionDetailConfiguration : EntityTypeConfiguration<FinancialTransactionDetail>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionDetailConfiguration"/> class.
        /// </summary>
        public FinancialTransactionDetailConfiguration()
        {
            this.HasRequired( d => d.Transaction ).WithMany( t => t.TransactionDetails ).HasForeignKey( d => d.TransactionId ).WillCascadeOnDelete( true );
            this.HasRequired( d => d.Account ).WithMany().HasForeignKey( d => d.AccountId ).WillCascadeOnDelete( false );
            this.HasOptional( d => d.EntityType ).WithMany().HasForeignKey( d => d.EntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}