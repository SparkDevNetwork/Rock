/*
<doc>
	<summary>
		This stored procedure returns data used by the giving analytics block
	</summary>
</doc>
*/
ALTER PROCEDURE [dbo].[spFinance_GivingAnalyticsQuery]
	  @StartDate datetime = NULL
	, @EndDate datetime = NULL
	, @MinAmount decimal(18,2) = NULL
	, @MaxAmount decimal(18,2) = NULL
	, @AccountIds varchar(max) = NULL
	, @CurrencyTypeIds varchar(max) = NULL
	, @SourceTypeIds varchar(max) = NULL
	, @ViewBy varchar(1) = 'G'		-- G = Giving Leader, A = Adults, C = Children, F = Family

AS

BEGIN

	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @Sql varchar(max)
	DECLARE @Columns varchar(max) 
	DECLARE @ColumnSums varchar(max)

	IF @AccountIds IS NOT NULL 
	BEGIN
		SELECT 
			@Columns = COALESCE( @Columns + ',','' ) + QUOTENAME([Id]),
			@ColumnSums = COALESCE( @ColumnSums + ',' + CHAR(10) + '        ','' ) + 'SUM(' + QUOTENAME([Id]) + ') AS ' + QUOTENAME([id])
		FROM ufnUtility_CsvToTable( @AccountIds )
	END

	SET @Sql = '
	;WITH CTE
	AS
	(
		SELECT 
			[p].[GivingLeaderId],
			[ftd].[AccountId],
			MIN( [ft].[TransactionDateTime] ) AS [FirstGift],
			COUNT( DISTINCT [ft].[Id] ) AS [NumberGifts],
			SUM( [ftd].[Amount] ) AS [Amount]
		FROM [FinancialTransactionDetail] [ftd]
		INNER JOIN [FinancialTransaction] [ft] ON [ft].[Id] = [ftd].[TransactionId]
		INNER JOIN [PersonAlias] [pa] ON [pa].[Id] = [ft].[AuthorizedPersonAliasId]
		INNER JOIN [Person] [p] ON [p].[Id] = [pa].[PersonId]
		WHERE [p].[Id] IS NOT NULL'

	IF @StartDate IS NOT NULL
	BEGIN
		SET @Sql = @Sql + '
		AND [TransactionDateTime] >= ''' + CAST( @StartDate as varchar ) + ''''
	END

	IF @EndDate IS NOT NULL
	BEGIN
		SET @Sql = @Sql + '
		AND [TransactionDateTime] < ''' + CAST( @EndDate as varchar ) + ''''
	END

	IF @AccountIds IS NOT NULL
	BEGIN
		SET @Sql = @Sql + '
		AND ftd.[AccountId] in (select * from ufnUtility_CsvToTable( ''' + @AccountIds + ''' ) )'
	END

	IF @CurrencyTypeIds IS NOT NULL
	BEGIN
		SET @Sql = @Sql + '
		AND ft.[CurrencyTypeValueId] in (select * from ufnUtility_CsvToTable( ''' + @CurrencyTypeIds + ''' ) )'
	END

	IF @SourceTypeIds IS NOT NULL
	BEGIN
		SET @Sql = @Sql + '
		AND ft.[SourceTypeValueId] in (select * from ufnUtility_CsvToTable( ''' + @SourceTypeIds + ''' ) )'
	END

		SET @Sql = @Sql + '
		GROUP BY [p].[GivingLeaderId],[ftd].[AccountId]
	),

	CTE1
	AS
	(
		 SELECT 
			[GivingLeaderId],
			[AccountId],
			[Amount]
		FROM CTE
	),
'

	IF @Columns IS NOT NULL 
	BEGIN
		SET @Sql = @Sql + '
	CTE2
	AS
	(
		SELECT [GivingLeaderId],' + @Columns + '
		FROM CTE1
		PIVOT
		(
			SUM( [Amount] ) FOR [AccountId] IN ( ' + @Columns + ')
		) AS PivotResult
	),
	'
	END

	SET @Sql = @Sql + '
	CTE3
	AS
	(
		SELECT *
		FROM (
			SELECT [GivingLeaderId],
			MIN( [FirstGift] ) AS [FirstGift],
			SUM( [NumberGifts] ) AS [NumberGifts],
			SUM( [Amount] ) AS [TotalAmount]
			FROM CTE
			GROUP BY [GivingLeaderId]
		) [s]
		WHERE [GivingLeaderId] IS NOT NULL'

	IF @MinAmount IS NOT NULL
	BEGIN
		SET @Sql = @Sql + '
		AND [TotalAmount] >= ' + CAST(@MinAmount as varchar) 
	END

	IF @MaxAmount IS NOT NULL
	BEGIN
		SET @Sql = @Sql + '
		AND [TotalAmount] < ' + CAST(@MaxAmount as varchar) 
	END

	SET @Sql = @Sql + '
	),

	CTE4
	AS
	('

	IF @Columns IS NOT NULL 
	BEGIN
		SET @Sql = @Sql + '
		SELECT 
			CTE3.*,
			' + @Columns + '
		FROM CTE2 
		INNER JOIN CTE3 ON CTE2.[GivingLeaderId] = CTE3.[GivingLeaderId]'
	END
	ELSE
	BEGIN
		SET @Sql = @Sql + '
		SELECT * 
		FROM CTE3'
	END

	SET @Sql = @Sql + '
	)
