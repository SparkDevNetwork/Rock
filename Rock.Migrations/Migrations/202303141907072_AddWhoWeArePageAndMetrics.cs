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
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Text;

    /// <summary>
    ///
    /// </summary>
    public partial class AddWhoWeArePageAndMetrics : Rock.Migrations.RockMigration
    {
        const string GENDER_CATEGORY_GUID = "5FAD0855-924D-4837-9E95-097565E87F48";
        const string AGE_RANGE_CATEGORY_GUID = "10716347-44BA-44F1-8E82-21C23445EB76";
        const string INFORMATION_COMPLETENESS_CATEGORY_GUID = "60702C6D-9C65-4FF1-B0F9-66CC7A88DCEE";
        const string WHO_WE_ARE_SCHEDULE_GUID = "20AFE3A5-86DC-4232-9FAE-9271D7A251FB";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddPage( Rock.SystemGuid.Page.REPORTING, Rock.SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Insights", "Shows high-level statistics of the Rock database", "721C8E32-CAAD-4670-AD1B-04FC42A26BB2" );
            RockMigrationHelper.AddBlockType( "Insights", "Shows high-level statistics of the Rock database.", "~/Blocks/Reporting/Insights.ascx", "Reporting", "B215F5FA-410C-4674-8C47-43DC40AF9F67" );
            RockMigrationHelper.AddBlock( "721C8E32-CAAD-4670-AD1B-04FC42A26BB2", null, "B215F5FA-410C-4674-8C47-43DC40AF9F67", "Insights", "Main", "", "", 0, "D916FCD5-F58C-4BCC-87CA-A78595A04734" );

            AddSchedule();
            AddMetrics();
        }

        private void AddSchedule()
        {
            Sql( $@"
DECLARE @ScheduleId int = ( 
    SELECT [Id] 
    FROM Schedule 
    WHERE Guid = '{WHO_WE_ARE_SCHEDULE_GUID}'),

@MetricsCategoryId int = ( 
    SELECT [Id] 
    FROM Category 
    WHERE Guid = '5A794741-5444-43F0-90D7-48E47276D426'
)
IF @ScheduleId IS NULL
BEGIN
	INSERT [dbo].[Schedule] ([Name], [Description], [iCalendarContent], [CategoryId], [Guid], [IsActive])
	VALUES (N'Insights Metrics Schedule', NULL, N'BEGIN:VCALENDAR
PRODID:-//github.com/rianjs/ical.net//NONSGML ical.net 4.0//EN
VERSION:2.0
BEGIN:VEVENT
DTEND:20230301T050001
DTSTAMP:20230301T102849
DTSTART:20230301T050000
RRULE:FREQ=MONTHLY;BYMONTHDAY=28
SEQUENCE:0
UID:3736edbb-2e51-4114-bd4f-3ed62b98ed12
END:VEVENT
END:VCALENDAR',
@MetricsCategoryId, N'{WHO_WE_ARE_SCHEDULE_GUID}', 1)
END" );
        }

        private void AddMetrics()
        {
            RockMigrationHelper.UpdateCategory( SystemGuid.EntityType.METRICCATEGORY, "Insights Metrics", "fa fa-chart-pie", "A few metrics to show high-level statistics of the Rock database.", SystemGuid.Category.INSIGHTS );

            AddGenderMetrics();
            AddConnectionStatusMetric();
            AddMaritalStatusMetric();
            AddRaceMetric();
            AddEthnicityMetric();
            AddPercentOfActiveRecordsMetric();
            AddPercentCompletenessMetrics();
            AddAgeMetrics();
        }

        private void AddAgeMetrics()
        {
            const string ageRangeSqlFormat = @"DECLARE @ActiveRecordStatusValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E'
    )
    ,@PersonRecordTypeValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = '36CF10D6-C695-413D-8E7C-4546EFEF385E'
    )     
    ,@Today Date = GetDate()

-- People, who are Active, who are not deceased and...
SELECT COUNT(1), P.[PrimaryCampusId]
FROM [Person] P
WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
    AND P.[IsDeceased] = 0
    AND {0}
GROUP BY P.[PrimaryCampusId] ORDER BY P.[PrimaryCampusId]";

            const string getAgeSqlFormat = @"DATEDIFF(YEAR, P.[BirthDate], @Today) - 
	CASE 
		WHEN DATEADD(YY, DATEDIFF(yy, P.[BirthDate], @Today), P.[BirthDate]) > @Today THEN 1
		ELSE 0
	END {0}
";

            // Add Age Range Sub-Category
            RockMigrationHelper.UpdateCategory( SystemGuid.EntityType.METRICCATEGORY, "Age Range", "fa fa-birthday-cake", "Metrics to show statistics of the age distribution of people in the database.", AGE_RANGE_CATEGORY_GUID, parentCategoryGuid: SystemGuid.Category.INSIGHTS );

            var zeroTo12RangeSql = string.Format( ageRangeSqlFormat, string.Format( getAgeSqlFormat, "BETWEEN 0 AND 12" ) );
            AddSqlSourcedMetric( "EEDEE264-F49D-46B9-815D-C5DBB5DCC9CE", "0-12", AGE_RANGE_CATEGORY_GUID, zeroTo12RangeSql, new List<PartitionDetails> { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) }, "Active people between the ages of 0 and 12" );

            var thirteenToSeventeenRangeSql = string.Format( ageRangeSqlFormat, string.Format( getAgeSqlFormat, "BETWEEN 13 AND 17" ) );
            AddSqlSourcedMetric( "5FFD8F4C-5199-41D7-BF33-14497DFEDD3F", "13-17", AGE_RANGE_CATEGORY_GUID, thirteenToSeventeenRangeSql, new List<PartitionDetails> { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) }, "Active people between the ages of 13 and 17" );

            var eighteenAndTwentyFour = string.Format( ageRangeSqlFormat, string.Format( getAgeSqlFormat, "BETWEEN 18 AND 24" ) );
            AddSqlSourcedMetric( "12B581AD-EFE3-4A77-B2E7-80A385EFDDEE", "18-24", AGE_RANGE_CATEGORY_GUID, eighteenAndTwentyFour, new List<PartitionDetails> { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) }, "Active people between the ages of 18 and 24" );

            var twentyFiveAndThirtyFour = string.Format( ageRangeSqlFormat, string.Format( getAgeSqlFormat, "BETWEEN 25 AND 34" ) );
            AddSqlSourcedMetric( "95141ECF-F29F-4AEB-A966-B63AE21B9520", "25-34", AGE_RANGE_CATEGORY_GUID, twentyFiveAndThirtyFour, new List<PartitionDetails> { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) }, "Active people between the ages of 25 and 34" );

            var thirtyFiveAndFourtyFour = string.Format( ageRangeSqlFormat, string.Format( getAgeSqlFormat, "BETWEEN 35 AND 44" ) );
            AddSqlSourcedMetric( "6EEB5736-E17A-44E2-AC27-0CF933E4EC37", "35-44", AGE_RANGE_CATEGORY_GUID, thirtyFiveAndFourtyFour, new List<PartitionDetails> { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) }, "Active people between the ages of 35 and 44" );

            var fourtyFiveAndFiftyFour = string.Format( ageRangeSqlFormat, string.Format( getAgeSqlFormat, "BETWEEN 45 AND 54" ) );
            AddSqlSourcedMetric( "411A8AF5-FE70-43E1-8BCE-C1FA52051663", "45-54", AGE_RANGE_CATEGORY_GUID, fourtyFiveAndFiftyFour, new List<PartitionDetails> { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) }, "Active people between the ages of 45 and 54" );

            var fiftyFiveAndSixtyFour = string.Format( ageRangeSqlFormat, string.Format( getAgeSqlFormat, "BETWEEN 55 AND 64" ) );
            AddSqlSourcedMetric( "93D36C22-6D88-4A2D-BCE2-0CF7CF1FC8F0", "55-64", AGE_RANGE_CATEGORY_GUID, fiftyFiveAndSixtyFour, new List<PartitionDetails> { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) }, "Active people between the ages of 55 and 64" );

            var overSixtyFive = string.Format( ageRangeSqlFormat, string.Format( getAgeSqlFormat, ">= 65" ) );
            AddSqlSourcedMetric( "9BF88708-D258-49DA-928E-CF8D894EF21B", "65+", AGE_RANGE_CATEGORY_GUID, overSixtyFive, new List<PartitionDetails> { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) }, "Active people of age 65 or older than 65" );

            var unknown = string.Format( ageRangeSqlFormat, "P.[BirthDate] IS NULL" );
            AddSqlSourcedMetric( "54EEDC65-4D5F-4E28-8709-7F282FA9412A", "Unknown", AGE_RANGE_CATEGORY_GUID, unknown, new List<PartitionDetails> { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) }, "Active people with unknown ages" );
        }

        private void AddPercentCompletenessMetrics()
        {
            const string informationCompletenessFormat = @"DECLARE
    @ActiveRecordStatusValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E'
    )
    ,@PersonRecordTypeValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = '36CF10D6-C695-413D-8E7C-4546EFEF385E'
    )        

