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
using System.Linq;
using System.Text;
using System.Web;

using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Job send list of new pending group members to the group's leaders.
    /// </summary>
    /// 

    [GroupTypeField( "Group Type", "The group type to look for new pending registrations", true, "", "", 0 )]
    [BooleanField( "Include Previously Notified", "Includes pending group members that have already been notified.", false, "", 1 )]
    [SystemEmailField( "Notification Email", "", true, "", "", 2 )]
    [GroupRoleField( null, "Group Role Filter", "Optional group role to filter the pending members by. To select the role you'll need to select a group type.", false, null, null, 3 )]
    [IntegerField( "Pending Age", "The number of days since the record was last updated. This keeps the job from notifying all the pending registrations on first run.", false, 1, order: 4 )]
    [DisallowConcurrentExecution]
    public class GroupLeaderPendingNotifications : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public GroupLeaderPendingNotifications()
        {
        }

        /// <summary>
        /// Job that will sync groups.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            try
            {
                int notificationsSent = 0;
                int errorsEncountered = 0;
                int pendingMembersCount = 0;

                // get groups set to sync
                RockContext rockContext = new RockContext();

                Guid? groupTypeGuid = dataMap.GetString( "GroupType" ).AsGuidOrNull();
                Guid? systemEmailGuid = dataMap.GetString( "NotificationEmail" ).AsGuidOrNull();
                Guid? groupRoleFilterGuid = dataMap.GetString( "GroupRoleFilter" ).AsGuidOrNull();
                int? pendingAge = dataMap.GetString( "PendingAge" ).AsIntegerOrNull();


                bool includePreviouslyNotificed = dataMap.GetString( "IncludePreviouslyNotified" ).AsBoolean();

                // get system email
                SystemEmailService emailService = new SystemEmailService( rockContext );

                SystemEmail systemEmail = null;
                if ( !systemEmailGuid.HasValue || systemEmailGuid == Guid.Empty )
                {
                    context.Result = "Job failed. Unable to find System Email";
                    throw new Exception( "No system email found." );
                }

                systemEmail = emailService.Get( systemEmailGuid.Value );

                // get group members
                if ( !groupTypeGuid.HasValue || groupTypeGuid == Guid.Empty )
                {
                    context.Result = "Job failed. Unable to find group type";
                    throw new Exception( "No group type found" );
                }

                var qry = new GroupMemberService( rockContext ).Queryable( "Person, Group, Group.Members.GroupRole" )
                                            .Where( m => m.Group.GroupType.Guid == groupTypeGuid.Value
                                                && m.GroupMemberStatus == GroupMemberStatus.Pending );

                if ( !includePreviouslyNotificed )
                {
                    qry = qry.Where( m => m.IsNotified == false );
                }

                if ( groupRoleFilterGuid.HasValue )
                {
                    qry = qry.Where( m => m.GroupRole.Guid == groupRoleFilterGuid.Value );
                }

                if ( pendingAge.HasValue && pendingAge.Value > 0 )
                {
                    var ageDate = RockDateTime.Now.AddDays( pendingAge.Value * -1 );
                    qry = qry.Where( m => m.ModifiedDateTime > ageDate );
                }

                var pendingGroupMembers = qry.ToList();

                var groups = pendingGroupMembers.GroupBy( m => m.Group );

                var errorList = new List<string>();
                foreach ( var groupKey in groups )
                {
                    var group = groupKey.Key;

                    // get list of pending people
                    var qryPendingIndividuals = group.Members.Where( m => m.GroupMemberStatus == GroupMemberStatus.Pending );

                    if ( !includePreviouslyNotificed )
                    {
                        qryPendingIndividuals = qryPendingIndividuals.Where( m => m.IsNotified == false );
                    }

                    if ( groupRoleFilterGuid.HasValue )
                    {
                        qryPendingIndividuals = qryPendingIndividuals.Where( m => m.GroupRole.Guid == groupRoleFilterGuid.Value );
                    }

                    var pendingIndividuals = qryPendingIndividuals.Select( m => m.Person ).ToList();

                    if ( !pendingIndividuals.Any() )
                    {
                        continue;
                    }

                    // get list of leaders
                    var groupLeaders = group.Members.Where( m => m.GroupRole.IsLeader == true && m.Person != null && m.Person.Email != null && m.Person.Email != string.Empty );

                    if ( !groupLeaders.Any() )
                    {
                        errorList.Add( "Unable to send emails to members in group " + group.Name + " because there is no group leader" );
                        continue;
                    }

                    var recipients = new List<RockEmailMessageRecipient>();
                    foreach ( var leader in groupLeaders )
                    {
                        // create merge object
                        var mergeFields = new Dictionary<string, object>();
                        mergeFields.Add( "PendingIndividuals", pendingIndividuals );
                        mergeFields.Add( "Group", group );
                        mergeFields.Add( "ParentGroup", group.ParentGroup );
                        mergeFields.Add( "Person", leader.Person );
                        recipients.Add( new RockEmailMessageRecipient( leader.Person, mergeFields ) );
                    }


                    var errorMessages = new List<string>();
                    var emailMessage = new RockEmailMessage( systemEmail.Guid );
                    emailMessage.SetRecipients( recipients );
                    var sendSuccess = emailMessage.Send( out errorMessages );

                    errorsEncountered += errorMessages.Count;
                    errorList.AddRange( errorMessages );

                    // be conservative: only mark as notified if we are sure the email didn't fail 
                    if ( sendSuccess == false )
                    {
                        continue;
                    }

                    notificationsSent += recipients.Count();
                    // mark pending members as notified as we go in case the job fails
                    var notifiedPersonIds = pendingIndividuals.Select( p => p.Id );
                    foreach ( var pendingGroupMember in pendingGroupMembers.Where( m => m.IsNotified == false && m.GroupId == group.Id && notifiedPersonIds.Contains( m.PersonId ) ) )
                    {
                        pendingGroupMember.IsNotified = true;
                    }

                    rockContext.SaveChanges();
                }

                context.Result = string.Format( "Sent {0} emails to leaders for {1} pending individuals. {2} errors encountered.", notificationsSent, pendingMembersCount, errorsEncountered );
                if ( errorList.Any() )
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine();
                    sb.Append( "Errors in GroupLeaderPendingNotificationJob: " );
                    errorList.ForEach( e => { sb.AppendLine(); sb.Append( e ); } );
                    string errors = sb.ToString();
                    context.Result += errors;
                    throw new Exception( errors );
                }
            }
            catch ( Exception ex )
            {
                HttpContext context2 = HttpContext.Current;
                ExceptionLogService.LogException( ex, context2 );
                throw;
            }
        }
    }
}