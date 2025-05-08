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
using System.Runtime.Serialization;

using Rock.CheckIn.v2;
using Rock.Data;
using Rock.Enums.Communication.Chat;
using Rock.Enums.Group;
using Rock.Model;
using Rock.Utility.Enums;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Contains minimal information about a group in cache for a short
    /// period of time.
    /// </summary>
    [Serializable]
    [DataContract]
    public class GroupCache : ModelCache<GroupCache, Rock.Model.Group>
    {
        private GroupConfigurationData _checkInData;

        #region Properties

        /// <inheritdoc/>
        public override TimeSpan? Lifespan
        {
            // Currently, we only check-in related groups for any period of time.
            // This is a 3ns check assuming the group types are already cached.
            get => GroupType?.GetCheckInConfigurationType() == null ? new TimeSpan( 0, 10, 0 ) : base.Lifespan;
        }

        /// <inheritdoc cref="Rock.Model.Group.Name" />
        [DataMember]
        public string Name { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.IsActive" />
        [DataMember]
        public bool IsActive { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.GroupTypeId" />
        [DataMember]
        public int GroupTypeId { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.IsSystem" />
        [DataMember]
        public bool IsSystem { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.ParentGroupId" />
        [DataMember]
        public int? ParentGroupId { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.CampusId" />
        [DataMember]
        public int? CampusId { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.ScheduleId" />
        [DataMember]
        public int? ScheduleId { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.Description" />
        [DataMember]
        public string Description { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.IsSecurityRole" />
        [DataMember]
        public bool IsSecurityRole { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.ElevatedSecurityLevel" />
        [DataMember]
        public ElevatedSecurityLevel ElevatedSecurityLevel { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.Order" />
        [DataMember]
        public int Order { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.AllowGuests" />
        [DataMember]
        public bool? AllowGuests { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.IsPublic" />
        [DataMember]
        public bool IsPublic { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.GroupCapacity" />
        [DataMember]
        public int? GroupCapacity { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.RequiredSignatureDocumentTemplateId" />
        [DataMember]
        public int? RequiredSignatureDocumentTemplateId { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.InactiveDateTime" />
        [DataMember]
        public DateTime? InactiveDateTime { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.IsArchived" />
        [DataMember]
        public bool IsArchived { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.ArchivedDateTime" />
        [DataMember]
        public DateTime? ArchivedDateTime { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.ArchivedByPersonAliasId" />
        [DataMember]
        public int? ArchivedByPersonAliasId { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.StatusValueId" />
        [DataMember]
        public int? StatusValueId { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.SchedulingMustMeetRequirements" />
        [DataMember]
        public bool SchedulingMustMeetRequirements { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.AttendanceRecordRequiredForCheckIn" />
        [DataMember]
        public AttendanceRecordRequiredForCheckIn AttendanceRecordRequiredForCheckIn { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.ScheduleCancellationPersonAliasId" />
        [Obsolete( "Use ScheduleCoordinatorPersonAliasId instead." )]
        [RockObsolete( "1.16" )]
        [DataMember]
        public int? ScheduleCancellationPersonAliasId => this.ScheduleCoordinatorPersonAliasId;

        /// <inheritdoc cref="Rock.Model.Group.ScheduleCoordinatorPersonAliasId" />
        [DataMember]
        public int? ScheduleCoordinatorPersonAliasId { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.ScheduleCoordinatorNotificationTypes" />
        [DataMember]
        public ScheduleCoordinatorNotificationType? ScheduleCoordinatorNotificationTypes { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.GroupAdministratorPersonAliasId" />
        [DataMember]
        public virtual int? GroupAdministratorPersonAliasId { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.InactiveReasonValueId" />
        [DataMember]
        public int? InactiveReasonValueId { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.InactiveReasonNote" />
        [DataMember]
        public string InactiveReasonNote { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.RSVPReminderSystemCommunicationId" />
        [DataMember]
        public int? RSVPReminderSystemCommunicationId { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.RSVPReminderOffsetDays" />
        [DataMember]
        public int? RSVPReminderOffsetDays { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.DisableScheduleToolboxAccess" />
        [DataMember]
        public bool DisableScheduleToolboxAccess { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.DisableScheduling" />
        [DataMember]
        public bool DisableScheduling { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.GroupSalutation" />
        [DataMember]
        public string GroupSalutation { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.GroupSalutationFull" />
        [DataMember]
        public string GroupSalutationFull { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.ConfirmationAdditionalDetails" />
        [DataMember]
        public string ConfirmationAdditionalDetails { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.ReminderSystemCommunicationId" />
        [DataMember]
        public int? ReminderSystemCommunicationId { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.ReminderOffsetDays" />
        [DataMember]
        public int? ReminderOffsetDays { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.ReminderAdditionalDetails" />
        [DataMember]
        public string ReminderAdditionalDetails { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.ScheduleConfirmationLogic" />
        [DataMember]
        public ScheduleConfirmationLogic? ScheduleConfirmationLogic { get; private set; }

        /// <inheritdoc cref="Rock.Model.Group.IsSpecialNeeds" />
        [DataMember]
        public bool IsSpecialNeeds { get; private set; }

        /// <inheritdoc cref="Group.IsChatEnabledOverride"/>
        [DataMember]
        public bool? IsChatEnabledOverride { get; private set; }

        /// <inheritdoc cref="Group.IsLeavingChatChannelAllowedOverride"/>
        [DataMember]
        public bool? IsLeavingChatChannelAllowedOverride { get; private set; }

        /// <inheritdoc cref="Group.IsChatChannelPublicOverride"/>
        [DataMember]
        public bool? IsChatChannelPublicOverride { get; private set; }

        /// <inheritdoc cref="Group.IsChatChannelAlwaysShownOverride"/>
        [DataMember]
        public bool? IsChatChannelAlwaysShownOverride { get; private set; }

        /// <inheritdoc cref="Group.ChatPushNotificationModeOverride"/>
        [DataMember]
        public ChatNotificationMode? ChatPushNotificationModeOverride { get; private set; }

        /// <inheritdoc cref="Group.ChatChannelKey"/>
        [MaxLength( 100 )]
        [DataMember]
        public string ChatChannelKey { get; private set; }

        /// <summary>
        /// Gets whether chat is enabled for this group. <see cref="GroupTypeCache.IsChatAllowed"/> will be checked
        /// first; if <see langword="false"/>, chat cannot be enabled for this group. <see cref="IsChatEnabledOverride"/>
        /// will be checked next; if <see langword="null"/>, then the value from
        /// <see cref="GroupTypeCache.IsChatEnabledForAllGroups"/> will be used.
        /// </summary>
        /// <returns>Whether chat is enabled for this group.</returns>
        internal bool GetIsChatEnabled()
        {
            if ( this.GroupType?.IsChatAllowed != true )
            {
                return false;
            }

            if ( this.IsChatEnabledOverride.HasValue )
            {
                return this.IsChatEnabledOverride.Value;
            }

            return this.GroupType?.IsChatEnabledForAllGroups ?? false;
        }

        /// <summary>
        /// Gets whether individuals are allowed to leave this chat channel. Otherwise, they will only be allowed to
        /// mute the channel. If <see cref="IsLeavingChatChannelAllowedOverride"/> is set to <see langword="null"/>,
        /// then the value of <see cref="GroupTypeCache.IsLeavingChatChannelAllowed"/> will be used.
        /// </summary>
        /// <returns>Whether individuals are allowed to leave this chat channel.</returns>
        internal bool GetIsLeavingChatChannelAllowed()
        {
            if ( this.IsLeavingChatChannelAllowedOverride.HasValue )
            {
                return this.IsLeavingChatChannelAllowedOverride.Value;
            }

            return this.GroupType?.IsLeavingChatChannelAllowed ?? false;
        }

        /// <summary>
        /// Gets whether this chat channel is public and visible to everyone when performing a search. This also implies
        /// that the channel may be joined by any person via the chat application. If
        /// <see cref="IsChatChannelPublicOverride"/> is set to <see langword="null"/>, then the value of
        /// <see cref="GroupTypeCache.IsChatChannelPublic"/> will be used.
        /// </summary>
        /// <returns>Whether this chat channel is public, and may be joined by any person.</returns>
        internal bool GetIsChatChannelPublic()
        {
            if ( this.IsChatChannelPublicOverride.HasValue )
            {
                return this.IsChatChannelPublicOverride.Value;
            }

            return this.GroupType?.IsChatChannelPublic ?? false;
        }

        /// <summary>
        /// Gets whether this chat channel is always shown in the channel list even if the person has not joined
        /// the channel. This also implies that the channel may be joined by any person via the chat application. If
        /// <see cref="IsChatChannelAlwaysShownOverride"/> is set to <see langword="null"/>, then the value of
        /// <see cref="GroupTypeCache.IsChatChannelAlwaysShown"/> will be used.
        /// </summary>
        /// <returns>Whether this chat channel is always shown in the channel list, and may be joined by any person.</returns>
        internal bool GetIsChatChannelAlwaysShown()
        {
            if ( this.IsChatChannelAlwaysShownOverride.HasValue )
            {
                return this.IsChatChannelAlwaysShownOverride.Value;
            }

            return this.GroupType?.IsChatChannelAlwaysShown ?? false;
        }

        /// <summary>
        /// Gets the <see cref="ChatNotificationMode"/> to control how push notifications are sent for this chat channel.
        /// If <see cref="ChatPushNotificationModeOverride"/> set to <see langword="null"/>, then the value of
        /// <see cref="GroupTypeCache.ChatPushNotificationMode"/> will be used.
        /// </summary>
        /// <returns>The <see cref="ChatNotificationMode"/> to control how pushS notifications are sent for this chat channel.</returns>
        internal ChatNotificationMode GetChatPushNotificationMode()
        {
            if ( this.ChatPushNotificationModeOverride.HasValue )
            {
                return this.ChatPushNotificationModeOverride.Value;
            }

            return this.GroupType?.ChatPushNotificationMode ?? ChatNotificationMode.AllMessages;
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
        /// Gets or sets the default Id of the Record Source Type <see cref="Rock.Model.DefinedValue"/>, representing
        /// the source of <see cref="GroupMember"/>s added to this <see cref="Group"/>. If set to <see langword="null"/>
        /// (or if <see cref="GroupTypeCache.AllowGroupSpecificRecordSource"/> is not <see langword="true"/>), then the
        /// value of <see cref="GroupTypeCache.GroupMemberRecordSourceValueId"/> will be used. Call the
        /// <see cref="GetGroupMemberRecordSourceValueId"/> method instead to get the value, as that method will also
        /// check the <see cref="GroupTypeCache.GroupMemberRecordSourceValueId"/> property.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Record Source Type <see cref="Rock.Model.DefinedValue"/>.
        /// </value>
        [DataMember]
        public int? GroupMemberRecordSourceValueId { get; private set; }

        #endregion

        #region Navigation Properties

        /// <inheritdoc cref="Rock.Model.Group.GroupType" />
        public GroupTypeCache GroupType => GroupTypeCache.Get( GroupTypeId );

        /// <inheritdoc cref="Rock.Model.Group.ParentGroup" />
        public GroupCache ParentGroup => ParentGroupId.HasValue ? Get( ParentGroupId.Value ) : null;

        /// <inheritdoc cref="Rock.Model.Group.Campus" />
        public CampusCache Campus => CampusId.HasValue ? CampusCache.Get( CampusId.Value ) : null;

        /// <inheritdoc cref="Rock.Model.Group.Schedule" />
        public NamedScheduleCache Schedule => ScheduleId.HasValue ? NamedScheduleCache.Get( ScheduleId.Value ) : null;

        /// <inheritdoc cref="Rock.Model.Group.StatusValue" />
        public DefinedValueCache StatusValue => StatusValueId.HasValue ? DefinedValueCache.Get( StatusValueId.Value ) : null;

        /// <inheritdoc cref="Rock.Model.Group.InactiveReasonValue" />
        public DefinedValueCache InactiveReasonValue => InactiveReasonValueId.HasValue ? DefinedValueCache.Get( InactiveReasonValueId.Value ) : null;

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Not supported on GroupCache.
        /// </summary>
        /// <returns>A list of all groups in their cache form.</returns>
        public static new List<GroupCache> All()
        {
            return All( null );
        }

        /// <summary>
        /// Not supported on GroupCache.
        /// </summary>
        /// <returns>A list of all groups in their cache form.</returns>
        public static new List<GroupCache> All( RockContext rockContext )
        {
            // Since there will be a very large number of groups in the
            // database, we don't support loading all of them.
            throw new NotSupportedException( "GroupCache does not support All()" );
        }

        /// <summary>
        /// Gets the check in data that represents all the attribute
        /// values of this group.
        /// </summary>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <returns>An instance of <see cref="GroupConfigurationData"/> or <c>null</c>.</returns>
        internal GroupConfigurationData GetCheckInData( RockContext rockContext )
        {
            if ( rockContext == null )
            {
                throw new ArgumentNullException( nameof( rockContext ) );
            }

            if ( _checkInData == null )
            {
                _checkInData = new GroupConfigurationData( this, rockContext );
            }

            return _checkInData;
        }

        /// <summary>
        /// Finds all root group types of this group. This uses the group
        /// type association tree which allows multiple parents.
        /// </summary>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>An enumeration of the root group types found for this group.</returns>
        internal IEnumerable<GroupTypeCache> GetRootGroupTypes( RockContext rockContext )
        {
            var groupType = GroupTypeCache.Get( GroupTypeId, rockContext );

            if ( groupType == null )
            {
                return Array.Empty<GroupTypeCache>();
            }

            return groupType.GetRootGroupTypes( rockContext );
        }

        /// <summary>
        /// Gets the default Id of the Record Source Type <see cref="Rock.Model.DefinedValue"/>, representing the source
        /// of <see cref="GroupMember"/>s added to this <see cref="Group"/>. If set to <see langword="null"/> (or if
        /// <see cref="GroupTypeCache.AllowGroupSpecificRecordSource"/> is not <see langword="true"/>), then the value
        /// of <see cref="GroupTypeCache.GroupMemberRecordSourceValueId"/> will be used.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Record Source Type <see cref="Rock.Model.DefinedValue"/>.
        /// </value>
        internal int? GetGroupMemberRecordSourceValueId()
        {
            if ( this.GroupMemberRecordSourceValueId.HasValue && this.GroupType?.AllowGroupSpecificRecordSource == true  )
            {
                return this.GroupMemberRecordSourceValueId.Value;
            }

            return this.GroupType?.GroupMemberRecordSourceValueId;
        }

        /// <summary>
        /// Gets the default Record Source Type <see cref="Rock.Model.DefinedValue"/>, representing the source of
        /// <see cref="GroupMember"/>s added to this <see cref="Group"/>. If set to <see langword="null"/> (or if
        /// <see cref="GroupTypeCache.AllowGroupSpecificRecordSource"/> is not <see langword="true"/>), then the value
        /// of <see cref="GroupTypeCache.GroupMemberRecordSourceValue"/> will be used.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.DefinedValue"/> representing the Record Source Type.
        /// </value>
        internal DefinedValueCache GetGroupMemberRecordSourceValue()
        {
            if ( this.GroupMemberRecordSourceValueId.HasValue && this.GroupType?.AllowGroupSpecificRecordSource == true )
            {
                return DefinedValueCache.Get( this.GroupMemberRecordSourceValueId.Value );
            }

            return this.GroupType?.GroupMemberRecordSourceValue;
        }

        /// <inheritdoc/>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            if ( !( entity is Rock.Model.Group group ) )
            {
                return;
            }

            Name = group.Name;
            IsActive = group.IsActive;
            GroupTypeId = group.GroupTypeId;
            IsSystem = group.IsSystem;
            ParentGroupId = group.ParentGroupId;
            CampusId = group.CampusId;
            ScheduleId = group.ScheduleId;
            Description = group.Description;
            IsSecurityRole = group.IsSecurityRole;
            ElevatedSecurityLevel = group.ElevatedSecurityLevel;
            Order = group.Order;
            AllowGuests = group.AllowGuests;
            IsPublic = group.IsPublic;
            GroupCapacity = group.GroupCapacity;
            RequiredSignatureDocumentTemplateId = group.RequiredSignatureDocumentTemplateId;
            InactiveDateTime = group.InactiveDateTime;
            IsArchived = group.IsArchived;
            ArchivedDateTime = group.ArchivedDateTime;
            ArchivedByPersonAliasId = group.ArchivedByPersonAliasId;
            StatusValueId = group.StatusValueId;
            SchedulingMustMeetRequirements = group.SchedulingMustMeetRequirements;
            AttendanceRecordRequiredForCheckIn = group.AttendanceRecordRequiredForCheckIn;
            ScheduleCoordinatorPersonAliasId = group.ScheduleCoordinatorPersonAliasId;
            ScheduleCoordinatorNotificationTypes = group.ScheduleCoordinatorNotificationTypes;
            GroupAdministratorPersonAliasId = group.GroupAdministratorPersonAliasId;
            InactiveReasonValueId = group.InactiveReasonValueId;
            InactiveReasonNote = group.InactiveReasonNote;
            RSVPReminderSystemCommunicationId = group.RSVPReminderSystemCommunicationId;
            RSVPReminderOffsetDays = group.RSVPReminderOffsetDays;
            DisableScheduleToolboxAccess = group.DisableScheduleToolboxAccess;
            DisableScheduling = group.DisableScheduling;
            GroupSalutation = group.GroupSalutation;
            GroupSalutationFull = group.GroupSalutationFull;
            ConfirmationAdditionalDetails = group.ConfirmationAdditionalDetails;
            ReminderSystemCommunicationId = group.ReminderSystemCommunicationId;
            ReminderOffsetDays = group.ReminderOffsetDays;
            ReminderAdditionalDetails = group.ReminderAdditionalDetails;
            ScheduleConfirmationLogic = group.ScheduleConfirmationLogic;
            IsSpecialNeeds = group.IsSpecialNeeds;
            IsChatEnabledOverride = group.IsChatEnabledOverride;
            IsLeavingChatChannelAllowedOverride = group.IsLeavingChatChannelAllowedOverride;
            IsChatChannelPublicOverride = group.IsChatChannelPublicOverride;
            IsChatChannelAlwaysShownOverride = group.IsChatChannelAlwaysShownOverride;
            ChatChannelKey = group.ChatChannelKey;
            GroupMemberRecordSourceValueId = group.GroupMemberRecordSourceValueId;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        #endregion Public Methods
    }
}
