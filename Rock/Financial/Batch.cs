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
using Rock.Model;
using Rock.Data;

namespace Rock.Financial
{
    /// <summary>
    /// Batch POCO class.
    /// </summary>
    [Table("FinancialBatch")]
    public partial class FinancialBatch : Model<FinancialBatch>
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [MaxLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the batch date.
        /// </summary>
        /// <value>
        /// The batch date.
        /// </value>
        public DateTime? BatchDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="FinancialBatch"/> is closed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if closed; otherwise, <c>false</c>.
        /// </value>
        public bool IsClosed { get; set; }

        /// <summary>
        /// Gets or sets the campus id.
        /// </summary>
        /// <value>
        /// The campus id.
        /// </value>
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        [MaxLength(50)]
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the entity id.
        /// </summary>
        /// <value>
        /// The entity id.
        /// </value>
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the foreign reference.
        /// </summary>
        /// <value>
        /// The foreign reference.
        /// </value>
        [MaxLength(50)]
        public string ForeignReference { get; set; }

        //public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets the transactions.
        /// </summary>
        /// <value>
        /// The transactions.
        /// </value>
        public virtual ICollection<FinancialTransaction> Transactions { get; set; }

        /// <summary>
        /// Gets the dto.
        /// </summary>
        /// <returns></returns>
        public override IDto Dto
        {
            get { return this.ToDto(); }
        }

        /// <summary>
        /// Static Method to return an object based on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static FinancialBatch Read( int id )
        {
            return Read<FinancialBatch>( id );
        }

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
    }

    /// <summary>
    /// Batch Configuration class.
    /// </summary>
    public partial class BatchConfiguration : EntityTypeConfiguration<FinancialBatch>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BatchConfiguration"/> class.
        /// </summary>
        public BatchConfiguration()
        {
        }
    }
}