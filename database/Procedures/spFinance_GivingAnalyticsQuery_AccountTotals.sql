ALTER PROCEDURE [dbo].[spFinance_GivingAnalyticsQuery_AccountTotals]
	                      @StartDate datetime = NULL
	                    , @EndDate datetime = NULL
	                    , @AccountIds varchar(max) = NULL
	                    , @CurrencyTypeIds varchar(max) = NULL
	                    , @SourceTypeIds varchar(max) = NULL
	                    , @TransactionTypeIds varchar(max) = NULL
						, @AllowOnlyActive bit = 0
						, @AllowOnlyTaxDeductible bit = 0
	                    WITH RECOMPILE
                    AS

                    BEGIN

	                    SET @StartDate = COALESCE( CONVERT( date, @StartDate ), '1900-01-01' )
	                    SET @EndDate = COALESCE( @EndDate, '2100-01-01' )

	                    DECLARE @AccountTbl TABLE ( [Id] int )
	                    INSERT INTO @AccountTbl SELECT value FROM STRING_SPLIT(@AccountIds,',')

	                    DECLARE @CurrencyTypeTbl TABLE ( [Id] int )
	                    INSERT INTO @CurrencyTypeTbl SELECT value FROM STRING_SPLIT(@CurrencyTypeIds,',')

	                    DECLARE @SourceTypeTbl TABLE ( [Id] int )
	                    INSERT INTO @SourceTypeTbl SELECT value FROM STRING_SPLIT(@SourceTypeIds,',')

	                    DECLARE @TransactionTypeTbl TABLE ( [Id] int )
	                    INSERT INTO @TransactionTypeTbl SELECT value FROM STRING_SPLIT(@TransactionTypeIds,',')

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
		                    LEFT OUTER JOIN @TransactionTypeTbl [tt4] ON [tt4].[id] = [ft].TransactionTypeValueId
		                    WHERE [ft].[TransactionDateTime] >= @StartDate 
		                    AND [ft].[TransactionDateTime] < @EndDate
		                    AND ( @AccountIds IS NULL OR [tt1].[Id] IS NOT NULL )
		                    AND ( @CurrencyTypeIds IS NULL OR [tt2].[Id] IS NOT NULL )
		                    AND ( @SourceTypeIds IS NULL OR [tt3].[Id] IS NOT NULL )
		                    AND ( @TransactionTypeIds IS NULL OR [tt4].[Id] IS NOT NULL )
							AND ( @AllowOnlyTaxDeductible IS NULL OR @AllowOnlyTaxDeductible = 0 OR [fa].[IsTaxDeductible] = 1)
							AND ( @AllowOnlyActive IS NULL OR @AllowOnlyActive = 0 OR [fa].[IsActive] = 1)
	                    ) AS [details]
	                    WHERE [GivingId] IS NOT NULL
	                    AND [AccountId] IS NOT NULL
	                    AND [Amount] IS NOT NULL
	                    GROUP BY [GivingId],[AccountId]

                    END