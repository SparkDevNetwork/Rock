// <copyright>
// Copyright by Central Christian Church
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;
namespace com.centralaz.Finance.Migrations
{
    [MigrationNumber( 5, "1.5.1" )]
    public class ContributionLetter : Migration
    {
        public override void Up()
        {
            Sql( @"
CREATE FUNCTION [dbo].[ufnCrm_GetFamilyFirstOrNickNames] (
    @PersonId INT
    ,@GroupId INT
    ,@GroupPersonIds VARCHAR(max) = NULL
    ,@UseNickName BIT = 0
    )
RETURNS @PersonNamesTable TABLE (PersonNames VARCHAR(max))
AS
BEGIN
       DECLARE @PersonNames VARCHAR(max);
    DECLARE @AdultLastNameCount INT;
    DECLARE @GroupFirstOrNickNames VARCHAR(max) = '';
    DECLARE @GroupLastName VARCHAR(max);
    DECLARE @GroupAdultFullNames VARCHAR(max) = '';
    DECLARE @GroupNonAdultFullNames VARCHAR(max) = '';
    DECLARE @GroupMemberTable TABLE (
        LastName VARCHAR(max)
        ,FirstOrNickName VARCHAR(max)
        ,FullName VARCHAR(max)
        ,Gender INT
        ,GroupRoleGuid UNIQUEIDENTIFIER
        );
    DECLARE @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT UNIQUEIDENTIFIER = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42';

    IF (@PersonId IS NOT NULL)
    BEGIN
        -- just getting the Person Names portion of the address for an individual person
        SELECT @PersonNames = CASE @UseNickName
                WHEN 1
                    THEN ISNULL([p].[NickName], '')
                ELSE ISNULL([p].[FirstName], '')
                END
        FROM [Person] [p]
        LEFT OUTER JOIN [DefinedValue] [dv] ON [dv].[Id] = [p].[SuffixValueId]
        WHERE [p].[Id] = @PersonId;
    END
    ELSE
    BEGIN
        -- populate a table variable with the data we'll need for the different cases
        -- if GroupPersonIds is set (comma-delimited) only a subset of the family members should be combined
        INSERT INTO @GroupMemberTable
        SELECT [p].[LastName]
            ,CASE @UseNickName
                WHEN 1
                    THEN ISNULL([p].[NickName], '')
                ELSE ISNULL([p].[FirstName], '')
                END [FirstName]
            ,CASE @UseNickName
                WHEN 1
                    THEN ISNULL([p].[NickName], '')
                ELSE ISNULL([p].[FirstName], '')
                END [FullName]
            ,[p].Gender
            ,[gr].[Guid]
        FROM [GroupMember] [gm]
        JOIN [Person] [p] ON [p].[Id] = [gm].[PersonId]
        LEFT OUTER JOIN [DefinedValue] [dv] ON [dv].[Id] = [p].[SuffixValueId]
        JOIN [GroupTypeRole] [gr] ON [gm].[GroupRoleId] = [gr].[Id]
        WHERE [GroupId] = @GroupId
            AND (
                ISNULL(@GroupPersonIds, '') = ''
                OR (
                    p.[Id] IN (
                        SELECT *
                        FROM ufnUtility_CsvToTable(@GroupPersonIds)
                        )
                    )
                )

        -- determine adultCount and if we can use the same lastname for all adults, and get lastname while we are at it
        SELECT @AdultLastNameCount = count(DISTINCT [LastName])
            ,@GroupLastName = max([LastName])
        FROM @GroupMemberTable
        WHERE [GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT;


        IF @AdultLastNameCount > 0
        BEGIN
            -- get the FirstNames and Adult FullNames for use in the cases of families with Adults
            SELECT @GroupFirstOrNickNames = @GroupFirstOrNickNames + [FirstOrNickName] + ' & '
                ,@GroupAdultFullNames = @GroupAdultFullNames + [FullName] + ' & '
            FROM @GroupMemberTable g
            WHERE g.[GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
            ORDER BY g.Gender
                ,g.FirstOrNickName

            -- cleanup the trailing ' &'s
            IF len(@GroupFirstOrNickNames) > 2
            BEGIN
                -- trim the extra ' &' off the end 
                SET @GroupFirstOrNickNames = SUBSTRING(@GroupFirstOrNickNames, 0, len(@GroupFirstOrNickNames) - 1)
            END

            IF len(@GroupAdultFullNames) > 2
            BEGIN
                -- trim the extra ' &' off the end 
                SET @GroupAdultFullNames = SUBSTRING(@GroupAdultFullNames, 0, len(@GroupAdultFullNames) - 1)
            END

            -- if all the firstnames are blanks, get rid of the '&'
            IF (LTRIM(RTRIM(@GroupFirstOrNickNames)) = '&')
            BEGIN
                SET @GroupFirstOrNickNames = ''
            END

            -- if all the fullnames are blanks, get rid of the '&'
            IF (LTRIM(RTRIM(@GroupAdultFullNames)) = '&')
            BEGIN
                SET @GroupAdultFullNames = ''
            END
        END

        IF @AdultLastNameCount = 0
        BEGIN
            -- get the NonAdultFullNames for use in the case of families without adults 
            SELECT @GroupNonAdultFullNames = @groupNonAdultFullNames + [FullName] + ' & '
            FROM @GroupMemberTable
            ORDER BY [FullName]

            IF len(@GroupNonAdultFullNames) > 2
            BEGIN
                -- trim the extra ' &' off the end 
                SET @GroupNonAdultFullNames = SUBSTRING(@GroupNonAdultFullNames, 0, len(@GroupNonAdultFullNames) - 1)
            END

            -- if all the fullnames are blanks, get rid of the '&'
            IF (LTRIM(RTRIM(@GroupNonAdultFullNames)) = '&')
            BEGIN
                SET @GroupNonAdultFullNames = ''
            END
        END

        IF (@AdultLastNameCount = 1)
        BEGIN
            -- just one lastname and at least one adult. Get the Person Names portion of the address in the format <MaleAdult> & <FemaleAdult> <LastName>
            SET @PersonNames = @GroupFirstOrNickNames;
        END
        ELSE IF (@AdultLastNameCount = 0)
        BEGIN
            -- no adults in family, list all members of the family in 'Fullname & FullName & ...' format
            SET @PersonNames = @GroupNonAdultFullNames;
        END
        ELSE
        BEGIN
            -- multiple adult lastnames
            SET @PersonNames = @GroupAdultFullNames;
        END
    END

    WHILE (len(@PersonNames) - len(replace(@PersonNames, ' & ', '  ')) > 1)
    BEGIN
        SET @PersonNames = Stuff(@PersonNames, CharIndex(' & ', @PersonNames), Len(' & '), ', ')
    END

    INSERT INTO @PersonNamesTable ([PersonNames])
    VALUES (@PersonNames);

    RETURN
END

            " );

            Sql( @"
CREATE PROCEDURE [dbo].[spFinance_GetGivingGroupAddresses]
	@StartDate datetime
	, @EndDate datetime
	, @Account1Id int
	, @Account2Id int
	, @Account3Id int
	, @Account4Id int
	, @MinimumAmount decimal
	, @ChapterNumber int
	, @ChapterSize int
	, @CampusList nvarchar(max)
AS
BEGIN

DECLARE @cTRANSACTION_TYPE_CONTRIBUTION uniqueidentifier = '2D607262-52D6-4724-910D-5C6E8FB89ACC'
DECLARE @transactionTypeContributionId int = (select top 1 Id from DefinedValue where [Guid] = @cTRANSACTION_TYPE_CONTRIBUTION)
DECLARE @cLOCATION_TYPE_HOME uniqueidentifier = '8C52E53C-2A66-435A-AE6E-5EE307D9A0DC'
DECLARE @cLOCATION_TYPE_WORK uniqueidentifier = 'E071472A-F805-4FC4-917A-D5E3C095C35C'
DECLARE @cGROUPTYPE_FAMILY uniqueidentifier = '790E3215-3B10-442B-AF69-616C0DCB998E'

Declare @givingIds table(
GivingId nvarchar(50)
)

insert into @givingIds
select GivingId from (
	select GivingId, row_number() over (order by PostalCode) as 'rowNumber'
	from (
		select p.GivingId, max(l.PostalCode) as 'PostalCode'
		from FinancialTransaction ft
		join FinancialTransactionDetail ftd on ftd.TransactionId = ft.Id
		join PersonAlias pa on ft.AuthorizedPersonAliasId = pa.Id
		join Person p on pa.PersonId = p.Id		
		JOIN [GroupMember] [gm] ON [gm].[PersonId] = [p].[Id]
		JOIN [Group] [g] ON [gm].[GroupId] = [g].[Id]
		Join [GroupLocation] gl on gl.GroupId = g.Id
		JOIN [Location] [l] ON [l].[Id] = [gl].[LocationId]
		Where ft.TransactionTypeValueId = @transactionTypeContributionId
		and (@startDate is null or ft.TransactionDateTime >= @StartDate)
		and ( @endDate is null or ft.TransactionDateTime < @EndDate)				
		and ( ( p.GivingGroupId is null AND [g].[GroupTypeId] = (SELECT Id FROM GroupType WHERE [Guid] = @cGROUPTYPE_FAMILY)) or p.GivingGroupId = g.Id)
		and g.CampusId in (SELECT * FROM [dbo].[ufnUtility_CsvToTable](@CampusList))
		and [gl].[IsMailingLocation] = 1
		AND [gl].[GroupLocationTypeValueId] = (SELECT Id FROM DefinedValue WHERE [Guid] = @cLOCATION_TYPE_HOME)
		group by GivingId
		having sum(ftd.Amount) >= @MinimumAmount		
		) g
	)og
where rowNumber between (@ChapterNumber-1)*@ChapterSize and @ChapterNumber*@ChapterSize


SELECT * FROM (
    SELECT 
		pg.GivingId		
		, [pt].[PersonNames] [AddressPersonNames]
		, [pn].[PersonNames] [GreetingPersonNames]
        , case when l.Id is null then 0 else 1 end [HasAddress]
		, [l].[Street1]
		, [l].[Street2]
		, [l].[City]
		, [l].[State]
		, [l].[PostalCode]
	FROM (
		-- Get distinct Giving Groups for Persons that have a specific GivingGroupId and have transactions that match the filter
		-- These are Persons that give as part of a Group.  For example, Husband and Wife
		SELECT DISTINCT
			null [PersonId] 
			, [g].[Id] [GroupId]
			,p.GivingId
		FROM  [Person] [p]
		INNER JOIN  [Group] [g] ON [p].[GivingGroupId] = [g].[Id]
        AND [p].GivingId in (SELECT GivingId FROM @givingIds)
		UNION
		-- Get Persons and their GroupId(s) that do not have GivingGroupId and have transactions that match the filter.        
		-- These are the persons that give as individuals vs as part of a group. We need the Groups (families they belong to) in order 
		-- to determine which address(es) the statements need to be mailed to 
		SELECT  
			[p].[Id] [PersonId],
			[g].[Id] [GroupId],
			p.givingId
		FROM [Person] [p]
		JOIN [GroupMember] [gm] ON [gm].[PersonId] = [p].[Id]
		JOIN [Group] [g] ON [gm].[GroupId] = [g].[Id]
		WHERE [p].[GivingGroupId] is null
		AND [g].[GroupTypeId] = (SELECT Id FROM GroupType WHERE [Guid] = @cGROUPTYPE_FAMILY)
		AND [p].GivingId IN (SELECT GivingId FROM @givingIds)
	) [pg]
	CROSS APPLY [ufnCrm_GetFamilyTitle]([pg].[PersonId], [pg].[GroupId], default, default) [pt]
	CROSS APPLY [ufnCrm_GetFamilyFirstOrNickNames]([pg].[PersonId], [pg].[GroupId], default, 1) [pn]
	LEFT OUTER JOIN (
    SELECT l.*, gl.GroupId 
	from [GroupLocation] [gl] 
	LEFT OUTER JOIN [Location] [l] ON [l].[Id] = [gl].[LocationId]
	WHERE [gl].[IsMailingLocation] = 1
	AND [gl].[GroupLocationTypeValueId] = (SELECT Id FROM DefinedValue WHERE [Guid] = @cLOCATION_TYPE_HOME)
        ) [l] 
        ON [l].[GroupId] = [pg].[GroupId]
    ) n
    WHERE n.HasAddress = 1
    ORDER BY PostalCode
	OPTION (RECOMPILE);

END

            " );

            Sql( @"
ALTER PROCEDURE [dbo].[spFinance_GetGivingGroupTransactions]
	@StartDate datetime
	, @EndDate datetime
	, @Account1Id int
	, @Account2Id int
	, @Account3Id int
	, @Account4Id int
	, @MinimumAmount decimal
	, @ChapterNumber int
	, @ChapterSize int
	, @CampusList nvarchar(max)
AS
BEGIN
DECLARE @cTRANSACTION_TYPE_CONTRIBUTION uniqueidentifier = '2D607262-52D6-4724-910D-5C6E8FB89ACC'
DECLARE @transactionTypeContributionId int = (select top 1 Id from DefinedValue where [Guid] = @cTRANSACTION_TYPE_CONTRIBUTION)
DECLARE @cLOCATION_TYPE_HOME uniqueidentifier = '8C52E53C-2A66-435A-AE6E-5EE307D9A0DC'
DECLARE @cLOCATION_TYPE_WORK uniqueidentifier = 'E071472A-F805-4FC4-917A-D5E3C095C35C'
DECLARE @cGROUPTYPE_FAMILY uniqueidentifier = '790E3215-3B10-442B-AF69-616C0DCB998E'

Declare @givingIds table(
GivingId nvarchar(50)
)

insert into @givingIds
select GivingId from (
	select GivingId, row_number() over (order by PostalCode) as 'rowNumber'
	from (
		select p.GivingId, max(l.PostalCode) as 'PostalCode'
		from FinancialTransaction ft
		join FinancialTransactionDetail ftd on ftd.TransactionId = ft.Id
		join PersonAlias pa on ft.AuthorizedPersonAliasId = pa.Id
		join Person p on pa.PersonId = p.Id		
		JOIN [GroupMember] [gm] ON [gm].[PersonId] = [p].[Id]
		JOIN [Group] [g] ON [gm].[GroupId] = [g].[Id]
		Join [GroupLocation] gl on gl.GroupId = g.Id
		JOIN [Location] [l] ON [l].[Id] = [gl].[LocationId]
		Where ft.TransactionTypeValueId = @transactionTypeContributionId
		and (@startDate is null or ft.TransactionDateTime >= @StartDate)
		and ( @endDate is null or ft.TransactionDateTime < @EndDate)				
		and ( ( p.GivingGroupId is null AND [g].[GroupTypeId] = (SELECT Id FROM GroupType WHERE [Guid] = @cGROUPTYPE_FAMILY)) or p.GivingGroupId = g.Id)
		and g.CampusId in (SELECT * FROM [dbo].[ufnUtility_CsvToTable](@CampusList))
		and [gl].[IsMailingLocation] = 1
		AND [gl].[GroupLocationTypeValueId] = (SELECT Id FROM DefinedValue WHERE [Guid] = @cLOCATION_TYPE_HOME)
		group by GivingId
		having sum(ftd.Amount) >= @MinimumAmount		
		) g
	)og
where rowNumber between (@ChapterNumber-1)*@ChapterSize and @ChapterNumber*@ChapterSize
 
Select
	p.GivingId as 'GivingId',
	ft.TransactionCode as 'TransactionCode',
	ft.TransactionDateTime as 'TransactionDateTime',
	Sum(case when ftd.AccountId = @Account1Id then ftd.Amount end) as 'Account 1 Sum',
	Sum(case when ftd.AccountId = @Account2Id then ftd.Amount end) as 'Account 2 Sum',
	Sum(case when ftd.AccountId = @Account3Id then ftd.Amount end) as 'Account 3 Sum',
	Sum(case when ftd.AccountId = @Account4Id then ftd.Amount end) as 'Account 4 Sum',
	Sum(case when (ftd.AccountId != @Account1Id and
	 ftd.AccountId != @Account2Id and
	  ftd.AccountId != @Account3Id and
	   ftd.AccountId != @Account4Id  )then ftd.Amount end) as 'Other Sum',
	Sum(ftd.Amount) as 'Total Sum'
from FinancialTransaction ft
join FinancialTransactionDetail ftd on ftd.TransactionId = ft.Id
join PersonAlias pa on ft.AuthorizedPersonAliasId = pa.Id
join Person p on pa.PersonId = p.Id
Where ft.TransactionTypeValueId = @transactionTypeContributionId
and (@startDate is null or ft.TransactionDateTime >= @StartDate)
and ( @endDate is null or ft.TransactionDateTime < @EndDate)
and p.GivingId in (select * from @givingIds)
group by ft.TransactionCode, ft.TransactionDateTime, p.GivingId
order by TransactionDateTime

SELECT * FROM (
    SELECT 
		pg.GivingId		
		, [pt].[PersonNames] [AddressPersonNames]
		, [pn].[PersonNames] [GreetingPersonNames]
        , case when l.Id is null then 0 else 1 end [HasAddress]
		, [l].[Street1]
		, [l].[Street2]
		, [l].[City]
		, [l].[State]
		, [l].[PostalCode]
	FROM (
		-- Get distinct Giving Groups for Persons that have a specific GivingGroupId and have transactions that match the filter
		-- These are Persons that give as part of a Group.  For example, Husband and Wife
		SELECT DISTINCT
			null [PersonId] 
			, [g].[Id] [GroupId]
			,p.GivingId
		FROM  [Person] [p]
		INNER JOIN  [Group] [g] ON [p].[GivingGroupId] = [g].[Id]
        AND [p].GivingId in (SELECT GivingId FROM @givingIds)
		UNION
		-- Get Persons and their GroupId(s) that do not have GivingGroupId and have transactions that match the filter.        
		-- These are the persons that give as individuals vs as part of a group. We need the Groups (families they belong to) in order 
		-- to determine which address(es) the statements need to be mailed to 
		SELECT  
			[p].[Id] [PersonId],
			[g].[Id] [GroupId],
			p.givingId
		FROM [Person] [p]
		JOIN [GroupMember] [gm] ON [gm].[PersonId] = [p].[Id]
		JOIN [Group] [g] ON [gm].[GroupId] = [g].[Id]
		WHERE [p].[GivingGroupId] is null
		AND [g].[GroupTypeId] = (SELECT Id FROM GroupType WHERE [Guid] = @cGROUPTYPE_FAMILY)
		AND [p].GivingId IN (SELECT GivingId FROM @givingIds)
	) [pg]
	CROSS APPLY [ufnCrm_GetFamilyTitle]([pg].[PersonId], [pg].[GroupId], default, default) [pt]
	CROSS APPLY [ufnCrm_GetFamilyFirstOrNickNames]([pg].[PersonId], [pg].[GroupId], default, default) [pn]
	LEFT OUTER JOIN (
    SELECT l.*, gl.GroupId 
	from [GroupLocation] [gl] 
	LEFT OUTER JOIN [Location] [l] ON [l].[Id] = [gl].[LocationId]
	WHERE [gl].[IsMailingLocation] = 1
	AND [gl].[GroupLocationTypeValueId] = (SELECT Id FROM DefinedValue WHERE [Guid] = @cLOCATION_TYPE_HOME)
        ) [l] 
        ON [l].[GroupId] = [pg].[GroupId]
    ) n
    WHERE n.HasAddress = 1
    ORDER BY PostalCode
	OPTION (RECOMPILE);

END
            " );
        }
        public override void Down()
        {
        }
    }
}
