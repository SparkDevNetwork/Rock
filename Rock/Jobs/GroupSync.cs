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
    /// Job to run quick SQL queries on a schedule
    /// </summary>
    [DisallowConcurrentExecution]
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

            try
            {
                // get groups set to sync
                RockContext rockContext = new RockContext();
                GroupService groupService = new GroupService( rockContext );
                var groupsThatSync = groupService.Queryable().Where( g => g.SyncDataViewId != null ).ToList();

                foreach ( var syncGroup in groupsThatSync )
                {
                    GroupMemberService groupMemberService = new GroupMemberService( rockContext );

                    var syncSource = new DataViewService( rockContext ).Get( syncGroup.SyncDataViewId.Value );

                    // ensure this is a person dataview
                    bool isPersonDataSet = syncSource.EntityTypeId == EntityTypeCache.Read( typeof( Rock.Model.Person ) ).Id;

                    if ( isPersonDataSet )
                    {
                        SortProperty sortById = new SortProperty();
                        sortById.Property = "Id";
                        sortById.Direction = System.Web.UI.WebControls.SortDirection.Ascending;
                        List<string> errorMessages = new List<string>();

                        var sourceItems = syncSource.GetQuery( sortById, 180, out errorMessages ).Select( q => q.Id ).ToList();
                        var targetItems = groupMemberService.Queryable().Where( gm => gm.GroupId == syncGroup.Id );

                        // delete items from the target not in the source
                        foreach ( var targetItem in targetItems.Where( t => !sourceItems.Contains( t.PersonId ) ) )
                        {
                            groupMemberService.Delete( targetItem );

                            if ( syncGroup.WelcomeSystemEmailId.HasValue )
                            {
                                SendExitEmail( syncGroup.WelcomeSystemEmailId.Value, targetItem.Person, syncGroup );
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

                            if ( syncGroup.WelcomeSystemEmailId.HasValue )
                            {
                                SendWelcomeEmail( syncGroup.WelcomeSystemEmailId.Value, sourceItem, syncGroup, syncGroup.AddUserAccountsDuringSync ?? false );
                            }
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

        /// <summary>
        /// Sends the welcome email.
        /// </summary>
        /// <param name="systemEmailId">The system email identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="createLogin">if set to <c>true</c> [create login].</param>
        private void SendWelcomeEmail( int systemEmailId, int personId, Group syncGroup, bool createLogin )
        {
            RockContext rockContext = new RockContext();
            SystemEmailService emailService = new SystemEmailService( rockContext );

            var systemEmail = emailService.Get( systemEmailId );

            if ( systemEmail != null )
            {
                string newPassword = string.Empty;
                
                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Group", syncGroup );
                
                // get person
                var recipient = new PersonService( rockContext ).Queryable("Users").Where(p => p.Id == personId).FirstOrDefault();

                if ( !string.IsNullOrWhiteSpace( recipient.Email ) )
                {
                    if ( createLogin && recipient.Users.Count == 0 )
                    {
                        newPassword = System.Web.Security.Membership.GeneratePassword( 9, 4 );
                        
                        // create user
                        UserLogin login = UserLoginService.Create(
                            rockContext,
                            recipient,
                            Rock.Model.AuthenticationServiceType.Internal,
                            EntityTypeCache.Read( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id,
                            tbUserName.Text,
                            newPassword,
                            true );
                    }
                    mergeFields.Add( "Person", recipient );
                    mergeFields.Add( "NewPassword", newPassword );

                    var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext ).GetValue( "ExternalApplicationRoot" );

                    var recipients = new List<RecipientData>();
                    recipients.Add( new RecipientData( recipient.Email, mergeFields ) );

                    Email.Send( systemEmail.Guid, recipients, appRoot );
                }
            }
        }

        /// <summary>
        /// Sends the exit email.
        /// </summary>
        /// <param name="systemEmailId">The system email identifier.</param>
        /// <param name="recipient">The recipient.</param>
        private void SendExitEmail( int systemEmailId, Person recipient, Group syncGroup )
        {
            RockContext rockContext = new RockContext();
            SystemEmailService emailService = new SystemEmailService( rockContext );

            var systemEmail = emailService.Get( systemEmailId );

            if ( systemEmail != null )
            {
                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Group", syncGroup );
                mergeFields.Add( "Person", recipient );

                var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext ).GetValue( "ExternalApplicationRoot" );

                var recipients = new List<RecipientData>();
                recipients.Add( new RecipientData( recipient.Email, mergeFields ) );

                Email.Send( systemEmail.Guid, recipients, appRoot );
            }
        }
    }
}
