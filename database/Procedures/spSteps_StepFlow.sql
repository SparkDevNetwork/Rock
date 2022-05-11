/*
<doc>
	<summary>
		This stored procedure returns the data needed to create a Step Flow 
        chart for a provided Step Program.
	</summary>

	<returns>
		* Level - The step level. 1 = Steps that were a person's first step, 2 = The second steps
        * SourceStepTypeId - The last step taken before the current one. This will be null for a person's first step.
		* TargetStepTypeId - The current step that should be draw.
		* TargetStepTypeOrder - The configured order for the step type.
		* StepCount - The number of steps from the source to the target.
		* AvgNumberOfDaysBetweenSteps - The average number of days it tool a person to move from the source to the target step type at that level
	</returns>
	<param name='StepProgramId' datatype='int'>The step program to filter on.</param>
	<param name='MaxLevels' datatype='int'>The max levels to return data for.</param>
	<param name='DateRangeStartDate' datatype='datetime'>The start date to filter steps for.</param>
	<param name='DateRangeEndDate' datatype='datetime'>The end date to filter steps for.</param>
	<param name='DataViewId' datatype='int'>The data view to filter people who took the steps. This data view must be persisted.</param>
    <param name='CampusId' datatype='int'>The campus to filter steps for.</param>
	<remarks>	
		Pass in null values to the inputs to not filter.

        It's important that on the windowing functions that the order is by competition date AND step type order. This 
        is because many steps are completed on the same date. This additional order by keeps a consistent order. Without this
        the order could shift based on other filters (start date time/end date time) as SQL might chose to build the final results
        in a different order. In these cases the number of steps returned would be the same, but the levels would be in different
        orders.
	</remarks>
	<code>
		EXEC [dbo].[spSteps_StepFlow] 1, 4, null, null, null, null                  -- Show all steps in program 1 showing 4 levels
        EXEC [dbo].[spSteps_StepFlow] 1, 4, '01-01-2014', '01-01-2015', null, null  -- Show all steps in program 1 showing 4 levels in 2014
        EXEC [dbo].[spSteps_StepFlow] 1, 4, '01-01-2014', '01-01-2015', 10, 1       -- Show all steps in program 1 showing 4 levels in 2014 
                                                                                       who's individual is in data view 10 and the step is for campus 1
	</code>
</doc>
*/

ALTER PROCEDURE [dbo].[spSteps_StepFlow]
	@StepProgramId int
	, @MaxLevels int
	, @DateRangeStartDate datetime -- null means don't filter by start date
	, @DateRangeEndDate datetime -- null means don't filter by end date
    , @DataViewId int -- null means don't filter by a data view
	, @CampusId int -- null means all campuses
AS
BEGIN
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
                , LAG(st.[Id]) OVER (PARTITION BY pa.[PersonId], s.[StepProgramCompletionId] ORDER BY s.[CompletedDateTime], st.[Order] ) AS [SourceStepTypeId] 
                , LAG(st.[Name]) OVER (PARTITION BY pa.[PersonId], s.[StepProgramCompletionId] ORDER BY s.[CompletedDateTime], st.[Order] ) AS [SourceStepName]
                , DATEDIFF(dd, LAG(s.[CompletedDateTime]) OVER (PARTITION BY pa.[PersonId], s.[StepProgramCompletionId] ORDER BY s.[CompletedDateTime], st.[Order] ), s.[CompletedDateTime] ) AS [DaysSincePreviousStep]
                , ROW_NUMBER() OVER (PARTITION BY pa.[PersonId], s.[StepProgramCompletionId] ORDER BY s.[CompletedDateTime], st.[Order] ) AS [Level]
                , st.[Order] AS [TargetStepTypeOrder]
            FROM
                [Step] s 
                INNER JOIN [StepType] st ON st.[Id] = s.[StepTypeId]
                INNER JOIN [StepProgram] sp ON sp.[Id] = st.[StepProgramId] AND sp.[Id] = @StepProgramId
                INNER JOIN [PersonAlias] pa ON pa.[Id] = s.[PersonAliasId]
            WHERE 
                ( @DateRangeStartDate IS NULL OR s.[CompletedDateTime] >= @DateRangeStartDate  )
                AND ( @DateRangeEndDate IS NULL OR s.[CompletedDateTime] <= @DateRangeEndDate  )
                AND ( @DataViewId IS NULL OR pa.[PersonId] IN (SELECT [EntityId] FROM [DataViewPersistedValue] dvpv WHERE dvpv.[DataViewId] = @DataViewId ) )
                AND ( @CampusId IS NULL OR s.[CampusId] = @CampusId )
        ) x
    WHERE 
        x.[Level] <= @MaxLevels
    GROUP BY x.[TargetStepTypeId], x.[Level], x.[SourceStepTypeId], x.[TargetStepTypeOrder]
END