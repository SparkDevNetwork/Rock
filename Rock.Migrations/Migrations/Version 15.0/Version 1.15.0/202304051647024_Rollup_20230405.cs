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
    public partial class Rollup_20230405 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddUpdateAppleDeviceModels();
            UpdateFutureWeeksAttributeForGroupSchedulingToolboxV2();
            CleanupMigrationHistory();
            CodeGenMigrations();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        private void AddUpdateAppleDeviceModels()
        {
            // Update Existing values
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone14,3", "iPhone 13 Pro Max", "50655253-B65A-4CDA-8B82-77D547F0EBEA", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPod7,1", "6th Gen iPod", "AF883B41-ED81-44C7-A69E-3D4BDD9BEF71", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch1,2", "Apple Watch 42mm case", "B05F49EC-2026-466C-9F4C-2779A57715AB", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch5,9", "Apple Watch SE 40mm case (GPS)", "EFFF3BAD-58D6-40FF-A8EA-B66F31087CF5", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch5,10", "Apple Watch SE 44mm case (GPS)", "1EEEFC4E-3BDD-4852-84D6-552B9B512C7A", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch5,11", "Apple Watch SE 40mm case (GPS+Cellular)", "9F574260-D240-408D-94A7-E72BAD1ABE74", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch5,12", "Apple Watch SE 44mm case (GPS+Cellular)", "BED75931-A35A-49E6-BF66-E8F8FFAE3864", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,1", "Apple Watch Series 6 40mm case (GPS)", "072EB602-236A-4EF8-907D-38709873E463", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,2", "Apple Watch Series 6 44mm case (GPS)", "55961288-307F-40CB-A388-09C033F71E03", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,3", "Apple Watch Series 6 40mm case (GPS+Cellular)", "854BBB1E-F2B2-4224-92BB-7AD9296F4858", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,4", "Apple Watch Series 6 44mm case (GPS+Cellular)", "C66C9680-C2AF-4841-AA69-224054E8D0EC", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,6", "Apple Watch Series 7 41mm case (GPS)", "82BDAEB1-E2CE-4A2E-ADCE-98EA1148277F", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,7", "Apple Watch Series 7 45mm case (GPS)", "A2F46F18-FB52-4685-9953-E84E46873DB6", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,8", "Apple Watch Series 7 41mm case (GPS+Cellular)", "38C5B568-63BE-486D-BCE4-19183DB15EDA", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch6,9", "Apple Watch Series 7 45mm case (GPS+Cellular)", "8DCF48E6-00E4-446D-A4D7-30236D95AF92", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad14,3", "iPad Pro 11 inch 4th Gen", "529BDBD1-85FB-4F38-82D7-747B6C2BCD65", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad14,4", "iPad Pro 11 inch 4th Gen", "04A2EE64-EA46-4F61-8497-4AC0EEE5F6A1", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad14,5", "iPad Pro 12.9 inch 6th Gen", "7D7D90BE-1456-47F9-BAC9-B22B7FBE8A7C", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad14,6", "iPad Pro 12.9 inch 6th Gen", "0FE19DB4-A0BB-4ED1-BF45-7B229AE9F3AD", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad13,18", "iPad 10th Gen", "31FC70CA-7963-458F-93F9-B6907690079B", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad13,19", "iPad 10th Gen", "C226B926-59A0-4D09-8737-5B3A0DC39A58", true );

            //Remove Invalid Values. No need to update PersonalDevice since the description applied is still correct.
            RockMigrationHelper.DeleteDefinedValue( "22C1DA23-C126-4C2C-8142-DB313453151C" ); //iPod6,1
            RockMigrationHelper.DeleteDefinedValue( "5837DBC4-EE9B-4B5A-9900-E8F8678609AC" ); //iPad14,3-B
            RockMigrationHelper.DeleteDefinedValue( "B2F49E10-5211-414B-8258-4B1DA0D7BB1A" ); //iPad14,4-B
            RockMigrationHelper.DeleteDefinedValue( "285FBF08-A40C-4EF3-8225-24C37E3ED8EA" ); //iPad14,5-B

            // Set PersonalDevice.Model to the description instead of the model.
            Sql( @"
                DECLARE @AppleDeviceDefinedTypeId INT = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = 'DAE31F78-7AB9-4ACE-9EE1-C1E6A734562C')

                UPDATE [PersonalDevice] SET [Model] = dv.[Description]
                FROM [PersonalDevice] pd
                JOIN [DefinedValue] dv ON pd.[Model] = dv.[Value]
                WHERE pd.[Manufacturer] = 'Apple'
                  AND pd.[Model] like '%,%'
                  AND dv.DefinedTypeId = @AppleDeviceDefinedTypeId" );
        }

        private void UpdateFutureWeeksAttributeForGroupSchedulingToolboxV2()
        {
            const string _169_UpdateFutureWeeksAttributeForGroupSchedulingToolboxV2 = @"
                DECLARE @EntityTypeId_BlockType INT = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block');
                DECLARE @BlockTypeId_ScheduleToolboxV2 INT = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '18A6DCE3-376C-4A62-B1DD-5E5177C11595');
                DECLARE @FieldTypeId_SlidingDateRange INT = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '55810BC5-45EA-4044-B783-0CCE0A445C6F');

                /*
                NOTE:  This migration has already been run in some existing systems to update the attribute value, so it must be safe to run again when
                they get the migration in Rock.  Because the Attribute Key changes, this migration should not perform any actions when run a second time.
                */
                UPDATE [AttributeValue]
                SET
	                  [Value] = 'Next|' + [Value] + '|Week||'
	                , [ValueAsNumeric] = NULL
	                , [PersistedTextValue] = NULL
	                , [PersistedHtmlValue] = NULL
	                , [PersistedCondensedTextValue] = NULL
	                , [PersistedCondensedHtmlValue] = NULL
	                , [IsPersistedValueDirty] = 1
                WHERE [AttributeId] IN (
	                SELECT [Id]
	                FROM [Attribute]
	                WHERE	[EntityTypeId] = @EntityTypeId_BlockType
		                AND	EntityTypeQualifierColumn = 'BlockTypeId'
		                AND	EntityTypeQualifierValue = @BlockTypeId_ScheduleToolboxV2
		                AND	[Key] = 'FutureWeeksToShow'
	                );

                UPDATE	[Attribute]
                SET
	                  [FieldTypeId] = @FieldTypeId_SlidingDateRange
	                , [Key] = 'FutureWeekDateRange'
	                , [Name] = 'Future Week Date Range'
	                , [AbbreviatedName] = 'Future Week Date Range'
	                , [Description] = 'The date range of future weeks to allow users to sign up for a schedule. Please note that only future dates will be accepted.'
	                , [IsRequired] = 1
	                , [DefaultValue] = 'Next|6|Week||'
	                , [IsDefaultPersistedValueDirty] = 1
                WHERE	[EntityTypeId] = @EntityTypeId_BlockType
	                AND	EntityTypeQualifierColumn = 'BlockTypeId'
	                AND	EntityTypeQualifierValue = @BlockTypeId_ScheduleToolboxV2
	                AND	[Key] = 'FutureWeeksToShow';";

            Sql( _169_UpdateFutureWeeksAttributeForGroupSchedulingToolboxV2 );
        }

        private void CodeGenMigrations()
        {
            // Add Block
            //  Block Name: Membership
            //  Page Name: Extended Attributes V1
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "B2EA21EB-AD0F-4378-B606-78B9D55D81E3".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"D70A59DC-16BE-43BE-9880-59598FA7A94C".AsGuid(), "Membership","SectionB1",@"",@"",0,"01CE8E98-FD2C-42F9-AD7C-DB5007F673AF");

              // Add Block Attribute Value
              //   Block: Membership
              //   BlockType: Attribute Values
              //   Category: CRM > Person Detail
              //   Block Location: Page=Extended Attributes V1, Site=Rock RMS
              //   Attribute: Category
              /*   Attribute Value: e919e722-f895-44a4-b86d-38db8fba1844 */
              RockMigrationHelper.AddBlockAttributeValue("01CE8E98-FD2C-42F9-AD7C-DB5007F673AF","EC43CF32-3BDF-4544-8B6A-CE9208DD7C81",@"e919e722-f895-44a4-b86d-38db8fba1844");
        }

        /// <summary>
        /// Cleanups the migration history records except the last one.
        /// </summary>
        private void CleanupMigrationHistory()
        {
            Sql( @"
                UPDATE [dbo].[__MigrationHistory]
                SET [Model] = 0x
                WHERE MigrationId < (SELECT TOP 1 MigrationId FROM __MigrationHistory ORDER BY MigrationId DESC)" );
        }
    }
}
