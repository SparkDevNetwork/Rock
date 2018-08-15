using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using org.newpointe.Stars.Data;

using Rock.Data;
using Rock.Model;

namespace org.newpointe.Stars.Model
{
    /// <summary>
    /// A Stars Model
    /// </summary>
    [Table( "_org_newpointe_Stars_Transactions" )]
    [DataContract]
    public class Stars : Rock.Data.Model<Stars>, Rock.Security.ISecured
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the Person Alias Id - for the person the Stars transaction is for.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required( ErrorMessage = "PersonAliasId is required")]
        [DataMember( IsRequired = true )]
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the transaction value.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [Required(ErrorMessage = "Value is required")]
        [DataMember]
        public decimal Value { get; set; }


        /// <summary>
        /// Gets or sets the transaction note.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Note { get; set; }


        /// <summary>
        /// Gets or sets the Transaction Date and Time.
        /// </summary>
        /// <value>
        /// The name of the contact.
        /// </value>
        [Required(ErrorMessage = "TransactionDateTime is required")]
        [DataMember]
        public DateTime TransactionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        [DataMember]
        public int? CampusId { get; set; }


        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the campus.
        /// </summary>
        /// <value>
        /// The campus.
        /// </value>
        public virtual Campus Campus { get; set; }


        public virtual PersonAlias PersonAlias { get; set; }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class StarsConfiguration : EntityTypeConfiguration<Stars>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StarsConfiguration"/> class.
        /// </summary>
        public StarsConfiguration()
        {
            this.HasOptional( r => r.Campus ).WithMany().HasForeignKey( r => r.CampusId).WillCascadeOnDelete( false );
        }
    }

    #endregion

}
