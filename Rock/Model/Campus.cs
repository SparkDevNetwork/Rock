//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Campus POCO Entity.
    /// </summary>
    [Table( "Campus" )]
    [DataContract]
    public partial class Campus : Model<Campus>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System indicates whether or not the campus is part of the core framework/system.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        /// <value>
        /// Given Name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [AlternateKey]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the campus abbreviation/short code.
        /// </summary>
        /// <value>
        /// Short Code.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string ShortCode { get; set; }

        /// <summary>
        /// Gets or sets the location id.
        /// </summary>
        /// <value>
        /// The location id.
        /// </value>
        [DataMember]
        public int? LocationId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        [DataMember]
        public virtual Location Location { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Campus Configuration class.
    /// </summary>
    public partial class CampusConfiguration : EntityTypeConfiguration<Campus>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CampusConfiguration"/> class.
        /// </summary>
        public CampusConfiguration()
        {
            this.HasOptional( c => c.Location ).WithMany().HasForeignKey( c => c.LocationId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}
