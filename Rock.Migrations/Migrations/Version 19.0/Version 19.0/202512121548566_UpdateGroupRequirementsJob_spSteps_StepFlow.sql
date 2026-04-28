/*
<doc>
    <summary>
        This stored procedure returns the data needed to create a Step Flow 
        chart for a provided Step Program. It supports filtering on date 
        ranges, data views, campuses, and an optional list of starting 
        step type IDs via a table-valued parameter.
    </summary>

    <returns>
        * Level - The step level. 1 = Steps that were a person's first step, 2 = The second steps, etc.
        * SourceStepTypeId - The last step taken before the current one. This will be null for a person's first step.
        * TargetStepTypeId - The current step that should be drawn.
        * TargetStepTypeOrder - The configured order for the step type.
        * StepCount - The number of steps from the source to the target.
        * AvgNumberOfDaysBetweenSteps - The average number of days it took a person to move from the source to the target step type at that level.
    </returns>

    <param name='StepProgramId' datatype='int'>The step program to filter on.</param>
    <param name='MaxLevels' datatype='int'>The max levels to return data for.</param>
    <param name='DateRangeStartDate' datatype='datetime'>The start date to filter steps for.</param>
    <param name='DateRangeEndDate' datatype='datetime'>The end date to filter steps for.</param>
    <param name='DataViewId' datatype='int'>The data view to filter people who took the steps. This data view must be persisted.</param>
    <param name='CampusId' datatype='int'>The campus to filter steps for.</param>
    <param name='StartingStepTypeIds' datatype='dbo.IdList'>
        A table-valued parameter (Id INT). If provided, only step completions 
        whose very first step type is in this list will be included. If empty, no filter is applied.
    </param>

    <remarks> 
        Pass in null values for filter parameters to not filter. Pass an empty table 
        for @StartingStepTypeIds to include all completions.

        It's important that on the windowing functions the ORDER BY includes both 
        CompletedDateTime and StepType.Order. Many steps can be completed on the 
        same date. This tie-breaker ensures consistent sequencing; without it, SQL 
        may return steps in different orders depending on query plan, which can 
        change level assignments while the counts remain the same.
    </remarks>

    <code>
        -- Show all steps in program 1, showing 4 levels
        EXEC [dbo].[spSteps_StepFlow] 1, 4, NULL, NULL, NULL, NULL, @StartingStepTypeIds = dbo.IdList()

        -- Show steps in program 1 for 2014, showing 4 levels
        EXEC [dbo].[spSteps_StepFlow] 1, 4, '2014-01-01', '2015-01-01', NULL, NULL, @StartingStepTypeIds = dbo.IdList()

        -- Show steps in program 1 for 2014, restricted to DataView 10, Campus 1, and only completions 
        -- that started with StepTypeId 5 or 6
        DECLARE @ids dbo.IdList;
        INSERT INTO @ids (Id) VALUES (5), (6);

        EXEC [dbo].[spSteps_StepFlow] 1, 4, '2014-01-01', '2015-01-01', 10, 1, @ids;
    </code>
</doc>
*/

CREATE PROCEDURE [dbo].[spSteps_StepFlow]
    @StepProgramId INT,
    @MaxLevels INT,
    @DateRangeStartDate DATETIME = NULL, -- null means don't filter by start date
    @DateRangeEndDate DATETIME = NULL,   -- null means don't filter by end date
    @DataViewId INT = NULL,              -- null means don't filter by a data view
    @CampusId INT = NULL,                -- null means all campuses
    @StartingStepTypeIds dbo.IdList READONLY -- table-valued param; empty = no filter
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
          x.[TargetStepTypeId]
        , x.[Level]
        , x.[SourceStepTypeId]
        , x.[TargetStepTypeOrder]
        , COUNT(*) AS [StepCount]
        , AVG(x.[DaysSincePreviousStep]) AS [AvgNumberOfDaysBetweenSteps]
    FROM 
        (
            SELECT 
                  pa.[PersonId]
                , s.[StepProgramCompletionId]
                , s.[CompletedDateTime]
                , st.[Id] AS [TargetStepTypeId]
                , LAG(st.[Id]) OVER (
                      PARTITION BY pa.[PersonId], s.[StepProgramCompletionId] 
                      ORDER BY s.[CompletedDateTime], st.[Order]
                  ) AS [SourceStepTypeId] 
                , LAG(st.[Name]) OVER (
                      PARTITION BY pa.[PersonId], s.[StepProgramCompletionId] 
                      ORDER BY s.[CompletedDateTime], st.[Order]
                  ) AS [SourceStepName]
                , DATEDIFF(dd, 
                      LAG(s.[CompletedDateTime]) OVER (
                          PARTITION BY pa.[PersonId], s.[StepProgramCompletionId] 
                          ORDER BY s.[CompletedDateTime], st.[Order]
                      ), 
                      s.[CompletedDateTime]
                  ) AS [DaysSincePreviousStep]
                , ROW_NUMBER() OVER (
                      PARTITION BY pa.[PersonId], s.[StepProgramCompletionId] 
                      ORDER BY s.[CompletedDateTime], st.[Order]
                  ) AS [Level]
                , st.[Order] AS [TargetStepTypeOrder]
                , FIRST_VALUE(st.[Id]) OVER (
                      PARTITION BY pa.[PersonId], s.[StepProgramCompletionId]
                      ORDER BY s.[CompletedDateTime], st.[Order]
                      ROWS BETWEEN UNBOUNDED PRECEDING AND UNBOUNDED FOLLOWING
                  ) AS [FirstTargetStepTypeId]
            FROM
                [Step] s 
                INNER JOIN [StepType] st ON st.[Id] = s.[StepTypeId]
                INNER JOIN [StepProgram] sp ON sp.[Id] = st.[StepProgramId] AND sp.[Id] = @StepProgramId
                INNER JOIN [PersonAlias] pa ON pa.[Id] = s.[PersonAliasId]
            WHERE 
                ( @DateRangeStartDate IS NULL OR s.[CompletedDateTime] >= @DateRangeStartDate )
                AND ( @DateRangeEndDate   IS NULL OR s.[CompletedDateTime] <= @DateRangeEndDate )
                AND ( @DataViewId IS NULL OR pa.[PersonId] IN (
                        SELECT [EntityId] 
                        FROM [DataViewPersistedValue] dvpv 
                        WHERE dvpv.[DataViewId] = @DataViewId
                    ))
                AND ( @CampusId IS NULL OR s.[CampusId] = @CampusId )
        ) x
    WHERE 
        x.[Level] <= @MaxLevels
        AND (
            NOT EXISTS (SELECT 1 FROM @StartingStepTypeIds) -- no filter if empty
            OR x.[FirstTargetStepTypeId] IN (SELECT Id FROM @StartingStepTypeIds)
        )
    GROUP BY 
          x.[TargetStepTypeId]
        , x.[Level]
        , x.[SourceStepTypeId]
        , x.[TargetStepTypeOrder];
END
