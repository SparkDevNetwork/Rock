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
using Rock.Communication.Chat;

namespace Rock.Migrations
{

    /// <summary>
    ///
    /// </summary>
    public partial class AddMobileChatBackend : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddMobileChatBackendSchemaChangesUp();
            AddMobileChatBackendSystemSettingsUp();
            AddMobileChatBackendSecurityRolesUp();
            AddMobileChatBackendGroupTypesUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddMobileChatBackendGroupTypesDown();
            AddMobileChatBackendSecurityRolesDown();
            AddMobileChatBackendSystemSettingsDown();
            AddMobileChatBackendSchemaChangesDown();
        }

        #region Schema Changes

        /// <summary>
        /// JPH: Add mobile chat backend schema changes - up.
        /// </summary>`
        private void AddMobileChatBackendSchemaChangesUp()
        {
            AddColumn( "dbo.Person", "IsChatProfilePublic", c => c.Boolean() );
            AddColumn( "dbo.Person", "IsChatOpenDirectMessageAllowed", c => c.Boolean() );

            AddColumn( "dbo.Group", "IsChatEnabledOverride", c => c.Boolean() );
            AddColumn( "dbo.Group", "IsLeavingChatChannelAllowedOverride", c => c.Boolean() );
            AddColumn( "dbo.Group", "IsChatChannelPublicOverride", c => c.Boolean() );
            AddColumn( "dbo.Group", "IsChatChannelAlwaysShownOverride", c => c.Boolean() );
            AddColumn( "dbo.Group", "ChatChannelKey", c => c.String( maxLength: 100 ) );

            AddColumn( "dbo.GroupType", "IsChatAllowed", c => c.Boolean( nullable: false, defaultValue: false ) );
            AddColumn( "dbo.GroupType", "IsChatEnabledForAllGroups", c => c.Boolean( nullable: false, defaultValue: false ) );
            AddColumn( "dbo.GroupType", "IsLeavingChatChannelAllowed", c => c.Boolean( nullable: false, defaultValue: false ) );
            AddColumn( "dbo.GroupType", "IsChatChannelPublic", c => c.Boolean( nullable: false, defaultValue: false ) );
            AddColumn( "dbo.GroupType", "IsChatChannelAlwaysShown", c => c.Boolean( nullable: false, defaultValue: false ) );

            AddColumn( "dbo.GroupTypeRole", "ChatRole", c => c.Int( nullable: false, defaultValue: 0 ) );

            AddColumn( "dbo.GroupMember", "IsChatMuted", c => c.Boolean( nullable: false, defaultValue: false ) );
            AddColumn( "dbo.GroupMember", "IsChatBanned", c => c.Boolean( nullable: false, defaultValue: false ) );
        }

        /// <summary>
        /// JPH: Add mobile chat backend schema changes - down.
        /// </summary>
        private void AddMobileChatBackendSchemaChangesDown()
        {
            DropColumn( "dbo.GroupMember", "IsChatBanned" );
            DropColumn( "dbo.GroupMember", "IsChatMuted" );

            DropColumn( "dbo.GroupTypeRole", "ChatRole" );

            DropColumn( "dbo.GroupType", "IsChatChannelAlwaysShown" );
            DropColumn( "dbo.GroupType", "IsChatChannelPublic" );
            DropColumn( "dbo.GroupType", "IsLeavingChatChannelAllowed" );
            DropColumn( "dbo.GroupType", "IsChatEnabledForAllGroups" );
            DropColumn( "dbo.GroupType", "IsChatAllowed" );

            DropColumn( "dbo.Group", "ChatChannelKey" );
            DropColumn( "dbo.Group", "IsChatChannelAlwaysShownOverride" );
            DropColumn( "dbo.Group", "IsChatChannelPublicOverride" );
            DropColumn( "dbo.Group", "IsLeavingChatChannelAllowedOverride" );
            DropColumn( "dbo.Group", "IsChatEnabledOverride" );

            DropColumn( "dbo.Person", "IsChatOpenDirectMessageAllowed" );
            DropColumn( "dbo.Person", "IsChatProfilePublic" );
        }

        #endregion Schema Changes

        #region System Settings

        /// <summary>
        /// JPH: Add mobile chat backend system settings - up.
        /// </summary>
        private void AddMobileChatBackendSystemSettingsUp()
        {
            var chatConfiguration = new ChatConfiguration();

            RockMigrationHelper.UpdateSystemSetting( Rock.SystemKey.SystemSetting.CHAT_CONFIGURATION, chatConfiguration.ToJson() );
        }

        /// <summary>
        /// JPH: Add mobile chat backend system settings - down.
        /// </summary>
        private void AddMobileChatBackendSystemSettingsDown()
        {
            Sql( $@"
DELETE FROM [Attribute]
WHERE [EntityTypeId] IS NULL
    AND [EntityTypeQualifierColumn] = 'SystemSetting'
    AND [Key] = '{Rock.SystemKey.SystemSetting.CHAT_CONFIGURATION}';" );
        }

        #endregion System Settings

        #region Security Roles

        /// <summary>
        /// JPH: Add mobile chat backend security roles - up.
        /// </summary>
        private void AddMobileChatBackendSecurityRolesUp()
        {
            RockMigrationHelper.AddSecurityRoleGroup( "APP - Chat Administrators", "Group of individuals who can participate in and moderate all chat channels, regardless of channel membership.", Rock.SystemGuid.Group.GROUP_CHAT_ADMINISTRATORS );
        }

        /// <summary>
        /// JPH: Add mobile chat backend security roles - down.
        /// </summary>
        private void AddMobileChatBackendSecurityRolesDown()
        {
            RockMigrationHelper.DeleteSecurityRoleGroup( Rock.SystemGuid.Group.GROUP_CHAT_ADMINISTRATORS );
        }

        #endregion Security Roles

        #region Group Types

        /// <summary>
        /// JPH: Add mobile chat backend group types - up.
        /// </summary>
        private void AddMobileChatBackendGroupTypesUp()
        {
            RockMigrationHelper.AddGroupType(
                name: "Chat Direct Message",
                description: "Used when a DM is created from inside the external chat application.",
                groupTerm: "Channel",
                groupMemberTerm: "Member",
                allowMultipleLocations: false,
                showInGroupList: false,
                showInNavigation: false,
                iconCssClass: null,
                order: 0,
                inheritedGroupTypeGuid: null,
                locationSelectionMode: 0, // None
                groupTypePurposeValueGuid: null,
                guid: Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_DIRECT_MESSAGE,
                isSystem: true );

            Sql( $@"
UPDATE [GroupType]
SET [IsChatAllowed] = 1
    , [IsChatEnabledForAllGroups] = 1
WHERE [Guid] = '{Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_DIRECT_MESSAGE}';" );

            RockMigrationHelper.AddGroupType(
                name: "Chat Shared Channel",
                description: "Used when a chat channel is created from inside the external chat application.",
                groupTerm: "Channel",
                groupMemberTerm: "Member",
                allowMultipleLocations: false,
                showInGroupList: false,
                showInNavigation: false,
                iconCssClass: null,
                order: 0,
                inheritedGroupTypeGuid: null,
                locationSelectionMode: 0, // None
                groupTypePurposeValueGuid: null,
                guid: Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_SHARED_CHANNEL,
                isSystem: true );

            Sql( $@"
UPDATE [GroupType]
SET [IsChatAllowed] = 1
    , [IsChatEnabledForAllGroups] = 1
    , [IsLeavingChatChannelAllowed] = 1
    , [IsChatChannelPublic] = 1
WHERE [Guid] = '{Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_SHARED_CHANNEL}';" );
        }

        /// <summary>
        /// JPH: Add mobile chat backend group types - down.
        /// </summary>
        private void AddMobileChatBackendGroupTypesDown()
        {
            RockMigrationHelper.DeleteGroupType( Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_DIRECT_MESSAGE );
            RockMigrationHelper.DeleteGroupType( Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_SHARED_CHANNEL );
        }

        #endregion Group Types
    }
}
