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
    public partial class StoredProcConsistency : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            @Sql( "DROP PROCEDURE spBinaryFileGet" );
            @Sql( "DROP PROCEDURE spContributionStatementQuery" );
            @Sql( "DROP FUNCTION ufnCsvToTable" );
            @Sql( "DROP FUNCTION ufnPersonGroupToPersonName" );

            @Sql( @"
/*
<doc>
	<summary>
 		This function converts a comma-delimited string of ints into a table of ints
        It comes from http://www.sql-server-helper.com/functions/comma-delimited-to-table
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
CREATE FUNCTION [dbo].[ufnUtility_CsvToTable] 
( 
    @Input varchar(max) 
)
RETURNS @OutputTable table ( [id] int )
AS
BEGIN
    DECLARE @Numericstring varchar(10)

    WHILE LEN(@Input) > 0
    BEGIN
        SET @Numericstring= LEFT(@input, ISNULL(NULLIF(CHARINDEX(',', @Input) - 1, -1), LEN(@Input)))
        SET @Input = SUBSTRING(@input,ISNULL(NULLIF(CHARINDEX(',', @Input), 0), LEN(@Input)) + 1, LEN(@Input))

        INSERT INTO @OutputTable ( [id] )
        VALUES ( CAST(@Numericstring as int) )
    END
    
    RETURN
END
" );

            @Sql( @"
/*
<doc>
	<summary>
 		This function returns either the FullName of the specified Person or a list of names of family members
        In the case of a group (family), it will return the names of the adults of the family. If there are no adults in the family, the names of the non-adults will be listed
        Example1 (specific person): Bob Smith 
        Example2 (family with kids): Bill and Sally Jones
        Example3 (different lastnames): Jim Jackson and Betty Sanders
        Example4 (just kids): Joey, George, and Jenny Swenson
	</summary>

	<returns>
		* Name(s)
	</returns>
    <param name=""PersonId"" datatype=""int"">The Person to get a full name for. NULL means use the GroupId paramter </param>
	<param name=""@GroupId"" datatype=""int"">The Group (family) to get the list of names for</param>
	<remarks>
		[ufnCrm_GetFamilyTitle] is used by spFinance_ContributionStatementQuery as part of generating Contribution Statements
	</remarks>
	<code>
		SELECT * FROM [dbo].[ufnCrm_GetFamilyTitle](2, null) -- Single Person
        SELECT * FROM [dbo].[ufnCrm_GetFamilyTitle](null, 3) -- Family
	</code>
</doc>
*/
CREATE FUNCTION [dbo].[ufnCrm_GetFamilyTitle] 
( 
    @PersonId int
    , @GroupId int
)
RETURNS @PersonNamesTable TABLE ( PersonNames varchar(max))
AS
BEGIN
    DECLARE @PersonNames varchar(max); 
    DECLARE @AdultLastNameCount int;
    DECLARE @GroupFirstNames varchar(max) = '';
    DECLARE @GroupLastName varchar(max);
    DECLARE @GroupAdultFullNames varchar(max) = '';
    DECLARE @GroupNonAdultFullNames varchar(max) = '';
    DECLARE @GroupMemberTable table (LastName varchar(max), FirstName varchar(max), FullName varchar(max), GroupRoleGuid uniqueidentifier );
    DECLARE @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT uniqueidentifier = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42';

    IF (@PersonId is not null) 
    BEGIN
        -- just getting the Person Names portion of the address for an individual person
        SELECT 
            @PersonNames = ISNULL([p].[NickName],'') + ' ' + ISNULL([p].[LastName],'') + ' ' + ISNULL([dv].[Name], '')
        FROM
            [Person] [p]
        LEFT OUTER JOIN 
            [DefinedValue] [dv] ON [dv].[Id] = [p].[SuffixValueId]
        WHERE 
            [p].[Id] = @PersonId;
    END
    ELSE
    BEGIN
        -- populate a table variable with the data we'll need for the different cases
        INSERT INTO @GroupMemberTable 
        SELECT 
            [p].[LastName] 
            , [p].[FirstName] 
            , ISNULL([p].[NickName],'') + ' ' + ISNULL([p].[LastName],'') + ' ' + ISNULL([dv].[Name], '') as [FullName] 
            , [gr].[Guid]
        FROM 
            [GroupMember] [gm] 
        JOIN 
            [Person] [p] 
        ON 
            [p].[Id] = [gm].[PersonId] 
        LEFT OUTER JOIN 
            [DefinedValue] [dv] ON [dv].[Id] = [p].[SuffixValueId]
        JOIN 
            [GroupTypeRole] [gr] ON [gm].[GroupRoleId] = [gr].[Id]
        WHERE 
            [GroupId] = @GroupId;
        
        -- determine adultCount and if we can use the same lastname for all adults, and get lastname while we are at it
        SELECT 
            @AdultLastNameCount = count(distinct [LastName])
            , @GroupLastName = max([LastName])
        FROM 
            @GroupMemberTable
        WHERE
            [GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT;  

        IF @AdultLastNameCount > 0 
        BEGIN
            -- get the FirstNames and Adult FullNames for use in the cases of families with Adults
            SELECT 
                @GroupFirstNames = @GroupFirstNames + [FirstName] + ' & '
                , @GroupAdultFullNames = @GroupAdultFullNames + [FullName] + ' & '
            FROM      
                @GroupMemberTable
            WHERE
                [GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT;

            -- cleanup the trailing ' &'s
            IF len(@GroupFirstNames) > 2 BEGIN
                -- trim the extra ' &' off the end 
                SET @GroupFirstNames = SUBSTRING(@GroupFirstNames, 0, len(@GroupFirstNames) - 1)
            END 

            IF len(@GroupAdultFullNames) > 2 BEGIN
                -- trim the extra ' &' off the end 
                SET @GroupAdultFullNames = SUBSTRING(@GroupAdultFullNames, 0, len(@GroupAdultFullNames) - 1)  
            END
        END             

        IF @AdultLastNameCount = 0        
        BEGIN
            -- get the NonAdultFullNames for use in the case of families without adults 
            SELECT 
                @GroupNonAdultFullNames = @groupNonAdultFullNames + [FullName] + ' & '
            FROM 
                @GroupMemberTable
            ORDER BY [FullName]

            IF len(@GroupNonAdultFullNames) > 2 BEGIN
                -- trim the extra ' &' off the end 
                SET @GroupNonAdultFullNames = SUBSTRING(@GroupNonAdultFullNames, 0, len(@GroupNonAdultFullNames) - 1)  
            END
        END

        IF (@AdultLastNameCount = 1)
        BEGIN
            -- just one lastname and at least one adult. Get the Person Names portion of the address in the format <MaleAdult> & <FemaleAdult> <LastName>
            SET @PersonNames = @GroupFirstNames + ' ' + @GroupLastName;
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

    INSERT INTO @PersonNamesTable ( [PersonNames] ) VALUES ( @PersonNames);

  RETURN
END
" );

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
        * Zip
        * StartDate
        * EndDate
        * CustomMessage1
        * CustomMessage2
	</returns>
	<param name=""StartDate"" datatype=""datetime"">The starting date of the date range</param>
    <param name=""EndDate"" datatype=""datetime"">The ending date of the date range</param>
	<param name=""AccountIds"" datatype=""varchar(max)"">Comma delimited list of account ids. NULL means all</param>
	<param name=""PersonId"" datatype=""int"">Person the statement if for. NULL means all persons that have transactions for the date range</param>
	<param name=""OrderByZipCode"" datatype=""int"">Set to 1 to have the results sorted by ZipCode, 0 for no particular order</param>
	<remarks>	
		Uses the following constants:
			* Group Type - Family: 790E3215-3B10-442B-AF69-616C0DCB998E
			* Group Role - Adult: 2639F9A5-2AAE-4E48-A8C3-4FFE86681E42
			* Group Role - Child: C8B1814F-6AA7-4055-B2D7-48FE20429CB9
	</remarks>
	<code>
		EXEC [dbo].[spFinance_ContributionStatementQuery] '01-01-2014', '01-01-2015', null, null, 1  -- year 2014 statements for all persons
	</code>
</doc>
*/
CREATE PROCEDURE [spFinance_ContributionStatementQuery]
	@StartDate datetime
    , @EndDate datetime
    , @AccountIds varchar(max) 
    , @PersonId int -- NULL means all persons
    , @OrderByZipCode bit
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
            [AuthorizedPersonId] 
        FROM 
            [FinancialTransaction] [ft]
        INNER JOIN 
            [FinancialTransactionDetail] [ftd] ON [ft].[Id] = [ftd].[TransactionId]
        WHERE 
            ([TransactionDateTime] >= @StartDate and [TransactionDateTime] < @EndDate)
        AND 
            (
                (@AccountIds is null)
                OR
                (ftd.[AccountId] in (select * from ufnUtility_CsvToTable(@AccountIds)))
            )
    )

    SELECT 
        [pg].[PersonId]
        , [pg].[GroupId]
        , [pn].[PersonNames] [AddressPersonNames]
        , [l].[Street1]
        , [l].[Street2]
        , [l].[City]
        , [l].[State]
        , [l].[Zip]
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
        AND [p].[Id] IN (SELECT * FROM tranListCTE)
    ) [pg]
    CROSS APPLY 
        [ufnCrm_GetFamilyTitle]([pg].[PersonId], [pg].[GroupId]) [pn]
    JOIN 
        [GroupLocation] [gl] 
    ON 
        [gl].[GroupId] = [pg].[GroupId]
    JOIN
        [Location] [l]
    ON 
        [l].[Id] = [gl].[LocationId]
    WHERE 
        [gl].[IsMailingLocation] = 1
    AND
        [gl].[GroupLocationTypeValueId] = (SELECT Id FROM DefinedValue WHERE [Guid] = @cLOCATION_TYPE_HOME)
    AND
        (
            (@personId is null) 
        OR 
            ([pg].[PersonId] = @personId)
        )
    ORDER BY
    CASE WHEN @orderByZipCode = 1 THEN Zip END
END

" );

            Sql( @"
-- create stored proc that retrieves a binaryfile record
/*
<doc>
	<summary>
 		This function returns the BinaryFile for a given Id or Guid, depending on which is specified
	</summary>

	<returns>
		* BinaryFile record
	</returns>
	<param name=""Id"" datatype=""int"">The binary id to use</param>
	<param name=""Guid"" datatype=""uniqueidentifier"">The binaryfile guid to use</param>
	<remarks>	
	</remarks>
	<code>
		EXEC [dbo].[spCore_BinaryFileGet] 14, null -- car-promo.jpg
	</code>
</doc>
*/
CREATE PROCEDURE [dbo].[spCore_BinaryFileGet]
    @Id int
    , @Guid uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    /* NOTE!: Column Order cannot be changed without changing BinaryFileService.partial.cs due to CommandBehavior.SequentialAccess */
    SELECT 
        bf.[Id]
        , bf.[IsTemporary] 
        , bf.[IsSystem]
        , bf.[BinaryFileTypeId]
        , bf.[Url]
        , bf.[FileName] 
        , bf.[MimeType]
        , bf.[ModifiedDateTime]
        , bf.[Description]
        , bf.[StorageEntityTypeId]
        , bf.[Guid]
        /* if the BinaryFile as StorageEntityTypeId set, use that. Otherwise use the default StorageEntityTypeId from BinaryFileType  */
        , COALESCE (bfse.[Name],bftse.[Name] ) as [StorageEntityTypeName]
        , bfd.[Content]
    FROM 
        [BinaryFile] bf 
    LEFT JOIN 
        [BinaryFileData] bfd ON bf.[Id] = bfd.[Id]
    LEFT JOIN 
        [EntityType] bfse ON bf.[StorageEntityTypeId] = bfse.[Id]
    LEFT JOIN 
        [BinaryFileType] bft ON bf.[BinaryFileTypeId] = bft.[Id]
    LEFT JOIN 
        [EntityType] bftse ON bft.[StorageEntityTypeId] = bftse.[Id]
    WHERE 
        (@Id > 0 and bf.[Id] = @Id)
        or
        (bf.[Guid] = @Guid)
END

" );


        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            @Sql( "DROP PROCEDURE spCore_BinaryFileGet" );
            @Sql( "DROP PROCEDURE spFinance_ContributionStatementQuery" );
            @Sql( "DROP FUNCTION ufnUtility_CsvToTable" );
            @Sql( "DROP FUNCTION ufnCrm_GetFamilyTitle" );

            @Sql( @"
CREATE PROCEDURE [spBinaryFileGet]
    @Id int,
    @Guid uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    /* NOTE!: Column Order cannot be changed without changing BinaryFileService.partial.cs due to CommandBehavior.SequentialAccess */
    SELECT 
        bf.Id,
        bf.IsTemporary, 
        bf.IsSystem,
        bf.BinaryFileTypeId,
        bf.Url,
        bf.[FileName], 
        bf.MimeType,
        bf.ModifiedDateTime,
        bf.[Description],
        bf.StorageEntityTypeId,
        bf.[Guid],
        /* if the BinaryFile as StorageEntityTypeId set, use that. Otherwise use the default StorageEntityTypeId from BinaryFileType  */
        COALESCE (bfse.Name,bftse.Name ) as StorageEntityTypeName,
        bfd.Content
    FROM BinaryFile bf 
    LEFT JOIN BinaryFileData bfd
        ON bf.Id = bfd.Id
    LEFT JOIN EntityType bfse
        ON bf.StorageEntityTypeId = bfse.Id
    LEFT JOIN BinaryFileType bft
        on bf.BinaryFileTypeId = bft.Id
    LEFT JOIN EntityType bftse
        ON bft.StorageEntityTypeId = bftse.Id
    WHERE 
        (@Id > 0 and bf.Id = @Id)
        or
        (bf.[Guid] = @Guid)
END
" );

            Sql( @"
--
-- from http://www.sql-server-helper.com/functions/comma-delimited-to-table
--

create function [dbo].[ufnCsvToTable] ( @input varchar(max) )
returns @outputTable table ( [id] int )
as
begin
    declare @numericstring varchar(10)

    while LEN(@input) > 0
    begin
        set @numericString      = LEFT(@input, 
                                ISNULL(NULLIF(CHARINDEX(',', @input) - 1, -1),
                                LEN(@input)))
        set @input = SUBSTRING(@input,
                                     ISNULL(NULLIF(CHARINDEX(',', @input), 0),
                                     LEN(@input)) + 1, LEN(@input))

        insert into @OutputTable ( [id] )
        values ( CAST(@numericString as int) )
    end
    
    return
end
" );

            Sql( @"
create function [dbo].[ufnPersonGroupToPersonName] 
( 
@personId int, -- NULL means generate person names from Group Members. NOT-NULL means just get FullName from Person
@groupId int
)
returns @personNamesTable table ( PersonNames varchar(max))
as
begin
    declare @personNames varchar(max); 
    declare @adultLastNameCount int;
    declare @groupFirstNames varchar(max) = '';
    declare @groupLastName varchar(max);
    declare @groupAdultFullNames varchar(max) = '';
    declare @groupNonAdultFullNames varchar(max) = '';
    declare @groupMemberTable table (LastName varchar(max), FirstName varchar(max), FullName varchar(max), GroupRoleGuid uniqueidentifier );
    declare @GROUPTYPEROLE_FAMILY_MEMBER_ADULT uniqueidentifier = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42';

    if (@personId is not null) 
    begin
        -- just getting the Person Names portion of the address for an individual person
        select @personNames = ISNULL([p].[NickName],'') + ' ' + ISNULL([p].[LastName],'') + ' ' + ISNULL([dv].[Name], '')
        from [Person] [p]
        left outer join [DefinedValue] [dv]
        on [dv].[Id] = [p].[SuffixValueId]
        where [p].[Id] = @personId;
    end
    else
    begin
        -- populate a table variable with the data we'll need for the different cases
        insert into @groupMemberTable 
        select 
            [p].[LastName], 
            [p].[FirstName], 
            ISNULL([p].[NickName],'') + ' ' + ISNULL([p].[LastName],'') + ' ' + ISNULL([dv].[Name], '') as [FullName], 
            [gr].[Guid]
        from 
            [GroupMember] [gm] 
        join 
            [Person] [p] 
        on 
            [p].[Id] = [gm].[PersonId] 
        left outer join 
            [DefinedValue] [dv]
        on
            [dv].[Id] = [p].[SuffixValueId]
        join
            [GroupTypeRole] [gr]
        on 
            [gm].[GroupRoleId] = [gr].[Id]
        where 
            [GroupId] = @groupId;
        
        -- determine adultCount and if we can use the same lastname for all adults, and get lastname while we are at it
        select 
            @adultLastNameCount = count(distinct [LastName])
            ,@groupLastName = max([LastName])
        from 
            @groupMemberTable
        where
            [GroupRoleGuid] = @GROUPTYPEROLE_FAMILY_MEMBER_ADULT;  

        if @adultLastNameCount > 0 
        begin
            -- get the FirstNames and Adult FullNames for use in the cases of families with Adults
            select 
                @groupFirstNames = @groupFirstNames + [FirstName] + ' & '
                ,@groupAdultFullNames = @groupAdultFullNames + [FullName] + ' & '
            from      
                @groupMemberTable
            where
                [GroupRoleGuid] = @GROUPTYPEROLE_FAMILY_MEMBER_ADULT;

            -- cleanup the trailing ' &'s
            if len(@groupFirstNames) > 2 begin
                -- trim the extra ' &' off the end 
                set @groupFirstNames = SUBSTRING(@groupFirstNames, 0, len(@groupFirstNames) - 1)
            end 

            if len(@groupAdultFullNames) > 2 begin
                -- trim the extra ' &' off the end 
                set @groupAdultFullNames = SUBSTRING(@groupAdultFullNames, 0, len(@groupAdultFullNames) - 1)  
            end
        end             

        if @adultLastNameCount = 0        
        begin
            -- get the NonAdultFullNames for use in the case of families without adults 
            select 
                @groupNonAdultFullNames = @groupNonAdultFullNames + [FullName] + ' & '
            from 
                @groupMemberTable
            order by [FullName]

            if len(@groupNonAdultFullNames) > 2 begin
                -- trim the extra ' &' off the end 
                set @groupNonAdultFullNames = SUBSTRING(@groupNonAdultFullNames, 0, len(@groupNonAdultFullNames) - 1)  
            end
        end

        if (@adultLastNameCount = 1)
        begin
            -- just one lastname and at least one adult. Get the Person Names portion of the address in the format <MaleAdult> & <FemaleAdult> <LastName>
            set @personNames = @groupFirstNames + ' ' + @groupLastName;
        end
        else if (@adultLastNameCount = 0)
        begin
             -- no adults in family, list all members of the family in 'Fullname & FullName & ...' format
            set @personNames = @groupNonAdultFullNames;
        end
        else
        begin
            -- multiple adult lastnames
            set @personNames = @groupAdultFullNames;
        end 
    end

    insert into @personNamesTable ( [PersonNames] ) values ( @personNames);

  return
end
" );

            Sql( @"
create procedure [spContributionStatementQuery]
	@startDate datetime,
    @endDate datetime,
    @accountIds varchar(max), -- comma delimited list of integers. NULL means all
    @personId int, -- NULL means all persons
    @orderByZipCode bit
as
begin
	/*  Note:  This stored procedure returns the Mailing Addresses and any CustomMessages for the Contribution Statement, but not the actual transactions */

    -- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	set nocount on;

    ;with tranListCTE
    as
    (
        select  
            [AuthorizedPersonId] 
         from 
            [FinancialTransaction] [ft]
         inner join [FinancialTransactionDetail] [ftd]
         on [ft].[Id] = [ftd].[TransactionId]
         where 
            ([TransactionDateTime] >= @startDate and [TransactionDateTime] < @endDate)
         and 
            (
                (@accountIds is null)
                or
                (ftd.AccountId in (select * from ufnCsvToTable(@accountIds)))
            )
    )

    select 
        [pg].[PersonId],
        [pg].[GroupId],
        [pn].[PersonNames] [AddressPersonNames],
        [l].[Street1],
        [l].[Street2],
        [l].[City],
        [l].[State],
        [l].[Zip],
        @startDate [StartDate],
        @endDate [EndDate],
        null [CustomMessage1],
        null [CustomMessage2]
    from (
        -- Get distinct Giving Groups for Persons that have a specific GivingGroupId and have transactions that match the filter
        -- These are Persons that give as part of a Group.  For example, Husband and Wife
        select distinct
            null [PersonId], 
            [g].[Id] [GroupId]
        from 
            [Person] [p]
        inner join 
            [Group] [g]
        on 
            [p].[GivingGroupId] = [g].[Id]
        where [p].[Id] in (select * from tranListCTE)
        union
        -- Get Persons and their GroupId(s) that do not have GivingGroupId and have transactions that match the filter.        
        -- These are the persons that give as individuals vs as part of a group. We need the Groups (families they belong to) in order 
        -- to determine which address(es) the statements need to be mailed to 
        select  
            [p].[Id] [PersonId],
            [g].[Id] [GroupId]
        from
            [Person] [p]
        join 
            [GroupMember] [gm]
        on 
            [gm].[PersonId] = [p].[Id]
        join 
            [Group] [g]
        on 
            [gm].[GroupId] = [g].[Id]
        where
            [p].[GivingGroupId] is null
        and
            [g].[GroupTypeId] = (select Id from GroupType where Guid = '790E3215-3B10-442B-AF69-616C0DCB998E' /* GROUPTYPE_FAMILY */)
        and [p].[Id] in (select * from tranListCTE)
    ) [pg]
    cross apply [ufnPersonGroupToPersonName]([pg].[PersonId], [pg].[GroupId]) [pn]
    join 
        [GroupLocation] [gl] 
    on 
        [gl].[GroupId] = [pg].[GroupId]
    join
        [Location] [l]
    on 
        [l].[Id] = [gl].[LocationId]
    where 
        [gl].[IsMailingLocation] = 1
    and
        [gl].[GroupLocationTypeValueId] = (select Id from DefinedValue where Guid = '8C52E53C-2A66-435A-AE6E-5EE307D9A0DC' /* LOCATION_TYPE_HOME */)
    and
        (
            (@personId is null) 
        or 
            ([pg].[PersonId] = @personId)
        )
    order by
    case when @orderByZipCode = 1 then Zip end
end

" );
        }
    }
}
