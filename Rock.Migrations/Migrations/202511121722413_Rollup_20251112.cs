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
    public partial class Rollup_20251112 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            JMH_CopyWebFormsCommunicationEntryWizardApproveAuthToObsidian_Up();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        #region JMH: Copy "Approve" security from WebForms Communication Entry Wizard to Obsidian 


        /// <summary>
        /// JMH: Copy WebForms CommunicationEntryWizard "Approve" Auth to Obsidian - Up.
        /// </summary>
        private void JMH_CopyWebFormsCommunicationEntryWizardApproveAuthToObsidian_Up()
        {
            // Copy "Approve" security from the Web Forms Communication Entry Wizard block to the Obsidian Communication Entry Wizard block
            // to allow the same individuals to approve communications in the Obsidian block.
            Sql( $@"DECLARE @ObsidianCommunicationPageId AS INT = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '{SystemGuid.Page.NEW_COMMUNICATION_OBSIDIAN}');
DECLARE @ObsidianCommunicationEntryWizardBlockTypeId AS INT = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = '9FFC7A4F-2061-4F30-AF79-D68C85EE9F27');
DECLARE @ObsidianCommunicationEntryWizardBlockId AS INT = (SELECT TOP 1 [Id] FROM [Block] B WHERE B.[BlockTypeId] = @ObsidianCommunicationEntryWizardBlockTypeId AND B.[PageId] = @ObsidianCommunicationPageId);

DECLARE @WebFormsCommunicationPageId AS INT = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '{SystemGuid.Page.NEW_COMMUNICATION}');
DECLARE @WebFormsCommunicationEntryWizardBlockTypeId AS INT = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = '{SystemGuid.BlockType.COMMUNICATION_ENTRY_WIZARD}');
DECLARE @WebFormsCommunicationEntryWizardBlockId AS INT = (SELECT TOP 1 [Id] FROM [Block] B WHERE B.[BlockTypeId] = @WebFormsCommunicationEntryWizardBlockTypeId AND B.[PageId] = @WebFormsCommunicationPageId);

DECLARE @BlockEntityTypeId AS INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '{SystemGuid.EntityType.BLOCK}');
DECLARE @AuthActionToCopy AS NVARCHAR(50) = N'{Security.Authorization.APPROVE}';
DECLARE @SourceEntityId AS INT = @WebFormsCommunicationEntryWizardBlockId;
DECLARE @DestinationEntityId AS INT = @ObsidianCommunicationEntryWizardBlockId;

IF (@SourceEntityId <> @DestinationEntityId AND @SourceEntityId > 0 AND @DestinationEntityId > 0) BEGIN
    INSERT INTO [Auth] (
        [EntityTypeId]
        , [EntityId]
        , [Order]
        , [Action]
        , [AllowOrDeny]
        , [SpecialRole]
        , [GroupId], [Guid], [PersonAliasId])
    SELECT 
          a1.[EntityTypeId]
        , @DestinationEntityId AS [EntityId]
        , a1.[Order]
        , a1.[Action]
        , a1.[AllowOrDeny]
        , a1.[SpecialRole]
        , a1.[GroupId]
        , NEWID() AS [Guid]
        , a1.[PersonAliasId]
    FROM [Auth] a1
    WHERE a1.[EntityTypeId] = @BlockEntityTypeId
        AND a1.[EntityId] = @SourceEntityId
        AND a1.[Action] = @AuthActionToCopy
        AND NOT EXISTS (
            SELECT 1
            FROM [Auth] a2
            WHERE a2.[EntityTypeId] = a1.[EntityTypeId]
                AND ISNULL(a2.[EntityId], -1) = ISNULL(@DestinationEntityId, -1)
                AND a2.[Order] = a1.[Order]
                AND a2.[Action] = a1.[Action]
                AND a2.[AllowOrDeny] = a1.[AllowOrDeny]
                AND a2.[SpecialRole] = a1.[SpecialRole]
                AND ISNULL(a2.[GroupId], -1) = ISNULL(a1.[GroupId], -1)
                AND ISNULL(a2.[PersonAliasId], -1) = ISNULL(a1.[PersonAliasId], -1))
END" );
        }

        #endregion
    }
}
