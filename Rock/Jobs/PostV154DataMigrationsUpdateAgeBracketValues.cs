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

using System.ComponentModel;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Run once job for v15.4 to update the AgeBracket values to reflect the new values after splitting the 0 - 12 bracket.
    /// </summary>
    [DisplayName( "Rock Update Helper v15.4 - Update AgeBracket Values" )]
    [Description( "This job will update the AgeBracket values to reflect the new values after splitting the 0 - 12 bracket." )]

    [IntegerField(
    "Command Timeout",
    Key = AttributeKey.CommandTimeout,
    Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of transactions, this could take several minutes or more.",
    IsRequired = false,
    DefaultIntegerValue = 14400 )]
    public class PostV154DataMigrationsUpdateAgeBracketValues : RockJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            // get the configured timeout, or default to 240 minutes if it is blank
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 14400;
            var jobMigration = new JobMigration( commandTimeout );

            CalculateAgeAndAgeBracketOnAnalyticsSourceDate( jobMigration );
            UpdateAgeAndAgeBracketOnPerson( jobMigration );

            DeleteJob();
        }

        /// <summary>
        /// Updates the age and age bracket on analytics source date.
        /// </summary>
        /// <param name="jobMigration">The job migration.</param>
        private void CalculateAgeAndAgeBracketOnAnalyticsSourceDate( JobMigration jobMigration )
        {
            jobMigration.Sql($@"
DECLARE @Today DATE = GETDATE()
BEGIN 
	UPDATE A
	SET [Age] = DATEDIFF(YEAR, A.[Date], @Today) - 
	CASE 
		WHEN DATEADD(YY, DATEDIFF(yy, A.[Date], @Today), A.[Date]) > @Today THEN 1
		ELSE 0
	END,
	[AgeBracket] = CASE
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
");
        }

        /// <summary>
        /// Updates the age and age bracket on person.
        /// </summary>
        /// <param name="jobMigration">The job migration.</param>
        private void UpdateAgeAndAgeBracketOnPerson( JobMigration jobMigration )
        {
            jobMigration.Sql( $@"
BEGIN
	UPDATE Person
	SET [BirthDateKey] = FORMAT([BirthDate],'yyyyMMdd')

	UPDATE P
	SET P.[Age] = CASE
		WHEN P.[DeceasedDate] IS NOT NULL THEN
			(DATEDIFF(YEAR, A.[Date], P.[DeceasedDate]) - 
				CASE 
					WHEN DATEADD(YY, DATEDIFF(yy, A.[Date], P.[DeceasedDate]), P.[DeceasedDate]) > p.[DeceasedDate] THEN 1
					ELSE 0
				END)
		WHEN p.[BirthDate] IS NULL THEN NULL
		ELSE A.[Age] 
		END,
	P.[AgeBracket] = CASE
        WHEN A.[AgeBracket] IS NULL THEN -1
        ELSE A.[AgeBracket]
        END        
	FROM Person P
	LEFT JOIN AnalyticsSourceDate A
	ON A.[DateKey] = P.[BirthDateKey]
END
" );
        }

        /// <summary>
        /// Deletes the job.
        /// </summary>
        private void DeleteJob()
        {
            using ( var rockContext = new RockContext() )
            {
                var jobService = new ServiceJobService( rockContext );
                var job = jobService.Get( GetJobId() );

                if ( job != null )
                {
                    jobService.Delete( job );
                    rockContext.SaveChanges();
                }
            }
        }
    }
}
