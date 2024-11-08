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
    using System.Linq;

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20230404 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateLegacyGroupAttendanceDetailBlock();
            JPH_UpdateObsidianBlockCategoriesUp();
            AddPageRouteAndUpdateInsightsMetricsCalculationsUp();
            ChartJsPercentAndCurrencyLabels();
            ContentImageFiletypeWhitelistJpegValueUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            JPH_UpdateObsidianBlockCategoriesDown();
            AddPageRouteAndUpdateInsightsMetricsCalculationsDown();

        }

        /// <summary>
        /// Updates the legacy group attendance detail block.
        /// </summary>
        private void UpdateLegacyGroupAttendanceDetailBlock()
        {
            Sql( @"UPDATE [BlockType] SET [Name] = 'Group Attendance Detail (Legacy)' WHERE [Path] = '~/Blocks/Groups/GroupAttendanceDetail.ascx'" );
        }

        /// <summary>
        /// JPH: Update Obsidian Block Categories Up.
        /// </summary>
        private void JPH_UpdateObsidianBlockCategoriesUp()
        {
            Sql( @"
                UPDATE [BlockType]
                SET [Category] = 'Obsidian > Group'
                WHERE [Guid] = '308DBA32-F656-418E-A019-9D18235027C1';

                UPDATE [BlockType]
                SET [Category] = 'Obsidian > Engagement > Sign-Up'
                WHERE [Guid] = '96D160D9-5668-46EF-9941-702BD3A577DB';

                UPDATE [BlockType]
                SET [Category] = 'Obsidian > Engagement > Sign-Up'
                WHERE [Guid] = '432123B4-8FDD-4A2E-BAF7-927C2B049CAB';

                UPDATE [BlockType]
                SET [Category] = 'Obsidian > Engagement > Sign-Up'
                WHERE [Guid] = '74A20402-00DF-4A87-98D1-B5A8920F1D32';

                UPDATE [BlockType]
                SET [Category] = 'Obsidian > Engagement > Sign-Up'
                WHERE [Guid] = '161587D9-7B74-4D61-BF8E-3CDB38F16A12';" );
        }

        /// <summary>
        /// JPH: Update Obsidian Block Categories Down.
        /// </summary>
        private void JPH_UpdateObsidianBlockCategoriesDown()
        {
            Sql( @"
                UPDATE [BlockType]
                SET [Category] = 'Obsidian > Groups'
                WHERE [Guid] = '308DBA32-F656-418E-A019-9D18235027C1';

                UPDATE [BlockType]
                SET [Category] = 'Engagement > Sign-Up'
                WHERE [Guid] = '96D160D9-5668-46EF-9941-702BD3A577DB';

                UPDATE [BlockType]
                SET [Category] = 'Engagement > Sign-Up'
                WHERE [Guid] = '432123B4-8FDD-4A2E-BAF7-927C2B049CAB';

                UPDATE [BlockType]
                SET [Category] = 'Engagement > Sign-Up'
                WHERE [Guid] = '74A20402-00DF-4A87-98D1-B5A8920F1D32';

                UPDATE [BlockType]
                SET [Category] = 'Engagement > Sign-Up'
                WHERE [Guid] = '161587D9-7B74-4D61-BF8E-3CDB38F16A12';" );
        }

        /// <summary>
        /// KA: Add Insights Page Route and Update Metrics Sql
        /// </summary>
        private void AddPageRouteAndUpdateInsightsMetricsCalculationsUp()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.AddPageRoute( "721C8E32-CAAD-4670-AD1B-04FC42A26BB2", "reporting/insights", "308B3E0D-CE23-4590-ACDB-0A95C4E9C330" );
#pragma warning restore CS0618 // Type or member is obsolete
            Sql( @"
                -- ACTIVE EMAIL
                UPDATE Metric 
                SET SourceSql = 'DECLARE
                    @ActiveRecordStatusValueId INT = (
                        SELECT TOP 1 Id
                        FROM DefinedValue
                        WHERE [Guid] = ''618F906C-C33D-4FA3-8AEF-E58CB7B63F1E''
                    )
                    ,@PersonRecordTypeValueId INT = (
                        SELECT TOP 1 Id
                        FROM DefinedValue
                        WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E''
                    )   

                DECLARE 
	                @ActivePeopleCount INT = (
		                SELECT COUNT(*) 
		                FROM Person
		                WHERE [RecordTypeValueId] = @PersonRecordTypeValueId
		                AND [RecordStatusValueId] = @ActiveRecordStatusValueId
		                AND [IsDeceased] = 0
	                )

                SELECT COUNT(*) * 100 / @ActivePeopleCount, P.[PrimaryCampusId] 
                FROM [Person] P
                WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
                    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
                    AND P.[IsDeceased] = 0
                    AND P.[Email] IS NOT NULL AND P.[IsEmailActive] = 1
                GROUP BY P.PrimaryCampusId'
                WHERE Guid = '0C1C1231-DB5D-4B44-9172-F39B56786960'

                -- AGE
                UPDATE Metric 
                SET SourceSql = 'DECLARE
                    @ActiveRecordStatusValueId INT = (
                        SELECT TOP 1 Id
                        FROM DefinedValue
                        WHERE [Guid] = ''618F906C-C33D-4FA3-8AEF-E58CB7B63F1E''
                    )
                    ,@PersonRecordTypeValueId INT = (
                        SELECT TOP 1 Id
                        FROM DefinedValue
                        WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E''
                    )   

                DECLARE 
	                @ActivePeopleCount INT = (
		                SELECT COUNT(*) 
		                FROM Person
		                WHERE [RecordTypeValueId] = @PersonRecordTypeValueId
		                AND [RecordStatusValueId] = @ActiveRecordStatusValueId
		                AND [IsDeceased] = 0
	                )

                SELECT COUNT(*) * 100 / @ActivePeopleCount, P.[PrimaryCampusId] 
                FROM [Person] P
                WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
                    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
                    AND P.[IsDeceased] = 0
                    AND P.[Age] IS NOT NULL
                GROUP BY P.PrimaryCampusId'
                WHERE Guid = '8046A160-941F-4CCD-9EB6-5BD7601DD536'

                -- DATE OF BIRTH
                UPDATE Metric 
                SET SourceSql = 'DECLARE
                    @ActiveRecordStatusValueId INT = (
                        SELECT TOP 1 Id
                        FROM DefinedValue
                        WHERE [Guid] = ''618F906C-C33D-4FA3-8AEF-E58CB7B63F1E''
                    )
                    ,@PersonRecordTypeValueId INT = (
                        SELECT TOP 1 Id
                        FROM DefinedValue
                        WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E''
                    )   

                DECLARE 
	                @ActivePeopleCount INT = (
		                SELECT COUNT(*) 
		                FROM Person
		                WHERE [RecordTypeValueId] = @PersonRecordTypeValueId
		                AND [RecordStatusValueId] = @ActiveRecordStatusValueId
		                AND [IsDeceased] = 0
	                )

                SELECT COUNT(*) * 100 / @ActivePeopleCount, P.[PrimaryCampusId] 
                FROM [Person] P
                WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
                    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
                    AND P.[IsDeceased] = 0
                    AND P.[BirthDate] IS NOT NULL
                GROUP BY P.PrimaryCampusId'
                WHERE Guid = 'D79DECDD-BA7B-4E4B-81F1-5B6392FD7BD8'

                -- GENDER
                UPDATE Metric 
                SET SourceSql = 'DECLARE
                    @ActiveRecordStatusValueId INT = (
                        SELECT TOP 1 Id
                        FROM DefinedValue
                        WHERE [Guid] = ''618F906C-C33D-4FA3-8AEF-E58CB7B63F1E''
                    )
                    ,@PersonRecordTypeValueId INT = (
                        SELECT TOP 1 Id
                        FROM DefinedValue
                        WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E''
                    )   

                DECLARE 
	                @ActivePeopleCount INT = (
		                SELECT COUNT(*) 
		                FROM Person
		                WHERE [RecordTypeValueId] = @PersonRecordTypeValueId
		                AND [RecordStatusValueId] = @ActiveRecordStatusValueId
		                AND [IsDeceased] = 0
	                )

                SELECT COUNT(*) * 100 / @ActivePeopleCount, P.[PrimaryCampusId] 
                FROM [Person] P
                WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
                    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
                    AND P.[IsDeceased] = 0
                    AND P.[GENDER] != 0
                GROUP BY P.PrimaryCampusId'
                WHERE Guid = 'C4F9A612-D487-4CE0-9D9B-691DC733857D'

                -- HOME ADDRESS
                UPDATE Metric 
                SET SourceSql = 'DECLARE
                    @ActiveRecordStatusValueId INT = (
                        SELECT TOP 1 Id
                        FROM DefinedValue
                        WHERE [Guid] = ''618F906C-C33D-4FA3-8AEF-E58CB7B63F1E''
                    )
                    ,@PersonRecordTypeValueId INT = (
                        SELECT TOP 1 Id
                        FROM DefinedValue
                        WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E''
                    )   

                DECLARE 
	                @ActivePeopleCount INT = (
		                SELECT COUNT(*) 
		                FROM Person
		                WHERE [RecordTypeValueId] = @PersonRecordTypeValueId
		                AND [RecordStatusValueId] = @ActiveRecordStatusValueId
		                AND [IsDeceased] = 0
	                )

                SELECT COUNT(*) * 100 / @ActivePeopleCount AS PercentWithHomeAddress, P.[PrimaryCampusId]
                FROM Person P
                JOIN GroupLocation G
                ON G.[GroupId] = P.[PrimaryFamilyId]
                WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
                    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
                    AND P.[IsDeceased] = 0
	                AND G.[GroupLocationTypeValueId] = (SELECT Id FROM DefinedValue WHERE [Guid] = ''8C52E53C-2A66-435A-AE6E-5EE307D9A0DC'')
                GROUP BY P.PrimaryCampusId'
                WHERE Guid = '7964D01D-41B7-469F-8CE7-0C4A84968E62'

                -- MARITAL STATUS
                UPDATE Metric 
                SET SourceSql = 'DECLARE
                    @ActiveRecordStatusValueId INT = (
                        SELECT TOP 1 Id
                        FROM DefinedValue
                        WHERE [Guid] = ''618F906C-C33D-4FA3-8AEF-E58CB7B63F1E''
                    )
                    ,@PersonRecordTypeValueId INT = (
                        SELECT TOP 1 Id
                        FROM DefinedValue
                        WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E''
                    )   

                DECLARE 
	                @ActivePeopleCount INT = (
		                SELECT COUNT(*) 
		                FROM Person
		                WHERE [RecordTypeValueId] = @PersonRecordTypeValueId
		                AND [RecordStatusValueId] = @ActiveRecordStatusValueId
		                AND [IsDeceased] = 0
	                )

                SELECT COUNT(*) * 100 / @ActivePeopleCount, P.[PrimaryCampusId] 
                FROM [Person] P
                WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
                    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
                    AND P.[IsDeceased] = 0
                    AND P.[MaritalStatusValueId] IS NOT NULL
                GROUP BY P.PrimaryCampusId'
                WHERE Guid = '17AC2A8A-B130-4900-B2BD-203D0F8FF971'

                -- MOBILE PHONE
                UPDATE Metric 
                SET SourceSql = 'DECLARE
                    @ActiveRecordStatusValueId INT = (
                        SELECT TOP 1 Id
                        FROM DefinedValue
                        WHERE [Guid] = ''618F906C-C33D-4FA3-8AEF-E58CB7B63F1E''
                    )
                    ,@PersonRecordTypeValueId INT = (
                        SELECT TOP 1 Id
                        FROM DefinedValue
                        WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E''
                    )     

                DECLARE 
	                @ActivePeopleCount INT = (
		                SELECT COUNT(*) 
		                FROM Person
		                WHERE [RecordTypeValueId] = @PersonRecordTypeValueId
		                AND [RecordStatusValueId] = @ActiveRecordStatusValueId
		                AND [IsDeceased] = 0
	                )

                SELECT COUNT(*) * 100 / @ActivePeopleCount AS PercentWithPhone, P.[PrimaryCampusId]
                FROM [Person] P
                JOIN [PhoneNumber] PH
                ON P.[Id] = PH.[PersonId]
                WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
                    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
                    AND P.[IsDeceased] = 0
                    AND PH.[NumberTypeValueId] = (SELECT Id FROM DefinedValue WHERE [Guid] = ''407E7E45-7B2E-4FCD-9605-ECB1339F2453'')
                GROUP BY P.PrimaryCampusId'
                WHERE Guid = '75A8E234-AEC3-4C75-B902-C3F954616BBC'

                -- PHOTO
                UPDATE Metric 
                SET Description = 'Percent of active people with a profile picture',
                SourceSql = 'DECLARE
                    @ActiveRecordStatusValueId INT = (
                        SELECT TOP 1 Id
                        FROM DefinedValue
                        WHERE [Guid] = ''618F906C-C33D-4FA3-8AEF-E58CB7B63F1E''
                    )
                    ,@PersonRecordTypeValueId INT = (
                        SELECT TOP 1 Id
                        FROM DefinedValue
                        WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E''
                    )   

                DECLARE 
	                @ActivePeopleCount INT = (
		                SELECT COUNT(*) 
		                FROM Person
		                WHERE [RecordTypeValueId] = @PersonRecordTypeValueId
		                AND [RecordStatusValueId] = @ActiveRecordStatusValueId
		                AND [IsDeceased] = 0
	                )

                SELECT COUNT(*) * 100 / @ActivePeopleCount, P.[PrimaryCampusId] 
                FROM [Person] P
                WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
                    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
                    AND P.[IsDeceased] = 0
                    AND P.[PhotoId] IS NOT NULL
                GROUP BY P.PrimaryCampusId'
                WHERE Guid = '4DACA1E0-E768-417C-BB5B-DAB5DC0BDA79'

                -- ACTIVE RECORDS
                UPDATE Metric 
                SET SourceSql = 'DECLARE
                    @ActiveRecordStatusValueId INT = (
                        SELECT TOP 1 Id
                        FROM DefinedValue
                        WHERE [Guid] = ''618F906C-C33D-4FA3-8AEF-E58CB7B63F1E''
                    )
                    ,@PersonRecordTypeValueId INT = (
                        SELECT TOP 1 Id
                        FROM DefinedValue
                        WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E''
                    )  
	
                DECLARE 
	                @ActivePeopleCount INT = (
		                SELECT COUNT(*) 
		                FROM Person
		                WHERE [RecordTypeValueId] = @PersonRecordTypeValueId
		                AND [IsDeceased] = 0
	                )

                SELECT COUNT(*) * 100 / @ActivePeopleCount, P.[PrimaryCampusId], P.[RecordStatusValueId]
                FROM [Person] P
                WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
                    AND P.[IsDeceased] = 0
                GROUP BY P.[RecordStatusValueId], P.[PrimaryCampusId] ORDER BY P.[PrimaryCampusId]'
                WHERE Guid = '7AE9475F-389E-496F-8DF0-508B66ADA6A0'

                DECLARE @RecordStatusPartitionId INT = (SELECT Id FROM MetricPartition WHERE Guid = 'ACEB6744-55F3-435C-B695-05EE3B360A2C') 
                IF (@RecordStatusPartitionId IS NULL)
                BEGIN
	                INSERT INTO [MetricPartition]
	                    (
	                        [MetricId]
	                        , [Label]
	                        , [EntityTypeId]
	                        , [IsRequired]
	                        , [Order]
	                        , [EntityTypeQualifierColumn]
	                        , [EntityTypeQualifierValue]
	                        , [CreatedDateTime]
	                        , [ModifiedDateTime]
	                        , [Guid]
	                    )
	                    VALUES
	                    (
	                        (SELECT Id FROM [Metric] WHERE [GUID] = '7AE9475F-389E-496F-8DF0-508B66ADA6A0')
	                        , 'Record Status'
	                        , (SELECT Id FROM [EntityType] WHERE [GUID] = '53D4BF38-C49E-4A52-8B0E-5E016FB9574E')
	                        , 1
	                        , 2
	                        , 'DefinedTypeId'
	                        , (SELECT Id FROM [DefinedType] WHERE [GUID] = '8522badd-2871-45a5-81dd-c76da07e2e7e')
	                        , GETDATE()
	                        , GETDATE()
	                        , 'ACEB6744-55F3-435C-B695-05EE3B360A2C'
	                    );
                END" );
        }

        /// <summary>
        /// KA: Add Insights Page Route and Update Metrics Sql
        /// </summary>
        private void AddPageRouteAndUpdateInsightsMetricsCalculationsDown()
        {
            RockMigrationHelper.DeletePageRoute( "308B3E0D-CE23-4590-ACDB-0A95C4E9C330" );
        }

        /// <summary>
        /// GJ: Update Chart JS to add Percentage and Currency Labels
        /// </summary>
        private void ChartJsPercentAndCurrencyLabels()
        {
            Sql( MigrationSQL._202304042314453_Rollup_20230404_updatechartjs );
        }

        /// <summary>
        /// Adds 'jpeg' as an allowed image upload file type if 'jpg' is specified.
        /// </summary>
        private void ContentImageFiletypeWhitelistJpegValueUp()
        {
            // The 'Content Image Filetype Whitelist' Attribute ID.
            var attributeId = SqlScalar( $"SELECT TOP 1 [Id] FROM [Attribute] WHERE Guid='{Rock.SystemGuid.Attribute.CONTENT_IMAGE_FILETYPE_WHITELIST}'" )
                .ToStringSafe().AsIntegerOrNull();

            // If the attribute exists (it should, but just in case it was removed for some reason)
            if ( attributeId != null )
            {
                // First we need to check if there is an existing attribute value (just take the first one)
                var attributeValue = SqlScalar( $"SELECT TOP 1 [Value] FROM [AttributeValue] WHERE [AttributeId]={attributeId}" ).ToStringSafe();

                // If so, we need to update that value to include jpeg.
                if ( attributeValue != null )
                {
                    var attributeValues = attributeValue.SplitDelimitedValues().ToList();

                    // We only want to append jpeg if jpg is specified.
                    if ( attributeValues.Any( s => s == "jpg" ) && !attributeValues.Any( s => s == "jpeg" ) )
                    {
                        attributeValues = attributeValues.Append( "jpeg" ).ToList();
                    }

                    // Update the attribute value.
                    var newAttributeValue = attributeValues.JoinStrings( "," );
                    Sql( $"UPDATE [AttributeValue] SET [Value] = '{newAttributeValue.Replace( "'", "''" )}' WHERE AttributeId='{attributeId}'" );
                }
            }

            // Updating the default value for the attribute.
            var defaultAttributeValue = SqlScalar( $"SELECT TOP 1 [DefaultValue] FROM [Attribute] WHERE Guid='{Rock.SystemGuid.Attribute.CONTENT_IMAGE_FILETYPE_WHITELIST}'" ).ToStringSafe();
            var whitelistedValues = defaultAttributeValue.SplitDelimitedValues().ToList();

            if ( whitelistedValues.Any( s => s == "jpg" ) && !whitelistedValues.Any( s => s == "jpeg" ) )
            {
                whitelistedValues = whitelistedValues.Append( "jpeg" ).ToList();
            }

            var newDefaultValue = whitelistedValues.JoinStrings( "," );
            Sql( $"UPDATE [Attribute] SET [DefaultValue] = '{newDefaultValue.Replace( "'", "''" )}' WHERE Guid='{Rock.SystemGuid.Attribute.CONTENT_IMAGE_FILETYPE_WHITELIST}'" );
        }
    }
}
