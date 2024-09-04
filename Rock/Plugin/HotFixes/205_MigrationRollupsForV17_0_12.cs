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

using System.Collections.Generic;

using Rock.Model;
using Rock.SystemKey;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 205, "1.16.4" )]
    public class MigrationRollupsForV17_0_12 : Migration
    {
        /// <summary>
        /// Up methods
        /// </summary>
        public override void Up()
        {
            AIProvidersUp();
            UpdateERAMetricScheduleUp();
            UpdateGivingAnalyticsAndFamilyAnalyticsAttendanceProceduresUp();
            LMSUpdatesUp();
            PrayerAutomationUp();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            UpdateGivingAnalyticsAndFamilyAnalyticsAttendanceProceduresDown();
            PrayerAutomationsDown();
        }

        #region JC: AI Providers

        private void AIProvidersUp()
        {
            AddAIProviderBlocksAndPages_Up();
            AddAIProviderDefaultInstance_Up();
        }

        private void AddAIProviderDefaultInstance_Up()
        {
            const string aiProviderEntityTypeGuid = "945A994F-F15E-43AC-B503-A54BDE70F77F";
            const string aiProviderComponentEntityTypeGuid = "8D3F25B1-4891-31AA-4FA6-365F5C808563";

            // Add/Update Entity Type: AIProvider.
            RockMigrationHelper.UpdateEntityType( "Rock.Model.AIProvider",
                "AI Provider",
                "Rock.Model.AIProvider, Rock, Version=1.16.5.4, Culture=neutral, PublicKeyToken=null",
                isEntity: true,
                isSecured: false,
                aiProviderEntityTypeGuid );

            // Add/Update Entity Type: OpenAIProvider.
            RockMigrationHelper.UpdateEntityType( "Rock.AI.OpenAI.Provider.OpenAIProvider",
                "Open AI Provider",
                "Rock.AI.Provider.AIProviderComponent, Rock, Version=1.16.5.4, Culture=neutral, PublicKeyToken=null",
                isEntity: true,
                isSecured: false,
                aiProviderComponentEntityTypeGuid );

            // Add AI Provider: OpenAI Provider.
            Sql( $@"
/*
    Create a new OpenAI AIProvider instance.
*/
IF NOT EXISTS (
    SELECT 1
    FROM [AIProvider]
    WHERE [Guid] = '2AA26B14-94CB-4A30-9E97-C7250BA464BB'
)
BEGIN
    DECLARE @componentEntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '8D3F25B1-4891-31AA-4FA6-365F5C808563');
    INSERT INTO [AIProvider] (
        [IsSystem]
        ,[IsActive]
        ,[Order]
        ,[Name]
        ,[Description]
        ,[ProviderComponentEntityTypeId]
        ,[Guid]
    ) VALUES (
        1
        ,1
        ,0
        ,'Open AI'
        ,'Provider to use the OpenAI API for use in Rock.'
        ,@componentEntityTypeId
        ,'2AA26B14-94CB-4A30-9E97-C7250BA464BB'
    );
END

/*
    Migrate OpenAI Component Settings to OpenAI Instance.
*/
-- Get Entity Type for AIProvider Component.
DECLARE @openAiProviderComponentEntityTypeId INT = (
        SELECT TOP 1 [Id]
        FROM [EntityType]
        WHERE [Guid] = '8d3f25b1-4891-31aa-4fa6-365f5c808563'
        )
-- Get Entity Type for AIProvider instances.
DECLARE @aiProviderEntityTypeId INT = (
        SELECT TOP 1 [Id]
        FROM [EntityType]
        WHERE [Guid] = '945A994F-F15E-43AC-B503-A54BDE70F77F'
        )

-- Get EntityId for OpenAIProvider instance.
DECLARE @openAiProviderId INT = (
        SELECT TOP 1 [Id]
        FROM [AIProvider]
        WHERE [Guid] = '2AA26B14-94CB-4A30-9E97-C7250BA464BB'
        )

-- Create the same value for the Attribute of the new OpenAIProvider instance.
DECLARE @textFieldTypeId INT = (
        SELECT TOP 1 [Id]
        FROM [FieldType]
        WHERE [Guid] = '9C204CD0-1233-41C5-818A-C5DA439445AA'
        );
DECLARE @booleanFieldTypeId INT = (
        SELECT TOP 1 [Id]
        FROM [FieldType]
        WHERE [Guid] = '1EDAFDED-DFE6-4334-B019-6EECBA89E05A'
        );
DECLARE @counter INT = 0;
DECLARE @attributeKey NVARCHAR(1000);
DECLARE @attributeName NVARCHAR(1000);
DECLARE @attributeValue NVARCHAR(MAX);
DECLARE @attributeFieldTypeId INT;
DECLARE @providerWasMigrated BIT = 0;

WHILE ( @counter < 3 )
BEGIN
    SET @counter = @counter + 1;

    IF ( @counter = 1 )
    BEGIN
        SET @attributeFieldTypeId = @textFieldTypeId;
        SET @attributeKey = 'SecretKey';
        SET @attributeName = 'Secret Key';
    END
    ELSE IF ( @counter = 2 )
    BEGIN
        SET @attributeFieldTypeId = @textFieldTypeId;
        SET @attributeKey = 'Organization'
        SET @attributeName = 'Organization'
    END
    ELSE IF ( @counter = 3 )
    BEGIN
        SET @attributeFieldTypeId = @booleanFieldTypeId;
        SET @attributeKey = 'Active'
        SET @attributeName = 'Active'
    END

    -- Get the Attribute for the existing OpenAIProvider Component.
    DECLARE @componentAttributeId INT = (
            SELECT TOP 1 [Id]
            FROM [Attribute]
            WHERE [EntityTypeId] = @openAiProviderComponentEntityTypeId
                AND [Key] = @attributeKey
            )
    DECLARE @componentAttributeValueId INT = (
            SELECT TOP 1 [Id]
            FROM [AttributeValue]
            WHERE [AttributeId] = @componentAttributeId
            );

    IF (@componentAttributeId IS NOT NULL AND @componentAttributeValueId IS NOT NULL)
    BEGIN
		-- Flag that we migrated at least 1 attribute.
		SET @providerWasMigrated = 1;

        -- Get the current value of this Attribute for the existing OpenAIProvider component.
        DECLARE @value NVARCHAR(MAX) = (
                SELECT TOP 1 [Value]
                FROM [AttributeValue]
                WHERE [AttributeId] = @componentAttributeId
                    AND [EntityId] = 0
                )

        -- Get the Attribute for the new OpenAIProvider instance.
        DECLARE @instanceAttributeId INT = (
                SELECT TOP 1 [Id]
                FROM [Attribute]
                WHERE [EntityTypeId] = @aiProviderEntityTypeId
                    AND [EntityTypeQualifierColumn] = 'ProviderComponentEntityTypeId'
                    AND [EntityTypeQualifierValue] = @openAiProviderComponentEntityTypeId
                    AND [Key] = @attributeKey
                )

        IF (@instanceAttributeId IS NULL)
        BEGIN
            DECLARE @newAttributeGuid UNIQUEIDENTIFIER = NEWID();

            INSERT INTO [Attribute] (
                [Guid],
                [Key],
                [Name],
                [EntityTypeId],
                [EntityTypeQualifierColumn],
                [EntityTypeQualifierValue],
                [IsSystem],
                [FieldTypeId],
                [Order],
                [IsGridColumn],
                [IsMultiValue],
                [IsRequired]
                )
            VALUES (
                @newAttributeGuid,
                @attributeKey,
                @attributeName,
                @aiProviderEntityTypeId,
                'ProviderComponentEntityTypeId',
                @openAiProviderComponentEntityTypeId,
                0,
                @attributeFieldTypeId,
                0,
                0,
                0,
                1
                )

            SET @instanceAttributeId = (
                    SELECT [Id]
                    FROM [Attribute]
                    WHERE [Guid] = @newAttributeGuid
                    );
        END

        SET @attributeValue = ( SELECT TOP 1 [Value] FROM [AttributeValue] WHERE [Id] = @componentAttributeValueId );

        DECLARE @instanceAttributeValueId int = ( SELECT TOP 1 [Id] FROM [AttributeValue] WHERE [AttributeId] = @instanceAttributeId );
        IF ( @instanceAttributeValueId IS NULL)
            BEGIN
                INSERT INTO [AttributeValue]
                ( IsSystem, AttributeId, EntityId, [Value], [Guid] )
                VALUES( 0, @instanceAttributeId, @openAiProviderId, @attributeValue, NEWID() )
            END
        ELSE 
            BEGIN
                UPDATE [AttributeValue]
                SET [Value] = @attributeValue
                WHERE [Id] = @instanceAttributeValueId
            END
    END
END

IF @providerWasMigrated = 1
	BEGIN
			
		-- Get the Attribute for the new OpenAIProvider instance.
		DECLARE @defaultModelAttributeId INT = ( SELECT TOP 1 [Id]
			FROM [Attribute]
			WHERE [EntityTypeId] = @aiProviderEntityTypeId
				AND [EntityTypeQualifierColumn] = 'ProviderComponentEntityTypeId'
				AND [EntityTypeQualifierValue] = @openAiProviderComponentEntityTypeId
				AND [Key] = 'DefaultModel'
			)
                
		IF ( @defaultModelAttributeId IS NULL )
		BEGIN
			INSERT INTO [Attribute] (
				[Guid],
				[Key],
				[Name],
				[EntityTypeId],
				[EntityTypeQualifierColumn],
				[EntityTypeQualifierValue],
				[IsSystem],
				[FieldTypeId],
				[Order],
				[IsGridColumn],
				[IsMultiValue],
				[IsRequired]
				)
			VALUES (
				NEWID(),
				'DefaultModel',
				'Default Model',
				@aiProviderEntityTypeId,
				'ProviderComponentEntityTypeId',
				@openAiProviderComponentEntityTypeId,
				0,
				@textFieldTypeId,
				0,
				0,
				0,
				1
				)

			SET @defaultModelAttributeId = SCOPE_IDENTITY();
		END
		
		INSERT INTO [AttributeValue]
			( IsSystem, AttributeId, EntityId, [Value], [Guid] )
			SELECT 0, @defaultModelAttributeId, @openAiProviderId, 'gpt-4o-mini', NEWID()
			WHERE NOT EXISTS (
				SELECT *
				FROM [AttributeValue] ex
				WHERE ex.AttributeId = @defaultModelAttributeId
					AND ex.EntityId = @openAiProviderId
			)
END
" );
        }

        private void AddAIProviderBlocksAndPages_Up()
        {
            // Add Page 
            //  Internal Name: AI Provider Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "54E421B1-B89C-4C3B-BECA-16349D750691", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "AI Provider Detail", "", "64D722C2-F9F5-4DF5-947E-33862B93EECA", "" );

            // Add/Update BlockType 
            //   Name: AI Provider List
            //   Category: Core
            //   Path: ~/Blocks/AI/AIProviderList.ascx
            //   EntityType: -
            RockMigrationHelper.UpdateBlockType( "AI Provider List", "Block for viewing the list of AI providers.", "~/Blocks/AI/AIProviderList.ascx", "Core", "B3F280BD-13F4-4195-A68A-AC4A64F574A5" );

            // Add/Update BlockType 
            //   Name: AI Provider Detail
            //   Category: Core
            //   Path: ~/Blocks/AI/AiProviderDetail.ascx
            //   EntityType: -
            RockMigrationHelper.UpdateBlockType( "AI Provider Detail", "Displays the details of an AI Provider.", "~/Blocks/AI/AiProviderDetail.ascx", "Core", "88820905-1B5A-4B82-8E56-F9A0736A0E98" );

            // Attribute for  BlockType
            //   BlockType: AI Provider List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B3F280BD-13F4-4195-A68A-AC4A64F574A5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"64D722C2-F9F5-4DF5-947E-33862B93EECA", "881771AA-00A3-4E7E-8B9D-F4E4EE434836" );

            // Add Block 
            //  Block Name: AI Provider List
            //  Page Name: AI Providers
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "54E421B1-B89C-4C3B-BECA-16349D750691".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "B3F280BD-13F4-4195-A68A-AC4A64F574A5".AsGuid(), "AI Provider List", "Main", @"", @"", 1, "EE972857-A6A9-435F-BCB3-159FFE72D892" );

            // Add Block Attribute Value
            //   Block: AI Provider List
            //   BlockType: AI Provider List
            //   Category: Core
            //   Block Location: Page=AI Providers, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 64d722c2-f9f5-4df5-947e-33862b93eeca */
            RockMigrationHelper.AddBlockAttributeValue( "EE972857-A6A9-435F-BCB3-159FFE72D892", "881771AA-00A3-4E7E-8B9D-F4E4EE434836", @"64d722c2-f9f5-4df5-947e-33862b93eeca" );

            // Add Block 
            //  Block Name: AI Provider Detail
            //  Page Name: AI Provider Detail
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "64D722C2-F9F5-4DF5-947E-33862B93EECA".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "88820905-1B5A-4B82-8E56-F9A0736A0E98".AsGuid(), "AI Provider Detail", "Main", @"", @"", 0, "C681F836-8FB1-490E-AB78-2E4273E5E98B" );

            // Remove the existing Component List block for AI components.
            RockMigrationHelper.DeleteBlock( "F62F88D5-9E56-4DA0-8990-D2B6BBA0D28E" );
        }

        #endregion

        #region KA: Migration to update Prayer Request metric SourceSql

        private const string UniqueScheduleGuid = "091E1F41-104A-4856-B7CD-F6506DD59AF7";
            private const string Schedule = @"
