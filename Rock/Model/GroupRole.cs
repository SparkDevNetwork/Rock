//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Group Role POCO Entity.
    /// </summary>
    [Table( "GroupRole" )]
    public partial class GroupRole : Model<GroupRole>
    {

        #region Entity Properties

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
        /// Is this role a leader of the group
        /// </summary>
        /// <value>
        /// The is leader.
        /// </value>
        public bool IsLeader { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the Group Type.
        /// </summary>
        /// <value>
        /// A <see cref="GroupType"/> object.
        /// </value>
        public virtual GroupType GroupType { get; set; }

        /// <summary>
        /// Gets the dto.
        /// </summary>
        /// <returns></returns>
        public override IDto Dto
        {
            get { return this.ToDto(); }
        }

        #endregion

        #region Public Methods

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

        #endregion

        #region Private Methods

        #endregion

        #region Static Methods

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
        /// Static method to return an object based on the GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public static GroupRole Read( Guid guid )
        {
            return Read<GroupRole>( guid );
        }

        #endregion

    }

    #region Entity Configuration

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

    #endregion

}
