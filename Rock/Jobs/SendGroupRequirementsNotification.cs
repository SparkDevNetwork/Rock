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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Lava;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// </summary>
    [DisplayName( "Send Group Requirements Notification" )]
    [Description( "This job sends out reminders to group leaders when group members do not meet all requirements." )]

    #region Attributes

    [SystemCommunicationField( "Notification Email Template",
        Description = "The system communication to use for the Group Requirements Notification.",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.NotificationEmailTemplate )]
    [GroupTypesField( "Group Types",
        Key = AttributeKey.GroupTypes,
        Description = "Group types use to check the group requirements on.",
        Order = 1 )]
    [EnumField( "Notify Parent Group's Notification-Roles",
        Key = AttributeKey.NotifyParentGroups,
        Description = "Depending on the setting, members of the parent groups whose role has 'Receive Requirements Notifications' enabled will also be notified.",
        EnumSourceType = typeof( NotificationOption ),
        IsRequired = true,
        DefaultEnumValue = ( int ) NotificationOption.None,
        Order = 2 )]
    [GroupField( "Accountability Group",
        Key = AttributeKey.AccountabilityGroup,
        Description = "Optional group that will receive a list of all group members that do not meet requirements.",
        IsRequired = false,
        Order = 3 )]

    #endregion

    public class SendGroupRequirementsNotification : RockJob
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string NotificationEmailTemplate = "NotificationEmailTemplate";
            public const string GroupTypes = "GroupTypes";
            public const string NotifyParentGroups = "NotifyParentLeaders";
            public const string AccountabilityGroup = "AccountabilityGroup";
        }

        #endregion Attribute Keys

        #region Fields

        List<NotificationItem> _notificationList = new List<NotificationItem>();
        List<GroupsMissingRequirements> _groupsMissingRequirements = new List<GroupsMissingRequirements>();

        #endregion

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public SendGroupRequirementsNotification()
        {
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            Guid? systemEmailGuid = GetAttributeValue( AttributeKey.NotificationEmailTemplate ).AsGuidOrNull();
            if ( !systemEmailGuid.HasValue )
            {
                this.Result = "Warning: No NotificationEmailTemplate found";
                return;
            }

            var errors = new List<string>();
            var rockContext = new RockContext();
            var selectedGroupTypes = new List<Guid>();

            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.GroupTypes ) ) )
            {
                selectedGroupTypes = GetAttributeValue( AttributeKey.GroupTypes ).Split( ',' ).Select( Guid.Parse ).ToList();
            }

            var notificationOption = GetAttributeValue( AttributeKey.NotifyParentGroups ).ConvertToEnum<NotificationOption>( NotificationOption.None );
            var accountAbilityGroupGuid = GetAttributeValue( AttributeKey.AccountabilityGroup ).AsGuid();

            var groupRequirementsQry = new GroupRequirementService( rockContext ).Queryable();

            // get groups matching of the types provided
            GroupService groupService = new GroupService( rockContext );
            var groups = groupService
                .Queryable()
                .AsNoTracking()
                .Where( g => selectedGroupTypes.Contains( g.GroupType.Guid )
                    && g.IsActive == true
                    && groupRequirementsQry.Any( a => ( a.GroupId.HasValue && a.GroupId == g.Id ) || ( a.GroupTypeId.HasValue && a.GroupTypeId == g.GroupTypeId ) ) );

            foreach ( var group in groups )
            {
                // check for members that don't meet requirements
                var groupMembersWithIssues = groupService.GroupMembersNotMeetingRequirements( group, true );

                if ( groupMembersWithIssues.Count > 0 )
                {
                    // add issues to issue list
                    GroupsMissingRequirements groupMissingRequirements = new GroupsMissingRequirements();
                    groupMissingRequirements.Id = group.Id;
                    groupMissingRequirements.Name = group.Name;
                    if ( group.GroupType != null )
                    {
                        groupMissingRequirements.GroupTypeId = group.GroupTypeId;
                        groupMissingRequirements.GroupTypeName = group.GroupType.Name;
                    }
                    groupMissingRequirements.AncestorPathName = groupService.GroupAncestorPathName( group.Id );

                    // Get list of the active, unarchived group leaders and those who should receive Group Requirement Notifications
                    groupMissingRequirements.Leaders = group.Members
                        .Where( m => ( m.GroupRole.ReceiveRequirementsNotifications || m.GroupRole.IsLeader )
                        && m.GroupMemberStatus == GroupMemberStatus.Active
                        && m.IsArchived == false )
                        .Select( m => new GroupMemberResult
                        {
                            Id = m.Id,
                            PersonId = m.PersonId,
                            FullName = m.Person.FullName
                        } ).ToList();

                    List<GroupMembersMissingRequirements> groupMembers = new List<GroupMembersMissingRequirements>();

                    foreach ( var groupMemberIssue in groupMembersWithIssues )
                    {
                        GroupMembersMissingRequirements groupMember = new GroupMembersMissingRequirements();
                        groupMember.FullName = groupMemberIssue.Key.Person.FullName;
                        groupMember.Id = groupMemberIssue.Key.Id;
                        groupMember.PersonId = groupMemberIssue.Key.PersonId;
                        groupMember.GroupMemberRole = groupMemberIssue.Key.GroupRole.Name;

                        List<MissingRequirement> missingRequirements = new List<MissingRequirement>();

                        // Now find exactly which ISSUE corresponds to the group member based on their role
                        foreach ( var issue in groupMemberIssue.Value )
                        {
                            // If the issue is tied to a role, does it match the person's role?
                            // If it does not, skip it.
                            if ( issue.Key.GroupRequirement.GroupRoleId != null && issue.Key.GroupRequirement.GroupRoleId != groupMemberIssue.Key.GroupRoleId )
                            {
                                continue;
                            }

                            if ( issue.Key.GroupRequirement.AppliesToAgeClassification == AppliesToAgeClassification.Adults && groupMemberIssue.Key.Person.AgeClassification != AgeClassification.Adult )
                            {
                                continue;
                            }

                            if ( issue.Key.GroupRequirement.AppliesToAgeClassification == AppliesToAgeClassification.Children && groupMemberIssue.Key.Person.AgeClassification != AgeClassification.Child )
                            {
                                continue;
                            }

                            MissingRequirement missingRequirement = new MissingRequirement();
                            missingRequirement.Id = issue.Key.GroupRequirement.GroupRequirementType.Id;
                            missingRequirement.Name = issue.Key.GroupRequirement.GroupRequirementType.Name;
                            missingRequirement.Status = issue.Key.MeetsGroupRequirement;
                            missingRequirement.OccurrenceDate = issue.Value;

                            switch ( issue.Key.MeetsGroupRequirement )
                            {
                                case MeetsGroupRequirement.Meets:
                                    missingRequirement.Message = issue.Key.GroupRequirement.GroupRequirementType.PositiveLabel;
                                    break;
                                case MeetsGroupRequirement.MeetsWithWarning:
                                    missingRequirement.Message = issue.Key.GroupRequirement.GroupRequirementType.WarningLabel;
                                    break;
                                case MeetsGroupRequirement.NotMet:
                                    missingRequirement.Message = issue.Key.GroupRequirement.GroupRequirementType.NegativeLabel;
                                    break;
                            }

                            missingRequirements.Add( missingRequirement );
                        }

                        groupMember.MissingRequirements = missingRequirements;

                        groupMembers.Add( groupMember );
                    }
                    groupMissingRequirements.GroupMembersMissingRequirements = groupMembers;

                    _groupsMissingRequirements.Add( groupMissingRequirements );

                    var membersToNotify = group.Members
                        .Where( m => m.GroupRole.ReceiveRequirementsNotifications
                            && m.GroupMemberStatus == GroupMemberStatus.Active
                            && m.IsArchived == false );

                    // add leaders as people to notify
                    foreach ( var leader in membersToNotify )
                    {
                        NotificationItem notification = new NotificationItem();
                        notification.GroupId = group.Id;
                        notification.Person = leader.Person;
                        _notificationList.Add( notification );
                    }

                    // notify parents
                    if ( notificationOption != NotificationOption.None )
                    {
                        var parentLeadersQuery = new GroupMemberService( rockContext ).Queryable( "Person" ).AsNoTracking()
                            .Where( m => m.GroupRole.ReceiveRequirementsNotifications
                                && m.GroupMemberStatus == GroupMemberStatus.Active
                                && m.IsArchived == false );

                        if ( notificationOption == NotificationOption.DirectParent )
                        {
                            // just the parent group
                            parentLeadersQuery = parentLeadersQuery.Where( m => m.GroupId == group.ParentGroupId );
                        }
                        else
                        {
                            // all parents in the hierarchy
                            var parentIds = groupService.GetAllAncestorIds( group.Id );
                            parentLeadersQuery = parentLeadersQuery.Where( m => parentIds.Contains( m.GroupId ) );
                        }

                        foreach ( var parentLeader in parentLeadersQuery.ToList() )
                        {
                            NotificationItem parentNotification = new NotificationItem();
                            parentNotification.Person = parentLeader.Person;
                            parentNotification.GroupId = group.Id;
                            _notificationList.Add( parentNotification );
                        }
                    }
                }
            }

            // send out notifications
            int recipients = 0;
            var notificationRecipients = _notificationList.GroupBy( p => p.Person.Id ).ToList();
            foreach ( var recipientId in notificationRecipients )
            {
                var recipient = _notificationList.Where( n => n.Person.Id == recipientId.Key ).Select( n => n.Person ).FirstOrDefault();

                if ( !recipient.IsEmailActive || recipient.Email.IsNullOrWhiteSpace() || recipient.EmailPreference == EmailPreference.DoNotEmail )
                {
                    continue;
                }

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                mergeFields.Add( "Person", recipient );
                var notificationGroupIds = _notificationList
                                                .Where( n => n.Person.Id == recipient.Id )
                                                .Select( n => n.GroupId )
                                                .ToList();
                var missingRequirements = _groupsMissingRequirements.Where( g => notificationGroupIds.Contains( g.Id ) ).ToList();
                mergeFields.Add( "GroupsMissingRequirements", missingRequirements );

                var emailMessage = new RockEmailMessage( systemEmailGuid.Value );
                emailMessage.AddRecipient( new RockEmailMessageRecipient( recipient, mergeFields ) );
                var emailErrors = new List<string>();
                emailMessage.Send( out emailErrors );
                errors.AddRange( emailErrors );

                recipients++;
            }

            // add accountability group members
            if ( !accountAbilityGroupGuid.IsEmpty() )
            {
                var accountabilityGroupMembers = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                                    .Where( m => m.Group.Guid == accountAbilityGroupGuid
                                                        && m.GroupMemberStatus == GroupMemberStatus.Active )
                                                    .Select( m => m.Person );

                var emailMessage = new RockEmailMessage( systemEmailGuid.Value );
                foreach ( var person in accountabilityGroupMembers )
                {
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                    mergeFields.Add( "Person", person );
                    mergeFields.Add( "GroupsMissingRequirements", _groupsMissingRequirements );
                    emailMessage.AddRecipient( new RockEmailMessageRecipient( person, mergeFields ) );
                    recipients++;
                }

                if ( accountabilityGroupMembers != null && accountabilityGroupMembers.Count() > 0 )
                { 
                    var emailErrors = new List<string>();
                    emailMessage.Send( out emailErrors );
                    errors.AddRange( emailErrors );
                }
            }

            this.Result = string.Format( "{0} requirement notification {1} sent", recipients, "email".PluralizeIf( recipients != 1 ) );

            if ( errors.Any() )
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine();
                sb.Append( string.Format( "{0} Errors: ", errors.Count() ) );
                errors.ForEach( e => { sb.AppendLine(); sb.Append( e ); } );
                string errorMessage = sb.ToString();
                this.Result += errorMessage;
                var exception = new Exception( errorMessage );
                HttpContext context2 = HttpContext.Current;
                ExceptionLogService.LogException( exception, context2 );
                throw exception;
            }
            
            
        }
    }

    #region Helper Classes

    /// <summary>
    /// Notification Option Enum
    /// </summary>
    public enum NotificationOption
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// The direct parent
        /// </summary>
        DirectParent = 1,

        /// <summary>
        /// All parents
        /// </summary>
        AllParents = 2
    }

    /// <summary>
    /// Notification Item
    /// </summary>
    public class NotificationItem
    {
        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public Person Person { get; set; }

        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        public int GroupId { get; set; }
    }

    /// <summary>
    /// Group Missing Requirements
    /// </summary>
    [LavaType( "Id", "Name", "GroupMembersMissingRequirements", "AncestorPathName", "GroupTypeId", "GroupTypeName", "Leaders" )]
    [DotLiquid.LiquidType( "Id", "Name", "GroupMembersMissingRequirements", "AncestorPathName", "GroupTypeId", "GroupTypeName", "Leaders" )]
    public class GroupsMissingRequirements
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the group members missing requirements.
        /// </summary>
        /// <value>
        /// The group members missing requirements.
        /// </value>
        public List<GroupMembersMissingRequirements> GroupMembersMissingRequirements { get; set; }

        /// <summary>
        /// Gets or sets the name of the ancestor path.
        /// </summary>
        /// <value>
        /// The name of the ancestor path.
        /// </value>
        public string AncestorPathName { get; set; }

        /// <summary>
        /// Gets or sets the group type identifier.
        /// </summary>
        /// <value>
        /// The group type identifier.
        /// </value>
        public int GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the name of the group type.
        /// </summary>
        /// <value>
        /// The name of the group type.
        /// </value>
        public string GroupTypeName { get; set; }

        /// <summary>
        /// Gets or sets the leaders.
        /// </summary>
        /// <value>
        /// The leaders.
        /// </value>
        public List<GroupMemberResult> Leaders { get; set; }
    }

    /// <summary>
    /// Group Member Missing Requirements
    /// </summary>
    [LavaType( "Id", "PersonId", "FullName", "GroupMemberRole", "MissingRequirements" )]
    [DotLiquid.LiquidType( "Id", "PersonId", "FullName", "GroupMemberRole", "MissingRequirements" )]
    public class GroupMembersMissingRequirements : GroupMemberResult
    {
        /// <summary>
        /// Gets or sets the missing requirements.
        /// </summary>
        /// <value>
        /// The missing requirements.
        /// </value>
        public List<MissingRequirement> MissingRequirements { get; set; }
    }

    /// <summary>
    /// Missing Requirement
    /// </summary>
    [LavaType( "Id", "Name", "Status", "Message", "OccurrenceDate" )]
    [DotLiquid.LiquidType( "Id", "Name", "Status", "Message", "OccurrenceDate" )]
    public class MissingRequirement
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public MeetsGroupRequirement Status { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the occurrence date.
        /// </summary>
        /// <value>
        /// The occurrence date.
        /// </value>
        public DateTime OccurrenceDate { get; set; }
    }

    /// <summary>
    /// Group Member Result
    /// </summary>
    [LavaType( "Id", "PersonId", "FullName", "GroupMemberRole" )]
    [DotLiquid.LiquidType( "Id", "PersonId", "FullName", "GroupMemberRole" )]
    public class GroupMemberResult
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the full name.
        /// </summary>
        /// <value>
        /// The full name.
        /// </value>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the group member role.
        /// </summary>
        /// <value>
        /// The group member role.
        /// </value>
        public string GroupMemberRole { get; set; }
    }

    #endregion
}
