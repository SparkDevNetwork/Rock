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
    /// Group Role POCO Entity.
    /// </summary>
    [Table( "crmGroupRole" )]
    public partial class GroupRole : Model<GroupRole>
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        [Required]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Group Type Id.
        /// </summary>
        /// <value>
        /// Group Type Id.
        /// </value>
        public int? GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        /// <value>
        /// Name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        /// <value>
        /// Description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        /// <value>
        /// The sort order.
        /// </value>
        public int? SortOrder { get; set; }

        /// <summary>
        /// Gets or sets the max count.
        /// </summary>
        /// <value>
        /// The max count.
        /// </value>
        public int? MaxCount { get; set; }

        /// <summary>
        /// Gets or sets the min count.
        /// </summary>
        /// <value>
        /// The min count.
        /// </value>
        public int? MinCount { get; set; }

        /// <summary>
        /// Gets the dto.
        /// </summary>
        /// <returns></returns>
        public override IDto Dto
        {
            get { return this.ToDto(); }
        }

        /// <summary>
        /// Static Method to return an object based on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static GroupRole Read( int id )
        {
            return Read<GroupRole>( id );
        }

        /// <summary>
        /// Gets or sets the Group Type.
        /// </summary>
        /// <value>
        /// A <see cref="GroupType"/> object.
        /// </value>
        public virtual GroupType GroupType { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }
    }

    /// <summary>
    /// Group Role Configuration class.
    /// </summary>
    public partial class GroupRoleConfiguration : EntityTypeConfiguration<GroupRole>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupRoleConfiguration"/> class.
        /// </summary>
        public GroupRoleConfiguration()
        {
            this.HasRequired( p => p.GroupType ).WithMany( p => p.Roles ).HasForeignKey( p => p.GroupTypeId ).WillCascadeOnDelete( true );
        }
    }
}