-- People, who are Active, who are not deceased and...
SELECT count(*) * 100 / (SELECT count(*) from Person), P.[PrimaryCampusId] 
FROM [Person] P
WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
    AND P.[IsDeceased] = 0
    AND {0}
GROUP BY P.PrimaryCampusId";

            const string phoneNumberCompletenessSql = @"DECLARE
    @ActiveRecordStatusValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E'
    )
    ,@PersonRecordTypeValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = '36CF10D6-C695-413D-8E7C-4546EFEF385E'
    )        

-- People, who are Active, who are not deceased and...
SELECT count(*) * 100 / (SELECT count(*) from Person) AS PercentWithPhone, P.[PrimaryCampusId]
FROM [Person] P
JOIN [PhoneNumber] PH
ON P.[Id] = PH.[PersonId]
WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
    AND P.[IsDeceased] = 0
    AND PH.[NumberTypeValueId] = (SELECT Id FROM DefinedValue WHERE [Guid] = '407E7E45-7B2E-4FCD-9605-ECB1339F2453')
GROUP BY P.PrimaryCampusId";

            const string homeAddressCompletenessSql = @"
DECLARE
    @ActiveRecordStatusValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E'
    )
    ,@PersonRecordTypeValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = '36CF10D6-C695-413D-8E7C-4546EFEF385E'
    )  
