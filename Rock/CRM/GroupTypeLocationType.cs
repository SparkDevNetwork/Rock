//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
namespace Rock.Crm
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "GroupTypeLocationType" )]
    public class GroupTypeLocationType
    {
        /// <summary>
        /// Gets or sets the group type id.
        /// </summary>
        /// <value>
        /// The group type id.
        /// </value>
        [Key]
        [Column(Order=0)]
        public int GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the location type id.
        /// </summary>
        /// <value>
        /// The location type id.
        /// </value>
        [Key]
        [Column( Order = 1 )]
        public int LocationTypeId { get; set; }

        /// <summary>
        /// Gets or sets the type of the group.
        /// </summary>
        /// <value>
        /// The type of the group.
        /// </value>
        public virtual GroupType GroupType { get; set; }

        /// <summary>
        /// Gets or sets the type of the location.
        /// </summary>
        /// <value>
        /// The type of the location.
        /// </value>
        public virtual Core.DefinedValue LocationType { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class GroupTypeLocationTypeConfiguration : EntityTypeConfiguration<GroupTypeLocationType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupTypeLocationTypeConfiguration" /> class.
        /// </summary>
        public GroupTypeLocationTypeConfiguration()
        {
            this.HasRequired( t => t.GroupType ).WithMany().HasForeignKey( t => t.GroupTypeId );
            this.HasRequired( t => t.LocationType ).WithMany().HasForeignKey( t => t.LocationTypeId );
        }
    }

}