BEGIN:VCALENDAR
PRODID:-//github.com/rianjs/ical.net//NONSGML ical.net 4.0//EN
VERSION:2.0
BEGIN:VEVENT
DTEND:20240724T090001
DTSTAMP:20240724T101307
DTSTART:20240724T210000
RRULE:FREQ=WEEKLY;BYDAY=FR
SEQUENCE:0
UID:ae4e25e4-bb92-4626-9729-513f4635745f
END:VEVENT
END:VCALENDAR";

            private void UpdateERAMetricScheduleUp()
            {
                // Create new unique weekly schedule at 9pm.
                Sql( $@"
DECLARE @ScheduleId int = ( 
    SELECT [Id] 
    FROM dbo.[Schedule] 
    WHERE Guid = '{UniqueScheduleGuid}'),

@MetricsCategoryId int = ( 
    SELECT [Id] 
    FROM dbo.[Category] 
    WHERE Guid = '5A794741-5444-43F0-90D7-48E47276D426'
)
IF @ScheduleId IS NULL
BEGIN
	INSERT [dbo].[Schedule] ([Name], [Description], [iCalendarContent], [CategoryId], [Guid], [IsActive], [IsPublic])
	VALUES ('', NULL, '{Schedule}',@MetricsCategoryId, '{UniqueScheduleGuid}', 1, 0)
END" );

                // Update eRA metrics to use 9pm schedule so it runs after FamilyAnalytics job which runs at 8pm.
                Sql( $@"
DECLARE @ScheduleId int = ( 
    SELECT [Id] 
    FROM dbo.[Schedule] 
    WHERE Guid = '{UniqueScheduleGuid}')

IF @ScheduleId IS NOT NULL
UPDATE dbo.[Metric]
SET [ScheduleId] = @ScheduleId
WHERE [Guid] IN ('D05D685A-9A88-4375-A563-70BB44FBD237','16A3FF64-31F0-4CFF-B5F4-83EEB69E0C25')
" );

                // Update Prayers metric to use prayer request campus instead of requester's campusId.
                Sql( @"
UPDATE [dbo].[Metric]
SET [SourceSql] = 'DECLARE @STARTDATE DATETIME = DATEADD(DAY, -7, GETDATE())
DECLARE @ENDDATE DATETIME = GETDATE()
SELECT COUNT(1) as PrayerRequests, pr.[CampusId]
FROM dbo.[PrayerRequest] pr
WHERE
   pr.[IsActive] = 1
   AND pr.[CreatedDateTime] BETWEEN @STARTDATE AND @ENDDATE
GROUP BY ALL pr.[CampusId]'
WHERE [Guid] = '2B5ECA35-47D8-4690-A8AD-72488485F2B4'
" );
            }

        #endregion

        #region KA : Migration to Update GivingAnalytics and FamilyAnalytics Attendance Procedures

        private void UpdateGivingAnalyticsAndFamilyAnalyticsAttendanceProceduresUp()
        {
            Sql( @"
ALTER PROCEDURE [dbo].[spCrm_FamilyAnalyticsAttendance]

AS
BEGIN
	
	-- configuration of the duration in weeks
	DECLARE @EntryAttendanceDurationWeeks int = 16
		
	DECLARE @cACTIVE_RECORD_STATUS_VALUE_GUID uniqueidentifier = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E'
	DECLARE @cPERSON_RECORD_TYPE_VALUE_GUID uniqueidentifier = '36CF10D6-C695-413D-8E7C-4546EFEF385E'
	DECLARE @cATTRIBUTE_IS_ERA_GUID uniqueidentifier = 'CE5739C5-2156-E2AB-48E5-1337C38B935E'
	DECLARE @cFAMILY_GROUPTYPE_GUID uniqueidentifier = '790E3215-3B10-442B-AF69-616C0DCB998E'
	DECLARE @cADULT_ROLE_GUID uniqueidentifier = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42'
	DECLARE @cCHILD_ROLE_GUID uniqueidentifier = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9'

	DECLARE @cATTRIBUTE_FIRST_ATTENDED uniqueidentifier  = 'AB12B3B0-55B8-D6A5-4C1F-DB9CCB2C4342'
	DECLARE @cATTRIBUTE_LAST_ATTENDED uniqueidentifier  = '5F4C6462-018E-D19C-4AB0-9843CB21C57E'
	DECLARE @cATTRIBUTE_TIMES_ATTENDED_IN_DURATION uniqueidentifier  = '45A1E978-DC5B-CFA1-4AF4-EA098A24C914'
	DECLARE @Now DATETIME = dbo.RockGetDate()

	-- --------- END CONFIGURATION --------------

	DECLARE @ActiveRecordStatusValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @cACTIVE_RECORD_STATUS_VALUE_GUID)
	DECLARE @PersonRecordTypeValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @cPERSON_RECORD_TYPE_VALUE_GUID)
	DECLARE @IsEraAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_IS_ERA_GUID)
	DECLARE @FamilyGroupTypeId int = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = @cFAMILY_GROUPTYPE_GUID)
	DECLARE @AdultRoleId int = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = @cADULT_ROLE_GUID)
	DECLARE @ChildRoleId int = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = @cCHILD_ROLE_GUID)

	-- calculate dates for query
	DECLARE @SundayDateStart datetime = [dbo].[ufnUtility_GetPreviousSundayDate]()
	DECLARE @SundayEntryAttendanceDuration datetime = DATEADD(DAY,  (7 * @EntryAttendanceDurationWeeks * -1), @SundayDateStart)
	

	-- first checkin
	DECLARE @FirstAttendedAttributeId int = (SELECT TOP 1 [Id] FROM dbo.[Attribute] WHERE [Guid] = @cATTRIBUTE_FIRST_ATTENDED);

	WITH
	  cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	  AS
	  (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	  )
	SELECT [PersonId], [FirstAttendedDate], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
	INTO #firstAttended
	FROM 
		(SELECT 
			i.[PersonId]
			, CASE WHEN [FamilyRole] = 'Adult' THEN 
					(SELECT 
						MIN(A.[StartDateTime] )
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] IN (SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](i.[PersonId])))
				ELSE
					(SELECT 
						MIN(a.[StartDateTime] )
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] = i.[PersonId])
			  END AS [FirstAttendedDate]
		FROM cteIndividual i ) AS a
	LEFT JOIN [AttributeValue] av ON av.[EntityId] = a.[PersonId]
	AND av.[AttributeId] = @FirstAttendedAttributeId
	WHERE a.[FirstAttendedDate] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[FirstAttendedDate],
		av.[ValueAsDateTime] = f.[FirstAttendedDate],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #firstAttended f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[FirstAttendedDate]

	-- Added new values.
	INSERT dbo.[AttributeValue] ([EntityId]
        , [AttributeId]
        , [Value]
        , [IsSystem]
        , [Guid]
        , [CreatedDateTime]
        , [ValueAsDateTime]
        , [IsPersistedValueDirty])
	SELECT f.[PersonId]
		, @FirstAttendedAttributeId
		, f.[FirstAttendedDate]
		, 0
		, newid()
		, @Now
		, f.[FirstAttendedDate]
		, 1
	FROM #firstAttended f
	WHERE f.[AttributeValueId] IS NULL

	-- Remove the temp table.
	DROP TABLE #firstAttended

	-- last checkin
	DECLARE @LastAttendedAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_LAST_ATTENDED);

	WITH
	  cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	  AS
	  (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	  )
	SELECT [PersonId], [LastAttendedDate], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
	INTO #lastAttended
	FROM 
		(SELECT 
			i.[PersonId]
			, CASE WHEN [FamilyRole] = 'Adult' THEN 
					(SELECT 
						MAX(a.StartDateTime )
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] IN (SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](i.[PersonId])))
				ELSE
					(SELECT 
						MAX(a.StartDateTime )
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] = i.[PersonId])
			  END AS [LastAttendedDate]
		FROM cteIndividual i ) AS a
	LEFT JOIN dbo.[AttributeValue] av ON av.[EntityId] = a.[PersonId] 
	AND av.[AttributeId] = @LastAttendedAttributeId
	WHERE a.[LastAttendedDate] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[LastAttendedDate],
		av.[ValueAsDateTime] = f.[LastAttendedDate],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #lastAttended f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[LastAttendedDate]

	-- Added new values.
	INSERT dbo.[AttributeValue] ([EntityId]
        , [AttributeId]
        , [Value]
        , [IsSystem]
        , [Guid]
        , [CreatedDateTime]
        , [ValueAsDateTime]
        , [IsPersistedValueDirty])
	SELECT f.PersonId
		, @LastAttendedAttributeId
		, f.[LastAttendedDate]
		, 0
		, newid()
		, @Now
		, f.[LastAttendedDate]
		, 1
	FROM #lastAttended f
	WHERE f.[AttributeValueId] IS NULL

	-- Remove the temp table.
	DROP TABLE #lastAttended

	-- times checkedin
	DECLARE @TimesAttendedAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_TIMES_ATTENDED_IN_DURATION)
	DELETE FROM [AttributeValue] WHERE [AttributeId] = @TimesAttendedAttributeId;

	WITH
	  cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	  AS
	  (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	  )
	SELECT [PersonId], [CheckinCount], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
	INTO #checkInCount
	FROM 
		(SELECT 
			i.[PersonId]
			, CASE WHEN [FamilyRole] = 'Adult' THEN 
					(SELECT 
						COUNT(*)
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
						AND CAST( a.[StartDateTime] AS DATE ) <= @SundayDateStart AND a.[StartDateTime] >= @SundayEntryAttendanceDuration
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] IN (SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](i.[PersonId])))
				ELSE
					(SELECT 
						COUNT(*)
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
						AND CAST( a.[StartDateTime] AS DATE ) <= @SundayDateStart AND a.[StartDateTime] >= @SundayEntryAttendanceDuration
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] = i.[PersonId])
			  END AS [CheckinCount]
		FROM cteIndividual i ) AS a
	LEFT JOIN dbo.[AttributeValue] av ON av.[EntityId] = a.[PersonId]
	AND av.[AttributeId] = @TimesAttendedAttributeId
	WHERE a.[CheckinCount] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[CheckinCount],
		av.[ValueAsNumeric] = f.[CheckinCount],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #checkInCount f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[CheckinCount]

	-- Added new values.
	INSERT dbo.[AttributeValue] ([EntityId]
        , [AttributeId]
        , [Value]
        , [IsSystem]
        , [Guid]
        , [CreatedDateTime]
        , [ValueAsNumeric]
        , [IsPersistedValueDirty])
	SELECT f.[PersonId]
		, @TimesAttendedAttributeId
		, f.[CheckinCount]
		, 0
		, newid()
		, @Now
		, f.[CheckinCount]
		, 1
	FROM #checkInCount f
	WHERE f.[AttributeValueId] IS NULL

	-- Remove the temp table.
	DROP TABLE #checkInCount
	
