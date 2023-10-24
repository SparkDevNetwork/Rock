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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Lava;
using Rock.Utility;
using Rock.Web.Cache;

using Z.EntityFramework.Plus;

namespace Rock.Model
{
    /// <summary>
    /// Represents a member of a group in Rock. A group member is a <see cref="Rock.Model.Person"/> who has a relationship with a <see cref="Rock.Model.Group"/>.
    /// </summary>
    [RockDomain( "Group" )]
    [Table( "GroupMember" )]
    [DataContract]
    [CodeGenerateRest]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.GROUP_MEMBER )]
    public partial class GroupMember : Model<GroupMember>, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this GroupMember is part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this GroupMember is a part of the Rock core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Group"/> that this GroupMember is associated with. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Group"/> that the GroupMember is associated with.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [EnableAttributeQualification]
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.GroupType"/> that this Group member belongs to. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.GroupType"/> that this group member part of.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [EnableAttributeQualification]
        public int GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Person"/> that is represented by the GroupMember. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Person"/> who is represented by the GroupMember.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the GroupMember's <see cref="Rock.Model.GroupMember.GroupRole"/> in the <see cref="Rock.Model.Group"/>. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.GroupTypeRole"/> that the Group Member is in.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [EnableAttributeQualification]
        public int GroupRoleId { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the note.
        /// </value>
        [DataMember]
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the GroupMember's status (<see cref="Rock.Model.GroupMemberStatus"/>) in the Group. This value is required.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.GroupMemberStatus"/> enum value that represents the GroupMember's status in the group.  A <c>GroupMemberStatus.Active</c> indicates that the GroupMember is active,
        /// A <c>GroupMemberStatus.Inactive</c> value indicates that the GroupMember is not active, otherwise their GroupMemberStatus will be <c>GroupMemberStatus.Pending</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public GroupMemberStatus GroupMemberStatus { get; set; } = GroupMemberStatus.Active;

        /// <summary>
        /// Gets or sets the number of additional guests that member will be bring to group.  Only applies when group has the 'AllowGuests' flag set to true.
        /// </summary>
        /// <value>
        /// The guest count.
        /// </value>
        [DataMember]
        public int? GuestCount { get; set; }

        /// <summary>
        /// Gets or sets the date/time that the person was added to the group.
        /// Rock will automatically set this value when a group member is added if it isn't set manually
        /// </summary>
        /// <value>
        /// The date added.
        /// </value>
        [DataMember]
        public DateTime? DateTimeAdded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is notified.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is notified; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsNotified { get; set; }

        /// <summary>
        /// Gets or sets the order of Groups of the Group's GroupType for the Person.
        /// For example, if this is a FamilyGroupType, GroupOrder can be used to specify which family should be 
        /// listed as 1st (primary), 2nd, 3rd, etc for the Person.
        /// If GroupOrder is null, the group will be listed in no particular order after the ones that do have a GroupOrder.
        /// NOTE: Use int.MaxValue in OrderBy statements for null GroupOrder values
        /// </summary>
        /// <value>
        /// The group order.
        /// </value>
        [DataMember]
        public int? GroupOrder { get; set; }

        /// <summary>
        /// Gets or sets the date that this group member became inactive
        /// </summary>
        /// <value>
        /// The in active date time.
        /// </value>
        [DataMember]
        public DateTime? InactiveDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this group member is archived (soft deleted)
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is archived; otherwise, <c>false</c>.
        /// </value>
        [HideFromReporting]
        [DataMember]
        public bool IsArchived { get; set; } = false;

        /// <summary>
        /// Gets or sets the date time that this group member was archived (soft deleted)
        /// </summary>
        /// <value>
        /// The archived date time.
        /// </value>
        [HideFromReporting]
        [DataMember]
        public DateTime? ArchivedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias">PersonAliasId</see> that archived (soft deleted) this group member
        /// </summary>
        /// <value>
        /// The archived by person alias identifier.
        /// </value>
        [HideFromReporting]
        [DataMember]
        public int? ArchivedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.GroupMemberScheduleTemplate"/>
        /// </summary>
        /// <value>
        /// The schedule template identifier.
        /// </value>
        [DataMember]
        public int? ScheduleTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the schedule start date to base the schedule off of. See <see cref="Rock.Model.GroupMemberScheduleTemplate"/>.
        /// </summary>
        /// <value>
        /// The schedule start date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? ScheduleStartDate { get; set; }

        /// <summary>
        /// Gets or sets the number of days prior to the schedule to send a reminder email. See also <seealso cref="GroupType.ScheduleReminderEmailOffsetDays"/>.
        /// </summary>
        /// <value>
        /// The schedule reminder email offset days.
        /// </value>
        [DataMember]
        public int? ScheduleReminderEmailOffsetDays { get; set; }

        /// <summary>
        /// Gets or sets the communication preference.
        /// </summary>
        /// <value>
        /// The communication preference.
        /// </value>
        [DataMember]
        public CommunicationType CommunicationPreference { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMember"/> class.
        /// </summary>
        public GroupMember()
            : base()
        {
            CommunicationPreference = CommunicationType.RecipientPreference;
        }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Person"/> representing the GroupMember.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.Person"/> representing the person who is the GroupMember.
        /// </value>
        [DataMember]
        public virtual Model.Person Person { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Group"/> that the GroupMember belongs to.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.Group"/> representing the Group that the GroupMember is a part of.
        /// </value>
        [LavaVisible]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the GroupMember's role (<see cref="Rock.Model.GroupTypeRole"/>) in the <see cref="Rock.Model.Group"/>.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.GroupTypeRole"/> representing the GroupMember's <see cref="Rock.Model.GroupTypeRole"/> in the <see cref="Rock.Model.Group"/>.
        /// </value>
        [DataMember]
        public virtual GroupTypeRole GroupRole { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/> that archived (soft deleted) this group member
        /// </summary>
        /// <value>
        /// The archived by person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias ArchivedByPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupMemberRequirement">group member requirements</see>.
        /// </summary>
        /// <value>
        /// The group member requirements.
        /// </value>
        [DataMember]
        public virtual ICollection<GroupMemberRequirement> GroupMemberRequirements { get; set; } = new Collection<GroupMemberRequirement>();

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupMemberScheduleTemplate"/>. 
        /// </summary>
        /// <value>
        /// The schedule template.
        /// </value>
        [DataMember]
        public virtual GroupMemberScheduleTemplate ScheduleTemplate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupMemberAssignment">group member assignments</see>.
        /// </summary>
        /// <value>
        /// The group member assignments.
        /// </value>
        [DataMember]
        public virtual ICollection<GroupMemberAssignment> GroupMemberAssignments { get; set; } = new Collection<GroupMemberAssignment>();

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
            return Person?.ToString();
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Member Configuration class.
    /// </summary>
    public partial class GroupMemberConfiguration : EntityTypeConfiguration<GroupMember>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMemberConfiguration"/> class.
        /// </summary>
        public GroupMemberConfiguration()
        {
            this.HasRequired( p => p.Person ).WithMany( p => p.Members ).HasForeignKey( p => p.PersonId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.Group ).WithMany( p => p.Members ).HasForeignKey( p => p.GroupId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.GroupRole ).WithMany().HasForeignKey( p => p.GroupRoleId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.ArchivedByPersonAlias ).WithMany().HasForeignKey( p => p.ArchivedByPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.ScheduleTemplate ).WithMany().HasForeignKey( p => p.ScheduleTemplateId ).WillCascadeOnDelete( false );

            // Tell EF that we never want archived group members. 
            // This will prevent archived members from being included in any GroupMember queries.
            // It will also prevent navigation properties of GroupMember from including archived group members.
            Z.EntityFramework.Plus.QueryFilterManager.Filter<GroupMember>( x => x.Where( m => m.IsArchived == false ) );

            // In the case of GroupMember as a property (not a collection), we DO want to fetch the groupMember record even if it is archived, so ensure that AllowPropertyFilter = false;
            // NOTE: This is not specific to GroupMember, it is for any Filtered Model (currently just Group and GroupMember)
            Z.EntityFramework.Plus.QueryFilterManager.AllowPropertyFilter = false;
        }
    }

    #endregion
}
