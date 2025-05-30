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
    /// This migration deprecates the ufnUtility_CsvToTable SQL table in favor of using the built-in STRING_SPLIT function that was added to SQL Server 2016.
    /// </summary>
    public partial class DeprecateCsvToTableFunction : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Update_ufnUtility_CsvToTable();
            Update_spFinance_ContributionStatementQuery();
            Update_spFinance_GivingAnalyticsQuery_AccountTotals();
            Update_spFinance_GivingAnalyticsQuery_PersonSummary();
            Update_spFinance_GivingAnalyticsQuery_TransactionData();
            Update_spCheckin_AttendanceAnalyticsQuery_AttendeeDates();
            Update_spCheckin_AttendanceAnalyticsQuery_AttendeeFirstDates();
            Update_spCheckin_AttendanceAnalyticsQuery_AttendeeLastAttendance();
            Update_spCheckin_AttendanceAnalyticsQuery_Attendees();
            Update_spCheckin_AttendanceAnalyticsQuery_NonAttendees();
        }

        private void Update_ufnUtility_CsvToTable()
        {
            Sql( @"
/*
<doc>
	<summary>

        *** THIS FUNCTION IS OBSOLETE.  PLEASE USE STRING_SPLIT(@YourString, ',') INSTEAD. ***

 		This function converts a comma-delimited string of values into a table of values
        The original version came from http://www.sqlservercentral.com/articles/Tally+Table/72993/
	</summary>
	<returns>
		* id
	</returns>
	<remarks>
        (Previously) Used by:
            * spFinance_ContributionStatementQuery
            * spFinance_GivingAnalyticsQuery_AccountTotals
            * spFinance_GivingAnalyticsQuery_PersonSummary
            * spFinance_GivingAnalyticsQuery_TransactionData
            * spCheckin_AttendanceAnalyticsQuery_AttendeeDates
            * spCheckin_AttendanceAnalyticsQuery_AttendeeFirstDates
            * spCheckin_AttendanceAnalyticsQuery_AttendeeLastAttendance
            * spCheckin_AttendanceAnalyticsQuery_Attendees
            * spCheckin_AttendanceAnalyticsQuery_NonAttendees
	</remarks>
	<code>
		SELECT * FROM [dbo].[ufnUtility_CsvToTable]('1,3,7,11,13') 
	</code>
</doc>
*/

/* #Obsolete# - Use STRING_SPLIT() instead */

ALTER FUNCTION [dbo].[ufnUtility_CsvToTable] 
(
 @pString VARCHAR(MAX)
)
RETURNS TABLE WITH SCHEMABINDING AS
RETURN
    SELECT Item = TRY_CONVERT(INT, TRIM(value))
    FROM STRING_SPLIT(@pString, ',');
" );
        }
        private void Update_spFinance_ContributionStatementQuery()
        {
            Sql( @"
/*
<doc>
	<summary>
		This stored procedure returns the Mailing Addresses and any CustomMessages for the Contribution Statement, but not the actual transactions
		The StatementGenerator utility uses this procedure along with querying transactions thru REST to generate statements
	</summary>

	<returns>
		* PersonId
		* GroupId
		* AddressPersonNames
		* Street1
		* Street2
		* City
		* State
		* PostalCode
		* StartDate
		* EndDate
		* CustomMessage1
		* CustomMessage2
	</returns>
	<param name='StartDate' datatype='datetime'>The starting date of the date range</param>
	<param name='EndDate' datatype='datetime'>The ending date of the date range</param>
	<param name='AccountIds' datatype='varchar(max)'>Comma delimited list of account ids. NULL means all</param>
	<param name='PersonId' datatype='int'>Person the statement if for. NULL means all persons that have transactions for the date range</param>
	<param name='OrderByPostalCode' datatype='int'>Set to 1 to have the results sorted by PostalCode, 0 for no particular order</param>
	<remarks>	
		Uses the following constants:
			* Group Type - Family: 790E3215-3B10-442B-AF69-616C0DCB998E
			* Group Role - Adult: 2639F9A5-2AAE-4E48-A8C3-4FFE86681E42
			* Group Role - Child: C8B1814F-6AA7-4055-B2D7-48FE20429CB9
	</remarks>
	<code>
		EXEC [dbo].[spFinance_ContributionStatementQuery] '01-01-2015', '01-01-2022', null, null, 0, 1 -- year 2015 statements for all persons that have a mailing address
        EXEC [dbo].[spFinance_ContributionStatementQuery] '01-01-2014', '01-01-2015', null, null, 1, 1 -- year 2014 statements for all persons regardless of mailing address
        EXEC [dbo].[spFinance_ContributionStatementQuery] '01-01-2014', '01-01-2015', null, 2, 1, 1  -- year 2014 statements for Ted Decker
	</code>
</doc>
*/


/* #Obsolete# - Statements can be gotten using the StatementGenerator, Statement Generator REST Endpoints */
ALTER PROCEDURE [dbo].[spFinance_ContributionStatementQuery]
	@StartDate datetime
	, @EndDate datetime
	, @AccountIds varchar(max) 
	, @PersonId int -- NULL means all persons
    , @IncludeIndividualsWithNoAddress bit 
	, @OrderByPostalCode bit
AS
BEGIN
	DECLARE @cGROUPTYPE_FAMILY uniqueidentifier = '790E3215-3B10-442B-AF69-616C0DCB998E'	
	DECLARE @cLOCATION_TYPE_HOME uniqueidentifier = '8C52E53C-2A66-435A-AE6E-5EE307D9A0DC'
	DECLARE @cLOCATION_TYPE_WORK uniqueidentifier = 'E071472A-F805-4FC4-917A-D5E3C095C35C'
	DECLARE @cTRANSACTION_TYPE_CONTRIBUTION uniqueidentifier = '2D607262-52D6-4724-910D-5C6E8FB89ACC'
	DECLARE @cRECORD_TYPE_BUSINESS uniqueidentifier = 'BF64ADD3-E70A-44CE-9C4B-E76BBED37550'
	DECLARE @transactionTypeContributionId int = (select top 1 Id from DefinedValue where [Guid] = @cTRANSACTION_TYPE_CONTRIBUTION)
	DECLARE @isBusiness bit = 0;
	DECLARE @recordTypeBusinessId int = (select top 1 Id from DefinedValue where [Guid]  = @cRECORD_TYPE_BUSINESS )

	if (@PersonId is not null) begin
	   if exists (select 1 from Person where Id = @PersonId and RecordTypeValueId = @recordTypeBusinessId)
	   begin
	      set @isBusiness = 1
	   end
	end

	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	;WITH tranListCTE
	AS
	(
		SELECT  
			[p].[GivingId]
		FROM 
			[FinancialTransaction] [ft]
		INNER JOIN 
			[FinancialTransactionDetail] [ftd] ON [ft].[Id] = [ftd].[TransactionId]
		INNER JOIN 
			[PersonAlias] [pa] ON [pa].[id] = [ft].[AuthorizedPersonAliasId]
		INNER JOIN 
			[Person] [p] ON [p].[id] = [pa].[PersonId]
		WHERE 
			([TransactionDateTime] >= @StartDate and [TransactionDateTime] < @EndDate)
        AND
			TransactionTypeValueId = @transactionTypeContributionId
		AND 
			(
				(@AccountIds is null)
				OR
				(ftd.[AccountId] in (SELECT value FROM STRING_SPLIT(@AccountIds, ',')))
			)
	)

	SELECT * FROM (
    SELECT 
		  [pg].[PersonId]
		, [pg].[GroupId]
		, [pg].[GroupSalutation] [AddressPersonNames]
        , case when l.Id is null then 0 else 1 end [HasAddress]
		, [l].[Street1]
		, [l].[Street2]
		, [l].[City]
		, [l].[State]
		, [l].[PostalCode]
		, @StartDate [StartDate]
		, DATEADD(DAY, -1, @EndDate) [EndDate] -- Subtract a day since this date is displayed to humans
		, null [CustomMessage1]
		, null [CustomMessage2]
	FROM (
		-- Get distinct Giving Groups for Persons that have a specific GivingGroupId and have transactions that match the filter
		-- These are Persons that give as part of a Group.  For example, Husband and Wife
		SELECT DISTINCT
			null [PersonId] 
			, [g].[Id] [GroupId]
            , [g].[GroupSalutation]
		FROM 
			[Person] [p]
		INNER JOIN 
			[Group] [g] ON [p].[GivingGroupId] = [g].[Id]
		WHERE 
		(
			(@personId is null) 
		OR 
			([p].[Id] = @personId)
		)
        AND
			[p].GivingId in (SELECT GivingId FROM tranListCTE)
		UNION
		-- Get Persons and their GroupId(s) that do not have GivingGroupId and have transactions that match the filter.        
		-- These are the persons that give as individuals vs as part of a group. We need the Groups (families they belong to) in order 
		-- to determine which address(es) the statements need to be mailed to 
		SELECT  
			[p].[Id] [PersonId],
			[g].[Id] [GroupId],
            [g].[GroupSalutation]
		FROM
			[Person] [p]
		JOIN 
			[GroupMember] [gm]
		ON 
			[gm].[PersonId] = [p].[Id]
		JOIN 
			[Group] [g]
		ON 
			[gm].[GroupId] = [g].[Id]
		WHERE
			[p].[GivingGroupId] is null
		AND
			[g].[GroupTypeId] = (SELECT Id FROM GroupType WHERE [Guid] = @cGROUPTYPE_FAMILY)
        AND
		(
			(@personId is null) 
		OR 
			([p].[Id] = @personId)
		)
		AND [p].GivingId IN (SELECT GivingId FROM tranListCTE)
	) [pg]
	LEFT OUTER JOIN (
    SELECT l.*, gl.GroupId from
		[GroupLocation] [gl] 
	LEFT OUTER JOIN
		[Location] [l]
	ON 
		[l].[Id] = [gl].[LocationId]
	WHERE 
		[gl].[IsMailingLocation] = 1
	AND
		([gl].[GroupLocationTypeValueId] = (SELECT Id FROM DefinedValue WHERE [Guid] = @cLOCATION_TYPE_HOME)
		 or 
		 (@isBusiness = 1 and [gl].[GroupLocationTypeValueId] = (SELECT Id FROM DefinedValue WHERE [Guid] = @cLOCATION_TYPE_WORK))
		)
        ) [l] 
        ON 
		[l].[GroupId] = [pg].[GroupId]
    ) n
    WHERE n.HasAddress = 1 or @IncludeIndividualsWithNoAddress = 1
    ORDER BY
	CASE WHEN @OrderByPostalCode = 1 THEN PostalCode END
	OPTION (RECOMPILE); -- specify option recompile to disable parameter sniffing since we would rather have to recalc the execution plan vs risk using a wrong execution plan
    
END
" );
        }
        private void Update_spFinance_GivingAnalyticsQuery_AccountTotals()
        {
            Sql( @"
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
" );
        }
        private void Update_spFinance_GivingAnalyticsQuery_PersonSummary()
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
	                INSERT INTO @AccountTbl SELECT value FROM STRING_SPLIT(@AccountIds,',')

	                DECLARE @CurrencyTypeTbl TABLE ( [Id] int )
	                INSERT INTO @CurrencyTypeTbl SELECT value FROM STRING_SPLIT(@CurrencyTypeIds,',')

	                DECLARE @SourceTypeTbl TABLE ( [Id] int )
	                INSERT INTO @SourceTypeTbl SELECT value FROM STRING_SPLIT(@SourceTypeIds,',')

	                DECLARE @TransactionTypeTbl TABLE ( [Id] int )
	                INSERT INTO @TransactionTypeTbl SELECT value FROM STRING_SPLIT(@TransactionTypeIds,',')

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

                END
" );
        }
        private void Update_spFinance_GivingAnalyticsQuery_TransactionData()
        {
            Sql( @"
ALTER PROCEDURE [dbo].[spFinance_GivingAnalyticsQuery_TransactionData]
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

                    END
" );

        }
        private void Update_spCheckin_AttendanceAnalyticsQuery_AttendeeDates()
        {
            Sql( @"
/*
<doc>
	<summary>
 		This function returns attendee person ids and the dates they attended based on selected filter criteria
	</summary>

	<returns>
		* PersonId
		* SundayDate
		* MonthDate
		* Year Date
	</returns>
	<param name='GroupTypeId' datatype='int'>The Check-in Area Group Type Id (only attendance for this are will be included</param>
	<param name='StartDate' datatype='datetime'>Beginning date range filter</param>
	<param name='EndDate' datatype='datetime'>Ending date range filter</param>
	<param name='GroupIds' datatype='varchar(max)'>Optional list of group ids to limit attendance to</param>
	<param name='CampusIds' datatype='varchar(max)'>Optional list of campus ids to limit attendance to</param>
	<param name='IncludeNullCampusIds' datatype='bit'>Flag indicating if attendance not tied to campus should be included</param>
	<remarks>	
	</remarks>
	<code>
		EXEC [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeDates] '24,25,26,27,28,29,30,31,32,56,57,58,59,111,112,113,114,115,116,117,118', '2019-09-17 00:00:00', '2019-10-12 00:00:00', null, 0, null
	</code>
</doc>
*/
ALTER PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeDates] @GroupIds VARCHAR(max)
	,@StartDate DATETIME = NULL
	,@EndDate DATETIME = NULL
	,@CampusIds VARCHAR(max) = NULL
	,@IncludeNullCampusIds BIT = 0
	,@ScheduleIds VARCHAR(max) = NULL
	WITH RECOMPILE
AS
BEGIN
	--  get the SundayDates within the StartDate and EndDate so we can query against AttendanceOccurrence.SundayDate
	DECLARE @startDateSundayDate DATE
	DECLARE @endDateSundayDate DATE

	SELECT @startDateSundayDate = x.StartSundayDate
		,@endDateSundayDate = x.EndSundayDate
	FROM dbo.ufnUtility_GetSundayDateRange(@StartDate, @EndDate) x

	DECLARE @CampusTbl TABLE ([Id] INT)

	INSERT INTO @CampusTbl
	SELECT value
	FROM STRING_SPLIT(@CampusIds, ',')

	DECLARE @ScheduleTbl TABLE ([Id] INT)

	INSERT INTO @ScheduleTbl
	SELECT value
	FROM STRING_SPLIT(@ScheduleIds, ',')

	DECLARE @GroupTbl TABLE ([Id] INT)

	INSERT INTO @GroupTbl
	SELECT value
	FROM STRING_SPLIT(@GroupIds, ',')

	-- Get all the attendance
	SELECT PA.[PersonId]
		,A.[SundayDate]
		,DATEADD(day, - (DATEPART(day, [SundayDate])) + 1, [SundayDate]) AS [MonthDate]
		,DATEADD(day, - (DATEPART(dayofyear, [SundayDate])) + 1, [SundayDate]) AS [YearDate]
	FROM (
		SELECT [PersonAliasId]
			,O.[GroupId]
			,[CampusId]
			,o.[SundayDate]
		FROM [Attendance] A
		INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
		INNER JOIN @GroupTbl [G] ON [G].[Id] = O.[GroupId]
		LEFT OUTER JOIN @CampusTbl [C] ON [C].[id] = [A].[CampusId]
		LEFT OUTER JOIN @ScheduleTbl [S] ON [S].[id] = [O].[ScheduleId]
		WHERE o.SundayDate BETWEEN @startDateSundayDate AND @endDateSundayDate AND A.[DidAttend] = 1 AND ((@CampusIds IS NULL OR [C].[Id] IS NOT NULL) OR (@IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL)) AND (@ScheduleIds IS NULL OR [S].[Id] IS NOT NULL)
		) A
	INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
END
" );

        }
        private void Update_spCheckin_AttendanceAnalyticsQuery_AttendeeFirstDates()
        {
            Sql( @"
/*
<doc>
	<summary>
        This function return people who attended based on selected filter criteria and the first 5 dates they ever attended the selected group type
	</summary>

	<returns>
		* PersonId
		* TimeAttending
		* SundayDate
	</returns>
	<param name='GroupTypeIds' datatype='varchar(max)'>The Group Type Ids (only attendance for these group types will be included</param>
	<param name='StartDate' datatype='datetime'>Beginning date range filter</param>
	<param name='EndDate' datatype='datetime'>Ending date range filter</param>
	<param name='GroupIds' datatype='varchar(max)'>Optional list of group ids to limit attendance to</param>
	<param name='CampusIds' datatype='varchar(max)'>Optional list of campus ids to limit attendance to</param>
	<param name='IncludeNullCampusIds' datatype='bit'>Flag indicating if attendance not tied to campus should be included</param>
	<remarks>	
	</remarks>
	<code>
        EXEC [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeFirstDates] '18,19,20,21,22,23,25', '24,25,26,27,28,29,30,31,32,56,57,58,59,111,112,113,114,115,116,117,118', '2019-09-17 00:00:00', '2019-10-23 00:00:00', null, 0, null
	</code>
</doc>
*/

ALTER PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeFirstDates]
	  @GroupTypeIds varchar(max)
	, @GroupIds varchar(max)
	, @StartDate datetime = NULL
	, @EndDate datetime = NULL
	, @CampusIds varchar(max) = NULL
	, @IncludeNullCampusIds bit = 0
	, @ScheduleIds varchar(max) = NULL
	WITH RECOMPILE

AS

BEGIN

    --  get the SundayDates within the StartDate and EndDate so we can query against AttendanceOccurrence.SundayDate
	DECLARE @startDateSundayDate DATE
	DECLARE @endDateSundayDate DATE

	SELECT @startDateSundayDate = x.StartSundayDate
		,@endDateSundayDate = x.EndSundayDate
	FROM dbo.ufnUtility_GetSundayDateRange(@StartDate, @EndDate) x

	DECLARE @CampusTbl TABLE ( [Id] int )
	INSERT INTO @CampusTbl SELECT value FROM STRING_SPLIT(@CampusIds,',')

	DECLARE @ScheduleTbl TABLE ( [Id] int )
	INSERT INTO @ScheduleTbl SELECT value FROM STRING_SPLIT(@ScheduleIds,',')

	DECLARE @GroupTbl TABLE ( [Id] int )
	INSERT INTO @GroupTbl SELECT value FROM STRING_SPLIT(@GroupIds,',')

	DECLARE @GroupTypeTbl TABLE ( [Id] int )
	INSERT INTO @GroupTypeTbl SELECT value FROM STRING_SPLIT(@GroupTypeIds,',')

	-- Get all the attendees
	DECLARE @PersonIdTbl TABLE ( [PersonId] INT NOT NULL )
	INSERT INTO @PersonIdTbl
	SELECT DISTINCT PA.[PersonId]
	FROM [Attendance] A
	INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
    INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
	INNER JOIN @GroupTbl [G] ON [G].[Id] = O.[GroupId]
	LEFT OUTER JOIN @CampusTbl [C] ON [C].[id] = [A].[CampusId]
	LEFT OUTER JOIN @ScheduleTbl [S] ON [S].[id] = [O].[ScheduleId]
    WHERE o.[SundayDate] BETWEEN @startDateSundayDate AND @endDateSundayDate
	AND [DidAttend] = 1
	AND ( 
		( @CampusIds IS NULL OR [C].[Id] IS NOT NULL ) OR  
		( @IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
	)
	AND ( @ScheduleIds IS NULL OR [S].[Id] IS NOT NULL  )

	-- Get the first 5 occasions on which each person attended any of the selected group types regardless of group or campus.
	-- Multiple attendances on the same date are considered as a single occasion.
	SELECT DISTINCT
	    [PersonId]
	    , [TimeAttending]
	    , [StartDate]
	FROM (
	    SELECT 
	        [P].[Id] AS [PersonId]
	        , DENSE_RANK() OVER ( PARTITION BY [P].[Id] ORDER BY [AO].[OccurrenceDate] ) AS [TimeAttending]
	        , [AO].[OccurrenceDate] AS [StartDate]
	    FROM
	        [Attendance] [A]
	        INNER JOIN [AttendanceOccurrence] [AO] ON [AO].[Id] = [A].[OccurrenceId]
	        INNER JOIN [Group] [G] ON [G].[Id] = [AO].[GroupId]
	        INNER JOIN [PersonAlias] [PA] ON [PA].[Id] = [A].[PersonAliasId] 
	        INNER JOIN [Person] [P] ON [P].[Id] = [PA].[PersonId]
	        INNER JOIN @GroupTypeTbl [GT] ON [GT].[id] = [G].[GroupTypeId]
	    WHERE 
	        [P].[Id] IN ( SELECT [PersonId] FROM @PersonIdTbl )
	        AND [DidAttend] = 1
	) [X]
    WHERE [X].[TimeAttending] <= 5

END
" );

        }
        private void Update_spCheckin_AttendanceAnalyticsQuery_AttendeeLastAttendance()
        {
            Sql( @"
/*
<doc>
	<summary>
        This function return people who attended based on selected filter criteria and the first 5 dates they ever attended the selected group type
	</summary>

	<returns>
		* PersonId
		* TimeAttending
		* SundayDate
	</returns>
	<param name='GroupTypeId' datatype='int'>The Check-in Area Group Type Id (only attendance for this are will be included</param>
	<param name='StartDate' datatype='datetime'>Beginning date range filter</param>
	<param name='EndDate' datatype='datetime'>Ending date range filter</param>
	<param name='GroupIds' datatype='varchar(max)'>Optional list of group ids to limit attendance to</param>
	<param name='CampusIds' datatype='varchar(max)'>Optional list of campus ids to limit attendance to</param>
	<param name='IncludeNullCampusIds' datatype='bit'>Flag indicating if attendance not tied to campus should be included</param>
	<remarks>	
	</remarks>
	<code>
		EXEC [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeLastAttendance] '24,25,26,27,28,29,30,31,32,56,57,58,59,111,112,113,114,115,116,117,118', '2019-09-17 00:00:00', '2019-10-23 00:00:00', null, 0, null
                                                                               
	</code>
</doc>
*/

ALTER PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeLastAttendance]
	  @GroupIds varchar(max) 
	, @StartDate datetime = NULL
	, @EndDate datetime = NULL
	, @CampusIds varchar(max) = NULL
	, @IncludeNullCampusIds bit = 0
	, @ScheduleIds varchar(max) = NULL

AS

BEGIN

    --  get the SundayDates within the StartDate and EndDate so we can query against AttendanceOccurrence.SundayDate
	DECLARE @startDateSundayDate DATE
	DECLARE @endDateSundayDate DATE

	SELECT @startDateSundayDate = x.StartSundayDate
		,@endDateSundayDate = x.EndSundayDate
	FROM dbo.ufnUtility_GetSundayDateRange(@StartDate, @EndDate) x

	DECLARE @CampusTbl TABLE ( [Id] int )
	INSERT INTO @CampusTbl SELECT value FROM STRING_SPLIT(@CampusIds,',')

	DECLARE @ScheduleTbl TABLE ( [Id] int )
	INSERT INTO @ScheduleTbl SELECT value FROM STRING_SPLIT(@ScheduleIds,',')

	DECLARE @GroupTbl TABLE ( [Id] int )
	INSERT INTO @GroupTbl SELECT value FROM STRING_SPLIT(@GroupIds,',')

	SELECT B.[PersonId], B.[CampusId], B.[CampusName], B.[GroupId], B.[GroupName], B.[ScheduleId], B.[StartDateTime], B.[LocationId], B.[RoleName], B.[LocationName] 
	FROM
	(
		SELECT PA.[PersonId], ROW_NUMBER() OVER (PARTITION BY PA.[PersonId] ORDER BY A.[StartDateTime] DESC) AS PersonRowNumber,
			A.[CampusId], CA.[Name] AS [CampusName], O.[GroupId], G.[Name] AS [GroupName], O.[ScheduleId], A.[StartDateTime], O.[LocationId], R.[RoleName], L.[Name] AS [LocationName]
		FROM [Attendance] A
		INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
		INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
		INNER JOIN [Group] G ON G.[Id] = O.[GroupId]
		INNER JOIN @GroupTbl [G2] ON [G2].[Id] = G.[Id]
		OUTER APPLY (
			SELECT TOP 1 R.[Name] AS [RoleName]
			FROM [GroupMember] M 
			INNER JOIN [GroupTypeRole] R
				ON R.[Id] = M.[GroupRoleId]
			WHERE M.[GroupId] = G.[Id]
			AND M.[PersonId] = PA.[PersonId]
			AND M.[GroupMemberStatus] <> 0
			ORDER BY R.[Order]
		) R
		LEFT OUTER JOIN [Location] L
			ON L.[Id] = O.[LocationId]
		LEFT OUTER JOIN @CampusTbl [C] ON [C].[id] = [A].[CampusId]
		LEFT OUTER JOIN @ScheduleTbl [S] ON [S].[id] = [O].[ScheduleId]
		LEFT OUTER JOIN [Campus] [CA] ON [A].[CampusId] = [CA].[Id]
		WHERE o.[SundayDate] BETWEEN @startDateSundayDate AND @endDateSundayDate
		AND [DidAttend] = 1
		AND ( 
			( @CampusIds IS NULL OR [C].[Id] IS NOT NULL ) OR  
			( @IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
		)
		AND ( @ScheduleIds IS NULL OR [S].[Id] IS NOT NULL  )
	) B
	WHERE B.PersonRowNumber = 1

END
" );

        }
        private void Update_spCheckin_AttendanceAnalyticsQuery_Attendees()
        {
            Sql( @"
/*
<doc>
	<summary>
 		This function returns the people that attended based on selected filter criteria
	</summary>

	<returns>
		* Id 
		* NickName
		* LastName
	</returns>
	<param name='GroupTypeId' datatype='int'>The Check-in Area Group Type Id (only attendance for this are will be included</param>
	<param name='StartDate' datatype='datetime'>Beginning date range filter</param>
	<param name='EndDate' datatype='datetime'>Ending date range filter</param>
	<param name='GroupIds' datatype='varchar(max)'>Optional list of group ids to limit attendance to</param>
	<param name='CampusIds' datatype='varchar(max)'>Optional list of campus ids to limit attendance to</param>
	<param name='IncludeNullCampusIds' datatype='bit'>Flag indicating if attendance not tied to campus should be included</param>
	<remarks>	
	</remarks>
	<code>
        EXEC [dbo].[spCheckin_AttendanceAnalyticsQuery_Attendees] '24,25,26,27,28,29,30,31,32,56,57,58,59,111,112,113,114,115,116,117,118', '2019-09-17 00:00:00', '2019-10-23 00:00:00', null, 0, 0, 0, null
	</code>
</doc>
*/

ALTER PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_Attendees]
	  @GroupIds varchar(max)
	, @StartDate datetime = NULL
	, @EndDate datetime = NULL
	, @CampusIds varchar(max) = NULL
	, @IncludeNullCampusIds bit = 0
    , @IncludeParentsWithChild bit = 0
    , @IncludeChildrenWithParents bit = 0
	, @ScheduleIds varchar(max) = NULL
	WITH RECOMPILE

AS

BEGIN

    --  get the SundayDates within the StartDate and EndDate so we can query against AttendanceOccurrence.SundayDate
	DECLARE @startDateSundayDate DATE
	DECLARE @endDateSundayDate DATE

	SELECT @startDateSundayDate = x.StartSundayDate
		,@endDateSundayDate = x.EndSundayDate
	FROM dbo.ufnUtility_GetSundayDateRange(@StartDate, @EndDate) x

	DECLARE @CampusTbl TABLE ( [Id] int )
	INSERT INTO @CampusTbl SELECT value FROM STRING_SPLIT(@CampusIds,',')

	DECLARE @ScheduleTbl TABLE ( [Id] int )
	INSERT INTO @ScheduleTbl SELECT value FROM STRING_SPLIT(@ScheduleIds,',')

	DECLARE @GroupTbl TABLE ( [Id] int )
	INSERT INTO @GroupTbl SELECT value FROM STRING_SPLIT(@GroupIds,',')

	-- Get all the attendees
    IF @IncludeChildrenWithParents = 0 AND @IncludeParentsWithChild = 0
    BEGIN

        -- Just return the person who attended
	    SELECT	
		    P.[Id],
		    P.[NickName],
		    P.[LastName],
			P.[Gender],
		    P.[Email],
            P.[GivingId],
		    P.[BirthDate],
            P.[ConnectionStatusValueId],
			P.[GraduationYear],
			P.[DeceasedDate]
	    FROM (
		    SELECT DISTINCT PA.[PersonId]
			FROM (
				SELECT 
					A.[PersonAliasId],
					A.[CampusId],
					O.[ScheduleId]
 				FROM [Attendance] A
				INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
				INNER JOIN @GroupTbl G ON G.[Id] = O.[GroupId]
				WHERE o.[SundayDate] BETWEEN @startDateSundayDate AND @endDateSundayDate
				AND [DidAttend] = 1
			) A
            INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
			LEFT OUTER JOIN @CampusTbl [C] ON [C].[id] = [A].[CampusId]
			LEFT OUTER JOIN @ScheduleTbl [S] ON [S].[id] = [A].[ScheduleId]
		    WHERE ( 
			    ( @CampusIds IS NULL OR [C].[Id] IS NOT NULL ) OR  
			    ( @IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
		    )
		    AND ( @ScheduleIds IS NULL OR [S].[Id] IS NOT NULL  )
	    ) [Attendee]
	    INNER JOIN [Person] P 
			ON P.[Id] = [Attendee].[PersonId]

    END
    ELSE
    BEGIN

        DECLARE @AdultRoleId INT = ( SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42' )
        DECLARE @ChildRoleId INT = ( SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9' )

        IF @IncludeParentsWithChild = 1 
   BEGIN

            -- Child attended, also include their parents
	        SELECT	
                C.[Id],
                C.[NickName],
                C.[LastName],
				C.[Gender],
                C.[Email],
                C.[GivingId],
                C.[BirthDate],
                C.[ConnectionStatusValueId],
				C.[GraduationYear],
				C.[DeceasedDate],
                A.[Id] AS [ParentId],
                A.[NickName] AS [ParentNickName],
                A.[LastName] AS [ParentLastName],
                A.[Email] AS [ParentEmail],
                A.[GivingId] as [ParentGivingId],
                A.[BirthDate] AS [ParentBirthDate],
				A.[Gender] AS [ParentGender],
				A.[DeceasedDate] AS [ParentDeceasedDate]
	        FROM (
				SELECT DISTINCT PA.[PersonId]
				FROM (
					SELECT 
						A.[PersonAliasId],
						A.[CampusId],
						O.[ScheduleId]
 					FROM [Attendance] A
					INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
					INNER JOIN @GroupTbl G ON G.[Id] = O.[GroupId]
					WHERE o.[SundayDate] BETWEEN @startDateSundayDate AND @endDateSundayDate
					AND [DidAttend] = 1
				) A
				INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
				LEFT OUTER JOIN @CampusTbl [C] ON [C].[id] = [A].[CampusId]
				LEFT OUTER JOIN @ScheduleTbl [S] ON [S].[id] = [A].[ScheduleId]
				WHERE ( 
					( @CampusIds IS NULL OR [C].[Id] IS NOT NULL ) OR  
					( @IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
				)
				AND ( @ScheduleIds IS NULL OR [S].[Id] IS NOT NULL  )
	        ) [Attendee]
	        INNER JOIN [Person] C 
				ON C.[Id] = [Attendee].[PersonId]
            INNER JOIN [GroupMember] GMC
                ON GMC.[PersonId] = C.[Id]
	            AND GMC.[GroupRoleId] = @ChildRoleId
            INNER JOIN [GroupMember] GMA
	            ON GMA.[GroupId] = GMC.[GroupId]
	            AND GMA.[GroupRoleId] = @AdultRoleId
            INNER JOIN [Person] A 
				ON A.[Id] = GMA.[PersonId]

        END
        
        IF @IncludeChildrenWithParents = 1
        BEGIN

            -- Parents attended, include their children
	        SELECT	
                A.[Id],
                A.[NickName],
                A.[LastName],
				A.[Gender],
                A.[Email],
                A.[GivingId],
                A.[BirthDate],
                A.[ConnectionStatusValueId],
				A.[DeceasedDate],
                C.[Id] AS [ChildId],
                C.[NickName] AS [ChildNickName],
                C.[LastName] AS [ChildLastName],
                C.[Email] AS [ChildEmail],
                C.[GivingId] as [ChildGivingId],
                C.[BirthDate] AS [ChildBirthDate],
				C.[Gender] as [ChildGender],
				C.[GraduationYear] as [ChildGraduationYear],
				C.[DeceasedDate] AS [ChildDeceasedDate]
	        FROM (
				SELECT DISTINCT PA.[PersonId]
				FROM (
					SELECT 
						A.[PersonAliasId],
						A.[CampusId],
						O.[ScheduleId]
 					FROM [Attendance] A
					INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
					INNER JOIN @GroupTbl G ON G.[Id] = O.[GroupId]
					WHERE o.[SundayDate] BETWEEN @startDateSundayDate AND @endDateSundayDate
					AND [DidAttend] = 1
				) A
				INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
				LEFT OUTER JOIN @CampusTbl [C] ON [C].[id] = [A].[CampusId]
				LEFT OUTER JOIN @ScheduleTbl [S] ON [S].[id] = [A].[ScheduleId]
				WHERE ( 
					( @CampusIds IS NULL OR [C].[Id] IS NOT NULL ) OR  
					( @IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
				)
				AND ( @ScheduleIds IS NULL OR [S].[Id] IS NOT NULL  )
	        ) [Attendee]
	        INNER JOIN [Person] A 
				ON A.[Id] = [Attendee].[PersonId]
            INNER JOIN [GroupMember] GMA
                ON GMA.[PersonId] = A.[Id]
	            AND GMA.[GroupRoleId] = @AdultRoleId
            INNER JOIN [GroupMember] GMC
	            ON GMC.[GroupId] = GMA.[GroupId]
	            AND GMC.[GroupRoleId] = @ChildRoleId
            INNER JOIN [Person] C 
				ON C.[Id] = GMC.[PersonId]

        END

    END

END
" );
        }
        private void Update_spCheckin_AttendanceAnalyticsQuery_NonAttendees()
        {
            Sql( @"
/*
<doc>
    <summary>
         This function returns any person ids for people that have attended previously but who have not attended since the beginning date
    </summary>

    <returns>
        * PersonId 
        * SundayDate - Last time attended
    </returns>
    <param name='GroupTypeIds' datatype='varchar(max)'>The Group Type Ids (only attendance for these group types will be included</param>
    <param name='StartDateTime' datatype='datetime'>Beginning date range filter</param>
    <param name='GroupIds' datatype='varchar(max)'>Optional list of group ids to limit attendance to</param>
    <param name='CampusIds' datatype='varchar(max)'>Optional list of campus ids to limit attendance to</param>
    <param name='IncludeNullCampusIds' datatype='bit'>Flag indicating if attendance not tied to campus should be included</param>
    <remarks>    
    </remarks>
    <code>
        EXEC [dbo].[spCheckin_AttendanceAnalyticsQuery_NonAttendees] '18,19,20,21,22,23,25', '24,25,26,27,28,29,30,31,32,56,57,58,59,111,112,113,114,115,116,117,118', '2019-09-17 00:00:00', '2019-10-23 00:00:00', null, 0, null, 0, 0
    </code>
</doc>
*/

ALTER PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_NonAttendees]
      @GroupTypeIds varchar(max)
    , @GroupIds varchar(max)
    , @StartDate datetime = NULL
    , @EndDate datetime = NULL
    , @CampusIds varchar(max) = NULL
    , @IncludeNullCampusIds bit = 0
    , @ScheduleIds varchar(max) = NULL
    , @IncludeParentsWithChild bit = 0
    , @IncludeChildrenWithParents bit = 0
    WITH RECOMPILE

AS

BEGIN

   	--  get the SundayDates within the StartDate and EndDate so we can query against AttendanceOccurrence.SundayDate
	DECLARE @startDateSundayDate DATE
	DECLARE @endDateSundayDate DATE

	SELECT @startDateSundayDate = x.StartSundayDate
		,@endDateSundayDate = x.EndSundayDate
	FROM dbo.ufnUtility_GetSundayDateRange(@StartDate, @EndDate) x

    DECLARE @PersonIdTbl TABLE ( [Id] INT NOT NULL )

    DECLARE @CampusTbl TABLE ( [Id] int )
    INSERT INTO @CampusTbl SELECT value FROM STRING_SPLIT(@CampusIds,',')

    DECLARE @ScheduleTbl TABLE ( [Id] int )
    INSERT INTO @ScheduleTbl SELECT value FROM STRING_SPLIT(@ScheduleIds,',')

    DECLARE @GroupTbl TABLE ( [Id] int )
    INSERT INTO @GroupTbl SELECT value FROM STRING_SPLIT(@GroupIds,',')

    DECLARE @GroupTypeTbl TABLE ( [Id] int )
    INSERT INTO @GroupTypeTbl SELECT value FROM STRING_SPLIT(@GroupTypeIds,',')

    -- Find all the person ids for people who belong to any of the selected groups and have not attended any group/campus of selected group type
    INSERT INTO @PersonIdTbl
    SELECT DISTINCT M.[PersonId]
    FROM @GroupTbl G
    INNER JOIN [GroupMember] M
        ON M.[GroupId] = G.[Id]
        AND M.[GroupMemberStatus] = 1
    WHERE M.[PersonId] NOT IN (
        SELECT DISTINCT PA.[PersonId]
        FROM (
            SELECT 
                A.[PersonAliasId],
                A.[CampusId],
                O.[ScheduleId]
             FROM [Attendance] A
            INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
            INNER JOIN @GroupTbl G ON G.[Id] = O.[GroupId]
            WHERE o.[SundayDate] BETWEEN @startDateSundayDate AND @endDateSundayDate
            AND [DidAttend] = 1
        ) A
        INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
        LEFT OUTER JOIN @CampusTbl [C] ON [C].[id] = [A].[CampusId]
        LEFT OUTER JOIN @ScheduleTbl [S] ON [S].[id] = [A].[ScheduleId]
        WHERE ( 
            ( @CampusIds IS NULL OR [C].[Id] IS NOT NULL ) OR  
            ( @IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
        )
        AND ( @ScheduleIds IS NULL OR [S].[Id] IS NOT NULL  )
    )

    IF @IncludeChildrenWithParents = 0 AND @IncludeParentsWithChild = 0
    BEGIN

        -- Just return the person 
 SELECT    
            P.[Id],
            P.[NickName],
            P.[LastName],
            P.[Gender],
            P.[Email],
            P.[GivingId],
            P.[BirthDate],
            P.[ConnectionStatusValueId],
			P.[DeceasedDate],
			P.[GraduationYear]
        FROM @PersonIdTbl M
        INNER JOIN [Person] P ON P.[Id] = M.[Id]

    END
    ELSE
    BEGIN

        DECLARE @AdultRoleId INT = ( SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42' )
        DECLARE @ChildRoleId INT = ( SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9' )

        IF @IncludeParentsWithChild = 1 
        BEGIN

            -- Child attended, also include their parents
            SELECT    
                C.[Id],
                C.[NickName],
                C.[LastName],
                C.[Gender],
                C.[Email],
                C.[GivingId],
                C.[BirthDate],
                C.[ConnectionStatusValueId],
				C.[DeceasedDate],
				C.[GraduationYear],
                A.[Id] AS [ParentId],
                A.[NickName] AS [ParentNickName],
                A.[LastName] AS [ParentLastName],
                A.[Email] AS [ParentEmail],
                A.[GivingId] as [ParentGivingId],
                A.[BirthDate] AS [ParentBirthDate],
				A.[DeceasedDate] AS [ParentDeceasedDate],
				A.[Gender] AS [ParentGender]
            FROM @PersonIdTbl M
            INNER JOIN [Person] C 
                ON C.[Id] = M.[Id]
            INNER JOIN [GroupMember] GMC
                ON GMC.[PersonId] = C.[Id]
                AND GMC.[GroupRoleId] = @ChildRoleId
            INNER JOIN [GroupMember] GMA
                ON GMA.[GroupId] = GMC.[GroupId]
                AND GMA.[GroupRoleId] = @AdultRoleId
            INNER JOIN [Person] A 
                ON A.[Id] = GMA.[PersonId]

        END
        
        IF @IncludeChildrenWithParents = 1
        BEGIN

            -- Parents attended, include their children
            SELECT    
                A.[Id],
                A.[NickName],
                A.[LastName],
                A.[Gender],
                A.[Email],
                A.[GivingId] as [GivingId],
                A.[BirthDate],
                A.[ConnectionStatusValueId],
				A.[DeceasedDate],
                C.[Id] AS [ChildId],
                C.[NickName] AS [ChildNickName],
                C.[LastName] AS [ChildLastName],
                C.[Email] AS [ChildEmail],
                C.[GivingId] as [ChildGivingId],
                C.[BirthDate] AS [ChildBirthDate],
				C.[Gender] as [ChildGender],
				C.[GraduationYear] as [ChildGraduationYear],
				C.[DeceasedDate] AS [ChildDeceasedDate]
            FROM @PersonIdTbl M
            INNER JOIN [Person] A 
                ON A.[Id] = M.[Id]
            INNER JOIN [GroupMember] GMA
                ON GMA.[PersonId] = A.[Id]
                AND GMA.[GroupRoleId] = @AdultRoleId
            INNER JOIN [GroupMember] GMC
                ON GMC.[GroupId] = GMA.[GroupId]
                AND GMC.[GroupRoleId] = @ChildRoleId
            INNER JOIN [Person] C 
                ON C.[Id] = GMC.[PersonId]

        END

    END

    -- Get the first 5 times they attended this group type (any group or campus)
    SELECT 
        P.[Id] AS [PersonId],
        D.[TimeAttending],
        D.[StartDate]
    FROM @PersonIdTbl P
    CROSS APPLY ( 
        SELECT TOP 5 
            ROW_NUMBER() OVER (ORDER BY [StartDate]) AS [TimeAttending],
            [StartDate]
        FROM (
            SELECT DISTINCT [StartDate]
            FROM [vCheckin_GroupTypeAttendance] A
            INNER JOIN @GroupTypeTbl [G] ON [G].[id] = [A].[GroupTypeId]
            AND A.[PersonId] = P.[Id]
        ) S
    ) D

    -- Get the last time they attended
    SELECT 
        PD.[PersonId],
        PD.[CampusId],
		CA.[Name] AS [CampusName],
        PD.[GroupId],
        PD.[GroupName],
        PD.[ScheduleId],
        PD.[StartDateTime],
        PD.[LocationId],
   R.[Name] AS [RoleName],
        L.[Name] AS [LocationName]
    FROM (
        SELECT 
            P.[Id] AS [PersonId],
            A.[CampusId],
            A.[GroupId],
  A.[GroupName],
            A.[ScheduleId],
            A.[StartDateTime],
            A.[LocationId]
        FROM @PersonIdTbl P
        CROSS APPLY (
            SELECT TOP 1 
                A.[CampusId],
                A.[GroupId],
                G.[Name] AS [GroupName],
                A.[ScheduleId],
                A.[LocationId],
                A.[StartDateTime]
            FROM (
                SELECT 
                    A.[PersonAliasId],
                    A.[CampusId],
                    O.[GroupId],
                    O.[ScheduleId],
                    O.[LocationId],
                    A.[StartDateTime]
                 FROM [Attendance] A
                INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
                INNER JOIN @GroupTbl G ON G.[Id] = O.[GroupId]
                WHERE o.[SundayDate] < @startDateSundayDate
                AND [DidAttend] = 1
            ) A
            INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId] 
            INNER JOIN [Group] G ON G.[Id] = A.[GroupId]
            LEFT OUTER JOIN @CampusTbl [C] ON [C].[id] = [A].[CampusId]
            LEFT OUTER JOIN @ScheduleTbl [S] ON [S].[id] = [A].[ScheduleId]
            WHERE PA.[PersonId] = P.[Id]
            AND ( 
                ( @CampusIds IS NULL OR [C].[Id] IS NOT NULL ) OR  
                ( @IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
            )
            AND ( @ScheduleIds IS NULL OR [S].[Id] IS NOT NULL  )
            ORDER BY A.[StartDateTime] DESC
        ) A
    ) PD
    OUTER APPLY (
        SELECT TOP 1 R.[Name]
        FROM [GroupMember] M 
        INNER JOIN [GroupTypeRole] R
            ON R.[Id] = M.[GroupRoleId]
        WHERE M.[GroupId] = PD.[GroupId]
        AND M.[PersonId] = PD.[PersonId]
        AND M.[GroupMemberStatus] <> 0
        ORDER BY R.[Order]
    ) R
    LEFT OUTER JOIN [Location] L
        ON L.[Id] = PD.[LocationId]
	LEFT OUTER JOIN [Campus] CA ON PD.[CampusId] = CA.[Id]
END
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RestoreOriginal_ufnUtility_CsvToTable();
            RestoreOriginal_spFinance_ContributionStatementQuery();
            RestoreOriginal_spFinance_GivingAnalyticsQuery_AccountTotals();
            RestoreOriginal_spFinance_GivingAnalyticsQuery_PersonSummary();
            RestoreOriginal_spFinance_GivingAnalyticsQuery_TransactionData();
            RestoreOriginal_spCheckin_AttendanceAnalyticsQuery_AttendeeDates();
            RestoreOriginal_spCheckin_AttendanceAnalyticsQuery_AttendeeFirstDates();
            RestoreOriginal_spCheckin_AttendanceAnalyticsQuery_AttendeeLastAttendance();
            RestoreOriginal_spCheckin_AttendanceAnalyticsQuery_Attendees();
            RestoreOriginal_spCheckin_AttendanceAnalyticsQuery_NonAttendees();
        }

        private void RestoreOriginal_ufnUtility_CsvToTable()
        {
            Sql( @"
/*
<doc>
	<summary>
 		This function converts a comma-delimited string of values into a table of values
        It comes from http://www.sqlservercentral.com/articles/Tally+Table/72993/
	</summary>
	<returns>
		* id
	</returns>
	<remarks>
		Used by spFinance_ContributionStatementQuery
	</remarks>
	<code>
		SELECT * FROM [dbo].[ufnUtility_CsvToTable]('1,3,7,11,13') 
	</code>
</doc>
*/
ALTER FUNCTION [dbo].[ufnUtility_CsvToTable] 
(
	@pString VARCHAR(8000)
)
RETURNS TABLE WITH SCHEMABINDING AS
RETURN
--Produce values from 1 up to 10,000. Enough to cover VARCHAR(8000).
WITH E1(N) AS ( --Create 10 rows
    SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL
    SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL
    SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1
),
E2(N) AS ( --10E+2 for 100 rows
	SELECT 1 
	FROM E1 a, E1 b
),   
E4(N) AS ( --10E+4 for 10,000 rows
	SELECT 1 
	FROM E2 a, E2 b
),
cteTally(N) AS (--==== This provides the ""base"" CTE and limits the number of rows right up front
                    -- for both a performance gain and prevention of accidental ""overruns""
	SELECT TOP (ISNULL(DATALENGTH(@pString),0)) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) 
	FROM E4
),
cteStart(N1) AS (--==== This returns N+1 (starting position of each ""element"" just once for each delimiter)
	SELECT 1 
	UNION ALL
	SELECT t.N+1 
	FROM cteTally t WHERE SUBSTRING(@pString,t.N,1) = ','
),
cteLen(N1,L1) AS(--==== Return start and length (for use in substring)
	SELECT s.N1, 
		ISNULL(NULLIF(CHARINDEX(',',@pString,s.N1),0)-s.N1,8000)
	FROM cteStart s
)
--===== Do the actual split. The ISNULL/NULLIF combo handles the length for the final element when no delimiter is found.
SELECT Item =  try_convert(int, SUBSTRING(@pString, l.N1, l.L1))
FROM cteLen l
" );
        }
        private void RestoreOriginal_spFinance_ContributionStatementQuery()
        {
            Sql( @"
/*
<doc>
	<summary>
		This stored procedure returns the Mailing Addresses and any CustomMessages for the Contribution Statement, but not the actual transactions
		The StatementGenerator utility uses this procedure along with querying transactions thru REST to generate statements
	</summary>

	<returns>
		* PersonId
		* GroupId
		* AddressPersonNames
		* Street1
		* Street2
		* City
		* State
		* PostalCode
		* StartDate
		* EndDate
		* CustomMessage1
		* CustomMessage2
	</returns>
	<param name='StartDate' datatype='datetime'>The starting date of the date range</param>
	<param name='EndDate' datatype='datetime'>The ending date of the date range</param>
	<param name='AccountIds' datatype='varchar(max)'>Comma delimited list of account ids. NULL means all</param>
	<param name='PersonId' datatype='int'>Person the statement if for. NULL means all persons that have transactions for the date range</param>
	<param name='OrderByPostalCode' datatype='int'>Set to 1 to have the results sorted by PostalCode, 0 for no particular order</param>
	<remarks>	
		Uses the following constants:
			* Group Type - Family: 790E3215-3B10-442B-AF69-616C0DCB998E
			* Group Role - Adult: 2639F9A5-2AAE-4E48-A8C3-4FFE86681E42
			* Group Role - Child: C8B1814F-6AA7-4055-B2D7-48FE20429CB9
	</remarks>
	<code>
		EXEC [dbo].[spFinance_ContributionStatementQuery] '01-01-2015', '01-01-2022', null, null, 0, 1 -- year 2015 statements for all persons that have a mailing address
        EXEC [dbo].[spFinance_ContributionStatementQuery] '01-01-2014', '01-01-2015', null, null, 1, 1 -- year 2014 statements for all persons regardless of mailing address
        EXEC [dbo].[spFinance_ContributionStatementQuery] '01-01-2014', '01-01-2015', null, 2, 1, 1  -- year 2014 statements for Ted Decker
	</code>
</doc>
*/


/* #Obsolete# - Statements can be gotten using the StatementGenerator, Statement Generator REST Endpoints */
ALTER PROCEDURE [dbo].[spFinance_ContributionStatementQuery]
	@StartDate datetime
	, @EndDate datetime
	, @AccountIds varchar(max) 
	, @PersonId int -- NULL means all persons
    , @IncludeIndividualsWithNoAddress bit 
	, @OrderByPostalCode bit
AS
BEGIN
	DECLARE @cGROUPTYPE_FAMILY uniqueidentifier = '790E3215-3B10-442B-AF69-616C0DCB998E'	
	DECLARE @cLOCATION_TYPE_HOME uniqueidentifier = '8C52E53C-2A66-435A-AE6E-5EE307D9A0DC'
	DECLARE @cLOCATION_TYPE_WORK uniqueidentifier = 'E071472A-F805-4FC4-917A-D5E3C095C35C'
	DECLARE @cTRANSACTION_TYPE_CONTRIBUTION uniqueidentifier = '2D607262-52D6-4724-910D-5C6E8FB89ACC'
	DECLARE @cRECORD_TYPE_BUSINESS uniqueidentifier = 'BF64ADD3-E70A-44CE-9C4B-E76BBED37550'
	DECLARE @transactionTypeContributionId int = (select top 1 Id from DefinedValue where [Guid] = @cTRANSACTION_TYPE_CONTRIBUTION)
	DECLARE @isBusiness bit = 0;
	DECLARE @recordTypeBusinessId int = (select top 1 Id from DefinedValue where [Guid]  = @cRECORD_TYPE_BUSINESS )

	if (@PersonId is not null) begin
	   if exists (select 1 from Person where Id = @PersonId and RecordTypeValueId = @recordTypeBusinessId)
	   begin
	      set @isBusiness = 1
	   end
	end

	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	;WITH tranListCTE
	AS
	(
		SELECT  
			[p].[GivingId]
		FROM 
			[FinancialTransaction] [ft]
		INNER JOIN 
			[FinancialTransactionDetail] [ftd] ON [ft].[Id] = [ftd].[TransactionId]
		INNER JOIN 
			[PersonAlias] [pa] ON [pa].[id] = [ft].[AuthorizedPersonAliasId]
		INNER JOIN 
			[Person] [p] ON [p].[id] = [pa].[PersonId]
		WHERE 
			([TransactionDateTime] >= @StartDate and [TransactionDateTime] < @EndDate)
        AND
			TransactionTypeValueId = @transactionTypeContributionId
		AND 
			(
				(@AccountIds is null)
				OR
				(ftd.[AccountId] in (select * from ufnUtility_CsvToTable(@AccountIds)))
			)
	)

	SELECT * FROM (
    SELECT 
		  [pg].[PersonId]
		, [pg].[GroupId]
		, [pg].[GroupSalutation] [AddressPersonNames]
        , case when l.Id is null then 0 else 1 end [HasAddress]
		, [l].[Street1]
		, [l].[Street2]
		, [l].[City]
		, [l].[State]
		, [l].[PostalCode]
		, @StartDate [StartDate]
		, DATEADD(DAY, -1, @EndDate) [EndDate] -- Subtract a day since this date is displayed to humans
		, null [CustomMessage1]
		, null [CustomMessage2]
	FROM (
		-- Get distinct Giving Groups for Persons that have a specific GivingGroupId and have transactions that match the filter
		-- These are Persons that give as part of a Group.  For example, Husband and Wife
		SELECT DISTINCT
			null [PersonId] 
			, [g].[Id] [GroupId]
            , [g].[GroupSalutation]
		FROM 
			[Person] [p]
		INNER JOIN 
			[Group] [g] ON [p].[GivingGroupId] = [g].[Id]
		WHERE 
		(
			(@personId is null) 
		OR 
			([p].[Id] = @personId)
		)
        AND
			[p].GivingId in (SELECT GivingId FROM tranListCTE)
		UNION
		-- Get Persons and their GroupId(s) that do not have GivingGroupId and have transactions that match the filter.        
		-- These are the persons that give as individuals vs as part of a group. We need the Groups (families they belong to) in order 
		-- to determine which address(es) the statements need to be mailed to 
		SELECT  
			[p].[Id] [PersonId],
			[g].[Id] [GroupId],
            [g].[GroupSalutation]
		FROM
			[Person] [p]
		JOIN 
			[GroupMember] [gm]
		ON 
			[gm].[PersonId] = [p].[Id]
		JOIN 
			[Group] [g]
		ON 
			[gm].[GroupId] = [g].[Id]
		WHERE
			[p].[GivingGroupId] is null
		AND
			[g].[GroupTypeId] = (SELECT Id FROM GroupType WHERE [Guid] = @cGROUPTYPE_FAMILY)
        AND
		(
			(@personId is null) 
		OR 
			([p].[Id] = @personId)
		)
		AND [p].GivingId IN (SELECT GivingId FROM tranListCTE)
	) [pg]
	LEFT OUTER JOIN (
    SELECT l.*, gl.GroupId from
		[GroupLocation] [gl] 
	LEFT OUTER JOIN
		[Location] [l]
	ON 
		[l].[Id] = [gl].[LocationId]
	WHERE 
		[gl].[IsMailingLocation] = 1
	AND
		([gl].[GroupLocationTypeValueId] = (SELECT Id FROM DefinedValue WHERE [Guid] = @cLOCATION_TYPE_HOME)
		 or 
		 (@isBusiness = 1 and [gl].[GroupLocationTypeValueId] = (SELECT Id FROM DefinedValue WHERE [Guid] = @cLOCATION_TYPE_WORK))
		)
        ) [l] 
        ON 
		[l].[GroupId] = [pg].[GroupId]
    ) n
    WHERE n.HasAddress = 1 or @IncludeIndividualsWithNoAddress = 1
    ORDER BY
	CASE WHEN @OrderByPostalCode = 1 THEN PostalCode END
	OPTION (RECOMPILE); -- specify option recompile to disable parameter sniffing since we would rather have to recalc the execution plan vs risk using a wrong execution plan
    
END
" );
        }
        private void RestoreOriginal_spFinance_GivingAnalyticsQuery_AccountTotals()
        {
            Sql( @"
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

                    END
" );

        }
        private void RestoreOriginal_spFinance_GivingAnalyticsQuery_PersonSummary()
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

                END
" );
        }
        private void RestoreOriginal_spFinance_GivingAnalyticsQuery_TransactionData()
        {
            Sql( @"
ALTER PROCEDURE [dbo].[spFinance_GivingAnalyticsQuery_TransactionData]
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

                    END
" );

        }
        private void RestoreOriginal_spCheckin_AttendanceAnalyticsQuery_AttendeeDates()
        {
            Sql( @"
/*
<doc>
	<summary>
 		This function returns attendee person ids and the dates they attended based on selected filter criteria
	</summary>

	<returns>
		* PersonId
		* SundayDate
		* MonthDate
		* Year Date
	</returns>
	<param name='GroupTypeId' datatype='int'>The Check-in Area Group Type Id (only attendance for this are will be included</param>
	<param name='StartDate' datatype='datetime'>Beginning date range filter</param>
	<param name='EndDate' datatype='datetime'>Ending date range filter</param>
	<param name='GroupIds' datatype='varchar(max)'>Optional list of group ids to limit attendance to</param>
	<param name='CampusIds' datatype='varchar(max)'>Optional list of campus ids to limit attendance to</param>
	<param name='IncludeNullCampusIds' datatype='bit'>Flag indicating if attendance not tied to campus should be included</param>
	<remarks>	
	</remarks>
	<code>
		EXEC [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeDates] '24,25,26,27,28,29,30,31,32,56,57,58,59,111,112,113,114,115,116,117,118', '2019-09-17 00:00:00', '2019-10-12 00:00:00', null, 0, null
	</code>
</doc>
*/
ALTER PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeDates] @GroupIds VARCHAR(max)
	,@StartDate DATETIME = NULL
	,@EndDate DATETIME = NULL
	,@CampusIds VARCHAR(max) = NULL
	,@IncludeNullCampusIds BIT = 0
	,@ScheduleIds VARCHAR(max) = NULL
	WITH RECOMPILE
AS
BEGIN
	--  get the SundayDates within the StartDate and EndDate so we can query against AttendanceOccurrence.SundayDate
	DECLARE @startDateSundayDate DATE
	DECLARE @endDateSundayDate DATE

	SELECT @startDateSundayDate = x.StartSundayDate
		,@endDateSundayDate = x.EndSundayDate
	FROM dbo.ufnUtility_GetSundayDateRange(@StartDate, @EndDate) x

	DECLARE @CampusTbl TABLE ([Id] INT)

	INSERT INTO @CampusTbl
	SELECT [Item]
	FROM ufnUtility_CsvToTable(ISNULL(@CampusIds, ''))

	DECLARE @ScheduleTbl TABLE ([Id] INT)

	INSERT INTO @ScheduleTbl
	SELECT [Item]
	FROM ufnUtility_CsvToTable(ISNULL(@ScheduleIds, ''))

	DECLARE @GroupTbl TABLE ([Id] INT)

	INSERT INTO @GroupTbl
	SELECT [Item]
	FROM ufnUtility_CsvToTable(ISNULL(@GroupIds, ''))

	-- Get all the attendance
	SELECT PA.[PersonId]
		,A.[SundayDate]
		,DATEADD(day, - (DATEPART(day, [SundayDate])) + 1, [SundayDate]) AS [MonthDate]
		,DATEADD(day, - (DATEPART(dayofyear, [SundayDate])) + 1, [SundayDate]) AS [YearDate]
	FROM (
		SELECT [PersonAliasId]
			,O.[GroupId]
			,[CampusId]
			,o.[SundayDate]
		FROM [Attendance] A
		INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
		INNER JOIN @GroupTbl [G] ON [G].[Id] = O.[GroupId]
		LEFT OUTER JOIN @CampusTbl [C] ON [C].[id] = [A].[CampusId]
		LEFT OUTER JOIN @ScheduleTbl [S] ON [S].[id] = [O].[ScheduleId]
		WHERE o.SundayDate BETWEEN @startDateSundayDate AND @endDateSundayDate AND A.[DidAttend] = 1 AND ((@CampusIds IS NULL OR [C].[Id] IS NOT NULL) OR (@IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL)) AND (@ScheduleIds IS NULL OR [S].[Id] IS NOT NULL)
		) A
	INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
END
" );

        }
        private void RestoreOriginal_spCheckin_AttendanceAnalyticsQuery_AttendeeFirstDates()
        {
            Sql( @"
/*
<doc>
	<summary>
        This function return people who attended based on selected filter criteria and the first 5 dates they ever attended the selected group type
	</summary>

	<returns>
		* PersonId
		* TimeAttending
		* SundayDate
	</returns>
	<param name='GroupTypeIds' datatype='varchar(max)'>The Group Type Ids (only attendance for these group types will be included</param>
	<param name='StartDate' datatype='datetime'>Beginning date range filter</param>
	<param name='EndDate' datatype='datetime'>Ending date range filter</param>
	<param name='GroupIds' datatype='varchar(max)'>Optional list of group ids to limit attendance to</param>
	<param name='CampusIds' datatype='varchar(max)'>Optional list of campus ids to limit attendance to</param>
	<param name='IncludeNullCampusIds' datatype='bit'>Flag indicating if attendance not tied to campus should be included</param>
	<remarks>	
	</remarks>
	<code>
        EXEC [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeFirstDates] '18,19,20,21,22,23,25', '24,25,26,27,28,29,30,31,32,56,57,58,59,111,112,113,114,115,116,117,118', '2019-09-17 00:00:00', '2019-10-23 00:00:00', null, 0, null
	</code>
</doc>
*/

ALTER PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeFirstDates]
	  @GroupTypeIds varchar(max)
	, @GroupIds varchar(max)
	, @StartDate datetime = NULL
	, @EndDate datetime = NULL
	, @CampusIds varchar(max) = NULL
	, @IncludeNullCampusIds bit = 0
	, @ScheduleIds varchar(max) = NULL
	WITH RECOMPILE

AS

BEGIN

    --  get the SundayDates within the StartDate and EndDate so we can query against AttendanceOccurrence.SundayDate
	DECLARE @startDateSundayDate DATE
	DECLARE @endDateSundayDate DATE

	SELECT @startDateSundayDate = x.StartSundayDate
		,@endDateSundayDate = x.EndSundayDate
	FROM dbo.ufnUtility_GetSundayDateRange(@StartDate, @EndDate) x

	DECLARE @CampusTbl TABLE ( [Id] int )
	INSERT INTO @CampusTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@CampusIds,'') )

	DECLARE @ScheduleTbl TABLE ( [Id] int )
	INSERT INTO @ScheduleTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@ScheduleIds,'') )

	DECLARE @GroupTbl TABLE ( [Id] int )
	INSERT INTO @GroupTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@GroupIds,'') )

	DECLARE @GroupTypeTbl TABLE ( [Id] int )
	INSERT INTO @GroupTypeTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@GroupTypeIds,'') )

	-- Get all the attendees
	DECLARE @PersonIdTbl TABLE ( [PersonId] INT NOT NULL )
	INSERT INTO @PersonIdTbl
	SELECT DISTINCT PA.[PersonId]
	FROM [Attendance] A
	INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
    INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
	INNER JOIN @GroupTbl [G] ON [G].[Id] = O.[GroupId]
	LEFT OUTER JOIN @CampusTbl [C] ON [C].[id] = [A].[CampusId]
	LEFT OUTER JOIN @ScheduleTbl [S] ON [S].[id] = [O].[ScheduleId]
    WHERE o.[SundayDate] BETWEEN @startDateSundayDate AND @endDateSundayDate
	AND [DidAttend] = 1
	AND ( 
		( @CampusIds IS NULL OR [C].[Id] IS NOT NULL ) OR  
		( @IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
	)
	AND ( @ScheduleIds IS NULL OR [S].[Id] IS NOT NULL  )

	-- Get the first 5 occasions on which each person attended any of the selected group types regardless of group or campus.
	-- Multiple attendances on the same date are considered as a single occasion.
	SELECT DISTINCT
	    [PersonId]
	    , [TimeAttending]
	    , [StartDate]
	FROM (
	    SELECT 
	        [P].[Id] AS [PersonId]
	        , DENSE_RANK() OVER ( PARTITION BY [P].[Id] ORDER BY [AO].[OccurrenceDate] ) AS [TimeAttending]
	        , [AO].[OccurrenceDate] AS [StartDate]
	    FROM
	        [Attendance] [A]
	        INNER JOIN [AttendanceOccurrence] [AO] ON [AO].[Id] = [A].[OccurrenceId]
	        INNER JOIN [Group] [G] ON [G].[Id] = [AO].[GroupId]
	        INNER JOIN [PersonAlias] [PA] ON [PA].[Id] = [A].[PersonAliasId] 
	        INNER JOIN [Person] [P] ON [P].[Id] = [PA].[PersonId]
	        INNER JOIN @GroupTypeTbl [GT] ON [GT].[id] = [G].[GroupTypeId]
	    WHERE 
	        [P].[Id] IN ( SELECT [PersonId] FROM @PersonIdTbl )
	        AND [DidAttend] = 1
	) [X]
    WHERE [X].[TimeAttending] <= 5

END
" );

        }
        private void RestoreOriginal_spCheckin_AttendanceAnalyticsQuery_AttendeeLastAttendance()
        {
            Sql( @"
/*
<doc>
	<summary>
        This function return people who attended based on selected filter criteria and the first 5 dates they ever attended the selected group type
	</summary>

	<returns>
		* PersonId
		* TimeAttending
		* SundayDate
	</returns>
	<param name='GroupTypeId' datatype='int'>The Check-in Area Group Type Id (only attendance for this are will be included</param>
	<param name='StartDate' datatype='datetime'>Beginning date range filter</param>
	<param name='EndDate' datatype='datetime'>Ending date range filter</param>
	<param name='GroupIds' datatype='varchar(max)'>Optional list of group ids to limit attendance to</param>
	<param name='CampusIds' datatype='varchar(max)'>Optional list of campus ids to limit attendance to</param>
	<param name='IncludeNullCampusIds' datatype='bit'>Flag indicating if attendance not tied to campus should be included</param>
	<remarks>	
	</remarks>
	<code>
		EXEC [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeLastAttendance] '24,25,26,27,28,29,30,31,32,56,57,58,59,111,112,113,114,115,116,117,118', '2019-09-17 00:00:00', '2019-10-23 00:00:00', null, 0, null
                                                                               
	</code>
</doc>
*/

ALTER PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeLastAttendance]
	  @GroupIds varchar(max) 
	, @StartDate datetime = NULL
	, @EndDate datetime = NULL
	, @CampusIds varchar(max) = NULL
	, @IncludeNullCampusIds bit = 0
	, @ScheduleIds varchar(max) = NULL

AS

BEGIN

    --  get the SundayDates within the StartDate and EndDate so we can query against AttendanceOccurrence.SundayDate
	DECLARE @startDateSundayDate DATE
	DECLARE @endDateSundayDate DATE

	SELECT @startDateSundayDate = x.StartSundayDate
		,@endDateSundayDate = x.EndSundayDate
	FROM dbo.ufnUtility_GetSundayDateRange(@StartDate, @EndDate) x

	DECLARE @CampusTbl TABLE ( [Id] int )
	INSERT INTO @CampusTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@CampusIds,'') )

	DECLARE @ScheduleTbl TABLE ( [Id] int )
	INSERT INTO @ScheduleTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@ScheduleIds,'') )

	DECLARE @GroupTbl TABLE ( [Id] int )
	INSERT INTO @GroupTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@GroupIds,'') )

	SELECT B.[PersonId], B.[CampusId], B.[CampusName], B.[GroupId], B.[GroupName], B.[ScheduleId], B.[StartDateTime], B.[LocationId], B.[RoleName], B.[LocationName] 
	FROM
	(
		SELECT PA.[PersonId], ROW_NUMBER() OVER (PARTITION BY PA.[PersonId] ORDER BY A.[StartDateTime] DESC) AS PersonRowNumber,
			A.[CampusId], CA.[Name] AS [CampusName], O.[GroupId], G.[Name] AS [GroupName], O.[ScheduleId], A.[StartDateTime], O.[LocationId], R.[RoleName], L.[Name] AS [LocationName]
		FROM [Attendance] A
		INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
		INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
		INNER JOIN [Group] G ON G.[Id] = O.[GroupId]
		INNER JOIN @GroupTbl [G2] ON [G2].[Id] = G.[Id]
		OUTER APPLY (
			SELECT TOP 1 R.[Name] AS [RoleName]
			FROM [GroupMember] M 
			INNER JOIN [GroupTypeRole] R
				ON R.[Id] = M.[GroupRoleId]
			WHERE M.[GroupId] = G.[Id]
			AND M.[PersonId] = PA.[PersonId]
			AND M.[GroupMemberStatus] <> 0
			ORDER BY R.[Order]
		) R
		LEFT OUTER JOIN [Location] L
			ON L.[Id] = O.[LocationId]
		LEFT OUTER JOIN @CampusTbl [C] ON [C].[id] = [A].[CampusId]
		LEFT OUTER JOIN @ScheduleTbl [S] ON [S].[id] = [O].[ScheduleId]
		LEFT OUTER JOIN [Campus] [CA] ON [A].[CampusId] = [CA].[Id]
		WHERE o.[SundayDate] BETWEEN @startDateSundayDate AND @endDateSundayDate
		AND [DidAttend] = 1
		AND ( 
			( @CampusIds IS NULL OR [C].[Id] IS NOT NULL ) OR  
			( @IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
		)
		AND ( @ScheduleIds IS NULL OR [S].[Id] IS NOT NULL  )
	) B
	WHERE B.PersonRowNumber = 1

END
" );

        }
        private void RestoreOriginal_spCheckin_AttendanceAnalyticsQuery_Attendees()
        {
            Sql( @"
/*
<doc>
	<summary>
 		This function returns the people that attended based on selected filter criteria
	</summary>

	<returns>
		* Id 
		* NickName
		* LastName
	</returns>
	<param name='GroupTypeId' datatype='int'>The Check-in Area Group Type Id (only attendance for this are will be included</param>
	<param name='StartDate' datatype='datetime'>Beginning date range filter</param>
	<param name='EndDate' datatype='datetime'>Ending date range filter</param>
	<param name='GroupIds' datatype='varchar(max)'>Optional list of group ids to limit attendance to</param>
	<param name='CampusIds' datatype='varchar(max)'>Optional list of campus ids to limit attendance to</param>
	<param name='IncludeNullCampusIds' datatype='bit'>Flag indicating if attendance not tied to campus should be included</param>
	<remarks>	
	</remarks>
	<code>
        EXEC [dbo].[spCheckin_AttendanceAnalyticsQuery_Attendees] '24,25,26,27,28,29,30,31,32,56,57,58,59,111,112,113,114,115,116,117,118', '2019-09-17 00:00:00', '2019-10-23 00:00:00', null, 0, 0, 0, null
	</code>
</doc>
*/

ALTER PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_Attendees]
	  @GroupIds varchar(max)
	, @StartDate datetime = NULL
	, @EndDate datetime = NULL
	, @CampusIds varchar(max) = NULL
	, @IncludeNullCampusIds bit = 0
    , @IncludeParentsWithChild bit = 0
    , @IncludeChildrenWithParents bit = 0
	, @ScheduleIds varchar(max) = NULL
	WITH RECOMPILE

AS

BEGIN

    --  get the SundayDates within the StartDate and EndDate so we can query against AttendanceOccurrence.SundayDate
	DECLARE @startDateSundayDate DATE
	DECLARE @endDateSundayDate DATE

	SELECT @startDateSundayDate = x.StartSundayDate
		,@endDateSundayDate = x.EndSundayDate
	FROM dbo.ufnUtility_GetSundayDateRange(@StartDate, @EndDate) x

	DECLARE @CampusTbl TABLE ( [Id] int )
	INSERT INTO @CampusTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@CampusIds,'') )

	DECLARE @ScheduleTbl TABLE ( [Id] int )
	INSERT INTO @ScheduleTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@ScheduleIds,'') )

	DECLARE @GroupTbl TABLE ( [Id] int )
	INSERT INTO @GroupTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@GroupIds,'') )

	-- Get all the attendees
    IF @IncludeChildrenWithParents = 0 AND @IncludeParentsWithChild = 0
    BEGIN

        -- Just return the person who attended
	    SELECT	
		    P.[Id],
		    P.[NickName],
		    P.[LastName],
			P.[Gender],
		    P.[Email],
            P.[GivingId],
		    P.[BirthDate],
            P.[ConnectionStatusValueId],
			P.[GraduationYear],
			P.[DeceasedDate]
	    FROM (
		    SELECT DISTINCT PA.[PersonId]
			FROM (
				SELECT 
					A.[PersonAliasId],
					A.[CampusId],
					O.[ScheduleId]
 				FROM [Attendance] A
				INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
				INNER JOIN @GroupTbl G ON G.[Id] = O.[GroupId]
				WHERE o.[SundayDate] BETWEEN @startDateSundayDate AND @endDateSundayDate
				AND [DidAttend] = 1
			) A
            INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
			LEFT OUTER JOIN @CampusTbl [C] ON [C].[id] = [A].[CampusId]
			LEFT OUTER JOIN @ScheduleTbl [S] ON [S].[id] = [A].[ScheduleId]
		    WHERE ( 
			    ( @CampusIds IS NULL OR [C].[Id] IS NOT NULL ) OR  
			    ( @IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
		    )
		    AND ( @ScheduleIds IS NULL OR [S].[Id] IS NOT NULL  )
	    ) [Attendee]
	    INNER JOIN [Person] P 
			ON P.[Id] = [Attendee].[PersonId]

    END
    ELSE
    BEGIN

        DECLARE @AdultRoleId INT = ( SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42' )
        DECLARE @ChildRoleId INT = ( SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9' )

        IF @IncludeParentsWithChild = 1 
   BEGIN

            -- Child attended, also include their parents
	        SELECT	
                C.[Id],
                C.[NickName],
                C.[LastName],
				C.[Gender],
                C.[Email],
                C.[GivingId],
                C.[BirthDate],
                C.[ConnectionStatusValueId],
				C.[GraduationYear],
				C.[DeceasedDate],
                A.[Id] AS [ParentId],
                A.[NickName] AS [ParentNickName],
                A.[LastName] AS [ParentLastName],
                A.[Email] AS [ParentEmail],
                A.[GivingId] as [ParentGivingId],
                A.[BirthDate] AS [ParentBirthDate],
				A.[Gender] AS [ParentGender],
				A.[DeceasedDate] AS [ParentDeceasedDate]
	        FROM (
				SELECT DISTINCT PA.[PersonId]
				FROM (
					SELECT 
						A.[PersonAliasId],
						A.[CampusId],
						O.[ScheduleId]
 					FROM [Attendance] A
					INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
					INNER JOIN @GroupTbl G ON G.[Id] = O.[GroupId]
					WHERE o.[SundayDate] BETWEEN @startDateSundayDate AND @endDateSundayDate
					AND [DidAttend] = 1
				) A
				INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
				LEFT OUTER JOIN @CampusTbl [C] ON [C].[id] = [A].[CampusId]
				LEFT OUTER JOIN @ScheduleTbl [S] ON [S].[id] = [A].[ScheduleId]
				WHERE ( 
					( @CampusIds IS NULL OR [C].[Id] IS NOT NULL ) OR  
					( @IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
				)
				AND ( @ScheduleIds IS NULL OR [S].[Id] IS NOT NULL  )
	        ) [Attendee]
	        INNER JOIN [Person] C 
				ON C.[Id] = [Attendee].[PersonId]
            INNER JOIN [GroupMember] GMC
                ON GMC.[PersonId] = C.[Id]
	            AND GMC.[GroupRoleId] = @ChildRoleId
            INNER JOIN [GroupMember] GMA
	            ON GMA.[GroupId] = GMC.[GroupId]
	            AND GMA.[GroupRoleId] = @AdultRoleId
            INNER JOIN [Person] A 
				ON A.[Id] = GMA.[PersonId]

        END
        
        IF @IncludeChildrenWithParents = 1
        BEGIN

            -- Parents attended, include their children
	        SELECT	
                A.[Id],
                A.[NickName],
                A.[LastName],
				A.[Gender],
                A.[Email],
                A.[GivingId],
                A.[BirthDate],
                A.[ConnectionStatusValueId],
				A.[DeceasedDate],
                C.[Id] AS [ChildId],
                C.[NickName] AS [ChildNickName],
                C.[LastName] AS [ChildLastName],
                C.[Email] AS [ChildEmail],
                C.[GivingId] as [ChildGivingId],
                C.[BirthDate] AS [ChildBirthDate],
				C.[Gender] as [ChildGender],
				C.[GraduationYear] as [ChildGraduationYear],
				C.[DeceasedDate] AS [ChildDeceasedDate]
	        FROM (
				SELECT DISTINCT PA.[PersonId]
				FROM (
					SELECT 
						A.[PersonAliasId],
						A.[CampusId],
						O.[ScheduleId]
 					FROM [Attendance] A
					INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
					INNER JOIN @GroupTbl G ON G.[Id] = O.[GroupId]
					WHERE o.[SundayDate] BETWEEN @startDateSundayDate AND @endDateSundayDate
					AND [DidAttend] = 1
				) A
				INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
				LEFT OUTER JOIN @CampusTbl [C] ON [C].[id] = [A].[CampusId]
				LEFT OUTER JOIN @ScheduleTbl [S] ON [S].[id] = [A].[ScheduleId]
				WHERE ( 
					( @CampusIds IS NULL OR [C].[Id] IS NOT NULL ) OR  
					( @IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
				)
				AND ( @ScheduleIds IS NULL OR [S].[Id] IS NOT NULL  )
	        ) [Attendee]
	        INNER JOIN [Person] A 
				ON A.[Id] = [Attendee].[PersonId]
            INNER JOIN [GroupMember] GMA
                ON GMA.[PersonId] = A.[Id]
	            AND GMA.[GroupRoleId] = @AdultRoleId
            INNER JOIN [GroupMember] GMC
	            ON GMC.[GroupId] = GMA.[GroupId]
	            AND GMC.[GroupRoleId] = @ChildRoleId
            INNER JOIN [Person] C 
				ON C.[Id] = GMC.[PersonId]

        END

    END

END
" );

        }
        private void RestoreOriginal_spCheckin_AttendanceAnalyticsQuery_NonAttendees()
        {
            Sql( @"
/*
<doc>
    <summary>
         This function returns any person ids for people that have attended previously but who have not attended since the beginning date
    </summary>

    <returns>
        * PersonId 
        * SundayDate - Last time attended
    </returns>
    <param name='GroupTypeIds' datatype='varchar(max)'>The Group Type Ids (only attendance for these group types will be included</param>
    <param name='StartDateTime' datatype='datetime'>Beginning date range filter</param>
    <param name='GroupIds' datatype='varchar(max)'>Optional list of group ids to limit attendance to</param>
    <param name='CampusIds' datatype='varchar(max)'>Optional list of campus ids to limit attendance to</param>
    <param name='IncludeNullCampusIds' datatype='bit'>Flag indicating if attendance not tied to campus should be included</param>
    <remarks>    
    </remarks>
    <code>
        EXEC [dbo].[spCheckin_AttendanceAnalyticsQuery_NonAttendees] '18,19,20,21,22,23,25', '24,25,26,27,28,29,30,31,32,56,57,58,59,111,112,113,114,115,116,117,118', '2019-09-17 00:00:00', '2019-10-23 00:00:00', null, 0, null, 0, 0
    </code>
</doc>
*/

ALTER PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_NonAttendees]
      @GroupTypeIds varchar(max)
    , @GroupIds varchar(max)
    , @StartDate datetime = NULL
    , @EndDate datetime = NULL
    , @CampusIds varchar(max) = NULL
    , @IncludeNullCampusIds bit = 0
    , @ScheduleIds varchar(max) = NULL
    , @IncludeParentsWithChild bit = 0
    , @IncludeChildrenWithParents bit = 0
    WITH RECOMPILE

AS

BEGIN

   	--  get the SundayDates within the StartDate and EndDate so we can query against AttendanceOccurrence.SundayDate
	DECLARE @startDateSundayDate DATE
	DECLARE @endDateSundayDate DATE

	SELECT @startDateSundayDate = x.StartSundayDate
		,@endDateSundayDate = x.EndSundayDate
	FROM dbo.ufnUtility_GetSundayDateRange(@StartDate, @EndDate) x

    DECLARE @PersonIdTbl TABLE ( [Id] INT NOT NULL )

    DECLARE @CampusTbl TABLE ( [Id] int )
    INSERT INTO @CampusTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@CampusIds,'') )

    DECLARE @ScheduleTbl TABLE ( [Id] int )
    INSERT INTO @ScheduleTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@ScheduleIds,'') )

    DECLARE @GroupTbl TABLE ( [Id] int )
    INSERT INTO @GroupTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@GroupIds,'') )

    DECLARE @GroupTypeTbl TABLE ( [Id] int )
    INSERT INTO @GroupTypeTbl SELECT [Item] FROM ufnUtility_CsvToTable( ISNULL(@GroupTypeIds,'') )

    -- Find all the person ids for people who belong to any of the selected groups and have not attended any group/campus of selected group type
    INSERT INTO @PersonIdTbl
    SELECT DISTINCT M.[PersonId]
    FROM @GroupTbl G
    INNER JOIN [GroupMember] M
        ON M.[GroupId] = G.[Id]
        AND M.[GroupMemberStatus] = 1
    WHERE M.[PersonId] NOT IN (
        SELECT DISTINCT PA.[PersonId]
        FROM (
            SELECT 
                A.[PersonAliasId],
                A.[CampusId],
                O.[ScheduleId]
             FROM [Attendance] A
            INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
            INNER JOIN @GroupTbl G ON G.[Id] = O.[GroupId]
            WHERE o.[SundayDate] BETWEEN @startDateSundayDate AND @endDateSundayDate
            AND [DidAttend] = 1
        ) A
        INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]
        LEFT OUTER JOIN @CampusTbl [C] ON [C].[id] = [A].[CampusId]
        LEFT OUTER JOIN @ScheduleTbl [S] ON [S].[id] = [A].[ScheduleId]
        WHERE ( 
            ( @CampusIds IS NULL OR [C].[Id] IS NOT NULL ) OR  
            ( @IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
        )
        AND ( @ScheduleIds IS NULL OR [S].[Id] IS NOT NULL  )
    )

    IF @IncludeChildrenWithParents = 0 AND @IncludeParentsWithChild = 0
    BEGIN

        -- Just return the person 
 SELECT    
            P.[Id],
            P.[NickName],
            P.[LastName],
            P.[Gender],
            P.[Email],
            P.[GivingId],
            P.[BirthDate],
            P.[ConnectionStatusValueId],
			P.[DeceasedDate],
			P.[GraduationYear]
        FROM @PersonIdTbl M
        INNER JOIN [Person] P ON P.[Id] = M.[Id]

    END
    ELSE
    BEGIN

        DECLARE @AdultRoleId INT = ( SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42' )
        DECLARE @ChildRoleId INT = ( SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9' )

        IF @IncludeParentsWithChild = 1 
        BEGIN

            -- Child attended, also include their parents
            SELECT    
                C.[Id],
                C.[NickName],
                C.[LastName],
                C.[Gender],
                C.[Email],
                C.[GivingId],
                C.[BirthDate],
                C.[ConnectionStatusValueId],
				C.[DeceasedDate],
				C.[GraduationYear],
                A.[Id] AS [ParentId],
                A.[NickName] AS [ParentNickName],
                A.[LastName] AS [ParentLastName],
                A.[Email] AS [ParentEmail],
                A.[GivingId] as [ParentGivingId],
                A.[BirthDate] AS [ParentBirthDate],
				A.[DeceasedDate] AS [ParentDeceasedDate],
				A.[Gender] AS [ParentGender]
            FROM @PersonIdTbl M
            INNER JOIN [Person] C 
                ON C.[Id] = M.[Id]
            INNER JOIN [GroupMember] GMC
                ON GMC.[PersonId] = C.[Id]
                AND GMC.[GroupRoleId] = @ChildRoleId
            INNER JOIN [GroupMember] GMA
                ON GMA.[GroupId] = GMC.[GroupId]
                AND GMA.[GroupRoleId] = @AdultRoleId
            INNER JOIN [Person] A 
                ON A.[Id] = GMA.[PersonId]

        END
        
        IF @IncludeChildrenWithParents = 1
        BEGIN

            -- Parents attended, include their children
            SELECT    
                A.[Id],
                A.[NickName],
                A.[LastName],
                A.[Gender],
                A.[Email],
                A.[GivingId] as [GivingId],
                A.[BirthDate],
                A.[ConnectionStatusValueId],
				A.[DeceasedDate],
                C.[Id] AS [ChildId],
                C.[NickName] AS [ChildNickName],
                C.[LastName] AS [ChildLastName],
                C.[Email] AS [ChildEmail],
                C.[GivingId] as [ChildGivingId],
                C.[BirthDate] AS [ChildBirthDate],
				C.[Gender] as [ChildGender],
				C.[GraduationYear] as [ChildGraduationYear],
				C.[DeceasedDate] AS [ChildDeceasedDate]
            FROM @PersonIdTbl M
            INNER JOIN [Person] A 
                ON A.[Id] = M.[Id]
            INNER JOIN [GroupMember] GMA
                ON GMA.[PersonId] = A.[Id]
                AND GMA.[GroupRoleId] = @AdultRoleId
            INNER JOIN [GroupMember] GMC
                ON GMC.[GroupId] = GMA.[GroupId]
                AND GMC.[GroupRoleId] = @ChildRoleId
            INNER JOIN [Person] C 
                ON C.[Id] = GMC.[PersonId]

        END

    END

    -- Get the first 5 times they attended this group type (any group or campus)
    SELECT 
        P.[Id] AS [PersonId],
        D.[TimeAttending],
        D.[StartDate]
    FROM @PersonIdTbl P
    CROSS APPLY ( 
        SELECT TOP 5 
            ROW_NUMBER() OVER (ORDER BY [StartDate]) AS [TimeAttending],
            [StartDate]
        FROM (
            SELECT DISTINCT [StartDate]
            FROM [vCheckin_GroupTypeAttendance] A
            INNER JOIN @GroupTypeTbl [G] ON [G].[id] = [A].[GroupTypeId]
            AND A.[PersonId] = P.[Id]
        ) S
    ) D

    -- Get the last time they attended
    SELECT 
        PD.[PersonId],
        PD.[CampusId],
		CA.[Name] AS [CampusName],
        PD.[GroupId],
        PD.[GroupName],
        PD.[ScheduleId],
        PD.[StartDateTime],
        PD.[LocationId],
   R.[Name] AS [RoleName],
        L.[Name] AS [LocationName]
    FROM (
        SELECT 
            P.[Id] AS [PersonId],
            A.[CampusId],
            A.[GroupId],
  A.[GroupName],
            A.[ScheduleId],
            A.[StartDateTime],
            A.[LocationId]
        FROM @PersonIdTbl P
        CROSS APPLY (
            SELECT TOP 1 
                A.[CampusId],
                A.[GroupId],
                G.[Name] AS [GroupName],
                A.[ScheduleId],
                A.[LocationId],
                A.[StartDateTime]
            FROM (
                SELECT 
                    A.[PersonAliasId],
                    A.[CampusId],
                    O.[GroupId],
                    O.[ScheduleId],
                    O.[LocationId],
                    A.[StartDateTime]
                 FROM [Attendance] A
                INNER JOIN [AttendanceOccurrence] O ON O.[Id] = A.[OccurrenceId]
                INNER JOIN @GroupTbl G ON G.[Id] = O.[GroupId]
                WHERE o.[SundayDate] < @startDateSundayDate
                AND [DidAttend] = 1
            ) A
            INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId] 
            INNER JOIN [Group] G ON G.[Id] = A.[GroupId]
            LEFT OUTER JOIN @CampusTbl [C] ON [C].[id] = [A].[CampusId]
            LEFT OUTER JOIN @ScheduleTbl [S] ON [S].[id] = [A].[ScheduleId]
            WHERE PA.[PersonId] = P.[Id]
            AND ( 
                ( @CampusIds IS NULL OR [C].[Id] IS NOT NULL ) OR  
                ( @IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
            )
            AND ( @ScheduleIds IS NULL OR [S].[Id] IS NOT NULL  )
            ORDER BY A.[StartDateTime] DESC
        ) A
    ) PD
    OUTER APPLY (
        SELECT TOP 1 R.[Name]
        FROM [GroupMember] M 
        INNER JOIN [GroupTypeRole] R
            ON R.[Id] = M.[GroupRoleId]
        WHERE M.[GroupId] = PD.[GroupId]
        AND M.[PersonId] = PD.[PersonId]
        AND M.[GroupMemberStatus] <> 0
        ORDER BY R.[Order]
    ) R
    LEFT OUTER JOIN [Location] L
        ON L.[Id] = PD.[LocationId]
	LEFT OUTER JOIN [Campus] CA ON PD.[CampusId] = CA.[Id]
END
" );
        }
    }
}
