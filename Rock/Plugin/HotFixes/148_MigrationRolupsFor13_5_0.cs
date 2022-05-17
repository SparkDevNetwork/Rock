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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 148, "1.13.4" )]
    public class MigrationRolupsFor13_5_0 : Migration
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
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
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
    }
}
