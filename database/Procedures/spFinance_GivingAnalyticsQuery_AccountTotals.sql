/*
<doc>
	<summary>
		This stored procedure returns account totals for each giving id based on filter values
	</summary>
</doc>
*/
ALTER PROCEDURE [dbo].[spFinance_GivingAnalyticsQuery_AccountTotals]
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
		WHERE [ft].[TransactionDateTime] BETWEEN @StartDate AND @EndDate
		AND ( @AccountIds IS NULL OR [ftd].[AccountId] IN ( SELECT * FROM ufnUtility_CsvToTable( @AccountIds ) ) )
		AND ( @CurrencyTypeIds IS NULL OR [fpd].[CurrencyTypeValueId] IN ( SELECT * FROM ufnUtility_CsvToTable( @CurrencyTypeIds ) ) )
		AND ( @SourceTypeIds IS NULL OR [ft].[SourceTypeValueId] IN ( SELECT * FROM ufnUtility_CsvToTable( @SourceTypeIds ) ) )
	) AS [details]
	GROUP BY [GivingId],[AccountId]

END