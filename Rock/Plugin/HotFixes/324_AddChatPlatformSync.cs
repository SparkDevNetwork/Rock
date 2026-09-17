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

using System;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Adds the Rock columns the chat restatement reads, and registers the job
    /// that runs it. A plugin hotfix rather than an Entity Framework migration
    /// because a hand-authored EF migration has no model snapshot and a null
    /// target throws at startup.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 324, "21.0" )]
    public class AddChatPlatformSync : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( $@"
IF COL_LENGTH('dbo.Group', 'ChatChannelFirstEnabledDateTime') IS NULL
BEGIN
    ALTER TABLE [dbo].[Group] ADD [ChatChannelFirstEnabledDateTime] datetime NULL;
END

IF COL_LENGTH('dbo.Group', 'CanViewMembersOverride') IS NULL
BEGIN
    ALTER TABLE [dbo].[Group] ADD [CanViewMembersOverride] bit NULL;
END

IF COL_LENGTH('dbo.Group', 'IsChatSearchIndexedOverride') IS NULL
BEGIN
    ALTER TABLE [dbo].[Group] ADD [IsChatSearchIndexedOverride] bit NULL;
END

IF COL_LENGTH('dbo.GroupType', 'CanViewMembers') IS NULL
BEGIN
    ALTER TABLE [dbo].[GroupType] ADD [CanViewMembers] bit NOT NULL CONSTRAINT [DF_GroupType_CanViewMembers] DEFAULT (0);
END

IF COL_LENGTH('dbo.GroupType', 'IsChatSearchIndexed') IS NULL
BEGIN
    ALTER TABLE [dbo].[GroupType] ADD [IsChatSearchIndexed] bit NOT NULL CONSTRAINT [DF_GroupType_IsChatSearchIndexed] DEFAULT (0);
END

IF COL_LENGTH('dbo.GroupMember', 'ChatBannedUntil') IS NULL
BEGIN
    ALTER TABLE [dbo].[GroupMember] ADD [ChatBannedUntil] datetime NULL;
END

IF NOT EXISTS ( SELECT [Id] FROM [ServiceJob] WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.CHAT_PLATFORM_SYNC}' )
BEGIN
    INSERT INTO [ServiceJob] (
        [IsSystem],
        [IsActive],
        [Name],
        [Description],
        [Class],
        [CronExpression],
        [NotificationStatus],
        [Guid] )
    VALUES (
        0,
        1,
        'Chat Restatement',
        'Sends this church''s people, channels, memberships and badges to the chat platform.',
        'Rock.Jobs.ChatPlatformSyncJob',
        '0 0 0/1 1/1 * ? *',
        1,
        '{Rock.SystemGuid.ServiceJob.CHAT_PLATFORM_SYNC}' );
END
" );

            RockMigrationHelper.AddEntityAttributeIfMissing(
                "Rock.Model.ServiceJob",
                SystemGuid.FieldType.INTEGER,
                "Class",
                "Rock.Jobs.ChatPlatformSyncJob",
                "Command Timeout",
                "Maximum seconds to wait for the projection queries. Leave blank to use 3600.",
                0,
                "3600",
                "C3A91F2B-8D04-4E77-9B1A-6E5C2F8D4A10",
                "CommandTimeout",
                false );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }
    }
}