SELECT count(*) AS PercentWithHomeAddress, P.[PrimaryCampusId]
FROM Person P
JOIN GroupLocation G
ON G.[GroupId] = P.[PrimaryFamilyId]
WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
    AND P.[IsDeceased] = 0
	AND G.[GroupLocationTypeValueId] = (SELECT Id FROM DefinedValue WHERE [Guid] = '8C52E53C-2A66-435A-AE6E-5EE307D9A0DC')
GROUP BY P.PrimaryCampusId
";

            // Add Information Completeness Sub-Category
            RockMigrationHelper.UpdateCategory( SystemGuid.EntityType.METRICCATEGORY, "Information Completeness", "fa fa-info-circle", "Metrics to show statistics of data completeness for people in the database.", INFORMATION_COMPLETENESS_CATEGORY_GUID, parentCategoryGuid: SystemGuid.Category.INSIGHTS );

            // Add Age Metric
            var ageSql = string.Format( informationCompletenessFormat, "P.[BirthDate] IS NOT NULL" );
            AddSqlSourcedMetric( "8046A160-941F-4CCD-9EB6-5BD7601DD536", "Age", INFORMATION_COMPLETENESS_CATEGORY_GUID, ageSql, new List<PartitionDetails> { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) }, "Percent of active people with birthdates" );

            // Add Gender Metric
            var genderCompletenessSql = string.Format( informationCompletenessFormat, "P.[GENDER] != 0" );
            AddSqlSourcedMetric( "C4F9A612-D487-4CE0-9D9B-691DC733857D", "Gender", INFORMATION_COMPLETENESS_CATEGORY_GUID, genderCompletenessSql, new List<PartitionDetails> { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) }, "Percent of active people with a known gender" );

            // Add Active Email Metric
            var activeEmailCompletenessSql = string.Format( informationCompletenessFormat, "P.[Email] IS NOT NULL AND P.[IsEmailActive] = 1" );
            AddSqlSourcedMetric( "0C1C1231-DB5D-4B44-9172-F39B56786960", "Active Email", INFORMATION_COMPLETENESS_CATEGORY_GUID, activeEmailCompletenessSql, new List<PartitionDetails> { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) }, "Percent of active people with an active email" );

            // Add MobilePhone Metric
            AddSqlSourcedMetric( "75A8E234-AEC3-4C75-B902-C3F954616BBC", "Mobile Phone", INFORMATION_COMPLETENESS_CATEGORY_GUID, phoneNumberCompletenessSql, new List<PartitionDetails> { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) }, "Percent of active people with a mobile phone number" );

            // Add MaritalStatus Metric
            var maritalStatusCompletenessSql = string.Format( informationCompletenessFormat, "P.[MaritalStatusValueId] IS NOT NULL" );
            AddSqlSourcedMetric( "17AC2A8A-B130-4900-B2BD-203D0F8FF971", "Marital Status", INFORMATION_COMPLETENESS_CATEGORY_GUID, maritalStatusCompletenessSql, new List<PartitionDetails> { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) }, "Percent of active people with a known marital status" );

            // Add Photo Metric
            var photoCompletenessSql = string.Format( informationCompletenessFormat, "P.[PhotoId] IS NOT NULL" );
            AddSqlSourcedMetric( "4DACA1E0-E768-417C-BB5B-DAB5DC0BDA79", "Photo", INFORMATION_COMPLETENESS_CATEGORY_GUID, photoCompletenessSql, new List<PartitionDetails> { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) }, "Active people whose gender is unknown" );

            // Add Date Of Birth Metric
            var dateOfBirthCompletenessSql = string.Format( informationCompletenessFormat, "P.[BirthDate] IS NOT NULL" );
            AddSqlSourcedMetric( "D79DECDD-BA7B-4E4B-81F1-5B6392FD7BD8", "Date of Birth", INFORMATION_COMPLETENESS_CATEGORY_GUID, dateOfBirthCompletenessSql, new List<PartitionDetails> { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) }, "Percent of active people with birthdates" );

            // Add Home Address Metric
            AddSqlSourcedMetric( "7964D01D-41B7-469F-8CE7-0C4A84968E62", "Home Address", INFORMATION_COMPLETENESS_CATEGORY_GUID, homeAddressCompletenessSql, new List<PartitionDetails> { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) }, "Percent of active people with a home address" );
        }

        private void AddPercentOfActiveRecordsMetric()
        {
            const string sql = @"DECLARE
    @ActiveRecordStatusValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E'
    )
    ,@PersonRecordTypeValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = '36CF10D6-C695-413D-8E7C-4546EFEF385E'
    )        