END
" );

            Sql( @"
ALTER PROCEDURE [dbo].[spCrm_FamilyAnalyticsGiving]
	
AS
BEGIN
	
	-- configuration of the duration in weeks
	DECLARE @GivingDurationLongWeeks int = 52
	DECLARE @GivingDurationShortWeeks int = 6
	
	DECLARE @cACTIVE_RECORD_STATUS_VALUE_GUID uniqueidentifier = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E'
	DECLARE @cPERSON_RECORD_TYPE_VALUE_GUID uniqueidentifier = '36CF10D6-C695-413D-8E7C-4546EFEF385E'
	DECLARE @cFAMILY_GROUPTYPE_GUID uniqueidentifier = '790E3215-3B10-442B-AF69-616C0DCB998E'
	DECLARE @cADULT_ROLE_GUID uniqueidentifier = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42'	
	DECLARE @cCONTRIBUTION_TYPE_VALUE_GUID uniqueidentifier = '2D607262-52D6-4724-910D-5C6E8FB89ACC'
	DECLARE @cATTRIBUTE_FIRST_GAVE uniqueidentifier  = 'EE5EC76A-D4B9-56B5-4B48-29627D945F10'
	DECLARE @cATTRIBUTE_LAST_GAVE uniqueidentifier  = '02F64263-E290-399E-4487-FC236F4DE81F'
	DECLARE @cATTRIBUTE_GIFT_COUNT_SHORT uniqueidentifier  = 'AC11EF53-AE55-79A0-4CAD-43721750E988'
	DECLARE @cATTRIBUTE_GIFT_COUNT_LONG uniqueidentifier  = '57700E8F-ED11-D787-415A-04DDF411BB10'
	-- --------- END CONFIGURATION --------------
	DECLARE @ActiveRecordStatusValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @cACTIVE_RECORD_STATUS_VALUE_GUID)
	DECLARE @PersonRecordTypeValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @cPERSON_RECORD_TYPE_VALUE_GUID)
	DECLARE @FamilyGroupTypeId int = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = @cFAMILY_GROUPTYPE_GUID)
	DECLARE @AdultRoleId int = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = @cADULT_ROLE_GUID)
	DECLARE @ContributionTypeId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @cCONTRIBUTION_TYPE_VALUE_GUID)
	
	-- calculate dates for queries
	DECLARE @SundayDateStart datetime = [dbo].[ufnUtility_GetPreviousSundayDate]()
	DECLARE @SundayGivingDurationLong datetime = DATEADD(DAY,  (7 * @GivingDurationLongWeeks * -1), @SundayDateStart)
	DECLARE @SundayGivingDurationShort datetime = DATEADD(DAY,  (7 * @GivingDurationShortWeeks * -1), @SundayDateStart);
	DECLARE @Now DATETIME = dbo.RockGetDate()

	-- first gift (people w/Giving Group)
	DECLARE @FirstGaveAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_FIRST_GAVE);
	WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
				AND p.[GivingGroupId] IS NOT NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )
		SELECT [PersonId], [FirstContributionDate], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
		INTO #firstGiftWithGroup
		FROM 
			(SELECT 
	    		[PersonId]
	    		, (SELECT MIN(ft.TransactionDateTime)
	    					FROM [FinancialTransaction] ft
	    						INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
	    						INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
	    						INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
	    						INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
	    					WHERE 
	    						gp.[GivingGroupId] = i.[GivingGroupId]
	    						AND ft.TransactionTypeValueId = @ContributionTypeId) AS [FirstContributionDate]
	    	FROM cteIndividual i
	    	WHERE [FamilyRole] = 'Adult') AS g
		LEFT JOIN [AttributeValue] av ON av.[EntityId] = g.[PersonId]
		AND av.[AttributeId] = @FirstGaveAttributeId
	    WHERE g.[FirstContributionDate] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[FirstContributionDate],
		av.[ValueAsDateTime] = f.[FirstContributionDate],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #firstGiftWithGroup f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[FirstContributionDate]

	-- Added new values.
	INSERT dbo.[AttributeValue] ([EntityId]
        , [AttributeId]
        , [Value]
        , [IsSystem]
        , [Guid]
        , [CreatedDateTime]
        , [ValueAsDateTime]
        , [IsPersistedValueDirty])
	SELECT f.[PersonId]
		, @FirstGaveAttributeId
		, f.[FirstContributionDate]
		, 0
		, newid()
		, @Now
		, f.[FirstContributionDate]
		, 1
	FROM #firstGiftWithGroup f
	WHERE f.[AttributeValueId] IS NULL

	-- Remove the temp table.
	DROP TABLE #firstGiftWithGroup

	-- first gift (people WITHOUT Giving Group)
	;WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
				AND p.[GivingGroupId] IS NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )

		SELECT [PersonId], [FirstContributionDate], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
		INTO #firstGiftWithoutGroup
		FROM 
	    	(SELECT 
	    		[PersonId]
	    		, (SELECT MIN(ft.TransactionDateTime)
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[Id] = i.[PersonId] -- match by person id
							AND ft.TransactionTypeValueId = @ContributionTypeId) AS [FirstContributionDate]
	    	FROM cteIndividual i
	    	WHERE [FamilyRole] = 'Adult') AS g
		LEFT JOIN [AttributeValue] av ON av.[EntityId] = g.[PersonId]
		AND av.[AttributeId] = @FirstGaveAttributeId
	    WHERE g.[FirstContributionDate] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[FirstContributionDate],
		av.[ValueAsDateTime] = f.[FirstContributionDate],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #firstGiftWithoutGroup f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[FirstContributionDate]

	-- Added new values.
	INSERT dbo.[AttributeValue] ([EntityId]
        , [AttributeId]
        , [Value]
        , [IsSystem]
        , [Guid]
        , [CreatedDateTime]
        , [ValueAsDateTime]
        , [IsPersistedValueDirty])
	SELECT f.[PersonId]
		, @FirstGaveAttributeId
		, f.[FirstContributionDate]
		, 0
		, newid()
		, @Now
		, f.[FirstContributionDate]
		, 1
	FROM #firstGiftWithoutGroup f
	WHERE f.[AttributeValueId] IS NULL

	-- Remove the temp table.
	DROP TABLE #firstGiftWithoutGroup
	
	-- last gift (people w/Giving Group)
	DECLARE @LastGaveAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_LAST_GAVE);
	WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
				AND p.[GivingGroupId] IS NOT NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )

		SELECT [PersonId], [LastContributionDate], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
		INTO #lastGiftWithGroup
		FROM 
	    	(SELECT 
	    		[PersonId]
	    		, (SELECT MAX(ft.TransactionDateTime)
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[GivingGroupId] = i.[GivingGroupId]
							AND ft.TransactionTypeValueId = @ContributionTypeId) AS [LastContributionDate]
	    	FROM cteIndividual i
	    	WHERE [FamilyRole] = 'Adult') AS g
		LEFT JOIN [AttributeValue] av ON av.[EntityId] = g.[PersonId]
		AND av.[AttributeId] = @LastGaveAttributeId
	    WHERE g.[LastContributionDate] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[LastContributionDate],
		av.[ValueAsDateTime] = f.[LastContributionDate],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #lastGiftWithGroup f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[LastContributionDate]

	-- Added new values.
	INSERT dbo.[AttributeValue] ([EntityId]
        , [AttributeId]
        , [Value]
        , [IsSystem]
        , [Guid]
        , [CreatedDateTime]
        , [ValueAsDateTime]
        , [IsPersistedValueDirty])
	SELECT f.[PersonId]
		, @LastGaveAttributeId
		, f.[LastContributionDate]
		, 0
		, newid()
		, @Now
		, f.[LastContributionDate]
		, 1
	FROM #lastGiftWithGroup f
	WHERE f.[AttributeValueId] IS NULL

	-- Remove the temp table.
	DROP TABLE #lastGiftWithGroup

	-- last gift (people WITHOUT Giving Group)
	;WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] -- match by person id
				AND p.[GivingGroupId] IS NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )

		SELECT [PersonId], [LastContributionDate], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
		INTO #lastGiftWithoutGroup
		FROM 
	    	(SELECT 
	    		[PersonId]
	    		, (SELECT MAX(ft.TransactionDateTime)
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[Id] = i.[PersonId]
							AND ft.TransactionTypeValueId = @ContributionTypeId) AS [LastContributionDate]
	    	FROM cteIndividual i
	    	WHERE [FamilyRole] = 'Adult') AS g
		LEFT JOIN [AttributeValue] av ON av.[EntityId] = g.[PersonId]
		AND av.[AttributeId] = @LastGaveAttributeId
	    WHERE g.[LastContributionDate] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[LastContributionDate],
		av.[ValueAsDateTime] = f.[LastContributionDate],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #lastGiftWithoutGroup f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[LastContributionDate]

	-- Added new values.
	INSERT dbo.[AttributeValue] ([EntityId]
        , [AttributeId]
        , [Value]
        , [IsSystem]
        , [Guid]
        , [CreatedDateTime]
        , [ValueAsDateTime]
        , [IsPersistedValueDirty])
	SELECT f.[PersonId]
		, @LastGaveAttributeId
		, f.[LastContributionDate]
		, 0
		, newid()
		, @Now
		, f.[LastContributionDate]
		, 1
	FROM #lastGiftWithoutGroup f
	WHERE f.[AttributeValueId] IS NULL

	-- Remove the temp table.
	DROP TABLE #lastGiftWithoutGroup

	-- number of gifts short duration (people w/Giving Group)
	DECLARE @GiftCountShortAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_GIFT_COUNT_SHORT);
	WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
				AND p.[GivingGroupId] IS NOT NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )

		SELECT [PersonId], [GiftCountDurationShort], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
		INTO #giftCountShortWithGroup
		FROM 
	    	(SELECT 
	    		[PersonId]
	    		, (SELECT COUNT(DISTINCT(ft.[Id])) 
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[GivingGroupId] = i.[GivingGroupId]
							AND ft.TransactionTypeValueId = @ContributionTypeId
							AND ft.TransactionDateTime >= @SundayGivingDurationShort
							AND ft.SundayDate <= @SundayDateStart) AS [GiftCountDurationShort]
	    	FROM cteIndividual i
	    	WHERE [FamilyRole] = 'Adult') AS g
		LEFT JOIN [AttributeValue] av ON av.[EntityId] = g.[PersonId]
		AND av.[AttributeId] = @GiftCountShortAttributeId
	    WHERE g.[GiftCountDurationShort] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[GiftCountDurationShort],
		av.[ValueAsNumeric] = f.[GiftCountDurationShort],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #giftCountShortWithGroup f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[GiftCountDurationShort]

	-- Added new values.
	INSERT dbo.[AttributeValue] ([EntityId]
        , [AttributeId]
        , [Value]
        , [IsSystem]
        , [Guid]
        , [CreatedDateTime]
        , [ValueAsNumeric]
        , [IsPersistedValueDirty])
	SELECT f.[PersonId]
		, @GiftCountShortAttributeId
		, f.[GiftCountDurationShort]
		, 0
		, newid()
		, @Now
		, f.[GiftCountDurationShort]
		, 1
	FROM #giftCountShortWithGroup f
	WHERE f.[AttributeValueId] IS NULL

    -- Remove the temp table.
	DROP TABLE #giftCountShortWithGroup

	-- number of gifts short duration (people WITHOUT Giving Group)
	;WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
				AND p.[GivingGroupId] IS NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )

		SELECT [PersonId], [GiftCountDurationShort], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
		INTO #giftCountShortWithoutGroup
		FROM 
	    	(SELECT 
	    		[PersonId]
	    		, (SELECT COUNT(DISTINCT(ft.[Id])) 
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[Id] = i.[PersonId] -- match by person id
							AND ft.TransactionTypeValueId = @ContributionTypeId
							AND ft.TransactionDateTime >= @SundayGivingDurationShort
							AND ft.SundayDate <= @SundayDateStart) AS [GiftCountDurationShort]
	    	FROM cteIndividual i
	    	WHERE [FamilyRole] = 'Adult') AS g
		LEFT JOIN [AttributeValue] av ON av.[EntityId] = g.[PersonId]
		AND av.[AttributeId] = @GiftCountShortAttributeId
	    WHERE g.[GiftCountDurationShort] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[GiftCountDurationShort],
		av.[ValueAsNumeric] = f.[GiftCountDurationShort],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #giftCountShortWithoutGroup f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[GiftCountDurationShort]

	-- Added new values.
	INSERT dbo.[AttributeValue] ([EntityId]
        , [AttributeId]
        , [Value]
        , [IsSystem]
        , [Guid]
        , [CreatedDateTime]
        , [ValueAsNumeric]
        , [IsPersistedValueDirty])
	SELECT f.[PersonId]
		, @GiftCountShortAttributeId
		, f.[GiftCountDurationShort]
		, 0
		, newid()
		, @Now
		, f.[GiftCountDurationShort]
		, 1
	FROM #giftCountShortWithoutGroup f
	WHERE f.[AttributeValueId] IS NULL

    -- Remove the temp table.
	DROP TABLE #giftCountShortWithoutGroup

	-- number of gifts long duration (people w/Giving Group)
	DECLARE @GiftCountLongAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_GIFT_COUNT_LONG);
	WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
				AND p.[GivingGroupId] IS NOT NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )

		SELECT [PersonId], [GiftCountDurationLong], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
		INTO #giftCountLongWithGroup
		FROM 
	    	(SELECT 
	    		[PersonId]
	    		, (SELECT COUNT(DISTINCT(ft.[Id])) 
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[GivingGroupId] = i.[GivingGroupId]
							AND ft.TransactionTypeValueId = @ContributionTypeId
							AND ft.TransactionDateTime >= @SundayGivingDurationLong
							AND ft.SundayDate <= @SundayDateStart) AS [GiftCountDurationLong]
	    	FROM cteIndividual i
	    	WHERE [FamilyRole] = 'Adult') AS g
		LEFT JOIN [AttributeValue] av ON av.[EntityId] = g.[PersonId]
		AND av.[AttributeId] = @GiftCountLongAttributeId
	    WHERE g.[GiftCountDurationLong] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[GiftCountDurationLong],
		av.[ValueAsNumeric] = f.[GiftCountDurationLong],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #giftCountLongWithGroup f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[GiftCountDurationLong]

	-- Added new values.
	INSERT dbo.[AttributeValue] ([EntityId]
        , [AttributeId]
        , [Value]
        , [IsSystem]
        , [Guid]
        , [CreatedDateTime]
        , [ValueAsNumeric]
        , [IsPersistedValueDirty])
	SELECT f.[PersonId]
		, @GiftCountLongAttributeId
		, f.[GiftCountDurationLong]
		, 0
		, newid()
		, @Now
		, f.[GiftCountDurationLong]
		, 1
	FROM #giftCountLongWithGroup f
	WHERE f.[AttributeValueId] IS NULL

    -- Remove the temp table.
	DROP TABLE #giftCountLongWithGroup
	
	-- number of gifts long duration (people WITHOUT Giving Group)
	;WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
				AND p.[GivingGroupId] IS NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )

		SELECT [PersonId], [GiftCountDurationLong], av.[Id] AttributeValueId, av.[ValueAsDateTime] ExistingValue
		INTO #giftCountLongWithoutGroup
		FROM 
	    	(SELECT 
	    		[PersonId]
	    		, (SELECT COUNT(DISTINCT(ft.[Id])) 
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[Id] = i.[PersonId] -- match by person id
							AND ft.TransactionTypeValueId = @ContributionTypeId
							AND ft.TransactionDateTime >= @SundayGivingDurationLong
							AND ft.SundayDate <= @SundayDateStart) AS [GiftCountDurationLong]
	    		, 0 AS [IsSystem]
	    		, newid() AS [Guid]
	    	FROM cteIndividual i
	    	WHERE [FamilyRole] = 'Adult') AS g
		LEFT JOIN [AttributeValue] av ON av.[EntityId] = g.[PersonId]
		AND av.[AttributeId] = @GiftCountLongAttributeId
	    WHERE g.[GiftCountDurationLong] IS NOT NULL

	-- Update Existing values before inserting to reduce number of logical reads.
	UPDATE av SET
		av.[Value] = f.[GiftCountDurationLong],
		av.[ValueAsNumeric] = f.[GiftCountDurationLong],
		av.[IsPersistedValueDirty] = 1
	FROM dbo.[AttributeValue] av
	JOIN #giftCountLongWithoutGroup f on f.[AttributeValueId] = av.[Id]
	WHERE f.[ExistingValue] IS NULL
	OR f.[ExistingValue] <> f.[GiftCountDurationLong]

	-- Added new values.
	INSERT dbo.[AttributeValue] ([EntityId]
        , [AttributeId]
        , [Value]
        , [IsSystem]
        , [Guid]
        , [CreatedDateTime]
        , [ValueAsNumeric]
        , [IsPersistedValueDirty])
	SELECT f.[PersonId]
		, @GiftCountLongAttributeId
		, f.[GiftCountDurationLong]
		, 0
		, newid()
		, @Now
		, f.[GiftCountDurationLong]
		, 1
	FROM #giftCountLongWithoutGroup f
	WHERE f.[AttributeValueId] IS NULL

    -- Remove the temp table.
	DROP TABLE #giftCountLongWithoutGroup
