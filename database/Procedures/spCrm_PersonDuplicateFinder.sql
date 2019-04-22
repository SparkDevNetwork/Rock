/*
<doc>
 <summary>
   This stored procedure detects potential duplicate person records and stores the results in [PersonDuplicate]
 </summary>
 
 <remarks> 
  Uses the following constants:
   * Group Type - Family: '790E3215-3B10-442B-AF69-616C0DCB998E'
            * Location Type - Home: '8C52E53C-2A66-435A-AE6E-5EE307D9A0DC'
            * Phone Type - Home: 'AA8732FB-2CEA-4C76-8D6D-6AAA2C6A4303'
            * Phone Type - Cell: '407E7E45-7B2E-4FCD-9605-ECB1339F2453'
 </remarks>
 <code>
  EXEC [dbo].[spCrm_PersonDuplicateFinder]
 </code>
</doc>
*/
ALTER PROCEDURE [dbo].[spCrm_PersonDuplicateFinder]
AS
BEGIN
    
 SET NOCOUNT ON
 -- DECLARE @ms DATETIME = GETDATE()
    -- Flags that enable the various compare functions
    DECLARE @compareByEmail BIT = 1
        ,@compareByPartialName BIT = 1
        ,@compareByFullFirstName BIT = 1
        ,@compareByFullLastName BIT = 1
        ,@compareByPhone BIT = 1
        ,@compareByAddress BIT = 1
        ,@compareByBirthDate BIT = 1
        ,@compareByGender BIT = 1
        ,@compareByCampus BIT = 1
        ,@compareByMaritalStatus BIT = 1
        ,@compareBySuffix BIT = 1
    --
    -- Scores
    -- ones marked /**/ only added to score if already a potential match
    DECLARE @cScoreWeightEmail INT = 4
        ,@cScoreWeightPartialName INT = 1
        ,@cScoreWeightFullFirstName INT = 3 /**/
        ,@cScoreWeightFullLastName INT = 3 /**/
        ,@cScoreWeightCellPhoneNumber INT = 4
        ,@cScoreWeightNonCellPhoneNumber INT = 2
        ,@cScoreWeightAddress INT = 2
        ,@cScoreWeightBirthdate INT = 3 /**/
        ,@cScoreWeightGender INT = 1 /**/
        ,@cScoreWeightCampus INT = 1 /**/
        ,@cScoreWeightMaritalStatus INT = 1 /**/
        ,@cScoreWeightSuffix INT = 4
    DECLARE @TotalCapacity INT = @cScoreWeightEmail + @cScoreWeightPartialName + @cScoreWeightFullFirstName + @cScoreWeightFullLastName + @cScoreWeightCellPhoneNumber + @cScoreWeightNonCellPhoneNumber + @cScoreWeightAddress + @cScoreWeightBirthdate + @cScoreWeightGender + @cScoreWeightCampus + @cScoreWeightMaritalStatus + @cScoreWeightSuffix
    --
    -- Guids that this proc uses
    DECLARE @cGROUPTYPE_FAMILY_GUID UNIQUEIDENTIFIER = '790E3215-3B10-442B-AF69-616C0DCB998E'
        ,@cLOCATION_TYPE_HOME_GUID UNIQUEIDENTIFIER = '8C52E53C-2A66-435A-AE6E-5EE307D9A0DC'
        ,@cHOME_PHONENUMBER_DEFINEDVALUE_GUID UNIQUEIDENTIFIER = 'AA8732FB-2CEA-4C76-8D6D-6AAA2C6A4303'
        ,@cCELL_PHONENUMBER_DEFINEDVALUE_GUID UNIQUEIDENTIFIER = '407E7E45-7B2E-4FCD-9605-ECB1339F2453'
    --
    -- Other Declarations
    DECLARE @processDateTime DATETIME = SYSDATETIME()
        ,@cHOME_PHONENUMBER_DEFINEDVALUE_ID INT = (
            SELECT TOP 1 [Id]
            FROM [DefinedValue]
            WHERE [Guid] = @cHOME_PHONENUMBER_DEFINEDVALUE_GUID
            )
        ,@cCELL_PHONENUMBER_DEFINEDVALUE_ID INT = (
            SELECT TOP 1 [Id]
            FROM [DefinedValue]
            WHERE [Guid] = @cCELL_PHONENUMBER_DEFINEDVALUE_GUID
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
 --PRINT'Declare Variables: ' + CAST(DATEDIFF(s, @ms, GETDATE()) as varchar)
 --SET @ms = GETDATE()
    /*
    Populate Temporary Tables for each match criteria (Email, PartialName, PhoneNumber, Address)
    */
    -- Find Duplicates by looking at people with the exact same email address
    CREATE TABLE #PersonDuplicateByEmailTable (
         Email NVARCHAR(75) NOT NULL
        ,PersonAliasId INT NOT NULL
        ,PRIMARY KEY CLUSTERED (Email, PersonAliasId)
        );
    INSERT INTO #PersonDuplicateByEmailTable (
        Email
        ,PersonAliasId
        )
    SELECT [e].[Email] [Email]
        ,[pa].[Id] [PersonAliasId]
    FROM (
        SELECT [a].[Email]
        FROM (
            SELECT [Email]
                ,COUNT(*) [EmailCount]
            FROM [Person] [p]
            WHERE isnull([Email], '') != ''
            GROUP BY [Email]
            ) [a]
        WHERE [a].[EmailCount] > 1
        ) [e]
    JOIN [Person] [p] ON [p].[Email] = [e].[Email]
    JOIN [PersonAlias] [pa] ON [pa].[PersonId] = [p].[Id]
    WHERE [pa].[AliasPersonId] = [pa].[PersonId] -- limit to only the primary alias
        AND @compareByEmail = 1
 --PRINT'Create Email Dups: ' + CAST(DATEDIFF(s, @ms, GETDATE()) as varchar)
 --SET @ms = GETDATE()
    -- Find Duplicates by looking at people with the exact same lastname and same first 2 chars of First/Nick name
    CREATE TABLE #PersonDuplicateByNameTable (
         First2 NVARCHAR(50) NOT NULL -- intentionally 50 vs 2 for performance reasons (sql server spends time on the length constraint if it's shorter than the source column)
        ,LastName NVARCHAR(50) NOT NULL
        ,PersonAliasId INT NOT NULL
        ,PRIMARY KEY CLUSTERED (First2, LastName, PersonAliasId)
        );
    INSERT INTO #PersonDuplicateByNameTable (
         First2
        ,LastName
        ,PersonAliasId
        )
    SELECT [e].[LastName]
        ,[e].[First2]
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
    WHERE [pa].[AliasPersonId] = [pa].[PersonId] -- limit to only the primary alias
        AND @compareByPartialName = 1
 --PRINT'Create Name Dups: ' + CAST(DATEDIFF(s, @ms, GETDATE()) as varchar)
 --SET @ms = GETDATE()
    -- Find Duplicates by looking at people with the exact same phone number
    CREATE TABLE #PersonDuplicateByPhoneTable (
         Number NVARCHAR(20) NOT NULL
        ,Extension NVARCHAR(20) NOT NULL
        ,CountryCode NVARCHAR(3) NOT NULL
        ,NumberTypeValueId INT NOT NULL
  ,GroupId INT NOT NULL
        ,PersonAliasId INT NOT NULL
        ,PRIMARY KEY CLUSTERED (Number, Extension, CountryCode, NumberTypeValueId, GroupId, PersonAliasId)
        );
    INSERT INTO #PersonDuplicateByPhoneTable (
        Number
        ,Extension
        ,CountryCode
        ,NumberTypeValueId
  ,GroupId
        ,PersonAliasId
        )
    SELECT DISTINCT [m].[Number]
        ,isnull([m].[Extension], '')
        ,isnull([m].[CountryCode], '')
        ,[m].[NumberTypeValueId]
  ,[g].[Id]
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
                ,COUNT(*) [MatchCount]
            FROM [PhoneNumber] [pn]
            JOIN [Person] [p] ON [p].[Id] = [pn].[PersonId]
            WHERE ISNUMERIC([pn].[Number]) = 1
                AND [pn].[NumberTypeValueId] IN (
                    @cHOME_PHONENUMBER_DEFINEDVALUE_ID
                    ,@cCELL_PHONENUMBER_DEFINEDVALUE_ID
                    )
            GROUP BY [pn].[Number]
                ,[pn].[Extension]
                ,[pn].[CountryCode]
                ,[pn].[NumberTypeValueId]
             ,[p].[Gender]
            ) [a]
        WHERE [a].[MatchCount] > 1
        ) [m]
    JOIN [PhoneNumber] [pn] ON [pn].[Number] = [m].[Number]
        AND ISNULL([pn].[Extension], '') = ISNULL([m].[Extension], '')
        AND ISNULL([pn].[CountryCode], '') = ISNULL([m].[CountryCode], '')
        AND [pn].[NumberTypeValueId] = [m].[NumberTypeValueId]
    JOIN [PersonAlias] [pa] ON [pa].[PersonId] = [pn].[PersonId]
    JOIN [GroupMember] [gm] ON [gm].[PersonId] = [pa].[PersonId]
    JOIN [Group] [g] ON [gm].[GroupId] = [g].[Id]
    WHERE [pa].[AliasPersonId] = [pa].[PersonId] -- limit to only the primary alias
  AND [g].[GroupTypeId] = @cGROUPTYPE_FAMILY_ID
        AND @compareByPhone = 1
 --PRINT'Create Phone Dups: ' + CAST(DATEDIFF(s, @ms, GETDATE()) as varchar)
 --SET @ms = GETDATE()
    -- Find Duplicates by looking at people with the exact same address (Location)
    CREATE TABLE #PersonDuplicateByAddressTable (
         LocationId INT NOT NULL
        ,GroupRoleId INT NOT NULL
  ,GroupId INT NOT NULL
        ,PersonAliasId INT NOT NULL
        ,PRIMARY KEY CLUSTERED (LocationId, GroupRoleId, GroupId, PersonAliasId)
        );
    INSERT INTO #PersonDuplicateByAddressTable (
        LocationId
        ,GroupRoleId
  ,GroupId
        ,PersonAliasId
        )
    -- from the locations that have multiple potential duplicate persons, select the person records (along with gender and family role) that are associated with that location
    -- in case the person has multiple home addresses, get the first one with preference given to MapLocation=true
    SELECT CASE 
            WHEN a.MaxMappedLocationId > 0
                THEN a.MaxMappedLocationId
            ELSE a.MaxNotMappedLocationId
            END [LocationId]
        ,[GroupRoleId]
        ,[GroupId]
        ,[PersonAliasId]
    FROM (
        SELECT max(CASE 
                    WHEN [IsMappedLocation] = 1
                        THEN [LocationId]
                    ELSE 0
                    END) [MaxMappedLocationId]
            ,max(CASE 
                    WHEN [IsMappedLocation] = 0
                        THEN [LocationId]
                    ELSE 0
                    END) [MaxNotMappedLocationId]
            ,[GroupRoleId]
   ,[GroupId]
            ,[PersonAliasId]
        FROM (
            SELECT [m].[LocationId]
                ,[gl].[IsMappedLocation]
                ,[m].[GroupRoleId]
    ,[g].[id] [GroupId]
                ,[pa].[Id] [PersonAliasId]
            FROM (
                SELECT [LocationId]
                    ,[GroupRoleId]
                    ,[MatchCount]
                FROM (
                    -- consider it a potential duplicate person if multiple people have the same address (LocationId), Gender and GroupRole (adult, child)
                    SELECT [gl].[LocationId]
                        ,[gm].[GroupRoleId]
                        ,COUNT(*) [MatchCount]
                    FROM [GroupMember] [gm] 
                    JOIN [Group] [g] ON [gm].[GroupId] = [g].[Id]
                    JOIN [GroupLocation] [gl] ON [gl].[GroupId] = [g].[id]
                    JOIN [Location] [l] ON [l].[Id] = [gl].[LocationId]
     WHERE [g].[GroupTypeId] = @cGROUPTYPE_FAMILY_ID
                    AND [gl].[GroupLocationTypeValueId] = @cLOCATION_TYPE_HOME_ID
                    GROUP BY [gl].[LocationID]
                        ,[gm].[GroupRoleId]
                    ) [a]
                WHERE [MatchCount] > 1
                ) [m]
            JOIN [GroupLocation] [gl] ON [gl].[LocationId] = [m].[LocationId]
            JOIN [Group] [g] ON [g].[Id] = [gl].[GroupId]
            JOIN [GroupMember] [gm] ON [gm].[GroupId] = [g].[Id]
            JOIN [PersonAlias] [pa] ON [gm].[PersonId] = [pa].[PersonId]
            JOIN [Person] [p] ON [p].[Id] = [pa].[PersonId]
            WHERE [m].[GroupRoleId] = [gm].[GroupRoleId] -- only consider it a potential duplicate person if the gender,role and location are the same (to reduce false matches due to married couples)
                AND [pa].[AliasPersonId] = [pa].[PersonId] -- limit to only the primary alias
                AND @compareByAddress = 1
            ) [a]
        GROUP BY [PersonAliasId]
            ,[GroupRoleId]
   ,[GroupId]
        ) a
 --PRINT'Create Address Dups: ' + CAST(DATEDIFF(s, @ms, GETDATE()) as varchar)
 --SET @ms = GETDATE()
    -- get the original ConfidenceScore of the IgnoreUntilScoreChanges records so that we can un-ignore the ones that have a changed score
    DECLARE @IgnoreUntilScoreChangesTable TABLE (
        Id INT
        ,ConfidenceScore FLOAT
        );
    INSERT INTO @IgnoreUntilScoreChangesTable
    SELECT [Id]
        ,[ConfidenceScore]
    FROM [PersonDuplicate]
    WHERE [IgnoreUntilScoreChanges] = 1;
 --PRINT'Create Ignore Score Changes: ' + CAST(DATEDIFF(s, @ms, GETDATE()) as varchar)
 --SET @ms = GETDATE()
    /* 
    Reset Scores for everybody. (We want to preserve each record's [IsConfirmedAsNotDuplicate] value)
    */
    UPDATE [PersonDuplicate]
    SET [Score] = 0
        ,[ScoreDetail] = '';
 --PRINT'Reset Scores: ' + CAST(DATEDIFF(s, @ms, GETDATE()) as varchar)
 --SET @ms = GETDATE()
    /*
    Merge Results of Matches into PersonDuplicate Table
    */
    -- Update PersonDuplicate table with results of email match
    MERGE [PersonDuplicate] AS TARGET
    USING (
        SELECT [e1].[PersonAliasId] [PersonAliasId]
            ,[e2].[PersonAliasId] [DuplicatePersonAliasId]
            ,max([e1].[Email]) [Email]
        FROM #PersonDuplicateByEmailTable [e1]
        JOIN #PersonDuplicateByEmailTable [e2] ON [e1].[email] = [e2].[email]
            AND [e1].[PersonAliasId] > [e2].[PersonAliasId] -- we only need the matched pair in there once (don't need both PersonA == PersonB and PersonB == PersonA)
        GROUP BY e1.PersonAliasId
            ,e2.PersonAliasId
        ) AS source(PersonAliasId, DuplicatePersonAliasId, Email)
        ON (target.PersonAliasId = source.PersonAliasId)
            AND (target.DuplicatePersonAliasId = source.DuplicatePersonAliasId)
    WHEN MATCHED
        THEN
            UPDATE
            SET [Score] = [Score] + @cScoreWeightEmail
                ,[ScoreDetail] += '|Email'
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
                ,'|Email'
                ,0
                ,@processDateTime
                ,@processDateTime
                ,NEWID()
                );
 --PRINT'Merge Email Matches: ' + CAST(DATEDIFF(s, @ms, GETDATE()) as varchar)
 --SET @ms = GETDATE()
    -- Update PersonDuplicate table with results of partial name match
 UPDATE [PD] SET 
   [Score] = [Score] + @cScoreWeightPartialName
        ,[ScoreDetail] += '|PartialName'
        ,[ModifiedDateTime] = @processDateTime
 FROM (
        SELECT [e1].[PersonAliasId] [PersonAliasId]
            ,[e2].[PersonAliasId] [DuplicatePersonAliasId]
            ,max([e1].[First2]) [First2]
            ,max([e1].[LastName]) [LastName]
        FROM #PersonDuplicateByNameTable [e1]
        JOIN #PersonDuplicateByNameTable [e2] ON [e1].[First2] = [e2].[First2]
            AND [e1].[LastName] = [e2].[LastName]
            AND [e1].[PersonAliasId] > [e2].[PersonAliasId] -- we only need the matched pair in there once (don't need both PersonA == PersonB and PersonB == PersonA)
        GROUP BY e1.PersonAliasId
            ,e2.PersonAliasId
        ) [NameDup]
 INNER JOIN [PersonDuplicate] [pd]
     ON ([pd].PersonAliasId = [NameDup].PersonAliasId)
        AND ([pd].DuplicatePersonAliasId = [NameDup].DuplicatePersonAliasId)
    INSERT INTO [PersonDuplicate] (
        PersonAliasId
        ,DuplicatePersonAliasId
        ,Score
        ,ScoreDetail
        ,IsConfirmedAsNotDuplicate
        ,ModifiedDateTime
        ,CreatedDateTime
        ,[Guid]
        )
    SELECT 
        [NameDup].PersonAliasId
        ,[NameDup].DuplicatePersonAliasId
        ,@cScoreWeightPartialName
        ,'|PartialName'
        ,0
        ,@processDateTime
        ,@processDateTime
        ,NEWID()
 FROM (
        SELECT [e1].[PersonAliasId] [PersonAliasId]
            ,[e2].[PersonAliasId] [DuplicatePersonAliasId]
            ,max([e1].[First2]) [First2]
            ,max([e1].[LastName]) [LastName]
        FROM #PersonDuplicateByNameTable [e1]
        JOIN #PersonDuplicateByNameTable [e2] ON [e1].[First2] = [e2].[First2]
            AND [e1].[LastName] = [e2].[LastName]
            AND [e1].[PersonAliasId] > [e2].[PersonAliasId] -- we only need the matched pair in there once (don't need both PersonA == PersonB and PersonB == PersonA)
        GROUP BY e1.PersonAliasId
            ,e2.PersonAliasId
        ) [NameDup]
 LEFT OUTER JOIN [PersonDuplicate] [pd]
     ON ([pd].PersonAliasId = [NameDup].PersonAliasId)
        AND ([pd].DuplicatePersonAliasId = [NameDup].DuplicatePersonAliasId)
 WHERE [pd].[id] IS NULL
 --PRINT'Merge Name Matches: ' + CAST(DATEDIFF(s, @ms, GETDATE()) as varchar)
 --SET @ms = GETDATE()
    --  Update PersonDuplicate table with results of phonenumber match for each number type. 
    --  For example, if both the Cell and Home phone match, that should be a higher score 
    DECLARE @PhoneNumberTypeValueId INT
    DECLARE @PhoneNumberTypeScore INT
    DECLARE @PhoneNumberTypeText VARCHAR(50)
    DECLARE phoneNumberTypeCursor CURSOR FAST_FORWARD
    FOR
    SELECT [Id]
    FROM [DefinedValue]
    WHERE [Id] IN (
            @cHOME_PHONENUMBER_DEFINEDVALUE_ID
            ,@cCELL_PHONENUMBER_DEFINEDVALUE_ID
            )
    ORDER BY [Id]
    OPEN phoneNumberTypeCursor
    FETCH NEXT
    FROM phoneNumberTypeCursor
    INTO @PhoneNumberTypeValueId
    -- loop thru each of the phone number types (home, cell)
    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF (@PhoneNumberTypeValueId = @cCELL_PHONENUMBER_DEFINEDVALUE_ID)
        BEGIN
            SET @PhoneNumberTypeScore = @cScoreWeightCellPhoneNumber
            SET @PhoneNumberTypeText = '|CellPhone'
        END
        ELSE
        BEGIN
            SET @PhoneNumberTypeScore = @cScoreWeightNonCellPhoneNumber
            SET @PhoneNumberTypeText = '|Phone'
        END
        MERGE [PersonDuplicate] AS TARGET
        USING (
            SELECT [e1].[PersonAliasId] [PersonAliasId]
                ,[e2].[PersonAliasId] [DuplicatePersonAliasId]
                ,max([e1].[Number])
                ,max([e1].[Extension])
                ,max([e1].[CountryCode])
                ,max([e1].[NumberTypeValueId])
            FROM #PersonDuplicateByPhoneTable [e1]
            JOIN #PersonDuplicateByPhoneTable [e2] ON [e1].[Number] = [e2].[Number]
                AND [e1].[Extension] = [e2].[Extension]
                AND [e1].[CountryCode] = [e2].[CountryCode]
                AND [e1].[NumberTypeValueId] = [e2].[NumberTypeValueId]
    AND [e1].[GroupId] <> [e2].[GroupId] -- Ignore people with duplicate phone number in same family
                AND [e1].[PersonAliasId] > [e2].[PersonAliasId] -- we only need the matched pair in there once (don't need both PersonA == PersonB and PersonB == PersonA)
            WHERE [e1].[NumberTypeValueId] = @PhoneNumberTypeValueId
            GROUP BY e1.PersonAliasId
                ,e2.PersonAliasId
            ) AS source(PersonAliasId, DuplicatePersonAliasId, Number, Extension, CountryCode, NumberTypeValueId)
            ON (target.PersonAliasId = source.PersonAliasId)
                AND (target.DuplicatePersonAliasId = source.DuplicatePersonAliasId)
        WHEN MATCHED
            THEN
                UPDATE
                SET [Score] = [Score] + @PhoneNumberTypeScore
                    ,[ScoreDetail] += @PhoneNumberTypeText
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
                    ,@PhoneNumberTypeScore
                    ,@PhoneNumberTypeText
                    ,0
                    ,@processDateTime
                    ,@processDateTime
                    ,NEWID()
                    );
        FETCH NEXT
        FROM phoneNumberTypeCursor
        INTO @PhoneNumberTypeValueId
    END
    CLOSE phoneNumberTypeCursor
    DEALLOCATE phoneNumberTypeCursor
 --PRINT'Merge Phone Matches: ' + CAST(DATEDIFF(s, @ms, GETDATE()) as varchar)
 --SET @ms = GETDATE()
    -- Update PersonDuplicate table with results of address (location) match
    MERGE [PersonDuplicate] AS TARGET
    USING (
        SELECT [e1].[PersonAliasId] [PersonAliasId]
            ,[e2].[PersonAliasId] [DuplicatePersonAliasId]
            ,max([e1].[LocationId])
            ,max([e1].[GroupRoleId])
        FROM #PersonDuplicateByAddressTable [e1]
        JOIN #PersonDuplicateByAddressTable [e2] ON [e1].[LocationId] = [e2].[LocationId]
            AND [e1].[GroupRoleId] = [e2].[GroupRoleId]
            AND [e1].[GroupId] <> [e2].[GroupId] -- Ignore people in same family
            AND [e1].[PersonAliasId] > [e2].[PersonAliasId] -- we only need the matched pair in there once (don't need both PersonA == PersonB and PersonB == PersonA)
        GROUP BY e1.PersonAliasId
            ,e2.PersonAliasId
        ) AS source(PersonAliasId, DuplicatePersonAliasId, LocationId, GroupRoleId)
        ON (target.PersonAliasId = source.PersonAliasId)
            AND (target.DuplicatePersonAliasId = source.DuplicatePersonAliasId)
    WHEN MATCHED
        THEN
            UPDATE
            SET [Score] = [Score] + @cScoreWeightAddress
                ,[ScoreDetail] += '|Address'
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
                ,@cScoreWeightAddress
                ,'|Address'
                ,0
                ,@processDateTime
                ,@processDateTime
                ,NEWID()
                );
 --PRINT'Merge Address Matches: ' + CAST(DATEDIFF(s, @ms, GETDATE()) as varchar)
 --SET @ms = GETDATE()
    /* Calculate Capacities before we do the additional scores
    */
    -- set base capacity to include MaritalStatus and Gender, since everybody has values for those
    UPDATE [PersonDuplicate]
    SET [Capacity] = CASE 
            WHEN @compareByGender = 1
                THEN @cScoreWeightGender
            ELSE 0
            END + CASE 
            WHEN @compareByMaritalStatus = 1
                THEN @cScoreWeightMaritalStatus
            ELSE 0
            END;
 --PRINT'Update Base Capacity: ' + CAST(DATEDIFF(s, @ms, GETDATE()) as varchar)
 --SET @ms = GETDATE()
    -- increment capacity values for Email, PartialName, Birthdate (do in one Update statement since these are all person fields)
    UPDATE [PersonDuplicate]
    SET [Capacity] += CASE 
            -- add the Email capacity
            WHEN @compareByEmail = 1
                AND isnull(p.Email, '') != ''
                THEN @cScoreWeightEmail
            ELSE 0
            END + CASE 
            -- add partial name capacity
            WHEN @compareByPartialName = 1
                AND (
                    isnull(p.LastName, '') != ''
                    OR isnull(p.FirstName, '') != ''
                    )
                THEN @cScoreWeightPartialName
            ELSE 0
            END + CASE 
            -- full first name capacity
            WHEN @compareByFullFirstName = 1
                AND (
                    isnull(p.FirstName, '') != ''
                    OR isnull(p.NickName, '') != ''
                    )
                THEN @cScoreWeightFullFirstName
            ELSE 0
            END + CASE 
            -- full last name capacity
            WHEN @compareByFullLastName = 1
                AND isnull(p.LastName, '') != ''
                THEN @cScoreWeightFullFirstName
            ELSE 0
            END + CASE 
            -- add the Birthday Capacity
            WHEN @compareByBirthDate = 1
                AND p.BirthDate IS NOT NULL
                THEN @cScoreWeightBirthdate
            ELSE 0
            END + CASE
   -- add the Suffix Capacity
   WHEN @compareBySuffix = 1
    AND p.SuffixValueId IS NOT NULL
    THEN @cScoreWeightSuffix
   ELSE 0
   END
    FROM PersonDuplicate pd
    JOIN PersonAlias pa ON pa.Id = pd.PersonAliasId
    JOIN Person p ON p.Id = pa.PersonId
 --PRINT'Increment Capacity for Email, Name, Birthdate: ' + CAST(DATEDIFF(s, @ms, GETDATE()) as varchar)
 --SET @ms = GETDATE()
    ---- NOTE Phone Capacity is higher if BOTH Home and Cell Phone are available
    -- increment capacity values for Home Phone
    UPDATE [PersonDuplicate]
    SET [Capacity] += @cScoreWeightNonCellPhoneNumber
    FROM PersonDuplicate pd
    JOIN PersonAlias pa ON pa.Id = pd.PersonAliasId
    JOIN Person p ON p.Id = pa.PersonId
    WHERE p.Id IN (
            SELECT PersonId
            FROM PhoneNumber
            WHERE NumberTypeValueId = @cHOME_PHONENUMBER_DEFINEDVALUE_ID
            )
        AND @compareByPhone = 1
 --PRINT'Increment Capacity for Home Phone: ' + CAST(DATEDIFF(s, @ms, GETDATE()) as varchar)
 --SET @ms = GETDATE()
    -- increment capacity values for Cell Phone
    UPDATE [PersonDuplicate]
    SET [Capacity] += @cScoreWeightCellPhoneNumber
    FROM PersonDuplicate pd
    JOIN PersonAlias pa ON pa.Id = pd.PersonAliasId
    JOIN Person p ON p.Id = pa.PersonId
    WHERE p.Id IN (
            SELECT PersonId
            FROM PhoneNumber
            WHERE NumberTypeValueId = @cCELL_PHONENUMBER_DEFINEDVALUE_ID
            )
        AND @compareByPhone = 1
 --PRINT'Increment Capacity for Cell Phone: ' + CAST(DATEDIFF(s, @ms, GETDATE()) as varchar)
 --SET @ms = GETDATE()
    -- increment capacity values for Address
    UPDATE [PersonDuplicate]
    SET [Capacity] += @cScoreWeightAddress
    FROM PersonDuplicate pd
    JOIN PersonAlias pa ON pa.Id = pd.PersonAliasId
    JOIN Person p ON p.Id = pa.PersonId
    WHERE p.Id IN (
            SELECT PersonId
            FROM [GroupMember] [gm]
            JOIN [Group] [g] ON [gm].[GroupId] = [g].[Id]
            JOIN [GroupLocation] [gl] ON [gl].[GroupId] = [g].[id]
            JOIN [Location] [l] ON [l].[Id] = [gl].[LocationId]
                AND [g].[GroupTypeId] = @cGROUPTYPE_FAMILY_ID
            WHERE [gl].[GroupLocationTypeValueId] = @cLOCATION_TYPE_HOME_ID
            )
        AND @compareByAddress = 1
 --PRINT'Increment Capacity for Address: ' + CAST(DATEDIFF(s, @ms, GETDATE()) as varchar)
 --SET @ms = GETDATE()
    -- increment capacity values for Campus
    UPDATE [PersonDuplicate]
    SET [Capacity] += @cScoreWeightCampus
    FROM PersonDuplicate pd
    JOIN PersonAlias pa ON pa.Id = pd.PersonAliasId
    JOIN Person p ON p.Id = pa.PersonId
    WHERE p.Id IN (
            SELECT PersonId
            FROM [GroupMember] [gm]
            JOIN [Group] [g] ON [gm].[GroupId] = [g].[Id]
            WHERE [g].[GroupTypeId] = @cGROUPTYPE_FAMILY_ID
                AND g.CampusId IS NOT NULL
            )
        AND @compareByCampus = 1
 --PRINT'Increment Capacity for Campus: ' + CAST(DATEDIFF(s, @ms, GETDATE()) as varchar)
 --SET @ms = GETDATE()
    /*
    Add additional scores to people that are already potential matches 
    */
    -- Increment the score on potential matches that have the same FirstName (or NickName)
    UPDATE pd
    SET [Score] = [Score] + @cScoreWeightFullFirstName
        ,[ScoreDetail] += '|FirstName'
    FROM PersonDuplicate pd
    JOIN PersonAlias pa1 ON pa1.Id = pd.PersonAliasId
    JOIN PersonAlias pa2 ON pa2.Id = pd.DuplicatePersonAliasId
    JOIN Person p1 ON p1.Id = pa1.PersonId
    JOIN Person p2 ON p2.Id = pa2.PersonId
    WHERE (
            (
                p1.FirstName = p2.FirstName
                AND isnull(p1.FirstName, '') != ''
                )
            OR (
                p1.NickName = p2.NickName
                AND isnull(p1.NickName, '') != ''
                )
            OR (
                p1.FirstName = p2.NickName
                AND isnull(p1.NickName, '') != ''
                AND isnull(p1.FirstName, '') != ''
                )
            OR (
                p1.NickName = p2.FirstName
                AND isnull(p1.NickName, '') != ''
                AND isnull(p1.FirstName, '') != ''
                )
            )
        AND @compareByFullFirstName = 1
 --PRINT'Update score for first name matches: ' + CAST(DATEDIFF(s, @ms, GETDATE()) as varchar)
 --SET @ms = GETDATE()
    -- Increment the score on potential matches that have the same LastName
    UPDATE pd
    SET [Score] = [Score] + @cScoreWeightFullFirstName
        ,[ScoreDetail] += '|LastName'
    FROM PersonDuplicate pd
    JOIN PersonAlias pa1 ON pa1.Id = pd.PersonAliasId
    JOIN PersonAlias pa2 ON pa2.Id = pd.DuplicatePersonAliasId
    JOIN Person p1 ON p1.Id = pa1.PersonId
    JOIN Person p2 ON p2.Id = pa2.PersonId
    WHERE p1.LastName = p2.LastName
        AND isnull(p1.LastName, '') != ''
        AND @compareByFullLastName = 1
 --PRINT'Update score for last name matches: ' + CAST(DATEDIFF(s, @ms, GETDATE()) as varchar)
 --SET @ms = GETDATE()
    -- Increment the score on potential matches that have the same birthday
    UPDATE pd
    SET [Score] = [Score] + @cScoreWeightBirthdate
        ,[ScoreDetail] += '|Birthdate'
    FROM PersonDuplicate pd
    INNER JOIN PersonAlias pa1 ON pa1.Id = pd.PersonAliasId
    INNER JOIN PersonAlias pa2 ON pa2.Id = pd.DuplicatePersonAliasId
    INNER JOIN Person p1 ON p1.Id = pa1.PersonId
    INNER JOIN Person p2 ON p2.Id = pa2.PersonId
    WHERE p1.BirthDate = p2.BirthDate
        AND @compareByBirthDate = 1
 --PRINT'Update score for birthday matches: ' + CAST(DATEDIFF(s, @ms, GETDATE()) as varchar)
 --SET @ms = GETDATE()
    -- Increment the score on potential matches that have the same gender
    UPDATE pd
    SET [Score] = [Score] + @cScoreWeightGender
        ,[ScoreDetail] += '|Gender'
    FROM PersonDuplicate pd
    JOIN PersonAlias pa1 ON pa1.Id = pd.PersonAliasId
    JOIN PersonAlias pa2 ON pa2.Id = pd.DuplicatePersonAliasId
    JOIN Person p1 ON p1.Id = pa1.PersonId
    JOIN Person p2 ON p2.Id = pa2.PersonId
    WHERE p1.Gender = p2.Gender
        AND @compareByGender = 1
 --PRINT'Update score for gender matches: ' + CAST(DATEDIFF(s, @ms, GETDATE()) as varchar)
 --SET @ms = GETDATE()
    -- Increment the score on potential matches that have the same campus
    UPDATE pd
    SET [Score] = [Score] + @cScoreWeightCampus
        ,[ScoreDetail] += '|Campus'
    FROM PersonDuplicate pd
    JOIN PersonAlias pa1 ON pa1.Id = pd.PersonAliasId
    JOIN PersonAlias pa2 ON pa2.Id = pd.DuplicatePersonAliasId
    JOIN Person p1 ON p1.Id = pa1.PersonId
    JOIN Person p2 ON p2.Id = pa2.PersonId
    JOIN [GroupMember] [gm1] ON [gm1].[PersonId] = [p1].[Id]
    JOIN [Group] [g1] ON [gm1].[GroupId] = [g1].[Id]
    JOIN [GroupMember] [gm2] ON [gm2].[PersonId] = [p2].[Id]
    JOIN [Group] [g2] ON [gm2].[GroupId] = [g2].[Id]
    WHERE g1.CampusId = g2.CampusId
        AND g1.CampusId IS NOT NULL
        AND g2.CampusId IS NOT NULL
        AND [g1].[GroupTypeId] = @cGROUPTYPE_FAMILY_ID
        AND [g2].[GroupTypeId] = @cGROUPTYPE_FAMILY_ID
        AND @compareByCampus = 1
 --PRINT'Update score for campus matches: ' + CAST(DATEDIFF(s, @ms, GETDATE()) as varchar)
 --SET @ms = GETDATE()
    -- Increment the score on potential matches that have the same marital status
    UPDATE pd
    SET [Score] = [Score] + @cScoreWeightMaritalStatus
        ,[ScoreDetail] += '|Marital'
    FROM PersonDuplicate pd
    JOIN PersonAlias pa1 ON pa1.Id = pd.PersonAliasId
    JOIN PersonAlias pa2 ON pa2.Id = pd.DuplicatePersonAliasId
    JOIN Person p1 ON p1.Id = pa1.PersonId
    JOIN Person p2 ON p2.Id = pa2.PersonId
    WHERE p1.MaritalStatusValueId = p2.MaritalStatusValueId
        AND @compareByMaritalStatus = 1
 --PRINT'Update score for marital status matches: ' + CAST(DATEDIFF(s, @ms, GETDATE()) as varchar)
 --SET @ms = GETDATE()
    -- Increment the score on potential matches that have the same suffix
    UPDATE pd
    SET [Score] = [Score] + @cScoreWeightSuffix
        ,[ScoreDetail] += '|Suffix'
    FROM PersonDuplicate pd
    INNER JOIN PersonAlias pa1 ON pa1.Id = pd.PersonAliasId
    INNER JOIN PersonAlias pa2 ON pa2.Id = pd.DuplicatePersonAliasId
    INNER JOIN Person p1 ON p1.Id = pa1.PersonId
    INNER JOIN Person p2 ON p2.Id = pa2.PersonId
    WHERE p1.SuffixValueId = p2.SuffixValueId
        AND @compareBySuffix = 1
 --PRINT'Update score for suffix matches: ' + CAST(DATEDIFF(s, @ms, GETDATE()) as varchar)
 --SET @ms = GETDATE()
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
 --PRINT'Remove stale matches: ' + CAST(DATEDIFF(s, @ms, GETDATE()) as varchar)
 --SET @ms = GETDATE()
    UPDATE [PersonDuplicate]
    SET [TotalCapacity] = @TotalCapacity
 --PRINT'Set Total Capacity: ' + CAST(DATEDIFF(s, @ms, GETDATE()) as varchar)
 --SET @ms = GETDATE()
 UPDATE [PersonDuplicate]
 SET [ConfidenceScore] = sqrt (
  ( CASE WHEN [TotalCapacity] > 0 
   THEN [Capacity] / ( [TotalCapacity] * 0.01 ) 
   ELSE 0 END )
  *
  ( CASE WHEN [Capacity] > 0 
   THEN [Score] / ( [Capacity] * 0.01 ) 
   ELSE 0 END )
 )
    UPDATE [PersonDuplicate]
    SET IgnoreUntilScoreChanges = 0
    FROM [PersonDuplicate] [pd]
    JOIN @IgnoreUntilScoreChangesTable [g] ON pd.Id = g.id
    WHERE pd.ConfidenceScore != g.ConfidenceScore;
 --PRINT'Set Ignore changes: ' + CAST(DATEDIFF(s, @ms, GETDATE()) as varchar)
 --SET @ms = GETDATE()
    /*
    Explicitly clean up temp tables before the proc exits (vs. have SQL Server do it for us after the proc is done)
    */
    DROP TABLE #PersonDuplicateByEmailTable;
    DROP TABLE #PersonDuplicateByNameTable;
    DROP TABLE #PersonDuplicateByPhoneTable;
    DROP TABLE #PersonDuplicateByAddressTable;
 --PRINT'Drop temp tables: ' + CAST(DATEDIFF(s, @ms, GETDATE()) as varchar)
 --SET @ms = GETDATE()
END