-- People, who are Active, who are not deceased and...
SELECT count(*) * 100 / (SELECT count(*) from Person), P.[PrimaryCampusId] 
FROM [Person] P
WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
    AND P.[IsDeceased] = 0
GROUP BY P.PrimaryCampusId";

            var partitions = new List<PartitionDetails>
            {
                new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ),
            };

            AddSqlSourcedMetric( "7AE9475F-389E-496F-8DF0-508B66ADA6A0", "Active Records", SystemGuid.Category.INSIGHTS, sql, partitions, "Percentage of people with an active Record status" );
        }

        private void AddEthnicityMetric()
        {
            const string sql = @"DECLARE
    @ActiveRecordStatusValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E'
    )
    ,@PersonRecordTypeValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = '36CF10D6-C695-413D-8E7C-4546EFEF385E'
    )        

-- People, who are Active, who are not deceased and...
SELECT COUNT(1), P.[PrimaryCampusId], P.[EthnicityValueId]
FROM [Person] P
WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
    AND P.[IsDeceased] = 0
GROUP BY  P.[PrimaryCampusId], P.[EthnicityValueId] ORDER BY P.[PrimaryCampusId]";

            var partitions = new List<PartitionDetails>
            {
                new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ),
                new PartitionDetails( "Ethnicity", Rock.SystemGuid.EntityType.DEFINED_VALUE, "DefinedTypeId", Rock.SystemGuid.DefinedType.PERSON_ETHNICITY ),
            };

            AddSqlSourcedMetric( "B0420908-6AED-487C-BCA4-9B63EA4F87F5", "Ethnicity", SystemGuid.Category.INSIGHTS, sql, partitions, "Active people broken down by Ethnicity" );
        }

        private void AddRaceMetric()
        {
            const string sql = @"DECLARE
    @ActiveRecordStatusValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E'
    )
    ,@PersonRecordTypeValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = '36CF10D6-C695-413D-8E7C-4546EFEF385E'
    )        

