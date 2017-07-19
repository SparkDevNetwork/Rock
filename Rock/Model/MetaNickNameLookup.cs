using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents entry to Look up on the basis of Nick Name <see cref="Rock.Model.MetaNickNameLookup"/>. 
    /// </summary>
    [RockDomain( "Meta" )]
    [Table( "MetaNickNameLookup" )]
    [DataContract]
    public class MetaNickNameLookup : Entity<MetaNickNameLookup>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing first name.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the nick name.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing nick name.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing gender.
        /// </value>
        [MaxLength( 1 )]
        [Column( TypeName = "char" )]
        [DataMember]
        public string Gender { get; set; }

        /// <summary>
        /// Gets or sets the Count.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing count.
        /// </value>
        [DataMember]
        public int? Count { get; set; }

        #endregion

        #region Virtual Properties

        #endregion

        #region Public Methods

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// MetaNickNameLookup Configuration class.
    /// </summary>
    public partial class MetaNickNameLookupConfiguration : EntityTypeConfiguration<MetaNickNameLookup>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetaNickNameLookupConfiguration" /> class.
        /// </summary>
        public MetaNickNameLookupConfiguration()
        {
         
        }
    }

    #endregion

}
