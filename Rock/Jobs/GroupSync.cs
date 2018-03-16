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
    /// Job to run quick SQL queries on a schedule
    /// </summary>
    [DisallowConcurrentExecution]
    [BooleanField( "Require Password Reset On New Logins", "Determines if new logins should be created in such a way that the individual will need to reset the password on their first login.", Key = "RequirePasswordReset" )]
    public class GroupSync : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public GroupSync()
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
            // Get the job setting(s)
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            bool requirePasswordReset = dataMap.GetBoolean( "RequirePasswordReset" );

            // Counters for displaying results
            int groupsSynced = 0;
            int groupsChanged = 0;

            try
            {
                // get groups set to sync
                var activeSyncIds = new List<int>();
                using ( var rockContext = new RockContext() )
                {
                    activeSyncIds = new GroupSyncService( rockContext )
                        .Queryable().AsNoTracking()
                        .Select( x => x.Id )
                        .ToList();
                }

                foreach ( var syncId in activeSyncIds )
                {
                    bool hasSyncChanged = false;

                    // Use a fresh rockContext per syc so that ChangeTracker doesn't get bogged down
                    using ( var rockContext = new RockContext() )
                    {
                        // increase the timeout just in case the dataview source is slow
                        rockContext.Database.CommandTimeout = 180;

                        // Get the Sync
                        var sync = new GroupSyncService( rockContext )
                            .Queryable().AsNoTracking()
                            .FirstOrDefault( s => s.Id == syncId );

                        // Ensure that the group's Sync Data View is a person dataview
                        if ( sync.SyncDataView.EntityTypeId == EntityTypeCache.Read( typeof( Person ) ).Id )
                        {
                            List<string> errorMessages = new List<string>();

                            // Get the person id's from the dataview (source)
                            var personService = new PersonService( rockContext );
                            var parameterExpression = personService.ParameterExpression;
                            var whereExpression = sync.SyncDataView.GetExpression( personService, parameterExpression, out errorMessages );
                            var sourcePersonIds = new PersonService( rockContext )
                                .Get( parameterExpression, whereExpression )
                                .Select( q => q.Id )
                                .ToList();

                            // Get the person id's in the group (target)
                            var targetPersonIds = new GroupMemberService( rockContext )
                                .Queryable().AsNoTracking()
                                .Where( gm => gm.GroupId == sync.GroupId )
                                .Select( gm => gm.PersonId )
                                .ToList();

                            // Delete people from the group/role that are no longer in the dataview
                            foreach ( var personId in targetPersonIds.Where( t => !sourcePersonIds.Contains( t ) ) )
                            {
                                // Use a new context to limit the amount of change-tracking required
                                using ( var groupMemberContext = new RockContext() )
                                {
                                    // Delete any group members for the role with the person id
                                    var groupMemberService = new GroupMemberService( groupMemberContext );
                                    foreach ( var groupMember in groupMemberService
                                        .Queryable()
                                        .Where( m =>
                                            m.GroupId == sync.GroupId &&
                                            m.GroupRoleId == sync.GroupTypeRoleId &&
                                            m.PersonId == personId )
                                        .ToList() )
                                    {
                                        groupMemberService.Delete( groupMember );
                                    }

                                    groupMemberContext.SaveChanges();

                                    // If the Group has an exit email, and person has an email address, send them the exit email
                                    if ( sync.ExitSystemEmail != null )
                                    {
                                        var person = new PersonService( groupMemberContext ).Get( personId );
                                        if ( person.Email.IsNotNullOrWhitespace() )
                                        {
                                            // Send the exit email
                                            var mergeFields = new Dictionary<string, object>();
                                            mergeFields.Add( "Group", sync.Group );
                                            mergeFields.Add( "Person", person );
                                            var emailMessage = new RockEmailMessage( sync.ExitSystemEmail );
                                            emailMessage.AddRecipient( new RecipientData( person.Email, mergeFields ) );
                                            emailMessage.Send();
                                        }
                                    }
                                }

                                hasSyncChanged = true;
                            }

                            foreach ( var personId in sourcePersonIds.Where( s => !targetPersonIds.Contains( s ) ) )
                            {
                                // Use a new context to limit the amount of change-tracking required
                                using ( var groupMemberContext = new RockContext() )
                                {
                                    // Add new person to the group with the role specified in the sync
                                    var groupMemberService = new GroupMemberService( groupMemberContext );
                                    var newGroupMember = new GroupMember { Id = 0 };
                                    newGroupMember.PersonId = personId;
                                    newGroupMember.GroupId = sync.GroupId;
                                    newGroupMember.GroupMemberStatus = GroupMemberStatus.Active;
                                    newGroupMember.GroupRoleId = sync.GroupTypeRoleId;
                                    groupMemberService.Add( newGroupMember );
                                    groupMemberContext.SaveChanges();

                                    // If the Group has a welcome email, and person has an email address, send them the welcome email and possibly create a login
                                    if ( sync.WelcomeSystemEmail != null )
                                    {
                                        var person = new PersonService( groupMemberContext ).Get( personId );
                                        if ( person.Email.IsNotNullOrWhitespace() )
                                        {
                                            // If the group is configured to add a user account for anyone added to the group, and person does not yet have an
                                            // account, add one for them.
                                            string newPassword = string.Empty;
                                            bool createLogin = sync.AddUserAccountsDuringSync;
                                            
                                            // Only create a login if requested, no logins exist and we have enough information to generate a username.
                                            if ( createLogin && !person.Users.Any() && !string.IsNullOrWhiteSpace( person.NickName ) && !string.IsNullOrWhiteSpace( person.LastName ) )
                                            {
                                                newPassword = System.Web.Security.Membership.GeneratePassword( 9, 1 );
                                                string username = Rock.Security.Authentication.Database.GenerateUsername( person.NickName, person.LastName );

                                                UserLogin login = UserLoginService.Create(
                                                    groupMemberContext,
                                                    person,
                                                    AuthenticationServiceType.Internal,
                                                    EntityTypeCache.Read( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id,
                                                    username,
                                                    newPassword,
                                                    true,
                                                    requirePasswordReset );
                                            }

                                            // Send the welcome email
                                            var mergeFields = new Dictionary<string, object>();
                                            mergeFields.Add( "Group", sync.Group );
                                            mergeFields.Add( "Person", person );
                                            mergeFields.Add( "NewPassword", newPassword );
                                            mergeFields.Add( "CreateLogin", createLogin );
                                            var emailMessage = new RockEmailMessage( sync.WelcomeSystemEmail );
                                            emailMessage.AddRecipient( new RecipientData( person.Email, mergeFields ) );
                                            emailMessage.Send();
                                        }
                                    }
                                }

                                hasSyncChanged = true;
                            }

                            // Increment Groups Changed Counter (if people were deleted or added to the group)
                            if ( hasSyncChanged )
                            {
                                groupsChanged++;
                            }

                            // Increment the Groups Synced Counter
                            groupsSynced++;

                            // If the group changed, and it was a security group, flush the security for the group
                            if ( hasSyncChanged && ( sync.Group.IsSecurityRole || sync.Group.GroupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() ) ) )
                            {
                                Rock.Security.Role.Flush( sync.GroupId );
                            }
                        }
                    }
                }

                // Format the result message
                var resultMessage = string.Empty;
                if ( groupsSynced == 0 )
                {
                    resultMessage = "No groups to sync";
                }
                else if ( groupsSynced == 1 )
                {
                    resultMessage = "1 group was sync'ed";
                }
                else
                {
                    resultMessage = string.Format( "{0} groups were sync'ed", groupsSynced );
                }

                resultMessage += string.Format( " and {0} groups were changed", groupsChanged );
                context.Result = resultMessage;
            }
            catch ( System.Exception ex )
            {
                HttpContext context2 = HttpContext.Current;
                ExceptionLogService.LogException( ex, context2 );
                throw;
            }
        }
    }
}