END
" );
        }

        private void UpdateGivingAnalyticsAndFamilyAnalyticsAttendanceProceduresDown()
        {
            Sql( @"
ALTER PROCEDURE [dbo].[spCrm_FamilyAnalyticsAttendance]

AS
BEGIN
	
	-- configuration of the duration in weeks
	DECLARE @EntryAttendanceDurationWeeks int = 16
		
	DECLARE @cACTIVE_RECORD_STATUS_VALUE_GUID uniqueidentifier = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E'
	DECLARE @cPERSON_RECORD_TYPE_VALUE_GUID uniqueidentifier = '36CF10D6-C695-413D-8E7C-4546EFEF385E'
	DECLARE @cATTRIBUTE_IS_ERA_GUID uniqueidentifier = 'CE5739C5-2156-E2AB-48E5-1337C38B935E'
	DECLARE @cFAMILY_GROUPTYPE_GUID uniqueidentifier = '790E3215-3B10-442B-AF69-616C0DCB998E'
	DECLARE @cADULT_ROLE_GUID uniqueidentifier = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42'
	DECLARE @cCHILD_ROLE_GUID uniqueidentifier = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9'

	DECLARE @cATTRIBUTE_FIRST_ATTENDED uniqueidentifier  = 'AB12B3B0-55B8-D6A5-4C1F-DB9CCB2C4342'
	DECLARE @cATTRIBUTE_LAST_ATTENDED uniqueidentifier  = '5F4C6462-018E-D19C-4AB0-9843CB21C57E'
	DECLARE @cATTRIBUTE_TIMES_ATTENDED_IN_DURATION uniqueidentifier  = '45A1E978-DC5B-CFA1-4AF4-EA098A24C914'

	-- --------- END CONFIGURATION --------------

	DECLARE @ActiveRecordStatusValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @cACTIVE_RECORD_STATUS_VALUE_GUID)
	DECLARE @PersonRecordTypeValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @cPERSON_RECORD_TYPE_VALUE_GUID)
	DECLARE @IsEraAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_IS_ERA_GUID)
	DECLARE @FamilyGroupTypeId int = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = @cFAMILY_GROUPTYPE_GUID)
	DECLARE @AdultRoleId int = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = @cADULT_ROLE_GUID)
	DECLARE @ChildRoleId int = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = @cCHILD_ROLE_GUID)

	-- calculate dates for query
	DECLARE @SundayDateStart datetime = [dbo].[ufnUtility_GetPreviousSundayDate]()
	DECLARE @SundayEntryAttendanceDuration datetime = DATEADD(DAY,  (7 * @EntryAttendanceDurationWeeks * -1), @SundayDateStart)
	


	-- first checkin
	DECLARE @FirstAttendedAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_FIRST_ATTENDED)
	DELETE FROM [AttributeValue] WHERE [AttributeId] = @FirstAttendedAttributeId;

	WITH
	  cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	  AS
	  (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	  )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsDateTime], [IsPersistedValueDirty])
	SELECT [PersonId], [AttributeId], [FirstAttendedDate], [IsSystem], [Guid], [CreateDate], [FirstAttendedDate], 1  FROM 
		(SELECT 
			i.[PersonId]
			, @FirstAttendedAttributeId AS [AttributeId]
			, CASE WHEN [FamilyRole] = 'Adult' THEN 
					(SELECT 
						MIN(A.[StartDateTime] )
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] IN (SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](i.[PersonId])))
				ELSE
					(SELECT 
						MIN(a.StartDateTime )
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] = i.[PersonId])
			  END AS [FirstAttendedDate]
			, 0 AS [IsSystem]
			, newid() AS [Guid]
			, getdate() AS [CreateDate]
		FROM cteIndividual i ) AS a
	WHERE a.[FirstAttendedDate] IS NOT NULL

	-- last checkin
	DECLARE @LastAttendedAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_LAST_ATTENDED)
	DELETE FROM [AttributeValue] WHERE [AttributeId] = @LastAttendedAttributeId;

	WITH
	  cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	  AS
	  (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	  )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsDateTime], [IsPersistedValueDirty])
	SELECT [PersonId], [AttributeId], [LastAttendedDate], [IsSystem], [Guid], [CreateDate], [LastAttendedDate], 1  FROM  
		(SELECT 
			i.[PersonId]
			, @LastAttendedAttributeId AS [AttributeId]
			, CASE WHEN [FamilyRole] = 'Adult' THEN 
					(SELECT 
						MAX(a.StartDateTime )
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] IN (SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](i.[PersonId])))
				ELSE
					(SELECT 
						MAX(a.StartDateTime )
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] = i.[PersonId])
			  END AS [LastAttendedDate]
			, 0 AS [IsSystem]
			, newid() AS [Guid]
			, getdate() AS [CreateDate]
		FROM cteIndividual i ) AS a
	WHERE a.[LastAttendedDate] IS NOT NULL

	-- times checkedin
	DECLARE @TimesAttendedAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_TIMES_ATTENDED_IN_DURATION)
	DELETE FROM [AttributeValue] WHERE [AttributeId] = @TimesAttendedAttributeId;

	WITH
	  cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	  AS
	  (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	  )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsNumeric], [IsPersistedValueDirty])
	SELECT [PersonId], [AttributeId], [CheckinCount], [IsSystem], [Guid], [CreateDate], [CheckinCount], 1
    FROM 
		(SELECT 
			i.[PersonId]
			, @TimesAttendedAttributeId AS [AttributeId]
			, CASE WHEN [FamilyRole] = 'Adult' THEN 
					(SELECT 
						COUNT(*)
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
						AND CAST( a.[StartDateTime] AS DATE ) <= @SundayDateStart AND a.[StartDateTime] >= @SundayEntryAttendanceDuration
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] IN (SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](i.[PersonId])))
				ELSE
					(SELECT 
						COUNT(*)
					FROM
						[Attendance] a
						INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
						INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
					WHERE 
						O.[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
						AND CAST( a.[StartDateTime] AS DATE ) <= @SundayDateStart AND a.[StartDateTime] >= @SundayEntryAttendanceDuration
                        AND a.[DidAttend] = 1
						AND pa.[PersonId] = i.[PersonId])
			  END AS [CheckinCount]
			, 0 AS [IsSystem]
			, newid() AS [Guid]
			, getdate() AS [CreateDate]
		FROM cteIndividual i ) AS a
	WHERE a.[CheckinCount] IS NOT NULL

	
