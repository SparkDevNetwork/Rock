﻿// <copyright>
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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Financial;

namespace Rock.Model
{
    /// <summary>
    /// Represents a transaction detail line item for a <see cref="Rock.Model.FinancialTransaction"/> in Rock.
    /// </summary>
    [RockDomain( "Finance" )]
    [Table( "FinancialTransactionDetail" )]
    [DataContract]
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
        /// Gets or sets a value indicating whether this is a non-cash detail.
        /// </summary>
        /// <value>
        ///   <c>true</c> if non-cash; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        [Obsolete("Field is not used. Non Cash transactions can be designated by the currency (tender) type.")]
        public bool IsNonCash { get; set; }

        /// <summary>
        /// Gets or sets the amount of the transaction detail.
        /// </summary>
        /// <value>
        /// A <see cref="System.Decimal"/> representing the amount of the transaction detail.
        /// </value>
        [DataMember]
        [BoundFieldTypeAttribute(typeof(Rock.Web.UI.Controls.CurrencyField))]
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

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialTransaction"/> that this detail item belongs to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.FinancialTransaction"/> that this detail item belongs to.
        /// </value>
        [LavaInclude]
        public virtual FinancialTransaction Transaction { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialAccount"/> that is affected by this detail line item.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.FinancialAccount"/> that is affected by this detail line item.
        /// </value>
        [LavaInclude]
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
        /// Gets or sets the history changes.
        /// </summary>
        /// <value>
        /// The history changes.
        /// </value>
        [NotMapped]
        public virtual List<string> HistoryChanges { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this detail item.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this detail item.
        /// </returns>
        public override string ToString()
        {
            return this.Amount.ToStringSafe();
        }

        /// <summary>
        /// Method that will be called on an entity immediately before the item is saved by context
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="entry"></param>
        public override void PreSaveChanges( Rock.Data.DbContext dbContext, System.Data.Entity.Infrastructure.DbEntityEntry entry )
        {
            var rockContext = (RockContext)dbContext;
            HistoryChanges = new List<string>();

            switch ( entry.State )
            {
                case System.Data.Entity.EntityState.Added:
                    {
                        string acct = History.GetValue<FinancialAccount>( this.Account, this.AccountId, rockContext );
                        HistoryChanges.Add( string.Format( "Added <span class='field-name'>{0}</span> account for <span class='field-value'>{1}</span>.", acct, Amount.FormatAsCurrency() ) );
                        break;
                    }

                case System.Data.Entity.EntityState.Modified:
                    {
                        string acct = History.GetValue<FinancialAccount>( this.Account, this.AccountId, rockContext );

                        int? accountId = this.Account != null ? this.Account.Id : this.AccountId;
                        int? origAccountId = entry.OriginalValues["AccountId"].ToStringSafe().AsIntegerOrNull();
                        if ( !accountId.Equals( origAccountId ) )
                        {
                            History.EvaluateChange( HistoryChanges, "Account", History.GetValue<FinancialAccount>( null, origAccountId, rockContext ), acct );
                        }

                        History.EvaluateChange( HistoryChanges, acct, entry.OriginalValues["Amount"].ToStringSafe().AsDecimal().FormatAsCurrency(), Amount.FormatAsCurrency() );

                        break;
                    }
                case System.Data.Entity.EntityState.Deleted:
                    {
                        string acct = History.GetValue<FinancialAccount>( this.Account, this.AccountId, rockContext );
                        HistoryChanges.Add( string.Format( "Removed <span class='field-name'>{0}</span> account for <span class='field-value'>{1}</span>.", acct, Amount.FormatAsCurrency() ) );
                        break;
                    }
            }

            base.PreSaveChanges( dbContext, entry );
        }

        /// <summary>
        /// Method that will be called on an entity immediately after the item is saved
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public override void PostSaveChanges( DbContext dbContext )
        {
            if ( HistoryChanges.Any() )
            {
                HistoryService.SaveChanges( (RockContext)dbContext, typeof( FinancialTransaction ), Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(), this.TransactionId, HistoryChanges, true, this.ModifiedByPersonAliasId );

                var txn = new FinancialTransactionService( (RockContext)dbContext ).Get( this.TransactionId );
                if ( txn != null && txn.BatchId != null )
                {
                    var batchHistory = new List<string> { string.Format( "Updated <span class='field-name'>Transaction</span> ID: <span class='field-value'>{0}</span>.", txn.Id ) };
                    HistoryService.SaveChanges( (RockContext)dbContext, typeof( FinancialBatch ), Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(), txn.BatchId.Value, batchHistory, string.Empty, typeof( FinancialTransaction ), this.TransactionId, true, this.ModifiedByPersonAliasId );
                }
            }

            base.PostSaveChanges( dbContext );
        }

        #endregion

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

    #endregion

}