'

	IF @ViewBy = 'G'
	BEGIN

		SET @Sql = @Sql + '
	SELECT 
		[p].[Id],
		[p].[Guid],
		[p].[NickName],
		[p].[LastName],
		[p].[NickName] + '' '' + [p].[LastName] AS [PersonName],
        [p].[Email],
		[p].[GivingId],
		CTE4.*
	FROM CTE4
	INNER JOIN [Person] [p] ON [p].[Id] = CTE4.[GivingLeaderId]
'

	END
	ELSE
	BEGIN

		DECLARE @AdultRoleId int = ( SELECT TOP 1 CAST([Id] as varchar) FROM [GroupTypeRole] WHERE [Guid] = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42' )
		DECLARE @ChildRoleId int = ( SELECT TOP 1 CAST([Id] as varchar)  FROM [GroupTypeRole] WHERE [Guid] = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9' )

		SET @Sql = @Sql + '
	SELECT 	
		[p].[Id],
		[p].[Guid],
		[p].[NickName],
		[p].[LastName],
		[p].[NickName] + '' '' + [p].[LastName] AS [PersonName],
        [p].[Email],
		[p].[GivingId],'

	IF @ColumnSums IS NOT NULL 
	BEGIN
		SET @Sql = @Sql + '
		' + @ColumnSums + ','
	END

	SET @Sql = @Sql + '
		MIN( CTE4.[FirstGift] ) AS [FirstGift],
		SUM( CTE4.[NumberGifts] ) AS [NumberGifts],
		SUM( CTE4.[TotalAmount] ) AS [TotalAmount]
	FROM CTE4
	INNER JOIN [GroupMember] [m]
		ON [m].[PersonId] = CTE4.[GivingLeaderId] 
		AND ( [m].[GroupRoleId] IN ( ' + CAST( @AdultRoleId as varchar )  + ',' + CAST( @ChildRoleId as varchar ) + ' ) )
	INNER JOIN [GroupMember] [m2]
		ON [m2].[GroupId] = [m].[GroupId]'

		IF @ViewBy = 'A'
		BEGIN
			SET @Sql = @Sql + '
		AND [m2].[GroupRoleId] = ' + CAST( @AdultRoleId as varchar )
		END

		IF @ViewBy = 'C'
		BEGIN
			SET @Sql = @Sql + '
		AND [m2].[GroupRoleId] = ' + CAST( @ChildRoleId as varchar )
		END

		SET @Sql = @Sql + '
	INNER JOIN [Person] [p] ON [p].[Id] = [m2].[PersonId]
	GROUP BY 
		[p].[Id],
		[p].[Guid],
		[p].[NickName],
		[p].[LastName],
		[p].[GivingId]
'		

	END

	SET @Sql = @Sql + '
	SELECT 
		[p2].[Id] AS [PersonId],
		MIN([ft].[TransactionDateTime]) AS [FirstEverGift]
	FROM [FinancialTransactionDetail] [ftd]
	INNER JOIN [FinancialAccount] [fa] 
		ON [fa].[id] = [ftd].[AccountId]
		AND [fa].[IsTaxDeductible] = 1
	INNER JOIN [FinancialTransaction] [ft] ON [ft].[Id] = [ftd].[TransactionId]
	INNER JOIN [PersonAlias] [pa] ON [pa].[Id] = [ft].[AuthorizedPersonAliasId]
	INNER JOIN [Person] [p] ON [p].[Id] = [pa].[PersonId]
	INNER JOIN [Person] [p2] ON [p2].[GivingId] = p.[GivingId]
	WHERE [ftd].[Amount] <> 0
	GROUP BY [p2].[Id]
'

	--SELECT @Sql
	EXEC ( @Sql )

END