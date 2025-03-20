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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 235, "1.17.0" )]
    public class AddChatConfigurationAdminUI : Migration
    {
        /// <summary>
        /// Up methods
        /// </summary>
        public override void Up()
        {
            JPH_AddChatConfigurationPageAndBlocks_Up();
            JPH_UpdateChatGroups_20250320_Up();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            JPH_AddChatConfigurationPageAndBlocks_Down();
            JPH_UpdateChatGroups_20250320_Down();
        }

        /// <summary>
        /// JPH: Add Chat Configuration page and blocks - up.
        /// </summary>
        private void JPH_AddChatConfigurationPageAndBlocks_Up()
        {
            // Add Page 
            //  Internal Name: Chat Configuration
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "199DC522-F4D6-4D82-AF44-3C16EE9D2CDA", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Chat Configuration", "", "03FA9A18-545E-4158-AEA8-2C78AE582C50", "fa fa-comments-o" );

            // Add Page Route
            //   Page:Chat Configuration
            //   Route:communications/chat-configuration
            RockMigrationHelper.AddOrUpdatePageRoute( "03FA9A18-545E-4158-AEA8-2C78AE582C50", "communications/chat-configuration", "05FF6DDF-B980-478A-977C-9A2A1C0786AB" );

            // Add Block 
            //  Block Name: Chat Configuration
            //  Page Name: Chat Configuration
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "03FA9A18-545E-4158-AEA8-2C78AE582C50".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "D5BE6AAE-70A2-4021-93F7-DD66A09B08CB".AsGuid(), "Chat Configuration", "Main", @"", @"", 0, "86E90886-01F5-4DA3-8162-0811A5A38F41" );

            // Add Block 
            //  Block Name: Chat Ban List
            //  Page Name: Chat Configuration
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "03FA9A18-545E-4158-AEA8-2C78AE582C50".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "88B7EFA9-7419-4D05-9F88-38B936E61EDD".AsGuid(), "Chat Ban List", "Main", @"", @"", 1, "2C4B26B2-D16A-4536-B1E5-192C536C4AA8" );

            // Add Block Attribute Value
            //   Block: Chat Ban List
            //   BlockType: Group Member List
            //   Category: Groups
            //   Block Location: Page=Chat Configuration, Site=Rock RMS
            //   Attribute: Person Profile Page
            /*   Attribute Value: 08dbd8a5-2c35-4146-b4a8-0f7652348b25,7e97823a-78a8-4e8e-a337-7a20f2da9e52 */
            RockMigrationHelper.AddBlockAttributeValue( "2C4B26B2-D16A-4536-B1E5-192C536C4AA8", "9E139BB9-D87C-4C9F-A241-DC4620AD340B", @"08dbd8a5-2c35-4146-b4a8-0f7652348b25,7e97823a-78a8-4e8e-a337-7a20f2da9e52" );

            // Add Block Attribute Value
            //   Block: Chat Ban List
            //   BlockType: Group Member List
            //   Category: Groups
            //   Block Location: Page=Chat Configuration, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 3905c63f-4d57-40f0-9721-c60a2f681911,3b7e7c82-c4a3-45c2-ba56-85f6970a9be2 */
            RockMigrationHelper.AddBlockAttributeValue( "2C4B26B2-D16A-4536-B1E5-192C536C4AA8", "E4CCB79C-479F-4BEE-8156-969B2CE05973", @"3905c63f-4d57-40f0-9721-c60a2f681911,3b7e7c82-c4a3-45c2-ba56-85f6970a9be2" );

            // Add Block Attribute Value
            //   Block: Chat Ban List
            //   BlockType: Group Member List
            //   Category: Groups
            //   Block Location: Page=Chat Configuration, Site=Rock RMS
            //   Attribute: Group
            /*   Attribute Value: c9e3a59f-3b5e-43b1-9d97-191ef82d73c4 */
            RockMigrationHelper.AddBlockAttributeValue( "2C4B26B2-D16A-4536-B1E5-192C536C4AA8", "9F2D3674-B780-4CD3-B4AB-3DF3EA21905A", @"c9e3a59f-3b5e-43b1-9d97-191ef82d73c4" );

            // Add Block Attribute Value
            //   Block: Chat Ban List
            //   BlockType: Group Member List
            //   Category: Groups
            //   Block Location: Page=Chat Configuration, Site=Rock RMS
            //   Attribute: Show Date Added
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "2C4B26B2-D16A-4536-B1E5-192C536C4AA8", "F281090E-A05D-4F81-AD80-A3599FB8E2CD", @"True" );

            // Add Block Attribute Value
            //   Block: Chat Ban List
            //   BlockType: Group Member List
            //   Category: Groups
            //   Block Location: Page=Chat Configuration, Site=Rock RMS
            //   Attribute: Show Campus Filter
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "2C4B26B2-D16A-4536-B1E5-192C536C4AA8", "65B9EA6C-D904-4105-8B51-CCA784DDAAFA", @"False" );

            // Add Block Attribute Value
            //   Block: Chat Ban List
            //   BlockType: Group Member List
            //   Category: Groups
            //   Block Location: Page=Chat Configuration, Site=Rock RMS
            //   Attribute: Show First/Last Attendance
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "2C4B26B2-D16A-4536-B1E5-192C536C4AA8", "65834FB0-0AB0-4F73-BE1B-9D2F9FFD2664", @"False" );

            // Add Block Attribute Value
            //   Block: Chat Ban List
            //   BlockType: Group Member List
            //   Category: Groups
            //   Block Location: Page=Chat Configuration, Site=Rock RMS
            //   Attribute: Show Note Column
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "2C4B26B2-D16A-4536-B1E5-192C536C4AA8", "5F54C068-1418-44FA-B215-FBF70072F6A5", @"True" );

            // Add Block Attribute Value
            //   Block: Chat Ban List
            //   BlockType: Group Member List
            //   Category: Groups
            //   Block Location: Page=Chat Configuration, Site=Rock RMS
            //   Attribute: Block Title
            /*   Attribute Value: Chat Ban List */
            RockMigrationHelper.AddBlockAttributeValue( "2C4B26B2-D16A-4536-B1E5-192C536C4AA8", "EB6292BE-96EA-4E08-A8CA-7245ACAA151D", @"Chat Ban List" );

            // Add Block Attribute Value
            //   Block: Chat Ban List
            //   BlockType: Group Member List
            //   Category: Groups
            //   Block Location: Page=Chat Configuration, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "2C4B26B2-D16A-4536-B1E5-192C536C4AA8", "2053BCCE-A4CA-49F0-BC94-4EA0FFA04E91", @"True" );

            // Add Block Attribute Value
            //   Block: Chat Ban List
            //   BlockType: Group Member List
            //   Category: Groups
            //   Block Location: Page=Chat Configuration, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "2C4B26B2-D16A-4536-B1E5-192C536C4AA8", "2924EC20-7A18-4DA6-98C5-6EF3E56A4B93", @"False" );
        }

        /// <summary>
        /// JPH: Add Chat Configuration page and blocks - down.
        /// </summary>
        private void JPH_AddChatConfigurationPageAndBlocks_Down()
        {
            // Remove Block
            //  Name: Chat Ban List, from Page: Chat Configuration, Site: Rock RMS
            //  from Page: Chat Configuration, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "2C4B26B2-D16A-4536-B1E5-192C536C4AA8" );

            // Remove Block
            //  Name: Chat Configuration, from Page: Chat Configuration, Site: Rock RMS
            //  from Page: Chat Configuration, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "86E90886-01F5-4DA3-8162-0811A5A38F41" );

            // Delete Page 
            //  Internal Name: Chat Configuration
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "03FA9A18-545E-4158-AEA8-2C78AE582C50" );

            // The [PageRoute] record will cascade-delete.
        }

        /// <summary>
        /// JPH: Update Chat groups - up.
        /// </summary>
        private void JPH_UpdateChatGroups_20250320_Up()
        {
            // Update previously-seeded "Chat Shared Channel" group type.
            // We're changing the description and making it visible in lists and nav.
            RockMigrationHelper.AddGroupType(
                name: "Chat Shared Channel",
                description: "Used when a chat shared channel is created within Rock.",
                groupTerm: "Channel",
                groupMemberTerm: "Member",
                allowMultipleLocations: false,
                showInGroupList: true,
                showInNavigation: true,
                iconCssClass: "fa fa-comments-o",
                order: 0,
                inheritedGroupTypeGuid: null,
                locationSelectionMode: 0, // None
                groupTypePurposeValueGuid: null,
                guid: Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_SHARED_CHANNEL,
                isSystem: true );

            // Add a new "Chat Shared Channels" group to be the parent for all chat shared channels created within Rock.
            RockMigrationHelper.UpdateGroup(
                parentGroupGuid: null,
                groupTypeGuid: Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_SHARED_CHANNEL,
                name: "Chat Shared Channels",
                description: "Parent group for all chat shared channels.",
                campusGuid: null,
                order: 0,
                guid: Rock.SystemGuid.Group.GROUP_CHAT_SHARED_CHANNELS,
                isSystem: true,
                isSecurityRole: false,
                isActive: true
            );

            // Add a new "Chat Direct Messages" group to be the parent for all chat direct messages created within the
            // external chat system.
            RockMigrationHelper.UpdateGroup(
                parentGroupGuid: null,
                groupTypeGuid: Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_DIRECT_MESSAGE,
                name: "Chat Direct Messages",
                description: "Parent group for all chat direct messages.",
                campusGuid: null,
                order: 0,
                guid: Rock.SystemGuid.Group.GROUP_CHAT_DIRECT_MESSAGES,
                isSystem: true,
                isSecurityRole: false,
                isActive: true
            );

            // Move the previously-created "Chat Ban List" group to live under the newly-created "Chat Shared Channels" parent group.
            RockMigrationHelper.UpdateGroup(
                parentGroupGuid: Rock.SystemGuid.Group.GROUP_CHAT_SHARED_CHANNELS,
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

            // Disable chat for the following system groups, as they should never by synced as a chat group.
            Sql( $@"
UPDATE [Group]
SET [IsChatEnabledOverride] = 0
WHERE [Guid] IN (
    '{Rock.SystemGuid.Group.GROUP_CHAT_BAN_LIST}'
    , '{Rock.SystemGuid.Group.GROUP_CHAT_SHARED_CHANNELS}'
    , '{Rock.SystemGuid.Group.GROUP_CHAT_DIRECT_MESSAGES}'
);" );
        }

        /// <summary>
        /// JPH: Update Chat groups - down.
        /// </summary>
        private void JPH_UpdateChatGroups_20250320_Down()
        {
            // Revert changes to previously-seeded "Chat Shared Channel" group type.
            RockMigrationHelper.AddGroupType(
                name: "Chat Shared Channel",
                description: "Used when a chat channel is created from inside the external chat application.",
                groupTerm: "Channel",
                groupMemberTerm: "Member",
                allowMultipleLocations: false,
                showInGroupList: false,
                showInNavigation: false,
                iconCssClass: "fa fa-comments-o",
                order: 0,
                inheritedGroupTypeGuid: null,
                locationSelectionMode: 0, // None
                groupTypePurposeValueGuid: null,
                guid: Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_SHARED_CHANNEL,
                isSystem: true );

            // Re-orphan the previously-created "Chat Ban List" group.
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

            // Delete "Chat Shared Channels" group.
            RockMigrationHelper.DeleteGroup( Rock.SystemGuid.Group.GROUP_CHAT_SHARED_CHANNELS );

            // Delete "Chat Direct Messages" group.
            RockMigrationHelper.DeleteGroup( Rock.SystemGuid.Group.GROUP_CHAT_DIRECT_MESSAGES );
        }
    }
}
