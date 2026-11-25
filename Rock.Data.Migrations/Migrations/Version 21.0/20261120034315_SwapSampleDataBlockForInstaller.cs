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
namespace Rock.Data.Migrations;

/*
    11/19/2026 - CLAUDE

    Replaces the legacy "Rock Solid Church Sample Data" block on the Sample Data
    page with the new Obsidian Sample Data Installer block. The page itself, the
    legacy block type, and the legacy block code all stay in place, because other
    pages may still use the legacy block type.

    1. The installer's EntityType and BlockType rows are registered here. Startup
       block type registration runs AFTER migrations, so on an upgrade the BlockType
       row would not exist yet and the block insert would have no block type to use.
       The block type is entity based, so it uses AddOrUpdateEntityBlockType.

    2. The legacy block type row (A42E0031-B2B9-403A-845B-9C968D7716A6) is still a
       path based row (~/Blocks/Examples/SampleData.ascx, no EntityTypeId) in a
       database built only from migrations. Startup links it to the Obsidian
       Rock.Blocks.Example.SampleData block by Guid. This migration never touches
       that row. Never use UpdateBlockType / UpdateBlockTypeByGuid on it or on the
       installer block type, because the path based DELETE in those methods can
       wipe out entity based block types.

    3. The blocks are added and removed with the standard RockMigrationHelper
       methods, per the established pattern. Known edge case: if an administrator
       deleted the Sample Data page (Id 270 in a fresh install), AddBlock would
       insert a block with no page. That page is a system page, so this is rare.

    4. The installer block is added with default settings only. "Include Prerelease
       Packages" stays off, so no attribute value is written for it.

    Reason: Swap the Sample Data page to the new Sample Data Installer block.
*/

/// <summary>
/// Replaces the legacy Rock Solid Church Sample Data block on the Sample Data page
/// with the Sample Data Installer block.
/// </summary>
[SqlMigration( 20261120034315, "21.0" )]
internal class SwapSampleDataBlockForInstaller : Migration
{
    #region Constants

    /// <summary>
    /// The Guid of the new Sample Data Installer block on the Sample Data page.
    /// </summary>
    private const string SampleDataInstallerBlockGuid = "B88C33FA-A871-478C-AAFD-42C5859B143A";

    /// <summary>
    /// The Guid of the legacy Rock Solid Church Sample Data block on the Sample Data page.
    /// </summary>
    private const string LegacySampleDataBlockGuid = "34CA1FA0-F8F1-449F-9788-B5E6315DC058";

    /// <summary>
    /// The Guid of the legacy block type's "XML Document URL" attribute.
    /// </summary>
    private const string LegacyXmlDocumentUrlAttributeGuid = "5E26439E-4E98-45B1-B19B-D5B2F3405963";

    #endregion Constants

    /// <summary>
    /// Operations to be performed during the upgrade process.
    /// </summary>
    public override void Up()
    {
        RegisterSampleDataInstallerBlockType_Up();
        RemoveLegacySampleDataBlock_Up();
        AddSampleDataInstallerBlock_Up();
    }

    #region Private Methods

    /// <summary>
    /// Registers the EntityType and BlockType rows for the Sample Data Installer block.
    /// </summary>
    private void RegisterSampleDataInstallerBlockType_Up()
    {
        RockMigrationHelper.UpdateEntityType(
            "Rock.Blocks.Example.SampleDataInstaller",
            "Sample Data Installer",
            "Rock.Blocks.Example.SampleDataInstaller, Rock.Blocks, Version=21.0.3.0, Culture=neutral, PublicKeyToken=null",
            false,
            false,
            SystemGuid.EntityType.OBSIDIAN_EXAMPLE_SAMPLE_DATA_INSTALLER );

        RockMigrationHelper.AddOrUpdateEntityBlockType(
            "Sample Data Installer",
            "Installs a sample data package into a fresh Rock database.",
            "Rock.Blocks.Example.SampleDataInstaller",
            "Examples",
            SystemGuid.BlockType.OBSIDIAN_EXAMPLE_SAMPLE_DATA_INSTALLER );
    }

    /// <summary>
    /// Removes the legacy Rock Solid Church Sample Data block and its XML Document URL
    /// value from the Sample Data page.
    /// </summary>
    private void RemoveLegacySampleDataBlock_Up()
    {
        RockMigrationHelper.DeleteBlockAttributeValue( LegacySampleDataBlockGuid, LegacyXmlDocumentUrlAttributeGuid );
        RockMigrationHelper.DeleteBlock( LegacySampleDataBlockGuid );
    }

    /// <summary>
    /// Adds the Sample Data Installer block to the Sample Data page.
    /// </summary>
    private void AddSampleDataInstallerBlock_Up()
    {
        RockMigrationHelper.AddBlock( true, SystemGuid.Page.SAMPLE_DATA, null, SystemGuid.BlockType.OBSIDIAN_EXAMPLE_SAMPLE_DATA_INSTALLER, "Sample Data Installer", "Main", "", "", 0, SampleDataInstallerBlockGuid );
    }

    #endregion Private Methods
}
