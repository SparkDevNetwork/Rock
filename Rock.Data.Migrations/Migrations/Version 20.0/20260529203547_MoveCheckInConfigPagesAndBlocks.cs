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

/// <summary>
///
/// </summary>
[SqlMigration( 20260529203547, "20.0" )]
internal class MoveCheckInConfigPagesAndBlocks : Migration
{
    /// <summary>
    /// The attribute qualifier key that stores a Boolean attribute's edit control type (drop down, checkbox, etc.).
    /// </summary>
    private const string BooleanControlTypeQualifierKey = "BooleanControlType";

    /// <summary>
    /// Operations to be performed during the upgrade process.
    /// </summary>
    public override void Up()
    {
        JPH_MigrateCheckInConfigPages_Up();
        JPH_MigrateCheckInConfigBlocks_Up();
        JPH_MigrateCheckInConfigAttributes_Up();
        JPH_AddCheckInQuickLinks_Up();
    }

    /// <summary>
    /// JPH: Migrate check-in config pages - up.
    /// </summary>
    private void JPH_MigrateCheckInConfigPages_Up()
    {
        // Reparent the existing Check-in landing page to the top-level Admin Tools page.
        RockMigrationHelper.MovePage( Rock.SystemGuid.Page.CHECK_IN_ROCK_SETTINGS, Rock.SystemGuid.Page.ROCK_SETTINGS );

        // ----------------------------------

        // Rename the Check-in Configuration page to "Configuration Settings".
        RockMigrationHelper.RenamePage( Rock.SystemGuid.Page.CHECK_IN_CONFIGURATION, "Configuration Settings" );

        // Add Page Route
        //   Page:Configuration Settings
        //   Route:admin/checkin/configuration-settings/{CheckInConfiguration}
        RockMigrationHelper.AddOrUpdatePageRoute( Rock.SystemGuid.Page.CHECK_IN_CONFIGURATION, "admin/checkin/configuration-settings/{CheckInConfiguration}", "4A5B39FE-C108-41A3-B920-64C70E84FA21" );

        // Delete Page Route
        //   Page:Configuration Settings
        //   Route:admin/checkin/configuration
        RockMigrationHelper.DeletePageRoute( "3F9DFC83-20C3-79D1-6C0E-FD92F3DB3B36" );

        // ----------------------------------

        // Reparent the existing Schedule Builder page from Check-in Configuration to Check-in so it becomes a
        // sibling of Configuration Settings.
        RockMigrationHelper.MovePage( Rock.SystemGuid.Page.SCHEDULE_BUILDER, Rock.SystemGuid.Page.CHECK_IN_ROCK_SETTINGS );

        // Add Page Route
        //   Page:Schedule Builder
        //   Route:admin/checkin/configuration-schedule-builder/{CheckInConfiguration}
        RockMigrationHelper.AddOrUpdatePageRoute( Rock.SystemGuid.Page.SCHEDULE_BUILDER, "admin/checkin/configuration-schedule-builder/{CheckInConfiguration}", "F39AF9BF-D1C8-451A-A88E-47622625D287" );

        // ----------------------------------

        // Add Page
        //  Internal Name: Areas and Groups
        //  Site: Rock RMS
        RockMigrationHelper.AddPage( true, Rock.SystemGuid.Page.CHECK_IN_ROCK_SETTINGS, Rock.SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Areas and Groups", "", Rock.SystemGuid.Page.CHECK_IN_AREAS_AND_GROUPS, "", Rock.SystemGuid.Page.CHECK_IN_CONFIGURATION );

        // Add Page Route
        //   Page:Areas and Groups
        //   Route:admin/checkin/configuration-areas-groups/{CheckInConfiguration}
        RockMigrationHelper.AddOrUpdatePageRoute( Rock.SystemGuid.Page.CHECK_IN_AREAS_AND_GROUPS, "admin/checkin/configuration-areas-groups/{CheckInConfiguration}", "370B7EF2-5972-4D47-80D7-8CE28E38758B" );

        // ----------------------------------

        // Free up `admin/checkin/labels` by moving Check-in Labels (Classic) pages to `admin/checkin/labels-classic`.
        RockMigrationHelper.UpdatePageRoute( "B43C6BE4-5FFD-127A-1FFE-8E30010703A9", Rock.SystemGuid.Page.CHECK_IN_LABELS, "admin/checkin/labels-classic" );
        RockMigrationHelper.UpdatePageRoute( "8AEF349D-2BF3-2100-4F4A-DD2A4D096F6E", Rock.SystemGuid.Page.CHECK_IN_LABEL, "admin/checkin/labels-classic/{BinaryFileId}" );
        RockMigrationHelper.UpdatePageRoute( "EE688EAE-9006-6BC3-048E-F855A7CF73FE", "15D3766A-6026-4F29-B5C6-5944204642F3", "admin/checkin/labels-classic/{BinaryFileId}/edit" );
        RockMigrationHelper.RenamePage( Rock.SystemGuid.Page.CHECK_IN_LABELS, "Classic Labels" );
        RockMigrationHelper.RenamePage( Rock.SystemGuid.Page.CHECK_IN_LABEL, "Classic Label" );
        RockMigrationHelper.RenamePage( "15D3766A-6026-4F29-B5C6-5944204642F3", "Edit Classic Label" );

        // Claim `admin/checkin/labels` for the Next-Gen Labels pages.
        RockMigrationHelper.UpdatePageRoute( "aed9acf2-6698-4868-8da6-81d1646933b1", "FD2A703D-528E-4763-AB87-5CFEB2349259", "admin/checkin/labels" );
        RockMigrationHelper.UpdatePageRoute( "372c87bf-8486-48e7-ab59-0903ee726165", "8DE681F3-0FE1-45B3-8CED-747E942BE135", "admin/checkin/labels/{CheckInLabelId}" );
        RockMigrationHelper.UpdatePageRoute( "8f61cb27-c679-4e7e-8b8a-8e9c79df406b", "C165DF04-2217-41AD-95D6-AD3CDCE667FD", "admin/checkin/labels/{CheckInLabelId}/designer" );
        RockMigrationHelper.RenamePage( "FD2A703D-528E-4763-AB87-5CFEB2349259", "Labels" );
        // The Next-Gen child label pages already have generic names.

        // ----------------------------------

        // Cloud Print did not already have a route.
        RockMigrationHelper.AddOrUpdatePageRoute( "8BBE9720-0B96-46EF-9FE5-CCAD48E7ABDA", "admin/checkin/cloud-print", "68184F58-AA48-4727-AE5A-DB46D149133E" );

        // ----------------------------------

        // Update the Label Merge Fields page's icon CSS class to stand apart from that of the main labels page.
        Sql( $@"
UPDATE [Page]
SET [IconCssClass] = 'ti ti-tag-plus'
WHERE [Guid] = '{Rock.SystemGuid.Page.LABEL_MERGE_FIELDS}';" );

        // Rename to "Classic Label Merge Fields" since merge fields are a Classic Labels concept (next-gen
        // labels use their own field data sources).
        RockMigrationHelper.RenamePage( Rock.SystemGuid.Page.LABEL_MERGE_FIELDS, "Classic Label Merge Fields" );
    }

    /// <summary>
    /// JPH: Migrate check-in config block types and blocks - up.
    /// </summary>
    private void JPH_MigrateCheckInConfigBlocks_Up()
    {
        // Remove the legacy PageMenu block from the Check-in landing so the new CheckInConfigurationList takes
        // its place.
        RockMigrationHelper.DeleteBlock( "67768707-9371-4638-BA0F-9144574FC25A" );

        // Add Block
        //  Block Name: Check-in Configuration List
        //  Page Name: Check-in
        //  Layout: -
        //  Site: Rock RMS
        RockMigrationHelper.AddBlock( true, Rock.SystemGuid.Page.CHECK_IN_ROCK_SETTINGS.AsGuid(), null, Rock.SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), "41233A39-404A-478F-A7FC-536B644E6728".AsGuid(), "Check-in Configuration List", "Main", @"", @"", 0, "15ECE4BE-407A-4788-B6D0-7813DE606FE0" );

        // ----------------------------------

        // Remove the legacy Check-in Types block from the Check-in Configuration page since its functionality is
        // being replaced by the new Check-in Configuration List block (which was not a direct "chop").
        RockMigrationHelper.DeleteBlock( "72578c6c-3970-4ae7-a528-afc761ea578a" );

        // Remove the legacy Check-in Types block type, as there was no direct Obsidian block type chop for this
        // one, and we don't want it to be accidentally reused. But first, point any remaining instances to the
        // new block type, just in case. The old block type had no attributes, so this should be safe.
        Sql( $@"
DECLARE @OldBlockTypeId INT = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = '50029382-75A6-4B73-9644-880845B3116A');
DECLARE @NewBlockTypeId INT = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = '41233A39-404A-478F-A7FC-536B644E6728');

IF @OldBlockTypeId IS NOT NULL AND @NewBlockTypeId IS NOT NULL
BEGIN
    UPDATE [Block]
    SET [BlockTypeId] = @NewBlockTypeId
    WHERE [BlockTypeId] = @OldBlockTypeId;
END" );

        RockMigrationHelper.DeleteBlockType( "50029382-75A6-4B73-9644-880845B3116A" );

        // ----------------------------------

        // Remove the legacy Check-in Areas block from the Check-in Configuration page since its chopped successor
        // (Check-in Areas and Groups) is being added to a new page.
        RockMigrationHelper.DeleteBlock( "db03dadc-36d8-4135-b339-dce1a02772a8" );

        // ----------------------------------

        // Add/Update Obsidian Block Entity Type
        //   EntityType:Rock.Blocks.CheckIn.Configuration.CheckInAreasAndGroups
        RockMigrationHelper.UpdateEntityType( "Rock.Blocks.CheckIn.Configuration.CheckInAreasAndGroups", "Check In Areas And Groups", "Rock.Blocks.CheckIn.Configuration.CheckInAreasAndGroups, Rock.Blocks, Version=20.0.2.0, Culture=neutral, PublicKeyToken=null", false, false, "B648BB88-E6C2-4BFF-A3A6-FB601C602776" );

        // Attribute for BlockType
        //   BlockType: Check-in Areas and Groups
        //   Category: Check-in > Configuration
        //   Attribute: Enable Classic Check-in Labels
        RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B7CD296F-3AAB-4BA3-902C-44DB96C79798", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Classic Check-in Labels", "EnableClassicCheckInLabels", "Enable Classic Check-in Labels", @"Enabling this will allow you to add Classic Check-in Labels to this area.", 0, @"True", "AF365EE6-6740-4EA4-9CE6-C44B2C093CE6" );

