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
using System.Linq;
using System.Text;
using System.Web;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{

    /// <summary>
    /// This job sends a list of group member absences to the group's leaders.
    /// </summary>
    [DisplayName( "Group Leader Absence Notifications" )]
    [Description( "This job sends a list of group member absences to the group's leaders." )]

    #region DataMap Field Attributes
    [GroupTypeField( "Group Type",
        Key = AttributeKey.GroupType,
        Description = "The group type to look for absent group members.",
        IsRequired = true,
        Order = 0 )]

    [SystemCommunicationField( "Notification Email",
        Key = AttributeKey.NotificationEmail,
        IsRequired = true,
        Order = 1 )]

    [GroupRoleField( null, "Group Role Filter", "Optional group role to filter the absent members by. To select the role you'll need to select a group type.", false, null, null, 2, AttributeKey.GroupRoleFilter )]

    [IntegerField( "Minimum Absences",
        Key = AttributeKey.MinimumAbsences,
        Description = @"The number of most recent consecutive meeting occurrences that the group member will need to have
                      missed to be included in the notification email. If group attendance is not recorded or the group did
                      not meet, that occurrence will not be considered.",
        IsRequired = false,
        DefaultIntegerValue = 3,
        Order = 3 )]

    #endregion

    public class GroupLeaderAbsenceNotifications : RockJob
    {
        /// <summary>
        /// Keys to use for job Attributes.
        /// </summary>
        private static class AttributeKey
        {
            public const string GroupType = "GroupType";
            public const string NotificationEmail = "NotificationEmail";
            public const string GroupRoleFilter = "GroupRoleFilter";
            public const string MinimumAbsences = "MinimumAbsences";
        }

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public GroupLeaderAbsenceNotifications()
        {
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            try
            {
                ProcessJob();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, HttpContext.Current );
                throw;
            }
        }

        /// <summary>
        /// Private method called by Execute() to process the job.
        /// </summary>
        private void ProcessJob()
        {
            int notificationsSent = 0;
            int errorsEncountered = 0;
            int absentMembersCount = 0;
            int sendFailed = 0;

            // get groups set to sync
            RockContext rockContext = new RockContext();

            Guid? groupTypeGuid = GetAttributeValue( AttributeKey.GroupType ).AsGuidOrNull();
            Guid? systemEmailGuid = GetAttributeValue( AttributeKey.NotificationEmail ).AsGuidOrNull();
            Guid? groupRoleFilterGuid = GetAttributeValue( AttributeKey.GroupRoleFilter ).AsGuidOrNull();
            int minimumAbsences = GetAttributeValue( AttributeKey.MinimumAbsences ).AsInteger();

            // get system email
            var emailService = new SystemCommunicationService( rockContext );

            SystemCommunication systemEmail = null;
            if ( !systemEmailGuid.HasValue || systemEmailGuid == Guid.Empty )
            {
                this.Result = "Job failed. Unable to find System Email.";
                throw new Exception( "No system email found." );
            }

            if ( minimumAbsences == default( int ) )
            {
                this.Result = "Job failed. There is no minimum absence count entered.";
                throw new Exception( "No minimum absence count found." );
            }
            systemEmail = emailService.Get( systemEmailGuid.Value );

            // get group members
            if ( !groupTypeGuid.HasValue || groupTypeGuid == Guid.Empty )
            {
                this.Result = "Job failed. Unable to find group type.";
                throw new Exception( "No group type found." );
            }

            var groupMemberQry = new GroupMemberService( rockContext )
                .Queryable( "Group, Group.Members.GroupRole" )
                .Where( m =>
                    !m.Group.IsArchived
                    && m.Group.IsActive
                    && m.Group.GroupType.Guid == groupTypeGuid.Value
                    && m.GroupMemberStatus != GroupMemberStatus.Inactive
                );

            if ( groupRoleFilterGuid.HasValue )
            {
                groupMemberQry = groupMemberQry.Where( m => m.GroupRole.Guid == groupRoleFilterGuid.Value );
            }

            var groupMembersByGroup = groupMemberQry.GroupBy( m => m.Group );
            var errorList = new List<string>();
            foreach ( var groupMembersInGroup in groupMembersByGroup )
            {
                var group = groupMembersInGroup.Key;
                var groupMemberPersonIds = groupMembersInGroup.Select( a => a.PersonId );

                // Get list of leaders to email.
                var groupLeaders = group.Members.Where( m =>
                    !m.IsArchived
                    && m.GroupMemberStatus != GroupMemberStatus.Inactive
                    && m.GroupRole.IsLeader == true
                    && m.Person != null
                    && m.Person.Email != null
                    && m.Person.Email != string.Empty
                );

                if ( !groupLeaders.Any() )
                {
                    errorList.Add( $"Unable to send emails to members in group {group.Name} because there is no group leader." );
                    continue;
                }

                // Attendees who miss the most recent N consecutive occurrences (ignoring occurrences that did not occur)
                // will be included in the notification email.
                var occurrences = new AttendanceOccurrenceService( rockContext )
                    .Queryable( "Attendees.PersonAlias.Person" )
                    .Where( a => a.DidNotOccur != true )
                    .Where( a => a.GroupId == group.Id )
                    .Where( a => a.OccurrenceDate <= RockDateTime.Today ) // Ignore future occurrences if there are any.
                    .OrderByDescending( a => a.OccurrenceDate )
                    .Take( minimumAbsences )
                    .ToList();

                if ( occurrences.Count != minimumAbsences )
                {
                    // Skip to the next group if this group doesn't
                    // have the minimum number of occurrences to check.
                    continue;
                }

                // Get the attendees who have missed every one of the most recent N occurrences.
                var absentPersons = occurrences
                    .SelectMany( a => a.Attendees )
                    .Where( a => a.DidAttend != true && groupMemberPersonIds.Contains( a.PersonAlias.PersonId ) )
                    .GroupBy( a => a.PersonAlias.Person )
                    .Where( a => a.Count() == minimumAbsences )
                    .Select( a => a.Key )
                    .ToList();

                if ( absentPersons.Count == 0 )
                {
                    // Skip to the next group if this group has no attendees
                    // who missed the most recent N occurrences.
                    continue;
                }

                var recipients = new List<RockEmailMessageRecipient>();
                foreach ( var leader in groupLeaders )
                {
                    var mergeFields = new Dictionary<string, object>
                    {
                        { "AbsentMembers", absentPersons },
                        { "Group", group },
                        { "Person", leader.Person }
                    };
                    recipients.Add( new RockEmailMessageRecipient( leader.Person, mergeFields ) );
                }

                var errorMessages = new List<string>();
                var emailMessage = new RockEmailMessage( systemEmail.Guid );
                emailMessage.SetRecipients( recipients );
                var sendSuccess = emailMessage.Send( out errorMessages );
                if ( !sendSuccess )
                {
                    sendFailed++;
                }

                errorsEncountered += errorMessages.Count;
                errorList.AddRange( errorMessages );

                // Be conservative: only mark as notified if we are sure the email didn't fail.
                if ( errorMessages.Any() )
                {
                    continue;
                }

                absentMembersCount += absentPersons.Count;
                notificationsSent += recipients.Count();
            }

            this.Result = $"Sent {notificationsSent} emails to leaders for {absentMembersCount} absent members. {errorsEncountered} errors encountered. {sendFailed} times Send reported a fail.";

            if ( errorList.Any() )
            {
                var errors = $"Errors in GroupLeaderAbsenceNotifications: {Environment.NewLine}{string.Join( Environment.NewLine, errorList )}";
                this.Result += $"{Environment.NewLine}{errors}";
                throw new Exception( errors );
            }
        }
    }
}