-- People, who are Active, who are not deceased and...
SELECT COUNT(1), P.[PrimaryCampusId], P.[RaceValueId]
FROM [Person] P
WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
    AND P.[IsDeceased] = 0
GROUP BY P.[PrimaryCampusId], P.[RaceValueId] ORDER BY P.[PrimaryCampusId]";

            var partitions = new List<PartitionDetails>
            {
                new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ),
                new PartitionDetails( "Race", Rock.SystemGuid.EntityType.DEFINED_VALUE, "DefinedTypeId", Rock.SystemGuid.DefinedType.PERSON_RACE ),
            };

            AddSqlSourcedMetric( "3EB53204-CED6-4ACF-8045-288BD2EA8E82", "Race", SystemGuid.Category.INSIGHTS, sql, partitions, "Active people broken down by Race" );
        }

        private void AddMaritalStatusMetric()
        {
            const string sql = @"DECLARE
    @ActiveRecordStatusValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E'
    )
    ,@PersonRecordTypeValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = '36CF10D6-C695-413D-8E7C-4546EFEF385E'
    )        

-- People, who are Active, who are not deceased and...
SELECT COUNT(1), P.[PrimaryCampusId], P.[MaritalStatusValueId]
FROM [Person] P
WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
    AND P.[IsDeceased] = 0
GROUP BY P.[MaritalStatusValueId], P.[PrimaryCampusId] ORDER BY P.[PrimaryCampusId]";

            var partitions = new List<PartitionDetails>
            {
                new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ),
                new PartitionDetails( "Marital Status", Rock.SystemGuid.EntityType.DEFINED_VALUE, "DefinedTypeId", Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS ),
            };

            AddSqlSourcedMetric( "D9B85AD9-2573-4EAE-8DCF-980FC13B81B5", "Marital Status", SystemGuid.Category.INSIGHTS, sql, partitions, "Active people broken down by Marital Status" );
        }

        private void AddConnectionStatusMetric()
        {
            const string sql = @"DECLARE
    @ActiveRecordStatusValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E'
    )
    ,@PersonRecordTypeValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = '36CF10D6-C695-413D-8E7C-4546EFEF385E'
    )        

-- People, who are Active, who are not deceased and...
SELECT COUNT(1), P.[PrimaryCampusId], P.[ConnectionStatusValueId]
FROM [Person] P
WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
    AND P.[IsDeceased] = 0
