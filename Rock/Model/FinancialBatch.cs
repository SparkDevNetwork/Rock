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
using System.Collections;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Batch POCO class.
    /// </summary>
    [Table("FinancialBatch")]
    [DataContract( IsReference = true )]
    public partial class FinancialBatch : Model<FinancialBatch>
    {
        
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [MaxLength(50)]
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the batch date start.
        /// </summary>
        /// <value>
        /// The batch date start.
        /// </value>
        [DataMember]
        public DateTime? BatchDateStart { get; set; }

        /// <summary>
        /// Gets or sets the batch date end.
        /// </summary>
        /// <value>
        /// The batch date end.
        /// </value>
        [DataMember]
        public DateTime? BatchDateEnd { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="FinancialBatch"/> is closed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if closed; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsClosed { get; set; }

        /// <summary>
        /// Gets or sets the campus id.
        /// </summary>
        /// <value>
        /// The campus id.
        /// </value>
        [DataMember]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        [MaxLength(50)]
        [DataMember]
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the entity id.
        /// </summary>
        /// <value>
        /// The entity id.
        /// </value>
        [DataMember]
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the foreign reference.
        /// </summary>
        /// <value>
        /// The foreign reference.
        /// </value>
        [MaxLength(50)]
        [DataMember]
        public string ForeignReference { get; set; }

        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets the Batch Type Value Id.
        /// </summary>
        /// <value>
        /// Batch Type Value Id.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [ForeignKey( "BatchType" )]
        public int BatchTypeValueId { get; set; }

        [DataMember]
        public virtual DefinedType BatchType { get; set; }

        /// <summary>
        /// Gets or sets the Control Amount.
        /// </summary>
        /// <value>
        /// Control Amount.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public float ControlAmount { get; set; }

        /// <summary>
        /// Gets or sets the transactions.
        /// </summary>
        /// <value>
        /// The transactions.
        /// </value>
        [DataMember]
        public virtual ICollection<FinancialTransaction> Transactions { get; set; }

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

        //////private List<FinancialTransaction> _TransactionList = null;
        //////public List<FinancialTransaction> TransactionList
        //////{
        //////    get {
        //////        if ( _TransactionList == null )
        //////        {
        //////            _TransactionList = Transactions.ToList();
        //////        }
        //////        return _TransactionList;
        //////    }
        //////}
    }

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
        }
    }
}