END
" );

            Sql( @"
ALTER PROCEDURE [dbo].[spCrm_FamilyAnalyticsGiving]
	
AS
BEGIN
	
	-- configuration of the duration in weeks
	DECLARE @GivingDurationLongWeeks int = 52
	DECLARE @GivingDurationShortWeeks int = 6
	
	DECLARE @cACTIVE_RECORD_STATUS_VALUE_GUID uniqueidentifier = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E'
	DECLARE @cPERSON_RECORD_TYPE_VALUE_GUID uniqueidentifier = '36CF10D6-C695-413D-8E7C-4546EFEF385E'
	DECLARE @cFAMILY_GROUPTYPE_GUID uniqueidentifier = '790E3215-3B10-442B-AF69-616C0DCB998E'
	DECLARE @cADULT_ROLE_GUID uniqueidentifier = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42'	
	DECLARE @cCONTRIBUTION_TYPE_VALUE_GUID uniqueidentifier = '2D607262-52D6-4724-910D-5C6E8FB89ACC'
	DECLARE @cATTRIBUTE_FIRST_GAVE uniqueidentifier  = 'EE5EC76A-D4B9-56B5-4B48-29627D945F10'
	DECLARE @cATTRIBUTE_LAST_GAVE uniqueidentifier  = '02F64263-E290-399E-4487-FC236F4DE81F'
	DECLARE @cATTRIBUTE_GIFT_COUNT_SHORT uniqueidentifier  = 'AC11EF53-AE55-79A0-4CAD-43721750E988'
	DECLARE @cATTRIBUTE_GIFT_COUNT_LONG uniqueidentifier  = '57700E8F-ED11-D787-415A-04DDF411BB10'
	-- --------- END CONFIGURATION --------------
	DECLARE @ActiveRecordStatusValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @cACTIVE_RECORD_STATUS_VALUE_GUID)
	DECLARE @PersonRecordTypeValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @cPERSON_RECORD_TYPE_VALUE_GUID)
	DECLARE @FamilyGroupTypeId int = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = @cFAMILY_GROUPTYPE_GUID)
	DECLARE @AdultRoleId int = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = @cADULT_ROLE_GUID)
	DECLARE @ContributionTypeId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @cCONTRIBUTION_TYPE_VALUE_GUID)
	
	-- calculate dates for queries
	DECLARE @SundayDateStart datetime = [dbo].[ufnUtility_GetPreviousSundayDate]()
	DECLARE @SundayGivingDurationLong datetime = DATEADD(DAY,  (7 * @GivingDurationLongWeeks * -1), @SundayDateStart)
	DECLARE @SundayGivingDurationShort datetime = DATEADD(DAY,  (7 * @GivingDurationShortWeeks * -1), @SundayDateStart);

	-- first gift (people w/Giving Group)
	DECLARE @FirstGaveAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_FIRST_GAVE)
	DELETE FROM [AttributeValue] WHERE [AttributeId] = @FirstGaveAttributeId;
	WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
				AND p.[GivingGroupId] IS NOT NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsDateTime], [IsPersistedValueDirty] )
	SELECT [PersonId], [AttributeId], [FirstContributionDate], [IsSystem], [Guid], [CreateDate], [FirstContributionDate], 1  FROM 
		(SELECT 
			[PersonId]
			, @FirstGaveAttributeId AS [AttributeId]
			, (SELECT MIN(ft.TransactionDateTime)
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[GivingGroupId] = i.[GivingGroupId]
							AND ft.TransactionTypeValueId = @ContributionTypeId) AS [FirstContributionDate]
			, 0 AS [IsSystem]
			, newid() AS [Guid]
			, getdate() AS [CreateDate]
		FROM cteIndividual i
		WHERE [FamilyRole] = 'Adult') AS g
	WHERE g.[FirstContributionDate] IS NOT NULL
	-- first gift (people WITHOUT Giving Group)
	;WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
				AND p.[GivingGroupId] IS NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsDateTime], [IsPersistedValueDirty])
	SELECT [PersonId], [AttributeId], [FirstContributionDate], [IsSystem], [Guid], [CreateDate], [FirstContributionDate], 1 FROM 
		(SELECT 
			[PersonId]
			, @FirstGaveAttributeId AS [AttributeId]
			, (SELECT MIN(ft.TransactionDateTime)
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[Id] = i.[PersonId] -- match by person id
							AND ft.TransactionTypeValueId = @ContributionTypeId) AS [FirstContributionDate]
			, 0 AS [IsSystem]
			, newid() AS [Guid]
			, getdate() AS [CreateDate]
		FROM cteIndividual i
		WHERE [FamilyRole] = 'Adult') AS g
	WHERE g.[FirstContributionDate] IS NOT NULL
	
	-- last gift (people w/Giving Group)
	DECLARE @LastGaveAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_LAST_GAVE)
	DELETE FROM [AttributeValue] WHERE [AttributeId] = @LastGaveAttributeId;
	WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
				AND p.[GivingGroupId] IS NOT NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsDateTime], [IsPersistedValueDirty])
	SELECT [PersonId], [AttributeId], [LastContributionDate], [IsSystem], [Guid], [CreateDate], [LastContributionDate], 1 FROM 
		(SELECT 
			[PersonId]
			, @LastGaveAttributeId AS [AttributeId]
			, (SELECT MAX(ft.TransactionDateTime)
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[GivingGroupId] = i.[GivingGroupId]
							AND ft.TransactionTypeValueId = @ContributionTypeId) AS [LastContributionDate]
			, 0 AS [IsSystem]
			, newid() AS [Guid]
			, getdate() AS [CreateDate]
		FROM cteIndividual i
		WHERE [FamilyRole] = 'Adult') AS g
	WHERE g.[LastContributionDate] IS NOT NULL
	-- last gift (people WITHOUT Giving Group)
	;WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] -- match by person id
				AND p.[GivingGroupId] IS NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsDateTime], [IsPersistedValueDirty] )
	SELECT [PersonId], [AttributeId], [LastContributionDate], [IsSystem], [Guid], [CreateDate], [LastContributionDate], 1 FROM 
		(SELECT 
			[PersonId]
			, @LastGaveAttributeId AS [AttributeId]
			, (SELECT MAX(ft.TransactionDateTime)
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[Id] = i.[PersonId]
							AND ft.TransactionTypeValueId = @ContributionTypeId) AS [LastContributionDate]
			, 0 AS [IsSystem]
			, newid() AS [Guid]
			, getdate() AS [CreateDate]
		FROM cteIndividual i
		WHERE [FamilyRole] = 'Adult') AS g
	WHERE g.[LastContributionDate] IS NOT NULL
	-- number of gifts short duration (people w/Giving Group)
	DECLARE @GiftCountShortAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_GIFT_COUNT_SHORT)
	DELETE FROM [AttributeValue] WHERE [AttributeId] = @GiftCountShortAttributeId;
	WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
				AND p.[GivingGroupId] IS NOT NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsNumeric], [IsPersistedValueDirty] )
	SELECT [PersonId], [AttributeId], [GiftCountDurationShort], [IsSystem], [Guid], [CreateDate], [GiftCountDurationShort], 1
    FROM 
		(SELECT 
			[PersonId]
			, @GiftCountShortAttributeId AS [AttributeId]
			, (SELECT COUNT(DISTINCT(ft.[Id])) 
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[GivingGroupId] = i.[GivingGroupId]
							AND ft.TransactionTypeValueId = @ContributionTypeId
							AND ft.TransactionDateTime >= @SundayGivingDurationShort
							AND ft.SundayDate <= @SundayDateStart) AS [GiftCountDurationShort]
			, 0 AS [IsSystem]
			, newid() AS [Guid]
			, getdate() AS [CreateDate]
		FROM cteIndividual i
		WHERE [FamilyRole] = 'Adult') AS g
	WHERE g.[GiftCountDurationShort] IS NOT NULL
	-- number of gifts short duration (people WITHOUT Giving Group)
	;WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
				AND p.[GivingGroupId] IS NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsNumeric], [IsPersistedValueDirty])
	SELECT [PersonId], [AttributeId], [GiftCountDurationShort], [IsSystem], [Guid], [CreateDate], [GiftCountDurationShort], 1
    FROM 
		(SELECT 
			[PersonId]
			, @GiftCountShortAttributeId AS [AttributeId]
			, (SELECT COUNT(DISTINCT(ft.[Id])) 
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[Id] = i.[PersonId] -- match by person id
							AND ft.TransactionTypeValueId = @ContributionTypeId
							AND ft.TransactionDateTime >= @SundayGivingDurationShort
							AND ft.SundayDate <= @SundayDateStart) AS [GiftCountDurationShort]
			, 0 AS [IsSystem]
			, newid() AS [Guid]
			, getdate() AS [CreateDate]
		FROM cteIndividual i
		WHERE [FamilyRole] = 'Adult') AS g
	WHERE g.[GiftCountDurationShort] IS NOT NULL
	-- number of gifts long duration (people w/Giving Group)
	DECLARE @GiftCountLongAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = @cATTRIBUTE_GIFT_COUNT_LONG)
	DELETE FROM [AttributeValue] WHERE [AttributeId] = @GiftCountLongAttributeId;
	WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
				AND p.[GivingGroupId] IS NOT NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsNumeric], [IsPersistedValueDirty])
	SELECT [PersonId], [AttributeId], [GiftCountDurationLong], [IsSystem], [Guid], [CreateDate], [GiftCountDurationLong], 1
    FROM 
		(SELECT 
			[PersonId]
			, @GiftCountLongAttributeId AS [AttributeId]
			, (SELECT COUNT(DISTINCT(ft.[Id])) 
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[GivingGroupId] = i.[GivingGroupId]
							AND ft.TransactionTypeValueId = @ContributionTypeId
							AND ft.TransactionDateTime >= @SundayGivingDurationLong
							AND ft.SundayDate <= @SundayDateStart) AS [GiftCountDurationLong]
			, 0 AS [IsSystem]
			, newid() AS [Guid]
			, getdate() AS [CreateDate]
		FROM cteIndividual i
		WHERE [FamilyRole] = 'Adult') AS g
	WHERE g.[GiftCountDurationLong] IS NOT NULL
	
	-- number of gifts long duration (people WITHOUT Giving Group)
	;WITH
	    cteIndividual ([PersonId], [GivingGroupId], [FamilyRole])
	    AS
	    (
		SELECT p.[Id] AS [PersonId], p.[GivingGroupId], CASE WHEN fr.[FamilyRole] = @AdultRoleId THEN 'Adult' ELSE 'Child' END
		FROM [Person] p
		CROSS APPLY
			(
			SELECT TOP 1 gm.[GroupRoleId] AS [FamilyRole]
			FROM    
				[GroupMember] gm 
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
				INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
			WHERE 
				gm.[PersonId] = p.[Id] 
				AND p.[GivingGroupId] IS NULL
				AND gm.IsArchived = 0
                AND g.IsArchived = 0
			) fr
		WHERE
			[RecordStatusValueId] = @ActiveRecordStatusValueId -- record is active
			AND [RecordTypeValueId] = @PersonRecordTypeValueId  -- person record type (not business)
	    )
	INSERT INTO AttributeValue ([EntityId], [AttributeId], [Value], [IsSystem], [Guid], [CreatedDateTime], [ValueAsNumeric], [IsPersistedValueDirty])
	SELECT [PersonId], [AttributeId], [GiftCountDurationLong], [IsSystem], [Guid], [CreateDate], [GiftCountDurationLong], 1
    FROM 
		(SELECT 
			[PersonId]
			, @GiftCountLongAttributeId AS [AttributeId]
			, (SELECT COUNT(DISTINCT(ft.[Id])) 
						FROM [FinancialTransaction] ft
							INNER JOIN [PersonAlias] pa ON pa.[Id] = ft.[AuthorizedPersonAliasId]
							INNER JOIN [Person] gp ON gp.[Id] = pa.[PersonId]
							INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
							INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.AccountId
						WHERE 
							gp.[Id] = i.[PersonId] -- match by person id
							AND ft.TransactionTypeValueId = @ContributionTypeId
							AND ft.TransactionDateTime >= @SundayGivingDurationLong
							AND ft.SundayDate <= @SundayDateStart) AS [GiftCountDurationLong]
			, 0 AS [IsSystem]
			, newid() AS [Guid]
			, getdate() AS [CreateDate]
		FROM cteIndividual i
		WHERE [FamilyRole] = 'Adult') AS g
	WHERE g.[GiftCountDurationLong] IS NOT NULL
	
