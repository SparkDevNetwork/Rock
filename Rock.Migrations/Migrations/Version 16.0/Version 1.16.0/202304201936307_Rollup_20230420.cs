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
    public partial class Rollup_20230420 : Rock.Migrations.RockMigration
    {
        private const string SelectPercentageActivePeopleSql = @"
DECLARE
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
    @ActivePeopleCount decimal = (
        SELECT COUNT(*) 
        FROM Person
        WHERE [RecordTypeValueId] = @PersonRecordTypeValueId
        AND [RecordStatusValueId] = @ActiveRecordStatusValueId
		AND [Guid] != ''7EBC167B-512D-4683-9D80-98B6BB02E1B9''
        AND [IsDeceased] = 0
    )

SELECT Round(CAST(COUNT(*) * 100 AS decimal) / @ActivePeopleCount, 1), P.[PrimaryCampusId]
FROM [Person] P
WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
    AND P.[IsDeceased] = 0
	AND [Guid] != ''7EBC167B-512D-4683-9D80-98B6BB02E1B9''
";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateSignUpFinderBlockSettingUp();
            UpdateAgeRangeMetrics();
            UpdateGenderMetrics();
            UpdateInformationCompletenessMetrics();
            UpdateUnCategorizedMetrics();
            UpdateColorsInTBD();
            MobileApplicationDuplicateInteractionJobCleanupUp();
            PersonPreferenceMigrationUp();
            RealTimeVisualizerDataUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UpdateSignUpFinderBlockSettingDown();
            MobileApplicationDuplicateInteractionJobCleanupDown();
            PersonPreferencesMigrationDown();
            RealTimeVisualizerDataDown();
        }

        /// <summary>
        /// JPH: Update Sign-Up Finder Block Setting Up.
        /// </summary>
        private void UpdateSignUpFinderBlockSettingUp()
        {
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Display Project Filters As", "DisplayProjectFiltersAs", "Display Project Filters As", @"Determines if the ""Project Types"", ""Campus"", and ""Named Schedule"" project filters should be shown as checkboxes or multi-select dropdowns.", 0, "Checkboxes", "F4640D8E-0EAC-4DEF-AD7A-3C07E3DD8FBC" );
        }

        /// <summary>
        /// JPH: Update Sign-Up Finder Block Setting Down.
        /// </summary>
        private void UpdateSignUpFinderBlockSettingDown()
        {
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Display Project Filters As", "DisplayProjectFiltersAs", "Display Project Filters As", "Determines if the project filters should be show as checkboxes or multi-select dropdowns.", 0, "Checkboxes", "F4640D8E-0EAC-4DEF-AD7A-3C07E3DD8FBC" );
        }

        private void UpdateAgeRangeMetrics()
        {
            string sql = string.Empty;
            const string updateAgeRangeSql = @"
UPDATE Metric 
SET SourceSql = 'DECLARE @ActiveRecordStatusValueId INT = (
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
	AND P.[AgeBracket] = {0}

GROUP BY ALL P.[PrimaryCampusId] ORDER BY P.[PrimaryCampusId]'
WHERE Guid = '{1}'
";
            sql += string.Format( updateAgeRangeSql, "1", "EEDEE264-F49D-46B9-815D-C5DBB5DCC9CE" );

            sql += string.Format( updateAgeRangeSql, "2", "5FFD8F4C-5199-41D7-BF33-14497DFEDD3F" );

            sql += string.Format( updateAgeRangeSql, "3", "12B581AD-EFE3-4A77-B2E7-80A385EFDDEE" );

            sql += string.Format( updateAgeRangeSql, "4", "95141ECF-F29F-4AEB-A966-B63AE21B9520" );

            sql += string.Format( updateAgeRangeSql, "5", "6EEB5736-E17A-44E2-AC27-0CF933E4EC37" );

            sql += string.Format( updateAgeRangeSql, "6", "411A8AF5-FE70-43E1-8BCE-C1FA52051663" );

            sql += string.Format( updateAgeRangeSql, "7", "93D36C22-6D88-4A2D-BCE2-0CF7CF1FC8F0" );

            sql += string.Format( updateAgeRangeSql, "8", "9BF88708-D258-49DA-928E-CF8D894EF21B" );

            sql += string.Format( updateAgeRangeSql, "0", "54EEDC65-4D5F-4E28-8709-7F282FA9412A" );

            Sql( sql );
        }

        private void UpdateGenderMetrics()
        {
            string sql = string.Empty;
            const string format = @"
UPDATE Metric 
SET SourceSql = 'DECLARE @GenderValue INT = {0} --  0=Unknown, 1=Male, 2=Female,
    ,@ActiveRecordStatusValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = ''618F906C-C33D-4FA3-8AEF-E58CB7B63F1E''
    )
    ,@PersonRecordTypeValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E''
    )        

