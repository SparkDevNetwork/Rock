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
    [BooleanField("Require Password Reset On New Logins", "Determines if new logins should be created in such a way that the individual will need to reset the password on their first login.", Key = "RequirePasswordReset")]
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
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            bool requirePasswordReset = dataMap.GetBoolean( "RequirePasswordReset" );

            int groupsSynced = 0;
            int groupsChanged = 0;

            try
            {
                // get groups set to sync
                GroupService groupService = new GroupService( new RockContext() );
                var groupIdsThatSync = groupService.Queryable().Where( g => g.SyncDataViewId != null ).Select( a => a.Id ).ToList();

                foreach ( var syncGroupId in groupIdsThatSync )
                {
                    bool hasGroupChanged = false;

                    // use a fresh rockContext per group so that ChangeTracker doesn't get bogged down
                    using ( var rockContext = new RockContext() )
                    {
                        var syncGroup = new GroupService( rockContext ).Get( syncGroupId );
                        GroupMemberService groupMemberService = new GroupMemberService( rockContext );

                        // increase the timeout just in case the dataview source is slow
                        rockContext.Database.CommandTimeout = 180;

                        var syncSource = new DataViewService( rockContext ).Get( syncGroup.SyncDataViewId.Value );

                        // ensure this is a person dataview
                        bool isPersonDataSet = syncSource.EntityTypeId == EntityTypeCache.Read( typeof( Rock.Model.Person ) ).Id;

                        if ( isPersonDataSet )
                        {
                            SortProperty sortById = new SortProperty();
                            sortById.Property = "Id";
                            sortById.Direction = System.Web.UI.WebControls.SortDirection.Ascending;
                            List<string> errorMessages = new List<string>();

                            var personService = new PersonService( rockContext );
                            var parameterExpression = personService.ParameterExpression;
                            var whereExpression = syncSource.GetExpression( personService, parameterExpression, out errorMessages );
                            var sourceItems = personService.Get( parameterExpression, whereExpression ).Select( q => q.Id ).ToList();
                            var targetItems = groupMemberService.Queryable().Where( gm => gm.GroupId == syncGroup.Id ).ToList();

                            // delete items from the target not in the source
                            foreach ( var targetItem in targetItems.Where( t => !sourceItems.Contains( t.PersonId ) ) )
                            {
                                // made a clone of the person as it will be detached when the group member is deleted. Also
                                // saving the delete before the email is sent in case an exception occurs so the user doesn't
                                // get an email everytime the agent runs.
                                Person recipient = (Person)targetItem.Person.Clone();
                                groupMemberService.Delete( targetItem );

                                rockContext.SaveChanges();

                                hasGroupChanged = true;

                                if ( syncGroup.ExitSystemEmailId.HasValue )
                                {
                                    SendExitEmail( syncGroup.ExitSystemEmailId.Value, recipient, syncGroup );
                                }
                            }

                            // add items not in target but in the source
                            foreach ( var sourceItem in sourceItems.Where( s => !targetItems.Select( t => t.PersonId ).Contains( s ) ) )
                            {
                                // add source to target
                                var newGroupMember = new GroupMember { Id = 0 };
                                newGroupMember.PersonId = sourceItem;
                                newGroupMember.Group = syncGroup;
                                newGroupMember.GroupMemberStatus = GroupMemberStatus.Active;
                                newGroupMember.GroupRoleId = syncGroup.GroupType.DefaultGroupRoleId ?? syncGroup.GroupType.Roles.FirstOrDefault().Id;
                                groupMemberService.Add( newGroupMember );

                                hasGroupChanged = true;

                                if ( syncGroup.WelcomeSystemEmailId.HasValue )
                                {
                                    SendWelcomeEmail( syncGroup.WelcomeSystemEmailId.Value, sourceItem, syncGroup, syncGroup.AddUserAccountsDuringSync ?? false, requirePasswordReset );
                                }
                            }

                            if ( hasGroupChanged )
                            {
                                groupsChanged++;
                            }

                            groupsSynced++;

                            rockContext.SaveChanges();

                            if ( hasGroupChanged && ( syncGroup.IsSecurityRole || syncGroup.GroupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() ) ) )
                            {
                                Rock.Security.Role.Flush( syncGroup.Id );
                            }
                        }
                    }
                }

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

        /// <summary>
        /// Sends the welcome email.
        /// </summary>
        /// <param name="systemEmailId">The system email identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="syncGroup">The synchronize group.</param>
        /// <param name="createLogin">if set to <c>true</c> [create login].</param>
        /// <param name="requirePasswordReset">if set to <c>true</c> [require password reset].</param>
        private void SendWelcomeEmail( int systemEmailId, int personId, Group syncGroup, bool createLogin, bool requirePasswordReset )
        {

            using ( var rockContext = new RockContext() )
            {
                SystemEmailService emailService = new SystemEmailService( rockContext );

                var systemEmail = emailService.Get( systemEmailId );

                if ( systemEmail != null )
                {
                    string newPassword = string.Empty;

                    var recipients = new List<RecipientData>();

                    var mergeFields = new Dictionary<string, object>();
                    mergeFields.Add( "Group", syncGroup );

                    // get person
                    var recipient = new PersonService( rockContext ).Queryable( "Users" ).Where( p => p.Id == personId ).FirstOrDefault();

                    if ( !string.IsNullOrWhiteSpace( recipient.Email ) )
                    {
                        if ( createLogin && recipient.Users.Count == 0 )
                        {
                            newPassword = System.Web.Security.Membership.GeneratePassword( 9, 1 );

                            // create user
                            string username = Rock.Security.Authentication.Database.GenerateUsername( recipient.NickName, recipient.LastName );

                            UserLogin login = UserLoginService.Create(
                                rockContext,
                                recipient,
                                Rock.Model.AuthenticationServiceType.Internal,
                                EntityTypeCache.Read( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id,
                                username,
                                newPassword,
                                true,
                                requirePasswordReset );
                        }
                        mergeFields.Add( "Person", recipient );
                        mergeFields.Add( "NewPassword", newPassword );
                        mergeFields.Add( "CreateLogin", createLogin );
                        recipients.Add( new RecipientData( recipient.Email, mergeFields ) );

                        var emailMessage = new RockEmailMessage( systemEmail.Guid );
                        emailMessage.SetRecipients( recipients );
                        emailMessage.Send();
                    }
                }
            }
        }

        /// <summary>
        /// Sends the exit email.
        /// </summary>
        /// <param name="systemEmailId">The system email identifier.</param>
        /// <param name="recipient">The recipient.</param>
        /// <param name="syncGroup">The synchronize group.</param>
        private void SendExitEmail( int systemEmailId, Person recipient, Group syncGroup )
        {
            if ( !string.IsNullOrWhiteSpace( recipient.Email ) )
            {
                using ( var rockContext = new RockContext() )
                {
                    var systemEmail = new SystemEmailService( rockContext ).Get( systemEmailId );
                    if ( systemEmail != null )
                    {
                        var mergeFields = new Dictionary<string, object>();
                        mergeFields.Add( "Group", syncGroup );
                        mergeFields.Add( "Person", recipient );

                        var emailMessage = new RockEmailMessage( systemEmail );
                        emailMessage.AddRecipient( new RecipientData( recipient.Email, mergeFields ) );
                        emailMessage.Send();
                    }
                }
            }
        }
    }
}
