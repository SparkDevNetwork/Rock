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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    ///
    /// </summary>
    public partial class AddDefaultChatGroupTypeRoles : Rock.Migrations.RockMigration
    {
        const string DirectMessageAdminGuid = "0F85C980-1A45-4FE2-BD2B-3E3A051FFDDF";
        const string DirectMessageModeratorGuid = "78CA32C4-F34A-4056-AE42-D49BCD39B027";
        const string DirectMessageMemberGuid = "EC36D21C-FF61-4A4E-8164-5A1145ABBD85";

        const string SharedChannelAdminGuid = "A04C6E1D-89EA-48D1-856F-E91F9AB6ACF3";
        const string SharedChannelModeratorGuid = "E0BC063F-ECAA-4C4F-9A87-A4752030AED7";
        const string SharedChannelMemberGuid = "E133E458-9785-4C59-B844-35E0C3B686D9";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateGroupTypeRole( Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_DIRECT_MESSAGE, "Admin", "Indicates the person is a chat channel admin.", order: 0, maxCount: null, minCount: null, guid: DirectMessageAdminGuid, isSystem: true, isLeader: true, isDefaultGroupTypeRole: false );
            RockMigrationHelper.UpdateGroupTypeRole( Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_DIRECT_MESSAGE, "Moderator", "Indicates the person is a chat channel moderator.", order: 1, maxCount: null, minCount: null, guid: DirectMessageModeratorGuid, isSystem: true, isLeader: false, isDefaultGroupTypeRole: false );
            RockMigrationHelper.UpdateGroupTypeRole( Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_DIRECT_MESSAGE, "Member", "Indicates the person is a chat channel member.", order: 2, maxCount: null, minCount: null, guid: DirectMessageMemberGuid, isSystem: true, isLeader: false, isDefaultGroupTypeRole: true );

            RockMigrationHelper.UpdateGroupTypeRole( Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_SHARED_CHANNEL, "Admin", "Indicates the person is a chat channel admin.", order: 0, maxCount: null, minCount: null, guid: SharedChannelAdminGuid, isSystem: true, isLeader: true, isDefaultGroupTypeRole: false );
            RockMigrationHelper.UpdateGroupTypeRole( Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_SHARED_CHANNEL, "Moderator", "Indicates the person is a chat channel moderator.", order: 1, maxCount: null, minCount: null, guid: SharedChannelModeratorGuid, isSystem: true, isLeader: false, isDefaultGroupTypeRole: false );
            RockMigrationHelper.UpdateGroupTypeRole( Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_SHARED_CHANNEL, "Member", "Indicates the person is a chat channel member.", order: 2, maxCount: null, minCount: null, guid: SharedChannelMemberGuid, isSystem: true, isLeader: false, isDefaultGroupTypeRole: true );

            Sql( $@"
UPDATE [GroupTypeRole]
SET [CanView] = 1
    , [CanEdit] = 1
    , [CanManageMembers] = 1
    , [ChatRole] = 2
WHERE [Guid] IN (
    '{DirectMessageAdminGuid}'
    , '{SharedChannelAdminGuid}'
);

UPDATE [GroupTypeRole]
SET [CanView] = 1
    , [ChatRole] = 1
WHERE [Guid] IN (
    '{DirectMessageModeratorGuid}'
    , '{SharedChannelModeratorGuid}'
);

UPDATE [GroupTypeRole]
SET [CanView] = 1
WHERE [Guid] IN (
    '{DirectMessageMemberGuid}'
    , '{SharedChannelMemberGuid}'
);" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteGroupTypeRole( DirectMessageAdminGuid );
            RockMigrationHelper.DeleteGroupTypeRole( DirectMessageModeratorGuid );
            RockMigrationHelper.DeleteGroupTypeRole( DirectMessageMemberGuid );

            RockMigrationHelper.DeleteGroupTypeRole( SharedChannelAdminGuid );
            RockMigrationHelper.DeleteGroupTypeRole( SharedChannelModeratorGuid );
            RockMigrationHelper.DeleteGroupTypeRole( SharedChannelMemberGuid );
        }
    }
}
