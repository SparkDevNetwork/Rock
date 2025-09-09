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
using System.ComponentModel;
using System.Linq;
using System.Text;

using Microsoft.EntityFrameworkCore;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Job that updates Learning Program Completion Data.
    /// </summary>
    [DisplayName( "Update Step Program Completions" )]
    [Description( "Job that updates Step Program Completion Data." )]

    [IntegerField(
        "Command Timeout",
        Key = AttributeKey.CommandTimeoutSeconds,
        Description = "Maximum amount of time (in seconds) to wait for the SQL operations to complete. Leave blank to use the default for this job (180).",
        IsRequired = false,
        DefaultIntegerValue = 60 * 3,
        Order = 1 )]

    public class UpdateStepProgramCompletions : RockJob
    {
        /// <summary>
        /// Keys to use for Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string CommandTimeoutSeconds = "CommandTimeoutSeconds";
        }

        /// <inheritdoc/>
        public override void Execute()
        {
            JobState state;
            using ( var rockContext = CreateRockContext() )
            {
                state = InitializeJobState( rockContext );
            }

            foreach ( var programId in state.ProgramIds )
            {
                using ( var individualRockContext = CreateRockContext() )
                {
                    CleanupStepProgramCompletions( programId, individualRockContext, state );
                }
            }

            UpdateLastStatusMessage( GetJobResultText( state ) );
        }

        /// <summary>
        /// Creates a new <see cref="RockContext"/> configured for this job.
        /// </summary>
        /// <returns>An instance of <see cref="RockContext"/>.</returns>
        private RockContext CreateRockContext()
        {
            var rockContext = new RockContext();

            rockContext.Database.SetCommandTimeout( GetAttributeValue( AttributeKey.CommandTimeoutSeconds ).AsIntegerOrNull() ?? 180 );

            return rockContext;
        }

        /// <summary>
        /// Get the job state object that will contain the information and
        /// cache used while the job is running.
        /// </summary>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>A new instance of <see cref="JobState"/> that contains the job data.</returns>
        private JobState InitializeJobState( RockContext rockContext )
        {
            var programIds = new StepProgramService( rockContext )
                .Queryable()
                .Where( p => p.IsActive )
                .Select( p => p.Id )
                .ToList();

            return new JobState( programIds );
        }

        /// <summary>
        /// Performs cleanup for Step Program Completions by removing invalid completions and adding missing ones.
        /// </summary>
        /// <param name="programId">The Step Program Id we are checking.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <param name="jobState">The job state object</param>
        private static void CleanupStepProgramCompletions( int programId, RockContext rockContext, JobState jobState )
        {
            AddMissingCompletions( rockContext, jobState, programId );
        }

        /// <summary>
        /// Adds any missing Step Program Completions and updates affected Steps.
        /// </summary>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <param name="jobState">The job state object</param>
        /// <param name="programId">The Step Program Id we are checking.</param>
        private static void AddMissingCompletions( RockContext rockContext, JobState jobState, int programId )
        {
            var addMissingStepCompletionsQuery = $@"
DECLARE @CompletionsAdded INT = 0;
DECLARE @StepsUpdated INT = 0;

CREATE TABLE #NewCompletions (
    Id INT,
    PersonAliasId INT
);

;WITH [PeopleWithAllSteps] AS (
    SELECT 
        [s].[PersonAliasId],
        MIN(COALESCE([s].[StartDateTime], [s].[CreatedDateTime])) AS [StartDateTime],
        MAX(COALESCE([s].[CompletedDateTime], [s].[EndDateTime])) AS [EndDateTime],
        MAX([s].[CampusId]) AS [CampusId]
    FROM [Step] AS [s]
    INNER JOIN [StepType] AS [st] ON [s].[StepTypeId] = [st].[Id]
    INNER JOIN [StepStatus] AS [ss] ON [s].[StepStatusId] = [ss].[Id] AND [ss].[IsCompleteStatus] = 1
    WHERE [st].[StepProgramId] = {programId}
    GROUP BY [s].[PersonAliasId]
    HAVING COUNT(DISTINCT [s].[StepTypeId]) = (
        SELECT COUNT(*) 
        FROM [StepType] 
        WHERE [StepProgramId] = {programId} AND [IsActive] = 1
    )
)
, [MissingCompletions] AS (
    SELECT p.*
    FROM [PeopleWithAllSteps] p
    WHERE NOT EXISTS (
        SELECT 1
        FROM [StepProgramCompletion] c
        WHERE c.[StepProgramId] = {programId}
          AND c.[PersonAliasId] = p.[PersonAliasId]
    )
)

