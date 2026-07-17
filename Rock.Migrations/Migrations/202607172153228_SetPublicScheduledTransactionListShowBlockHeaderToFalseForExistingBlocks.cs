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
    public partial class SetPublicScheduledTransactionListShowBlockHeaderToFalseForExistingBlocks : Rock.Migrations.RockMigration
    {

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Pre-register the ShowBlockHeader attribute against the chopped
            // BlockType (081FF29F-... is the former WebForms Scheduled
            // Transaction List Liquid GUID which now hosts the Obsidian
            // Public Scheduled Transaction List block). Idempotent: subsequent
            // startups match this row by Key + BlockTypeId qualifier and
            // refresh in place rather than inserting a duplicate.
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                blockTypeGuid: "081FF29F-0A9F-4EC3-95AD-708FA0E6132D",
                fieldTypeGuid: "1EDAFDED-DFE6-4334-B019-6EECBA89E05A",
                name: "Show Block Header",
                key: "ShowBlockHeader",
                abbreviatedName: "Show Block Header",
                description: "When enabled, displays a title and description at the top of the block.",
                order: 0,
                defaultValue: "True",
                guid: "08AEB9D3-8A70-45F2-BA15-1CD6AF063A14" );

            // Rename the BlockType and any Block instances whose Name still
            // exactly matches the WebForms-era default. The block-registration
            // reflection pass will re-sync BlockType.Name from the C# [DisplayName]
            // on the next startup regardless, but Block.Name (the per-page
            // block label admins see in the page-admin UI) is user-owned data
            // that Rock does not auto-update. Guarding on the exact old string
            // preserves any admin who intentionally renamed a block instance.
            //
            // Also force ShowBlockHeader = False on every pre-existing block
            // instance of the chopped BlockType. The subquery-based guard
            // ensures we only INSERT for blocks that do not already have
            // an AttributeValue row (idempotent), and blocks added after
            // this migration runs continue to inherit the C# default of
            // True (i.e., new pages that get the block show the header).
            Sql( @"
DECLARE @AttributeId INT = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '08AEB9D3-8A70-45F2-BA15-1CD6AF063A14')
DECLARE @BlockTypeId INT = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = '081FF29F-0A9F-4EC3-95AD-708FA0E6132D')
DECLARE @BlockEntityTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block')

DECLARE @TransferButtonTextAttributeId INT = (SELECT TOP 1 [Id] FROM [Attribute]
    WHERE [EntityTypeId] = @BlockEntityTypeId
      AND [EntityTypeQualifierColumn] = 'BlockTypeId'
      AND [EntityTypeQualifierValue] = @BlockTypeId
      AND [Key] = 'TransferButtonText')

IF @TransferButtonTextAttributeId IS NOT NULL
BEGIN
    -- Migrate per-instance TransferButtonText overrides from the old default
    -- (""Transfer"") to the new one (""Transfer Gateway""). Only touches rows
    -- that still exactly equal the old default so any admin-customized label
    -- is preserved. Block instances that had NO AttributeValue row are
    -- unaffected here — they inherit from Attribute.DefaultValue, which the
    -- block-registration reflection pass resyncs to the new C# default on
    -- startup, so those automatically pick up ""Transfer Gateway"".
    UPDATE [AttributeValue]
    SET [Value] = 'Transfer Gateway',
        [IsPersistedValueDirty] = 1
    WHERE [AttributeId] = @TransferButtonTextAttributeId
      AND [Value] = 'Transfer'
END

IF @BlockTypeId IS NOT NULL
BEGIN
    -- Rename the BlockType row if it still carries the WebForms-era default name.
    UPDATE [BlockType]
    SET [Name] = 'Public Scheduled Transaction List'
    WHERE [Id] = @BlockTypeId
      AND [Name] = 'Scheduled Transaction List Liquid'

    -- Rename any Block instances that still carry the WebForms-era default name.
    UPDATE [Block]
    SET [Name] = 'Public Scheduled Transaction List'
    WHERE [BlockTypeId] = @BlockTypeId
      AND [Name] = 'Scheduled Transaction List Liquid'
END

IF @AttributeId IS NOT NULL AND @BlockTypeId IS NOT NULL
BEGIN
    INSERT INTO [AttributeValue]
        ([IsSystem], [AttributeId], [EntityId], [Value], [Guid], [IsPersistedValueDirty])
        SELECT
            0,
            @AttributeId,
            [B].[Id],
            'False',
            NEWID(),
            1
        FROM [Block] AS [B]
        WHERE [B].[BlockTypeId] = @BlockTypeId
        AND [B].[Id] NOT IN (SELECT [EntityId] FROM [AttributeValue] WHERE [AttributeId] = @AttributeId)
END
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
