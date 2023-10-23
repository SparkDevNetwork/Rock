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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a batch or collection of <see cref="Rock.Model.FinancialTransaction">FinancialTransactions</see> for a specified date-time range, campus (if applicable) and transaction type.  A batch 
    /// has a known total value of all transactions that are included in the batch.
    /// </summary>
    [RockDomain( "Finance" )]
    [Table( "FinancialBatch" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.FINANCIAL_BATCH )]
    public partial class FinancialBatch : Model<FinancialBatch>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the batch.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the name of the batch.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the start posting date and time range of <see cref="Rock.Model.FinancialTransaction">FinancialTransactions</see> that are included in this batch.  
        /// Transactions that post on or after this date and time and before the <see cref="BatchEndDateTime"/> can be included in this batch.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the posting start date for the batch.
        /// </value>
        [DataMember]
        [Column( TypeName = "DateTime" )]
        public DateTime? BatchStartDateTime { get; set; }

        /// <summary>
        /// Gets or sets end of the posting date and time range for <see cref="Rock.Model.FinancialTransaction">FinancialTransactions</see> that are included in this batch.
        /// Transactions that post before or on this date and time and after the <see cref="BatchStartDateTime"/> can be included in this batch.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the posting end date for the batch.
        /// </value>
        [DataMember]
        [Column( TypeName = "DateTime" )]
        public DateTime? BatchEndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the status of the batch.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.BatchStatus"/> representing the status of the batch.
        /// When this value is <c>BatchStatus.Pending</c>  it means that transactions are still being added to the batch.
        /// When this value is <c>BatchStatus.Open</c> it means that all transactions have been added and are ready to be matched up.
        /// When this value is <c>BatchStatus.Closed</c> it means that the batch has balanced and has been closed.
        /// </value>
        [DataMember]
        public BatchStatus Status { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is automated.
        /// If IsAutomated is True, the UI should not allow the status of Pending to be changed to Open or Closed ( an external process will be in change of changing the status )
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is automated; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsAutomated { get; set; } = false;

        /// <summary>
        /// Gets or sets the CampusId of the <see cref="Rock.Model.Campus"/> that this batch is associated with. If the batch is not linked
        /// to a campus, this value will be null.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the CampusId of the <see cref="Rock.Model.Campus"/> that this batch is associated with.
        /// </value>
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.CAMPUS )]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets an optional transaction code from an accounting system that batch is associated with
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Accounting System transaction code for the batch.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string AccountingSystemCode { get; set; }

        /// <summary>
        /// Gets or sets the control amount. This should match the total value of all
        /// <see cref="Rock.Model.FinancialTransaction">FinancialTransactions</see> that are included in the batch.
        /// Use <see cref="FinancialBatchService.IncrementControlAmount"/> if you are incrementing the control amount
        /// based on a transaction amount.
        /// </summary>
        /// <value>
        /// A <see cref="System.Decimal"/> representing the control amount of the batch.
        /// </value>
        [DataMember]
        [BoundFieldType( typeof( Rock.Web.UI.Controls.CurrencyField ) )]
        public decimal ControlAmount { get; set; }

        /// <summary>
        /// Gets or sets the control item count.
        /// </summary>
        /// <value>
        /// The control item count.
        /// </value>
        [DataMember]
        public int? ControlItemCount { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>
        /// The note.
        /// </value>
        [DataMember]
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the URL to view the remote settlement information of
        /// the batch. This is usually set by gateways to view the related
        /// information on the back-end gateway's site.
        /// </summary>
        /// <value>
        /// The URL to view the remote settlement information of the batch.
        /// </value>
        [MaxLength( 300 )]
        [DataMember]
        public string RemoteSettlementBatchUrl { get; set; }

        /// <summary>
        /// Gets or sets the Batch Key for tracking an external system's settlement batch id/key
        /// </summary>
        /// <value>
        /// The Batch Key
        /// </value>
        public string RemoteSettlementBatchKey { get; set; }

        /// <summary>
        /// Gets or sets the external system's settlement batch amount.
        /// </summary>
        /// <value>
        /// The Settlement Batch Amount.
        /// </value>
        public decimal? RemoteSettlementAmount { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Campus"/> that this batch is associated with.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Campus"/> that the batch is associated with.
        /// </value>
        [DataMember]
        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets a collection that contains the <see cref="Rock.Model.FinancialTransaction">FinancialTransactions</see> that are 
        /// included in the batch.
        /// </summary>
        /// <value>
        /// A collection that contains the <see cref="Rock.Model.FinancialTransaction">FinancialTransactions</see> that are included in the batch.
        /// </value>
        [DataMember]
        public virtual ICollection<FinancialTransaction> Transactions
        {
            get { return _transactions ?? ( _transactions = new Collection<FinancialTransaction>() ); }
            set { _transactions = value; }
        }

        private ICollection<FinancialTransaction> _transactions;

        #endregion Navigation Properties

        #region ISecured overrides

        /// <summary>
        /// Provides a <see cref="Dictionary{TKey, TValue}"/> of actions that this model supports, and the description of each.
        /// </summary>
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                var supportedActions = base.SupportedActions;
                supportedActions.AddOrReplace( "Delete", "The roles and/or users that can delete a batch." );
                supportedActions.AddOrReplace( "ReopenBatch", "The roles and/or users that can reopen a closed batch." );
                return supportedActions;
            }
        }

        #endregion ISecured overrides
    }

    #region Entity Configuration

    /// <summary>
    /// Batch Configuration class.
    /// </summary>
    public partial class FinancialBatchConfiguration : EntityTypeConfiguration<FinancialBatch>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialBatchConfiguration"/> class.
        /// </summary>
        public FinancialBatchConfiguration()
        {
            this.HasOptional( b => b.Campus ).WithMany().HasForeignKey( b => b.CampusId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}