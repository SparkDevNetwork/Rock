//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Rock.Data;

namespace Rock.Crm
{
    /// <summary>
    /// GroupLocation POCO class.
    /// </summary>
    [Table( "crmGroupLocation" )]
    public partial class GroupLocation
    {
        /// <summary>
        /// Gets or sets the group id.
        /// </summary>
        /// <value>
        /// The group id.
        /// </value>
        [Key]
        [Column(Order = 0)]
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the location id.
        /// </summary>
        /// <value>
        /// The location id.
        /// </value>
        [Key]
        [Column(Order = 1)]
        public int LocationId { get; set; }

        /// <summary>
        /// Gets or sets the location type.
        /// </summary>
        /// <value>
        /// The location type.
        /// </value>
        public int? LocationTypeId { get; set; }

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        public virtual Rock.Crm.Location Location { get; set; }

        /// <summary>
        /// Gets or sets the Location Type.
        /// </summary>
        /// <value>
        /// A <see cref="Core.DefinedValue"/> object.
        /// </value>
        public virtual Core.DefinedValue LocationType { get; set; }
    }

    /// <summary>
    /// GroupLocation Configuration class
    /// </summary>
    public partial class GroupLocationConfiguration : EntityTypeConfiguration<GroupLocation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupLocationConfiguration"/> class.
        /// </summary>
        public GroupLocationConfiguration()
        {
            this.HasRequired( t => t.Group ).WithMany( t => t.Locations ).HasForeignKey( t => t.GroupId );
            this.HasRequired( t => t.Location ).WithMany().HasForeignKey( t => t.LocationId );
            this.HasOptional( t => t.LocationType ).WithMany().HasForeignKey( t => t.LocationTypeId ).WillCascadeOnDelete( false );
        }
    }
}