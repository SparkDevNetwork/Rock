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
using System.ComponentModel;
using System.Data.Entity;
using System.Drawing.Text;
using System.Linq;
using System.ServiceModel.Channels;

using Rock.Attribute;
using Rock.Data;
using Rock.Mobile;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Run once job for v15 to create a 'Mobile Application Users' rest security group, and add existing mobile application users into that group.
    /// </summary>
    [DisplayName( "Rock Update Helper v15.0 - Update Mobile Application Rest User Security." )]
    [Description( "This job will create (if doesn't exist) a new 'Mobile Application Users' security group, and add all current mobile application rest users to that group." )]

    [IntegerField( "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of transactions, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 14400 )]
    public class PostV15DataMigrationsMobileApplicationUserRestGroup : RockJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <inheritdoc />
        public override void Execute()
        {
            // get the configured timeout, or default to 240 minutes if it is blank
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 14400;
            var jobMigration = new JobMigration( commandTimeout );
            var migrationHelper = new MigrationHelper( jobMigration );

            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                var groupService = new GroupService( rockContext );

                // Checking to see if the 'RSR: Mobile Application Users' group exists.
                var mobileAppUsersGuid = SystemGuid.Group.GROUP_MOBILE_APPLICATION_USERS.AsGuid();
                var group = groupService
                    .Queryable()
                    .Include( g => g.Members )
                    .Where( g => g.Guid == mobileAppUsersGuid )
                    .SingleOrDefault();

                // If not, create it with the default values.
                if ( group == null )
                {
                    group = GetDefaultMobileUsersGroup();

                    groupService.Add( group );
                    rockContext.SaveChanges();
                }

                var groupTypeId = GroupTypeCache.Get( group.GroupTypeId ).Id;

                // Ensure all of the mobile application are in this group.
                EnsureAllMobileApplicationsAreInDefaultGroup( rockContext, group, groupTypeId );
                AddGroupsToMobileRelatedEndpointSecurityGroups( migrationHelper );

                rockContext.SaveChanges();
            }

            DeleteJob();
        }

        /// <summary>
        /// Ensures all mobile application users are in the default group.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="group">The group.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        private void EnsureAllMobileApplicationsAreInDefaultGroup( RockContext rockContext, Group group, int groupTypeId )
        {
            var userLoginService = new UserLoginService( rockContext );

            // A list of an 'ApiKeyId' object that every mobile application should contain.
            var mobileSiteUserIds = SiteCache.GetAllActiveSites()
                .Where( s => s.SiteType == Rock.Model.SiteType.Mobile )
                .Select( s => s.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>()?.ApiKeyId )
                .Where( id => id.HasValue )
                .ToList();

            // Get all of the user logins associated with a mobile site.
            var mobileSitePeopleIds = userLoginService.Queryable()
                .Where( ul => mobileSiteUserIds.Contains( ul.Id ) && ul.PersonId.HasValue )
                .Select( ul => ul.PersonId )
                .ToList();

            var groupType = GroupTypeCache.Get( groupTypeId );

            // Add each mobile application user into the group.
            foreach ( var personId in mobileSitePeopleIds )
            {
                // Ensure there are no duplicates.
                if ( !group.Members.Any( gm => gm.PersonId == personId ) )
                {

                    GroupMember groupMember = new GroupMember();

                    // We explicitly set the group here because if we don't,
                    // the group will later be loaded from the database via
                    // a pre-save hook.
                    groupMember.Group = group;
                    groupMember.GroupId = group.Id;
                    groupMember.PersonId = personId.Value;
                    groupMember.GroupRoleId = groupType?.DefaultGroupRoleId ?? 0;
                    groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                    group.Members.Add( groupMember );
                }
            }
        }


        /// <summary>
        /// Adds the groups to mobile related endpoint security.
        /// </summary>
        /// <param name="migrationHelper">The migration helper.</param>
        private static void AddGroupsToMobileRelatedEndpointSecurityGroups( MigrationHelper migrationHelper )
        {
            // 
            // The 'Entity Sets' related endpoints.
            //
            migrationHelper.AddRestAction( "50B248D7-C52A-4698-B4AF-C9DE305394EC",
                "EntitySets",
                "Rock.Rest.Controllers.EntitySetsController" );

            migrationHelper.AddSecurityAuthForRestAction( "50B248D7-C52A-4698-B4AF-C9DE305394EC",
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_MOBILE_APPLICATION_USERS,
                Model.SpecialRole.None,
                "36336448-3021-4A03-B62D-B85CC3FB5FA2" );

            //
            // The 'Followings' related endpoints.
            //
            string followingControllerName = "Followings";
            string followingControllerClass = "Rock.Rest.Controllers.FollowingsController";

            // Follow( int, int, string ) POST.
            migrationHelper.AddRestAction( "1C1F80FE-2567-463E-8BFE-E49ECB8450C7",
                followingControllerName,
                followingControllerClass );

            migrationHelper.AddSecurityAuthForRestAction( "1C1F80FE-2567-463E-8BFE-E49ECB8450C7",
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_MOBILE_APPLICATION_USERS,
                Model.SpecialRole.None,
                "C5D068B8-A3CF-4C8F-BD2B-43BEF433BAC6" );

            // Delete( int, int, string ) DELETE.
            migrationHelper.AddRestAction( "AAB5800B-A429-40D2-A402-D3DE7E15776E",
                followingControllerName,
                followingControllerClass );

            migrationHelper.AddSecurityAuthForRestAction( "AAB5800B-A429-40D2-A402-D3DE7E15776E",
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_MOBILE_APPLICATION_USERS,
                Model.SpecialRole.None,
                "B2F664B7-F4E9-4CDA-A1B3-29B6A713070A" );

            // Follow( Guid, Guid, string ) POST.
            migrationHelper.AddRestAction( "3D09FD68-06F9-4860-9110-60A015004071",
                followingControllerName,
                followingControllerClass );

            migrationHelper.AddSecurityAuthForRestAction( "3D09FD68-06F9-4860-9110-60A015004071",
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_MOBILE_APPLICATION_USERS,
                Model.SpecialRole.None,
                "865D2996-9A90-483C-9BD2-25122355229A" );

            // Delete( Guid, Guid, string ) DELETE.
            migrationHelper.AddRestAction( "A3358897-942F-4332-B060-12718DC87CE6",
                followingControllerName,
                followingControllerClass );

            migrationHelper.AddSecurityAuthForRestAction( "A3358897-942F-4332-B060-12718DC87CE6",
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_MOBILE_APPLICATION_USERS,
                Model.SpecialRole.None,
                "5F878F7E-5E4C-4DA1-ADB3-24F5B871B9EC" );

            //
            // The 'Prayer Requests' related endpoints.
            //
            migrationHelper.AddRestAction( "86675411-D420-4D8B-A421-2BD8C92F4069",
                "PrayerRequests",
                "Rock.Rest.Controllers.PrayerRequestsController" );

            migrationHelper.AddSecurityAuthForRestAction( "86675411-D420-4D8B-A421-2BD8C92F4069",
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_MOBILE_APPLICATION_USERS,
                Model.SpecialRole.None,
                "98f43094-1188-491b-a43e-1993731ff313" );

            migrationHelper.AddRestAction( "9696DB0A-4CCC-4530-BC8D-E4E54A438BFA",
                "PrayerRequests",
                "Rock.Rest.Controllers.PrayerRequestsController" );

            migrationHelper.AddSecurityAuthForRestAction( "9696DB0A-4CCC-4530-BC8D-E4E54A438BFA",
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_MOBILE_APPLICATION_USERS,
                Model.SpecialRole.None,
                "3DE44323-7873-4BA8-BFDB-4BCC0AC25970" );

            // 
            // The 'Impersonation Token' related endpoints.
            //
            migrationHelper.AddRestAction( "A4765A37-043B-49CE-AA9F-C3FFF055176C",
                "People",
                "Rock.Rest.Controllers.PeopleController" );

            migrationHelper.AddSecurityAuthForRestAction( "A4765A37-043B-49CE-AA9F-C3FFF055176C",
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_MOBILE_APPLICATION_USERS,
                Model.SpecialRole.None,
                "96CF823C-D014-4AF3-A17D-64B0C60B6150" );
        }

        /// <summary>
        /// Returns the default settings for creation of the 'Mobile Application Users' security group.
        /// </summary>
        /// <returns>Group.</returns>
        private Group GetDefaultMobileUsersGroup()
        {
            return new Group
            {
                Guid = SystemGuid.Group.GROUP_MOBILE_APPLICATION_USERS.AsGuid(),
                IsSystem = true,
                ParentGroupId = null,
                GroupTypeId = 1,
                CampusId = null,
                Name = "RSR - Mobile Application Users",
                Description = "Group of mobile application people to use for endpoint authorization.",
                IsSecurityRole = true,
                IsActive = true,
                ElevatedSecurityLevel = Utility.Enums.ElevatedSecurityLevel.Extreme,
                IsPublic = true
            };
        }

        /// <summary>
        /// Deletes the job.
        /// </summary>
        private void DeleteJob()
        {
            using ( var rockContext = new RockContext() )
            {
                var jobService = new ServiceJobService( rockContext );
                var job = jobService.Get( GetJobId() );

                if ( job != null )
                {
                    jobService.Delete( job );
                    rockContext.SaveChanges();
                }
            }
        }
    }
}
