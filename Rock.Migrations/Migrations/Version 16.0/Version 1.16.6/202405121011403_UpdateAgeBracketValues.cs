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
    /// Updates the AgeBracket database values to match their updated C# values.
    /// </summary>
    public partial class UpdateAgeBracketValues : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateAgeBracketOnAnalyticsSourceDate();
            UpdateAgeBracketOnPerson();
            UpdateAgeRangeMetricsUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UpdateAgeBracketOnAnalyticsSourceDate();
            UpdateAgeBracketOnPerson();
            UpdateAgeRangeMetricsDown();
        }

        private void UpdateAgeBracketOnAnalyticsSourceDate()
        {
            Sql( $@"
DECLARE @Today DATE = GETDATE()
BEGIN 
	UPDATE A
	SET [AgeBracket] = CASE
        -- When the age is between 0 and 5 then use the ZeroToFive value.
		WHEN (DATEDIFF(YEAR, A.[Date], @Today) - 
			CASE 
				WHEN DATEADD(YY, DATEDIFF(yy, A.[Date], @Today), A.[Date]) > @Today THEN 1
			    ELSE 0
		    END)
		BETWEEN 0 AND 5 THEN {Rock.Enums.Crm.AgeBracket.ZeroToFive.ConvertToInt()}
        -- When the age is between 6 and 12 then use the SixToTwelve value.
		WHEN (DATEDIFF(YEAR, A.[Date], @Today) - 
			CASE 
				WHEN DATEADD(YY, DATEDIFF(yy, A.[Date], @Today), A.[Date]) > @Today THEN 1
			    ELSE 0
		    END)
        BETWEEN 6 AND 12 THEN {Rock.Enums.Crm.AgeBracket.SixToTwelve.ConvertToInt()}
        -- When the age is between 13 and 17 then use the ThirteenToSeventeen value.
		WHEN (DATEDIFF(YEAR, A.[Date], @Today) - 
			CASE 
				WHEN DATEADD(YY, DATEDIFF(yy, A.[Date], @Today), A.[Date]) > @Today THEN 1
			    ELSE 0
		    END)
        BETWEEN 13 AND 17 THEN {Rock.Enums.Crm.AgeBracket.ThirteenToSeventeen.ConvertToInt()}
        -- When the age is between 18 and 24 then use the EighteenToTwentyFour value.
		WHEN (DATEDIFF(YEAR, A.[Date], @Today) - 
			CASE 
				WHEN DATEADD(YY, DATEDIFF(yy, A.[Date], @Today), A.[Date]) > @Today THEN 1
			    ELSE 0
		    END)
        BETWEEN 18 AND 24 THEN {Rock.Enums.Crm.AgeBracket.EighteenToTwentyFour.ConvertToInt()}
        -- When the age is between 25 and 34 then use the TwentyFiveToThirtyFour value.
		WHEN (DATEDIFF(YEAR, A.[Date], @Today) - 
			CASE 
				WHEN DATEADD(YY, DATEDIFF(yy, A.[Date], @Today), A.[Date]) > @Today THEN 1
			    ELSE 0
		    END)
        BETWEEN 25 AND 34 THEN {Rock.Enums.Crm.AgeBracket.TwentyFiveToThirtyFour.ConvertToInt()}
        -- When the age is between 35 and 44 then use the ThirtyFiveToFortyFour value.
		WHEN (DATEDIFF(YEAR, A.[Date], @Today) - 
			CASE 
				WHEN DATEADD(YY, DATEDIFF(yy, A.[Date], @Today), A.[Date]) > @Today THEN 1
			    ELSE 0
		    END)
        BETWEEN 35 AND 44 THEN {Rock.Enums.Crm.AgeBracket.ThirtyFiveToFortyFour.ConvertToInt()}
        -- When the age is between 45 and 54 then use the FortyFiveToFiftyFour value.
		WHEN (DATEDIFF(YEAR, A.[Date], @Today) - 
			CASE 
				WHEN DATEADD(YY, DATEDIFF(yy, A.[Date], @Today), A.[Date]) > @Today THEN 1
			    ELSE 0
		    END)
        BETWEEN 45 AND 54 THEN {Rock.Enums.Crm.AgeBracket.FortyFiveToFiftyFour.ConvertToInt()}
        -- When the age is between 55 and 64 then use the FiftyFiveToSixtyFour value.
		WHEN (DATEDIFF(YEAR, A.[Date], @Today) - 
			CASE 
				WHEN DATEADD(YY, DATEDIFF(yy, A.[Date], @Today), A.[Date]) > @Today THEN 1
			    ELSE 0
		    END)
        BETWEEN 55 AND 64 THEN {Rock.Enums.Crm.AgeBracket.FiftyFiveToSixtyFour.ConvertToInt()}
        -- When the age is greater than 65 then use the SixtyFiveOrOlder value.
		ELSE {Rock.Enums.Crm.AgeBracket.SixtyFiveOrOlder.ConvertToInt()}
	END
	FROM AnalyticsSourceDate A
	INNER JOIN AnalyticsSourceDate B
	ON A.[DateKey] = B.[DateKey]
	WHERE A.[Date] <= @Today
END
" );
        }

        private void UpdateAgeBracketOnPerson()
        {
            Sql( @"
BEGIN
	UPDATE Person
	SET [BirthDateKey] = FORMAT([BirthDate],'yyyyMMdd')

	UPDATE P
	SET P.[AgeBracket] = CASE
        WHEN A.[AgeBracket] IS NULL THEN 0
        ELSE A.[AgeBracket]
        END        
	FROM Person P
	LEFT JOIN AnalyticsSourceDate A
	ON A.[DateKey] = P.[BirthDateKey]
END
" );
        }

        private void UpdateAgeRangeMetricsUp()
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

            sql += string.Format( updateAgeRangeSql, "2", "909FA7E8-F991-44EC-B2BF-07BEE9A29558" );

            sql += string.Format( updateAgeRangeSql, "3", "5FFD8F4C-5199-41D7-BF33-14497DFEDD3F" );

            sql += string.Format( updateAgeRangeSql, "4", "12B581AD-EFE3-4A77-B2E7-80A385EFDDEE" );

            sql += string.Format( updateAgeRangeSql, "5", "95141ECF-F29F-4AEB-A966-B63AE21B9520" );

            sql += string.Format( updateAgeRangeSql, "6", "6EEB5736-E17A-44E2-AC27-0CF933E4EC37" );

            sql += string.Format( updateAgeRangeSql, "7", "411A8AF5-FE70-43E1-8BCE-C1FA52051663" );

            sql += string.Format( updateAgeRangeSql, "8", "93D36C22-6D88-4A2D-BCE2-0CF7CF1FC8F0" );

            sql += string.Format( updateAgeRangeSql, "9", "9BF88708-D258-49DA-928E-CF8D894EF21B" );

            sql += string.Format( updateAgeRangeSql, "0", "54EEDC65-4D5F-4E28-8709-7F282FA9412A" );

            Sql( sql );
        }

        private void UpdateAgeRangeMetricsDown()
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
            sql += string.Format( updateAgeRangeSql, "1", "909FA7E8-F991-44EC-B2BF-07BEE9A29558" );

            sql += string.Format( updateAgeRangeSql, "2", "5FFD8F4C-5199-41D7-BF33-14497DFEDD3F" );

            sql += string.Format( updateAgeRangeSql, "3", "12B581AD-EFE3-4A77-B2E7-80A385EFDDEE" );

            sql += string.Format( updateAgeRangeSql, "4", "95141ECF-F29F-4AEB-A966-B63AE21B9520" );

            sql += string.Format( updateAgeRangeSql, "5", "6EEB5736-E17A-44E2-AC27-0CF933E4EC37" );

            sql += string.Format( updateAgeRangeSql, "6", "411A8AF5-FE70-43E1-8BCE-C1FA52051663" );

            sql += string.Format( updateAgeRangeSql, "7", "93D36C22-6D88-4A2D-BCE2-0CF7CF1FC8F0" );

            sql += string.Format( updateAgeRangeSql, "8", "9BF88708-D258-49DA-928E-CF8D894EF21B" );

            sql += string.Format( updateAgeRangeSql, "9", "EEDEE264-F49D-46B9-815D-C5DBB5DCC9CE" );

            sql += string.Format( updateAgeRangeSql, "0", "54EEDC65-4D5F-4E28-8709-7F282FA9412A" );

            Sql( sql );
        }
    }
}
