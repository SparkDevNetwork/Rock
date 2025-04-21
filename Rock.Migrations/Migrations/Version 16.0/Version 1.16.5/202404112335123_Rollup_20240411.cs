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
    using Rock.Data;
    using Rock.SystemGuid;

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20240411 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            MigrationGenderBlockTypeAttributesUp();
            AddBlockAttributeKeysToIgnoreAttributeToPostUpdateDataMigrationsReplaceWebFormsBlocksWithObsidianBlocksJob();
            AddBirthMonthRangeAttributeUp();
            AddSavedCheckInConfigurationsDefinedTypeUp();
            AddDeviceAdsContentChannelType();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddBirthMonthRangeAttributeDown();
            AddSavedCheckInConfigurationsDefinedTypeDown();
        }

        /// <summary>
        /// KA: Also Be Sure to Migrate All Block Instances for that BlockType (find existing data migration where we've done this already)
        /// </summary>
        private void MigrationGenderBlockTypeAttributesUp()
        {
            const string publicProfileEditBlockTypeGuid = "841D1670-8BFD-4913-8409-FB47EB7A2AB9";
            const string genderBlockTypeAttributeGuid = "DD636ABE-3E5B-442F-9548-9F85DF768FFF";

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( publicProfileEditBlockTypeGuid, Rock.SystemGuid.FieldType.SINGLE_SELECT, "Gender", "Gender", "Gender", "How should Gender be displayed?", 26, "Required", genderBlockTypeAttributeGuid );

            Sql( @"
DECLARE @BlockEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block')
DECLARE @BlockTypeId INT = (SELECT [Id] FROM [BlockType] WHERE Guid = '841D1670-8BFD-4913-8409-FB47EB7A2AB9')

DECLARE @GenderAttributeId INT = (SELECT [Id] FROM [Attribute] 
WHERE [KEY] = 'Gender' 
AND [EntityTypeId] = @BlockEntityTypeId 
AND [EntityTypeQualifierColumn] = 'BlockTypeId' 
AND [EntityTypeQualifierValue] = CAST(@BlockTypeId AS VARCHAR))

DECLARE @RequireGenderAttributeId INT = (SELECT [Id] FROM [Attribute] 
WHERE [KEY] = 'RequireGender' 
AND [EntityTypeId] = @BlockEntityTypeId 
AND [EntityTypeQualifierColumn] = 'BlockTypeId' 
AND [EntityTypeQualifierValue] = CAST(@BlockTypeId AS VARCHAR))

DECLARE @ShowGenderAttributeId INT = (SELECT [Id] FROM [Attribute] 
WHERE [KEY] = 'ShowGender' 
AND [EntityTypeId] = @BlockEntityTypeId 
AND [EntityTypeQualifierColumn] = 'BlockTypeId' 
AND [EntityTypeQualifierValue] = CAST(@BlockTypeId AS VARCHAR))

DECLARE @BlockId INT
DECLARE @RequireGender VARCHAR(50)
DECLARE @ShowGender VARCHAR(50)
DECLARE @TheValue VARCHAR(50)

DECLARE block_cursor CURSOR FOR
SELECT [Id] FROM [Block] WHERE BlockTypeId = @BlockTypeId

OPEN block_cursor
FETCH NEXT FROM block_cursor INTO @BlockId

WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @RequireGender = [Value] FROM [AttributeValue] WHERE [EntityId] = @BlockId AND [AttributeId] = @RequireGenderAttributeId
    SELECT @ShowGender = [Value] FROM [AttributeValue] WHERE [EntityId] = @BlockId AND [AttributeId] = @ShowGenderAttributeId

    SET @TheValue = CASE
        WHEN @RequireGender = 'True' AND @ShowGender = 'True' THEN 'Required'
        WHEN @RequireGender = 'True' AND @ShowGender = 'False' THEN 'Required'
        WHEN @RequireGender = 'False' AND @ShowGender = 'False' THEN 'Hide'
        WHEN @RequireGender = 'False' AND @ShowGender = 'True' THEN 'Optional'
        ELSE 'Required'
    END

    IF EXISTS (SELECT 1 FROM [AttributeValue] WHERE [EntityId] = @BlockId AND [AttributeId] = @GenderAttributeId)  
    BEGIN  
        UPDATE [AttributeValue]   
        SET [Value] = @TheValue 
        WHERE [EntityId] = @BlockId AND [AttributeId] = @GenderAttributeId;  
    END  
    ELSE  
    BEGIN  
        INSERT INTO [AttributeValue] (
            [IsSystem],
            [AttributeId],
            [EntityId],
            [Value],
            [Guid])
        VALUES(
            1,
            @GenderAttributeId,
            @BlockId,
            @TheValue,
            NEWID()) 
    END

    FETCH NEXT FROM block_cursor INTO @BlockId
END

CLOSE block_cursor
DEALLOCATE block_cursor
" );
        }

        /// <summary>
        /// PA: Add BlockAttributeKeysToIgnore Attribute to PostUpdateDataMigrationsReplaceWebFormsBlocksWithObsidianBlocks Job.
        /// Also populate the existing jobs with the Block Attributes to be ignored so that they do not fail.
        /// </summary>
        private void AddBlockAttributeKeysToIgnoreAttributeToPostUpdateDataMigrationsReplaceWebFormsBlocksWithObsidianBlocksJob()
        {
            // Update the Block Attribute Key of the old webforms Login Legacy block to match the corresponding one in the new Obsidian version.
            Sql( "UPDATE [ATTRIBUTE] SET [KEY] = 'HideNewAccountOption' WHERE [GUID] = '7D47046D-5D66-45BB-ACFA-7460DE112FC2'" );

            // Update the Block Attribute Keys of the old webforms Group Registration block to match the corresponding ones in the new Obsidian version.
            Sql( "UPDATE [ATTRIBUTE] SET [KEY] = 'RequireEmail' WHERE [GUID] = '37E22E5F-19C9-4F17-8E1D-8C0E5F52DE1D'" );
            Sql( "UPDATE [ATTRIBUTE] SET [KEY] = 'RequireMobilePhone' WHERE [GUID] = '7CE78567-4438-47E7-B0D0-25D5B6498515'" );

            // Add the attribute to the existing Chop/Swap job if needed.
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", Rock.SystemGuid.FieldType.KEY_VALUE_LIST, "Class", "Rock.Jobs.PostUpdateDataMigrationsReplaceWebFormsBlocksWithObsidianBlocks", "Webform BlockType Attribute Keys To Ignore During Validation", "Webform BlockType Attribute Keys To Ignore During Validation", "A Guid [key] of the old Webform BlockType and the [value] as a comma delimited list of BlockType Attribute keys to ignore when validating the Obsidian and Webforms blocks have the same keys.", 3, "", "D5122332-BA5E-4919-BCE0-5F8EF301B43C", "BlockAttributeKeysToIgnore" );

            // Exclude Attributes from the BlockTypes.
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

DECLARE @ChopEmailPreferenceEntryServiceJobId int = (SELECT [Id] FROM [ServiceJob] WHERE [Guid] = '{ServiceJob.DATA_MIGRATIONS_162_CHOP_EMAIL_PREFERENCE_ENTRY}');
IF NOT EXISTS ( SELECT * FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @ChopEmailPreferenceEntryServiceJobId ) AND @ChopEmailPreferenceEntryServiceJobId IS NOT NULL
BEGIN
INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
VALUES (1, @AttributeId, @ChopEmailPreferenceEntryServiceJobId, N'B3C076C7-1325-4453-9549-456C23702069^UnsubscribefromListText', NEWID());
END

DECLARE @ChopGroupScheduleV1ServiceJobId int = (SELECT [Id] FROM [ServiceJob] WHERE [Guid] = '{ServiceJob.DATA_MIGRATIONS_161_SWAP_BLOCK_GROUP_SCHEDULE_TOOLBOX_V1}');
IF NOT EXISTS ( SELECT * FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @ChopGroupScheduleV1ServiceJobId ) AND @ChopGroupScheduleV1ServiceJobId IS NOT NULL
BEGIN
INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
VALUES (1, @AttributeId, @ChopGroupScheduleV1ServiceJobId, N'7F9CEA6F-DCE5-4F60-A551-924965289F1D^SignupInstructions,DeclineReasonPage,FutureWeeksToShow,EnableSignup', NEWID());
END

DECLARE @ChopGroupScheduleV2ServiceJobId int = (SELECT [Id] FROM [ServiceJob] WHERE [Guid] = '{ServiceJob.DATA_MIGRATIONS_161_CHOP_BLOCK_GROUP_SCHEDULE_TOOLBOX_V2}');
IF NOT EXISTS ( SELECT * FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @ChopGroupScheduleV2ServiceJobId ) AND @ChopGroupScheduleV2ServiceJobId IS NOT NULL
BEGIN
INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
VALUES (1, @AttributeId, @ChopGroupScheduleV2ServiceJobId, N'18A6DCE3-376C-4A62-B1DD-5E5177C11595^SignupInstructions,DeclineReasonPage', NEWID());
END

DECLARE @SwapFinancialBatchListServiceJobId int = (SELECT [Id] FROM [ServiceJob] WHERE [Guid] = '7750ECFD-26E3-49DE-8E90-1B1A6DCCC3FE');
IF NOT EXISTS ( SELECT * FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @SwapFinancialBatchListServiceJobId ) AND @SwapFinancialBatchListServiceJobId IS NOT NULL
BEGIN
INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
VALUES (1, @AttributeId, @SwapFinancialBatchListServiceJobId, N'AB345CE7-5DC6-41AF-BBDC-8D23D52AFE25^SummaryLavaTemplate', NEWID());
END

DECLARE @ChopAccountEntryLoginServiceJobId int = (SELECT [Id] FROM [ServiceJob] WHERE [Guid] = 'A65D26C1-229E-4198-B388-E269C3534BC0');
IF NOT EXISTS ( SELECT * FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @ChopAccountEntryLoginServiceJobId ) AND @ChopAccountEntryLoginServiceJobId IS NOT NULL
BEGIN
INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
VALUES (1, @AttributeId, @ChopAccountEntryLoginServiceJobId, N'7B83D513-1178-429E-93FF-E76430E038E4^RemoteAuthorizationTypes', NEWID() );
END

DECLARE @ChopGroupRegistrationServiceJobId int = (SELECT [Id] FROM [ServiceJob] WHERE [Guid] = '72D9EC04-517A-4CA0-B631-9F9A41F1790D');
IF NOT EXISTS ( SELECT * FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @ChopGroupRegistrationServiceJobId ) AND @ChopGroupRegistrationServiceJobId IS NOT NULL
BEGIN
INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
VALUES (1, @AttributeId, @ChopGroupRegistrationServiceJobId, N'9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7^EnableDebug', NEWID() );
END

DECLARE @ChopBlockTypesGroup1 int = (SELECT [Id] FROM [ServiceJob] WHERE [Guid] = '54FACAE5-2175-4FE0-AC9F-5CDA957BCFB5');
IF NOT EXISTS ( SELECT * FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @ChopBlockTypesGroup1 ) AND @ChopBlockTypesGroup1 IS NOT NULL
BEGIN
INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
VALUES (1, @AttributeId, @ChopBlockTypesGroup1, N'CE34CE43-2CCF-4568-9AEB-3BE203DB3470^DetailPage', NEWID() );
END

DECLARE @SwapNoteBlockTypeServiceJobId int = (SELECT [Id] FROM [ServiceJob] WHERE [Guid] = '8390C1AC-88D6-474A-AC05-8FFBD358F75D');
IF NOT EXISTS ( SELECT * FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @SwapNoteBlockTypeServiceJobId )  AND @SwapNoteBlockTypeServiceJobId IS NOT NULL
BEGIN
INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
VALUES (1, @AttributeId, @SwapNoteBlockTypeServiceJobId, N'2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3^NoteViewLavaTemplate,NoteType', NEWID() );
END

DECLARE @ChopFamilyPreRegistrationServiceJobId int = (SELECT [Id] FROM [ServiceJob] WHERE [Guid] = '{ServiceJob.DATA_MIGRATIONS_152_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_BLOCKS}');
IF NOT EXISTS ( SELECT * FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @ChopFamilyPreRegistrationServiceJobId ) AND @ChopFamilyPreRegistrationServiceJobId IS NOT NULL
BEGIN
INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
VALUES (1, @AttributeId, @ChopFamilyPreRegistrationServiceJobId, N'463A454A-6370-4B4A-BCA1-415F2D9B0CB7^Columns,ShowCampusType,ShowCampusStatus', NEWID() );
END
" );
        }

        /// <summary>
        /// DH: Add new Birth Month Range attribute for check-in.
        /// Adds the birth month range attribute.
        /// </summary>
        private void AddBirthMonthRangeAttributeUp()
        {
            var checkInByAgeGroupTypeGuid = "0572A5FE-20A4-4BF1-95CD-C71DB5281392";
            var checkInCategoryGuid = "C8E0FD8D-3032-4ACD-9DB9-FF70B11D6BCC";
            var order = SqlScalar( $"SELECT [Order] FROM [Attribute] WHERE [Guid] = '{SystemGuid.Attribute.GROUP_BIRTHDATE_RANGE}'" ) as int?;

            // Adjust the order of all attributes with a later order so
            // they are pushed back by one to make room for the new attribute.
            if ( order.HasValue )
            {
                Sql( $@"
            DECLARE @EntityTypeId int
            SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Group')

            DECLARE @GroupTypeId int
            SET @GroupTypeId = (SELECT [Id] FROM [GroupType] WHERE [Guid] = '{checkInByAgeGroupTypeGuid}')

            UPDATE [Attribute]
            SET [Order] = [Order] + 1
            WHERE [EntityTypeId] = @EntityTypeId
            AND [EntityTypeQualifierColumn] = 'GroupTypeId'
            AND [EntityTypeQualifierValue] = CAST(@GroupTypeId AS VARCHAR)
            AND [Order] > {order.Value}" );
            }

            // Add or update the attribute on the "Check in by Age" group type.
            RockMigrationHelper.AddGroupTypeGroupAttribute(
                checkInByAgeGroupTypeGuid,
                SystemGuid.FieldType.INTEGER_RANGE,
                "Birth Month Range",
                "The inclusive birth month range (1-12) allowed to check in to this group. Only supported in NextGen check-in.",
                order.HasValue ? order.Value + 1 : 0,
                string.Empty,
                SystemGuid.Attribute.GROUP_BIRTH_MONTH_RANGE );

            // Add the attribute to the "Check-in" category.
            Sql( $@"
        DECLARE @AttributeId int
        SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{SystemGuid.Attribute.GROUP_BIRTH_MONTH_RANGE}')

        DECLARE @CategoryId int
        SET @CategoryId = (SELECT [Id] FROM [Category] WHERE [Guid] = '{checkInCategoryGuid}')

        IF NOT EXISTS (
            SELECT *
            FROM [AttributeCategory]
            WHERE [AttributeId] = @AttributeId
            AND [CategoryId] = CategoryId )
        BEGIN
            INSERT INTO [AttributeCategory] ( [AttributeId], [CategoryId] )
            VALUES( @AttributeId, @CategoryId )
        END" );

            // Update the help text of the existing Age Range attribute.
            Sql( $@"
        UPDATE [Attribute]
        SET [Description] = 'The age range allowed to check in to this group. If both Age Range and Birthdate Range are specified then individuals are only required to match one of the ranges.'
        WHERE [Guid] = '{SystemGuid.Attribute.GROUP_AGE_RANGE}'" );

            // Update the help text of the existing Birthdate Range attribute.
            Sql( $@"
        UPDATE [Attribute]
        SET [Description] = 'The birth date range allowed to check in to this group. If both Age Range and Birthdate Range are specified then individuals are only required to match one of the ranges.'
        WHERE [Guid] = '{SystemGuid.Attribute.GROUP_BIRTHDATE_RANGE}'" );
        }

        /// <summary>
        /// DH: Add new Birth Month Range attribute for check-in.
        /// Removes the birth month range attribute.
        /// </summary>
        private void AddBirthMonthRangeAttributeDown()
        {
            // Revert the help text of the existing Age Range attribute.
            Sql( $@"
        UPDATE [Attribute]
        SET [Description] = 'The age range allowed to check in to these group types.'
        WHERE [Guid] = '{SystemGuid.Attribute.GROUP_AGE_RANGE}'" );

            // Revert the help text of the existing Birthdate Range attribute.
            Sql( $@"
        UPDATE [Attribute]
        SET [Description] = 'The birth date range allowed to check in to these group types.'
        WHERE [Guid] = '{SystemGuid.Attribute.GROUP_BIRTHDATE_RANGE}'" );

            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.GROUP_BIRTH_MONTH_RANGE );
        }

        /// <summary>
        /// DH: Add Saved Check-in Configurations Defined Type.
        /// Adds the Saved Check-in Configuration defined type.
        /// </summary>
        private void AddSavedCheckInConfigurationsDefinedTypeUp()
        {
            RockMigrationHelper.AddDefinedType(
                "Check-in",
                "Saved Check-in Configurations",
                "The common configurations that have been saved for use with the check-in kiosk.",
                SystemGuid.DefinedType.SAVED_KIOSK_TEMPLATES );

            RockMigrationHelper.AddDefinedTypeAttribute(
                SystemGuid.DefinedType.SAVED_KIOSK_TEMPLATES,
                SystemGuid.FieldType.CAMPUSES,
                "Campuses",
                "Campuses",
                "Which campuses this configuration is valid for. It will be available for all campuses if not set.",
                0,
                true,
                string.Empty,
                false,
                false,
                "764AB4C0-631F-479D-A9AE-1333CFDB7CFD" );

            RockMigrationHelper.AddDefinedTypeAttribute(
                SystemGuid.DefinedType.SAVED_KIOSK_TEMPLATES,
                SystemGuid.FieldType.CODE_EDITOR,
                "Settings",
                "SettingsJson",
                "The JSON encoded settings for this configuration.",
                1,
                false,
                string.Empty,
                false,
                false,
                "B457F305-46AA-4066-9862-DD76E8C192A5" );

            RockMigrationHelper.AddAttributeQualifier(
                "B457F305-46AA-4066-9862-DD76E8C192A5",
                "editorMode",
                "4", // JavaScript
                "3D0BB6AF-A117-42D6-9B08-5ED3FC5A5A3C" );
        }

        /// <summary>
        /// DH: Add Saved Check-in Configurations Defined Type.
        /// Removes the Saved Check-in Configuration defined type.
        /// </summary>
        private void AddSavedCheckInConfigurationsDefinedTypeDown()
        {
            RockMigrationHelper.DeleteDefinedType( SystemGuid.DefinedType.SAVED_KIOSK_TEMPLATES );
        }

        /// <summary>
        /// SK: Added Device Ads Content Channel Type
        /// SK:  Migrationfor Added Device Ads Content Channel Type
        /// </summary>
        private void AddDeviceAdsContentChannelType()
        {
            Sql( @"
-- Add new Content Channel Type for Device Ads
IF NOT EXISTS( SELECT * FROM [ContentChannelType] WHERE [Guid] = '09F46D0D-72BC-4445-90E7-C256E4778666' )
BEGIN
    INSERT INTO [ContentChannelType]
           ([IsSystem], [Name], [DateRangeType], [Guid], [DisablePriority], [IncludeTime], [DisableContentField], [DisableStatus], [ShowInChannelList])
     VALUES
           (1, 'Device Ads', 2, '09F46D0D-72BC-4445-90E7-C256E4778666', 1, 1, 1, 0, 1)
END

-- Add new Content Channel for Default Check-in Kiosk Ads
IF NOT EXISTS( SELECT * FROM [ContentChannel] WHERE [Guid] = 'A57BDBCD-FA77-4A6E-967D-1C5ACE962587' )
BEGIN

DECLARE @DeviceAdsContentChannelTypeId int = (SELECT TOP 1 [Id] FROM [ContentChannelType] WHERE [Guid] = '09F46D0D-72BC-4445-90E7-C256E4778666')

INSERT INTO [dbo].[ContentChannel]
           ([ContentChannelTypeId], [Name], [Description], [IconCssClass], [RequiresApproval], [EnableRss], [ChannelUrl], [ItemUrl], [TimeToLive], [Guid], [ContentControlType], [RootImageDirectory], [ItemsManuallyOrdered], [ChildItemsManuallyOrdered], [IsIndexEnabled], [IsTaggingEnabled], [ItemTagCategoryId], [IsStructuredContent])
     VALUES
           ( @DeviceAdsContentChannelTypeId, 'Default Check-in Kiosk Ads', 'The default collection of advertisements to display on check-in kiosks. These will appear on the welcome screen while waiting for somebody to start the check-in process.', '', 0, 0, '', '', 0, 'A57BDBCD-FA77-4A6E-967D-1C5ACE962587', 0, '', 1, 0, 0, 0, null, 0)

END" );
            // Because an EF migration had to be reverted before deployment, but after commit 
            // Some developers who've pulled these commits may need delete these attributes
            // if they exist before being able to execute this EF migration.
            RockMigrationHelper.DeleteAttribute( "268220a5-01b2-4aa9-a75d-94a4ac7a30f6" );
            RockMigrationHelper.DeleteAttribute( "53304137-7C16-4DBD-B5FB-EA68B7E7DDA2" );
            RockMigrationHelper.DeleteAttribute( "AF39FF1E-A3B9-4C8B-B135-C479CD57574E" );

            // Entity: Rock.Model.ContentChannelItem Attribute: Image
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ContentChannelItem", "97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D", "ContentChannelTypeId", "", "Image", "Image", @"This image will be displayed on the check-in device screen. We recommend a resolution of 2200x1400 for best results.", 0, @"", "268220A5-01B2-4AA9-A75D-94A4AC7A30F6", "Image" );
            // Entity: Rock.Model.ContentChannelItem Attribute: Display Duration
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ContentChannelItem", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "ContentChannelTypeId", "", "Display Duration", "Display Duration", @"The duration in seconds that this image will stay on screen before moving to the next one.", 1, @"15", "53304137-7C16-4DBD-B5FB-EA68B7E7DDA2", "DisplayDuration" );
            // Entity: Rock.Model.ContentChannelItem Attribute: Campuses
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ContentChannelItem", "69254F91-C97F-4C2D-9ACB-1683B088097B", "ContentChannelTypeId", "", "Campuses", "Campuses", @"If any campuses are selected here then only kiosks determined to reside at that campus will show this image.", 2, @"", "AF39FF1E-A3B9-4C8B-B135-C479CD57574E", "Campuses" );

            Sql( @"
                UPDATE [Attribute] SET [IsRequired] = 1 WHERE [Guid] = '268220A5-01B2-4AA9-A75D-94A4AC7A30F6'

                DECLARE @ContentChannelItemEntityTypeId int = ( SELECT TOP 1[Id] FROM[EntityType] WHERE[Name] = 'Rock.Model.ContentChannelItem' )
                DECLARE @DeviceAdsContentChannelTypeId int = ( SELECT TOP 1[Id] FROM [ContentChannelType] WHERE [Guid] = '09F46D0D-72BC-4445-90E7-C256E4778666' )
                IF @ContentChannelItemEntityTypeId IS NOT NULL AND @DeviceAdsContentChannelTypeId IS NOT NULL
                BEGIN
                    UPDATE [Attribute] SET [EntityTypeQualifierValue] = CAST( @DeviceAdsContentChannelTypeId AS varchar )
                    WHERE [EntityTypeId] = @ContentChannelItemEntityTypeId
                    AND [EntityTypeQualifierColumn] = 'ContentChannelTypeId'
                    AND [Guid] IN ('268220A5-01B2-4AA9-A75D-94A4AC7A30F6', '53304137-7C16-4DBD-B5FB-EA68B7E7DDA2', 'AF39FF1E-A3B9-4C8B-B135-C479CD57574E')
                END
            " );

            // Qualifier for attribute: Image
            RockMigrationHelper.UpdateAttributeQualifier( "268220A5-01B2-4AA9-A75D-94A4AC7A30F6", "binaryFileType", @"c1142570-8cd6-4a20-83b1-acb47c1cd377", "69FBD646-5DB8-4C8D-AF00-DFDC3E9FFDE0" );
            // Qualifier for attribute: Image
            RockMigrationHelper.UpdateAttributeQualifier( "268220A5-01B2-4AA9-A75D-94A4AC7A30F6", "formatAsLink", @"False", "4B595316-3EF9-47BD-99C4-281C4C1D3458" );
            // Qualifier for attribute: Image
            RockMigrationHelper.UpdateAttributeQualifier( "268220A5-01B2-4AA9-A75D-94A4AC7A30F6", "img_tag_template", @"", "CC41CD75-5A69-464B-AF1C-656A49883653" );
            // Qualifier for attribute: Campuses
            RockMigrationHelper.UpdateAttributeQualifier( "AF39FF1E-A3B9-4C8B-B135-C479CD57574E", "enhancedselection", @"False", "6F48C59A-F49B-4D88-BC02-35F910AF2068" );
            // Qualifier for attribute: Campuses
            RockMigrationHelper.UpdateAttributeQualifier( "AF39FF1E-A3B9-4C8B-B135-C479CD57574E", "filterCampusStatus", @"", "5588198F-A6CC-4450-A5EA-D28D30165C86" );
            // Qualifier for attribute: Campuses
            RockMigrationHelper.UpdateAttributeQualifier( "AF39FF1E-A3B9-4C8B-B135-C479CD57574E", "filterCampusTypes", @"", "9003C098-F6A6-4BCA-AE7A-4E69D2A26492" );
            // Qualifier for attribute: Campuses
            RockMigrationHelper.UpdateAttributeQualifier( "AF39FF1E-A3B9-4C8B-B135-C479CD57574E", "includeInactive", @"False", "FDFCABD5-9A67-4BF8-97ED-176127340451" );
            // Qualifier for attribute: Campuses
            RockMigrationHelper.UpdateAttributeQualifier( "AF39FF1E-A3B9-4C8B-B135-C479CD57574E", "repeatColumns", @"", "38248DD1-1582-4D61-9118-3D1824E9E498" );
            // Qualifier for attribute: Campuses
            RockMigrationHelper.UpdateAttributeQualifier( "AF39FF1E-A3B9-4C8B-B135-C479CD57574E", "SelectableCampusIds", @"", "6FA069FE-5D04-4CAA-B45D-87117266ED87" );
        }
    }
}
