//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.CRM;
using Rock.Data;

namespace Rock.Financial
{
    /// <summary>
    /// Batch POCO class.
    /// </summary>
    [Table("financialBatch")]
    public partial class Batch : ModelWithAttributes<Batch>, IAuditable
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        [MaxLength(50)]
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
        /// Gets or sets a value indicating whether this <see cref="Batch"/> is closed.
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
        [DataMember]
        [MaxLength(50)]
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
        [DataMember]
        [MaxLength(50)]
        public string ForeignReference { get; set; }

        //public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets the transactions.
        /// </summary>
        /// <value>
        /// The transactions.
        /// </value>
        public virtual ICollection<Transaction> Transactions { get; set; }

        /// <summary>
        /// Gets or sets the modified date time.
        /// </summary>
        /// <value>
        /// The modified date time.
        /// </value>
        [DataMember]
        public DateTime? ModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the created date time.
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        [DataMember]
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the created by person id.
        /// </summary>
        /// <value>
        /// The created by person id.
        /// </value>
        [DataMember]
        public int? CreatedByPersonId { get; set; }

        /// <summary>
        /// Gets or sets the modified by person id.
        /// </summary>
        /// <value>
        /// The modified by person id.
        /// </value>
        [DataMember]
        public int? ModifiedByPersonId { get; set; }

        /// <summary>
        /// Gets or sets the created by person.
        /// </summary>
        /// <value>
        /// The created by person.
        /// </value>
        public virtual Person CreatedByPerson { get; set; }

        /// <summary>
        /// Gets or sets the modified by person.
        /// </summary>
        /// <value>
        /// The modified by person.
        /// </value>
        public virtual Person ModifiedByPerson { get; set; }

        /// <summary>
        /// Gets the auth entity.
        /// </summary>
        public override string AuthEntity { get { return "Financial.Batch"; } }
    }

    /// <summary>
    /// Batch Configuration class.
    /// </summary>
    public partial class BatchConfiguration : EntityTypeConfiguration<Batch>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BatchConfiguration"/> class.
        /// </summary>
        public BatchConfiguration()
        {
            this.HasOptional(p => p.CreatedByPerson).WithMany().HasForeignKey(p => p.CreatedByPersonId).WillCascadeOnDelete(false);
            this.HasOptional(p => p.ModifiedByPerson).WithMany().HasForeignKey(p => p.ModifiedByPersonId).WillCascadeOnDelete(false);
        }
    }
}