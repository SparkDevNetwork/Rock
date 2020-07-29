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
using System.Threading.Tasks;
using System.Web;
using Quartz;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{

    /// <summary>
    /// This job sends a list of group member absences to the group's leaders.
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
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

    [DisallowConcurrentExecution]
    public class GroupLeaderAbsenceNotifications : IJob
    {
        /// <summary>
        /// Keys for DataMap Field Attributes.
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

        /// <summary>
        /// Job that will sync groups. Called by the <see cref="IScheduler" /> when an <see cref="ITrigger" />
        /// fires that is associated with the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            try
            {
                ProcessJob( context );
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
        private void ProcessJob( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            int notificationsSent = 0;
            int errorsEncountered = 0;
            int absentMembersCount = 0;
            int sendFailed = 0;

            // get groups set to sync
            RockContext rockContext = new RockContext();

            Guid? groupTypeGuid = dataMap.GetString( AttributeKey.GroupType ).AsGuidOrNull();
            Guid? systemEmailGuid = dataMap.GetString( AttributeKey.NotificationEmail ).AsGuidOrNull();
            Guid? groupRoleFilterGuid = dataMap.GetString( AttributeKey.GroupRoleFilter ).AsGuidOrNull();
            int minimumAbsences = dataMap.GetString( AttributeKey.MinimumAbsences ).AsInteger();

            // get system email
            var emailService = new SystemCommunicationService( rockContext );

            SystemCommunication systemEmail = null;
            if ( !systemEmailGuid.HasValue || systemEmailGuid == Guid.Empty )
            {
                context.Result = "Job failed. Unable to find System Email";
                throw new Exception( "No system email found." );
            }

            if ( minimumAbsences == default( int ) )
            {
                context.Result = "Job failed. The is no minimum absense count entered.";
                throw new Exception( "No minimum absense count found." );
            }
            systemEmail = emailService.Get( systemEmailGuid.Value );

            // get group members
            if ( !groupTypeGuid.HasValue || groupTypeGuid == Guid.Empty )
            {
                context.Result = "Job failed. Unable to find group type";
                throw new Exception( "No group type found." );
            }

            var groupMemberQry = new GroupMemberService( rockContext ).Queryable( "Group, Group.Members.GroupRole" )
                                     .Where( m => m.Group.GroupType.Guid == groupTypeGuid.Value );

            if ( groupRoleFilterGuid.HasValue )
            {
                groupMemberQry = groupMemberQry.Where( m => m.GroupRole.Guid == groupRoleFilterGuid.Value );
            }

            var groupMembers = groupMemberQry.GroupBy( m => m.Group );
            var errorList = new List<string>();
            foreach ( var groupGroupMember in groupMembers )
            {
                var group = groupGroupMember.Key;
                var filteredPersons = groupGroupMember.Select( a => a.PersonId );
                // get list of leaders
                var groupLeaders = group.Members.Where( m => m.GroupRole.IsLeader == true && m.Person != null && m.Person.Email != null && m.Person.Email != string.Empty );

                if ( !groupLeaders.Any() )
                {
                    errorList.Add( "Unable to send emails to members in group " + group.Name + " because there is no group leader" );
                    continue;
                }

                // Get all the occurrences for this group
                var occurrences = new AttendanceOccurrenceService( rockContext )
                    .Queryable( "Attendees.PersonAlias.Person" )
                    .Where( a => a.DidNotOccur.HasValue && !a.DidNotOccur.Value && a.GroupId == group.Id )
                    .OrderByDescending( a => a.OccurrenceDate )
                    .Take( minimumAbsences )
                    .ToList();

                if ( occurrences.Count == minimumAbsences )
                {
                    var absentPersons = occurrences
                                                .SelectMany( a => a.Attendees )
                                                .Where( a => a.DidAttend.HasValue && !a.DidAttend.Value && filteredPersons.Contains( a.PersonAlias.PersonId ) )
                                                .GroupBy( a => a.PersonAlias.Person )
                                                .Where( a => a.Count() == minimumAbsences )
                                                .Select( a => a.Key )
                                                .ToList();

                    if ( absentPersons.Count > 0 )
                    {
                        var recipients = new List<RockEmailMessageRecipient>();
                        foreach ( var leader in groupLeaders )
                        {
                            // create merge object
                            var mergeFields = new Dictionary<string, object>();
                            mergeFields.Add( "AbsentMembers", absentPersons );
                            mergeFields.Add( "Group", group );
                            mergeFields.Add( "Person", leader.Person );
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

                        // be conservative: only mark as notified if we are sure the email didn't fail
                        if ( errorMessages.Any() )
                        {
                            continue;
                        }

                        absentMembersCount += absentPersons.Count;
                        notificationsSent += recipients.Count();
                    }
                }
            }

            context.Result = string.Format( "Sent {0} emails to leaders for {1} absent members. {2} errors encountered. {3} times Send reported a fail.", notificationsSent, absentMembersCount, errorsEncountered, sendFailed );

            if ( errorList.Any() )
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine();
                sb.Append( "Errors in GroupLeaderAbsenceNotifications: " );
                errorList.ForEach( e => { sb.AppendLine(); sb.Append( e ); } );
                string errors = sb.ToString();
                context.Result += errors;
                throw new Exception( errors );
            }

        }
    }
}