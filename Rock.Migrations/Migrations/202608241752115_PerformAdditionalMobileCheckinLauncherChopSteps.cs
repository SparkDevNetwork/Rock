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

    /// <summary>
    ///
    /// </summary>
    public partial class PerformAdditionalMobileCheckinLauncherChopSteps : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            JPH_AddMobileCheckinLauncherBlockEntityTypeAndBlockSettings_Up();
            JPH_ClearMobileCheckinLauncherCheckinThemeSetting_Up();
            JPH_MoveMobileCheckinLauncherPageToNextGenCheckinSite_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            JPH_MoveMobileCheckinLauncherPageToNextGenCheckinSite_Down();
            JPH_ClearMobileCheckinLauncherCheckinThemeSetting_Down();
            JPH_AddMobileCheckinLauncherBlockEntityTypeAndBlockSettings_Down();
        }

        #region Add Block Entity Type and Block Settings

        /// <summary>
        /// JPH: Adds the Mobile Check-in Launcher block entity type and block settings - up.
        /// </summary>
        private void JPH_AddMobileCheckinLauncherBlockEntityTypeAndBlockSettings_Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.CheckIn.MobileCheckInLauncher
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.CheckIn.MobileCheckInLauncher", "Mobile Check In Launcher", "Rock.Blocks.CheckIn.MobileCheckInLauncher, Rock.Blocks, Version=20.0.7.0, Culture=neutral, PublicKeyToken=null", false, false, "FA4A6783-BFAA-4129-AE24-5BF871518EE9" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Disable QR Code
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FA4D15E6-4C85-4247-A374-5E592E711CFD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable QR Code", "DisableQRCode", "Disable QR Code", @"If disabled, no QR code is shown on the mobile device after check-in. Use this for events that do not print labels.", 0, @"False", "9C4FA18D-4DEB-4506-8361-AD4B99B6D765" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Select All Schedules Automatically
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FA4D15E6-4C85-4247-A374-5E592E711CFD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Select All Schedules Automatically", "SelectAllSchedulesAutomatically", "Select All Schedules Automatically", @"When enabled, all available schedules are selected automatically instead of asking the individual to make a selection. This will also disable the 'skip' screen when there is nothing to check into, instead those individuals will quietly be skipped and not checked in.", 1, @"False", "62823651-4819-4D50-84CD-41CA381FFDB1" );
        }

        /// <summary>
        /// JPH: Adds the Mobile Check-in Launcher block entity type and block settings - down.
        /// </summary>
        private void JPH_AddMobileCheckinLauncherBlockEntityTypeAndBlockSettings_Down()
        {
            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Select All Schedules Automatically
            RockMigrationHelper.DeleteAttribute( "62823651-4819-4D50-84CD-41CA381FFDB1" );

            // Attribute for BlockType
            //   BlockType: Mobile Check-in Launcher
            //   Category: Check-in
            //   Attribute: Disable QR Code
            RockMigrationHelper.DeleteAttribute( "9C4FA18D-4DEB-4506-8361-AD4B99B6D765" );

            // Remove the Obsidian Block Entity Type from the Mobile Check-in Launcher Block Type.
            Sql( @"
DECLARE @BlockTypeId INT = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = 'FA4D15E6-4C85-4247-A374-5E592E711CFD');

IF @BlockTypeId IS NOT NULL
BEGIN
    UPDATE [BlockType]
    SET [Path] = '~/Blocks/CheckIn/MobileLauncher.ascx'
        , [EntityTypeId] = NULL
    WHERE [Id] = @BlockTypeId;
END" );

            // Delete Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.CheckIn.MobileCheckInLauncher
            RockMigrationHelper.DeleteEntityType( "FA4A6783-BFAA-4129-AE24-5BF871518EE9" );
        }

        #endregion Add Block Entity Type and Block Settings

        #region Clear Check-in Theme Setting

        /// <summary>
        /// JPH: Clears the Mobile Check-in Launcher CheckinTheme setting - up.
        /// </summary>
        private void JPH_ClearMobileCheckinLauncherCheckinThemeSetting_Up()
        {
            Sql( $@"
DECLARE @BlockEntityTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '{Rock.SystemGuid.EntityType.BLOCK}');
DECLARE @BlockTypeId INT = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = 'FA4D15E6-4C85-4247-A374-5E592E711CFD');

