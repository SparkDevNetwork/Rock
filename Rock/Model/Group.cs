﻿// <copyright>
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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents A collection of <see cref="Rock.Model.Person"/> entities. This can be a family, small group, Bible study, security group,  etc. Groups can be hierarchical.
    /// </summary>
    /// <remarks>
    /// In Rock any collection or defined subset of people are considered a group.
    /// </remarks>
    [Table( "Group" )]
    [DataContract]
    public partial class Group : Model<Group>, IOrdered
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this Group is a part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this Group is part of the Rock core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Group's Parent Group.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the Group's Parent Group.
        /// </value>
        [DataMember]
        public int? ParentGroupId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.GroupType"/> that this Group is a member belongs to. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.GroupType"/> that this group is a member of.
        /// </value>
        [Required]
        [HideFromReporting]
        [DataMember( IsRequired = true )]
        public int GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Campus"/> that this Group is associated with.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Campus"/> that the Group is associated with. If the group is not 
        /// associated with a campus, this value is null.
        /// </value>
        [HideFromReporting]
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.CAMPUS )]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the schedule identifier.
        /// </summary>
        /// <value>
        /// The schedule identifier.
        /// </value>
        [HideFromReporting]
        [DataMember]
        public int? ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the Name of the Group. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the Group. 
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        [Previewable]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the optional description of the group.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the group.
        /// </value>
        [DataMember]
        [Previewable]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this Group is a Security Role. This value is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this Group is a security role, otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Previewable]
        public bool IsSecurityRole { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this is an active group. This value is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this group is active, otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Previewable]
        public bool IsActive		
        {		
            get { return _isActive; }		
            set { _isActive = value; }		
        }		
        private bool _isActive = true;

        /// <summary>
        /// Gets or sets the display order of the group in the group list and group hierarchy. The lower the number the higher the 
        /// display priority this group has. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the display order of the group.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets whether group allows members to specify additional "guests" that will be part of the group (i.e. attend event)
        /// </summary>
        /// <value>
        /// The allow guests flag
        /// </value>
        [DataMember]
        public bool? AllowGuests { get; set; }

        /// <summary>
        /// Gets or sets the welcome system email template.
        /// </summary>
        /// <value>
        /// The welcome system email.
        /// </value>
        [HideFromReporting]
        [DataMember]
        public int? WelcomeSystemEmailId { get; set; }

        /// <summary>
        /// Gets or sets the exit system email template.
        /// </summary>
        /// <value>
        /// The exit system email.
        /// </value>
        [HideFromReporting]
        [DataMember]
        public int? ExitSystemEmailId { get; set; }

        /// <summary>
        /// Gets or sets the data view to sync with.
        /// </summary>
        /// <value>
        /// The sync data view.
        /// </value>
        [HideFromReporting]
        [DataMember]
        public int? SyncDataViewId { get; set; }

        /// <summary>
        /// Gets or sets whether a user account should be generated when a person is added through the sync.
        /// </summary>
        /// <value>
        /// The add user accounts through sync.
        /// </value>
        [HideFromReporting]
        [DataMember]
        public bool? AddUserAccountsDuringSync { get; set; }

        /// <summary>
        /// Gets or sets whether a group member can only be added if all the GroupRequirements have been met
        /// </summary>
        /// <value>
        /// The must meet requirements to add member.
        /// </value>
        [DataMember]
        public bool? MustMeetRequirementsToAddMember { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group should be shown in group finders
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is public; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsPublic
        {
            get { return _isPublic; }
            set { _isPublic = value; }
        }
        private bool _isPublic = true;

        /// <summary>
        /// Gets or sets the group capacity.
        /// </summary>
        /// <value>
        /// The group capacity.
        /// </value>
        [DataMember]
        public int? GroupCapacity { get; set; }

        /// <summary>
        /// Gets or sets the required signature document type identifier.
        /// </summary>
        /// <value>
        /// The required signature document type identifier.
        /// </value>
        [DataMember]
        public int? RequiredSignatureDocumentTemplateId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets this parent Group of this Group.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.Group"/> representing the Group's parent group. If this Group does not have a parent, the value will be null.
        /// </value>
        public virtual Group ParentGroup { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupType"/> that this Group is a member of.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.GroupType"/> that this Group is a member of.
        /// </value>
        [DataMember]
        public virtual GroupType GroupType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Campus"/> that this Group is associated with.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Campus"/> that this Group is associated with.
        /// </value>
        [DataMember]
        public virtual Rock.Model.Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets the schedule.
        /// </summary>
        /// <value>
        /// The schedule.
        /// </value>
        [DataMember]
        public virtual Rock.Model.Schedule Schedule { get; set; }

        /// <summary>
        /// Gets or sets the welcome system email.
        /// </summary>
        /// <value>
        /// The welcome system email.
        /// </value>
        [DataMember]
        public virtual Rock.Model.SystemEmail WelcomeSystemEmail { get; set; }

        /// <summary>
        /// Gets or sets the exit system email.
        /// </summary>
        /// <value>
        /// The exit system email.
        /// </value>
        [DataMember]
        public virtual Rock.Model.SystemEmail ExitSystemEmail { get; set; }

        /// <summary>
        /// Gets or sets the data view to sync with.
        /// </summary>
        /// <value>
        /// The sync data view.
        /// </value>
        [DataMember]
        public virtual Rock.Model.DataView SyncDataView { get; set; }

        /// <summary>
        /// Gets or sets the type of the required signature document.
        /// </summary>
        /// <value>
        /// The type of the required signature document.
        /// </value>
        [DataMember]
        public virtual SignatureDocumentTemplate RequiredSignatureDocumentTemplate { get; set; }

        /// <summary>
        /// Gets or sets a collection the Groups that are children of this group.
        /// </summary>
        /// <value>
        /// A collection of Groups that are children of this group.
        /// </value>
        [LavaInclude]
        public virtual ICollection<Group> Groups
        {
            get { return _groups ?? ( _groups = new Collection<Group>() ); }
            set { _groups = value; }
        }
        private ICollection<Group> _groups;

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.GroupMember">GroupMembers</see> who are associated with the Group.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.GroupMember">GroupMembers</see> who are associated with the Group.
        /// </value>
        [DataMember]
        public virtual ICollection<GroupMember> Members
        {
            get { return _members ?? ( _members = new Collection<GroupMember>() ); }
            set { _members = value; }
        }
        private ICollection<GroupMember> _members;

        /// <summary>
        /// Gets or Sets the <see cref="Rock.Model.GroupLocation">GroupLocations</see> that are associated with the Group.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.GroupLocation">GroupLocations</see> that are associated with the Group.
        /// </value>
        [DataMember]
        public virtual ICollection<GroupLocation> GroupLocations
        {
            get { return _groupLocations ?? ( _groupLocations = new Collection<GroupLocation>() ); }
            set { _groupLocations = value; }
        }
        private ICollection<GroupLocation> _groupLocations;

        /// <summary>
        /// Gets or sets the group requirements.
        /// </summary>
        /// <value>
        /// The group requirements.
        /// </value>
        [DataMember]
        public virtual ICollection<GroupRequirement> GroupRequirements
        {
            get { return _groupsRequirements ?? ( _groupsRequirements = new Collection<GroupRequirement>() ); }
            set { _groupsRequirements = value; }
        }
        private ICollection<GroupRequirement> _groupsRequirements;

        /// <summary>
        /// Gets or sets the group member workflow triggers.
        /// </summary>
        /// <value>
        /// The group member workflow triggers.
        /// </value>
        public virtual ICollection<GroupMemberWorkflowTrigger> GroupMemberWorkflowTriggers
        {
            get { return _triggers ?? ( _triggers = new Collection<GroupMemberWorkflowTrigger>() ); }
            set { _triggers = value; }
        }
        private ICollection<GroupMemberWorkflowTrigger> _triggers;

        /// <summary>
        /// Gets or sets the linkages.
        /// </summary>
        /// <value>
        /// The linkages.
        /// </value>
        public virtual ICollection<EventItemOccurrenceGroupMap> Linkages
        {
            get { return _linkages ?? ( _linkages = new Collection<EventItemOccurrenceGroupMap>() ); }
            set { _linkages = value; }
        }
        private ICollection<EventItemOccurrenceGroupMap> _linkages;
        
        /// <summary>
        /// Gets the securable object that security permissions should be inherited from.  If block is located on a page
        /// security will be inherited from the page, otherwise it will be inherited from the site.
        /// </summary>
        /// <value>
        /// The parent authority. If the block is located on the page, security will be
        /// inherited from the page, otherwise it will be inherited from the site.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.ParentGroup != null ? this.ParentGroup : base.ParentAuthority;
            }
        }

        /// <summary>
        /// Gets the parent authority2.
        /// </summary>
        /// <value>
        /// The parent authority2.
        /// </value>
        public override Security.ISecured ParentAuthorityPre
        {
            get
            {
                return this.GroupType;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets all the group member workflow triggers from the group and the group type sorted by order
        /// </summary>
        /// <param name="includeGroupTypeTriggers">if set to <c>true</c> [include group type triggers].</param>
        /// <returns></returns>
        public IOrderedEnumerable<GroupMemberWorkflowTrigger> GetGroupMemberWorkflowTriggers( bool includeGroupTypeTriggers = true )
        {
            return this.GroupMemberWorkflowTriggers.Union( this.GroupType.GroupMemberWorkflowTriggers ).OrderBy( a => a.Order ).ThenBy( a => a.Name );
        }

        /// <summary>
        /// Determines whether the specified action is authorized.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>
        ///   <c>true</c> if the specified action is authorized; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsAuthorized( string action, Person person )
        {
            // Check to see if user is authorized using normal authorization rules
            bool authorized = base.IsAuthorized( action, person );

            // If the user is not authorized for group through normal security roles, and this is a logged
            // in user trying to view or edit, check to see if they should be allowed based on their role
            // in the group.
            if ( !authorized && person != null && ( action == Authorization.VIEW || action == Authorization.EDIT ) )
            {
                // Get the cached group type
                var groupType = GroupTypeCache.Read( this.GroupTypeId );
                if ( groupType != null )
                {
                    // For each occurrence of this person in this group, check to see if their role is valid
                    // for the group type and if the role grants them authorization
                    using ( var rockContext = new RockContext() )
                    {
                        foreach ( int roleId in new GroupMemberService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( m =>
                                m.PersonId == person.Id &&
                                m.GroupId == this.Id &&
                                m.GroupMemberStatus == GroupMemberStatus.Active )
                            .Select( m => m.GroupRoleId ) )
                        {
                            var role = groupType.Roles.FirstOrDefault( r => r.Id == roleId );
                            if ( role != null )
                            {
                                if ( action == Authorization.VIEW && role.CanView )
                                {
                                    return true;
                                }

                                if ( action == Authorization.EDIT && role.CanEdit )
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return authorized;
        }

        /// <summary>
        /// Returns a list of the Group Requirements for this Group along with the status ordered by GroupRequirement Name
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="groupRoleId">The group role identifier.</param>
        /// <returns></returns>
        public IEnumerable<PersonGroupRequirementStatus> PersonMeetsGroupRequirements( int personId, int? groupRoleId )
        {
            var result = new List<PersonGroupRequirementStatus>();
            foreach ( var groupRequirement in this.GroupRequirements.OrderBy( a => a.GroupRequirementType.Name ) )
            {
                var requirementStatus = groupRequirement.PersonMeetsGroupRequirement( personId, groupRoleId );
                result.Add( requirementStatus );
            }

            return result;
        }

        /// <summary>
        /// Pres the save changes.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="state">The state.</param>
        public override void PreSaveChanges( Rock.Data.DbContext dbContext, System.Data.Entity.EntityState state )
        {
            if ( state == System.Data.Entity.EntityState.Deleted )
            {
                // manually delete any grouprequirements of this group since it can't be cascade deleted
                var groupRequirementService = new GroupRequirementService( dbContext as RockContext );
                var groupRequirements = groupRequirementService.Queryable().Where( a => a.GroupId == this.Id ).ToList();
                if ( groupRequirements.Any() )
                {
                    groupRequirementService.DeleteRange( groupRequirements );
                }

                // manually set any attendance search group ids to null
                var attendanceService = new AttendanceService( dbContext as RockContext );
                foreach ( var attendance in attendanceService.Queryable()
                    .Where( a => 
                        a.SearchResultGroupId.HasValue &&
                        a.SearchResultGroupId.Value == this.Id ) )
                {
                    attendance.SearchResultGroupId = null;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public override bool IsValid
        {
            get
            {
                var result = base.IsValid;
                if ( result )
                {
                    string errorMessage;
                    using ( var rockContext = new RockContext() )
                    {
                        // validate that a campus is not required
                        var groupType = this.GroupType ?? new GroupTypeService( rockContext ).Queryable().Where( g => g.Id == this.GroupTypeId ).FirstOrDefault();

                        if (groupType != null )
                        {
                            if (groupType.GroupsRequireCampus && this.CampusId == null )
                            {
                                errorMessage = string.Format( "{0} require a campus.", groupType.Name.Pluralize() );
                                ValidationResults.Add( new ValidationResult( errorMessage ));
                                result = false;
                            }
                        }
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> containing the Name of the Group that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> containing the Name of the Group that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion
    }

    #region Entity Configuration

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
            this.HasOptional( p => p.Schedule ).WithMany().HasForeignKey( p => p.ScheduleId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.WelcomeSystemEmail ).WithMany().HasForeignKey( p => p.WelcomeSystemEmailId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.ExitSystemEmail ).WithMany().HasForeignKey( p => p.ExitSystemEmailId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.SyncDataView ).WithMany().HasForeignKey( p => p.SyncDataViewId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.RequiredSignatureDocumentTemplate ).WithMany().HasForeignKey( p => p.RequiredSignatureDocumentTemplateId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

    #region Custom Exceptions

    /// <summary>
    /// Represents a circular reference exception. This occurs when a group is set as a parent of a group that is higher in the group hierarchy. 
    /// </summary>
    /// <remarks>
    ///  An example of this is when a child group is set as the parent of its parent group.
    /// </remarks>
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

    #endregion
}
