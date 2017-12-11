IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spFinance_GivingAnalyticsQuery_AccountTotals]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spFinance_GivingAnalyticsQuery_AccountTotals]
GO

/*
<doc>
	<summary>
		This stored procedure returns account totals for each giving id based on filter values
	</summary>
</doc>
*/
CREATE PROCEDURE [dbo].[spFinance_GivingAnalyticsQuery_AccountTotals]
	  @StartDate datetime = NULL
	, @EndDate datetime = NULL
	, @AccountIds varchar(max) = NULL
	, @CurrencyTypeIds varchar(max) = NULL
	, @SourceTypeIds varchar(max) = NULL
	WITH RECOMPILE
AS

BEGIN

	SET @StartDate = COALESCE( CONVERT( date, @StartDate ), '1900-01-01' )
	SET @EndDate = COALESCE( @EndDate, '2100-01-01' )

	DECLARE @AccountTbl TABLE ( [Id] int )
	INSERT INTO @AccountTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@AccountIds,'') )

	DECLARE @CurrencyTypeTbl TABLE ( [Id] int )
	INSERT INTO @CurrencyTypeTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@CurrencyTypeIds,'') )

	DECLARE @SourceTypeTbl TABLE ( [Id] int )
	INSERT INTO @SourceTypeTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@SourceTypeIds,'') )

 	SELECT
		[GivingId],
		[AccountId],
		SUM( [Amount] ) AS [Amount]
	FROM (
		SELECT
			[p].[GivingId],
			[ftd].[AccountId],
			[ftd].[Amount]
		FROM [FinancialTransaction] [ft] WITH (NOLOCK)
		INNER JOIN [FinancialTransactionDetail] [ftd] WITH (NOLOCK) 
			ON [ftd].[TransactionId] = [ft].[Id]
		INNER JOIN [FinancialAccount] [fa] WITH (NOLOCK) 
			ON [fa].[Id] = [ftd].[AccountId]
		INNER JOIN [PersonAlias] [pa] WITH (NOLOCK) 
			ON [pa].[Id] = [ft].[AuthorizedPersonAliasId]
		INNER JOIN [Person] [p] WITH (NOLOCK) 
			ON [p].[Id] = [pa].[PersonId]
		LEFT OUTER JOIN [FinancialPaymentDetail] [fpd] WITH (NOLOCK) 
			ON [fpd].[Id] = [ft].[FinancialPaymentDetailId]
		LEFT OUTER JOIN @AccountTbl [tt1] ON [tt1].[id] = [ftd].[AccountId]
		LEFT OUTER JOIN @CurrencyTypeTbl [tt2] ON [tt2].[id] = [fpd].[CurrencyTypeValueId]
		LEFT OUTER JOIN @SourceTypeTbl [tt3] ON [tt3].[id] = [ft].[SourceTypeValueId]
		WHERE [ft].[TransactionDateTime] BETWEEN @StartDate AND @EndDate
		AND ( @AccountIds IS NULL OR [tt1].[Id] IS NOT NULL )
		AND ( @CurrencyTypeIds IS NULL OR [tt2].[Id] IS NOT NULL )
		AND ( @SourceTypeIds IS NULL OR [tt3].[Id] IS NOT NULL )
	) AS [details]
	WHERE [GivingId] IS NOT NULL
	AND [AccountId] IS NOT NULL
	AND [Amount] IS NOT NULL
	GROUP BY [GivingId],[AccountId]

END