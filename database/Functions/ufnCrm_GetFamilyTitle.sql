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
    <param name='PersonId' datatype='int'>The Person to get a full name for. NULL means use the GroupId paramter </param>
	<param name='@GroupId' datatype='int'>The Group (family) to get the list of names for</param>
	<remarks>
		[ufnCrm_GetFamilyTitle] is used by spFinance_ContributionStatementQuery as part of generating Contribution Statements
	</remarks>
	<code>
		SELECT * FROM [dbo].[ufnCrm_GetFamilyTitle](2, null, default) -- Single Person
        SELECT * FROM [dbo].[ufnCrm_GetFamilyTitle](null, 44, default) -- Family
        SELECT * FROM [dbo].[ufnCrm_GetFamilyTitle](null, 44, '2,3') -- Family, limited to the specified PersonIds
	</code>
</doc>
*/
ALTER FUNCTION [dbo].[ufnCrm_GetFamilyTitle] 
( 
    @PersonId int
    , @GroupId int
    , @GroupPersonIds varchar(max) = null
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
    DECLARE @GroupMemberTable table (LastName varchar(max), FirstName varchar(max), FullName varchar(max), Gender int, GroupRoleGuid uniqueidentifier );
    DECLARE @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT uniqueidentifier = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42';

    IF (@PersonId is not null) 
    BEGIN
        -- just getting the Person Names portion of the address for an individual person
        SELECT 
            @PersonNames = ISNULL([p].[NickName],'') + ' ' + ISNULL([p].[LastName],'') + ' ' + ISNULL([dv].[Value], '')
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
        -- if GroupPersonIds is set (comma-delimited) only a subset of the family members should be combined
        INSERT INTO @GroupMemberTable 
        SELECT 
            [p].[LastName] 
            , [p].[FirstName] 
            , ISNULL([p].[NickName],'') + ' ' + ISNULL([p].[LastName],'') + ' ' + ISNULL([dv].[Value], '') as [FullName] 
            , [p].Gender
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
            [GroupId] = @GroupId
        AND 
            (ISNULL(@GroupPersonIds, '') = '' OR (p.[Id] in (select * from ufnUtility_CsvToTable(@GroupPersonIds))) )

        
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
                @GroupMemberTable g
            WHERE
                g.[GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
            ORDER BY g.Gender, g.FirstName

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