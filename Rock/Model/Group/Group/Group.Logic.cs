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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;

using Rock.Attribute;
using Rock.Communication.Chat;
using Rock.Data;
using Rock.Enums.Group;
using Rock.Security;
using Rock.SystemGuid;
using Rock.UniversalSearch;
using Rock.UniversalSearch.IndexModels;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class Group
    {
        #region Properties

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
        /// An optional additional parent authority.  (i.e for Groups, the GroupType is main parent
        /// authority, but parent group is an additional parent authority )
        /// </summary>
        public override Security.ISecured ParentAuthorityPre
        {
            get
            {
                if ( this.GroupTypeId > 0 )
                {
                    GroupTypeCache groupType = GroupTypeCache.Get( this.GroupTypeId );
                    return groupType;
                }
                else
                {
                    return base.ParentAuthorityPre;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether [allows interactive bulk indexing].
        /// </summary>
        /// <value>
        /// <c>true</c> if [allows interactive bulk indexing]; otherwise, <c>false</c>.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        [NotMapped]
        public bool AllowsInteractiveBulkIndexing => true;

        /// <summary>
        /// Gets or sets the history change list.
        /// </summary>
        /// <value>
        /// The history change list.
        /// </value>
        [NotMapped]
        [RockObsolete( "1.14" )]
        [Obsolete( "Does nothing. No longer needed. We replaced this with a private property under the SaveHook class for this entity.", true )]
        public virtual History.HistoryChangeList HistoryChangeList { get; set; }

        /// <summary>
        /// Gets whether this group is overriding its parent group type's peer network configuration in any way.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "17.0" )]
        public bool IsOverridingGroupTypePeerNetworkConfiguration
        {
            get
            {
                if ( this.GroupType?.IsPeerNetworkEnabled != true )
                {
                    return false;
                }

                if ( this.RelationshipGrowthEnabledOverride.HasValue
                    && this.RelationshipGrowthEnabledOverride.Value != this.GroupType.RelationshipGrowthEnabled )
                {
                    return true;
                }

                if ( this.RelationshipStrengthOverride.HasValue
                    && this.RelationshipStrengthOverride.Value != this.GroupType.RelationshipStrength )
                {
                    return true;
                }

                if ( this.LeaderToLeaderRelationshipMultiplierOverride.HasValue
                    && this.LeaderToLeaderRelationshipMultiplierOverride.Value != this.GroupType.LeaderToLeaderRelationshipMultiplier )
                {
                    return true;
                }

                if ( this.LeaderToNonLeaderRelationshipMultiplierOverride.HasValue
                    && this.LeaderToNonLeaderRelationshipMultiplierOverride.Value != this.GroupType.LeaderToNonLeaderRelationshipMultiplier )
                {
                    return true;
                }

                if ( this.NonLeaderToLeaderRelationshipMultiplierOverride.HasValue
                    && this.NonLeaderToLeaderRelationshipMultiplierOverride.Value != this.GroupType.NonLeaderToLeaderRelationshipMultiplier )
                {
                    return true;
                }

                if ( this.NonLeaderToNonLeaderRelationshipMultiplierOverride.HasValue
                    && this.NonLeaderToNonLeaderRelationshipMultiplierOverride.Value != this.GroupType.NonLeaderToNonLeaderRelationshipMultiplier )
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets whether any relationship multipliers have been customized for this group or its parent group type.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "17.0" )]
        public bool AreAnyRelationshipMultipliersCustomized
        {
            get
            {
                // First, if all of this group's direct values are 100%, treat this as not customized.
                if ( LeaderToLeaderRelationshipMultiplierOverride.GetValueOrDefault() == 1m
                    && LeaderToNonLeaderRelationshipMultiplierOverride.GetValueOrDefault() == 1m
                    && NonLeaderToLeaderRelationshipMultiplierOverride.GetValueOrDefault() == 1m
                    && NonLeaderToNonLeaderRelationshipMultiplierOverride.GetValueOrDefault() == 1m )
                {
                    return false;
                }

                // Next, check if any of the parent group type's values are customized (not 100%).
                if ( GroupType?.AreAnyRelationshipMultipliersCustomized == true )
                {
                    return true;
                }

                // Last, check if any of this group's direct values are defined.
                return LeaderToLeaderRelationshipMultiplierOverride != null
                    || LeaderToNonLeaderRelationshipMultiplierOverride != null
                    || NonLeaderToLeaderRelationshipMultiplierOverride != null
                    || NonLeaderToNonLeaderRelationshipMultiplierOverride != null;
            }
        }

        #endregion Properties

        #region Indexing Methods

        /// <summary>
        /// Bulks the index documents.
        /// </summary>
        public void BulkIndexDocuments()
        {
            List<IndexModelBase> indexableItems = new List<IndexModelBase>();

            var rockContext = new RockContext();

            // return people
            var groups = new GroupService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( g => g.IsActive == true && g.GroupType.IsIndexEnabled == true );

            int recordCounter = 0;

            foreach ( var group in groups )
            {
                var indexableGroup = GroupIndex.LoadByModel( group );
                indexableItems.Add( indexableGroup );

                recordCounter++;

                if ( recordCounter > 100 )
                {
                    IndexContainer.IndexDocuments( indexableItems );
                    indexableItems = new List<IndexModelBase>();
                    recordCounter = 0;
                }
            }

            IndexContainer.IndexDocuments( indexableItems );
        }

        /// <summary>
        /// Indexes the document.
        /// </summary>
        /// <param name="id"></param>
        public void IndexDocument( int id )
        {
            var groupEntity = new GroupService( new RockContext() ).Get( id );
            if ( groupEntity == null )
            {
                return;
            }

            // check that this group type is set to be indexed.
            if ( groupEntity.GroupType.IsIndexEnabled && groupEntity.IsActive )
            {
                var indexItem = GroupIndex.LoadByModel( groupEntity );
                IndexContainer.IndexDocument( indexItem );
            }
        }

        /// <summary>
        /// Deletes the indexed document.
        /// </summary>
        /// <param name="id"></param>
        public void DeleteIndexedDocument( int id )
        {
            Type indexType = Type.GetType( "Rock.UniversalSearch.IndexModels.GroupIndex" );
            IndexContainer.DeleteDocumentById( indexType, id );
        }

        /// <summary>
        /// Deletes the indexed documents.
        /// </summary>
        public void DeleteIndexedDocuments()
        {
            IndexContainer.DeleteDocumentsByType<GroupIndex>();
        }

        /// <summary>
        /// Indexes the name of the model.
        /// </summary>
        /// <returns></returns>
        public Type IndexModelType()
        {
            return typeof( GroupIndex );
        }

        /// <summary>
        /// Gets the index filter values.
        /// </summary>
        /// <returns></returns>
        public ModelFieldFilterConfig GetIndexFilterConfig()
        {
            ModelFieldFilterConfig filterConfig = new ModelFieldFilterConfig();
            filterConfig.FilterValues = new GroupTypeService( new RockContext() ).Queryable().AsNoTracking().Where( t => t.IsIndexEnabled ).Select( t => t.Name ).ToList();
            filterConfig.FilterLabel = "Group Types";
            filterConfig.FilterField = "groupTypeName";

            return filterConfig;
        }

        /// <summary>
        /// Method that will be called on an entity immediately after the item is saved by context
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The entry.</param>
        /// <param name="state">The state.</param>
        public override void PreSaveChanges( Data.DbContext dbContext, DbEntityEntry entry, EntityState state )
        {
            if ( state == EntityState.Modified || state == EntityState.Deleted )
            {
                _originalGroupTypeId = entry.OriginalValues[nameof( this.GroupTypeId )]?.ToString().AsIntegerOrNull();
                _originalIsSecurityRole = entry.OriginalValues[nameof( this.IsSecurityRole )]?.ToString().AsBooleanOrNull();
            }

            base.PreSaveChanges( dbContext, entry, state );
        }

        /// <summary>
        /// Gets the index filter field.
        /// </summary>
        /// <returns></returns>
        public bool SupportsIndexFieldFiltering()
        {
            return true;
        }

        #endregion Indexing Methods

        #region ICacheable

        private int? _originalGroupTypeId;
        private bool? _originalIsSecurityRole;

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            // doesn't apply
            return null;
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            if ( ChatHelper.IsChatEnabled && entityState == EntityState.Deleted )
            {
                ChatHelper.TryRemoveCachedChatChannel( this.Id );
            }

            GroupCache.UpdateCachedEntity( Id, entityState );

            // If the group changed, and it was a security group, flush the security for the group
            Guid? originalGroupTypeGuid = null;
            Guid groupTypeSecurityRole = Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid();
            if ( _originalGroupTypeId.HasValue && _originalGroupTypeId != this.GroupTypeId )
            {
                originalGroupTypeGuid = GroupTypeCache.Get( _originalGroupTypeId.Value, ( RockContext ) dbContext )?.Guid;
            }

            var groupTypeGuid = GroupTypeCache.Get( this.GroupTypeId, ( RockContext ) dbContext )?.Guid;
            if ( this.IsSecurityRole || ( _originalIsSecurityRole == true ) || ( groupTypeGuid == groupTypeSecurityRole ) || ( originalGroupTypeGuid == groupTypeSecurityRole ) )
            {
                RoleCache.FlushItem( this.Id );
            }
        }

        #endregion ICacheable

        #region Methods

        /// <summary>
        /// Gets any group type role limit warnings based on GroupTypeRole restrictions
        /// </summary>
        /// <param name="warningMessageHtml">The warning message HTML.</param>
        /// <returns></returns>
        public bool GetGroupTypeRoleLimitWarnings( out string warningMessageHtml )
        {
            var roleLimitWarnings = new StringBuilder();
            var group = this;
            var groupType = GroupTypeCache.Get( group.GroupTypeId );
            if ( groupType?.Roles != null && groupType.Roles.Any() )
            {
                var groupMemberService = new GroupMemberService( new RockContext() );
                foreach ( var role in groupType.Roles.Where( a => a.MinCount.HasValue || a.MaxCount.HasValue ) )
                {
                    int curCount = groupMemberService.Queryable().Where( m => m.GroupId == group.Id && m.GroupRoleId == role.Id && m.GroupMemberStatus == GroupMemberStatus.Active ).Count();

                    if ( role.MinCount.HasValue && role.MinCount.Value > curCount )
                    {
                        string format = "The <strong>{1}</strong> role is currently below its minimum requirement of {2:N0} active {3}.<br/>";
                        roleLimitWarnings.AppendFormat( format, role.Name.Pluralize().ToLower(), role.Name.ToLower(), role.MinCount, role.MinCount == 1 ? groupType.GroupMemberTerm.ToLower() : groupType.GroupMemberTerm.Pluralize().ToLower() );
                    }

                    if ( role.MaxCount.HasValue && role.MaxCount.Value < curCount )
                    {
                        string format = "The <strong>{1}</strong> role is currently above its maximum limit of {2:N0} active {3}.<br/>";
                        roleLimitWarnings.AppendFormat( format, role.Name.Pluralize().ToLower(), role.Name.ToLower(), role.MaxCount, role.MaxCount == 1 ? groupType.GroupMemberTerm.ToLower() : groupType.GroupMemberTerm.Pluralize().ToLower() );
                    }
                }
            }

            warningMessageHtml = roleLimitWarnings.ToString();
            return !string.IsNullOrEmpty( warningMessageHtml );
        }

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

            if ( authorized || person == null )
            {
                return authorized;
            }

            var groupType = GroupTypeCache.Get( this.GroupTypeId );

            if ( groupType == null )
            {
                return authorized;
            }

            // if the person isn't authorized through normal security roles, check if the person has a group role that authorizes them
            // First, check if there are any roles that could authorized them. If not, we can avoid a database lookup.
            List<int> checkMemberRoleIds = new List<int>();
            if ( action == Authorization.VIEW )
            {
                checkMemberRoleIds.AddRange( groupType.Roles.Where( a => a.CanView || a.CanTakeAttendance ).Select( a => a.Id ) );
            }
            else if ( action == Authorization.MANAGE_MEMBERS )
            {
                checkMemberRoleIds.AddRange( groupType.Roles.Where( a => a.CanEdit || a.CanManageMembers ).Select( a => a.Id ) );
            }
            else if ( action == Authorization.EDIT )
            {
                checkMemberRoleIds.AddRange( groupType.Roles.Where( a => a.CanEdit ).Select( a => a.Id ) );
            }
            else if ( action == Authorization.TAKE_ATTENDANCE )
            {
                checkMemberRoleIds.AddRange( groupType.Roles.Where( a => a.CanEdit || a.CanTakeAttendance ).Select( a => a.Id ) );
            }

            if ( !checkMemberRoleIds.Any() )
            {
                return authorized;
            }

            // For each occurrence of this person in this group for the roles that might grant them auth,
            // check to see if their role is valid for the group type and if the role grants them authorization
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
                        if ( action == Authorization.VIEW && ( role.CanView || role.CanTakeAttendance ) )
                        {
                            return true;
                        }

                        if ( action == Authorization.MANAGE_MEMBERS && ( role.CanEdit || role.CanManageMembers ) )
                        {
                            return true;
                        }

                        if ( action == Authorization.EDIT && role.CanEdit )
                        {
                            return true;
                        }

                        if ( action == Authorization.TAKE_ATTENDANCE && ( role.CanEdit || role.CanTakeAttendance ) )
                        {
                            return true;
                        }
                    }
                }
            }

            return authorized;
        }

        /// <summary>
        /// Determines whether is a Security role based on either <see cref="Group.IsSecurityRole" />
        /// or if <see cref="Group.GroupTypeId"/> is the Security Role Group Type.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is security role or security group type]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsSecurityRoleOrSecurityGroupType()
        {
            var groupTypeSecurityRole = GroupTypeCache.GetSecurityRoleGroupType();
            return this.IsSecurityRole || this.GroupTypeId == groupTypeSecurityRole?.Id;
        }

        /// <summary>
        /// Gets the group requirements.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public IQueryable<GroupRequirement> GetGroupRequirements( RockContext rockContext )
        {
            return new GroupRequirementService( rockContext ).Queryable().Include( a => a.GroupRequirementType ).Where( a => ( a.GroupId.HasValue && a.GroupId == this.Id ) || ( a.GroupTypeId.HasValue && a.GroupTypeId == this.GroupTypeId ) );
        }

        /// <summary>
        /// Persons the meets group requirements.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="groupRoleId">The group role identifier.</param>
        /// <returns></returns>
        public IEnumerable<PersonGroupRequirementStatus> PersonMeetsGroupRequirements( RockContext rockContext, int personId, int? groupRoleId )
        {
            var result = new List<PersonGroupRequirementStatus>();
            foreach ( var groupRequirement in this.GetGroupRequirements( rockContext ).OrderBy( a => a.GroupRequirementType.Name ) )
            {
                var requirementStatus = groupRequirement.PersonMeetsGroupRequirement( rockContext, personId, this.Id, groupRoleId );
                result.Add( requirementStatus );
            }

            return result;
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
                        var groupType = this.GroupType ?? new GroupTypeService( rockContext ).Queryable().Where( gt => gt.Id == this.GroupTypeId ).FirstOrDefault();

                        if ( groupType != null )
                        {
                            if ( groupType.GroupsRequireCampus && this.CampusId == null )
                            {
                                errorMessage = string.Format( "{0} require a campus.", groupType.Name.Pluralize() );
                                ValidationResults.Add( new ValidationResult( errorMessage ) );
                                result = false;
                            }
                        }
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Get a list of all inherited Attributes that should be applied to this entity.
        /// </summary>
        /// <returns>A list of all inherited AttributeCache objects.</returns>
        public override List<AttributeCache> GetInheritedAttributes( Rock.Data.RockContext rockContext )
        {
            var groupTypeCache = GroupTypeCache.Get( GroupTypeId );

            return groupTypeCache?.GetInheritedAttributesForQualifier( TypeId, "GroupTypeId" );
        }

        /// <summary>
        /// Gets the ṣchedule confirmation Logic.
        /// </summary>
        /// <returns></returns>
        public ScheduleConfirmationLogic GetScheduleConfirmationLogic()
        {
            var scheduleConfirmationLogic = Enums.Group.ScheduleConfirmationLogic.Ask;
            if ( this.ScheduleConfirmationLogic.HasValue )
            {
                scheduleConfirmationLogic = this.ScheduleConfirmationLogic.Value;
            }
            else if ( this.GroupType != null )
            {
                scheduleConfirmationLogic = this.GroupType.ScheduleConfirmationLogic;
            }
            else
            {
                var groupType = GroupTypeCache.Get( this.GroupTypeId );
                if ( groupType != null )
                {
                    scheduleConfirmationLogic = groupType.ScheduleConfirmationLogic;
                }
            }

            return scheduleConfirmationLogic;
        }

        /// <summary>
        /// Gets whether a notification of the provided type should be sent to a group schedule coordinator.
        /// If any notification types are defined at the group level, they will take precedence over any that are
        /// defined at the group type level.
        /// </summary>
        /// <param name="notificationType">The notification type in question.</param>
        /// <returns>Whether a notification of the provided type should be sent to a group schedule coordinator.</returns>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "1.16.7" )]
        public bool ShouldSendScheduleCoordinatorNotificationType( ScheduleCoordinatorNotificationType notificationType )
        {
            // Just in case the caller of this method passed in "None".
            if ( notificationType == ScheduleCoordinatorNotificationType.None )
            {
                return false;
            }

            // Prefer notification types defined on the group.
            if ( this.ScheduleCoordinatorNotificationTypes.HasValue )
            {
                return this.ScheduleCoordinatorNotificationTypes.Value.HasFlag( notificationType );
            }

            // Fall back to notification types defined on the group type.
            return this.GroupType?.ScheduleCoordinatorNotificationTypes?.HasFlag( notificationType ) == true;
        }

        /// <summary>
        /// Gets whether chat is enabled for this group. <see cref="GroupType.IsChatAllowed"/> will be checked first; if
        /// <see langword="false"/>, chat cannot be enabled for this group. <see cref="IsChatEnabledOverride"/> will be
        /// checked next; if <see langword="null"/>, then the value from <see cref="GroupType.IsChatEnabledForAllGroups"/>
        /// will be used.
        /// </summary>
        /// <returns>Whether chat is enabled for this group.</returns>
        [RockInternal( "17.1", true )]
        public bool GetIsChatEnabled()
        {
            var groupTypeIsChatAllowed = false;
            var groupTypeIsChatEnabledForAllGroups = false;

            var groupTypeCache = GroupTypeCache.Get( this.GroupTypeId );
            if ( groupTypeCache != null )
            {
                groupTypeIsChatAllowed = groupTypeCache.IsChatAllowed;
                groupTypeIsChatEnabledForAllGroups = groupTypeCache.IsChatEnabledForAllGroups;
            }

            if ( !groupTypeIsChatAllowed )
            {
                return false;
            }

            if ( this.IsChatEnabledOverride.HasValue )
            {
                return this.IsChatEnabledOverride.Value;
            }

            return groupTypeIsChatEnabledForAllGroups;
        }

        /// <summary>
        /// Gets whether individuals are allowed to leave this chat channel. Otherwise, they will only be allowed to
        /// mute the channel. If <see cref="IsLeavingChatChannelAllowedOverride"/> is set to <see langword="null"/>,
        /// then the value of <see cref="GroupType.IsLeavingChatChannelAllowed"/> will be used.
        /// </summary>
        /// <returns>Whether individuals are allowed to leave this chat channel.</returns>
        [RockInternal( "17.1", true )]
        public bool GetIsLeavingChatChannelAllowed()
        {
            if ( this.IsLeavingChatChannelAllowedOverride.HasValue )
            {
                return this.IsLeavingChatChannelAllowedOverride.Value;
            }

            var groupTypeCache = GroupTypeCache.Get( this.GroupTypeId );
            if ( groupTypeCache != null )
            {
                return groupTypeCache.IsLeavingChatChannelAllowed;
            }

            return false;
        }

        /// <summary>
        /// Gets whether this chat channel is public and visible to everyone when performing a search. This also implies
        /// that the channel may be joined by any person via the chat application. If
        /// <see cref="IsChatChannelPublicOverride"/> is set to <see langword="null"/>, then the value of
        /// <see cref="GroupType.IsChatChannelPublic"/> will be used.
        /// </summary>
        /// <returns>Whether this chat channel is public, and may be joined by any person.</returns>
        [RockInternal( "17.1", true )]
        public bool GetIsChatChannelPublic()
        {
            if ( this.IsChatChannelPublicOverride.HasValue )
            {
                return this.IsChatChannelPublicOverride.Value;
            }

            var groupTypeCache = GroupTypeCache.Get( this.GroupTypeId );
            if ( groupTypeCache != null )
            {
                return groupTypeCache.IsChatChannelPublic;
            }

            return false;
        }

        /// <summary>
        /// Gets whether this chat channel is always shown in the channel list even if the person has not joined
        /// the channel. This also implies that the channel may be joined by any person via the chat application. If
        /// <see cref="IsChatChannelAlwaysShownOverride"/> is set to <see langword="null"/>, then the value of
        /// <see cref="GroupType.IsChatChannelAlwaysShown"/> will be used.
        /// </summary>
        /// <returns>Whether this chat channel is always shown in the channel list, and may be joined by any person.</returns>
        [RockInternal( "17.1", true )]
        public bool GetIsChatChannelAlwaysShown()
        {
            if ( this.IsChatChannelAlwaysShownOverride.HasValue )
            {
                return this.IsChatChannelAlwaysShownOverride.Value;
            }

            var groupTypeCache = GroupTypeCache.Get( this.GroupTypeId );
            if ( groupTypeCache != null )
            {
                return groupTypeCache.IsChatChannelAlwaysShown;
            }

            return false;
        }

        /// <summary>
        /// Gets whether this chat channel is active.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if <see cref="GetIsChatEnabled"/>, <see cref="IsActive"/> and NOT <see cref="IsArchived"/>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        internal bool GetIsChatChannelActive()
        {
            return this.GetIsChatEnabled() && this.IsActive && !this.IsArchived;
        }

        /// <summary>
        /// Gets the default Id of the Record Source Type <see cref="Rock.Model.DefinedValue"/>, representing the source
        /// of <see cref="GroupMember"/>s added to this <see cref="Group"/>. If set to <see langword="null"/> (or if
        /// <see cref="GroupType.AllowGroupSpecificRecordSource"/> is not <see langword="true"/>), then the value of
        /// <see cref="GroupType.GroupMemberRecordSourceValueId"/> will be used.
        /// </summary>
        /// <returns>
        /// A <see cref="System.Int32"/> representing the Id of the Record Source Type <see cref="Rock.Model.DefinedValue"/>.
        /// </returns>
        internal int? GetGroupMemberRecordSourceValueId()
        {
            var groupTypeCache = GroupTypeCache.Get( this.GroupTypeId );

            if ( this.GroupMemberRecordSourceValueId.HasValue && groupTypeCache?.AllowGroupSpecificRecordSource == true )
            {
                return this.GroupMemberRecordSourceValueId.Value;
            }

            return groupTypeCache?.GroupMemberRecordSourceValueId;
        }

        /// <summary>
        /// Gets the default Record Source Type <see cref="Rock.Model.DefinedValue"/>, representing the source of
        /// <see cref="GroupMember"/>s added to this <see cref="Group"/>. If set to <see langword="null"/> (or if
        /// <see cref="GroupType.AllowGroupSpecificRecordSource"/> is not <see langword="true"/>), then the value of
        /// <see cref="GroupType.GroupMemberRecordSourceValue"/> will be used.
        /// </summary>
        /// <returns>
        /// A <see cref="Rock.Model.DefinedValue"/> representing the Record Source Type.
        /// </returns>
        internal DefinedValue GetGroupMemberRecordSourceValue()
        {
            if ( this.GroupMemberRecordSourceValue != null && this.GroupType?.AllowGroupSpecificRecordSource == true )
            {
                return this.GroupMemberRecordSourceValue;
            }

            return this.GroupType?.GroupMemberRecordSourceValue;
        }

        #endregion
    }
}
