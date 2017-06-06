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
using System.Configuration;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using Humanizer;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a member of a group in Rock. A group member is a <see cref="Rock.Model.Person"/> who has a relationship with a <see cref="Rock.Model.Group"/>.
    /// </summary>
    [Table( "GroupMember" )]
    [DataContract]
    public partial class GroupMember : Model<GroupMember>
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
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Person"/> that is represented by the GroupMember. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Person"/> who is reprensented by the GroupMember.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the GroupMember's <see cref="GroupRole"/> in the <see cref="Rock.Model.Group"/>. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.GroupTypeRole"/> that the Group Member is in.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
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
        /// Gets or sets the GroupMember's status in the Group. This value is required.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.GroupMemberStatus"/> enum value that represents the GroupMember's status in the group.  A <c>GroupMemberStatus.Active</c> indicates that the GroupMember is active,
        /// A <c>GroupMemberStatus.Inactive</c> value indicates that the GroupMember is not active, otherwise their GroupMemberStatus will be <c>GroupMemberStatus.Pending</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public GroupMemberStatus GroupMemberStatus
        {
            get { return _groupMemberStatus; }
            set { _groupMemberStatus = value; }
        }
        private GroupMemberStatus _groupMemberStatus = GroupMemberStatus.Active;

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

        #endregion

        #region Virtual Properties

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
        [LavaInclude]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the the GroupMember's role in the <see cref="Rock.Model.Group"/>.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.GroupTypeRole"/> representing the GroupMember's <see cref="Rock.Model.GroupTypeRole"/> in the <see cref="Rock.Model.Group"/>.
        /// </value>
        [DataMember]
        public virtual GroupTypeRole GroupRole { get; set; }

        /// <summary>
        /// Gets or sets the group member requirements.
        /// </summary>
        /// <value>
        /// The group member requirements.
        /// </value>
        [DataMember]
        public virtual ICollection<GroupMemberRequirement> GroupMemberRequirements
        {
            get { return _groupMemberRequirements ?? ( _groupMemberRequirements = new Collection<GroupMemberRequirement>() ); }
            set { _groupMemberRequirements = value; }
        }

        private ICollection<GroupMemberRequirement> _groupMemberRequirements;

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Person.ToStringSafe();
        }

        /// <summary>
        /// Pres the save changes.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The entry.</param>
        public override void PreSaveChanges( DbContext dbContext, System.Data.Entity.Infrastructure.DbEntityEntry entry )
        {
            var transaction = new Rock.Transactions.GroupMemberChangeTransaction( entry );
            Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );

            base.PreSaveChanges( dbContext, entry );
        }

        /// <summary>
        /// Pres the save changes.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="state">The state.</param>
        public override void PreSaveChanges( DbContext dbContext, System.Data.Entity.EntityState state )
        {
            try
            {
                string verb = string.Empty;
                string action = string.Empty;

                var rockContext = (RockContext)dbContext;
                Group group = null;

                if ( state == System.Data.Entity.EntityState.Added )
                {
                    verb = "ADD";
                    action = "Added to group.";
                    if ( !this.DateTimeAdded.HasValue )
                    {
                        this.DateTimeAdded = RockDateTime.Now;
                    }
                }
                else if ( state == System.Data.Entity.EntityState.Modified )
                {
                    verb = "UPDATE";

                    var changeEntry = dbContext.ChangeTracker.Entries<GroupMember>().Where( a => a.Entity == this ).FirstOrDefault();
                    if ( changeEntry != null )
                    {
                        var origGroupMemberStatus = (GroupMemberStatus)changeEntry.OriginalValues["GroupMemberStatus"];
                        if ( origGroupMemberStatus != this.GroupMemberStatus )
                        {
                            action = string.Format( "Group member status changed from {0} to {1}", origGroupMemberStatus.ToString(), this.GroupMemberStatus.ToString() );
                        }

                        var origGroupRoleId = (int)changeEntry.OriginalValues["GroupRoleId"];
                        if ( origGroupRoleId != this.GroupRoleId )
                        {
                            if ( group == null )
                            {
                                group = this.Group;
                            }
                            if ( group == null )
                            {
                                group = new GroupService( rockContext ).Get( this.GroupId );
                            }
                            if ( group != null )
                            {
                                var groupType = GroupTypeCache.Read( group.GroupTypeId );
                                if ( groupType != null )
                                {
                                    var origRole = groupType.Roles.FirstOrDefault( r => r.Id == origGroupRoleId );
                                    var newRole = groupType.Roles.FirstOrDefault( r => r.Id == this.GroupRoleId );
                                    action = string.Format( "Group role changed from {0} to {1}",
                                        ( origRole != null ? origRole.Name : "??" ),
                                        ( newRole != null ? newRole.Name : "??" ) );
                                }
                            }
                        }
                    }
                }
                else if ( state == System.Data.Entity.EntityState.Deleted )
                {
                    verb = "DELETE";
                    action = "Removed from group.";
                }

                if ( !string.IsNullOrWhiteSpace( action ) )
                {
                    if ( group == null )
                    {
                        group = this.Group;
                    }
                    if ( group == null )
                    {
                        group = new GroupService( rockContext ).Get( this.GroupId );
                    }
                    if ( group != null )
                    {
                        var personEntityTypeId = EntityTypeCache.Read( "Rock.Model.Person" ).Id;
                        var groupEntityTypeId = EntityTypeCache.Read( "Rock.Model.Group" ).Id;
                        var groupMembershipCategoryId = CategoryCache.Read( Rock.SystemGuid.Category.HISTORY_PERSON_GROUP_MEMBERSHIP.AsGuid(), rockContext ).Id;

                        new HistoryService( rockContext ).Add( new History
                        {
                            EntityTypeId = personEntityTypeId,
                            CategoryId = groupMembershipCategoryId,
                            EntityId = this.PersonId,
                            Summary = action,
                            Caption = group.Name,
                            RelatedEntityTypeId = groupEntityTypeId,
                            RelatedEntityId = this.GroupId,
                            Verb = verb
                        } );
                    }
                }
            }
            catch { }

            base.PreSaveChanges( dbContext, state );
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// NOTE: Try using IsValidGroupMember instead
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public override bool IsValid
        {
            get
            {
                using ( var rockContext = new RockContext() )
                {
                    return this.IsValidGroupMember( rockContext );
                }
            }
        }

        /// <summary>
        /// Calls IsValid with the specified context (to avoid deadlocks)
        /// Try to call this instead of IsValid when possible
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        ///   <c>true</c> if [is valid group member] [the specified rock context]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValidGroupMember( RockContext rockContext )
        {
            var result = base.IsValid;
            if ( result )
            {
                string errorMessage;
                
                if ( !ValidateGroupMembership( rockContext, out errorMessage ) )
                {
                    ValidationResults.Add( new ValidationResult( errorMessage ) );
                    result = false;
                }
                
            }

            return result;
        }

        /// <summary>
        /// Validates the group membership.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private bool ValidateGroupMembership( RockContext rockContext, out string errorMessage )
        {
            errorMessage = string.Empty;

            // load group including members to save queries in group member validation
            var group = this.Group ?? new GroupService( rockContext ).Queryable( "Members" ).Where( g => g.Id == this.GroupId ).FirstOrDefault();

            var groupType = GroupTypeCache.Read( group.GroupTypeId );
            var groupRole = groupType.Roles.First( a => a.Id == this.GroupRoleId );

            var existingGroupMembership = group.Members.Where(m => m.PersonId == this.PersonId);

            // check to see if the person is already a member of the group/role
            bool allowDuplicateGroupMembers = ConfigurationManager.AppSettings["AllowDuplicateGroupMembers"].AsBoolean();

            if ( !allowDuplicateGroupMembers )
            {
                if ( existingGroupMembership.Any( a => a.GroupRoleId == this.GroupRoleId && a.Id != this.Id ) )
                {
                    var person = this.Person ?? new PersonService( rockContext ).Get( this.PersonId );

                    errorMessage = string.Format(
                        "{0} already belongs to the {1} role for this {2}, and cannot be added again with the same role",
                        person,
                        groupRole.Name.ToLower(),
                        groupType.GroupTerm.ToLower() );

                    return false;
                }
            }

            if ( groupRole.MaxCount.HasValue && this.GroupMemberStatus == GroupMemberStatus.Active )
            {
                int memberCountInRole = group.Members
                                            .Where( m =>
                                                m.GroupRoleId == this.GroupRoleId
                                                && m.GroupMemberStatus == GroupMemberStatus.Active )
                                            .Where( m => m != this)
                                            .Count();

                bool roleMembershipAboveMax = false;

                // if adding new group member..
                if ( this.Id.Equals( 0 ) )
                {
                    // verify that active count has not exceeded the max
                    if ( ( memberCountInRole + 1 ) > groupRole.MaxCount )
                    {
                        roleMembershipAboveMax = true;
                    }
                }
                else
                {
                    // if existing group member changing role or status..
                    if ( this.IsStatusOrRoleModified( rockContext ) )
                    {
                        // verify that active count has not exceeded the max
                        if ( groupRole.MaxCount != null && ( memberCountInRole + 1 ) > groupRole.MaxCount )
                        {
                            roleMembershipAboveMax = true;
                        }
                    }
                }

                // throw error if above max.. do not proceed
                if ( roleMembershipAboveMax )
                {
                    errorMessage = string.Format(
                "The number of {0} for this {1} is above its maximum allowed limit of {2:N0} active {3}.",
                groupRole.Name.Pluralize().ToLower(),
                groupType.GroupTerm.ToLower(),
                groupRole.MaxCount,
                groupType.GroupMemberTerm.Pluralize( groupRole.MaxCount == 1 ) ).ToLower();
                    return false;
                }
            }
            
            // if the GroupMember is getting Added (or if Person or Role is different), and if this Group has requirements that must be met before the person is added, check those
            if ( this.IsNewOrChangedGroupMember( rockContext ) )
            {
                if ( group.MustMeetRequirementsToAddMember ?? false )
                {
                    var requirementStatuses = group.PersonMeetsGroupRequirements( this.PersonId, this.GroupRoleId );

                    if ( requirementStatuses.Any( a => a.MeetsGroupRequirement == MeetsGroupRequirement.NotMet ) )
                    {
                        // deny if any of the non-manual requirements are not met
                        errorMessage = "This person does not meet the following requirements for this group: "
                            + requirementStatuses.Where( a => a.MeetsGroupRequirement == MeetsGroupRequirement.NotMet )
                            .Select( a => string.Format( "{0}", a.GroupRequirement.GroupRequirementType ) )
                            .ToList().AsDelimited( ", " );

                        return false;
                    }
                }
            }

            

            // check group capacity
            if ( groupType.GroupCapacityRule == GroupCapacityRule.Hard && group.GroupCapacity.HasValue )
            {
                var currentActiveGroupMemberCount = group.Members.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active ).Count();

                // check if this would be adding an active group member (new active group member or changing existing group member status to active)
                if ( 
                    (this.Id.Equals( 0 ) && this.GroupMemberStatus == GroupMemberStatus.Active) 
                    || (!this.Id.Equals(0) 
                            && existingGroupMembership.Where(m => m.Id == this.Id && m.GroupMemberStatus != GroupMemberStatus.Active).Any() 
                            && this.GroupMemberStatus == GroupMemberStatus.Active)
                   )
                {
                    if ( currentActiveGroupMemberCount + 1 > group.GroupCapacity )
                    {
                        errorMessage = "Adding this individual would put the group over capacity.";
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether this is an existing record but the GroupMemberStatus or GroupRoleId was modified since loaded from the database
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public bool IsStatusOrRoleModified( RockContext rockContext )
        {
            if ( this.Id == 0 )
            {
                // new group member
                return false;
            }
            else
            {
                var groupMemberService = new GroupMemberService( rockContext );
                var databaseGroupMemberRecord = groupMemberService.Get( this.Id );

                if ( databaseGroupMemberRecord == null )
                {
                    return false;
                }

                // existing groupmember record, but person or role was changed
                var hasChanged = ( ( this.GroupMemberStatus != databaseGroupMemberRecord.GroupMemberStatus ) || ( this.GroupRoleId != databaseGroupMemberRecord.GroupRoleId ) );

                if ( !hasChanged )
                {
                    var entry = rockContext.Entry( this );
                    if ( entry != null )
                    {
                        hasChanged = rockContext.Entry( this ).Property( "GroupMemberStatus" )?.IsModified == true || rockContext.Entry( this ).Property( "GroupRoleId" )?.IsModified == true;
                    }
                }

                return hasChanged;
            }
        }

        /// <summary>
        /// Determines whether this is a new group member (just added) or if either Person or Role is different than what is stored in the database
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public bool IsNewOrChangedGroupMember( RockContext rockContext )
        {
            if ( this.Id == 0 )
            {
                // new group member
                return true;
            }
            else
            {
                var groupMemberService = new GroupMemberService( rockContext );
                var databaseGroupMemberRecord = groupMemberService.Get( this.Id );

                // existing groupmember record, but person or role was changed
                var hasChanged = ( ( this.PersonId != databaseGroupMemberRecord.PersonId ) || ( this.GroupRoleId != databaseGroupMemberRecord.GroupRoleId ) );

                if ( !hasChanged )
                {
                    var entry = rockContext.Entry( this );
                    if ( entry != null )
                    {
                        hasChanged = rockContext.Entry( this ).Property( "PersonId" )?.IsModified == true || rockContext.Entry( this ).Property( "GroupRoleId" )?.IsModified == true;
                    }
                }

                return hasChanged;
            }
        }

        /// <summary>
        /// Returns the current values of the group requirements statuses for this GroupMember from the last time they were calculated ordered by GroupRequirementType.Name
        /// </summary>
        /// <returns></returns>
        public IEnumerable<GroupRequirementStatus> GetGroupRequirementsStatuses()
        {
            var metRequirements = this.GroupMemberRequirements.Select( a => new
            {
                GroupRequirementId = a.GroupRequirement.Id,
                MeetsGroupRequirement = a.RequirementMetDateTime.HasValue
                    ? a.RequirementWarningDateTime.HasValue ? MeetsGroupRequirement.MeetsWithWarning : MeetsGroupRequirement.Meets
                    : MeetsGroupRequirement.NotMet,
                a.RequirementWarningDateTime,
                a.LastRequirementCheckDateTime,
            } );

            // get all the group requirements that apply the group member's role
            var allGroupRequirements = this.Group.GroupRequirements.Where( a => !a.GroupRoleId.HasValue || a.GroupRoleId == this.GroupRoleId ).OrderBy( a => a.GroupRequirementType.Name );

            // outer join on group requirements
            var result = from groupRequirement in allGroupRequirements
                         join metRequirement in metRequirements on groupRequirement.Id equals metRequirement.GroupRequirementId into j
                         from metRequirement in j.DefaultIfEmpty()
                         select new GroupRequirementStatus
                         {
                             GroupRequirement = groupRequirement,
                             MeetsGroupRequirement = metRequirement != null ? metRequirement.MeetsGroupRequirement : MeetsGroupRequirement.NotMet,
                             RequirementWarningDateTime = metRequirement != null ? metRequirement.RequirementWarningDateTime : null,
                             LastRequirementCheckDateTime = metRequirement != null ? metRequirement.LastRequirementCheckDateTime : null
                         };

            return result;
        }

        /// <summary>
        /// If Group has GroupRequirements, will calculate and update the GroupMemberRequirements for the GroupMember, then save the changes to the database
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="saveChanges">if set to <c>true</c> [save changes].</param>
        public void CalculateRequirements( RockContext rockContext, bool saveChanges = true )
        {
            // recalculate and store in the database if the groupmember isn't new or changed
            var groupMemberRequirementsService = new GroupMemberRequirementService( rockContext );
            var group = this.Group ?? new GroupService( rockContext ).Queryable( "GroupRequirements" ).FirstOrDefault( a => a.Id == this.GroupId );
            if ( !group.GroupRequirements.Any() )
            {
                // group doesn't have requirements so no need to calculate
                return;
            }

            var updatedRequirements = group.PersonMeetsGroupRequirements( this.PersonId, this.GroupRoleId );

            foreach ( var updatedRequirement in updatedRequirements )
            {
                updatedRequirement.GroupRequirement.UpdateGroupMemberRequirementResult( rockContext, updatedRequirement.PersonId, updatedRequirement.MeetsGroupRequirement );
            }

            if ( saveChanges )
            {
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool IsEqualTo( GroupMember other )
        {
            return
                this.GroupId == other.GroupId &&
                this.PersonId == other.PersonId &&
                this.GroupRoleId == other.GroupRoleId;
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
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// Represents the status of a <see cref="Rock.Model.GroupMember"/> in a <see cref="Rock.Model.Group"/>.
    /// </summary>
    public enum GroupMemberStatus
    {
        /// <summary>
        /// The <see cref="Rock.Model.GroupMember"/> is not an active member of the <see cref="Rock.Model.Group"/>.
        /// </summary>
        Inactive = 0,

        /// <summary>
        /// The <see cref="Rock.Model.GroupMember"/> is an active member of the <see cref="Rock.Model.Group"/>.
        /// </summary>
        Active = 1,

        /// <summary>
        /// The <see cref="Rock.Model.GroupMember">GroupMember's</see> membership in the <see cref="Rock.Model.Group"/> is pending.
        /// </summary>
        Pending = 2
    }

    #endregion

    #region Exceptions

    /// <summary>
    /// Exception to throw if GroupMember validation rules are invalid (and can't be checked using .IsValid)
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class GroupMemberValidationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMemberValidationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public GroupMemberValidationException( string message ) : base( message )
        {
        }
    }

    #endregion
}