GROUP BY  P.[PrimaryCampusId], P.[ConnectionStatusValueId] ORDER BY P.[PrimaryCampusId]";

            var partitions = new List<PartitionDetails>
            {
                new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ),
                new PartitionDetails( "Connection Status", Rock.SystemGuid.EntityType.DEFINED_VALUE, "DefinedTypeId", Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS ),
            };

            AddSqlSourcedMetric( "08A7360A-642E-4FA9-A5F8-288496D380EF", "Connection Status", SystemGuid.Category.INSIGHTS, sql, partitions, "Active people broken down by Connection Status" );
        }

        private void AddGenderMetrics()
        {
            const string genderSqlFormat = @"DECLARE @GenderValue INT = {0} --  0=Unknown, 1=Male, 2=Female,
    ,@ActiveRecordStatusValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E'
    )
    ,@PersonRecordTypeValueId INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = '36CF10D6-C695-413D-8E7C-4546EFEF385E'
    )        

-- People, who are Active, who are not deceased and...
SELECT COUNT(1), P.[PrimaryCampusId] 
FROM [Person] P
WHERE P.[RecordTypeValueId] = @PersonRecordTypeValueId
    AND P.[RecordStatusValueId] = @ActiveRecordStatusValueId
    AND P.[IsDeceased] = 0
    AND P.[Gender] = @GenderValue
