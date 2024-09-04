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

    using Rock.Configuration;
    using Rock.Enums.Configuration;
    using Rock.Utility.Settings;

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20221117 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddSql2016CheckToAdminCheckList();
            UserLoginViewAccessToAllUsersUp();
            Update_spFinance_GivingAnalyticsQuery_TransactionData();
            Update_spFinance_GivingAnalyticsQuery_PersonSummary();
            Update_spFinance_GivingAnalyticsQuery_AccountTotals();
            GetCurrentPersonImpersonationSecurityUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UserLoginViewAccessToAllUsersDown();
            GetCurrentPersonImpersonationSecurityDown();
        }

        /// <summary>
        /// KA: Migration to add Item to AdminCheckList for Sql server 2016 check.
        /// </summary>
        private void AddSql2016CheckToAdminCheckList()
        {
            var is2016OrHigher = CheckSqlServerVersionGreaterThanSqlServer2016();
            if ( !is2016OrHigher )
            {
                RockMigrationHelper.UpdateDefinedValue( "4BF34677-37E9-4E71-BD03-252B66C9373D", "Upgrade to SQL Server 2016 or Later", "Please remember that starting with v15 Rock will no longer support SQL Server 2014 (see this link<https://community.rockrms.com/connect/ending-support-for-sql-server-2014> for more details).", "6538909A-B75E-46C3-A141-3CA02DD6DE06" );
            }
        }

        /// <summary>
        /// Checks the SQL server version greater than SQL server2016.
        /// Used by AddSql2016CheckToAdminCheckList()
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool CheckSqlServerVersionGreaterThanSqlServer2016()
        {
            var database = RockApp.Current.GetDatabaseConfiguration();

            var isOk = database.Platform == DatabasePlatform.AzureSql;
            if ( !isOk )
            {
                try
                {
                    var versionParts = database.VersionNumber.Split( '.' );
                    int.TryParse( versionParts[0], out var majorVersion );
                    if ( majorVersion > 13 )
                    {
                        isOk = true;
                    }
                }
                catch
                {
                    // This would be pretty bad, but regardless we'll just
                    // return the isOk (not) and let the caller proceed.
                }
            }

            return isOk;
        }

        /// <summary>
        /// SK: Give 'All Users' View access to UserLogin Available API Endpoint
        /// </summary>
        private void UserLoginViewAccessToAllUsersUp()
        {
            // This is the wrong RestActionGuid. 
            // We ran into an issue in which the RestAction is created
            // in the CreateDatabase file with the wrong Guid.
            var oldUserLoginsAvailableRestActionGuid = "f659ee78-4dae-44fc-b5b7-0ab2dbb741fa";

            RockMigrationHelper.AddRestAction( oldUserLoginsAvailableRestActionGuid,
                "UserLogins",
                "Rock.Rest.Controllers.UserLoginsController" );

            RockMigrationHelper.AddSecurityAuthForRestAction( oldUserLoginsAvailableRestActionGuid, 
                0, 
                "View", 
                true, 
                string.Empty, 
                Model.SpecialRole.AllUsers, 
                "49E34E32-EF7E-41D4-AD5C-C97E09295D68" );
        }
    
        /// <summary>
        /// SK: Give 'All Users' View access to UserLogin Available API Endpoint
        /// </summary>
        private void UserLoginViewAccessToAllUsersDown()
        {
            RockMigrationHelper.DeleteSecurityAuth( "49E34E32-EF7E-41D4-AD5C-C97E09295D68" );
        }
    
        /// <summary>
        /// PA:Migration to Update spFinance_GivingAnalyticsQuery_TransactionData.sql to exclude the inactive and non tax deductible transactions if needed
        /// </summary>
        private void Update_spFinance_GivingAnalyticsQuery_TransactionData()
        {
            Sql( $@"ALTER PROCEDURE [dbo].[spFinance_GivingAnalyticsQuery_TransactionData]
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
							AND ( @AllowOnlyTaxDeductible IS NULL OR @AllowOnlyTaxDeductible = 0 OR [fa].[IsTaxDeductible] = 1)
							AND ( @AllowOnlyActive IS NULL OR @AllowOnlyActive = 0 OR [fa].[IsActive] = 1)
	                    ) AS [details]

                    END" );
        }

        /// <summary>
        /// PA:Migration to Update Update_spFinance_GivingAnalyticsQuery_PersonSummary.sql to exclude the inactive and non tax deductible transactions if needed
        /// </summary>
        private void Update_spFinance_GivingAnalyticsQuery_PersonSummary()
        {
            Sql( $@"ALTER PROCEDURE [dbo].[spFinance_GivingAnalyticsQuery_PersonSummary]
	                  @StartDate datetime = NULL
	                , @EndDate datetime = NULL
	                , @MinAmount decimal(18,2) = NULL
	                , @MaxAmount decimal(18,2) = NULL
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
					                AND ( @AllowOnlyTaxDeductible IS NULL OR @AllowOnlyTaxDeductible = 0 OR [fa].[IsTaxDeductible] = 1)
					                AND ( @AllowOnlyActive IS NULL OR @AllowOnlyActive = 0 OR [fa].[IsActive] = 1)
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
				                WHERE [ft].[TransactionDateTime] >= @StartDate AND [ft].[TransactionDateTime] < @EndDate
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

                END" );
        }

        /// <summary>
        /// PA:Migration to Update spFinance_GivingAnalyticsQuery_AccountTotals.sql to exclude the inactive and non tax deductible transactions if needed
        /// </summary>
        private void Update_spFinance_GivingAnalyticsQuery_AccountTotals()
        {
            Sql( $@"ALTER PROCEDURE [dbo].[spFinance_GivingAnalyticsQuery_AccountTotals]
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
							AND ( @AllowOnlyTaxDeductible IS NULL OR @AllowOnlyTaxDeductible = 0 OR [fa].[IsTaxDeductible] = 1)
							AND ( @AllowOnlyActive IS NULL OR @AllowOnlyActive = 0 OR [fa].[IsActive] = 1)
	                    ) AS [details]
	                    WHERE [GivingId] IS NOT NULL
	                    AND [AccountId] IS NOT NULL
	                    AND [Amount] IS NOT NULL
	                    GROUP BY [GivingId],[AccountId]

                    END" );
        }
    
        /// <summary>
        /// BC: Adding access for all authenticated users to receive an impersonation token using this endpoint.
        /// Fixes: https://app.asana.com/0/1122678768870900/1203305042901245/f
        /// </summary>
        private void GetCurrentPersonImpersonationSecurityUp()
        {
            RockMigrationHelper.AddRestAction( "8911b107-8e79-4e5d-949b-ae61be130bf9", "People", "Rock.Rest.Controllers.PeopleController" );
            RockMigrationHelper.AddSecurityAuthForRestAction( "8911b107-8e79-4e5d-949b-ae61be130bf9",
                0,
                Rock.Security.Authorization.VIEW,
                true,
                string.Empty,
                Rock.Model.SpecialRole.AllAuthenticatedUsers,
                "d011d9ed-7a18-4da8-8c2d-d091dea12f9d" );
        }

        /// <summary>
        /// BC: Removes access for all authenticated users to receive an impersonation token using this endpoint.
        /// Unfixes: https://app.asana.com/0/1122678768870900/1203305042901245/f
        /// </summary>
        private void GetCurrentPersonImpersonationSecurityDown()
        {
            RockMigrationHelper.DeleteSecurityAuth( "d011d9ed-7a18-4da8-8c2d-d091dea12f9d" );
        }
    }
}