END
" );
        }

        #endregion

        #region JC: LMS

        private void LMSUpdatesUp()
        {
            LMSProgramListUp();
            ActivitiesAvailableSystemCommunicationUp();
        }

        private void LMSProgramListUp()
        {
            // Attribute for BlockType
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Attribute: Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA1460D8-E895-4B23-8A8E-10EBBED3990F", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "Lava Template", @"The lava template to use to render the page. Merge fields include: Programs, ShowCompletionStatus, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", 1, @"
//- Styles
{% stylesheet %}
    .page-container {
        display: flex;
        flex-direction: column;
    }
    
    .page-header-section {
        align-items: center; 
        border-radius: 12px;
        background-size: cover;
    }
 
    .header-block {
        display: flex;
        flex-direction: column;
        position: relative;
        left: 10%;
        background-color: white;
        border-radius: 12px;
        border: 1px solid #D9EDF2;
        width: 80%;
    }
    
    .page-sub-header {
        padding-left: 10%; 
        padding-right: 10%; 
        margin-bottom: 12px;
        padding-bottom: 12px;
    }
    
    .program-item-container {
        background-color: white; 
        border-radius: 12px;
        margin: 8px;
        display: flex;
        flex-direction: column;
        justify-content: space-between;
    }
{% endstylesheet %}

<div class=""page-container"" >
	<div class=""page-header-section"">
		<div class=""header-block text-center"">
			<div class=""h1 text-bold"">
				Growth Through Learning
			</div>
			<div class=""page-sub-header"">
				We believe that spiritual growth is deeply intertwined with continuous learning. 
				""Growth Through Learning"" is our commitment to nurture your faith journey, providing resources and opportunies to deepen your understanding of God's Word and his purpose for your life.
			</div>
		</div>
	</div>
	
	{% if Programs == empty %}
    	<div class=""programs-list-header-section center-block text-center mt-4 mb-4"">
    		<div class=""program-list-sub-header text-muted"">
 			There are currently no programs available.
		</div>
    	</div>
    {% else %}
    	<div class=""programs-list-header-section center-block text-center mb-4"">
    		<span class=""program-list-header h5"">
    			Programs Available
    		</span>
    
    		<div class=""program-list-sub-header text-muted"">
    			The following types of classes are available.
    		</div>
    	</div>
	{% endif %}
	<div class=""program-list-container d-flex flex-fill"">
		{% for program in Programs %}
		<div class=""program-item-container"">
		
			{% if program.ImageFileGuid and program.ImageFileGuid != '' %}
			<div class=""program-image program-item-header"" >
				<img style=""border-radius: 12px 12px 0 0;"" src=""/GetImage.ashx?guid={{program.ImageFileGuid}}&maxwidth=300&maxheight=150&mode=crop"" />
			</div>
			{% endif %}
			
			<div class=""program-item-middle p-3"">
			
				<h4 class=""program-name"">
					{{ program.Entity.PublicName }}
				</h4>
				<div class=""program-category"">
					<span class=""badge badge-info"">{{ program.Category }}</span>
				</div>
				<div class=""program-summary text-muted"">
					{{ program.Entity.Summary }} 
				</div>
			</div>
			
			<div class=""program-item-footer d-flex justify-content-between mt-4 p-3"">
				<a class=""btn btn-default"" href=""{{ program.CoursesLink }}"">Learn More</a>
				
				{% if ShowCompletionStatus %}
    				{% if program.CompletionStatus == 'Completed' %}
    					<span class=""badge badge-success p-2"" style=""line-height: normal;"">Completed</span>
    				{% elseif program.CompletionStatus == 'Pending' %}
    					<span class=""badge badge-info p-2"" style=""line-height: normal;"">Enrolled</span>
    				{% endif %}
				{% endif %}
			</div>
		</div>
		{% endfor %}
	</div>
</div>
", "D7B2012B-3D59-4DE3-8328-53B7EB9F7C1C" );
        }

        /// <summary>
        /// Updates the SystemCommunication for Newly Available Learning Activities.
        /// </summary>
        private void ActivitiesAvailableSystemCommunicationUp()
        {
            Sql( @"
UPDATE s SET
	Subject = 'New {%if ActivityCount > 1 %}Activities{%else%}Activity{%endif%} Available',
	Body = '
{% assign currentDate = ''Now'' | Date:''MMMM d, yyyy'' %}
{{ ''Global'' | Attribute:''EmailHeader'' }}
<h1 style=""margin:0;"">
	Your Activities
</h1>

<p>
    Below are your newly available activities as of {{ currentDate }}.
</p>

{% for course in Courses %}
	<h2> {{course.ProgramName}}: {{course.CourseName}} - {{course.CourseCode}} </h2>
	
	{% for activity in course.Activities %}
		<p class=""mb-4"">
			<strong>Activity:</strong>
			{{activity.ActivityName}}
				{% if activity.AvailableDate == null %}
					(always available)
				{% else %}
					(available {{ activity.AvailableDate | Date: ''MMM dd'' }})
				{% endif %}
			<br />
			<strong>Due:</strong>
			{% if activity.DueDate == null %}
				Optional
			{% else %}
				{{ activity.DueDate | HumanizeDateTime }}
			{% endif %}
		</p>	
	{% endfor %}
{% endfor %}

{{ ''Global'' | Attribute:''EmailFooter'' }}'
from systemcommunication s
WHERE [Guid] = 'D40A9C32-F179-4E5E-9B0D-CE208C5D1870'
" );
        }

        #endregion

        #region JC: Prayers

        private void PrayerAutomationUp()
        {
            RockMigrationHelper.UpdateCategory( SystemGuid.EntityType.CATEGORY, "AI Automations", "fa fa-brain", "Configurations for AI Automation", SystemGuid.Category.AI_AUTOMATION );

            var categoryName = "Global";
            RockMigrationHelper.AddDefinedType( categoryName, "Sentiment Emotions", "The sentiment of the related text as determined by an AI automation.", SystemGuid.DefinedType.SENTIMENT_EMOTIONS );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.SENTIMENT_EMOTIONS, "Anger", "The most identifiable sentiment of the available options is anger." );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.SENTIMENT_EMOTIONS, "Anticipation", "The most identifiable sentiment of the available options is anticipation." );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.SENTIMENT_EMOTIONS, "Disgust", "The most identifiable sentiment of the available options is disgust." );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.SENTIMENT_EMOTIONS, "Fear", "The most identifiable sentiment of the available options is fear." );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.SENTIMENT_EMOTIONS, "Joy", "The most identifiable sentiment of the available options is joy." );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.SENTIMENT_EMOTIONS, "Neutral", "The most identifiable sentiment of the available options is neutral." );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.SENTIMENT_EMOTIONS, "Sadness", "The most identifiable sentiment of the available options is sadness." );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.SENTIMENT_EMOTIONS, "Worry", "The most identifiable sentiment of the available options is worry." );

            Rock.Web.SystemSettings.SetValue( "core_PrayerRequestAICompletions", PrayerRequestAICompletionTemplate().ToJson() );

            RockMigrationHelper.UpdateFieldType( "AI Provider", "Field type to select an AI Provider.", "Rock", "Rock.Field.Types.AIProviderFieldType", SystemGuid.FieldType.AI_PROVIDER_PICKER );

            AIAutomationAttributesUp();
            PrayerCategoriesPageUp();
        }

        private void PrayerCategoriesPageUp()
        {
            var categoryBlockTypeGuid = "E0EF4ED1-A621-43A1-A75D-C6319F7F10D2";
            var categoryTreeViewInstanceGuid = "42E90A50-D8EC-4370-B970-83E48518BC26";
            var categoryDetailInstanceGuid = "4B617C53-556E-4A1C-882E-D86BBC7B2CBC";
            var categoryTreeViewBlockTypeGuid = "ADE003C7-649B-466A-872B-B8AC952E7841";

            RockMigrationHelper.UpdatePageLayout( SystemGuid.Page.PRAYER_CATEGORIES, SystemGuid.Layout.LEFT_SIDEBAR_INTERNAL_SITE );
            RockMigrationHelper.DeleteBlock( categoryBlockTypeGuid );
            RockMigrationHelper.AddBlock( true, SystemGuid.Page.PRAYER_CATEGORIES, null, categoryTreeViewBlockTypeGuid, "Category Tree View", "Sidebar1", "", "", 0, categoryTreeViewInstanceGuid );
            RockMigrationHelper.AddBlock( true, SystemGuid.Page.PRAYER_CATEGORIES, null, SystemGuid.BlockType.OBSIDIAN_CATEGORY_DETAIL, "Category Detail", "Main", "", "", 0, categoryDetailInstanceGuid );

            Sql( $@"
-- Page and Block Type Guids.
DECLARE @prayerCategoriesPageGuid NVARCHAR(40) = '{SystemGuid.Page.PRAYER_CATEGORIES}';
DECLARE @categoryDetailBlockTypeGuid NVARCHAR(40) = '{SystemGuid.BlockType.OBSIDIAN_CATEGORY_DETAIL}';
DECLARE @categoryTreeViewBlockTypeGuid NVARCHAR(40) = '{categoryTreeViewBlockTypeGuid}';


-- Entity Type Id and Guid.
DECLARE @prayerRequestEntityTypeGuid NVARCHAR(40) = '{SystemGuid.EntityType.PRAYER_REQUEST}';
DECLARE @prayerRequestEntityTypeId NVARCHAR(40) = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = @prayerRequestEntityTypeGuid);

-- Attribute Guids
DECLARE @pageParameterKeyTreeViewAttributeGuid NVARCHAR(40) = 'AA057D3E-00CC-42BD-9998-600873356EDB';
DECLARE @entityTypeTreeViewAttributeGuid NVARCHAR(40) = '06D414F0-AA20-4D3C-B297-1530CCD64395';
DECLARE @detailPageTreeViewAttributeGuid NVARCHAR(40) = 'AEE521D8-124D-4BB3-8A80-5F368E5CEC15';
DECLARE @entityTypeCategoryDetailAttributeGuid NVARCHAR(40) = '3C6E056B-5087-4E02-B9FD-853B658E3C85';

-- Block Ids based on BlockType.Guid and Page.Guid
DECLARE @categoryTreeViewBlockId INT = (
	SELECT TOP 1 b.[Id]
	FROM [Page] p
	JOIN [Block] b ON b.[PageId] = p.[Id]
	JOIN [BlockType] bt ON bt.[Id] = b.[BlockTypeId]
	WHERE p.[Guid] = @prayerCategoriesPageGuid
		AND bt.[Guid] = @categoryTreeViewBlockTypeGuid
);
DECLARE @categoryDetailBlockId INT = (
	SELECT TOP 1 b.[Id]
	from [Page] p
	JOIN [Block] b ON b.[PageId] = p.[Id]
	JOIN [BlockType] bt ON bt.[Id] = b.BlockTypeId
	WHERE p.[Guid] = @prayerCategoriesPageGuid
		AND bt.[Guid] = @categoryDetailBlockTypeGuid
);

-- Add AttributeValues for the Category Tree View block instance.
INSERT AttributeValue (
	[IsSystem],
	[AttributeId],
	[EntityId],
	[Value],
	[Guid],
	[IsPersistedValueDirty]
)
SELECT 0, a.[AttributeId], @categoryTreeViewBlockId, a.[Value], NEWID(), 1
FROM (
	SELECT 
		a.[Id] [AttributeId], 
		CASE a.[Guid] 
			WHEN @pageParameterKeyTreeViewAttributeGuid THEN 'CategoryId'
			WHEN @entityTypeTreeViewAttributeGuid THEN @prayerRequestEntityTypeGuid
			WHEN @detailPageTreeViewAttributeGuid THEN @prayerCategoriesPageGuid -- Use the same page Guid.
		END [Value]
	FROM [Attribute] a
	WHERE a.[Guid] IN (
		@pageParameterKeyTreeViewAttributeGuid,
		@entityTypeTreeViewAttributeGuid,
		@detailPageTreeViewAttributeGuid
	)
) a
WHERE NOT EXISTS (
	SELECT *
	FROM [AttributeValue] ex
	WHERE ex.[AttributeId] = a.[AttributeId]
		AND ex.[EntityId] = @categoryTreeViewBlockId
)

-- Add AttributeValues for the Category Detail block instance.
INSERT [AttributeValue] (
	[IsSystem],
	[AttributeId],
	[EntityId],
	[Value],
	[Guid],
	[IsPersistedValueDirty]
)
SELECT 0, a.[Id], @categoryDetailBlockId, @prayerRequestEntityTypeId, NEWID(), 1
FROM [Attribute] a
WHERE a.[Guid] = @entityTypeCategoryDetailAttributeGuid
	AND NOT EXISTS (
	    SELECT *
	    FROM [AttributeValue] ex
	    WHERE ex.[AttributeId] = a.Id
		    AND ex.[EntityId] = @categoryDetailBlockId
)" );
        }

        private void AIAutomationAttributesUp()
        {
            Sql( $@"
DECLARE @categoryEntityTypeId INT = (SELECT Id FROM EntityType WHERE [Guid] = '{SystemGuid.EntityType.CATEGORY}');
DECLARE @prayerRequestEntityTypeId INT = (SELECT Id FROM EntityType WHERE [Guid] = '{SystemGuid.EntityType.PRAYER_REQUEST}');
DECLARE @nextOrder INT = (SELECT MAX([Order]) FROM Attribute);

DECLARE @aiProviderAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_AI_PROVIDER}';
DECLARE @aiTextEnhancementAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_TEXT_ENHANCEMENT}';
DECLARE @aiNameRemovalAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_REMOVE_NAMES}';
DECLARE @aiSentimentAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_CLASSIFY_SENTIMENT}';
DECLARE @aiAutoCategorizeAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_AUTO_CATEGORIZE}';
DECLARE @aiAIModerationAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_ENABLE_AI_MODERATION}';
DECLARE @aiModerationWorkflowAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_MODERATION_ALERT_WORKFLOW_TYPE}';
DECLARE @aiPublicAppropriatenessAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_CHECK_PUBLIC_APPROPRIATENESS}';
DECLARE @aiChildCategoriesInheritAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_CHILD_CATEGORIES_INHERIT_CONFIGURATION}';