INSERT INTO [StepProgramCompletion] 
    ([StepProgramId], [PersonAliasId], [CampusId], 
     [StartDateTime], [EndDateTime], [CreatedDateTime], 
     [StartDateKey], [EndDateKey], [Guid])
OUTPUT INSERTED.[Id], INSERTED.[PersonAliasId]
INTO #NewCompletions ([Id], [PersonAliasId])
SELECT 
    {programId},
    [PersonAliasId],
    [CampusId],
    [StartDateTime],
    [EndDateTime],
    GETDATE(),
    CONVERT(INT, FORMAT([StartDateTime], 'yyyyMMdd')),
    CASE WHEN [EndDateTime] IS NOT NULL 
         THEN CONVERT(INT, FORMAT([EndDateTime], 'yyyyMMdd')) 
         ELSE NULL 
    END,
    NEWID()
FROM [MissingCompletions];

SET @CompletionsAdded = (SELECT COUNT(*) FROM #NewCompletions);

UPDATE [s]
SET [s].[StepProgramCompletionId] = nc.[Id]
FROM [Step] s
INNER JOIN #NewCompletions nc ON s.[PersonAliasId] = nc.[PersonAliasId]
INNER JOIN [StepType] st ON s.[StepTypeId] = st.[Id]
WHERE st.[StepProgramId] = {programId};

SET @StepsUpdated = @@ROWCOUNT;

DROP TABLE #NewCompletions;

;WITH [OrphanedCompletions] AS (
    SELECT c.Id, c.PersonAliasId
    FROM [StepProgramCompletion] c
    WHERE c.[StepProgramId] = {programId}
)
UPDATE s
SET s.[StepProgramCompletionId] = oc.[Id]
FROM [Step] s
INNER JOIN [StepType] st ON s.[StepTypeId] = st.[Id]
INNER JOIN [OrphanedCompletions] oc 
    ON s.[PersonAliasId] = oc.[PersonAliasId]
WHERE st.[StepProgramId] = {programId}
  AND s.[StepProgramCompletionId] IS NULL;

SET @StepsUpdated = @StepsUpdated + @@ROWCOUNT;

SELECT @CompletionsAdded AS CompletionsAdded, @StepsUpdated AS StepsUpdated;
";

            var result = rockContext.Database.SqlQuery<CleanupResult>( addMissingStepCompletionsQuery ).FirstOrDefault();

            if ( result != null )
            {
                jobState.CompletionsAdded += result.CompletionsAdded;
                jobState.StepsUpdated += result.StepsUpdated;
            }
        }

        /// <summary>
        /// Gets the status message for the Job.
        /// </summary>
        /// <param name="jobState">The job state object</param>
        /// <returns>The status message string</returns>
        private static string GetJobResultText( JobState jobState )
        {
            var completionsAddedText = "completion".PluralizeIf( jobState.CompletionsAdded != 1 );
            var stepsUpdatedText = "step".PluralizeIf( jobState.StepsUpdated != 1 );
            var results = new StringBuilder();

            results.AppendLine( $"{jobState.CompletionsAdded} {completionsAddedText} added." );
            results.AppendLine( $"{jobState.StepsUpdated} {stepsUpdatedText} updated." );

            return results.ToString();
        }

        #region Support Classes

        private class JobState
        {
            public List<int> ProgramIds { get; set; }

            public int CompletionsAdded { get; set; }

            public int StepsUpdated { get; set; }

            public JobState( List<int> programIds )
            {
                ProgramIds = programIds;
            }
        }

        private class CleanupResult
        {
            public int CompletionsAdded { get; set; }
            public int StepsUpdated { get; set; }
        }

        #endregion
    }
}