IF @BlockEntityTypeId IS NOT NULL AND @BlockTypeId IS NOT NULL
BEGIN
    DECLARE @AttributeId INT = (
        SELECT TOP 1 [Id]
        FROM [Attribute]
        WHERE [Key] = 'CheckinTheme'
            AND [EntityTypeId] = @BlockEntityTypeId
            AND [EntityTypeQualifierColumn] = 'BlockTypeId'
            AND [EntityTypeQualifierValue] = CAST(@BlockTypeId AS VARCHAR)
    );

    IF @AttributeId IS NOT NULL
    BEGIN
        -- Clear the attribute's default value and any persisted values.
        -- Set the attribute to no longer be required, so it can be inherited from the site.
        UPDATE [Attribute]
        SET [DefaultValue] = ''
            , [DefaultPersistedTextValue] = NULL
            , [DefaultPersistedHtmlValue] = NULL
            , [DefaultPersistedCondensedTextValue] = NULL
            , [DefaultPersistedCondensedHtmlValue] = NULL
            , [IsDefaultPersistedValueDirty] = 1
            , [IsRequired] = 0
        WHERE [Id] = @AttributeId;

        -- Delete any existing values for the attribute.
        DELETE FROM [AttributeValue]
        WHERE [AttributeId] = @AttributeId;
    END
END
" );
        }

        /// <summary>
        /// JPH: Clears the Mobile Check-in Launcher CheckinTheme setting - down.
        /// </summary>
        private void JPH_ClearMobileCheckinLauncherCheckinThemeSetting_Down()
        {
            Sql( $@"
DECLARE @BlockEntityTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '{Rock.SystemGuid.EntityType.BLOCK}');
DECLARE @BlockTypeId INT = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = 'FA4D15E6-4C85-4247-A374-5E592E711CFD');

IF @BlockEntityTypeId IS NOT NULL AND @BlockTypeId IS NOT NULL
BEGIN
    DECLARE @AttributeId INT = (
        SELECT TOP 1 [Id]
        FROM [Attribute]
        WHERE [Key] = 'CheckinTheme'
            AND [EntityTypeId] = @BlockEntityTypeId
            AND [EntityTypeQualifierColumn] = 'BlockTypeId'
            AND [EntityTypeQualifierValue] = CAST(@BlockTypeId AS VARCHAR)
    );

    IF @AttributeId IS NOT NULL
    BEGIN
        -- Restore the attribute's default value and clear any persisted values.
        -- Set the attribute to once again be required, since we have a default value.
        UPDATE [Attribute]
        SET [DefaultValue] = 'CheckinElectric'
            , [DefaultPersistedTextValue] = NULL
            , [DefaultPersistedHtmlValue] = NULL
            , [DefaultPersistedCondensedTextValue] = NULL
            , [DefaultPersistedCondensedHtmlValue] = NULL
            , [IsDefaultPersistedValueDirty] = 1
            , [IsRequired] = 1
        WHERE [Id] = @AttributeId;

        -- We have no way to restore values.
    END
END
" );
        }

        #endregion Clear Check-in Theme Setting

        #region Move Mobile Check-in Launcher Page

        /// <summary>
        /// JPH: Moves the Mobile Check-in Launcher page to the Next Gen Check-in site and layout - up.
        /// </summary>
        private void JPH_MoveMobileCheckinLauncherPageToNextGenCheckinSite_Up()
        {
            Sql( $@"
DECLARE @SiteId INT = (SELECT TOP 1 [Id] FROM [Site] WHERE [Guid] = '{Rock.SystemGuid.Site.NEXT_GEN_CHECK_IN}');
DECLARE @LayoutId INT = (SELECT TOP 1 [Id] FROM [Layout] WHERE [Guid] = '{Rock.SystemGuid.Layout.NEXT_GEN_CHECK_IN_CHECKIN}');

IF @SiteId IS NOT NULL AND @LayoutId IS NOT NULL
BEGIN
    UPDATE [Page]
    SET [SiteId] = @SiteId
        , [LayoutId] = @LayoutId
    WHERE [Guid] = '2D0CD3CA-E952-4A63-B968-94833F95B389';
END" );
        }

        /// <summary>
        /// JPH: Moves the Mobile Check-in Launcher page to the Next Gen Check-in site and layout - down.
        /// </summary>
        private void JPH_MoveMobileCheckinLauncherPageToNextGenCheckinSite_Down()
        {
            Sql( $@"
DECLARE @SiteId INT = (SELECT TOP 1 [Id] FROM [Site] WHERE [Guid] = '15AEFC01-ACB3-4F5D-B83E-AB3AB7F2A54A');        -- [Classic] Rock Check-in
DECLARE @LayoutId INT = (SELECT TOP 1 [Id] FROM [Layout] WHERE [Guid] = '66FA0143-F04C-4447-A67A-2A10A6BB1A2B');    -- [Classic] Checkin

IF @SiteId IS NOT NULL AND @LayoutId IS NOT NULL
BEGIN
    UPDATE [Page]
    SET [SiteId] = @SiteId
        , [LayoutId] = @LayoutId
    WHERE [Guid] = '2D0CD3CA-E952-4A63-B968-94833F95B389';
END" );
        }

        #endregion Move Mobile Check-in Launcher Page
    }
}
