//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a role that a <see cref="Rock.Model.GroupMember"/> can have in a <see cref="Rock.Model.Group"/>.  In RockChMS each member of a group has one 
    /// or more <see cref="Rock.Model.GroupRole">GroupRoles</see> assigned to them (for instance the leader of a group can have both a leader and member role). Examples
    /// of roles include leader, member, team leader, coach, host, etc.
    /// </summary>
    [Table( "GroupRole" )]
    [DataContract]
    public partial class GroupRole : Model<GroupRole>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this GroupRole is part of the RockChMS core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this GroupRole is part of the RockChMS core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.GroupType"/> that this GroupRole belongs to. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.GroupType"/> that this GroupRole belongs to.
        /// </value>
        [Required]
        [DataMember (IsRequired = true) ]
        public int? GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Name of the GroupRole. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> representing the Name of the GroupRole.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the user defined description of the GroupRole. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the user defined description of the GroupRole.
        /// </value>
        [DataMember( IsRequired = true )]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the sort order position of the GroupRole.  The lower the SortOrder the higher the GroupRole shows in lists/controls.  
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the sort order of the GroupRole.
        /// </value>
        [DataMember( IsRequired = true )]
        public int? SortOrder { get; set; }


        /// <summary>
        /// Gets or sets the maximum count of <see cref="Rock.Model.GroupMember">GroupMembers</see> that a <see cref="Rock.Model.Group"/> can have who 
        /// belong to this GroupRole.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the maximum count of <see cref="Rock.Model.GroupMember">GroupMembers</see> that a <see cref="Rock.Model.Group"/> can have
        /// who belong to this GroupRole. If there isn't a maximum, this value is null.
        /// </value>
        [DataMember]
        public int? MaxCount { get; set; }

        /// <summary>
        /// Gets or sets the minimum count of <see cref="Rock.Model.GroupMember">GroupMembers</see> that a <see cref="Rock.Model.Group"/> can have who belong to this GroupRole.
        /// </summary>
        /// <value>
        /// The min count of <see cref="Rock.Model.GroupMember">GroupMebers</see> that a <see cref="Rock.Model.Group"/> can have who belong to this GroupRole. If there is no minimum
        /// this value is null.
        /// </value>
        [DataMember]
        public int? MinCount { get; set; }


        /// <summary>
        /// Gets or sets a flag indicating if this is a group leader role.  
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this is a GroupLeader role; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsLeader { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupType"/> that this GroupRole belongs to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.GroupType"/> that this GroupRole belongs to.
        /// </value>
        public virtual GroupType GroupType { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> containing Name of the GroupRole that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> containing the Name of the GroupRole that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
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
