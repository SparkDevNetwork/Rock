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
using System.Collections.Generic;

namespace Rock.Migrations
{
    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20240104 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateAgeBracketValuesAndMetricsUp();
            UpdateReminderNotificationSystemCommunicationBody();
            ChopEmailPreferenceEntryBlocksUp();
            AddCommunicationIdToEmailMediumUnsubscribeHtmlAttributeValue();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UpdateAgeBracketValuesAndMetricsDown();
            ChopEmailPreferenceEntryBlocksDown();
        }

        /// <summary>
        /// PA: Update the Body of Reminder Notification System Communication
        /// </summary>
        private void UpdateReminderNotificationSystemCommunicationBody()
        {
            Sql( @"
        UPDATE [dbo].[SystemCommunication]
        SET [Body] = REPLACE(Body, '{{ MaxRemindersPerEntityType }} of', '')
        WHERE [Guid] = '7899958C-BC2F-499E-A5CC-11DE1EF8DF20'" );
        }

        /// <summary>
        /// JMH: Chop Email Preference Entry blocks up.
        /// </summary>
        private void ChopEmailPreferenceEntryBlocksUp()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop EmailPreferenceEntry",
                blockTypeReplacements: new Dictionary<string, string> {
            { "B3C076C7-1325-4453-9549-456C23702069", "476FBA19-005C-4FF4-996B-CA1B165E5BC8" }, // Email Preference Entry
                },
                migrationStrategy: "Chop",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_162_CHOP_EMAIL_PREFERENCE_ENTRY );
        }

        /// <summary>
        /// JMH: Chop Email Preference Entry blocks down.
        /// </summary>
        private void ChopEmailPreferenceEntryBlocksDown()
        {
            // Delete the Service Job Entity
            RockMigrationHelper.DeleteByGuid( SystemGuid.ServiceJob.DATA_MIGRATIONS_162_CHOP_EMAIL_PREFERENCE_ENTRY, "ServiceJob" );
        }

        /// <summary>
        /// JMH: Add the CommunicationId to the Email Medium UnsubscribeHTML attribute value.
        /// </summary>
        private void AddCommunicationIdToEmailMediumUnsubscribeHtmlAttributeValue()
        {
            var entityTypeGuid = SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL;
            var attributeKey = "UnsubscribeHTML";
            var oldValue = "<a href='{{ 'Global' | Attribute:'PublicApplicationRoot' }}Unsubscribe/{{ Person | PersonActionIdentifier:'Unsubscribe' }}'>Unsubscribe</a>";
            var newValue = "<a href='{{ 'Global' | Attribute:'PublicApplicationRoot' }}Unsubscribe/{{ Person | PersonActionIdentifier:'Unsubscribe' }}?CommunicationId={{ Communication.Id }}'>Unsubscribe</a>";

            Sql( $@"
-- Escape single quotes in search and replace string.
DECLARE @OldValue       AS nvarchar(max) = N'{oldValue.Replace( "'", "''" )}.'
DECLARE @NewValue       AS nvarchar(max) = N'{newValue.Replace( "'", "''" )}'
DECLARE @AttributeKey   AS nvarchar(1000) = N'{attributeKey}'
DECLARE @EntityTypeGuid AS uniqueidentifier = '{entityTypeGuid}'
DECLARE @SearchPattern  AS nvarchar(max) = N'%' + @OldValue + N'%'
DECLARE @EntityTypeId   AS int = (SELECT [Id] FROM [EntityType] WHERE [Guid] = @EntityTypeGuid)
UPDATE AV
   SET AV.[Value] = REPLACE( AV.[Value], @OldValue, @NewValue )
  FROM [AttributeValue] AV
 INNER JOIN [Attribute] A
       ON AV.[AttributeId] = A.[Id]
 WHERE A.[EntityTypeId] = @EntityTypeId
       AND A.[Key] = @AttributeKey
       AND AV.[Value] LIKE @SearchPattern" );
        }

        /// <summary>
        /// KA: Migration Update AgeBracket values and Metrics
        /// </summary>
        private void UpdateAgeBracketValuesAndMetricsUp()
        {
            // NOTE: This migration is copied over from the rollup migration: 193_MigratoinRollupForV15_4_1 in 15.4. Back then Service Jobs were added using SQL scripts like below.
            // Starting from v16, the AddPostUpdateServiceJob in the MigrationHelper should be used to add a new service job.

            // add ServiceJob: Rock Update Helper v15.4 - Update AgeBracket Values
            Sql( $@"IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.PostV154DataMigrationsUpdateAgeBracketValues' AND [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_154_UPDATE_AGE_BRACKET_VALUES}' )
            BEGIN
               INSERT INTO [ServiceJob] (
                  [IsSystem]
                  ,[IsActive]
                  ,[Name]
                  ,[Description]
                  ,[Class]
                  ,[CronExpression]
                  ,[NotificationStatus]
                  ,[Guid] )
               VALUES ( 
                  1
                  ,1
                  ,'Rock Update Helper v15.4 - Update AgeBracket Values'
                  ,'This job will update the AgeBracket values to reflect the new values after splitting the 0 - 12 bracket.'
                  ,'Rock.Jobs.PostV154DataMigrationsUpdateAgeBracketValues'
                  ,'0 0 21 1/1 * ? *'
                  ,1
                  ,'{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_154_UPDATE_AGE_BRACKET_VALUES}'
                  );
            END" );

            UpdateMetricsUp();
        }

        private void UpdateMetricsUp()
        {
            Sql( $@"
UPDATE Metric SET
	[Title] = '0-5',
	[Description] = 'Active people between the ages of 0 and 5',
	SourceSql = 'DECLARE @ActiveRecordStatusValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = ''618F906C-C33D-4FA3-8AEF-E58CB7B63F1E''
    )
    ,@PersonRecordTypeValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E''
    )     
    ,@Today Date = GetDate()   
	SELECT COUNT(1), P.[PrimaryCampusId]  
    FROM [Person] P  
    WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
	    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
	    AND P.[IsDeceased] = 0
	    AND P.[Guid] != ''7EBC167B-512D-4683-9D80-98B6BB02E1B9''
	    AND P.[AgeBracket] = {Rock.Enums.Crm.AgeBracket.ZeroToFive.ConvertToInt()}    
	GROUP BY ALL P.[PrimaryCampusId] ORDER BY P.[PrimaryCampusId]'
	WHERE [Guid] = 'EEDEE264-F49D-46B9-815D-C5DBB5DCC9CE'

	DECLARE @MetricId [int] = (SELECT [Id] FROM [Metric] WHERE ([Guid] = '909FA7E8-F991-44EC-B2BF-07BEE9A29558'))
    , @SourceValueTypeId [int] = (SELECT [Id] FROM [DefinedValue] WHERE ([Guid] = '6A1E1A1B-A636-4E12-B90C-D7FD1BDAE764'))
    , @MetricCategoryId [int] = (SELECT [Id] FROM [Category] WHERE ([Guid] = '10716347-44BA-44F1-8E82-21C23445EB76'))
    , @Description [varchar] (max) = 'Active people between the ages of 6 and 12';

IF (@MetricId IS NULL AND @SourceValueTypeId IS NOT NULL AND @MetricCategoryId IS NOT NULL)
BEGIN
    DECLARE @Now [datetime] = GETDATE();
    -- Create 06-12 Metric
    INSERT INTO [Metric]
    (
        [IsSystem]
        , [Title]
        , [Description]
        , [IsCumulative]
        , [SourceValueTypeId]
        , [SourceSql]
        , [ScheduleId]
        , [CreatedDateTime]
        , [ModifiedDateTime]
        , [Guid]
        , [NumericDataType]
        , [EnableAnalytics]
    )
    VALUES
    (
        0
        , '06-12'
        , @Description
        , 0
        , @SourceValueTypeId
        , 'DECLARE @ActiveRecordStatusValueId INT = (
		    SELECT TOP 1 Id
            FROM DefinedValue
            WHERE [Guid] = ''618F906C-C33D-4FA3-8AEF-E58CB7B63F1E''
	    )
	    ,@PersonRecordTypeValueId INT = (
	    	SELECT TOP 1 Id
            FROM DefinedValue
            WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E''
	    )     
	    ,@Today Date = GetDate()      
	    SELECT COUNT(1), P.[PrimaryCampusId]
	    FROM [Person] P  
	    WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
	    	AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
	    	AND P.[IsDeceased] = 0
	    	AND P.[Guid] != ''7EBC167B-512D-4683-9D80-98B6BB02E1B9''
	    	AND P.[AgeBracket] = {Rock.Enums.Crm.AgeBracket.SixToTwelve.ConvertToInt()}    
	    GROUP BY ALL P.[PrimaryCampusId] ORDER BY P.[PrimaryCampusId]'
        , (SELECT [Id] FROM Schedule WHERE Guid = '20AFE3A5-86DC-4232-9FAE-9271D7A251FB')
        , @Now
        , @Now
        , '909FA7E8-F991-44EC-B2BF-07BEE9A29558'
        , 1
        , 0
    );
    SET @MetricId = SCOPE_IDENTITY();
    -- Create Metric Category
    INSERT INTO [MetricCategory]
    (
        [MetricId]
        , [CategoryId]
        , [Order]
        , [Guid]
    )
    VALUES
    (
        @MetricId
        , @MetricCategoryId
        , 0
        , NEWID()
    );
    -- Create Partitions for newly created 06-12 Metric
    INSERT INTO [MetricPartition]
    (
        [MetricId]
        , [Label]
        , [EntityTypeId]
        , [IsRequired]
        , [Order]
        , [CreatedDateTime]
        , [ModifiedDateTime]
        , [Guid]
    )
    VALUES
    (
        @MetricId
        , 'CAMPUS'
        , (SELECT Id FROM [EntityType] WHERE GUID = '{Rock.SystemGuid.EntityType.CAMPUS}')
        , 1
        , 0
        , @Now
        , @Now
        , NEWID()
    );
END
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        private void UpdateAgeBracketValuesAndMetricsDown()
        {
            Sql( $"DELETE FROM [ServiceJob] WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_154_UPDATE_AGE_BRACKET_VALUES}'" );
            UpdateMetricsDown();
        }

        private void UpdateMetricsDown()
        {
            Sql( @"
UPDATE Metric SET
	[Title] = '0-12',
	[Description] = 'Active people between the ages of 0 and 12',
	SourceSql = 'DECLARE @ActiveRecordStatusValueId INT = (SELECT TOP 1 Id FROM DefinedValue WHERE [Guid] = ''618F906C-C33D-4FA3-8AEF-E58CB7B63F1E'')
						,@PersonRecordTypeValueId INT = (SELECT TOP 1 Id FROM DefinedValue WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E'')
						,@Today Date = GetDate()    
						SELECT COUNT(1), P.[PrimaryCampusId]  FROM [Person] P  WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
						AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
						AND P.[IsDeceased] = 0
						AND P.[Guid] != ''7EBC167B-512D-4683-9D80-98B6BB02E1B9''
						AND P.[AgeBracket] = 1    
						GROUP BY ALL P.[PrimaryCampusId] ORDER BY P.[PrimaryCampusId]'
	WHERE [Guid] = 'EEDEE264-F49D-46B9-815D-C5DBB5DCC9CE'

DELETE FROM Metric WHERE [Guid] = '909FA7E8-F991-44EC-B2BF-07BEE9A29558'
" );
        }
    }
}