        // Add Block
        //  Block Name: Check-in Areas and Groups
        //  Page Name: Areas and Groups
        //  Layout: -
        //  Site: Rock RMS
        RockMigrationHelper.AddBlock( true, Rock.SystemGuid.Page.CHECK_IN_AREAS_AND_GROUPS.AsGuid(), null, Rock.SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), "B7CD296F-3AAB-4BA3-902C-44DB96C79798".AsGuid(), "Check-in Areas and Groups", "Main", @"", @"", 0, "BD909EDA-8DCE-45C7-8703-2BEA051939AC" );

        // ----------------------------------

        // Update the preexisting Legacy CheckInAreas block type and any instances to reflect the new CheckInAreasAndGroups block type.
        // If they've changed the name of any instances from the previous default, leave their names as-is.
        Sql( @"
DECLARE @BlockTypeId INT = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = 'B7CD296F-3AAB-4BA3-902C-44DB96C79798');

IF @BlockTypeId IS NOT NULL
BEGIN
    UPDATE [BlockType]
    SET [Name] = 'Check-in Areas and Groups'
        , [Description] = 'Helps to build the areas and groups used for check-in.'
    WHERE [Id] = @BlockTypeId;

    UPDATE [Block]
    SET [Name] = 'Check-in Areas and Groups'
    WHERE [BlockTypeId] = @BlockTypeId
        AND [Name] = 'Check-in Areas';
