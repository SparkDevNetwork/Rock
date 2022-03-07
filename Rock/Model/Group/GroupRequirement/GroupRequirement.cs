// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    ///
    /// </summary>
    [RockDomain( "Group" )]
    [Table( "GroupRequirement" )]
    [DataContract]
    public partial class GroupRequirement : Model<GroupRequirement>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Group"/> identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        [DataMember]
#if REVIEW_WEBFORMS
        [Index( "IDX_GroupRequirementTypeGroup", IsUnique = true, Order = 0 )]
#endif
        [IgnoreCanDelete]
        public int? GroupId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupType"/> identifier.
        /// </summary>
        /// <value>
        /// The group type identifier.
        /// </value>
        [DataMember]
#if REVIEW_WEBFORMS
        [Index( "IDX_GroupRequirementTypeGroup", IsUnique = true, Order = 1 )]
#endif
        [IgnoreCanDelete]
        public int? GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the group requirement type identifier.
        /// </summary>
        /// <value>
        /// The group requirement type identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
#if REVIEW_WEBFORMS
        [Index( "IDX_GroupRequirementTypeGroup", IsUnique = true, Order = 2 )]
#endif
        public int GroupRequirementTypeId { get; set; }

        /// <summary>
        /// The specific GroupRoleId that this requirement is for. NULL means this requirement applies to all roles.
        /// </summary>
        /// <value>
        /// The group role identifier.
        /// </value>
        [DataMember]
#if REVIEW_WEBFORMS
        [Index( "IDX_GroupRequirementTypeGroup", IsUnique = true, Order = 3 )]
#endif
        public int? GroupRoleId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a member must meet this requirement before adding (only applies to DataView and SQL RequirementCheckType)
        /// </summary>
        /// <value>
        /// <c>true</c> if [must meet requirement to add member]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool MustMeetRequirementToAddMember { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Group"/>.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [LavaVisible]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupType">type</see> of the group.
        /// </summary>
        /// <value>
        /// The type of the group.
        /// </value>
        [LavaVisible]
        public virtual GroupType GroupType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupRequirementType">type</see> of the group requirement.
        /// </summary>
        /// <value>
        /// The type of the group requirement.
        /// </value>
        [DataMember]
        public virtual GroupRequirementType GroupRequirementType { get; set; }

        /// <summary>
        /// The specific <see cref="Rock.Model.GroupTypeRole">Group Role</see> that this requirement is for. NULL means this requirement applies to all roles.
        /// </summary>
        /// <value>
        /// The group type role.
        /// </value>
        [LavaVisible]
        public virtual GroupTypeRole GroupRole { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    ///
    /// </summary>
    public partial class GroupRequirementConfiguration : EntityTypeConfiguration<GroupRequirement>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupRequirementConfiguration"/> class.
        /// </summary>
        public GroupRequirementConfiguration()
        {
            // NOTE: would be nice if this would cascade delete, but doing so results in a "may cause cycles or multiple cascade paths" error
            this.HasOptional( a => a.Group ).WithMany( g => g.GroupRequirements ).HasForeignKey( a => a.GroupId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.GroupType ).WithMany( gt => gt.GroupRequirements ).HasForeignKey( a => a.GroupTypeId ).WillCascadeOnDelete( false );

            this.HasRequired( a => a.GroupRequirementType ).WithMany().HasForeignKey( a => a.GroupRequirementTypeId ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.GroupRole ).WithMany().HasForeignKey( a => a.GroupRoleId ).WillCascadeOnDelete( true );
        }
    }

    #endregion
}
