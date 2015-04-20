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
		EXEC [dbo].[spFinance_ContributionStatementQuery] '01-01-2014', '01-01-2015', null, null, 0, 1 -- year 2014 statements for all persons that have a mailing address
        EXEC [dbo].[spFinance_ContributionStatementQuery] '01-01-2014', '01-01-2015', null, null, 1, 1 -- year 2014 statements for all persons regardless of mailing address
        EXEC [dbo].[spFinance_ContributionStatementQuery] '01-01-2014', '01-01-2015', null, 2, 1, 1  -- year 2014 statements for Ted Decker
	</code>
</doc>
*/
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

	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	;WITH tranListCTE
	AS
	(
		SELECT  
			[pa].[PersonId] 
		FROM 
			[FinancialTransaction] [ft]
		INNER JOIN 
			[FinancialTransactionDetail] [ftd] ON [ft].[Id] = [ftd].[TransactionId]
		INNER JOIN 
			[PersonAlias] [pa] ON [pa].[id] = [ft].[AuthorizedPersonAliasId]
		WHERE 
			([TransactionDateTime] >= @StartDate and [TransactionDateTime] < @EndDate)
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
		, [pn].[PersonNames] [AddressPersonNames]
        , case when l.Id is null then 0 else 1 end [HasAddress]
		, [l].[Street1]
		, [l].[Street2]
		, [l].[City]
		, [l].[State]
		, [l].[PostalCode]
		, @StartDate [StartDate]
		, @EndDate [EndDate]
		, null [CustomMessage1]
		, null [CustomMessage2]
	FROM (
		-- Get distinct Giving Groups for Persons that have a specific GivingGroupId and have transactions that match the filter
		-- These are Persons that give as part of a Group.  For example, Husband and Wife
		SELECT DISTINCT
			null [PersonId] 
			, [g].[Id] [GroupId]
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
			[p].[Id] in (SELECT * FROM tranListCTE)
		UNION
		-- Get Persons and their GroupId(s) that do not have GivingGroupId and have transactions that match the filter.        
		-- These are the persons that give as individuals vs as part of a group. We need the Groups (families they belong to) in order 
		-- to determine which address(es) the statements need to be mailed to 
		SELECT  
			[p].[Id] [PersonId],
			[g].[Id] [GroupId]
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
		AND [p].[Id] IN (SELECT * FROM tranListCTE)
	) [pg]
	CROSS APPLY 
		[ufnCrm_GetFamilyTitle]([pg].[PersonId], [pg].[GroupId], default, default) [pn]
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
		[gl].[GroupLocationTypeValueId] = (SELECT Id FROM DefinedValue WHERE [Guid] = @cLOCATION_TYPE_HOME)
        ) [l] 
        ON 
		[l].[GroupId] = [pg].[GroupId]
    ) n
    WHERE n.HasAddress = 1 or @IncludeIndividualsWithNoAddress = 1
    ORDER BY
	CASE WHEN @OrderByPostalCode = 1 THEN PostalCode END
    
END
