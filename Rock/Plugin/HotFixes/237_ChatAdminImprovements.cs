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
    [MigrationNumber( 237, "1.17.0" )]
    public class ChatAdminImprovements : Migration
    {
        /// <summary>
        /// Up methods
        /// </summary>
        public override void Up()
        {
            JPH_MoveChatBanListGroup_20250321_Up();
            JPH_SeedChatSyncJob_20250321_Up();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            JPH_MoveChatBanListGroup_20250321_Down();
            JPH_SeedChatSyncJob_20250321_Down();
        }

        /// <summary>
        /// JPH: Move "Chat Ban List" group - up.
        /// </summary>
        private void JPH_MoveChatBanListGroup_20250321_Up()
        {
            // Add a new "Hidden Application Group" group type.
            RockMigrationHelper.AddGroupType(
                name: "Hidden Application Group",
                description: "A generic group type used by specific features in Rock. Groups of this type are not shown in nav or in lists and have a single 'Member' role. For example, the Chat Ban List group is of this type.",
                groupTerm: "Group",
                groupMemberTerm: "Member",
                allowMultipleLocations: false,
                showInGroupList: false,
                showInNavigation: false,
                iconCssClass: "fa fa-gears",
                order: 0,
                inheritedGroupTypeGuid: null,
                locationSelectionMode: 0, // None
                groupTypePurposeValueGuid: null,
                guid: Rock.SystemGuid.GroupType.GROUPTYPE_HIDDEN_APPLICATION_GROUP,
                isSystem: true );

            // Add default "Member" group type role to the "Hidden Application Group" group type.
            RockMigrationHelper.UpdateGroupTypeRole( Rock.SystemGuid.GroupType.GROUPTYPE_HIDDEN_APPLICATION_GROUP, "Member", "Member of a group", order: 0, maxCount: null, minCount: null, guid: Rock.SystemGuid.GroupRole.GROUP_ROLE_HIDDEN_APPLICATION_GROUP_MEMBER, isSystem: true, isLeader: false, isDefaultGroupTypeRole: true );

            // Change any existing, "Chat Ban List" members to have this new role.
            Sql( $@"
DECLARE @OldGroupRoleId INT = (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = '{Rock.SystemGuid.GroupRole.GROUP_ROLE_APPLICATION_GROUP_MEMBER}');
DECLARE @NewGroupRoleId INT = (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = '{Rock.SystemGuid.GroupRole.GROUP_ROLE_HIDDEN_APPLICATION_GROUP_MEMBER}');

UPDATE [GroupMember]
SET [GroupRoleId] = @NewGroupRoleId
WHERE [GroupRoleId] = @OldGroupRoleId;" );

            // Change the previously-created "Chat Ban List" group be of type "Hidden Application Group".
            // Also clear out its parent group (previously "Chat Shared Channels").
            RockMigrationHelper.UpdateGroup(
                parentGroupGuid: null,
                groupTypeGuid: Rock.SystemGuid.GroupType.GROUPTYPE_HIDDEN_APPLICATION_GROUP,
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
        /// JPH: Move "Chat Ban List" group - down.
        /// </summary>
        private void JPH_MoveChatBanListGroup_20250321_Down()
        {
            // Revert changes to existing, "Chat Ban List" members.
            Sql( $@"
DECLARE @OldGroupRoleId INT = (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = '{Rock.SystemGuid.GroupRole.GROUP_ROLE_HIDDEN_APPLICATION_GROUP_MEMBER}');
DECLARE @NewGroupRoleId INT = (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = '{Rock.SystemGuid.GroupRole.GROUP_ROLE_APPLICATION_GROUP_MEMBER}');

UPDATE [GroupMember]
SET [GroupRoleId] = @NewGroupRoleId
WHERE [GroupRoleId] = @OldGroupRoleId;" );

            // Revert changes made to the "Chat Ban List" group.
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

            // Delete the "Hidden Application Group" group type and role.
            RockMigrationHelper.DeleteGroupTypeRole( Rock.SystemGuid.GroupRole.GROUP_ROLE_HIDDEN_APPLICATION_GROUP_MEMBER );
            RockMigrationHelper.DeleteGroupType( Rock.SystemGuid.GroupType.GROUPTYPE_HIDDEN_APPLICATION_GROUP );
        }

        /// <summary>
        /// JPH: Seed an instance of the "Chat Sync" job - up.
        /// </summary>
        private void JPH_SeedChatSyncJob_20250321_Up()
        {
            Sql( $@"
DECLARE @Now DATETIME = (SELECT GETDATE());

IF EXISTS
(
    SELECT [Id]
    FROM [ServiceJob]
    WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.CHAT_SYNC_JOB}'
)
BEGIN
    UPDATE [ServiceJob]
    SET [Name] = 'Chat Sync'
        , [Description] = 'Job that performs synchronization tasks between Rock and the external chat system.'
        , [Class] = 'Rock.Jobs.ChatSync'
        , [CronExpression] = '0 0 5 1/1 * ? *'
        , [ModifiedDateTime] = @Now
    WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.CHAT_SYNC_JOB}';
END
ELSE
BEGIN
    INSERT INTO [ServiceJob]
    (
        [IsSystem]
        , [IsActive]
        , [Name]
        , [Description]
        , [Class]
        , [CronExpression]
        , [NotificationStatus]
        , [Guid]
        , [CreatedDateTime]
        , [ModifiedDateTime]
        , [HistoryCount]
    )
    VALUES
    (
        0
        , 1
        , 'Chat Sync'
        , 'Job that performs synchronization tasks between Rock and the external chat system.'
        , 'Rock.Jobs.ChatSync'
        , '0 0 5 1/1 * ? *'
        , 1
        , '{Rock.SystemGuid.ServiceJob.CHAT_SYNC_JOB}'
        , @Now
        , @Now
        , 500
    );
END" );

            RockMigrationHelper.AddEntityAttributeIfMissing(
                entityTypeName: "Rock.Model.ServiceJob",
                fieldTypeGuid: SystemGuid.FieldType.BOOLEAN,
                entityTypeQualifierColumn: "Class",
                entityTypeQualifierValue: "Rock.Jobs.ChatSync",
                name: "Synchronize Data",
                description: "Determines if data synchronization should be performed between Rock and the external chat system. If enabled, this will ensure that all chat-related data in Rock is in sync with the corresponding data in the external chat system.",
                order: 1,
                defaultValue: true.ToString(),
                guid: "B63C2896-C0EE-4396-9A3E-24F6F0E3C1AB",
                key: "SynchronizeData",
                isRequired: false
            );

            RockMigrationHelper.AddEntityAttributeIfMissing(
                entityTypeName: "Rock.Model.ServiceJob",
                fieldTypeGuid: SystemGuid.FieldType.BOOLEAN,
                entityTypeQualifierColumn: "Class",
                entityTypeQualifierValue: "Rock.Jobs.ChatSync",
                name: "Create Interactions",
                description: "Determines if chat interaction records should be created. If enabled, this will create a daily interaction for each person who posted one or more messages within a given chat channel, and will include how many messages that person posted within that channel for that day. Will only look back up to 5 days when determining the interactions to create.",
                order: 2,
                defaultValue: true.ToString(),
                guid: "716A7180-26BA-4C99-9BE8-E66B65A60458",
                key: "CreateInteractions",
                isRequired: false
            );

            RockMigrationHelper.AddEntityAttributeIfMissing(
                entityTypeName: "Rock.Model.ServiceJob",
                fieldTypeGuid: SystemGuid.FieldType.BOOLEAN,
                entityTypeQualifierColumn: "Class",
                entityTypeQualifierValue: "Rock.Jobs.ChatSync",
                name: "Delete Merged Chat Individuals",
                description: "Determines if non-prevailing, merged chat individuals should be deleted in the external chat system. If enabled, when two people in Rock have been merged, and both had an associated chat individual, the non-prevailing chat individual will be deleted from the external chat system to ensure other people can send future messages to only the prevailing chat individual.",
                order: 3,
                defaultValue: true.ToString(),
                guid: "E91B3F20-BA58-4710-8E50-9F3A28B7DC8F",
                key: "DeleteMergedChatUsers",
                isRequired: false
            );

            RockMigrationHelper.AddEntityAttributeIfMissing(
                entityTypeName: "Rock.Model.ServiceJob",
                fieldTypeGuid: SystemGuid.FieldType.BOOLEAN,
                entityTypeQualifierColumn: "Class",
                entityTypeQualifierValue: "Rock.Jobs.ChatSync",
                name: "Enforce Default Grants Per Role",
                description: "This is an experimental setting that might be removed in a future version of Rock. If enabled, will overwrite all permission grants (per role) in the external chat system with default values. This will be helpful during the early stages of the Rock Chat feature, as we learn the best way to fine-tune these permissions.",
                order: 4,
                defaultValue: true.ToString(),
                guid: "91FC0EF3-2D63-40D2-9FD0-31E5D8605319",
                key: "EnforceDefaultGrantsPerRole",
                isRequired: false
            );

            RockMigrationHelper.AddEntityAttributeIfMissing(
                entityTypeName: "Rock.Model.ServiceJob",
                fieldTypeGuid: SystemGuid.FieldType.BOOLEAN,
                entityTypeQualifierColumn: "Class",
                entityTypeQualifierValue: "Rock.Jobs.ChatSync",
                name: "Enforce Default Sync Settings",
                description: "This is an experimental setting that might be removed in a future version of Rock. If enabled, will overwrite all settings (e.g. channel type and channel settings) in the external chat system with default values. This will be helpful during the early stages of the Rock Chat feature, as we learn the best way to fine-tune these settings.",
                order: 5,
                defaultValue: true.ToString(),
                guid: "1D4AA3DE-06C1-44DE-97FC-BC04F1AEC800",
                key: "EnforceDefaultSyncSettings",
                isRequired: false
            );

            RockMigrationHelper.AddEntityAttributeIfMissing(
                entityTypeName: "Rock.Model.ServiceJob",
                fieldTypeGuid: SystemGuid.FieldType.BOOLEAN,
                entityTypeQualifierColumn: "Class",
                entityTypeQualifierValue: "Rock.Jobs.ChatSync",
                name: "Command Timeout",
                description: "Maximum amount of time (in seconds) to wait for the sql operations to complete. Leave blank to use the default for this job (3600). Note, some operations could take several minutes, so you might want to set it at 3600 (60 minutes) or higher.",
                order: 6,
                defaultValue: 3600.ToString(),
                guid: "6DA10AE8-CF9B-4393-824A-F45AF96DDCB0",
                key: "CommandTimeout",
                isRequired: false
            );
        }

        /// <summary>
        /// JPH: Seed an instance of the "Chat Sync" job - down.
        /// </summary>
        private void JPH_SeedChatSyncJob_20250321_Down()
        {
            RockMigrationHelper.DeleteAttribute( "B63C2896-C0EE-4396-9A3E-24F6F0E3C1AB" );
            RockMigrationHelper.DeleteAttribute( "716A7180-26BA-4C99-9BE8-E66B65A60458" );
            RockMigrationHelper.DeleteAttribute( "E91B3F20-BA58-4710-8E50-9F3A28B7DC8F" );
            RockMigrationHelper.DeleteAttribute( "91FC0EF3-2D63-40D2-9FD0-31E5D8605319" );
            RockMigrationHelper.DeleteAttribute( "1D4AA3DE-06C1-44DE-97FC-BC04F1AEC800" );
            RockMigrationHelper.DeleteAttribute( "6DA10AE8-CF9B-4393-824A-F45AF96DDCB0" );

            Sql( $@"
DELETE FROM [ServiceJob]
WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.CHAT_SYNC_JOB}';" );
        }
    }
}