END" );
    }

    /// <summary>
    /// JPH: Migrate check-in config attributes - up.
    /// </summary>
    private void JPH_MigrateCheckInConfigAttributes_Up()
    {
        // The attribute is already a Boolean field type; whether it edits as a checkbox or a Yes/No drop down is
        // driven by its BooleanControlType qualifier. Flip it to a checkbox.
        var checkboxControlType = ( ( int ) Rock.Enums.Controls.BooleanControlType.Checkbox ).ToString();

        Sql( $@"
DECLARE @AttributeId INT = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '6DC6E992-4CAF-4C9F-B11D-5918D244BD40');

IF @AttributeId IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM [AttributeQualifier] WHERE [AttributeId] = @AttributeId AND [Key] = '{BooleanControlTypeQualifierKey}')
    BEGIN
        UPDATE [AttributeQualifier]
        SET [Value] = '{checkboxControlType}'
        WHERE [AttributeId] = @AttributeId AND [Key] = '{BooleanControlTypeQualifierKey}';
    END
    ELSE
    BEGIN
        INSERT INTO [AttributeQualifier] ([IsSystem], [AttributeId], [Key], [Value], [Guid])
        VALUES (0, @AttributeId, '{BooleanControlTypeQualifierKey}', '{checkboxControlType}', NEWID());
    END
END" );
    }

    /// <summary>
    /// JPH: Add check-in quick links - up.
    /// </summary>
    private void JPH_AddCheckInQuickLinks_Up()
    {
        // The existing "External Website" quick link that the new links are inserted directly after.
        var externalWebsiteLink = @"<li class=""list-group-item""><a href=""~/page/1"">External Website</a></li>";
        var checkInLink = @"<li class=""list-group-item""><a href=""~/nextgen-checkin"">Check-in</a></li>";
        var classicCheckInLink = @"<li class=""list-group-item""><a href=""~/checkin"">Classic Check-in</a></li>";
        var checkInManagerLink = @"<li class=""list-group-item""><a href=""~/checkinmanager"">Check-in Manager</a></li>";

        // Use NormalizeColumnCRLF when attempting to do a WHERE clause or REPLACE using multi line strings!
        var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "Content" );

        // Scope to the block, not the single version row, so every version of this content (the active one and
        // any drafts) picks up the new links.
        Sql( $@"
DECLARE @BlockId INT = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '6A648E77-ABA9-4AAF-A8BB-027A12261ED9');

UPDATE [HtmlContent]
SET [Content] = REPLACE({targetColumn}, '{externalWebsiteLink}', '{externalWebsiteLink}' + CHAR(13) + CHAR(10) + '{checkInLink}' + CHAR(13) + CHAR(10) + '{classicCheckInLink}' + CHAR(13) + CHAR(10) + '{checkInManagerLink}')
WHERE [BlockId] = @BlockId
    AND {targetColumn} LIKE '%{externalWebsiteLink}%'
    AND {targetColumn} NOT LIKE '%>Check-in</a>%';" );
    }
}