DECLARE @aiProviderPickerFieldTypeId INT = (SELECT TOP 1 Id FROM FieldType WHERE [Guid] = '{SystemGuid.FieldType.AI_PROVIDER_PICKER}');
DECLARE @booleanFieldTypeId INT = (SELECT TOP 1 Id FROM FieldType WHERE [Guid] = '{SystemGuid.FieldType.BOOLEAN}');
DECLARE @singleSelectFieldTypeId INT = (SELECT TOP 1 Id FROM FieldType WHERE [Guid] = '{SystemGuid.FieldType.SINGLE_SELECT}');
DECLARE @workflowTypeFieldTypeId INT = (SELECT TOP 1 Id FROM FieldType WHERE [Guid] = '{SystemGuid.FieldType.WORKFLOW_TYPE}');
DECLARE @definedValuePickerFieldTypeId INT = (SELECT TOP 1 Id FROM FieldType WHERE [Guid] = '{SystemGuid.FieldType.DEFINED_VALUE}');

INSERT [Attribute] ( [IsSystem], [FieldTypeId], [Key], [Name], [Order], [IsGridColumn], [IsMultiValue], [IsRequired], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Description], [Guid] )
SELECT [IsSystem], [FieldTypeId], [Key], [Name], [Order], [IsGridColumn], [IsMultiValue], [IsRequired], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Description], [Guid]
FROM (
		  SELECT 0 [IsSystem], @aiProviderPickerFieldTypeId [FieldTypeId], 'AIProvider' [Key], 'AI Provider' [Name], @nextOrder [Order], 1 [IsGridColumn], 0 [IsMultiValue], 0 [IsRequired], @categoryEntityTypeId [EntityTypeId], 'EntityTypeId' [EntityTypeQualifierColumn], @prayerRequestEntityTypeId [EntityTypeQualifierValue], 'Determines what AI service to use for the processing. If no value is provided the first active provider configured will be used.' [Description], NULL [DefaultValue], @aiProviderAttributeGuid [Guid]
	UNION SELECT 0 [IsSystem], @singleSelectFieldTypeId [FieldTypeId], 'PrayerRequestTextEnhancement' [Key], 'Prayer Request Text Enhancement' [Name], @nextOrder + 1 [Order], 1 [IsGridColumn], 0 [IsMultiValue], 0 [IsRequired], @categoryEntityTypeId [EntityTypeId], 'EntityTypeId' [EntityTypeQualifierColumn], @prayerRequestEntityTypeId [EntityTypeQualifierValue], 'Determines if the AI should attempt to polish the text of the request.' [Description], '' [DefaultValue], @aiTextEnhancementAttributeGuid [Guid]
	UNION SELECT 0 [IsSystem], @singleSelectFieldTypeId [FieldTypeId], 'RemoveNames' [Key], 'Remove Names' [Name], @nextOrder + 2 [Order], 1 [IsGridColumn], 0 [IsMultiValue], 0 [IsRequired], @categoryEntityTypeId [EntityTypeId], 'EntityTypeId' [EntityTypeQualifierColumn], @prayerRequestEntityTypeId [EntityTypeQualifierValue], 'Determines if the AI should remove names from the request text.' [Description], '' [DefaultValue], @aiNameRemovalAttributeGuid [Guid]
	UNION SELECT 0 [IsSystem], @booleanFieldTypeId [FieldTypeId], 'ClassifySentiment' [Key], 'Classify Sentiment' [Name], @nextOrder + 3 [Order], 1 [IsGridColumn], 0 [IsMultiValue], 0 [IsRequired], @categoryEntityTypeId [EntityTypeId], 'EntityTypeId' [EntityTypeQualifierColumn], @prayerRequestEntityTypeId [EntityTypeQualifierValue], 'Determines if the AI should determine the type of emotion found in the request. The list of sentiments are configured in the Sentiment Emotions defined type.' [Description], 'False' [DefaultValue], @aiSentimentAttributeGuid [Guid]
	UNION SELECT 0 [IsSystem], @booleanFieldTypeId [FieldTypeId], 'AutoCategorize' [Key], 'Auto Categorize' [Name], @nextOrder + 4 [Order], 1 [IsGridColumn], 0 [IsMultiValue], 0 [IsRequired], @categoryEntityTypeId [EntityTypeId], 'EntityTypeId' [EntityTypeQualifierColumn], @prayerRequestEntityTypeId [EntityTypeQualifierValue], 'Determines if the AI should pick the child category that best matches the text of the request.' [Description], 'False' [DefaultValue], @aiAutoCategorizeAttributeGuid [Guid]
	UNION SELECT 0 [IsSystem], @booleanFieldTypeId [FieldTypeId], 'EnableAIModeration' [Key], 'Enable AI Moderation' [Name], @nextOrder + 5 [Order], 1 [IsGridColumn], 0 [IsMultiValue], 0 [IsRequired], @categoryEntityTypeId [EntityTypeId], 'EntityTypeId' [EntityTypeQualifierColumn], @prayerRequestEntityTypeId [EntityTypeQualifierValue], 'Determines if the AI should moderate the request. This will determine if any of the moderation categories (e.g. Self-Harm, Violence, Sexual etc.) are present.' [Description], 'False' [DefaultValue], @aiAIModerationAttributeGuid [Guid]
	UNION SELECT 0 [IsSystem], @workflowTypeFieldTypeId [FieldTypeId], 'ModerationAlertWorkflowType' [Key], 'Moderation Alert Workflow Type' [Name], @nextOrder + 6 [Order], 1 [IsGridColumn], 0 [IsMultiValue], 0 [IsRequired], @categoryEntityTypeId [EntityTypeId], 'EntityTypeId' [EntityTypeQualifierColumn], @prayerRequestEntityTypeId [EntityTypeQualifierValue], 'The workflow to launch if any of the moderation categories are found.' [Description], '' [DefaultValue], @aiModerationWorkflowAttributeGuid [Guid]
	UNION SELECT 0 [IsSystem], @booleanFieldTypeId [FieldTypeId], 'CheckforPublicAppropriateness' [Key], 'Check for Public Appropriateness' [Name], @nextOrder + 7 [Order], 1 [IsGridColumn], 0 [IsMultiValue], 0 [IsRequired], @categoryEntityTypeId [EntityTypeId], 'EntityTypeId' [EntityTypeQualifierColumn], @prayerRequestEntityTypeId [EntityTypeQualifierValue], 'Determines if the AI should review the text for public appropriateness.' [Description], 'False' [DefaultValue], @aiPublicAppropriatenessAttributeGuid [Guid]
	UNION SELECT 0 [IsSystem], @booleanFieldTypeId [FieldTypeId], 'ChildCategoriesInherit' [Key], 'Child Categories Inherit Configuration' [Name], @nextOrder + 8 [Order], 1 [IsGridColumn], 0 [IsMultiValue], 0 [IsRequired], @categoryEntityTypeId [EntityTypeId], 'EntityTypeId' [EntityTypeQualifierColumn], @prayerRequestEntityTypeId [EntityTypeQualifierValue], 'Specifies whether this configuration should apply to requests in categories under this parent.' [Description], 'False' [DefaultValue], @aiChildCategoriesInheritAttributeGuid [Guid]
) [seed]
WHERE NOT EXISTS (
	SELECT 1
	FROM [Attribute] [ex]
	WHERE [ex].[Guid] = [seed].[Guid]
);

-- AttributeQualifer configuration for SingleSelect value lists.
DECLARE @valuesQualifierKey NVARCHAR(100) = 'values';
DECLARE @textEnhancementValuesList NVARCHAR(MAX) = '1^Minor Formatting and Spelling,2^Enhance Readability';
DECLARE @nameRemovalValuesList NVARCHAR(MAX) = '1^Last Names Only,2^First and Last Names';

-- AttributeQualifier configuration for SingleSelect as a radio button list.
DECLARE @radioButtonControlTypeQualifierKey NVARCHAR(100) = 'fieldtype';
DECLARE @radioButtonControlTypeId NVARCHAR(MAX) = 'rb';

-- AttributeQualifier configuration for Booleans as a checkbox.
DECLARE @booleanControlTypeQualifierKey NVARCHAR(100) = 'BooleanControlType';
DECLARE @checkboxControlTypeId NVARCHAR(MAX) = '1';

