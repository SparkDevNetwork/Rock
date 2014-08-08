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

    DECLARE @cPhoneNumberDefinedTypeId int = (select top 1 id from DefinedType where Guid = '8345DD45-73C6-4F5E-BEBD-B77FC83F18FD')
        ,@cScoreWeightEmail INT = 10
        ,@cScoreWeightName INT = 3
        ,@cScoreWeightPhoneNumber INT = 4
        ,@processDateTime DATETIME = SYSDATETIME()

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
        ,NumberTypeValueId int not null
        ,PersonAliasId INT NOT NULL
        ,CONSTRAINT [pk_PersonDuplicateByPhoneTable] PRIMARY KEY CLUSTERED (Id)
        );

    INSERT INTO #PersonDuplicateByPhoneTable
    SELECT [m].[Number] + isnull(',ex: ' + [m].[Extension], '') + isnull(',countrycode: ' + [m].[CountryCode], '') + ',Type:' + cast([m].[NumberTypeValueId] AS VARCHAR(max)) [PhoneNumberMatch]
        ,[m].NumberTypeValueId
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

    /* 
    Reset Scores for everybody. Preserve records where [IsConfirmedAsNotDuplicate] 
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
    declare @NumberTypeValueId int
    declare numberTypeCursor cursor fast_forward for select id from DefinedValue where DefinedTypeId = @cPhoneNumberDefinedTypeId;
    open numberTypeCursor

    fetch next from numberTypeCursor into @NumberTypeValueId

    while @@FETCH_STATUS = 0
    begin
        MERGE [PersonDuplicate] AS TARGET
        USING (
            SELECT [e1].[PersonAliasId] [PersonAliasId]
                ,[e2].[PersonAliasId] [DuplicatePersonAliasId]
                ,[e1].[PhoneNumberMatch] [PhoneNumberMatch]
            FROM #PersonDuplicateByPhoneTable [e1]
            JOIN #PersonDuplicateByPhoneTable [e2] ON [e1].[PhoneNumberMatch] = [e2].[PhoneNumberMatch]
                AND [e1].[Id] != [e2].[Id] and [e1].[NumberTypeValueId] = [e2].[NumberTypeValueId]
                where [e1].[NumberTypeValueId] = @NumberTypeValueId
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
        fetch next from numberTypeCursor into @NumberTypeValueId
    end

    close numberTypeCursor
    deallocate numberTypeCursor

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

    DROP TABLE #PersonDuplicateByEmailTable;

    DROP TABLE #PersonDuplicateByNameTable;

    DROP TABLE #PersonDuplicateByPhoneTable;

    COMMIT
END
GO

-- DEBUG
/*
SET STATISTICS TIME ON
GO

EXEC [dbo].[spCrm_PersonDuplicateFinder]
GO
*/

