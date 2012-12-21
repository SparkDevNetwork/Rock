//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Group POCO Entity.
    /// </summary>
    [Table( "Group" )]
    [DataContract( IsReference = true )]
    public partial class Group : Model<Group>
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Parent Group Id.
        /// </summary>
        /// <value>
        /// Parent Group Id.
        /// </value>
        [DataMember]
        public int? ParentGroupId { get; set; }

        /// <summary>
        /// Gets or sets the Group Type Id.
        /// </summary>
        /// <value>
        /// Group Type Id.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Campus Id.
        /// </summary>
        /// <value>
        /// Campus Id.
        /// </value>
        [DataMember]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        /// <value>
        /// Name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        /// <value>
        /// Description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Is Security Role.
        /// </summary>
        /// <value>
        /// Is Security Role.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSecurityRole { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the Groups.
        /// </summary>
        /// <value>
        /// Collection of Groups.
        /// </value>
        [DataMember]
        public virtual ICollection<Group> Groups { get; set; }

        /// <summary>
        /// Gets or sets the Members.
        /// </summary>
        /// <value>
        /// Collection of Members.
        /// </value>
        [DataMember]
        public virtual ICollection<GroupMember> Members { get; set; }

        /// <summary>
        /// Gets or sets the Locations.
        /// </summary>
        /// <value>
        /// Collection of Locations.
        /// </value>
        [DataMember]
        public virtual ICollection<GroupLocation> Locations { get; set; }

        /// <summary>
        /// Gets or sets the Parent Group.
        /// </summary>
        /// <value>
        /// A <see cref="Group"/> object.
        /// </value>
        [DataMember]
        public virtual Group ParentGroup { get; set; }

        /// <summary>
        /// Gets or sets the Group Type.
        /// </summary>
        /// <value>
        /// A <see cref="GroupType"/> object.
        /// </value>
        [DataMember]
        public virtual GroupType GroupType { get; set; }

        /// <summary>
        /// Gets or sets the Campus.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.Campus"/> object.
        /// </value>
        [DataMember]
        public virtual Rock.Model.Campus Campus { get; set; }

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

        /// <summary>
        /// Determines whether [is ancestor of group] [the specified parent group id].
        /// </summary>
        /// <param name="parentGroupId">The parent group id.</param>
        /// <returns>
        ///   <c>true</c> if [is ancestor of group] [the specified parent group id]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAncestorOfGroup( int parentGroupId )
        {
            HashSet<Guid> ancestorList = new HashSet<Guid>();

            Group parentGroup = this.ParentGroup;
            while ( parentGroup != null )
            {
                if ( ancestorList.Contains( parentGroup.Guid ) )
                {
                    throw new GroupParentCircularReferenceException();
                }
                else
                {
                    ancestorList.Add( parentGroup.Guid );
                }

                if ( parentGroup.Id.Equals( parentGroupId ) )
                {
                    return true;
                }

                parentGroup = parentGroup.ParentGroup;
            }

            return false;
        }

    }

    /// <summary>
    /// Group Configuration class.
    /// </summary>
    public partial class GroupConfiguration : EntityTypeConfiguration<Group>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupConfiguration"/> class.
        /// </summary>
        public GroupConfiguration()
        {
            this.HasOptional( p => p.ParentGroup ).WithMany( p => p.Groups ).HasForeignKey( p => p.ParentGroupId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.GroupType ).WithMany( p => p.Groups ).HasForeignKey( p => p.GroupTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.Campus ).WithMany().HasForeignKey( p => p.CampusId ).WillCascadeOnDelete( false );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class GroupParentCircularReferenceException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupParentCircularReferenceException" /> class.
        /// </summary>
        public GroupParentCircularReferenceException()
            : base( "Circular Reference in Group Parents" )
        {
        }
    }
}
