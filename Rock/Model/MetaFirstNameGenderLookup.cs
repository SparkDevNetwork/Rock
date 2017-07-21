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
    /// Represents entry to Look up on the basis of Gender <see cref="Rock.Model.MetaFirstNameGenderLookup"/>. 
    /// </summary>
    [RockDomain( "Meta" )]
    [Table( "MetaFirstNameGenderLookup" )]
    [DataContract]
    public class MetaFirstNameGenderLookup : Entity<MetaFirstNameGenderLookup>
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
        /// Gets or sets the male count.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing Male Count.
        /// </value>
        [DataMember]
        public int? MaleCount { get; set; }

        /// <summary>
        /// Gets or sets the female count.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing female Count.
        /// </value>
        [DataMember]
        public int? FemaleCount { get; set; }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing country.
        /// </value>
        [MaxLength( 10 )]
        [DataMember]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing language.
        /// </value>
        [MaxLength( 10 )]
        [DataMember]
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the total count.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing total Count.
        /// </value>
        [DataMember]
        public int? TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the female percent.
        /// </summary>
        /// <value>
        /// A <see cref="System.Decimal"/> representing female percent.
        /// </value>
        [DataMember]
        public decimal? FemalePercent { get; set; }

        /// <summary>
        /// Gets or sets the male percent.
        /// </summary>
        /// <value>
        /// A <see cref="System.Decimal"/> representing male percent.
        /// </value>
        [DataMember]
        public decimal? MalePercent { get; set; }

        #endregion

        #region Virtual Properties

        #endregion

        #region Public Methods

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// MetaFirstNameGenderLookup Configuration class.
    /// </summary>
    public partial class MetaFirstNameGenderLookupConfiguration : EntityTypeConfiguration<MetaFirstNameGenderLookup>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetaFirstNameGenderLookupConfiguration" /> class.
        /// </summary>
        public MetaFirstNameGenderLookupConfiguration()
        {

        }
    }

    #endregion
}