GROUP BY P.[PrimaryCampusId] ORDER BY P.[PrimaryCampusId]";

            // Add Gender Sub-Category
            RockMigrationHelper.UpdateCategory( SystemGuid.EntityType.METRICCATEGORY, "Gender", "fa fa-venus-mars", "Metrics to show statistics of the gender distribution of people in the database.", GENDER_CATEGORY_GUID, parentCategoryGuid: SystemGuid.Category.INSIGHTS );

            var maleSql = string.Format( genderSqlFormat, "1" );
            AddSqlSourcedMetric( "44A00879-D836-4BA1-8CD1-B74EC2C53D5F", "Men", GENDER_CATEGORY_GUID, maleSql, new List<PartitionDetails> { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) }, "Active people whose gender is male" );

            var femaleSql = string.Format( genderSqlFormat, "2" );
            AddSqlSourcedMetric( "71CDC173-8B4E-4AFC-AD0E-925F69D299DA", "Women", GENDER_CATEGORY_GUID, femaleSql, new List<PartitionDetails> { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) }, "Active people whose gender is female" );

            var unknownSql = string.Format( genderSqlFormat, "0" );
            AddSqlSourcedMetric( "53883982-E730-4396-8B12-0D83304C5880", "Unknown", GENDER_CATEGORY_GUID, unknownSql, new List<PartitionDetails> { new PartitionDetails( "Campus", Rock.SystemGuid.EntityType.CAMPUS ) }, "Active people whose gender is unknown" );
        }

        private void AddSqlSourcedMetric( string guid, string title, string categoryGuid, string sourceSql, List<PartitionDetails> partitions, string description = null, string subtitle = null )
        {
            var formattedTitle = title?.Replace( "'", "''" ) ?? throw new ArgumentNullException( nameof( title ) );
            var createMetricAndMetricCategorySql = $@"DECLARE @MetricId [int] = (SELECT [Id] FROM [Metric] WHERE ([Guid] = '{guid}'))
    , @SourceValueTypeId [int] = (SELECT [Id] FROM [DefinedValue] WHERE ([Guid] = '{SystemGuid.DefinedValue.METRIC_SOURCE_VALUE_TYPE_SQL}'))
    , @MetricCategoryId [int] = (SELECT [Id] FROM [Category] WHERE ([Guid] = '{categoryGuid}'))
    , @Subtitle [varchar] (100) = {( string.IsNullOrWhiteSpace( subtitle ) ? "NULL" : $"'{subtitle.Replace( "'", "''" ).Substring( 0, subtitle.Length > 100 ? 100 : subtitle.Length )}'" )}
    , @Description [varchar] (max) = {( string.IsNullOrWhiteSpace( description ) ? "NULL" : $"'{description.Replace( "'", "''" )}'" )};

IF (@MetricId IS NULL AND @SourceValueTypeId IS NOT NULL AND @MetricCategoryId IS NOT NULL)
BEGIN
    DECLARE @Now [datetime] = GETDATE();
    INSERT INTO [Metric]
    (
        [IsSystem]
        , [Title]
        , [Subtitle]
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
        , '{formattedTitle}'
        , @Subtitle
        , @Description
        , 0
        , @SourceValueTypeId
        , '{sourceSql.Replace( "'", "''" )}'
        , (SELECT [Id] FROM Schedule WHERE Guid = '{WHO_WE_ARE_SCHEDULE_GUID}')
        , @Now
        , @Now
        , '{guid}'
        , 1
        , 0
    );
    SET @MetricId = SCOPE_IDENTITY();
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
    );";
            var sqlBuilder = new StringBuilder( createMetricAndMetricCategorySql );

            if ( partitions == null || partitions.Count == 0 )
            {
                sqlBuilder.Append( $@"INSERT INTO [MetricPartition]
    (
        [MetricId]
        , [IsRequired]
        , [Order]
        , [Guid]
    )
    VALUES
    (
        @MetricId
        , 1
        , 0
        , NEWID()
    );" );
            }
            else
            {
                foreach ( var partitionDetail in partitions )
                {
                    var createMetricPartitionSql = $@"INSERT INTO [MetricPartition]
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
        @MetricId
        , '{partitionDetail.Label}'
        , (SELECT Id FROM [EntityType] WHERE GUID = '{partitionDetail.EntityTypeGuid}')
        , 1
        , {partitions.IndexOf( partitionDetail )}
        , {partitionDetail.GetEntityTypeQualifierColumnValue()}
        , {partitionDetail.GetEntityTypeQualifierValue()}
        , @Now
        , @Now
        , NEWID()
    );";
                    sqlBuilder.Append( createMetricPartitionSql );
                }
            }

            sqlBuilder.AppendLine( "END" );

            Sql( sqlBuilder.ToString() );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteBlock( "D916FCD5-F58C-4BCC-87CA-A78595A04734" );
            RockMigrationHelper.DeleteBlockType( "b215f5fa-410c-4674-8c47-43dc40af9f67" );
            RockMigrationHelper.DeletePage( "721C8E32-CAAD-4670-AD1B-04FC42A26BB2" );

            // Delete Gender Metrics
            DeleteMetric( "44A00879-D836-4BA1-8CD1-B74EC2C53D5F" );
            DeleteMetric( "71CDC173-8B4E-4AFC-AD0E-925F69D299DA" );
            DeleteMetric( "53883982-E730-4396-8B12-0D83304C5880" );

            // Delete Connection Status Metrics
            DeleteMetric( "08A7360A-642E-4FA9-A5F8-288496D380EF" );

            // Delete Marital Status Metrics
            DeleteMetric( "D9B85AD9-2573-4EAE-8DCF-980FC13B81B5" );

            // Delete Race Metrics
            DeleteMetric( "3EB53204-CED6-4ACF-8045-288BD2EA8E82" );

            // Delete Ethnicity Metrics
            DeleteMetric( "B0420908-6AED-487C-BCA4-9B63EA4F87F5" );

            // Delete Percent Completed Metrics
            DeleteMetric( "8046A160-941F-4CCD-9EB6-5BD7601DD536" );
            DeleteMetric( "C4F9A612-D487-4CE0-9D9B-691DC733857D" );
            DeleteMetric( "0C1C1231-DB5D-4B44-9172-F39B56786960" );
            DeleteMetric( "75A8E234-AEC3-4C75-B902-C3F954616BBC" );
            DeleteMetric( "17AC2A8A-B130-4900-B2BD-203D0F8FF971" );
            DeleteMetric( "4DACA1E0-E768-417C-BB5B-DAB5DC0BDA79" );
            DeleteMetric( "D79DECDD-BA7B-4E4B-81F1-5B6392FD7BD8" );
            DeleteMetric( "7964D01D-41B7-469F-8CE7-0C4A84968E62" );

            // Delete Percent Of Active Records Metric
            DeleteMetric( "7AE9475F-389E-496F-8DF0-508B66ADA6A0" );

            // Delete Age Range Metrics
            DeleteMetric( "EEDEE264-F49D-46B9-815D-C5DBB5DCC9CE" );
            DeleteMetric( "5FFD8F4C-5199-41D7-BF33-14497DFEDD3F" );
            DeleteMetric( "12B581AD-EFE3-4A77-B2E7-80A385EFDDEE" );
            DeleteMetric( "95141ECF-F29F-4AEB-A966-B63AE21B9520" );
            DeleteMetric( "6EEB5736-E17A-44E2-AC27-0CF933E4EC37" );
            DeleteMetric( "411A8AF5-FE70-43E1-8BCE-C1FA52051663" );
            DeleteMetric( "93D36C22-6D88-4A2D-BCE2-0CF7CF1FC8F0" );
            DeleteMetric( "9BF88708-D258-49DA-928E-CF8D894EF21B" );
            DeleteMetric( "54EEDC65-4D5F-4E28-8709-7F282FA9412A" );

            // Delete Schedule
            Sql( $"DELETE FROM Schedule WHERE Guid = '{WHO_WE_ARE_SCHEDULE_GUID}'" );

            // Delete Categories
            RockMigrationHelper.DeleteCategory( GENDER_CATEGORY_GUID );
            RockMigrationHelper.DeleteCategory( AGE_RANGE_CATEGORY_GUID );
            RockMigrationHelper.DeleteCategory( INFORMATION_COMPLETENESS_CATEGORY_GUID );
            RockMigrationHelper.DeleteCategory( SystemGuid.Category.INSIGHTS );
        }

        private void DeleteMetric( string guid )
        {
            Sql( $@"DECLARE @MetricId [int] = (SELECT [Id] FROM [Metric] WHERE ([Guid] = '{guid}'));
IF (@MetricId IS NOT NULL)
BEGIN
    DELETE FROM [MetricPartition] WHERE ([MetricId] = @MetricId);
    DELETE FROM [MetricCategory] WHERE ([MetricId] = @MetricId);
    DELETE FROM [Metric] WHERE ([Id] = @MetricId);
END" );
        }

        /// <summary>
        /// Represents the metric partion details.
        /// </summary>
        private sealed class PartitionDetails
        {
            public PartitionDetails( string label, string entityTypeGuid )
            {
                Label = label;
                EntityTypeGuid = entityTypeGuid;
            }

            public PartitionDetails( string label, string entityTypeGuid, string entityTypeQualifierColumn, string entityTypeQualifierValue )
            {
                Label = label;
                EntityTypeGuid = entityTypeGuid;
                EntityTypeQualifierColumn = entityTypeQualifierColumn;
                EntityTypeQualifierValue = entityTypeQualifierValue;
            }

            public PartitionDetails()
            {
            }

            /// <summary>
            /// Gets or sets the label.
            /// </summary>
            /// <value>
            /// The label.
            /// </value>
            public string Label { get; set; }

            /// <summary>
            /// Gets or sets the entity type unique identifier.
            /// </summary>
            /// <value>
            /// The entity type unique identifier.
            /// </value>
            public string EntityTypeGuid { get; set; }

            /// <summary>
            /// Gets or sets the entity type qualifier column.
            /// </summary>
            /// <value>
            /// The entity type qualifier column.
            /// </value>
            public string EntityTypeQualifierColumn { get; set; }

            /// <summary>
            /// Gets or sets the entity type qualifier value.
            /// </summary>
            /// <value>
            /// The entity type qualifier value.
            /// </value>
            public string EntityTypeQualifierValue { get; set; }

            public string GetEntityTypeQualifierColumnValue()
            {
                return EntityTypeQualifierColumn.IsNullOrWhiteSpace() ? "NULL" : $"'{EntityTypeQualifierColumn}'";
            }

            public string GetEntityTypeQualifierValue()
            {
                return EntityTypeQualifierValue.IsNullOrWhiteSpace() ? "NULL" : $"(SELECT Id FROM [DefinedType] WHERE GUID = '{EntityTypeQualifierValue}')";
            }
        }
    }
}
