//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "GroupTypeLocationType" )]
    [DataContract( IsReference = true )]
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
        [DataMember]
        public int GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the location type id.
        /// </summary>
        /// <value>
        /// The location type id.
        /// </value>
        [Key]
        [Column( Order = 1 )]
        [DataMember]
        public int LocationTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the type of the group.
        /// </summary>
        /// <value>
        /// The type of the group.
        /// </value>
        [DataMember]
        public virtual GroupType GroupType { get; set; }

        /// <summary>
        /// Gets or sets the type of the location.
        /// </summary>
        /// <value>
        /// The type of the location.
        /// </value>
        [DataMember]
        public virtual Model.DefinedValue LocationTypeValue { get; set; }
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
            this.HasRequired( t => t.LocationTypeValue ).WithMany().HasForeignKey( t => t.LocationTypeValueId );
        }
    }

}