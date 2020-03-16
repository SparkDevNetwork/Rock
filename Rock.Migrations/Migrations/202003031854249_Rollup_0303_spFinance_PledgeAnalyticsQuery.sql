/*
<doc>
	<summary>
		This stored procedure returns data used by the pledge analytics block
	</summary>
</doc>
*/
ALTER PROCEDURE [dbo].[spFinance_PledgeAnalyticsQuery]
	  @AccountId int
	, @StartDate datetime = NULL
	, @EndDate datetime = NULL
	, @MinAmountPledged decimal(18,2) = NULL
	, @MaxAmountPledged decimal(18,2) = NULL
	, @MinComplete decimal(18,2) = NULL
	, @MaxComplete decimal(18,2) = NULL
	, @MinAmountGiven decimal(18,2) = NULL
	, @MaxAmountGiven decimal(18,2) = NULL
	, @IncludePledges bit = 1
	, @IncludeGifts bit = 0

AS

BEGIN

	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF @StartDate IS NULL
	BEGIN
		SET @StartDate = CAST('17530101' as datetime)
	END

	IF @EndDate IS NULL
	BEGIN
		SET @EndDate = CAST('99991231' as datetime)
	END

	DECLARE @Sql varchar(max)
	SET @Sql = '
	;WITH CTE_ACCOUNTS AS (
		SELECT [Id]
		FROM [FinancialAccount] WHERE Id = ' + CAST(@AccountId as varchar) + '
		UNION ALL
		SELECT A.[Id]
		FROM [FinancialAccount] A
		INNER JOIN [CTE_ACCOUNTS] CTE ON CTE.[Id] = A.[ParentAccountId]
	),

	CTE
	AS
	(
		SELECT 
			[p].[GivingLeaderId],
			[fp].[TotalAmount] AS [PledgeAmount],
			1 AS [PledgeCount],
			0 AS [GiftAmount],
			0 AS [GiftCount]
		FROM [FinancialPledge] [fp]
		INNER JOIN [PersonAlias] [pa] ON [pa].[Id] = [fp].[PersonAliasId]
		INNER JOIN [Person] [p] ON [p].[Id] = [pa].[PersonId]
		WHERE [fp].[AccountId] = ' + CAST(@AccountId as varchar)

		SET @Sql = @Sql + '
		AND ( 
			( [StartDate] >= ''' + CAST( @StartDate as varchar ) + ''' AND [StartDate] < ''' + CAST( @EndDate as varchar ) + ''') OR
			( [EndDate] >= ''' + CAST( @StartDate as varchar ) + ''' AND [EndDate] < ''' + CAST( @EndDate as varchar ) + ''') OR
			( ''' + CAST( @StartDate as varchar ) + ''' >= [StartDate] AND ''' + CAST( @StartDate as varchar ) + ''' < [EndDate]) OR
			( ''' + CAST( @EndDate as varchar ) + ''' > [StartDate] AND ''' + CAST( @EndDate as varchar ) + ''' < [EndDate])
		)'

	SET @Sql = @Sql + '

		UNION ALL

		SELECT 
			[p].[GivingLeaderId],
			0 AS [PledgeAmount],
			0 AS [PledgeCount],
			[ftd].[Amount] AS [GiftAmount],
			1 AS [GiftCount]
		FROM [FinancialTransactionDetail] [ftd]
		INNER JOIN [CTE_ACCOUNTS] [a] ON [a].[Id] = [ftd].[AccountId]
		INNER JOIN [FinancialTransaction] [ft] ON [ft].[Id] = [ftd].[TransactionId]
		INNER JOIN [PersonAlias] [pa] ON [pa].[Id] = [ft].[AuthorizedPersonAliasId]
		INNER JOIN [Person] [p] ON [p].[Id] = [pa].[PersonId]
	),

	CTE1
	AS
	(
		SELECT 
			[GivingLeaderId],
			SUM( [PledgeAmount] ) AS [PledgeAmount],
			SUM( [PledgeCount] ) AS [PledgeCount],
			SUM( [GiftAmount] ) AS [GiftAmount],
			SUM( [GiftCount] ) AS [GiftCount]
		FROM CTE
		GROUP BY [GivingLeaderId]
	),

	CTE2
	AS
	(
		SELECT
			CTE1.*,
			CASE WHEN [PledgeAmount] > 0 
				THEN CAST( ( [GiftAmount] / [PledgeAmount] ) as decimal(9,2) )
				ELSE CAST( 0 as decimal(9, 2) )
			END AS [PercentComplete]
		FROM CTE1
	)

	SELECT 
		[p].[Id],
		[p].[Guid],
		[p].[NickName],
		[p].[LastName],
		[p].[NickName] + '' '' + [p].[LastName] AS [PersonName],
        [p].[Email],
		[p].[GivingId],
		CTE2.*
	FROM CTE2
	INNER JOIN [Person] [p] ON [p].[Id] = CTE2.[GivingLeaderId]
	WHERE P.[Id] IS NOT NULL'

	IF @IncludePledges IS NOT NULL AND @IncludePledges = 1
	AND @IncludeGifts IS NOT NULL AND @IncludeGifts = 1
	BEGIN
			SET @Sql = @Sql + '
	AND ( [PledgeAmount] > 0 OR [GiftAmount] > 0 )'
	END
	ELSE
	BEGIN
		IF @IncludePledges IS NOT NULL AND @IncludePledges = 1
		BEGIN
			SET @Sql = @Sql + '
	AND [PledgeAmount] > 0' 
		END

		IF @IncludeGifts IS NOT NULL AND @IncludeGifts = 1
		BEGIN
			SET @Sql = @Sql + '
	AND [GiftAmount] > 0' 
		END
	END

	IF @MinAmountPledged IS NOT NULL
	BEGIN
		SET @Sql = @Sql + '
	AND [PledgeAmount] >= ' + CAST(@MinAmountPledged as varchar) 
	END

	IF @MaxAmountPledged IS NOT NULL
	BEGIN
		SET @Sql = @Sql + '
	AND [PledgeAmount] <= ' + CAST(@MaxAmountPledged as varchar) 
	END

	IF @MinComplete IS NOT NULL
	BEGIN
		SET @Sql = @Sql + '
	AND [PercentComplete] >= ' + CAST(@MinComplete as varchar) 
	END

	IF @MaxComplete IS NOT NULL
	BEGIN
		SET @Sql = @Sql + '
	AND [PercentComplete] <= ' + CAST(@MaxComplete as varchar) 
	END

	IF @MinAmountGiven IS NOT NULL
	BEGIN
		SET @Sql = @Sql + '
	AND [GiftAmount] >= ' + CAST(@MinAmountGiven as varchar) 
	END

	IF @MaxAmountGiven IS NOT NULL
	BEGIN
		SET @Sql = @Sql + '
	AND [GiftAmount] <= ' + CAST(@MaxAmountGiven as varchar) 
	END

		SET @Sql = @Sql + '
	ORDER BY [p].[LastName], [p].[NickName]'

	--SELECT @Sql
	EXEC ( @Sql )

END