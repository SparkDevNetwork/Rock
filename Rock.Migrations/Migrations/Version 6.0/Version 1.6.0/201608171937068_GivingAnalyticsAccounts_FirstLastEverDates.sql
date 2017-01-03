IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spFinance_GivingAnalyticsQuery_FirstLastEverDates]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spFinance_GivingAnalyticsQuery_FirstLastEverDates]
GO

/*
<doc>
	<summary>
		This stored procedure returns the first ever, and last ever dates that each giving id
		gave a contribution to a tax deductible account
	</summary>
</doc>
*/
CREATE PROCEDURE [dbo].[spFinance_GivingAnalyticsQuery_FirstLastEverDates]
AS

BEGIN

	SELECT 
		[p].[GivingId],
		MIN([ft].[TransactionDateTime]) AS [FirstEverGift],
		MAX([ft].[TransactionDateTime]) AS [LastEverGift]
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
	GROUP BY [p].[GivingId]

END