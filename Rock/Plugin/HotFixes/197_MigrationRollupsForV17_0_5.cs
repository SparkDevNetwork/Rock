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

using Rock.SystemGuid;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 197, "1.16.4" )]
    public class MigrationRollupsForV17_0_5: Migration
    {
        /// <summary>
        /// PA: Added Missing Block Attributes to ignore list Chop Communication Recipient List Block Job.
        /// </summary>
        public override void Up()
        {
            Sql( $@"
DECLARE @ServiceJobEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.ServiceJob' )
-- Get the Attribute Id For Service Job by the key
DECLARE @AttributeId int
SET @AttributeId = (
    SELECT [Id]
    FROM [Attribute]
    WHERE [EntityTypeId] = @ServiceJobEntityTypeId
        AND [EntityTypeQualifierColumn] = 'Class'
        AND [EntityTypeQualifierValue] = 'Rock.Jobs.PostUpdateDataMigrationsReplaceWebFormsBlocksWithObsidianBlocks'
        AND [Key] = 'BlockAttributeKeysToIgnore' );

DECLARE @ChopCommunicationRecipientListBlockServiceJobId int = (SELECT [Id] FROM [ServiceJob] WHERE [Guid] = '{ServiceJob.DATA_MIGRATIONS_170_REMOVE_COMMUNICATION_RECIPIENT_LIST_BLOCK}');
IF NOT EXISTS ( SELECT * FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @ChopCommunicationRecipientListBlockServiceJobId ) AND @ChopCommunicationRecipientListBlockServiceJobId IS NOT NULL
BEGIN
INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
VALUES (1, @AttributeId, @ChopCommunicationRecipientListBlockServiceJobId, N'EBEA5996-5695-4A42-A21C-29E11E711BE8^ContextEntityType,DetailPage,core.CustomGridColumnsConfig,core.CustomActionsConfigs,core.EnableDefaultWorkflowLauncher,core.CustomGridEnableStickyHeaders', NEWID());
END" );
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
        }
    }
}
