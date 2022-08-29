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
    public partial class Rollup_0609 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Update_ufnCrm_GetAddress();
            AddMetricDetailDataViewDetailAttributeUp();
            FixGroupViewLavaTemplateDQ();
            AddPrayerRequestCommentsNotificationToSystemCommunicationUp();
            AddGivingStatementDocumentAndFileTypesUp();
            BlockAndFieldTypeMigrations();
            WindowsCheckinClientDownloadLinkUp();
            UpdateStatementGeneratorDownloadLinkUp();
            AddBlockSettingToGroupScheduleToolboxv2Up();
            UpdateGroupTypeDefaultLavaUp();
            UpdatePanelLavaShortCodeDefault();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            WindowsCheckinClientDownloadLinkDown();
            UpdateStatementGeneratorDownloadLinkDown();
        }

        /// <summary>
        /// KA: Migration to Update ufnCrm_GetAddress to use PrimaryFamilyId to get address
        /// </summary>
        private void Update_ufnCrm_GetAddress()
        {
            Sql(@"
/*
<doc>
    <summary>
         This function returns the address of the person provided.
    </summary>

    <returns>
        Address of the person.
    </returns>
    <remarks>
        This function allows you to request an address for a specific person. It will return
        the first address of that type (multiple address are possible if the individual is in
        multiple families). 
        
        You can provide the address type by specifing 'Home', 'Previous', 
        'Work'. For custom address types provide the AddressTypeId like '19'.

        You can also determine which component of the address you'd like. Values include:
            + 'Full' - the full address 
            + 'Street1'
            + 'Street2'
            + 'City'
            + 'State'
            + 'PostalCode'
            + 'Country'
            + 'Latitude'
            + 'Longitude'

    </remarks>
    <code>
        SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'Full')
        SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'Street1')
        SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'Street2')
        SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'City')
        SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'State')
        SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'PostalCode')
        SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'Country')
        SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'Latitude')
        SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'Longitude')
    </code>
</doc>
*/

ALTER FUNCTION [dbo].[ufnCrm_GetAddress](
    @PersonId int,
    @AddressType varchar(20),
    @AddressComponent varchar(20)) 

RETURNS nvarchar(500) AS

BEGIN
    DECLARE @AddressTypeId int,
        @Address nvarchar(500)

    -- get address type
    IF (@AddressType = 'Home')
        BEGIN
        SET @AddressTypeId = 19
        END
    ELSE IF (@AddressType = 'Work')
        BEGIN
        SET @AddressTypeId = 20
        END
    ELSE IF (@AddressType = 'Previous')
        BEGIN
        SET @AddressTypeId = 137
        END
    ELSE
        BEGIN
        SET @AddressTypeId = CAST(@AddressType AS int)
        END

    -- return address component
    IF (@AddressComponent = 'Street1')
        BEGIN
        SET @Address = (SELECT [Street1] FROM [Location] WHERE [Id] = (SELECT TOP 1 [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [PrimaryFamilyId] FROM [Person] WHERE [Id] = @PersonId) ))
        END
    ELSE IF (@AddressComponent = 'Street2')
        BEGIN
        SET @Address = (SELECT [Street2] FROM [Location] WHERE [Id] = (SELECT TOP 1 [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [PrimaryFamilyId] FROM [Person] WHERE [Id] = @PersonId) )) 
        END
    ELSE IF (@AddressComponent = 'City')
        BEGIN
        SET @Address = (SELECT [City] FROM [Location] WHERE [Id] = (SELECT TOP 1 [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [PrimaryFamilyId] FROM [Person] WHERE [Id] = @PersonId) )) 
        END
    ELSE IF (@AddressComponent = 'State')
        BEGIN
        SET @Address = (SELECT [State] FROM [Location] WHERE [Id] = (SELECT TOP 1 [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [PrimaryFamilyId] FROM [Person] WHERE [Id] = @PersonId) )) 
 END
    ELSE IF (@AddressComponent = 'PostalCode')
        BEGIN
        SET @Address = (SELECT [PostalCode] FROM [Location] WHERE [Id] = (SELECT TOP 1 [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [PrimaryFamilyId] FROM [Person] WHERE [Id] = @PersonId) )) 
        END
    ELSE IF (@AddressComponent = 'Country')
        BEGIN
        SET @Address = (SELECT [Country] FROM [Location] WHERE [Id] = (SELECT TOP 1 [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [PrimaryFamilyId] FROM [Person] WHERE [Id] = @PersonId) )) 
        END
    ELSE IF (@AddressComponent = 'Latitude')
        BEGIN
        SET @Address = (SELECT [GeoPoint].[Lat] FROM [Location] WHERE [Id] = (SELECT TOP 1 [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [PrimaryFamilyId] FROM [Person] WHERE [Id] = @PersonId) )) 
        END
    ELSE IF (@AddressComponent = 'Longitude')
        BEGIN
        SET @Address = (SELECT [GeoPoint].[Long] FROM [Location] WHERE [Id] = (SELECT TOP 1 [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [PrimaryFamilyId] FROM [Person] WHERE [Id] = @PersonId) )) 
        END
    ELSE 
        BEGIN
        SET @Address = (SELECT ISNULL([Street1], '') + ' ' + ISNULL([Street2], '') + ' ' + ISNULL([City], '') + ', ' + ISNULL([State], '') + ' ' + ISNULL([PostalCode], '') FROM [Location] WHERE [Id] = (SELECT TOP 1 [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [PrimaryFamilyId] FROM [Person] WHERE [Id] = @PersonId) )) 
        END

    RETURN @Address
END");
        }

        /// <summary>
        /// DV: Add Metric Detail Data View Detail block attribute
        /// </summary>
        private void AddMetricDetailDataViewDetailAttributeUp()
        {
            // Attrib for BlockType: Metric Detail :Data View Detail
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D77341B9-BA38-4693-884E-E5C1D908CEC4",
                "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108",
                "Data View Page",
                "DataViewPage",
                "Data View Page",
                @"The page to edit data views.",
                4,
                @"",
                "255A0555-D798-47B9-8EE9-975015AEEFE6" );

            RockMigrationHelper.AddBlockAttributeValue( true,
                "F85FE71D-927D-45AF-B419-02A8909C6E72",
                "255A0555-D798-47B9-8EE9-975015AEEFE6",
                "4011CB37-28AA-46C4-99D5-826F4A9CADF5" ); // Rock.Client.SystemGuid.Page.DATA_VIEWS
        }

        /// <summary>
        /// NA: Fix Double Quotes in Group Type Lava for Groups Created between v13.5 and v12.4
        /// </summary>
        private void FixGroupViewLavaTemplateDQ()
        {
            // Replaces the double-single-quote ''warning'' with just a single quote 'warning' in the GroupType's GroupViewLavaTemplate
            Sql( @"
            UPDATE [GroupType]
            SET [GroupViewLavaTemplate] = REPLACE(GroupViewLavaTemplate,'''''warning''''','''warning''')
            WHERE [GroupViewLavaTemplate] like '%warningLevel = ''''warning''''%'" );
        }

        /// <summary>
        /// KA: Data Migration to AddPrayerRequestCommentsNotification to SystemCommunication
        /// </summary>
        private void AddPrayerRequestCommentsNotificationToSystemCommunicationUp()
        {
            Sql(  $@"
                DECLARE @Id int
                SET @Id = (SELECT [Id] FROM [SystemCommunication] WHERE [guid] = '{SystemGuid.SystemCommunication.PRAYER_REQUEST_COMMENTS_NOTIFICATION}')
                IF @Id IS NULL
                BEGIN
                    INSERT INTO [SystemCommunication]
                    (
                      [IsSystem]
                      ,[Title]
                      ,[From]
                      ,[To]
                      ,[Cc]
                      ,[Bcc]
                      ,[Subject]
                      ,[Body]
                      ,[Guid]
                      ,[CreatedDateTime]
                      ,[ModifiedDateTime]
                      ,[CreatedByPersonAliasId]
                      ,[ModifiedByPersonAliasId]
                      ,[FromName]
                      ,[ForeignKey]
                      ,[CategoryId]
                      ,[ForeignGuid]
                      ,[ForeignId]
                    )
                    SELECT
                      [IsSystem]
                      ,[Title]
                      ,[From]
                      ,[To]
                      ,[Cc]
                      ,[Bcc]
                      ,[Subject]
                      ,[Body]
                      ,[Guid]
                      ,[CreatedDateTime]
                      ,[ModifiedDateTime]
                      ,[CreatedByPersonAliasId]
                      ,[ModifiedByPersonAliasId]
                      ,[FromName]
                      ,[ForeignKey]
                      ,[CategoryId]
                      ,[ForeignGuid]
                      ,[ForeignId]
                    FROM [SystemEmail]
                    WHERE [guid] = '{SystemGuid.SystemCommunication.PRAYER_REQUEST_COMMENTS_NOTIFICATION}'
                    END
            "  );
        }

        /// <summary>
        /// DV: Add Giving Statement Document And File Types
        /// </summary>
        private void AddGivingStatementDocumentAndFileTypesUp()
        {
	        var rockStorageDatabaseProviderGuid = "0AA42802-04FD-4AEC-B011-FEB127FC85CD"; // Rock.SystemGuid.EntityType.STORAGE_PROVIDER_DATABASE
	        var givingStatementBinaryFileGuid = "1733C78D-344B-405E-8CA0-9F062CBD6AB0";
	        var givingStatementDocumentTypeGuid = "E8513F11-165D-4EDB-AC27-9204B84FB016";

	        RockMigrationHelper.UpdateBinaryFileTypeRecord( rockStorageDatabaseProviderGuid, "Giving Statement", "File related to generated giving statements used by the Statement Generator app and related workflow action.", "fa fa-heart", givingStatementBinaryFileGuid, false, true );

	        // Add the same security as Financial Transcation Image.
	        // BinaryFileType: Giving Statement, Group: <all users>.
	        RockMigrationHelper.AddSecurityAuthForBinaryFileType( givingStatementBinaryFileGuid, 3, "View", false, "", Model.SpecialRole.AllUsers, "F1346FFD-D1DB-426D-95CB-9A47394FA728" );
	        // BinaryFileType: Giving Statement, Group: 628C51A8-4613-43ED-A18D-4A6FB999273E ( RSR - Rock Administration ).
	        RockMigrationHelper.AddSecurityAuthForBinaryFileType( givingStatementBinaryFileGuid, 2, "View", true, "628C51A8-4613-43ED-A18D-4A6FB999273E", Model.SpecialRole.None, "A4181C9A-BA06-41BB-9F3C-F049FC3D8BDD" );
	        // BinaryFileType: Giving Statement, Group: 2539CF5D-E2CE-4706-8BBF-4A9DF8E763E9 ( RSR - Finance Worker ).
	        RockMigrationHelper.AddSecurityAuthForBinaryFileType( givingStatementBinaryFileGuid, 1, "View", true, "2539CF5D-E2CE-4706-8BBF-4A9DF8E763E9", Model.SpecialRole.None, "E0539428-9BF0-4CE7-B99D-0D2AB6F3F480" );
	        // BinaryFileType: Giving Statement, Group: 6246A7EF-B7A3-4C8C-B1E4-3FF114B84559 ( RSR - Finance Administration ).
	        RockMigrationHelper.AddSecurityAuthForBinaryFileType( givingStatementBinaryFileGuid, 0, "View", true, "6246A7EF-B7A3-4C8C-B1E4-3FF114B84559", Model.SpecialRole.None, "6FE9A2E5-8BDF-479E-8BED-DC68DC580B05" );

	        // BinaryFileType: Giving Statement, Group: 2539CF5D-E2CE-4706-8BBF-4A9DF8E763E9 ( RSR - Finance Worker ).
	        RockMigrationHelper.AddSecurityAuthForBinaryFileType( givingStatementBinaryFileGuid, 1, "Edit", true, "2539CF5D-E2CE-4706-8BBF-4A9DF8E763E9", Model.SpecialRole.None, "B7F4548D-F1F4-4690-9C41-4B4697961EA8" );
	        // BinaryFileType: Giving Statement, Group: 6246A7EF-B7A3-4C8C-B1E4-3FF114B84559 ( RSR - Finance Administration ).
	        RockMigrationHelper.AddSecurityAuthForBinaryFileType( givingStatementBinaryFileGuid, 0, "Edit", true, "6246A7EF-B7A3-4C8C-B1E4-3FF114B84559", Model.SpecialRole.None, "D753305E-C91B-43A9-95D0-DC5A7786C8AE" );

	        // Not using string interpolation here because of the lava template.
	        Sql( $@"DECLARE @BinaryFileTypeGuid UNIQUEIDENTIFIER = '{givingStatementBinaryFileGuid}';
		        DECLARE @GivingStatementGuid UNIQUEIDENTIFIER = '{givingStatementDocumentTypeGuid}';

		        DECLARE @EntityTypeId INT = (
			        SELECT TOP 1 [Id]
			        FROM [EntityType]
			        WHERE [Name] = 'Rock.Model.Person'
			        )

		        DECLARE @BinaryFileTypeId INT = (
			        SELECT TOP 1 [Id]
			        FROM [BinaryFileType]
			        WHERE [Guid] = @BinaryFileTypeGuid
			        )

		        IF NOT EXISTS (
			        SELECT 1
			        FROM [DocumentType]
			        WHERE [Guid] = @GivingStatementGuid )
		        BEGIN
			        INSERT INTO [DocumentType]
				        ([IsSystem], [Name], [IconCssClass], [EntityTypeId], [BinaryFileTypeId], [DefaultDocumentNameTemplate], [UserSelectable], [Order], [Guid])
			        VALUES
				        (1, 'Giving Statement', 'fa fa-heart', @EntityTypeId, @BinaryFileTypeId
				        ,'{{{{ NickName }}}} {{{{ LastName }}}} - {{{{ DocumentTypeName }}}} - {{{{ DocumentPurposeKey }}}}'
				        ,1, 0, @GivingStatementGuid)
		        END" );
        }

        /// <summary>
        /// Autogenerated block and field type migrations.
        /// </summary>
        private void BlockAndFieldTypeMigrations()
        {
            // Add/Update Obsidian Block Type
            //   Name:Attributes
            //   Category:Obsidian > Core
            //   EntityType:Rock.Blocks.Core.Attributes
            RockMigrationHelper.UpdateMobileBlockType("Attributes", "Allows for the managing of attributes.", "Rock.Blocks.Core.Attributes", "Obsidian > Core", "7BEE29A5-212A-4992-A882-56F27452E873");

            // Attribute for BlockType
            //   BlockType: Group Schedule Confirmation
            //   Category: Group Scheduling
            //   Attribute: Decline Message Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B783DEC7-E2B7-4805-B2DD-B5EDF6495A2C", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Decline Message Template", "DeclineMessageTemplate", "Decline Message Template", @"Message to display when a person declines a schedule RSVP. <span class='tip tip-lava'></span>", 3, @"<div class='alert alert-success'><strong>Thank You</strong> We’ll try to schedule another person for: {{ ScheduledItem.Occurrence.Group.Name }}.</div>", "DFE0AA2B-9067-4D25-8F26-627E1B8E2A3C" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Security
            //   Attribute: Save Communication History
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "99362B60-71A5-44C6-BCFE-DDA9B00CC7F3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Save Communication History", "CreateCommunicationRecord", "Save Communication History", @"Should a record of communication from this block be saved to the recipient's profile?", 24, @"False", "D787795A-156C-4F98-B7E5-7993D1DD379D" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Security
            //   Attribute: Show Gender
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "99362B60-71A5-44C6-BCFE-DDA9B00CC7F3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Gender", "ShowGender", "Show Gender", @"Determines if the gender selection field should be shown.", 23, @"True", "BD966496-8EDB-4270-9921-9CA24EBCDB8E" );

            // Attribute for BlockType
            //   BlockType: Forgot Username
            //   Category: Security
            //   Attribute: Save Communication History
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "02B3D7D1-23CE-4154-B602-F4A15B321757", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Save Communication History", "CreateCommunicationRecord", "Save Communication History", @"Should a record of communication from this block be saved to the recipient's profile?", 5, @"False", "232270C1-C9D5-4B88-AAE0-4F29A711587B" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7BEE29A5-212A-4992-A882-56F27452E873", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity", "Entity", "Entity", @"Entity Name", 0, @"", "AA348FC4-5D39-4994-9BC9-A53E2250A156" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7BEE29A5-212A-4992-A882-56F27452E873", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Column", "EntityQualifierColumn", "Entity Qualifier Column", @"The entity column to evaluate when determining if this attribute applies to the entity", 1, @"", "F0396D06-20A5-48C7-B37C-4C8072AC8B48" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Value
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7BEE29A5-212A-4992-A882-56F27452E873", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Value", "EntityQualifierValue", "Entity Qualifier Value", @"The entity column value to evaluate.  Attributes will only apply to entities with this value", 2, @"", "F88B480F-0A49-40D2-B640-3375FF7A03BB" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Allow Setting of Values
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7BEE29A5-212A-4992-A882-56F27452E873", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Setting of Values", "AllowSettingofValues", "Allow Setting of Values", @"Should UI be available for setting values of the specified Entity ID?", 3, @"false", "CAD33F6D-DBDD-4A1C-ACAF-E8B6853F8C8C" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Id
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7BEE29A5-212A-4992-A882-56F27452E873", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Entity Id", "EntityId", "Entity Id", @"The entity id that values apply to", 4, @"0", "0324E357-78E3-4D90-AA9D-346371E53EDD" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Enable Show In Grid
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7BEE29A5-212A-4992-A882-56F27452E873", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Show In Grid", "EnableShowInGrid", "Enable Show In Grid", @"Should the 'Show In Grid' option be displayed when editing attributes?", 5, @"false", "3D9E970F-2A00-4BF7-801C-55ABCB79DAC7" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Category Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7BEE29A5-212A-4992-A882-56F27452E873", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Category Filter", "CategoryFilter", "Category Filter", @"A comma separated list of category GUIDs to limit the display of attributes to.", 6, @"", "2D87A83C-1E3B-4F73-BBDF-4022CFBF4A90" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Hide Columns on Grid
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7BEE29A5-212A-4992-A882-56F27452E873", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Hide Columns on Grid", "HideColumnsOnGrid", "Hide Columns on Grid", @"The grid columns that should be hidden.", 7, @"", "4C5AAB42-2128-440E-BF8E-3B039A184D00" );

            // Attribute for BlockType
            //   BlockType: Schedule Preferences
            //   Category: Mobile > Groups
            //   Attribute: Schedule Preference Landing Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6D0A258-F97E-4561-B881-ACBF985F89DC", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Schedule Preference Landing Template", "LandingTemplate", "Schedule Preference Landing Template", @"The XAML passed into the landing page, where the user's groups are listed.", 0, @"C3A98DBE-E977-499C-B823-0B3676731E48", "A93E4436-B226-4911-8C9C-780D9F83C2A5" );

            // Attribute for BlockType
            //   BlockType: Schedule Sign Up
            //   Category: Mobile > Groups
            //   Attribute: Schedule Preference Landing Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CA27CB14-22FD-4DE6-9C3B-0EAA0AA84708", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Schedule Preference Landing Template", "LandingTemplate", "Schedule Preference Landing Template", @"The XAML passed into the landing page, where the user's groups are listed.", 0, @"C4BFED3A-C2A1-4A68-A646-44C3B499C75A", "B89B35C0-EC4C-43B9-89FF-13B67D5EF296" );

            // Attribute for BlockType
            //   BlockType: Schedule Sign Up
            //   Category: Mobile > Groups
            //   Attribute: Future Weeks To Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CA27CB14-22FD-4DE6-9C3B-0EAA0AA84708", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Future Weeks To Show", "FutureWeeksToShow", "Future Weeks To Show", @"The amount of weeks in the future you would like to display scheduling opportunities.", 1, @"6", "0894056C-B20F-4C80-9505-9BE289FC86F6" );

            // Attribute for BlockType
            //   BlockType: Schedule Toolbox
            //   Category: Mobile > Groups
            //   Attribute: Toolbox Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E00F3C6D-D007-4408-8A41-AD2A6AB29D6E", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Toolbox Template", "ToolboxTemplate", "Toolbox Template", @"The template used to render the scheduling data.", 0, @"CD2629E5-8EB0-4D52-ACAB-8EDF9AF84814", "21CC1DAD-87A6-4F00-A324-281E6D7190D0" );

            // Attribute for BlockType
            //   BlockType: Schedule Toolbox
            //   Category: Mobile > Groups
            //   Attribute: Confirm Decline Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E00F3C6D-D007-4408-8A41-AD2A6AB29D6E", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Confirm Decline Template", "ConfirmDeclineTemplate", "Confirm Decline Template", @"The template used on the decline reason modal. Must require a decline reason in group type.", 1, @"92D39913-7D69-4B73-8FF9-72AC161BE381", "3755EF28-B0FC-4039-8012-6EDDA4E10FFF" );

            // Attribute for BlockType
            //   BlockType: Schedule Unavailability
            //   Category: Mobile > Groups
            //   Attribute: Schedule Unavailability Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AEFF246D-A514-4D46-801E-D717E1D1D209", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Schedule Unavailability Template", "TypeTemplate", "Schedule Unavailability Template", @"The template used to render the scheduling data.", 0, @"1A699B18-AB29-4CD5-BC02-AF55159D5DA6", "476529FA-F47B-4AD7-8B8F-77E3BD72F3EA" );

            // Attribute for BlockType
            //   BlockType: Schedule Unavailability
            //   Category: Mobile > Groups
            //   Attribute: Description required?
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AEFF246D-A514-4D46-801E-D717E1D1D209", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Description required?", "IsDescriptionRequired", "Description required?", @"Whether or not the user is required to input a description.", 1, @"False", "9B34B12E-0930-44E6-AE39-FB109E64E8EF" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Additional Time Sign-Up Button Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "18A6DCE3-376C-4A62-B1DD-5E5177C11595", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Additional Time Sign-Up Button Text", "AdditionalTimeSignUpButtonText", "Additional Time Sign-Up Button Text", @"The text to display for the Additional Time Sign-Up button.", 7, @"Sign-Up for Additional Times", "9CB5A3B2-56BB-453D-9DAB-2BAD05DCFDA3" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Update Schedule Preferences Button Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "18A6DCE3-376C-4A62-B1DD-5E5177C11595", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Update Schedule Preferences Button Text", "UpdateSchedulePreferencesButtonText", "Update Schedule Preferences Button Text", @"The text to display for the Update Schedule Preferences button.", 9, @"Update Schedule Preferences", "ED187882-EA6D-4CAB-A922-32BCD61BDE0A" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Schedule Unavailability Button Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "18A6DCE3-376C-4A62-B1DD-5E5177C11595", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Schedule Unavailability Button Text", "ScheduleUnavailabilityButtonText", "Schedule Unavailability Button Text", @"The text to display for the Schedule Unavailability button.", 11, @"Schedule Unavailability", "F3935CDD-50E8-4CE2-8568-82D4693A5186" );

            // Attribute for BlockType
            //   BlockType: Dynamic Data
            //   Category: Reporting
            //   Attribute: Enable Sticky Header on Grid
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Sticky Header on Grid", "EnableStickyHeaderOnGrid", "Enable Sticky Header on Grid", @"Determines whether the header on the grid will be stick at the top of the page.", 0, @"False", "F438B5A5-78F3-44EF-9926-77193BAC0EF2" );
        }

        /// <summary>
        /// MP: Update Windows Check-in Client Download Link
        /// </summary>
        private void WindowsCheckinClientDownloadLinkUp()
        {
            Sql( @"
                DECLARE @winCheckinClientDefinedValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid] = 'C162F21E-7D86-4BB4-A72C-7F4A0E5B02C3')
                DECLARE @downloadUrlAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9')

                UPDATE [AttributeValue]
                SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/windowscheckin/1.13.0/checkinclient.msi'
                WHERE AttributeId = @downloadUrlAttributeId and EntityId = @winCheckinClientDefinedValueId" );
        }

        /// <summary>
        /// MP: Update Windows Check-in Client Download Link - Restores the old Rock Windows Check-in Client download link.
        /// </summary>
        private void WindowsCheckinClientDownloadLinkDown()
        {
            Sql( @"
                DECLARE @winCheckinClientDefinedValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid] = 'C162F21E-7D86-4BB4-A72C-7F4A0E5B02C3')
                DECLARE @downloadUrlAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9')

                UPDATE [AttributeValue]
                SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/windowscheckin/1.13.5/checkinclient.msi'
                WHERE AttributeId = @downloadUrlAttributeId and EntityId = @winCheckinClientDefinedValueId" );
        }

        /// <summary>
        /// MP: Statement Generator Download Location - Updates the statement generator download link up.
        /// </summary>
        private void UpdateStatementGeneratorDownloadLinkUp()
        {
            Sql( @"
                DECLARE @statementGeneratorDefinedValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid] = '54E1EBCC-5A5A-4B26-9CCB-36E7CEB49C3C')
                DECLARE @downloadUrlAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9')

                UPDATE [AttributeValue]
                SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/statementgenerator/1.13.5/statementgenerator.msi'
                WHERE AttributeId = @downloadUrlAttributeId and EntityId = @statementGeneratorDefinedValueId" );
        }

        /// <summary>
        /// MP: Statement Generator Download Location - Updates the statement generator download link down.
        /// </summary>
        private void UpdateStatementGeneratorDownloadLinkDown()
        {
            Sql( @"
                DECLARE @statementGeneratorDefinedValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid] = '54E1EBCC-5A5A-4B26-9CCB-36E7CEB49C3C')
                DECLARE @downloadUrlAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9')

                UPDATE [AttributeValue]
                SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/statementgenerator/1.13.1/statementgenerator.msi'
                WHERE AttributeId = @downloadUrlAttributeId and EntityId = @statementGeneratorDefinedValueId" );
        }

        /// <summary>
        /// SK: Add BlockSetting to Group Schedule Toolbox v2 and Replace Group Schedule Toolbox to Group Schedule Toolbox V2 in Person Profile Page
        /// </summary>
        private void AddBlockSettingToGroupScheduleToolboxv2Up()
        {
            // Attribute for BlockType              //   BlockType: Group Schedule Toolbox v2              //   Category: Group Scheduling              //   Attribute: Schedule Unavailability Header
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "18A6DCE3-376C-4A62-B1DD-5E5177C11595", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Schedule Unavailability Header", "ScheduleUnavailabilityHeader", "Schedule Unavailability Header", @"Header content to put on the Schedule Unavailability panel. <span class='tip tip-lava'></span>", 13, @"  <p>      <a class=""btn btn-sm btn-default"" href=""javascript:history.back()""><i class=""fa fa-chevron-left""></i> Back</a>  </p>", "B175A2F3-0A8A-48D2-B122-FDBBD7EA44C0" );
            // Attribute for BlockType              //   BlockType: Group Schedule Toolbox v2              //   Category: Group Scheduling              //   Attribute: Update Schedule Preferences Header
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "18A6DCE3-376C-4A62-B1DD-5E5177C11595", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Update Schedule Preferences Header", "UpdateSchedulePreferencesHeader", "Update Schedule Preferences Header", @"Header content to put on the Update Schedule Preferences panel. <span class='tip tip-lava'></span>", 14, @"  <p>      <a class=""btn btn-sm btn-default"" href=""javascript:history.back()""><i class=""fa fa-chevron-left""></i> Back</a>  </p>", "7AE2253D-4A0E-41A3-BDE4-34B350FD1E2E" );
            // Attribute for BlockType              //   BlockType: Group Schedule Toolbox v2              //   Category: Group Scheduling              //   Attribute: Sign-up for Additional Times Header
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "18A6DCE3-376C-4A62-B1DD-5E5177C11595", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Sign-up for Additional Times Header", "SignupforAdditionalTimesHeader", "Sign-up for Additional Times Header", @"Header content to put on the Sign-up for Additional Times panel. <span class='tip tip-lava'></span>", 15, @"  <p>      <a class=""btn btn-sm btn-default"" href=""javascript:history.back()""><i class=""fa fa-chevron-left""></i> Back</a>  </p>", "5EAC6DAB-4E16-4302-B388-E77ED31EBFBB" );
            // Attribute for BlockType              //   BlockType: Group Schedule Toolbox v2              //   Category: Group Scheduling              //   Attribute: Override Hide from Toolbox
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "18A6DCE3-376C-4A62-B1DD-5E5177C11595", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Override Hide from Toolbox", "OverrideHideFromToolbox", "Override Hide from Toolbox", @" When enabled this setting will show all schedule enabled groups no matter what their 'Disable Schedule Toolbox Access' setting is set to.", 12, @"False", "4C4E1DAD-F2D8-4F53-8A5E-762E8E2E937E" );

            Sql( @"
                DECLARE @GroupScheduleToolboxId int = ( SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = '7F9CEA6F-DCE5-4F60-A551-924965289F1D' )
                DECLARE @PageId int = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '183B7B7E-105A-4C9A-A4BC-06CD26B7FE6D' )
                DECLARE @GroupScheduleToolboxBlockId int = ( SELECT TOP 1 [Id] FROM [Block] WHERE [BlockTypeId] = @GroupScheduleToolboxId AND [PageId]=@PageId  )
                DECLARE @GroupScheduleToolboxV2Id int = ( SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = '18A6DCE3-376C-4A62-B1DD-5E5177C11595' )

                IF @GroupScheduleToolboxBlockId IS NOT NULL
                BEGIN
                        -- update block of Group Schedule Toolbox block type with Group Schedule Toolbox v2  Block Type Id
                        UPDATE 
                            [dbo].[Block]
                        SET [BlockTypeId] = @GroupScheduleToolboxV2Id
                        WHERE
                            [Id] = @GroupScheduleToolboxBlockId

                        UPDATE
                            a
                        SET a.AttributeId=c.[Id]
                        FROM [dbo].[AttributeValue] a INNER JOIN [dbo].[Attribute] b ON a.AttributeId = b.[Id] AND b.[EntityTypeQualifierColumn] = 'BlockTypeId' and b.EntityTypeQualifierValue = @GroupScheduleToolboxId
                        INNER JOIN [dbo].[Attribute] c ON c.[EntityTypeQualifierColumn] = 'BlockTypeId' AND c.[EntityTypeQualifierValue] = @GroupScheduleToolboxV2Id AND c.[Key] = b.[Key]
                        WHERE a.[EntityId] = @GroupScheduleToolboxBlockId

                        UPDATE
                            a
                        SET a.AttributeId=c.[Id]
                        FROM [dbo].[AttributeValue] a INNER JOIN [dbo].[Attribute] b ON a.AttributeId = b.[Id] AND b.[EntityTypeQualifierColumn] = 'BlockTypeId' and b.EntityTypeQualifierValue = @GroupScheduleToolboxId
                        INNER JOIN [dbo].[Attribute] c ON c.[EntityTypeQualifierColumn] = 'BlockTypeId' AND c.[EntityTypeQualifierValue] = @GroupScheduleToolboxV2Id AND c.[Key] = 'EnableAdditionalTimeSignUp' AND B.[Key]='EnableSignup'
                        WHERE a.[EntityId] = @GroupScheduleToolboxBlockId
                END
                " );

            // Block Attribute Value for Override Hide from Toolbox
            RockMigrationHelper.AddBlockAttributeValue( "47199FAE-BB88-4CDC-B9EA-5BAB72042D64", "4C4E1DAD-F2D8-4F53-8A5E-762E8E2E937E", @"True" );
        }

        /// <summary>
        /// GJ: Update Group Type Default Lava to hide if value is blank
        /// </summary>
        private void UpdateGroupTypeDefaultLavaUp()
        {
            Sql( @"
UPDATE
        [GroupType] 
SET [GroupViewLavaTemplate] = REPLACE([GroupViewLavaTemplate], '<dl>
        {% for attribute in Group.AttributeValues %}
        <dt>{{ attribute.AttributeName }}:</dt>

<dd>{{ attribute.ValueFormatted }} </dd>
        {% endfor %}
        </dl>', '<dl>
        {% for attribute in Group.AttributeValues %}
            {% if attribute.ValueFormatted != '''' %}
                <dt>{{ attribute.AttributeName }}</dt>
                <dd>{{ attribute.ValueFormatted }}</dd>
            {% endif %}
        {% endfor %}
        </dl>')
WHERE
        [GroupViewLavaTemplate] LIKE '%<dl>
        {% for attribute in Group.AttributeValues %}
        <dt>{{ attribute.AttributeName }}:</dt>

<dd>{{ attribute.ValueFormatted }} </dd>
        {% endfor %}
        </dl>%'
" );
        }

        /// <summary>
        /// Updates the panel lava short code default.
        /// </summary>
        private void UpdatePanelLavaShortCodeDefault()
        {
            Sql( @"
                UPDATE [LavaShortcode]
                SET [Documentation]=N'<p>
The panel shortcode allows you to easily add a 
<a href=""https://community.rockrms.com/styling/components/panels"" target=""_blank"">Bootstrap panel</a> to your markup. This is a pretty simple shortcode, but it does save you some time.
</p>

<p>Basic Usage:<br>  
</p><pre>{[ panel title:''Important Stuff'' icon:''fa fa-star'' ]}<br>  
This is a super simple panel.<br> 
{[ endpanel ]}</pre>

<p></p><p>
As you can see the body of the shortcode is placed in the body of the panel. Optional parameters include:
</p>

<ul>
<li><b>title</b> – The title to show in the heading. If no title is provided then the panel title section will not be displayed.</li>
<li><b>icon </b> – The icon to use with the title.</li>
<li><b>footer</b> – If provided the text will be placed in the panel’s footer.</li>
<li><b>type</b> (default) – Change the type of panel displayed. Options include: default, primary, success, info, warning, danger, block and widget.</li>
</ul>', [Parameters]=N'type^default|icon^|title^|footer^'
                WHERE ([Guid]='ADB1F75D-500D-4305-9805-99AF04A2CD88')" );
        }
    }
}
