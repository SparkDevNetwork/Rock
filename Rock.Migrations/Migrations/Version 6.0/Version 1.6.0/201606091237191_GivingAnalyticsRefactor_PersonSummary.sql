IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spFinance_GivingAnalyticsQuery_PersonSummary]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spFinance_GivingAnalyticsQuery_PersonSummary]
GO

/*
<doc>
	<summary>
		This stored procedure returns data used by the giving analytics block
	</summary>
</doc>
*/
CREATE PROCEDURE [dbo].[spFinance_GivingAnalyticsQuery_PersonSummary]
	  @StartDate datetime = NULL
	, @EndDate datetime = NULL
	, @MinAmount decimal(18,2) = NULL
	, @MaxAmount decimal(18,2) = NULL
	, @AccountIds varchar(max) = NULL
	, @CurrencyTypeIds varchar(max) = NULL
	, @SourceTypeIds varchar(max) = NULL
	WITH RECOMPILE
AS

BEGIN

	SET @StartDate = COALESCE( CONVERT( date, @StartDate ), '1900-01-01' )
	SET @EndDate = COALESCE( @EndDate, '2100-01-01' )

	DECLARE @ContributionTypeId int = ( SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '2D607262-52D6-4724-910D-5C6E8FB89ACC' )
	DECLARE @AdultRoleId int = ( SELECT TOP 1 CAST([Id] as varchar) FROM [GroupTypeRole] WHERE [Guid] = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42' )
	DECLARE @ChildRoleId int = ( SELECT TOP 1 CAST([Id] as varchar)  FROM [GroupTypeRole] WHERE [Guid] = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9' )

	SELECT
		[p].[Id],
		[p].[Guid],
		[p].[NickName],
		[p].[LastName],
		[p].[Email],
		[t].[GivingId],
		[t].[FirstGift],
		[t].[LastGift],
		[t].[NumberGifts],
		[t].[TotalAmount],
		CASE WHEN [p].[Id] = [p].[GivingLeaderId]
			THEN CAST( 1 as bit )
			ELSE CAST( 0 as bit )
		END AS [IsGivingLeader],
		CASE WHEN EXISTS ( SELECT [Id] FROM [GroupMember] WHERE [PersonId] = [p].[Id] AND [GroupRoleId] = @AdultRoleId )
			THEN CAST( 1 as bit )
			ELSE CAST( 0 as bit )
		END AS [IsAdult],
		CASE WHEN EXISTS ( SELECT [Id] FROM [GroupMember] WHERE [PersonId] = [p].[Id] AND [GroupRoleId] = @ChildRoleId )
			THEN CAST( 1 as bit )
			ELSE CAST( 0 as bit )
		END AS [IsChild]
	FROM (
		SELECT *
		FROM (
			SELECT 
				[GivingId],
				MIN( [TransactionDateTime] ) AS [FirstGift],
				MAX( [TransactionDateTime] ) AS [LastGift],
				COUNT( DISTINCT [Id] ) AS [NumberGifts],
				SUM( [Amount] ) AS [TotalAmount]
			FROM (
				SELECT
					[p].[GivingId],
					[ftd].[AccountId],
					[ft].[TransactionDateTime],
					[ft].[Id],
					[ftd].[Amount]
				FROM [FinancialTransaction] [ft] WITH (NOLOCK)
				INNER JOIN [FinancialTransactionDetail] [ftd] WITH (NOLOCK) 
					ON [ftd].[TransactionId] = [ft].[Id]
				INNER JOIN [FinancialAccount] [fa] WITH (NOLOCK) 
					ON [fa].[Id] = [ftd].[AccountId]
					AND [fa].[IsTaxDeductible] = 1
				INNER JOIN [PersonAlias] [pa] WITH (NOLOCK) 
					ON [pa].[Id] = [ft].[AuthorizedPersonAliasId]
				INNER JOIN [Person] [p] WITH (NOLOCK) 
					ON [p].[Id] = [pa].[PersonId]
				LEFT OUTER JOIN [FinancialPaymentDetail] [fpd] WITH (NOLOCK) 
					ON [fpd].[Id] = [ft].[FinancialPaymentDetailId]
				WHERE [ft].[TransactionDateTime] BETWEEN @StartDate AND @EndDate
				AND [ft].[TransactionTypeValueId] IS NOT NULL
				AND [ft].[TransactionTypeValueId] = @ContributionTypeId
				AND ( @AccountIds IS NULL OR [ftd].[AccountId] IN ( SELECT * FROM ufnUtility_CsvToTable( @AccountIds ) ) )
				AND ( @CurrencyTypeIds IS NULL OR [fpd].[CurrencyTypeValueId] IN ( SELECT * FROM ufnUtility_CsvToTable( @CurrencyTypeIds ) ) )
				AND ( @SourceTypeIds IS NULL OR [ft].[SourceTypeValueId] IN ( SELECT * FROM ufnUtility_CsvToTable( @SourceTypeIds ) ) )
			) AS [details]
			GROUP BY [GivingId]
		) AS [s]
		WHERE [s].[GivingId] IS NOT NULL
		AND ( @MinAmount IS NULL OR [s].[TotalAmount] >= @MinAmount )
		AND ( @MaxAmount IS NULL OR [TotalAmount] <= @MaxAmount )
	) AS [t]
	INNER JOIN [Person] [p] ON [p].[GivingId] = [t].[GivingId]

END