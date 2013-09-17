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
    /// Represents the a type of <see cref="Rock.Model.Location"/> that is supported by a <see cref="Rock.Model.GroupType"/>.
    /// </summary>
    [Table( "GroupTypeLocationType" )]
    [DataContract]
    public class GroupTypeLocationType
    {
        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.GroupType"/>. This property is required, and is part of the key.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.GroupType"/>.
        /// </value>
        [Key]
        [Column(Order=0)]
        [DataMember]
        public int GroupTypeId { get; set; }


        /// <summary>
        /// Gets or sets the Id of the LocationType <see cref="Rock.Model.DefinedValue"/> that represents a type of <see cref="Rock.Model.Location"/> that is
        /// supported by a <see cref="Rock.Model.GroupType"/>. This property is required and is part of the key.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of a LocationType <see cref="Rock.Model.DefinedValue">  that is supported by a <see cref="Rock.Model.GroupType"/>.
        /// </value>
        [Key]
        [Column( Order = 1 )]
        [DataMember]
        public int LocationTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupType"/>.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.GroupType"/>.
        /// </value>
        [DataMember]
        public virtual GroupType GroupType { get; set; }

        /// <summary>
        /// Gets or sets the a <see cref="Rock.Model.DefinedType"/> that is supported by the <see cref="Rock.Model.GroupType"/>.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.GroupType" /> that is supported by the <see cref="Rock.Model.GroupType"/>.
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