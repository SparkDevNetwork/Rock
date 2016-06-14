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

	DECLARE @ContributionTypeId int = ( SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '2D607262-52D6-4724-910D-5C6E8FB89ACC' )

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
	GROUP BY [GivingId],[AccountId]

END