// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a detail item of a <see cref="Rock.Model.FinancialScheduledTransaction"/>.  It represents a gift/payment to be made to an <see cref="Rock.Model.FinancialAccount"/>/account
    /// as part of the scheduled transaction. This allows multiple payments/gifts to be consolidated into one scheduled transaction.
    /// </summary>
    [Table( "FinancialScheduledTransactionDetail" )]
    [DataContract]
    public partial class FinancialScheduledTransactionDetail : Model<FinancialScheduledTransactionDetail>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the ScheduledTransactionId of the <see cref="Rock.Model.FinancialScheduledTransaction"/> that this detail item belongs to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the ScheudledTransactionId of the <see cref="Rock.Model.FinancialScheduledTransaction"/> that this detail item belongs to.
        /// </value>
        [DataMember]
        public int ScheduledTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the AccountId of the <see cref="Rock.Model.FinancialAccount"/>/account that that the transaction detail <see cref="Amount"/> should be directed toward.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the AccountId of the <see cref="Rock.Model.FinancialAccount"/>/account that this transaction detail is directed toward.
        /// </value>
        [DataMember]
        public int AccountId { get; set; }

        /// <summary>
        /// Gets or sets the purchase/gift amount.
        /// </summary>
        /// <value>
        /// A <see cref="System.Decimal"/> representing the purchase/gift amount.
        /// </value>
        [DataMember]
        [BoundFieldTypeAttribute( typeof( Rock.Web.UI.Controls.CurrencyField ) )]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the summary of this scheduled transaction detail.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the summary of this scheduled transaction detail.
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

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialScheduledTransaction"/> that this transaction detail belongs to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.FinancialScheduledTransaction"/> that the transaction detail belongs to.
        /// </value>
        public virtual FinancialScheduledTransaction ScheduledTransaction { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialAccount"/>/account that the <see cref="Amount"/> of this transaction detail will be credited toward.
        /// </summary>
        /// <value>
        /// Tehe <see cref="Rock.Model.FinancialAccount"/>/account that the <see cref="Amount"/> of this transaction detail will be credited toward.
        /// </value>
        public virtual FinancialAccount Account { get; set; }

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Amount.ToStringSafe();
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// TransactionDetail Configuration class
    /// </summary>
    public partial class FinancialScheduledTransactionDetailConfiguration : EntityTypeConfiguration<FinancialScheduledTransactionDetail>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialScheduledTransactionDetailConfiguration"/> class.
        /// </summary>
        public FinancialScheduledTransactionDetailConfiguration()
        {
            this.HasRequired( d => d.ScheduledTransaction ).WithMany( t => t.ScheduledTransactionDetails ).HasForeignKey( d => d.ScheduledTransactionId ).WillCascadeOnDelete( true );
            this.HasRequired( d => d.Account ).WithMany().HasForeignKey( d => d.AccountId ).WillCascadeOnDelete( false );
            this.HasOptional( d => d.EntityType ).WithMany().HasForeignKey( d => d.EntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}