SELECT COUNT(1), P.[PrimaryCampusId] 
FROM [Person] P
WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
    AND P.[IsDeceased] = 0
    AND P.[Guid] != ''7EBC167B-512D-4683-9D80-98B6BB02E1B9''
    AND P.[Gender] = @GenderValue
GROUP BY ALL P.[PrimaryCampusId] ORDER BY P.[PrimaryCampusId]'

WHERE Guid = '{1}'
";

            sql += string.Format( format, "1", "44A00879-D836-4BA1-8CD1-B74EC2C53D5F" );

            sql += string.Format( format, "2", "71CDC173-8B4E-4AFC-AD0E-925F69D299DA" );

            sql += string.Format( format, "0", "53883982-E730-4396-8B12-0D83304C5880" );

            Sql( sql );
        }

        private void UpdateInformationCompletenessMetrics()
        {
            Sql( $@"
                -- ACTIVE EMAIL
                UPDATE Metric 
                SET SourceSql = '{SelectPercentageActivePeopleSql}
                    AND P.[Email] != ''''
                    AND P.[Email] IS NOT NULL
                    AND P.[IsEmailActive] = 1
                GROUP BY ALL P.PrimaryCampusId'
                WHERE Guid = '0C1C1231-DB5D-4B44-9172-F39B56786960'

                -- AGE
                UPDATE Metric 
                SET SourceSql = '{SelectPercentageActivePeopleSql}
                    AND P.[Age] IS NOT NULL
                GROUP BY ALL P.PrimaryCampusId',
                Description = 'Percent of active people with known ages'
                WHERE Guid = '8046A160-941F-4CCD-9EB6-5BD7601DD536'

                -- DATE OF BIRTH
                UPDATE Metric 
                SET SourceSql = '{SelectPercentageActivePeopleSql}
                    AND P.[BirthDate] IS NOT NULL
                GROUP BY ALL P.PrimaryCampusId'
                WHERE Guid = 'D79DECDD-BA7B-4E4B-81F1-5B6392FD7BD8'

                -- GENDER
                UPDATE Metric 
                SET SourceSql = '{SelectPercentageActivePeopleSql}
                    AND P.[GENDER] != 0
                GROUP BY ALL P.PrimaryCampusId'
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
	                @ActivePeopleCount decimal = (
		                SELECT COUNT(*) 
		                FROM Person
		                WHERE [RecordTypeValueId] = @PersonRecordTypeValueId
		                AND [RecordStatusValueId] = @ActiveRecordStatusValueId
                        AND [Guid] != ''7EBC167B-512D-4683-9D80-98B6BB02E1B9''
		                AND [IsDeceased] = 0
	                )

                SELECT Round(CAST(COUNT(*) * 100 AS decimal) / @ActivePeopleCount, 1) AS PercentWithHomeAddress, P.[PrimaryCampusId]
                FROM Person P
                JOIN GroupLocation G
                ON G.[GroupId] = P.[PrimaryFamilyId]
                WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
                    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
                    AND P.[IsDeceased] = 0
                    AND P.[Guid] != ''7EBC167B-512D-4683-9D80-98B6BB02E1B9''
	                AND G.[GroupLocationTypeValueId] = (SELECT Id FROM DefinedValue WHERE [Guid] = ''8C52E53C-2A66-435A-AE6E-5EE307D9A0DC'')
                GROUP BY ALL P.PrimaryCampusId'
                WHERE Guid = '7964D01D-41B7-469F-8CE7-0C4A84968E62'

                -- MARITAL STATUS
                UPDATE Metric 
                SET SourceSql = '{SelectPercentageActivePeopleSql}
                    AND P.[MaritalStatusValueId] IS NOT NULL
                    AND P.[MaritalStatusValueId] != (SELECT Id FROM DefinedValue WHERE [Guid] = ''99844b92-3d63-4246-bb22-b0db7bda8d01'')
                GROUP BY ALL P.PrimaryCampusId'
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
	                @ActivePeopleCount decimal = (
		                SELECT COUNT(*) 
		                FROM Person
		                WHERE [RecordTypeValueId] = @PersonRecordTypeValueId
		                AND [RecordStatusValueId] = @ActiveRecordStatusValueId
                        AND [Guid] != ''7EBC167B-512D-4683-9D80-98B6BB02E1B9''
		                AND [IsDeceased] = 0
	                )

                SELECT Round(CAST(COUNT(*) * 100 AS decimal) / @ActivePeopleCount, 1) AS PercentWithPhone, P.[PrimaryCampusId]
                FROM [Person] P
                JOIN [PhoneNumber] PH
                ON P.[Id] = PH.[PersonId]
                WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
                    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
                    AND P.[IsDeceased] = 0
                    AND P.[Guid] != ''7EBC167B-512D-4683-9D80-98B6BB02E1B9''
                    AND PH.[NumberTypeValueId] = (SELECT Id FROM DefinedValue WHERE [Guid] = ''407E7E45-7B2E-4FCD-9605-ECB1339F2453'')
                GROUP BY ALL P.PrimaryCampusId'
                WHERE Guid = '75A8E234-AEC3-4C75-B902-C3F954616BBC'

                -- PHOTO
                UPDATE Metric 
                SET SourceSql = '{SelectPercentageActivePeopleSql}
                    AND P.[PhotoId] IS NOT NULL
                GROUP BY ALL P.PrimaryCampusId'
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
	                @ActivePeopleCount decimal = (
		                SELECT COUNT(*) 
		                FROM Person
		                WHERE [RecordTypeValueId] = @PersonRecordTypeValueId
                        AND [Guid] != ''7EBC167B-512D-4683-9D80-98B6BB02E1B9''
		                AND [IsDeceased] = 0
	                )

                SELECT Round(CAST(COUNT(*) * 100 AS decimal) / @ActivePeopleCount, 1), P.[PrimaryCampusId], P.[RecordStatusValueId]
                FROM [Person] P
                WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
                    AND [Guid] != ''7EBC167B-512D-4683-9D80-98B6BB02E1B9''
                    AND P.[IsDeceased] = 0
                GROUP BY ALL P.[RecordStatusValueId], P.[PrimaryCampusId] ORDER BY P.[PrimaryCampusId]'
                WHERE Guid = '7AE9475F-389E-496F-8DF0-508B66ADA6A0'
" );
        }

        private void UpdateUnCategorizedMetrics()
        {
            string sql = string.Empty;
            const string format = @"
UPDATE METRIC
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

SELECT COUNT(1), P.[PrimaryCampusId], {0}
FROM [Person] P
WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
	AND P.[Guid] != ''7EBC167B-512D-4683-9D80-98B6BB02E1B9''
    AND P.[IsDeceased] = 0
    {2}
GROUP BY ALL P.[PrimaryCampusId], {0} ORDER BY P.[PrimaryCampusId]'
WHERE Guid = '{1}'
";

            sql += string.Format( format, "P.[ConnectionStatusValueId]", "08A7360A-642E-4FA9-A5F8-288496D380EF", "AND P.[ConnectionStatusValueId] IS NOT NULL" );
            sql += string.Format( format, "P.[EthnicityValueId]", "B0420908-6AED-487C-BCA4-9B63EA4F87F5", "AND P.[EthnicityValueId] IS NOT NULL" );
            sql += string.Format( format, "P.[RaceValueId]", "3EB53204-CED6-4ACF-8045-288BD2EA8E82", "AND P.[RaceValueId] IS NOT NULL" );
            sql += string.Format( format, "P.[MaritalStatusValueId]", "D9B85AD9-2573-4EAE-8DCF-980FC13B81B5", "AND P.[MaritalStatusValueId] IS NOT NULL" );
            sql += string.Format( @"
UPDATE Metric 
SET SourceSql = 'DECLARE @ActiveRecordStatusValueId INT = (
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

-- People, who are Active, who are not deceased and...
SELECT COUNT(1), P.[PrimaryCampusId]
FROM [Person] P
WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
    AND P.[IsDeceased] = 0
	AND P.[Guid] != ''7EBC167B-512D-4683-9D80-98B6BB02E1B9''
    AND P.[Age] IS NULL
GROUP BY ALL P.[PrimaryCampusId] ORDER BY P.[PrimaryCampusId]'
WHERE Guid = '{0}'
", "54EEDC65-4D5F-4E28-8709-7F282FA9412A" );

            Sql( sql );
        }

        /// <summary>
        /// NA:GJ Update Colors in TBD
        /// </summary>
        private void UpdateColorsInTBD()
        {
            Sql( @"UPDATE [AttributeValue] SET [Value]=N'{
    ""SeriesColors"": [
      ""#38BDF8"",
      ""#A3E635"",
      ""#34D399"",
      ""#FB7185"",
      ""#818CF8"",
      ""#FB923C"",
      ""#C084FC"",
      ""#FBBF24"",
      ""#A8A29E""
    ],
    ""GoalSeriesColor"": ""red"",
    ""Grid"": {
      ""ColorGradient"": null,
      ""Color"": null,
      ""BackgroundColorGradient"": null,
      ""BackgroundColor"": ""transparent"",
      ""BorderWidth"": {
        ""top"": 0,
        ""right"": 0,
        ""bottom"": 1,
        ""left"": 1
      },
      ""BorderColor"": null
    },
    ""XAxis"": {
      ""Color"": ""rgba(81, 81, 81, 0.2)"",
      ""Font"": {
        ""Size"": 10,
        ""Family"": null,
        ""Color"": ""#515151""
      },
      ""DateTimeFormat"": ""%b %e,<br />%Y""
    },
    ""YAxis"": {
      ""Color"": ""rgba(81, 81, 81, 0.2)"",
      ""Font"": {
        ""Size"": null,
        ""Family"": null,
        ""Color"": ""#515151""
      },
      ""DateTimeFormat"": null
    },
    ""FillOpacity"": 0.2,
    ""FillColor"": null,
    ""Legend"": {
      ""BackgroundColor"": ""transparent"",
      ""BackgroundOpacity"": null,
      ""LabelBoxBorderColor"": null
    },
    ""Title"": {
      ""Font"": {
        ""Size"": 16,
        ""Family"": null,
        ""Color"": null
      },
      ""Align"": ""left""
    },
    ""Subtitle"": {
      ""Font"": {
        ""Size"": 12,
        ""Family"": null,
        ""Color"": null
      },
      ""Align"": ""left""
    }
  }' WHERE ([Guid]='81c5ac0e-1065-4d57-8ebb-5bc2e60090b9')" );
        }

        /// <summary>
        /// DH: Person Preferences Data Migration Up
        /// </summary>
        private void PersonPreferenceMigrationUp()
        {
            Sql( $@"IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_160_MOVE_PERSON_PREFERENCES}' )
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
            0
            ,1
            ,'Rock Update Helper v16.0 - Migrate Person Preferences'
            ,'This job will initialize all person preferences from attribute values.'
            ,'Rock.Jobs.PostV16DataMigrationsMovePersonPreferences'
            ,'0 0 21 1/1 * ? *'
            ,1
            ,'{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_160_MOVE_PERSON_PREFERENCES}'
            );
    END" );
        }

        /// <summary>
        /// DH: Person Preferences Data Migration Up
        /// </summary>
        private void PersonPreferencesMigrationDown()
        {
            Sql( $"DELETE FROM [ServiceJob] WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_160_MOVE_PERSON_PREFERENCES}'" );
        }

        /// <summary>
        /// DH: RealTime Visualizer Data Up
        /// </summary>
        private void RealTimeVisualizerDataUp()
        {
            const string STANDARD_ICON_SVG = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";

            // Create the "RealTime Visualizer Themes" Defined Type.
            RockMigrationHelper.AddDefinedType(
                "CMS Settings",
                "RealTime Visualizer Themes",
                "A set of themes used by the RealTime Visualizer block to determine how items are displayed.",
                "B8A57DFE-827A-40C1-B8DE-F6EA0C50B864" );

            RockMigrationHelper.AddDefinedTypeAttribute(
                "B8A57DFE-827A-40C1-B8DE-F6EA0C50B864",
                "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5",
                "Page Template",
                "PageTemplate",
                "The template that will be used to render the initial page content when the block loads.",
                0,
                string.Empty,
                "46A1C62A-109B-45BC-9E64-F817EB2E2183" );

            RockMigrationHelper.AddAttributeQualifier(
                "46A1C62A-109B-45BC-9E64-F817EB2E2183",
                "editorMode",
                "3",
                "B39C4E45-F562-496C-981E-B779B572D64D" );

            RockMigrationHelper.AddDefinedTypeAttribute(
                "B8A57DFE-827A-40C1-B8DE-F6EA0C50B864",
                "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5",
                "Style",
                "Style",
                "The CSS styles that will be injected onto the page to customize how the items look.",
                1,
                string.Empty,
                "A0A3BDEE-89AF-462E-807D-8157CAD8D3A0" );

            RockMigrationHelper.AddAttributeQualifier(
                "A0A3BDEE-89AF-462E-807D-8157CAD8D3A0",
                "editorMode",
                "0",
                "6D406002-394D-4349-83F8-C0CAA456B7C0" );

            RockMigrationHelper.AddDefinedTypeAttribute(
                "B8A57DFE-827A-40C1-B8DE-F6EA0C50B864",
                "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5",
                "Script",
                "Script",
                "The JavaScript that will be used to present each item.",
                2,
                string.Empty,
                "6F875F31-F43B-4C33-9183-33FD78EB21F7" );

            RockMigrationHelper.AddAttributeQualifier(
                "6F875F31-F43B-4C33-9183-33FD78EB21F7",
                "editorMode",
                "4",
                "4EFF8E34-FD03-413C-A9A1-4457F4E718BE" );

            RockMigrationHelper.AddDefinedTypeAttribute(
                "B8A57DFE-827A-40C1-B8DE-F6EA0C50B864",
                "73B02051-0D38-4AD9-BF81-A2D477DE4F70",
                "Settings",
                "Settings",
                "Any settings that will be provided by this theme. These can be set on the block to customize the look and behavior of the theme.",
                3,
                string.Empty,
                "93B52ED4-DE25-436F-A7F3-E2422E394B4C" );

            RockMigrationHelper.AddAttributeQualifier(
                "93B52ED4-DE25-436F-A7F3-E2422E394B4C",
                "valueprompt",
                "Default Value",
                "9622A8DF-FC32-4FBC-B3BB-D5A36726DF50" );

            RockMigrationHelper.AddDefinedTypeAttribute(
                "B8A57DFE-827A-40C1-B8DE-F6EA0C50B864",
                "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5",
                "Help Content",
                "HelpContent",
                "HTML content to be displayed with this theme that describes the features and how to use it.",
                4,
                string.Empty,
                "C00E01E9-9111-4A33-A82D-FC5AEA68A696" );

            RockMigrationHelper.AddAttributeQualifier(
                "C00E01E9-9111-4A33-A82D-FC5AEA68A696",
                "editorMode",
                "2",
                "41F08A81-C8D7-45C7-B2F6-220C3DD6415C" );

            // Add the "Toast" Defined Value.
            RockMigrationHelper.AddDefinedValue(
                "B8A57DFE-827A-40C1-B8DE-F6EA0C50B864",
                "Toast",
                "The Toast theme displays the item at the top of the area for a short period of time and then removes it. If additional items are displayed before previous ones are removed then they are inserted at the top.",
                "1D830402-3378-43C0-A8A7-088DBD3CE57B",
                true );

            RockMigrationHelper.AddDefinedValueAttributeValue(
                "1D830402-3378-43C0-A8A7-088DBD3CE57B",
                "46A1C62A-109B-45BC-9E64-F817EB2E2183",
                @"<div class=""visualizer-container"">
</div>" );

            RockMigrationHelper.AddDefinedValueAttributeValue(
                "1D830402-3378-43C0-A8A7-088DBD3CE57B",
                "A0A3BDEE-89AF-462E-807D-8157CAD8D3A0",
                @".visualizer-container {
    display: flex;
    flex-direction: column;
    min-height: 400px;
    position: relative;
    background-color: black;
{% if Settings.fullscreen == ""true"" %}
    position: fixed;
    left: 0px;
    top: 0px;
    right: 0px;
    bottom: 0px;
{% endif %}
}

.visualizer-container > canvas {
    position: absolute;
    left: 0px;
    top: 0px;
    right: 0px;
    bottom: 0px;
}

.visualizer-container > .realtime-visualizer-item {
    height: 0px;
}

/* IN transition initial states. */
.visualizer-container > .realtime-visualizer-item.left-in {
    transform: translateX(calc(var(--slideAmount) * -1));
}

.visualizer-container > .realtime-visualizer-item.top-in {
    transform: translateY(calc(var(--slideAmount) * -1));
}

.visualizer-container > .realtime-visualizer-item.right-in {
    transform: translateX(var(--slideAmount));
}

.visualizer-container > .realtime-visualizer-item.bottom-in {
    transform: translateY(var(--slideAmount));
}

.visualizer-container > .realtime-visualizer-item.fade-in {
    opacity: 0;
}

/* IN transition final states. */
.visualizer-container > .realtime-visualizer-item.left-in.in,
.visualizer-container > .realtime-visualizer-item.top-in.in,
.visualizer-container > .realtime-visualizer-item.right-in.in,
.visualizer-container > .realtime-visualizer-item.bottom-in.in {
    transform: initial;
}

.visualizer-container > .realtime-visualizer-item.fade-in.in {
    opacity: 1;
}

/* OUT transition final states. */
.visualizer-container > .realtime-visualizer-item.left-out.out {
    transform: translateX(calc(var(--slideAmount) * -1));
}

.visualizer-container > .realtime-visualizer-item.top-out.out {
    transform: translateY(calc(var(--slideAmount) * -1));
}

.visualizer-container > .realtime-visualizer-item.right-out.out {
    transform: translateX(var(--slideAmount));
}

.visualizer-container > .realtime-visualizer-item.bottom-out.out {
    transform: translateY(var(--slideAmount));
}

.visualizer-container > .realtime-visualizer-item.fade-out.out {
    opacity: 0;
    overflow-y: initial;
}

/* Transition Timings. */
.visualizer-container > .realtime-visualizer-item.in {
    transition: height var(--animationDuration) ease-out, transform var(--animationDuration) ease-out, opacity var(--animationDuration) ease-out;
}

.visualizer-container > .realtime-visualizer-item.out {
    transition: opacity var(--animationDuration) ease-in, transform var(--animationDuration) ease-in, height var(--animationDuration) ease-in;
}
" );

            RockMigrationHelper.AddDefinedValueAttributeValue(
                "1D830402-3378-43C0-A8A7-088DBD3CE57B",
                "6F875F31-F43B-4C33-9183-33FD78EB21F7",
                @"let helper = undefined;

// Called one time to initialize everything before any
// calls to showItem() are made.
async function setup(container, settings) {
    const itemContainer = container.getElementsByClassName(""visualizer-container"")[0];
    const fingerprint = crypto.randomUUID ? crypto.randomUUID() : Math.random().toString();

    const Helper = (await import(`/Scripts/Rock/UI/realtimevisualizer/common.js?v=${fingerprint}`)).Helper;
    helper = new Helper(itemContainer);
}

// Shows a single visual item from the RealTime system.
async function showItem(content, container, settings) {
    if (!content) {
        return;
    }
    
    const itemContainer = container.getElementsByClassName(""visualizer-container"")[0];
    
    // Configure the item and it's content.
    const item = document.createElement(""div"");
    item.classList.add(""realtime-visualizer-item"");
    item.innerHTML = content;
    itemContainer.prepend(item);

    // Configure all the animation classes.
    if (settings.fade === ""true"") {
        item.classList.add(""fade-in"", ""fade-out"");
    }

    if (settings.slideInDirection) {
        item.classList.add(`${settings.slideInDirection}-in`);
    }

    if (settings.slideOutDirection) {
        item.classList.add(`${settings.slideOutDirection}-out`);
    }

    // Show the item.
    helper.setItemHeight(item);
    item.classList.add(""in"");

    // Start up all the extras.
    if (settings.playAudio) {
        helper.playAudio(item, settings.defautlAudioUrl);
    }
    
    if (settings.confetti) {
        helper.showConfetti();
    }
    
    if (settings.fireworks) {
        helper.startFireworks();
    }
    
    // Wait until this item should be removed and then start
    // the removal process.
    setTimeout(() => {
        item.classList.add(""out"");
        item.style.height = ""0px"";

        item.addEventListener(""transitionend"", () => {
            if (item.parentElement) {
                item.remove();

                if (settings.fireworks) {
                    helper.stopFireworks();
                }
            }
        });
    }, parseInt(settings.duration) || 5000);
}
" );

            RockMigrationHelper.AddDefinedValueAttributeValue(
                "1D830402-3378-43C0-A8A7-088DBD3CE57B",
                "93B52ED4-DE25-436F-A7F3-E2422E394B4C",
                @"fullscreen^true|slideAmount^15px|animationDuration^0.5s|duration^5000|fireworks^false|confetti^false|playAudio^false|defaultAudioUrl^|fade^true|slideInDirection^|slideOutDirection^" );

            RockMigrationHelper.AddDefinedValueAttributeValue(
                "1D830402-3378-43C0-A8A7-088DBD3CE57B",
                "C00E01E9-9111-4A33-A82D-FC5AEA68A696",
                @"<p>
    The Toast theme displays the item at the top of the area for a short period of time
    and then removes it. If additional items are displayed before previous ones are
    removed then they are inserted at the top.
</p>

<div><strong>Presentation Settings</strong></div>

<p>
    There are a few properties that define how an item is presented that can be
    set to customize how this theme looks and behaves.
</p>

<ul>
    <li><strong>animationDuration:</strong> The number of seconds the item will spend transitioning onto or off of the screen. (Default: <strong>0.5s</strong>)</li>
    <li><strong>duration:</strong> The number of milliseconds the item will stay on screen, this does not include the animationDuration. (Default: <strong>5000</strong>)</li>
    <li><strong>fade:</strong> Determines if the item should fade in and out. Valid values are ""true"" and ""false"". (Default: <strong>true</strong>)</li>
    <li><strong>fullscreen:</strong> Determines if the theme should render itself full-screen. You can turn this off to use CSS to customize what part of the screen to fill. (Default: <strong>true</strong>)</li>
    <li><strong>slideAmount:</strong> The number of pixels to slide the item when slideInDirection or slideOutDirection are specified. (Default: <strong>15px</strong>)</li>
    <li><strong>slideInDirection:</strong> If set to a value this will determine what direction the item will slide in from. Valid values are ""left"", ""top"", ""right"" and ""bottom"". (No default)</li>
    <li><strong>slideOutDirection: </strong>If set to a value this will determine what direction the item will slide out towards. Valid values are ""left"", ""top"", ""right"" and ""bottom"". (No default)</li>
</ul>

<div><strong>Advanced Settings</strong></div>

<p>
    There are some other settings you can use to customize the behavior of the theme.
</p>

<ul>
    <li><strong>confetti:</strong> If turned on, a burst of confetti will appear from both sides of the screen when an item is displayed. Valid values are ""true"" and ""false"". (Default: <strong>false</strong>)</li>
    <li><strong>fireworks:</strong> If turned on, fireworks will be displayed during the entire duration that any item is displayed. Valid values are ""true"" and ""false"". (Default: <strong>false</strong>)</li>
    <li><strong>defaultAudioUrl:</strong> When playAudio is enabled, this provides the default audio file to use if none is specified in the item template. (No default)</li>
    <li><strong>playAudio:</strong> If turned on, an audio file will be played when an item appears on screen. If the item includes a ""data-audio-url"" attribute then it will be used as the URL of the audio file to play. Otherwise any value in defaultAudioUrl will be used. (Default: <strong>false</strong>)</li>
</ul>
" );

            // Add the "Swap" Defined Value.
            RockMigrationHelper.AddDefinedValue(
                "B8A57DFE-827A-40C1-B8DE-F6EA0C50B864",
                "Swap",
                "Swaps the content out whenever new content arrives.",
                "6262166B-B25A-458C-B531-2FD4768059AD",
                true );

            RockMigrationHelper.AddDefinedValueAttributeValue(
                "6262166B-B25A-458C-B531-2FD4768059AD",
                "46A1C62A-109B-45BC-9E64-F817EB2E2183",
                @"<div class=""visualizer-container"">
</div>" );

            RockMigrationHelper.AddDefinedValueAttributeValue(
                "6262166B-B25A-458C-B531-2FD4768059AD",
                "A0A3BDEE-89AF-462E-807D-8157CAD8D3A0",
                @".visualizer-container {
    display: flex;
    flex-direction: column;
    min-height: 400px;
    position: relative;
    background-color: black;
{% if Settings.fullscreen == ""true"" %}
    position: fixed;
    left: 0px;
    top: 0px;
    right: 0px;
    bottom: 0px;
{% endif %}
}

.visualizer-container > canvas {
    position: absolute;
    left: 0px;
    top: 0px;
    right: 0px;
    bottom: 0px;
}

.visualizer-item.in {
  animation: 1.5s incoming both;
}
::view-transition-old(outgoing) {
  animation: 1.5s outgoing both;
}

@keyframes outgoing {
  0% {
    opacity: 1;
  }
  100% {
    opacity: 0;
  }
}

@keyframes incoming {
  0% {
    opacity: 0;
  }
  100% {
    opacity: 1;
  }
}
" );

            RockMigrationHelper.AddDefinedValueAttributeValue(
                "6262166B-B25A-458C-B531-2FD4768059AD",
                "6F875F31-F43B-4C33-9183-33FD78EB21F7",
                @"let helper = undefined;
let itemCount = 0;

// Called one time to initialize everything before any
// calls to showItem() are made.
async function setup(container, settings) {
    const itemContainer = container.getElementsByClassName(""visualizer-container"")[0];
    const fingerprint = crypto.randomUUID ? crypto.randomUUID() : Math.random().toString();

    const Helper = (await import(`/Scripts/Rock/UI/realtimevisualizer/common.js?v=${fingerprint}`)).Helper;
    helper = new Helper(itemContainer);
}

// Shows a single visual item from the RealTime system.
async function showItem(content, container, settings) {
    if (!content) {
        return;
    }
    
    const itemContainer = container.getElementsByClassName(""visualizer-container"")[0];
    
    // Configure the item and it's content.
    const item = document.createElement(""div"");
    item.classList.add(""visualizer-item"", ""in"");
    item.style.viewTransitionName = `visualizer-item-${itemCount++}`;
    item.innerHTML = content;

    // Prepare old items for removal.
    const oldItems = itemContainer.querySelectorAll("".visualizer-item"");
    for (let i = 0; i < oldItems.length; i++) {
        oldItems[i].classList.remove(""in"");
        oldItems[i].style.viewTransitionName = ""outgoing"";
    }

    if (document.startViewTransition) {
        document.startViewTransition(() => {
            itemContainer.prepend(item);
            for (let i = 0; i < oldItems.length; i++) {
                oldItems[i].remove();
            }
        });
    }
    else {
        itemContainer.prepend(item);
        for (let i = 0; i < oldItems.length; i++) {
            oldItems[i].remove();
        }
    }

    // Start up all the extras.
    if (settings.playAudio) {
        helper.playAudio(item, settings.defautlAudioUrl);
    }
    
    if (settings.confetti) {
        helper.showConfetti();
    }
}
" );

            RockMigrationHelper.AddDefinedValueAttributeValue(
                "6262166B-B25A-458C-B531-2FD4768059AD",
                "93B52ED4-DE25-436F-A7F3-E2422E394B4C",
                @"fullscreen^false|confetti^false|playAudio^false|defaultAudioUrl^" );

            RockMigrationHelper.AddDefinedValueAttributeValue(
                "6262166B-B25A-458C-B531-2FD4768059AD",
                "C00E01E9-9111-4A33-A82D-FC5AEA68A696",
                @"<p>
    The Swap theme displays a single item at a time. When a new item needs to be
    displayed it will replace the previous item.
</p>

<div><strong>Presentation Settings</strong></div>

<p>
    There are a few properties that define how an item is presented that can be
    set to customize how this theme looks and behaves.
</p>

<ul>
    <li><strong>fullscreen:</strong> Determines if the theme should render itself full-screen. You can turn this off to use CSS to customize what part of the screen to fill. (Default: <strong>true</strong>)</li>
</ul>

<div><strong>Advanced Settings</strong></div>

<p>
    There are some other settings you can use to customize the behavior of the theme.
</p>

<ul>
    <li><strong>confetti:</strong> If turned on, a burst of confetti will appear from both sides of the screen when an item is displayed. Valid values are ""true"" and ""false"". (Default: <strong>false</strong>)</li>
    <li><strong>defaultAudioUrl:</strong> When playAudio is enabled, this provides the default audio file to use if none is specified in the item template. (No default)</li>
    <li><strong>playAudio:</strong> If turned on, an audio file will be played when an item appears on screen. If the item includes a ""data-audio-url"" attribute then it will be used as the URL of the audio file to play. Otherwise any value in defaultAudioUrl will be used. (Default: <strong>false</strong>)</li>
</ul>
" );

            // Add Utility > RealTime Visualizer Defined Value for Block Template.
            RockMigrationHelper.AddOrUpdateTemplateBlock(
                "74ae7a3d-7335-439c-8c06-ae30b033a82b",
                "Utility > RealTime Visualizer",
                string.Empty );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "b23a64aa-75f1-4213-8ce9-46b6839d8702",
                "74ae7a3d-7335-439c-8c06-ae30b033a82b",
                "Attendance Banner",
                @"{% if Topic == ""Rock.RealTime.Topics.EntityUpdatedTopic"" and Message == ""attendanceUpdated"" and Args[0].Status == 1 %}
<div class=""alert alert-info"">
    <div class=""d-flex"">
        <img src=""{{ Args[0].PersonPhotoUrl }}"" class=""mr-4"" style=""width: 64px; height: 64px; border-radius: 8px;"">
        <span style=""font-size: 2em; align-self: center;"">Welcome {{ Args[0].PersonFullName }}!</span>
    </div>
</div>
{% endif %}",
                STANDARD_ICON_SVG,
                "standard-template.svg",
                "image/svg+xml" );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "c7d2ebb5-d35e-489d-bd70-e784fcdb2787",
                "74ae7a3d-7335-439c-8c06-ae30b033a82b",
                "Achievement Banner",
                @"{% if Topic == ""Rock.RealTime.Topics.EntityUpdatedTopic"" and Message == ""achievementCompleted"" %}
<div class=""alert alert-info"">
    <div class=""d-flex"">
        <img src=""{{ Args[0].EntityPhotoUrl }}"" class=""mr-4"" style=""width: 64px; height: 64px; border-radius: 8px;"">
        <span style=""font-size: 2em; align-self: center;"">{{ Args[0].EntityName }} has completed the {{ Args[0].AchievementTypeName }} achievement!</span>
    </div>
</div>
{% endif %}",
                STANDARD_ICON_SVG,
                "standard-template.svg",
                "image/svg+xml" );
        }

        /// <summary>
        /// DH: RealTime Visualizer Data Down
        /// </summary>
        private void RealTimeVisualizerDataDown()
        {
            RockMigrationHelper.DeleteTemplateBlockTemplate( "c7d2ebb5-d35e-489d-bd70-e784fcdb2787" );
            RockMigrationHelper.DeleteTemplateBlockTemplate( "b23a64aa-75f1-4213-8ce9-46b6839d8702" );
            RockMigrationHelper.DeleteTemplateBlock( "74ae7a3d-7335-439c-8c06-ae30b033a82b" );

            RockMigrationHelper.DeleteDefinedValue( "6262166B-B25A-458C-B531-2FD4768059AD" );
            RockMigrationHelper.DeleteDefinedValue( "1D830402-3378-43C0-A8A7-088DBD3CE57B" );
            RockMigrationHelper.DeleteAttribute( "C00E01E9-9111-4A33-A82D-FC5AEA68A696" );
            RockMigrationHelper.DeleteAttribute( "93B52ED4-DE25-436F-A7F3-E2422E394B4C" );
            RockMigrationHelper.DeleteAttribute( "6F875F31-F43B-4C33-9183-33FD78EB21F7" );
            RockMigrationHelper.DeleteAttribute( "A0A3BDEE-89AF-462E-807D-8157CAD8D3A0" );
            RockMigrationHelper.DeleteAttribute( "46A1C62A-109B-45BC-9E64-F817EB2E2183" );
            RockMigrationHelper.DeleteDefinedType( "B8A57DFE-827A-40C1-B8DE-F6EA0C50B864" );
        }

        /// <summary>
        /// Creates the Service Job responsible for creating a custom security group 
        /// for mobile application users.
        /// </summary>
        private void MobileApplicationDuplicateInteractionJobCleanupUp()
        {
            // Create the Mobile Application Users Rest group service job.
            Sql( $@"IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.PostV151DataMigrationsDuplicateMobileInteractionsCleanup' AND [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_151_DUPLICATE_MOBILE_INTERACTIONS_CLEANUP}' )
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
                  0
                  ,1
                  ,'Rock Update Helper v15.1 - Mobile Duplicate Interaction Cleanup'
                  ,'This job will clean up duplicate interactions generated by the mobile shell..'
                  ,'Rock.Jobs.PostV151DataMigrationsDuplicateMobileInteractionsCleanup'
                  ,'0 0 21 1/1 * ? *'
                  ,1
                  ,'{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_151_DUPLICATE_MOBILE_INTERACTIONS_CLEANUP}'
                  );
            END" );

        }

        /// <summary>
        /// Removes the service job responsible for creating a custom security group
        /// for mobile application users.
        /// </summary>
        private void MobileApplicationDuplicateInteractionJobCleanupDown()
        {
            // Delete the Mobile Application Duplicate Interactions Cleanup service job.
            Sql( $"DELETE FROM [ServiceJob] WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_151_DUPLICATE_MOBILE_INTERACTIONS_CLEANUP}'" );
        }
    }
}
