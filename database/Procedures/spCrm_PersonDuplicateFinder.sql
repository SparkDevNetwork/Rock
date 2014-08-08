/*
<doc>
	<summary>
 		This stored procedure detects potential duplicate person records and stores the results in [PersonDuplicate]
	</summary>
	
	<remarks>	
		Uses the following constants:
			##TODO##
	</remarks>
	<code>
		EXEC [dbo].[spCrm_PersonDuplicateFinder]
	</code>
</doc>
*/
ALTER PROCEDURE [dbo].[spCrm_PersonDuplicateFinder]
AS
BEGIN
    BEGIN TRANSACTION

    -- prevent stored proc from running simultaneously since we are using #temp tables
    EXEC sp_getapplock 'spCrm_PersonDuplicateFinder'
        ,'Exclusive'
        ,'Transaction'
        ,0

    -- Guids that this proc uses
    DECLARE @cGROUPTYPE_FAMILY_GUID UNIQUEIDENTIFIER = '790E3215-3B10-442B-AF69-616C0DCB998E'
        ,@cLOCATION_TYPE_HOME_GUID UNIQUEIDENTIFIER = '8C52E53C-2A66-435A-AE6E-5EE307D9A0DC'
        ,@cPHONENUMBER_DEFINEDTYPE_GUID UNIQUEIDENTIFIER = '8345DD45-73C6-4F5E-BEBD-B77FC83F18FD'
    -- 
    DECLARE @cScoreWeightEmail INT = 10
        ,@cScoreWeightName INT = 3
        ,@cScoreWeightPhoneNumber INT = 4
        ,@cScoreWeightAddress INT = 4
        ,@processDateTime DATETIME = SYSDATETIME()
        ,@cPHONENUMBER_DEFINEDTYPE_ID INT = (
            SELECT TOP 1 [Id]
            FROM [DefinedType]
            WHERE [Guid] = @cPHONENUMBER_DEFINEDTYPE_GUID
            )
        ,@cGROUPTYPE_FAMILY_ID INT = (
            SELECT TOP 1 [Id]
            FROM GroupType
            WHERE [Guid] = @cGROUPTYPE_FAMILY_GUID
            )
        ,@cLOCATION_TYPE_HOME_ID INT = (
            SELECT TOP 1 [Id]
            FROM DefinedValue
            WHERE [Guid] = @cLOCATION_TYPE_HOME_GUID
            )

    /*
    Populate Temporary Tables for each match criteria
    */
    -- Find Duplicates by looking at people with the exact same email address
    CREATE TABLE #PersonDuplicateByEmailTable (
        Id INT NOT NULL IDENTITY(1, 1)
        ,Email NVARCHAR(75) NOT NULL
        ,PersonAliasId INT NOT NULL
        ,CONSTRAINT [pk_PersonDuplicateByEmailTable] PRIMARY KEY CLUSTERED (Id)
        );

    INSERT INTO #PersonDuplicateByEmailTable
    SELECT [e].[Email] [Email]
        ,[pa].[Id] [PersonAliasId]
    FROM (
        SELECT [a].[Email]
        FROM (
            SELECT [Email]
                ,COUNT(*) [EmailCount]
            FROM [Person] [p]
            WHERE [Email] IS NOT NULL
                AND [Email] != ''
            GROUP BY [Email]
            ) [a]
        WHERE [a].[EmailCount] > 1
        ) [e]
    JOIN [Person] [p] ON [p].[Email] = [e].[Email]
    JOIN [PersonAlias] [pa] ON [pa].[PersonId] = [p].[Id]
    ORDER BY [Email]

    -- Find Duplicates by looking at people with the exact same lastname and same first 2 chars of First/Nick name
    CREATE TABLE #PersonDuplicateByNameTable (
        Id INT NOT NULL IDENTITY(1, 1)
        ,NameMatch NVARCHAR(152) NOT NULL
        ,PersonAliasId INT NOT NULL
        ,CONSTRAINT [pk_PersonDuplicateByNameTable] PRIMARY KEY CLUSTERED (Id)
        );

    INSERT INTO #PersonDuplicateByNameTable
    SELECT [e].[LastName] + ' ' + [e].[First2] [NameMatch]
        ,[pa].[Id] [PersonAliasId]
    FROM (
        SELECT [a].[First2]
            ,[a].[LastName]
        FROM (
            SELECT SUBSTRING([FirstName], 1, 2) [First2]
                ,[LastName]
                ,COUNT(*) [MatchCount]
            FROM [Person] [p]
            WHERE isnull([LastName], '') != ''
                AND [FirstName] IS NOT NULL
                AND LEN([FirstName]) >= 2
            GROUP BY [LastName]
                ,SUBSTRING([FirstName], 1, 2)
            ) [a]
        WHERE [a].[MatchCount] > 1
        ) [e]
    JOIN [Person] [p] ON [p].[LastName] = [e].[LastName]
        AND [p].[FirstName] LIKE (e.First2 + '%')
    JOIN [PersonAlias] [pa] ON [pa].[PersonId] = [p].[Id]
    ORDER BY [NameMatch]

    -- Find Duplicates by looking at people with the exact same phone number
    CREATE TABLE #PersonDuplicateByPhoneTable (
        Id INT NOT NULL IDENTITY(1, 1)
        ,PhoneNumberMatch NVARCHAR(max) NOT NULL
        ,NumberTypeValueId INT NOT NULL
        ,PersonAliasId INT NOT NULL
        ,CONSTRAINT [pk_PersonDuplicateByPhoneTable] PRIMARY KEY CLUSTERED (Id)
        );

    INSERT INTO #PersonDuplicateByPhoneTable
    SELECT [m].[Number] + isnull(',ex: ' + [m].[Extension], '') + isnull(',countrycode: ' + [m].[CountryCode], '') + ',Type:' + cast([m].[NumberTypeValueId] AS VARCHAR(max)) [PhoneNumberMatch]
        ,[m].[NumberTypeValueId]
        ,[pa].[Id] [PersonAliasId]
    FROM (
        SELECT DISTINCT [Number]
            ,[Extension]
            ,[CountryCode]
            ,[NumberTypeValueId]
        FROM (
            SELECT [pn].[Number]
                ,[pn].[Extension]
                ,[pn].[CountryCode]
                ,[pn].[NumberTypeValueId]
                ,[p].[Gender]
                ,COUNT(*) [MatchCount]
            FROM [PhoneNumber] [pn]
            JOIN [Person] [p] ON [p].[Id] = [pn].[PersonId]
            GROUP BY [pn].[Number]
                ,[pn].[Extension]
                ,[pn].[CountryCode]
                ,[pn].[NumberTypeValueId]
                ,[p].[Gender] -- only consider it a potential duplicate person if the gender and the phone number are the same
            ) [a]
        WHERE [a].[MatchCount] > 1
        ) [m]
    JOIN [PhoneNumber] [pn] ON [pn].[Number] = [m].[Number]
        AND ISNULL([pn].[Extension], '') = ISNULL([m].[Extension], '')
        AND ISNULL([pn].[CountryCode], '') = ISNULL([m].[CountryCode], '')
        AND [pn].[NumberTypeValueId] = [m].[NumberTypeValueId]
    JOIN [PersonAlias] [pa] ON [pa].[PersonId] = [pn].[PersonId]
    JOIN [Person] [p] ON [p].[Id] = [pa].[PersonId]
    ORDER BY [PhoneNumberMatch]

    -- Find Duplicates by looking at people with the exact same address (Location)
    CREATE TABLE #PersonDuplicateByAddressTable (
        Id INT NOT NULL IDENTITY(1, 1)
        ,LocationId INT NOT NULL
        ,Gender INT NOT NULL
        ,GroupRoleId INT NOT NULL
        ,PersonAliasId INT NOT NULL
        ,CONSTRAINT [pk_PersonDuplicateByAddressTable] PRIMARY KEY CLUSTERED (Id)
        );

    INSERT INTO #PersonDuplicateByAddressTable
    -- from the locations that have multiple potential duplicate persons, select the person records (along with gender and family role) that are associated with that location
    SELECT [m].[LocationId]
        ,[m].[Gender]
        ,[m].[GroupRoleId]
        ,[pa].[Id] [PersonAliasId]
    FROM (
        SELECT [LocationId]
            ,[Gender]
            ,[GroupRoleId]
            ,[MatchCount]
        FROM (
            -- consider it a potential duplicate person if multiple people have the same address (LocationId), Gender and GroupRole (adult, child)
            SELECT [gl].[LocationId]
                ,[gm].[GroupRoleId]
                ,[p].[Gender]
                ,COUNT(*) [MatchCount]
            FROM [Person] [p]
            JOIN [GroupMember] [gm] ON [gm].[PersonId] = [p].[Id]
            JOIN [Group] [g] ON [gm].[GroupId] = [g].[Id]
            JOIN [GroupLocation] [gl] ON [gl].[GroupId] = [g].[id]
            JOIN [Location] [l] ON [l].[Id] = [gl].[LocationId]
                AND [g].[GroupTypeId] = @cGROUPTYPE_FAMILY_ID
            WHERE [gl].[GroupLocationTypeValueId] = @cLOCATION_TYPE_HOME_ID
            GROUP BY [gl].[LocationID]
                ,[p].[Gender]
                ,[gm].[GroupRoleId]
            ) [a]
        WHERE [MatchCount] > 1
        ) [m]
    JOIN [GroupLocation] [gl] ON [gl].[LocationId] = [m].[LocationId]
    JOIN [Group] [g] ON [g].[Id] = [gl].[GroupId]
    JOIN [GroupMember] [gm] ON [gm].[GroupId] = [g].[Id]
    JOIN [PersonAlias] [pa] ON [gm].[PersonId] = [pa].[PersonId]

    /* 
    Reset Scores for everybody. (We want to preserve each record's [IsConfirmedAsNotDuplicate] value)
    */
    UPDATE [PersonDuplicate]
    SET [Score] = 0
        ,[ScoreDetail] = '';

    /*
    Merge Results of Matches into PersonDuplicate Table
    */
    -- Update PersonDuplicate table with results of email match
    MERGE [PersonDuplicate] AS TARGET
    USING (
        SELECT [e1].[PersonAliasId] [PersonAliasId]
            ,[e2].[PersonAliasId] [DuplicatePersonAliasId]
            ,[e1].[Email] [Email]
        FROM #PersonDuplicateByEmailTable [e1]
        JOIN #PersonDuplicateByEmailTable [e2] ON [e1].[email] = [e2].[email]
            AND [e1].[Id] != [e2].[Id]
        ) AS source(PersonAliasId, DuplicatePersonAliasId, Email)
        ON (target.PersonAliasId = source.PersonAliasId)
            AND (target.DuplicatePersonAliasId = source.DuplicatePersonAliasId)
    WHEN MATCHED
        THEN
            UPDATE
            SET [Score] = [Score] + @cScoreWeightEmail
                ,[ScoreDetail] = [ScoreDetail] + '| Email:' + source.Email
                ,[ModifiedDateTime] = @processDateTime
    WHEN NOT MATCHED
        THEN
            INSERT (
                PersonAliasId
                ,DuplicatePersonAliasId
                ,Score
                ,ScoreDetail
                ,IsConfirmedAsNotDuplicate
                ,ModifiedDateTime
                ,CreatedDateTime
                ,[Guid]
                )
            VALUES (
                source.PersonAliasId
                ,source.DuplicatePersonAliasId
                ,@cScoreWeightEmail
                ,'| Email:' + source.Email
                ,0
                ,@processDateTime
                ,@processDateTime
                ,NEWID()
                );

    -- Update PersonDuplicate table with results of name match
    MERGE [PersonDuplicate] AS TARGET
    USING (
        SELECT [e1].[PersonAliasId] [PersonAliasId]
            ,[e2].[PersonAliasId] [DuplicatePersonAliasId]
            ,[e1].[NameMatch] [NameMatch]
        FROM #PersonDuplicateByNameTable [e1]
        JOIN #PersonDuplicateByNameTable [e2] ON [e1].[NameMatch] = [e2].[NameMatch]
            AND [e1].[Id] != [e2].[Id]
        ) AS source(PersonAliasId, DuplicatePersonAliasId, NameMatch)
        ON (target.PersonAliasId = source.PersonAliasId)
            AND (target.DuplicatePersonAliasId = source.DuplicatePersonAliasId)
    WHEN MATCHED
        THEN
            UPDATE
            SET [Score] = [Score] + @cScoreWeightName
                ,[ScoreDetail] = [ScoreDetail] + '| NameMatch:' + source.NameMatch
                ,[ModifiedDateTime] = @processDateTime
    WHEN NOT MATCHED
        THEN
            INSERT (
                PersonAliasId
                ,DuplicatePersonAliasId
                ,Score
                ,ScoreDetail
                ,IsConfirmedAsNotDuplicate
                ,ModifiedDateTime
                ,CreatedDateTime
                ,[Guid]
                )
            VALUES (
                source.PersonAliasId
                ,source.DuplicatePersonAliasId
                ,@cScoreWeightName
                ,'| NameMatch:' + source.NameMatch
                ,0
                ,@processDateTime
                ,@processDateTime
                ,NEWID()
                );

    --  Update PersonDuplicate table with results of phonenumber match for each number type. 
    --  For example, if both the Cell and Home phone match, that should be a higher score 
    DECLARE @NumberTypeValueId INT

    DECLARE numberTypeCursor CURSOR FAST_FORWARD
    FOR
    SELECT [Id]
    FROM [DefinedValue]
    WHERE [DefinedTypeId] = @cPHONENUMBER_DEFINEDTYPE_ID;

    OPEN numberTypeCursor

    FETCH NEXT
    FROM numberTypeCursor
    INTO @NumberTypeValueId

    -- loop thru each of the phone number types (home, cell, work, etc)
    WHILE @@FETCH_STATUS = 0
    BEGIN
        MERGE [PersonDuplicate] AS TARGET
        USING (
            SELECT [e1].[PersonAliasId] [PersonAliasId]
                ,[e2].[PersonAliasId] [DuplicatePersonAliasId]
                ,[e1].[PhoneNumberMatch] [PhoneNumberMatch]
            FROM #PersonDuplicateByPhoneTable [e1]
            JOIN #PersonDuplicateByPhoneTable [e2] ON [e1].[PhoneNumberMatch] = [e2].[PhoneNumberMatch]
                AND [e1].[Id] != [e2].[Id]
                AND [e1].[NumberTypeValueId] = [e2].[NumberTypeValueId]
            WHERE [e1].[NumberTypeValueId] = @NumberTypeValueId
            ) AS source(PersonAliasId, DuplicatePersonAliasId, PhoneNumberMatch)
            ON (target.PersonAliasId = source.PersonAliasId)
                AND (target.DuplicatePersonAliasId = source.DuplicatePersonAliasId)
        WHEN MATCHED
            THEN
                UPDATE
                SET [Score] = [Score] + @cScoreWeightPhoneNumber
                    ,[ScoreDetail] = [ScoreDetail] + '| PhoneNumberMatch:' + source.PhoneNumberMatch
                    ,[ModifiedDateTime] = @processDateTime
        WHEN NOT MATCHED
            THEN
                INSERT (
                    PersonAliasId
                    ,DuplicatePersonAliasId
                    ,Score
                    ,ScoreDetail
                    ,IsConfirmedAsNotDuplicate
                    ,ModifiedDateTime
                    ,CreatedDateTime
                    ,[Guid]
                    )
                VALUES (
                    source.PersonAliasId
                    ,source.DuplicatePersonAliasId
                    ,@cScoreWeightPhoneNumber
                    ,'| PhoneNumberMatch:' + source.PhoneNumberMatch
                    ,0
                    ,@processDateTime
                    ,@processDateTime
                    ,NEWID()
                    );

        FETCH NEXT
        FROM numberTypeCursor
        INTO @NumberTypeValueId
    END

    CLOSE numberTypeCursor

    DEALLOCATE numberTypeCursor

    -- Update PersonDuplicate table with results of address (location) match
    MERGE [PersonDuplicate] AS TARGET
    USING (
        SELECT [e1].[PersonAliasId] [PersonAliasId]
            ,[e2].[PersonAliasId] [DuplicatePersonAliasId]
            ,[e1].[LocationId]
            ,[e1].[Gender]
            ,[e1].[GroupRoleId]
        FROM #PersonDuplicateByAddressTable [e1]
        JOIN #PersonDuplicateByAddressTable [e2] ON [e1].[LocationId] = [e2].[LocationId]
            AND [e1].[Gender] = [e2].[Gender]
            AND [e1].[GroupRoleId] = [e2].[GroupRoleId]
            AND [e1].[Id] != [e2].[Id]
        ) AS source(PersonAliasId, DuplicatePersonAliasId, LocationId, Gender, GroupRoleId)
        ON (target.PersonAliasId = source.PersonAliasId)
            AND (target.DuplicatePersonAliasId = source.DuplicatePersonAliasId)
    WHEN MATCHED
        THEN
            UPDATE
            SET [Score] = [Score] + @cScoreWeightPhoneNumber
                ,[ScoreDetail] = [ScoreDetail] + '| AddressMatch LocationId:' + cast(source.LocationId AS VARCHAR(max))
                ,[ModifiedDateTime] = @processDateTime
    WHEN NOT MATCHED
        THEN
            INSERT (
                PersonAliasId
                ,DuplicatePersonAliasId
                ,Score
                ,ScoreDetail
                ,IsConfirmedAsNotDuplicate
                ,ModifiedDateTime
                ,CreatedDateTime
                ,[Guid]
                )
            VALUES (
                source.PersonAliasId
                ,source.DuplicatePersonAliasId
                ,@cScoreWeightPhoneNumber
                ,'| AddressMatch LocationId:' + cast(source.LocationId AS VARCHAR(max))
                ,0
                ,@processDateTime
                ,@processDateTime
                ,NEWID()
                );

    /* 
    Clean up records that no longer are duplicates 
    */
    DECLARE @staleCount INT;

    SELECT @staleCount = count(*)
    FROM [PersonDuplicate]
    WHERE [ModifiedDateTime] < @processDateTime

    IF (@staleCount != 0)
    BEGIN
        DELETE
        FROM [PersonDuplicate]
        WHERE [ModifiedDateTime] < @processDateTime
    END

    /*
    Explicitly clean up temp tables before the proc exists (vs. have SQL Server do it for us after the proc is done)
    */
    DROP TABLE #PersonDuplicateByEmailTable;

    DROP TABLE #PersonDuplicateByNameTable;

    DROP TABLE #PersonDuplicateByPhoneTable;

    DROP TABLE #PersonDuplicateByAddressTable;

    COMMIT
END
GO

-- DEBUG
/*
SET STATISTICS TIME ON
SET STATISTICS IO ON
GO

EXEC [dbo].[spCrm_PersonDuplicateFinder]
GO

SELECT p1.FirstName + ' ' + p1.LastName [Person], p2.FirstName  + ' ' +  p2.LastName [DuplicatePerson], Score, ScoreDetail
FROM PersonDuplicate pd
JOIN PersonAlias pa1 ON pa1.Id = pd.PersonAliasId
JOIN PersonAlias pa2 ON pa2.Id = pd.DuplicatePersonAliasId
JOIN Person p1 on p1.Id = pa1.PersonId
JOIN Person p2 on p2.Id = pa2.PersonId
order by Score Desc
*/
