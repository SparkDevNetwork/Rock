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
	<param name='@GroupPersonIds' datatype='varchar(max)'>The Persons within the Group (family) to get the list of names for</param>
	<param name='@UseNickName' datatype='bit'>Determines if nickname (1) or firstname (0,default) is used in list of names</param>
	<param name='@IncludeInactive' datatype='bit'>Determines if list of names contains inactive records (1,default) or active records (0)</param>
	<remarks>
		[ufnCrm_GetFamilyTitleIncludeInactive] is called by [ufn_GetFamilyTitle] which is used by spFinance_ContributionStatementQuery as part of generating Contribution Statements
	</remarks>
	<code>
		SELECT * FROM [dbo].[ufnCrm_GetFamilyTitleIncludeInactive](2, null, default, default, default) -- Single Person
        SELECT * FROM [dbo].[ufnCrm_GetFamilyTitleIncludeInactive](null, 68, default, default, default) -- Family
        SELECT * FROM [dbo].[ufnCrm_GetFamilyTitleIncludeInactive](null, 68, '2,3', default, default) -- Family, limited to the specified PersonIds
	</code>
</doc>
*/

/* #Obsolete# - Family Title can be gotten from Group.GroupSalutation */
ALTER FUNCTION [dbo].[ufnCrm_GetFamilyTitleIncludeInactive] (
    @PersonId INT
    ,@GroupId INT
    ,@GroupPersonIds VARCHAR(max) = NULL
    ,@UseNickName BIT = 0
	,@IncludeInactive BIT = 1
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
	DECLARE @cPERSON_RECORD_STATUS_ACTIVE_ID INT = (SELECT TOP 1 ID FROM DefinedValue where [Guid] = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E');


    IF (@PersonId IS NOT NULL)
    BEGIN
        -- just getting the Person Names portion of the address for an individual person
        SELECT @PersonNames = CASE @UseNickName
                WHEN 1
                    THEN ISNULL([p].[NickName], '')
                ELSE ISNULL([p].[FirstName], '')
                END + ' ' + ISNULL([p].[LastName], '') + ISNULL(' ' + [dv].[Value], '')
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
                END + ' ' + ISNULL([p].[LastName], '') + ISNULL(' ' + [dv].[Value], '') [FullName]
            ,[p].Gender
            ,[gr].[Guid]
        FROM [GroupMember] [gm]
        JOIN [Person] [p] ON [p].[Id] = [gm].[PersonId]
        LEFT OUTER JOIN [DefinedValue] [dv] ON [dv].[Id] = [p].[SuffixValueId]
        JOIN [GroupTypeRole] [gr] ON [gm].[GroupRoleId] = [gr].[Id]
        WHERE [GroupId] = @GroupId
			AND (@IncludeInactive = 1 OR P.RecordStatusValueId = @cPERSON_RECORD_STATUS_ACTIVE_ID)
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
            SET @PersonNames = @GroupFirstOrNickNames + ' ' + @GroupLastName;
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