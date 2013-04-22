//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Batch POCO class.
    /// </summary>
    [Table( "FinancialBatch" )]
    [DataContract]
    public partial class FinancialBatch : Model<FinancialBatch>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the batch date.
        /// </summary>
        /// <value>
        /// The batch date.
        /// </value>
        [DataMember]
        public DateTime? BatchDate { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [DataMember]
        public BatchStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the campus id.
        /// </summary>
        /// <value>
        /// The campus id.
        /// </value>
        [DataMember]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets an optional transaction code from an accounting system that batch is associated with
        /// </summary>
        /// <value>
        /// The accounting system code.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string AccountingSystemCode { get; set; }

        /// <summary>
        /// Gets or sets the control amount.
        /// </summary>
        /// <value>
        /// The control amount.
        /// </value>
        [DataMember]
        public decimal ControlAmount { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the campus.
        /// </summary>
        /// <value>
        /// The campus.
        /// </value>
        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets the transactions.
        /// </summary>
        /// <value>
        /// The transactions.
        /// </value>
        [DataMember]
        public virtual ICollection<FinancialTransaction> Transactions { get; set; }

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
            return this.Name;
        }

        #endregion
    }

    #region EntityConfiguration

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

    #endregion

    #region Enumerations

    /// <summary>
    /// The status of a batch
    /// </summary>
    public enum BatchStatus
    {
        /// <summary>
        /// Pending
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Open
        /// </summary>
        Open = 1,

        /// <summary>
        /// Closed
        /// </summary>
        Closed = 2
    }

    #endregion


}