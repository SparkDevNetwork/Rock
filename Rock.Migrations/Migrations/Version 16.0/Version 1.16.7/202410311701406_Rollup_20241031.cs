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
    using System.Collections.Generic;
    using System.Data;
    using Rock.Data;
    using Rock.Security;
    using Rock.Utility.Enums;

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20241031 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            IncludeProfilesWithMediumProtectionLevelInDuplicateCheck();
            AddObsidianPageParameterFilterBlockTypeUp();
            UpdateProxyDeviceTypeUp();
            WindowsCheckinClientDownloadLinkUp();
            AddNewDevices();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddObsidianPageParameterFilterBlockTypeDown();
            WindowsCheckinClientDownloadLinkDown();
        }

        private void AddNewDevices()
        {
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone17,4", "iPhone 16 Plus", "0278EAD9-0B7C-4B9F-9D05-F4D5692F459A", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone17,3", "iPhone 16", "1CC01CDF-0F56-498C-A3B9-9080D40FC710", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone17,2", "iPhone 16 Pro Max", "646F4ABE-5AA4-4E9B-8C34-63C27C5EF7A1", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone17,1", "iPhone 16 Pro", "FBF3D89C-7010-4315-A513-FC50F8CDC441", true );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad16,6", "iPad Pro 12.9 inch 7th Gen", "19843044-0870-4760-A644-3CECABBEAA09", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad16,5", "iPad Pro 12.9 inch 7th Gen", "C4CA706E-8B22-4DFE-B15E-4027B1208353", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad16,4", "iPad Pro 11 inch 5th Gen", "CD445D01-E04F-44F3-AA21-46378CC12743", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad16,3", "iPad Pro 11 inch 5th Gen", "369840FF-A488-49EF-ACFA-9515E72F8D3D", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad14,9", "iPad Air 6th Gen", "4F5AD619-A051-4958-B999-E3C8D92B04EC", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad14,8", "iPad Air 6th Gen", "7145FAA3-5F35-49C8-913C-F826C4E85E4E", true );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad14,11", "iPad Air 7th Gen", "4192D9A8-2B97-420A-9851-34F1DE237827", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad14,10", "iPad Air 7th Gen", "B050BD9D-64F9-4DA4-8E1F-8FB14BE8C996", true );
        }

        #region PA:Include Profiles With Medium Protection Level In Duplicate Checks.

        /// <summary>
        /// PA: Include Profiles With Medium Protection Level In Duplicate Checks.
        /// </summary>
        private static void IncludeProfilesWithMediumProtectionLevelInDuplicateCheck()
        {
            string sql = $@"
SELECT [DefaultValue]
FROM [Attribute]
WHERE [Guid] = '{SystemGuid.Attribute.SYSTEM_SECURITY_SETTINGS}'";

            var rockSecuritySystemSettings = DbService.GetDataTable( sql, CommandType.Text, new Dictionary<string, object>() );

            foreach ( DataRow row in rockSecuritySystemSettings.Rows )
            {
                var value = row["DefaultValue"].ToString().FromJsonOrNull<SecuritySettings>();
                if ( value == null )
                {
                    continue;
                }

                var existingProtectiomProfilesToBeIngoredInDuplicateDetection = new HashSet<AccountProtectionProfile> {
                    AccountProtectionProfile.Medium,
                    AccountProtectionProfile.High,
                    AccountProtectionProfile.Extreme
                };

                // If the Security Setting has been updated, leave the setting as is.
                if ( !existingProtectiomProfilesToBeIngoredInDuplicateDetection.SetEquals( value.AccountProtectionProfilesForDuplicateDetectionToIgnore ) )
                {
                    continue;
                }

                value.AccountProtectionProfilesForDuplicateDetectionToIgnore.Remove( AccountProtectionProfile.Medium );

                DbService.ExecuteCommand( "UPDATE [Attribute] SET [DefaultValue] = @value WHERE [Guid] = @guid", CommandType.Text, new Dictionary<string, object>
                {
                    ["guid"] = SystemGuid.Attribute.SYSTEM_SECURITY_SETTINGS.AsGuid(),
                    ["value"] = value.ToJson()
                } );
            }
        }

        #endregion

        #region JPH: Add the obsidian page parameter filter block type and attributes up.

        /// <summary>
        /// JPH: Add the obsidian page parameter filter block type and attributes up.
        /// </summary>
        private void AddObsidianPageParameterFilterBlockTypeUp()
        {
            // Delete unused web forms page parameter filter block attribute.
            Sql( @"
DECLARE @EntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block');
DECLARE @BlockTypeId INT = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '6F8D3F32-ED4C-46A9-A8AD-8740BC3495D7');

DELETE
FROM [Attribute]
WHERE [EntityTypeId] = @EntityTypeId
    AND [EntityTypeQualifierColumn] = 'BlockTypeId'
    AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as NVARCHAR(200))
    AND [Key] = 'HideFilterActions';" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Reporting.PageParameterFilter
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Reporting.PageParameterFilter", "Page Parameter Filter", "Rock.Blocks.Reporting.PageParameterFilter, Rock.Blocks, Version=1.16.6.9, Culture=neutral, PublicKeyToken=null", false, false, "59F94307-B2B0-4383-9C2C-88A4E154C461" );

            // Add/Update Obsidian Block Type
            //   Name:Page Parameter Filter
            //   Category:Reporting
            //   EntityType:Rock.Blocks.Reporting.PageParameterFilter
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Page Parameter Filter", "Filter block that passes the filter values as query string parameters.", "Rock.Blocks.Reporting.PageParameterFilter", "Reporting", "842DFBC2-DA38-465D-BFD2-B6C8585AA3BF" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Block Title Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "842DFBC2-DA38-465D-BFD2-B6C8585AA3BF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Block Title Text", "BlockTitleText", "Block Title Text", @"The text to display as the block title.", 0, @"", "8B424C65-1C4D-4A47-AFF6-64C99F777E6A" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Block Title Icon CSS Class
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "842DFBC2-DA38-465D-BFD2-B6C8585AA3BF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Block Title Icon CSS Class", "BlockTitleIconCSSClass", "Block Title Icon CSS Class", @"The CSS class name to use for the block title icon.", 0, @"fa fa-filter", "7E543C23-6FB8-4538-9249-261C35DE1491" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Show Block Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "842DFBC2-DA38-465D-BFD2-B6C8585AA3BF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Block Title", "ShowBlockTitle", "Show Block Title", @"Determines if the block title should be displayed.", 0, @"True", "54DCAD13-576B-475D-A6D2-5ABA57639212" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Filter Button Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "842DFBC2-DA38-465D-BFD2-B6C8585AA3BF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Filter Button Text", "FilterButtonText", "Filter Button Text", @"The text to display on the filter button.", 0, @"Filter", "EFA46D88-B9A8-422C-B4A5-ADDA5CF13301" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Filter Button Size
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "842DFBC2-DA38-465D-BFD2-B6C8585AA3BF", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Filter Button Size", "FilterButtonSize", "Filter Button Size", @"The size of the filter button.", 0, @"3", "F6FFA298-9434-4BA8-9863-051E4DD52994" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Show Filter Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "842DFBC2-DA38-465D-BFD2-B6C8585AA3BF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Filter Button", "ShowFilterButton", "Show Filter Button", @"Determines if the filter button should be displayed.", 0, @"True", "B85336F2-6F40-4C7B-980D-1F957C4CE167" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Show Reset Filters Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "842DFBC2-DA38-465D-BFD2-B6C8585AA3BF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Reset Filters Button", "ShowResetFiltersButton", "Show Reset Filters Button", @"Determines if the reset filters button should be displayed.", 0, @"True", "82327B97-8A4F-4743-93CF-2E32FF5A8097" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Filters Per Row
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "842DFBC2-DA38-465D-BFD2-B6C8585AA3BF", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Filters Per Row", "FiltersPerRow", "Filters Per Row", @"The number of filters to display per row. Maximum is 12.", 0, @"2", "FF78F581-A1E8-41B4-96A0-70DCDC996A82" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Redirect Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "842DFBC2-DA38-465D-BFD2-B6C8585AA3BF", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Redirect Page", "RedirectPage", "Redirect Page", @"If set, will redirect to the selected page when applying filters.", 0, @"", "0C59A9E4-017C-4675-88E0-AC8428DA2501" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Filter Selection Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "842DFBC2-DA38-465D-BFD2-B6C8585AA3BF", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Filter Selection Action", "DoesSelectionCausePostback", "Apply Filters Upon Selection", @"Describes the action to take when a non-textbox filter is selected by the individual. If ""Apply Filters"", all filters are applied instantly without the need to click the filter button. If ""Update Filters"", any filters whose available values rely on the selected values of other filters will be updated, but the user must click the filter button to apply them. If ""Do Nothing"", no updates happen, and the user must click the button to apply filters.", 0, @"0", "80239FA8-6643-4529-B7C0-F36E1B2304E4" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Enable Legacy Reload
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "842DFBC2-DA38-465D-BFD2-B6C8585AA3BF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Legacy Reload", "EnableLegacyReload", "Enable Legacy Reload", @"If enabled, a full page reload will be triggered to apply the filter selections (helpful when using this block to drive the behavior of legacy blocks on the page). If disabled, the filter selections will be communicated directly to any Obsidian blocks listening for these filters, so they can respond accordingly.", 0, @"False", "FA6FFA32-FC5D-44C0-B54C-FB35E4209156" );
        }

        /// <summary>
        /// JPH: Add the obsidian page parameter filter block type and attributes down.
        /// </summary>
        private void AddObsidianPageParameterFilterBlockTypeDown()
        {
            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Enable Legacy Reload
            RockMigrationHelper.DeleteAttribute( "FA6FFA32-FC5D-44C0-B54C-FB35E4209156" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Filter Selection Action
            RockMigrationHelper.DeleteAttribute( "80239FA8-6643-4529-B7C0-F36E1B2304E4" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Redirect Page
            RockMigrationHelper.DeleteAttribute( "0C59A9E4-017C-4675-88E0-AC8428DA2501" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Filters Per Row
            RockMigrationHelper.DeleteAttribute( "FF78F581-A1E8-41B4-96A0-70DCDC996A82" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Show Reset Filters Button
            RockMigrationHelper.DeleteAttribute( "82327B97-8A4F-4743-93CF-2E32FF5A8097" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Show Filter Button
            RockMigrationHelper.DeleteAttribute( "B85336F2-6F40-4C7B-980D-1F957C4CE167" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Filter Button Size
            RockMigrationHelper.DeleteAttribute( "F6FFA298-9434-4BA8-9863-051E4DD52994" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Filter Button Text
            RockMigrationHelper.DeleteAttribute( "EFA46D88-B9A8-422C-B4A5-ADDA5CF13301" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Show Block Title
            RockMigrationHelper.DeleteAttribute( "54DCAD13-576B-475D-A6D2-5ABA57639212" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Block Title Icon CSS Class
            RockMigrationHelper.DeleteAttribute( "7E543C23-6FB8-4538-9249-261C35DE1491" );

            // Attribute for BlockType
            //   BlockType: Page Parameter Filter
            //   Category: Reporting
            //   Attribute: Block Title Text
            RockMigrationHelper.DeleteAttribute( "8B424C65-1C4D-4A47-AFF6-64C99F777E6A" );

            // Delete BlockType
            //   Name: Page Parameter Filter
            //   Category: Reporting
            //   Path: -
            //   EntityType: Page Parameter Filter
            RockMigrationHelper.DeleteBlockType( "842DFBC2-DA38-465D-BFD2-B6C8585AA3BF" );

            // Delete Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Reporting.PageParameterFilter
            RockMigrationHelper.DeleteEntityType( "59F94307-B2B0-4383-9C2C-88A4E154C461" );
        }

        #endregion

        #region DH: Update Proxy defined value

        private void UpdateProxyDeviceTypeUp()
        {
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.DEVICE_TYPE,
                "Cloud Print Proxy",
                "A proxy device that handles communication between the server and a printer.",
                SystemGuid.DefinedValue.DEVICE_TYPE_CLOUD_PRINT_PROXY,
                true );
        }

        #endregion

        #region DH: Update Windows Check-in Download Link

        private void WindowsCheckinClientDownloadLinkUp()
        {
            Sql( @"
        DECLARE @winCheckinClientDefinedValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid] = 'C162F21E-7D86-4BB4-A72C-7F4A0E5B02C3')
        DECLARE @downloadUrlAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9')

        UPDATE [AttributeValue]
        SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/windowscheckin/1.16.7/checkinclient.msi'
        WHERE AttributeId = @downloadUrlAttributeId and EntityId = @winCheckinClientDefinedValueId" );
        }

        private void WindowsCheckinClientDownloadLinkDown()
        {
            Sql( @"
        DECLARE @winCheckinClientDefinedValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid] = 'C162F21E-7D86-4BB4-A72C-7F4A0E5B02C3')
        DECLARE @downloadUrlAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9')

        UPDATE [AttributeValue]
        SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/windowscheckin/1.13.8/checkinclient.msi'
        WHERE AttributeId = @downloadUrlAttributeId and EntityId = @winCheckinClientDefinedValueId" );
        }

        #endregion
    }
}
