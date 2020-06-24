// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class UpdateGivingAnalyticsStoredProcs : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            spFinance_GivingAnalyticsQuery_AccountTotals_Up();
            spFinance_GivingAnalyticsQuery_TransactionData_Up();
            spFinance_GivingAnalyticsQuery_PersonSummary_Up();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            spFinance_GivingAnalyticsQuery_AccountTotals_Down();
            spFinance_GivingAnalyticsQuery_TransactionData_Down();
            spFinance_GivingAnalyticsQuery_PersonSummary_Down();
        }

        private void spFinance_GivingAnalyticsQuery_AccountTotals_Up()
        {
            Sql( @"
                    ALTER PROCEDURE [dbo].[spFinance_GivingAnalyticsQuery_AccountTotals]
	                      @StartDate datetime = NULL
	                    , @EndDate datetime = NULL
	                    , @AccountIds varchar(max) = NULL
	                    , @CurrencyTypeIds varchar(max) = NULL
	                    , @SourceTypeIds varchar(max) = NULL
	                    , @TransactionTypeIds varchar(max) = NULL
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

	                    DECLARE @TransactionTypeTbl TABLE ( [Id] int )
	                    INSERT INTO @TransactionTypeTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@TransactionTypeIds,'') )

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
	                    ) AS [details]
	                    WHERE [GivingId] IS NOT NULL
	                    AND [AccountId] IS NOT NULL
	                    AND [Amount] IS NOT NULL
	                    GROUP BY [GivingId],[AccountId]

                    END
            " );
        }

        private void spFinance_GivingAnalyticsQuery_TransactionData_Up()
        {
            Sql( @"
                    ALTER PROCEDURE [dbo].[spFinance_GivingAnalyticsQuery_TransactionData]
	                        @StartDate datetime = NULL
	                    , @EndDate datetime = NULL
	                    , @AccountIds varchar(max) = NULL
	                    , @CurrencyTypeIds varchar(max) = NULL
	                    , @SourceTypeIds varchar(max) = NULL
	                    , @TransactionTypeIds varchar(max) = NULL
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

	                    DECLARE @TransactionTypeTbl TABLE ( [Id] int )
	                    INSERT INTO @TransactionTypeTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@TransactionTypeIds,'') )

 	                    SELECT
		                    [GivingId],
		                    [Amount],
		                    [TransactionDateTime],
		                    [SundayDate],
		                    DATEADD( day, -( DATEPART( day, [SundayDate] ) ) + 1, [SundayDate] ) AS [MonthDate],
		                    DATEADD( day, -( DATEPART( dayofyear, [SundayDate] ) ) + 1, [SundayDate] ) AS [YearDate],
		                    [AccountId],
		                    [AccountName],
		                    [GLCode],
		                    [CampusId]
	                    FROM (
		                    SELECT
			                    [p].[GivingId],
			                    [ftd].[Amount],
			                    [ft].[TransactionDateTime],
			                    [ft].[SundayDate],
			                    [fa].[Id] AS [AccountId],
			                    [fa].[Name] AS [AccountName],
			                    [fa].[GlCode] AS [GLCode],
			                    [fa].[CampusId]
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
	                    ) AS [details]

                    END
            " );
        }

        private void spFinance_GivingAnalyticsQuery_PersonSummary_Up()
        {
            Sql( @"
                    ALTER PROCEDURE [dbo].[spFinance_GivingAnalyticsQuery_PersonSummary]
	                      @StartDate datetime = NULL
	                    , @EndDate datetime = NULL
	                    , @MinAmount decimal(18,2) = NULL
	                    , @MaxAmount decimal(18,2) = NULL
	                    , @AccountIds varchar(max) = NULL
	                    , @CurrencyTypeIds varchar(max) = NULL
	                    , @SourceTypeIds varchar(max) = NULL
	                    , @TransactionTypeIds varchar(max) = NULL
	                    WITH RECOMPILE
                    AS

                    BEGIN

	                    SET @StartDate = COALESCE( CONVERT( date, @StartDate ), '1900-01-01' )
	                    SET @EndDate = COALESCE( @EndDate, '2100-01-01' )

	                    DECLARE @AdultRoleId int = ( SELECT TOP 1 CAST([Id] as varchar) FROM [GroupTypeRole] WHERE [Guid] = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42' )
	                    DECLARE @ChildRoleId int = ( SELECT TOP 1 CAST([Id] as varchar)  FROM [GroupTypeRole] WHERE [Guid] = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9' )

	                    DECLARE @AccountTbl TABLE ( [Id] int )
	                    INSERT INTO @AccountTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@AccountIds,'') )

	                    DECLARE @CurrencyTypeTbl TABLE ( [Id] int )
	                    INSERT INTO @CurrencyTypeTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@CurrencyTypeIds,'') )

	                    DECLARE @SourceTypeTbl TABLE ( [Id] int )
	                    INSERT INTO @SourceTypeTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@SourceTypeIds,'') )

	                    DECLARE @TransactionTypeTbl TABLE ( [Id] int )
	                    INSERT INTO @TransactionTypeTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@TransactionTypeIds,'') )

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
			                    ) AS [details]
			                    GROUP BY [GivingId]
		                    ) AS [s]
		                    WHERE [s].[GivingId] IS NOT NULL
		                    AND [s].[TotalAmount] >= ISNULL( @MinAmount, -2147483648 )
		                    AND [TotalAmount] <= ISNULL( @MaxAmount, 2147483647 )
	                    ) AS [t]
	                    INNER JOIN [Person] [p] ON [p].[GivingId] = [t].[GivingId]

                    END
            " );
        }

        private void spFinance_GivingAnalyticsQuery_PersonSummary_Down()
        {
            Sql( @"
                    ALTER PROCEDURE [dbo].[spFinance_GivingAnalyticsQuery_PersonSummary]
	                      @StartDate datetime = NULL
	                    , @EndDate datetime = NULL
	                    , @MinAmount decimal(18,2) = NULL
	                    , @MaxAmount decimal(18,2) = NULL
	                    , @AccountIds varchar(max) = NULL
	                    , @CurrencyTypeIds varchar(max) = NULL
	                    , @SourceTypeIds varchar(max) = NULL
	                    , @TransactionTypeIds varchar(max) = NULL
	                    WITH RECOMPILE
                    AS

                    BEGIN

	                    SET @StartDate = COALESCE( CONVERT( date, @StartDate ), '1900-01-01' )
	                    SET @EndDate = COALESCE( @EndDate, '2100-01-01' )

	                    DECLARE @AdultRoleId int = ( SELECT TOP 1 CAST([Id] as varchar) FROM [GroupTypeRole] WHERE [Guid] = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42' )
	                    DECLARE @ChildRoleId int = ( SELECT TOP 1 CAST([Id] as varchar)  FROM [GroupTypeRole] WHERE [Guid] = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9' )

	                    DECLARE @AccountTbl TABLE ( [Id] int )
	                    INSERT INTO @AccountTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@AccountIds,'') )

	                    DECLARE @CurrencyTypeTbl TABLE ( [Id] int )
	                    INSERT INTO @CurrencyTypeTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@CurrencyTypeIds,'') )

	                    DECLARE @SourceTypeTbl TABLE ( [Id] int )
	                    INSERT INTO @SourceTypeTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@SourceTypeIds,'') )

	                    DECLARE @TransactionTypeTbl TABLE ( [Id] int )
	                    INSERT INTO @TransactionTypeTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@TransactionTypeIds,'') )

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
				                    WHERE [ft].[TransactionDateTime] BETWEEN @StartDate AND @EndDate
				                    AND ( @AccountIds IS NULL OR [tt1].[Id] IS NOT NULL )
				                    AND ( @CurrencyTypeIds IS NULL OR [tt2].[Id] IS NOT NULL )
				                    AND ( @SourceTypeIds IS NULL OR [tt3].[Id] IS NOT NULL )
				                    AND ( @TransactionTypeIds IS NULL OR [tt4].[Id] IS NOT NULL )
			                    ) AS [details]
			                    GROUP BY [GivingId]
		                    ) AS [s]
		                    WHERE [s].[GivingId] IS NOT NULL
		                    AND [s].[TotalAmount] >= ISNULL( @MinAmount, -2147483648 )
		                    AND [TotalAmount] <= ISNULL( @MaxAmount, 2147483647 )
	                    ) AS [t]
	                    INNER JOIN [Person] [p] ON [p].[GivingId] = [t].[GivingId]

                    END
            " );
        }

        private void spFinance_GivingAnalyticsQuery_TransactionData_Down()
        {
            Sql( @"
                    ALTER PROCEDURE [dbo].[spFinance_GivingAnalyticsQuery_TransactionData]
	                      @StartDate datetime = NULL
	                    , @EndDate datetime = NULL
	                    , @AccountIds varchar(max) = NULL
	                    , @CurrencyTypeIds varchar(max) = NULL
	                    , @SourceTypeIds varchar(max) = NULL
	                    , @TransactionTypeIds varchar(max) = NULL
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

	                    DECLARE @TransactionTypeTbl TABLE ( [Id] int )
	                    INSERT INTO @TransactionTypeTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@TransactionTypeIds,'') )

 	                    SELECT
		                    [GivingId],
		                    [Amount],
		                    [TransactionDateTime],
		                    [SundayDate],
		                    DATEADD( day, -( DATEPART( day, [SundayDate] ) ) + 1, [SundayDate] ) AS [MonthDate],
		                    DATEADD( day, -( DATEPART( dayofyear, [SundayDate] ) ) + 1, [SundayDate] ) AS [YearDate],
		                    [AccountId],
		                    [AccountName],
		                    [GLCode],
		                    [CampusId]
	                    FROM (
		                    SELECT
			                    [p].[GivingId],
			                    [ftd].[Amount],
			                    [ft].[TransactionDateTime],
			                    [ft].[SundayDate],
			                    [fa].[Id] AS [AccountId],
			                    [fa].[Name] AS [AccountName],
			                    [fa].[GlCode] AS [GLCode],
			                    [fa].[CampusId]
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
		                    WHERE [ft].[TransactionDateTime] BETWEEN @StartDate AND @EndDate
		                    AND ( @AccountIds IS NULL OR [tt1].[Id] IS NOT NULL )
		                    AND ( @CurrencyTypeIds IS NULL OR [tt2].[Id] IS NOT NULL )
		                    AND ( @SourceTypeIds IS NULL OR [tt3].[Id] IS NOT NULL )
		                    AND ( @TransactionTypeIds IS NULL OR [tt4].[Id] IS NOT NULL )
	                    ) AS [details]

                    END
            " );
        }

        private void spFinance_GivingAnalyticsQuery_AccountTotals_Down()
        {
            Sql( @"
                ALTER PROCEDURE [dbo].[spFinance_GivingAnalyticsQuery_AccountTotals]
	                  @StartDate datetime = NULL
	                , @EndDate datetime = NULL
	                , @AccountIds varchar(max) = NULL
	                , @CurrencyTypeIds varchar(max) = NULL
	                , @SourceTypeIds varchar(max) = NULL
	                , @TransactionTypeIds varchar(max) = NULL
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

	                DECLARE @TransactionTypeTbl TABLE ( [Id] int )
	                INSERT INTO @TransactionTypeTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@TransactionTypeIds,'') )

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
		                WHERE [ft].[TransactionDateTime] BETWEEN @StartDate AND @EndDate
		                AND ( @AccountIds IS NULL OR [tt1].[Id] IS NOT NULL )
		                AND ( @CurrencyTypeIds IS NULL OR [tt2].[Id] IS NOT NULL )
		                AND ( @SourceTypeIds IS NULL OR [tt3].[Id] IS NOT NULL )
		                AND ( @TransactionTypeIds IS NULL OR [tt4].[Id] IS NOT NULL )
	                ) AS [details]
	                WHERE [GivingId] IS NOT NULL
	                AND [AccountId] IS NOT NULL
	                AND [Amount] IS NOT NULL
	                GROUP BY [GivingId],[AccountId]

                END
            " );
        }
    }
}
