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

    /// <summary>
    ///
    /// </summary>
    public partial class AddChatWebhookSupport : Rock.Migrations.RockMigration
    {
        const string ChatControllerWebhookActionGuid = "7D1B2D15-E2F5-4159-AD84-AE7D9262750D";
        const string ChatControllerWebhookActionSecurityAuthGuid = "4C229859-CA28-472E-806D-95FCF452F1A1";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateRockChatGroupsUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UpdateRockChatGroupsDown();
        }

        /// <summary>
        /// JPH: Update Rock chat groups - up.
        /// </summary>
        private void UpdateRockChatGroupsUp()
        {
            // Supplement previously-seeded group types.
            Sql( $@"
UPDATE [GroupType]
SET [IconCssClass] = 'fa fa-comments-o'
WHERE [Guid] IN (
    '{Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_DIRECT_MESSAGE}'
    , '{Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_SHARED_CHANNEL}'
);" );

            // Add a new "Chat Ban List" group to hold people who are globally-banned.
            RockMigrationHelper.UpdateGroup(
                parentGroupGuid: null,
                groupTypeGuid: Rock.SystemGuid.GroupType.GROUPTYPE_APPLICATION_GROUP,
                name: "Chat Ban List",
                description: "Used to identify individuals who are globally banned from chat. Anyone who belongs to this group will be unable to access Rock chat, even if they belong to chat-enabled groups.",
                campusGuid: null,
                order: 0,
                guid: Rock.SystemGuid.Group.GROUP_CHAT_BAN_LIST,
                isSystem: true,
                isSecurityRole: false,
                isActive: true
            );
        }

        /// <summary>
        /// JPH: Update Rock chat groups - down.
        /// </summary>
        private void UpdateRockChatGroupsDown()
        {
            // Revert group type changes.
            Sql( $@"
UPDATE [GroupType]
SET [IconCssClass] = ''
WHERE [Guid] IN (
    '{Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_DIRECT_MESSAGE}'
    , '{Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_SHARED_CHANNEL}'
);" );

            // Delete "Chat Ban List" group.
            RockMigrationHelper.DeleteGroup( Rock.SystemGuid.Group.GROUP_CHAT_BAN_LIST );
        }
    }
}
