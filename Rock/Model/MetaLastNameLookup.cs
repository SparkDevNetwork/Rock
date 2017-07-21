using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a MetaLastNameLookup <see cref="Rock.Model.MetaLastNameLookup"/>. 
    /// </summary>
    [RockDomain( "Meta" )]
    [Table( "MetaLastNameLookup" )]
    [DataContract]
    public class MetaLastNameLookup : Entity<MetaLastNameLookup>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing last name.
        /// </value>
        [Required]
        [MaxLength( 10 )]
        [DataMember( IsRequired = true )]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the Count.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing count.
        /// </value>
        [DataMember]
        public int? Count { get; set; }

        /// <summary>
        /// Gets or sets the Rank.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing Rank.
        /// </value>
        [DataMember]
        public int? Rank { get; set; }

        /// <summary>
        /// Gets or sets the count in 100k.
        /// </summary>
        /// <value>
        /// A <see cref="System.Decimal"/> representing the count in 100k.
        /// </value>
        [DataMember]
        public decimal? CountIn100k { get; set; }

        #endregion

        #region Virtual Properties

        #endregion

        #region Public Methods

        #endregion
    }

    /// <summary>
    /// MetaLastNameLookup Configuration class.
    /// </summary>
    public partial class MetaLastNameLookupConfiguration : EntityTypeConfiguration<MetaLastNameLookup>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetaLastNameLookupConfiguration" /> class.
        /// </summary>
        public MetaLastNameLookupConfiguration()
        {

        }
    }
}