-- Add value lists for single selects.
INSERT [AttributeQualifier] ( [IsSystem], [AttributeId], [Key], [Value], [Guid] )
SELECT 
	0 [IsSystem],
	a.Id [AttributeId], 
	@valuesQualifierKey [Key], 
	-- Use appropriate value list for the attribute being inserted.
	IIF(
		a.[Guid] = @aiTextEnhancementAttributeGuid, 
		@textEnhancementValuesList, 
		@nameRemovalValuesList
	) [Value], 
	NEWID() [Guid]
FROM [Attribute] a
WHERE a.[Guid] IN (
	-- Single-Select field types.
	@aiTextEnhancementAttributeGuid,
	@aiNameRemovalAttributeGuid
)	
	AND NOT EXISTS (
		SELECT *
		FROM [AttributeQualifier] aq
		WHERE aq.AttributeId = a.Id
			AND aq.[Key] = @valuesQualifierKey
	)

-- Add control type qualifiers for both single selects and boolean field types.
INSERT [AttributeQualifier] ( [IsSystem], [AttributeId], [Key], [Value], [Guid] )
SELECT 
	0 [IsSystem],
	a.Id [AttributeId], 
	-- Use the fieldtype key for single-selects and the BooleanControlType for booleans.
	IIF(
		a.[Guid] IN (@aiTextEnhancementAttributeGuid, @aiNameRemovalAttributeGuid), 
		@radioButtonControlTypeQualifierKey, 
		@booleanControlTypeQualifierKey
	) [Key], 
	-- Use the 'rb' value for single-selects/radio buttons and '1' for booleans.
	IIF(
		a.[Guid] IN (@aiTextEnhancementAttributeGuid, @aiNameRemovalAttributeGuid), 
		@radioButtonControlTypeId, 
		@checkboxControlTypeId
	) [Value], 
	NEWID() [Guid]
FROM [Attribute] a
WHERE a.[Guid] IN (
	-- Single-Select field types.
	@aiTextEnhancementAttributeGuid,
	@aiNameRemovalAttributeGuid,

	-- Boolean field types.
	@aiSentimentAttributeGuid,
	@aiAutoCategorizeAttributeGuid,
	@aiAIModerationAttributeGuid,
	@aiPublicAppropriatenessAttributeGuid,
	@aiChildCategoriesInheritAttributeGuid
)	
	AND NOT EXISTS (
		SELECT *
		FROM [AttributeQualifier] aq
		WHERE aq.AttributeId = a.Id
			AND aq.[Key] = IIF(
				a.[Guid] IN (@aiTextEnhancementAttributeGuid, @aiNameRemovalAttributeGuid), 
				@radioButtonControlTypeQualifierKey, 
				@booleanControlTypeQualifierKey
			)
	)

DECLARE @aiAutomationsCategoryId INT = (SELECT Id FROM Category WHERE [Guid] = '{SystemGuid.Category.AI_AUTOMATION}' );

INSERT [AttributeCategory] (AttributeId, CategoryId)
SELECT a.Id, @aiAutomationsCategoryId
FROM [Attribute] a
WHERE a.[Guid] IN (
	@aiProviderAttributeGuid,
	@aiTextEnhancementAttributeGuid,
	@aiNameRemovalAttributeGuid,
	@aiSentimentAttributeGuid,
	@aiAutoCategorizeAttributeGuid,
	@aiAIModerationAttributeGuid,
	@aiModerationWorkflowAttributeGuid,
	@aiPublicAppropriatenessAttributeGuid,
	@aiChildCategoriesInheritAttributeGuid
)
	AND NOT EXISTS (
		SELECT *
		FROM [AttributeCategory] ex
		WHERE ex.AttributeId = a.Id
			AND ex.CategoryId = @aiAutomationsCategoryId
	)" );
        }

        private PrayerRequest.PrayerRequestAICompletions PrayerRequestAICompletionTemplate()
        {
            return new PrayerRequest.PrayerRequestAICompletions
            {
                PrayerRequestFormatterTemplate = @"
{%- comment -%}
    This is the lava template for the AI automation that occurs in the PrayerRequest PostSave SaveHook.
    Available Lava Fields:

    PrayerRequest - The PrayerRequest entity object.
    ParentCategoryId - The integer identifier of the parent category (for the PrayerRequest).
    HasTextTransformations - True if the AI automation is configured to perform any text changes.
    AutoCategorize - True if the AI automation is configured to auto-categorize the prayer request.
    ClassifySentiment - True if the AI automation is configured to classify the sentiment of the prayer request.
    CheckAppropriateness - True if the AI automation is configured to check if prayer request is appropriate for public viewing.
    EnableFixFormattingAndSpelling - True if the AI Automation AttributeValue for TextEnhancement is equal to MinorFormattingAndSpelling; otherwise false.
    EnableEnhancedReadability - True if the AI Automation AttributeValue for TextEnhancement is equal to EnhanceReadability; otherwise false.    
    EnableRemovalLastNames - True if the AI Automation AttributeValue for NameRemoval is equal to LastNamesOnly; otherwise false.
    EnableRemovalFirstAndLastNames - True if the AI Automation AttributeValue for NameRemoval is equal to FirstAndLastNames; otherwise false.
    Categories - The child categories of the currently selected Prayer Request category or children of ""All Church"" if no category is selected.
    SentimentEmotions - The Sentiment Emotions DefinedType with DefinedValues from Cache (well-known Guid: C9751C20-DA81-4521-81DE-0099D6F598BA).
{%- endcomment -%}
{%- if EnableRemovalLastNames == true and EnableRemovalFirstAndLastNames == false %}
Remove surname names, but leave first names from the text below.
{%- endif -%}
{%- if EnableRemovalFirstAndLastNames == true %}
Remove names, both first and last, from the text below.
{%- endif -%}
{%- if EnableFixFormattingAndSpelling == ture and EnableEnhancedReadability == false %}
Fix any formatting and spelling mistakes, but do not change the text.
{% endif -%}
{%- if EnableEnhancedReadability == true %}
Make the request more readable and polished.
{% endif -%}

```Prayer Request```
{{ PrayerRequest.Text }}
```Prayer Request```
",
                PrayerRequestAnalyzerTemplate = @"
{%- comment -%}
    This is the lava template for the AI automation that occurs in the PrayerRequest PostSave SaveHook.
    Available Lava Fields:

    PrayerRequest - The PrayerRequest entity object.
    ParentCategoryId - The integer identifier of the parent category (for the PrayerRequest).
    AutoCategorize - True if the AI automation is configured to auto-categorize the prayer request.
    ClassifySentiment - True if the AI automation is configured to classify the sentiment of the prayer request.
    CheckAppropriateness - True if the AI automation is configured to check if prayer request is appropriate for public viewing.
    Categories - The child categories of the currently selected Prayer Request category or children of ""All Church"" if no category is selected.
    SentimentEmotions - The Sentiment Emotions DefinedType with DefinedValues from Cache (well-known Guid: C9751C20-DA81-4521-81DE-0099D6F598BA).
{%- endcomment -%}

{%- if AutoCategorize == true %}

Determine the category for the text delimited by ```Prayer Request```. Include the Id of the category that most closely matches from the JSON list below.



    {%- assign categoriesJson = '[' -%}

    {%- for category in Categories -%}

        {%- capture categoriesJsonRow -%}
            {
                ""Id"": {{ category.Id }},
                ""CategoryName"": {{category.Name | ToJSON }}
            }{% unless forloop.last %},{% endunless %}
        {%- endcapture -%}

        {%- assign categoriesJson = categoriesJson | Append:categoriesJsonRow -%}
    {%- endfor -%}

    {%- assign categoriesJson = categoriesJson | Append: ']' %}

```Categories```
{{ categoriesJson | FromJSON | ToJSON }}
```Categories```
{% endif -%}

{%- if ClassifySentiment == true %}

Determine the person's sentiment for the text. Include the Id of the sentiment that most closely matches from the JSON list below.

    {%- assign sentimentsJson = '[' -%}

    {%- for definedValue in SentimentEmotions.DefinedValues -%}

        {%- capture sentimentsJsonRow -%}
            {
                ""Id"": {{ definedValue.Id }},
                ""Sentiment"": {{ definedValue.Value | ToJSON }}
            }{% unless forloop.last %},{% endunless %}
        {%- endcapture -%}

        {%- assign sentimentsJson = sentimentsJson | Append:sentimentsJsonRow -%}
    {%- endfor -%}

    {%- assign sentimentsJson = sentimentsJson | Append: ']' %}

```Sentiments```
{{ sentimentsJson | FromJSON | ToJSON }}
```Sentiments```
{% endif -%}

{%- if CheckAppropriateness == true -%}
    Determine if the text is appropriate for public viewing being sensitive to privacy and legal concerns.
{%- endif %}

```Prayer Request```
{{ PrayerRequest.Text }}
```Prayer Request```

Respond with ONLY a JSON object in the following format:
{
    {% if ClassifySentiment == true %}""sentimentId"": <The Id of the Sentiment from the list delimited by ```Sentiments``` that most closely matches the modified Prayer Request text>,{% endif %}
    {% if AutoCategorize == true %}""categoryId"": <The Id of the Category from the list delimited by ```Categories``` that most closely matches the modified Prayer Request text>,{% endif %}
    {% if CheckAppropriateness == true %}""isAppropriateForPublic"": <boolean value indicating whether the modified text is appropriate for public viewing>{% endif %}
}
"
            };
        }

        private void PrayerAutomationsDown()
        {
            PrayerCategoriesPageDown();
            AIAutomationAttributesDown();
        }

        private void PrayerCategoriesPageDown()
        {
            var categoryBlockTypeGuid = "620FC4A2-6587-409F-8972-22065919D9AC";
            var categoryBlockGuid = "E0EF4ED1-A621-43A1-A75D-C6319F7F10D2";
            var treeViewInstanceGuid = "42E90A50-D8EC-4370-B970-83E48518BC26";
            var categoryDetailInstanceGuid = "4B617C53-556E-4A1C-882E-D86BBC7B2CBC";
            RockMigrationHelper.UpdatePageLayout( SystemGuid.Page.PRAYER_CATEGORIES, SystemGuid.Layout.FULL_WIDTH );
            RockMigrationHelper.DeleteBlock( treeViewInstanceGuid );
            RockMigrationHelper.DeleteBlock( categoryDetailInstanceGuid );
            RockMigrationHelper.AddBlock( SystemGuid.Page.PRAYER_CATEGORIES, SystemGuid.Layout.FULL_WIDTH, categoryBlockTypeGuid, "Categories", "Main", "", "", 0, categoryBlockGuid );

        }

        private void AIAutomationAttributesDown()
        {
            Sql( $@"
DECLARE @aiProviderAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_AI_PROVIDER}';
DECLARE @aiTextEnhancementAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_TEXT_ENHANCEMENT}';
DECLARE @aiNameRemovalAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_REMOVE_NAMES}';
DECLARE @aiSentimentAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_CLASSIFY_SENTIMENT}';
DECLARE @aiAutoCategorizeAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_AUTO_CATEGORIZE}';
DECLARE @aiAIModerationAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_ENABLE_AI_MODERATION}';
DECLARE @aiModerationWorkflowAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_MODERATION_ALERT_WORKFLOW_TYPE}';
DECLARE @aiPublicAppropriatenessAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_CHECK_PUBLIC_APPROPRIATENESS}';
DECLARE @aiChildCategoriesInheritAttributeGuid NVARCHAR(40) = '{SystemGuid.Attribute.AI_AUTOMATION_CHILD_CATEGORIES_INHERIT_CONFIGURATION}';

DELETE a
FROM Attribute a
WHERE [Guid] IN (
	@aiProviderAttributeGuid,
	@aiTextEnhancementAttributeGuid,
	@aiNameRemovalAttributeGuid,
	@aiSentimentAttributeGuid,
	@aiAutoCategorizeAttributeGuid,
	@aiAIModerationAttributeGuid,
	@aiModerationWorkflowAttributeGuid,
	@aiPublicAppropriatenessAttributeGuid,
	@aiChildCategoriesInheritAttributeGuid
)" );
        }

        #endregion
    }
}
