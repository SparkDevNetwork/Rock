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
    public partial class Rollup_20260204 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            JE_IconTransitionTableUpdate_Up(); // v18.3

            // v19.0.5 Remove blocks
            NA_RemoveThreeUnneededBlocks_Up();

            PS_RenameBeaconDashboardToOutreachDashboard_Up();

            // Removed.  See note below.
            // NA_CleanupOldObsoleteBlockTypes_Up();
        }

        private void JE_IconTransitionTableUpdate_Up()
        {
            Sql( @"
  DELETE FROM [__IconTransition]
  WHERE [FontAwesomeFull] = 'fa fa-file-medical-alt'

  DELETE FROM [__IconTransition]
  WHERE [FontAwesomeFull] = 'fa fa-graduation-cap'

  INSERT INTO [__IconTransition]
  ([FontAwesomeClass],[FontAwesomeFull], [TablerClass], [TablerFull])
  VALUES
  ('fa-sign-in','fa fa-sign-in', 'ti-package-import', 'ti ti-package-import'),
  ('fa-hdd-o', 'fa fa-hdd-o', 'ti-server-2', 'ti ti-server-2'),
  ('fa-file-medical-alt', 'fa fa-file-medical-alt', 'ti-heart-rate-monitor', 'ti ti-heart-rate-monitor'),
  ('fa-graduation-cap', 'fa fa-graduation-cap', 'ti-school', 'ti ti-school'),
  ('fa-lightbulb-o', 'fa fa-lightbulb-o', 'ti-bulb', 'ti ti-bulb')

    -- Fix Issue With Double Spacing
     UPDATE [Page]
    SET [IconCssClass] = 'fa fa-database'
    WHERE [IconCssClass] = 'fa  fa-database'

    -- Update page icon for Workflow Import/Export
    UPDATE [Page]
    SET [IconCssClass] = 'ti ti-package-import'
    WHERE [Guid] = 'b6096c72-fe05-472f-b668-b31253dd5e25'
" );
        }

        private void NA_RemoveThreeUnneededBlocks_Up()
        {
            Sql( @"
DECLARE @StarkDynamicAttributesBlockTypeGuid UNIQUEIDENTIFIER = '7C34A0FA-ED0D-4B8B-B458-6EC970711726';
DECLARE @DeveloperEnvironmentInfoBlockTypeGuid UNIQUEIDENTIFIER = '03BFBFCA-36C4-480D-A10B-3CF349F4A6EA';
DECLARE @CacheReaderBlockTypeGuid UNIQUEIDENTIFIER = 'B2859CA9-F796-4D83-A83B-62AA44FC6BC5';

/* -------------------------------------------------------------------
   Delete Block instances for the targeted BlockTypes
------------------------------------------------------------------- */
DELETE [Block]
FROM [Block]
JOIN [BlockType]
    ON [Block].[BlockTypeId] = [BlockType].[Id]
WHERE [BlockType].[Guid] IN (
      @StarkDynamicAttributesBlockTypeGuid
    , @DeveloperEnvironmentInfoBlockTypeGuid
    , @CacheReaderBlockTypeGuid
);

/* -------------------------------------------------------------------
   Delete the targeted BlockTypes
------------------------------------------------------------------- */
DELETE [BlockType]
FROM [BlockType]
WHERE [BlockType].[Guid] IN (
      @StarkDynamicAttributesBlockTypeGuid
    , @DeveloperEnvironmentInfoBlockTypeGuid
    , @CacheReaderBlockTypeGuid
);" );
        }

        private void PS_RenameBeaconDashboardToOutreachDashboard_Up()
        {
            RockMigrationHelper.RenameEntityType(
                "A3D9F1C4-E3C1-4D3A-8C2E-7F4B5B6D9F1C",
                "Rock.Blocks.Types.Mobile.Engagement.OutreachDashboard",
                "Outreach Dashboard",
                "Rock.Blocks.Types.Mobile.Engagement.OutreachDashboard, Rock, Version=19.0.5.0, Culture=neutral, PublicKeyToken=null",
                false,
                false );

            RockMigrationHelper.AddOrUpdateEntityBlockType( "Outreach Dashboard", "Outreach dashboard allows you to view your touchpoint statistic and as well as start connecting with your contact.", "Rock.Blocks.Types.Mobile.Engagement.OutreachDashboard", "Engagement", "A1B2C3D4-E5F6-4789-ABCD-1234567890AB" );
        }

        /// <summary>
        /// Remove old, obsolete block types which are still defined on some Rock systems.
        /// </summary>
        private void NA_CleanupOldObsoleteBlockTypes_Up()
        {
            /*
                 3/17/2026 - NA

                 Disabled cleanup of legacy BlockTypes because some installations may not yet
                 have the Obsidian replacements if Chop Jobs have not executed. Running
                 this cleanup too early could remove required WebForms blocks and cause missing
                 functionality (the Obsidian block won't be put onto the right pages).

                 Reason: Prevent premature deletion of BlockTypes before Obsidian equivalents exist.
            */
            //            Sql( @"
            //-- Delete old, chopped (v15.2) core webforms FamilyPreRegistration.ascx block
            //DELETE [BlockType] WHERE [Path] = '~/Blocks/Crm/FamilyPreRegistration.ascx' AND [Guid] = '463a454a-6370-4b4a-bca1-415f2d9b0cb7'

            //-- Delete old, chopped (v17.1) core Scheduled Job Detail block
            //DELETE [BlockType] WHERE [Path] = '~/Blocks/Core/ScheduledJobDetail.ascx'

            //-- Delete old, very obsolete (v0.1) PluginManager block
            //DECLARE @PluginManagerBlockTypeId INT = ( SELECT TOP (1) [Id] FROM [BlockType] WHERE [Path] = '~/Blocks/Core/PluginManager.ascx' AND [Guid] = 'F80268E6-2625-4565-AA2E-790C5E40A119' );

            //IF @PluginManagerBlockTypeId IS NOT NULL
            //BEGIN
            //    DELETE FROM [Block]
            //    WHERE [BlockTypeId] = @PluginManagerBlockTypeId;

            //    DELETE FROM [BlockType]
            //    WHERE [Id] = @PluginManagerBlockTypeId;
            //END
            //" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            PS_RenameBeaconDashboardToOutreachDashboard_Down();
        }

        private void PS_RenameBeaconDashboardToOutreachDashboard_Down()
        {
            RockMigrationHelper.RenameEntityType(
                "A3D9F1C4-E3C1-4D3A-8C2E-7F4B5B6D9F1C",
                "Rock.Blocks.Types.Mobile.Engagement.BeaconDashboard",
                "Beacon Dashboard",
                "Rock.Blocks.Types.Mobile.Engagement.BeaconDashboard, Rock, Version=19.0.5.0, Culture=neutral, PublicKeyToken=null",
                false,
                false );

            RockMigrationHelper.AddOrUpdateEntityBlockType( "Beacon Dashboard", "Beacon dashboard allows you to view your touchpoint statistic and as well as start connecting with your contact.", "Rock.Blocks.Types.Mobile.Engagement.BeaconDashboard", "Engagement", "A1B2C3D4-E5F6-4789-ABCD-1234567890AB" );
        }
    }
}
