// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Data.Entity;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Web.Cache;
using Rock.Communication;

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
                // get groups set to sync
                RockContext rockContext = new RockContext();

                Guid? groupTypeGuid = dataMap.GetString( "GroupType" ).AsGuidOrNull();
                Guid? systemEmailGuid = dataMap.GetString( "NotificationEmail" ).AsGuidOrNull();
                Guid? groupRoleFilterGuid = dataMap.GetString( "GroupRoleFilter" ).AsGuidOrNull();

                bool includePreviouslyNotificed = dataMap.GetString( "IncludePreviouslyNotified" ).AsBoolean();

                // get system email
                SystemEmailService emailService = new SystemEmailService( rockContext );

                SystemEmail systemEmail = null;
                if ( systemEmailGuid.HasValue )
                {
                    systemEmail = emailService.Get( systemEmailGuid.Value );
                }

                if ( systemEmail == null )
                {
                    // no email specified, so nothing to do
                    return;
                }

                // get group members
                if ( groupTypeGuid.HasValue && groupTypeGuid != Guid.Empty )
                {
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

                    var pendingGroupMembers = qry.ToList();


                    var groups = pendingGroupMembers.GroupBy( m => m.Group );

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

                        // get list of leaders
                        var groupLeaders = group.Members.Where( m => m.GroupRole.IsLeader == true );

                        var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext ).GetValue( "ExternalApplicationRoot" );

                        var recipients = new List<RecipientData>();
                        foreach ( var leader in groupLeaders )
                        {
                            // create merge object
                            var mergeFields = new Dictionary<string, object>();
                            mergeFields.Add( "PendingIndividuals", pendingIndividuals );
                            mergeFields.Add( "Group", group );
                            mergeFields.Add( "ParentGroup", group.ParentGroup );
                            mergeFields.Add( "Person", leader.Person );
                            recipients.Add( new RecipientData( leader.Person.Email, mergeFields ) );
                        }

                        if ( pendingIndividuals.Count() > 0 )
                        {
                            Email.Send( systemEmail.Guid, recipients, appRoot );
                        }

                        // mark pending members as notified as we go in case the job fails
                        var notifiedPersonIds = pendingIndividuals.Select( p => p.Id );
                        foreach ( var pendingGroupMember in pendingGroupMembers.Where( m => m.IsNotified == false && notifiedPersonIds.Contains( m.PersonId ) ) )
                        {
                            pendingGroupMember.IsNotified = true;
                        }

                        rockContext.SaveChanges();

                    }
                }


            }
            catch ( System.Exception ex )
            {
                HttpContext context2 = HttpContext.Current;
                ExceptionLogService.LogException( ex, context2 );
                throw ex;
            }
        }
    }
}
