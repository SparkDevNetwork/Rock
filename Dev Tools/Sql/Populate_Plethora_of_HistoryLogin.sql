
DECLARE @HistoryLoginCount INT = 100000                         -- How many [HistoryLogin] records to create
    , @StartDateTime DATETIME = DATEADD(DAY, -365, GETDATE())   -- How many days ago should the logins start?
    , @EndDateTime DATETIME = GETDATE();                        -- When should the logins end?

---------------------------------------------------------------------------------------------------
DECLARE @I INT = 0
    , @UserLoginId INT
    , @UserName NVARCHAR(255)
    , @PersonId INT
    , @PersonAliasId INT
    , @ClientIpAddress NVARCHAR(45)
    , @ExternalSource NVARCHAR(200)
    , @SourceSiteId INT
    , @RelatedDataJson NVARCHAR(MAX)
    , @WasSuccessful BIT
    , @LoginFailureReason INT
    , @LoginFailureMessage NVARCHAR(MAX)
    , @LoginDateTime DATETIME = @StartDateTime
    , @TimeSpanSeconds BIGINT = DATEDIFF(SECOND, @StartDateTime, @EndDateTime);

-- Spread the logins across the date range.
DECLARE @BaseIncrementSeconds INT = @TimeSpanSeconds / @HistoryLoginCount;
IF @BaseIncrementSeconds < 1 SET @BaseIncrementSeconds = 1;

WHILE @I < @HistoryLoginCount
BEGIN
    IF ABS(CHECKSUM(NEWID())) % 100 < 85 -- Tie 85% of the records to a [UserLogin] record.
    BEGIN
        SET @UserLoginId = (
            SELECT TOP 1 [Id]
            FROM [UserLogin]
            ORDER BY NEWID()
        );

        SELECT @UserName = [UserName]
            , @PersonId = [PersonId]
        FROM [UserLogin]
        WHERE [Id] = @UserLoginId;

        SET @PersonAliasId = (
            SELECT pa.[Id]
            FROM [PersonAlias] pa
            INNER JOIN [Person] p
                ON p.[Id] = pa.[PersonId]
            WHERE pa.[PersonId] = @PersonId
                AND pa.[AliasPersonId] = pa.[PersonId]
        );
    END
    ELSE
    BEGIN
        SET @UserLoginId = NULL;
        SET @UserName = CONCAT('UserName', CAST(@I AS NVARCHAR(10)));
        SET @PersonAliasId = NULL;
    END

    -- Add ±10% jitter
    DECLARE @Jitter INT = (ABS(CHECKSUM(NEWID())) % (@BaseIncrementSeconds / 5 + 1)) - (@BaseIncrementSeconds / 10);
    SET @LoginDateTime = DATEADD(SECOND, @BaseIncrementSeconds + @Jitter, @LoginDateTime);

    IF @LoginDateTime > GETDATE()
        SET @LoginDateTime = GETDATE();

    SET @ClientIpAddress = 
        CONCAT(
            ABS(CHECKSUM(NEWID())) % 256, '.',
            ABS(CHECKSUM(NEWID())) % 256, '.',
            ABS(CHECKSUM(NEWID())) % 256, '.',
            ABS(CHECKSUM(NEWID())) % 256
        );

    -- Reset these variables for each iteration.
    SET @ExternalSource = NULL;
    SET @RelatedDataJson = NULL;
    SET @LoginFailureReason = NULL;
    SET @LoginFailureMessage = NULL;

    -- Set a random external source for 3% of records.
    IF ABS(CHECKSUM(NEWID())) % 100 < 3
    BEGIN
        -- Generate a random string between 20–60 characters.
        DECLARE @Length INT = 20 + (ABS(CHECKSUM(NEWID())) % 41); -- range: 20–60
        SET @ExternalSource = LEFT(REPLACE(CONVERT(NVARCHAR(100), NEWID()) + CONVERT(NVARCHAR(100), NEWID()), '-', ''), @Length);
    END

    -- Randomly select an existing [Site].[Id].
    SET @SourceSiteId = (
        SELECT TOP 1 [Id]
        FROM [Site]
        ORDER BY NEWID()
    );

    -- Randomize success with a 80% success rate.
    SET @WasSuccessful = CASE WHEN ABS(CHECKSUM(NEWID())) % 100 < 80 THEN 1 ELSE 0 END;
    IF @WasSuccessful = 1
    BEGIN
        -- Set 10% of successful logins to be impersonations.
        IF @UserLoginId IS NOT NULL AND ABS(CHECKSUM(NEWID())) % 100 < 10
        BEGIN
            SET @RelatedDataJson = '{ "ImpersonatedByPersonFullName": "Some Name", "LoginContext": "Impersonation" }';
            SET @UserName = 'rckipid=XXXXXXXXXXXXXXXXXXXXXXXXXXXX';
        END
    END
    ELSE
    BEGIN
        -- Pick a random failure reason 0–7.
        SET @LoginFailureReason = ABS(CHECKSUM(NEWID())) % 8;

        -- Only set message if reason is 0 (Other).
        IF @LoginFailureReason = 0
            -- Generate a random 20-character message using NEWID().
            -- This uses base36 chars: 0–9 + A–Z.
            SET @LoginFailureMessage = LEFT(REPLACE(CONVERT(NVARCHAR(36), NEWID()), '-', ''), 20);
    END

    INSERT INTO [HistoryLogin]
    (
        [UserName]
        , [UserLoginId]
        , [PersonAliasId]
        , [LoginAttemptDateTime]
        , [ClientIpAddress]
        , [ExternalSource]
        , [SourceSiteId]
        , [RelatedDataJson]
        , [WasLoginSuccessful]
        , [LoginFailureReason]
        , [LoginFailureMessage]
        , [Guid]
    )
    VALUES
    (
        @UserName
        , @UserLoginId
        , @PersonAliasId
        , @LoginDateTime
        , @ClientIpAddress
        , @ExternalSource
        , @SourceSiteId
        , @RelatedDataJson
        , @WasSuccessful
        , @LoginFailureReason
        , @LoginFailureMessage
        , NEWID()
    );

    SET @I = @I + 1;
END

/*

SELECT * FROM [HistoryLogin] ORDER BY [Id] DESC;

TRUNCATE TABLE [HistoryLogin];

*/
