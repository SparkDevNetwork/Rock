
SET NOCOUNT ON;

----------------------------------------------
-- README
-- Generates [Communication], [CommunicationRecipient], [Interaction] (and associated) records.
-- This generates mostly garbage data, so it is really only useful for performance and UI testing.
-- Will likely take several (5+) minutes to run with default configuration.

-- IMPORTANT
-- This script will limit its recipient count for a given communication to - at most - the count of
-- [Person] records in your database, regardless of the @MaxRecipientCount value below. This ensures
-- a given person receives only a single copy of the communication. With this in mind, you might want
-- to first run the "Populate_Plethora_of_Person_wRandomNames.sql" script so that you'll have enough
-- people to satisfy the @MaxRecipientCount.
----------------------------------------------

DECLARE @CommCount              INT = 250;      -- How many [Communication] records to create.
DECLARE @MaxRecipientCount      INT = 2500;     -- Max randomized count of [CommunicationRecipient] records per communication.

DECLARE @StartDate  DATETIME = DATEADD(DAY, -365, GETDATE());   -- Start date for communication date range.
DECLARE @EndDate    DATETIME = DATEADD(DAY,  14, GETDATE());    -- End date for communication date range.

-- SET to 1 to spread interactions across a wide date/time window (weigheted more heavily towards the first week, more-closely mimicking reality).
-- SET to 0 to force a very short window for all interactions (to see how analytics visuals perform with a small date/time window).
DECLARE @UseRealisticInteractionDateRange BIT = 1;

---------------------------------------------------------------------------------------------------
-- DON'T MODIFY ANYTHING BELOW

-- Seed [InteractionDeviceType]s to be used for email recipients.
DECLARE @SeedEmailDeviceTypes TABLE
(
    [Name] NVARCHAR(100)
    , [ClientType] NVARCHAR(25)
    , [OperatingSystem] NVARCHAR(100)
    , [Application] NVARCHAR(100)
    , [Guid] UNIQUEIDENTIFIER
);

INSERT INTO @SeedEmailDeviceTypes VALUES
('Android 6.0 - Chrome Mobile 59.0.3324',           'Mobile',   'Android 6.0',          'Chrome Mobile',    '6F11745E-70CE-41E7-AD4F-00D2C5E133C1'),
('iOS 18.3.2 - Chrome Mobile iOS 137.0.7151',       'Mobile',   'iOS 18.3.2',           'Chrome Mobile',    'B9BE2586-0EFC-42F0-8FB1-2A44F1F2A522'),
('Mac OS X 10.14.2 - Chrome 118.0.5993',            'Desktop',  'Mac OS X 10.14.2',     'Chrome',           '15A08E1F-6E1F-4F7F-B202-11505CF86A33'),
('OS X - Outlook',                                  'Desktop',  'OS X',                 'Outlook',          'C9B1D3F1-37A5-4BB6-BFFC-893E561F7C44'),
('iOS - Outlook-iOS 3.24.0',                        'Mobile',   'iOS',                  'Outlook-iOS',      'E1B63C47-3B66-4F34-B2C1-F4DA0607F855'),
('Mac OS X - Apple Mail 601.3.9',                   'Desktop',  'Mac OS X',             'Apple Mail',       '5DC2A435-819F-4562-870D-302783AC3466'),
('Android 9 - Chrome Mobile WebView 103.0.5060',    'Tablet',   'Android 9',            'Android',          'A1FDD8E8-6346-44DF-AC23-9731139EFD77'),
('Windows 10 - Firefox 140.0',                      'Desktop',  'Windows 10',           'Firefox',          '55A3C8E0-A169-4F09-9F16-2E833F7DEE88'),
('Windows 10 - Edge 137.0.0',                       'Desktop',  'Windows 10',           'Edge',             '8ED7E8B3-970B-4060-B88F-89377E8D1DF9'),
('unknown - bingbot',                               'robot',    'unknown',              'bingbot',          '0C41F4F9-BBE8-4EF4-ADAF-179F9B7AE1AA');

INSERT INTO [InteractionDeviceType] (
    [Name]
    , [ClientType]
    , [OperatingSystem]
    , [Application]
    , [Guid]
)
SELECT s.[Name]
    , s.[ClientType]
    , s.[OperatingSystem]
    , s.[Application]
    , s.[Guid]
FROM @SeedEmailDeviceTypes s
WHERE NOT EXISTS (SELECT 1 FROM [InteractionDeviceType] d WHERE d.[Guid] = s.[Guid]);

DECLARE @EmailDeviceTypeIds TABLE ([Id] INT);

INSERT INTO @EmailDeviceTypeIds
SELECT [Id]
FROM [InteractionDeviceType]
WHERE [Guid] IN (SELECT [Guid] FROM @SeedEmailDeviceTypes);

-- Seed [InteractionDeviceType]s to be used for push notification recipients.
DECLARE @SeedPushDeviceTypes TABLE
(
    [Name] NVARCHAR(100)
    , [ClientType] NVARCHAR(25)
    , [OperatingSystem] NVARCHAR(100)
    , [Application] NVARCHAR(100)
    , [Guid] UNIQUEIDENTIFIER
);

INSERT INTO @SeedPushDeviceTypes VALUES
('Android 14.0 - Rock Core 5.0.1',      'Mobile',   'Android 14.0',     'Rock Core 5.0.1',  '3838BC1C-E71F-4151-921E-A921DC1427EA'),
('iOS 15.8.2 - Rock Core 5.0.1',        'Mobile',   'iOS 15.8.2',       'Rock Core 5.0.1',  '8B9991F4-0FD2-4B8C-9EAF-685314D535E5'),
('Android 8.0.0 - Rock Core 5.0.0',     'Mobile',   'Android 8.0.0',    'Rock Core 5.0.0',  'B403BF82-6AE9-4B81-9130-199E2DF9A810'),
('iOS 17.5.1 - Rock Core 5.0.0',        'Mobile',   'iOS 17.5.1',       'Rock Core 5.0.0',  'C9FAEFD5-20C4-43A8-AE45-A1F7C7F7398F');

INSERT INTO [InteractionDeviceType] (
    [Name]
    , [ClientType]
    , [OperatingSystem]
    , [Application]
    , [Guid]
)
SELECT s.[Name]
    , s.[ClientType]
    , s.[OperatingSystem]
    , s.[Application]
    , s.[Guid]
FROM @SeedPushDeviceTypes s
WHERE NOT EXISTS (SELECT 1 FROM [InteractionDeviceType] d WHERE d.[Guid] = s.[Guid]);

DECLARE @PushDeviceTypeIds TABLE ([Id] INT);

INSERT INTO @PushDeviceTypeIds
SELECT [Id]
FROM [InteractionDeviceType]
WHERE [Guid] IN (SELECT [Guid] FROM @SeedPushDeviceTypes);

-- Insert a reusable [SystemPhoneNumber] for SMS notifications.
DECLARE @SystemPhoneNumberGuid UNIQUEIDENTIFIER = 'E8E5C6EB-5565-4E13-991D-F17214AA9785';
DECLARE @SystemPhoneNumberId INT = (SELECT TOP 1 [Id] FROM [SystemPhoneNumber] WHERE [Guid] = @SystemPhoneNumberGuid);

IF @SystemPhoneNumberId IS NULL
BEGIN
    INSERT INTO [SystemPhoneNumber]
    (
        [Name]
        , [Number]
        , [IsActive]
        , [IsSmsEnabled]
        , [IsSmsForwardingEnabled]
        , [Order]
        , [Guid]
    )
    VALUES
    (
        'Populate Communcations Script SMS Number'
        , '+15555555555'
        , 1                         -- [IsActive]
        , 1                         -- [IsSmsEnabled]
        , 0                         -- [IsSmsForwardingEnabled]
        , 0                         -- [Order]
        , @SystemPhoneNumberGuid
    );

    SET @SystemPhoneNumberId = SCOPE_IDENTITY();
END

-- Insert a reusable [PersonalDevice] for push
DECLARE @PersonalDeviceGuid UNIQUEIDENTIFIER = 'D1BE6A18-B2D9-49C5-A6B6-51AE117BA55A';
DECLARE @PersonalDeviceId INT = (SELECT TOP 1 [Id] FROM [PersonalDevice] WHERE [Guid] = @PersonalDeviceGuid);

IF @PersonalDeviceId IS NULL
BEGIN
    INSERT INTO [PersonalDevice]
    (
        [DeviceRegistrationId]
        , [NotificationsEnabled]
        , [IsActive]
        , [Guid]
        , [Name]
        , [Model]
        , [Manufacturer]
        , [DeviceVersion]
    )
    VALUES
    (
        NEWID()
        , 1                     -- [NotificationsEnabled]
        , 1                     -- [IsActive]
        , @PersonalDeviceGuid
        , 'Test Device'
        , 'Model X'
        , 'OpenAI'
        , '1.0.0'
    );

    SET @PersonalDeviceId = SCOPE_IDENTITY();
END

-- Preload IDs for well-known Guids.
DECLARE @EmailMediumEntityTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '5A653EBE-6803-44B4-85D2-FB7B8146D55D');
DECLARE @SMSMediumEntityTypeId  INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '4BC02764-512A-4A10-ACDE-586F71D8A8BD');
DECLARE @PushMediumEntityTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '3638C6DF-4FF3-4A52-B4B8-AFB754991597');

DECLARE @InteractionChannelId INT = (SELECT TOP 1 [Id] FROM [InteractionChannel] WHERE [Guid] = 'C88A187F-0343-4E7C-AF3F-79A8989DFA65');

-- Preload all primary [PersonAlias] IDs (and their associated [Person] ID).
DECLARE @PersonAndAliasIds TABLE(
    [PersonId] INT
    , [PersonAliasId] INT
);

INSERT INTO @PersonAndAliasIds
SELECT pa.[PersonId]
    , MIN(pa.[Id])
FROM [PersonAlias] pa
WHERE pa.[AliasPersonId] = pa.[PersonId]    -- only primary aliases
GROUP BY pa.[PersonId];                     -- one alias per person

-- Ensure the max recipient count doesn't exceed the available person count.
DECLARE @TotalPersonCount INT = (SELECT COUNT(1) FROM @PersonAndAliasIds);

IF @MaxRecipientCount > @TotalPersonCount
BEGIN
    SET @MaxRecipientCount = @TotalPersonCount;
END

-- Preload communication topic [DefinedValue] IDs.
DECLARE @TopicValueIds TABLE ([Id] INT);

INSERT INTO @TopicValueIds
SELECT dv.[Id]
FROM [DefinedValue] dv
INNER JOIN [DefinedType] dt
    ON dt.[Id] = dv.[DefinedTypeId]
WHERE dt.[Guid] = 'A798492C-F0A4-496E-9142-97D9336C3E99';

-- Generate pseudo-random strings to use for [Communication] text fields.
DECLARE @LoremIpsum TABLE ([Id] INT IDENTITY(1, 1), [Text] NVARCHAR(MAX));

;WITH Sentences AS (
    SELECT 'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus lacinia odio vitae vestibulum vestibulum.' AS txt UNION ALL
    SELECT 'Cras pulvinar mattis nunc sed blandit libero volutpat sed. Quisque id diam vel quam elementum pulvinar.' UNION ALL
    SELECT 'Etiam sit amet nisl purus in mollis nunc sed id. Dui nunc mattis enim ut tellus elementum sagittis vitae et.' UNION ALL
    SELECT 'Morbi tristique senectus et netus et malesuada fames ac turpis egestas. Vestibulum mattis ullamcorper velit sed ullamcorper morbi.' UNION ALL
    SELECT 'Faucibus pulvinar elementum integer enim neque volutpat ac tincidunt vitae semper quis lectus nulla at volutpat diam ut venenatis.'
)
INSERT INTO @LoremIpsum ([Text])
SELECT TOP 1000 txt
FROM Sentences
CROSS JOIN (SELECT TOP 300 1 AS n FROM sys.all_objects) AS x -- plenty of rows to shuffle
ORDER BY NEWID(); -- randomize

-- Date-range helpers: Spread the generated [Communications] across the available date range.
DECLARE @TimeSpanSeconds BIGINT = DATEDIFF(SECOND, @StartDate, @EndDate);
DECLARE @BaseIncrementSeconds INT = CASE
    WHEN @CommCount = 0 THEN 1
    ELSE @TimeSpanSeconds / @CommCount
END;

IF @BaseIncrementSeconds < 1
BEGIN
    SET @BaseIncrementSeconds = 1;
END

-- Declare all communication variables that will be used when looping below.
DECLARE @CommIndex INT = 0

    , @Jitter INT
    , @CommCreatedDateTime DATETIME
    , @FutureSendDateTime DATETIME
    , @SendDateTime DATETIME
    , @CommunicationAgeInMinutes INT

    , @CommType INT
    , @RandomizeCommStatus INT
    , @CommStatus INT
    , @IsBulk BIT
    , @SenderId INT
    , @ReviewerId INT

    , @NameLength INT
    , @SubjectLength INT
    , @SMSMessageLength INT
    , @PushTitleLength INT
    , @PushMessageLength INT
    , @SummaryLength INT

    , @TopicId INT
    , @Name NVARCHAR(100)
    , @Subject NVARCHAR(1000)
    , @Message NVARCHAR(MAX)
    , @SMSMessage NVARCHAR(MAX)
    , @PushTitle NVARCHAR(100)
    , @PushMessage NVARCHAR(MAX)
    , @Summary NVARCHAR(600)

    , @CommId INT

    , @LinkPoolCount INT
    , @LinkIndex INT

    , @InteractionComponentId INT;

-- Declare all communication recipient variables that will be used when looping below.
DECLARE @RecipientCount INT
    , @RecipientIndex INT

    , @RandomizeRecipStatus INT
    , @RecipientStatus INT
    , @RecipientPersonId INT
    , @RecipientPersonAliasId INT
    , @MediumEntityTypeId INT

    , @UnsubscribeDateTime DATETIME
    , @UnsubscribeLevel  INT
    , @RandomizeUnsubscribeOffsetMinutes INT
    , @UnsubscribeOffsetMinutes INT

    , @RecipientId INT;

-- Declare all interaction variables that will be used when looping below.
DECLARE @ShouldGenerateOpenedInteractions BIT
    , @RandomizeOpenCount INT
    , @OpenCount INT
    , @OpenIndex INT

    , @MinOpenOffsetMinutes INT
    , @RandomizeOpenOnDayCount INT
    , @InteractionOffsetMinutes INT
    , @InteractionDateTime DATETIME

    , @InteractionSessionId INT
    , @DeviceTypeId INT
    , @ClientIpAddress NVARCHAR(45)

    , @ShouldGenerateClickInteractions BIT
    , @ClickCount INT
    , @ClickIndex INT
    
    , @UseSameLink BIT
    , @RepeatedLink NVARCHAR(200)

    , @RandomizeClickOnDayCount INT;

-- Create temp tables used within the loops below.
IF OBJECT_ID('tempdb..#LinkPool') IS NULL
BEGIN
    CREATE TABLE #LinkPool ([Link] NVARCHAR(200));
END

IF OBJECT_ID('tempdb..#AvailableRecipients') IS NULL
BEGIN
    CREATE TABLE #AvailableRecipients (
        [RowNum] INT IDENTITY(1, 1)
        , [PersonId] INT
        , [PersonAliasId] INT
    );
END

IF OBJECT_ID('tempdb..#AlreadyAddedPersonIds') IS NULL
BEGIN
    CREATE TABLE #AlreadyAddedPersonIds ([Id] INT);
END

IF OBJECT_ID('tempdb..#AlreadyAddedPersonAliasIds') IS NULL
BEGIN
    CREATE TABLE #AlreadyAddedPersonAliasIds ([Id] INT);
END

/* ===================
   Communication Loop
   ===================*/
SET @CommCreatedDateTime = @StartDate;

WHILE @CommIndex < @CommCount
BEGIN
    -- Advance the created datetime.
    SET @Jitter = (ABS(CHECKSUM(NEWID())) % (@BaseIncrementSeconds / 5 + 1)) - (@BaseIncrementSeconds / 10);
    SET @CommCreatedDateTime = DATEADD(SECOND, @BaseIncrementSeconds + @Jitter, @CommCreatedDateTime);

    -- Ensure created datetime is never in the future.
    IF @CommCreatedDateTime > GETDATE()
    BEGIN
        SET @CommCreatedDateTime = GETDATE();
    END

    SET @FutureSendDateTime = NULL;
    SET @SendDateTime = NULL;

    SET @CommType = ABS(CHECKSUM(NEWID())) % 4; -- 0=recipient preference,1=email,2=sms,3=push
    SET @RandomizeCommStatus = ABS(CHECKSUM(NEWID())) % 100;
    SET @CommStatus = CASE
        WHEN @RandomizeCommStatus < 75 THEN 3   -- 75% Approved
        WHEN @RandomizeCommStatus < 85 THEN 2   -- 10% Pending Approval
        WHEN @RandomizeCommStatus < 90 THEN 1   -- 5% Draft
        WHEN @RandomizeCommStatus < 95 THEN 4   -- 5% Denied
        ELSE 0                                  -- 5% Transient
    END;

    SET @IsBulk = CASE WHEN ABS(CHECKSUM(NEWID())) % 100 < 60 THEN 1 ELSE 0 END; -- 60% bulk

    -- Adjust [SendDateTime] and [FutureSendDateTime] based on status.
    IF @CommStatus = 3 -- Approved
    BEGIN
        IF ABS(CHECKSUM(NEWID())) % 2 = 0
        BEGIN
            -- Assign only a send datetime 50% of the time.
            SET @SendDateTime = DATEADD(MINUTE, ABS(CHECKSUM(NEWID())) % 60, @CommCreatedDateTime);

            -- Ensure send datetime is never in the future.
            IF @SendDateTime > GETDATE()
            BEGIN
                SET @SendDateTime = GETDATE();
            END
        END
        ELSE
        BEGIN
            -- Assign both a future send datetime and a send datetime 50% of the time.
            SET @FutureSendDateTime = DATEADD(MINUTE, ABS(CHECKSUM(NEWID())) % 1440, @CommCreatedDateTime);
            SET @SendDateTime       = DATEADD(MINUTE, ABS(CHECKSUM(NEWID())) % 60, @FutureSendDateTime);

            -- Ensure send datetime is never in the future.
            IF @SendDateTime > GETDATE()
            BEGIN
                SET @FutureSendDateTime = GETDATE();
                SET @SendDateTime = @FutureSendDateTime;
            END
        END
    END
    ELSE IF @CommStatus NOT IN (0, 1) -- NOT transient or draft.
    BEGIN
        SET @FutureSendDateTime = DATEADD(MINUTE, ABS(CHECKSUM(NEWID())) % 1440, @CommCreatedDateTime);
    END

    SET @CommunicationAgeInMinutes = DATEDIFF(MINUTE, @SendDateTime, GETDATE());

    SET @SenderId = (SELECT TOP 1 [PersonAliasId] FROM @PersonAndAliasIds ORDER BY NEWID());
    SET @ReviewerId = CASE
        WHEN ABS(CHECKSUM(NEWID())) % 2 = 0 THEN @SenderId -- Self-approved 50% of the time.
        ELSE (SELECT TOP 1 [PersonAliasId] FROM @PersonAndAliasIds ORDER BY NEWID()) -- Approved by someone else 50% of the time.
    END;

    SET @TopicId = NULL;
    IF ABS(CHECKSUM(NEWID())) % 100 < 30 -- Assign a random topic 30% of the time.
    BEGIN
        SET @TopicId = (SELECT TOP 1 [Id] FROM @TopicValueIds ORDER BY NEWID());
    END

    -- Randomize character lengths.
    SET @NameLength = 10 + (ABS(CHECKSUM(NEWID())) % 91); -- 10-100 chars
    SET @SubjectLength = 30 + (ABS(CHECKSUM(NEWID())) % 971); -- 30-1000
    SET @SMSMessageLength = 100 + (ABS(CHECKSUM(NEWID())) % 901); -- 100-1000
    SET @PushTitleLength = 10 + (ABS(CHECKSUM(NEWID())) % 91); -- 10-100
    SET @PushMessageLength = 200 + (ABS(CHECKSUM(NEWID())) % 301); -- 200-500
    SET @SummaryLength = 200 + (ABS(CHECKSUM(NEWID())) % 401); -- 200-600

    SET @Name = LEFT((SELECT TOP 1 [Text] FROM @LoremIpsum ORDER BY NEWID()), @NameLength);

    SET @SMSMessage = (
            SELECT LEFT(STUFF((
                SELECT ' ' + [Text]
                FROM (SELECT TOP 10 [Text] FROM @LoremIpsum ORDER BY NEWID()) AS x
                FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, ''), @SMSMessageLength)
        );

    SET @Subject = NULL;
    SET @Message = NULL;
    SET @PushTitle = NULL;
    SET @PushMessage = NULL;
    SET @Summary = NULL;

    IF @CommType IN (0, 1) -- Recipient Preference, Email
    BEGIN
        SET @Subject = (
            SELECT LEFT(STUFF((
                SELECT ' ' + [Text]
                FROM (SELECT TOP 10 [Text] FROM @LoremIpsum ORDER BY NEWID()) AS x
                FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, ''), @SubjectLength)
            );

        SET @Message = '<html><body><p>' + (SELECT TOP 1 [Text] FROM @LoremIpsum ORDER BY NEWID()) + '</p></body></html>';

        SET @Summary = (
            SELECT LEFT(STUFF((
                SELECT ' ' + [Text]
                FROM (SELECT TOP 6 [Text] FROM @LoremIpsum ORDER BY NEWID()) AS x
                FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, ''), @SummaryLength)
            );
    END
    ELSE IF @CommType = 2 -- SMS
    BEGIN
        SET @Summary = LEFT(@SMSMessage, 600);
    END
    ELSE IF @CommType = 3 -- Push Notification
    BEGIN
        SET @PushTitle = LEFT((SELECT TOP 1 [Text] FROM @LoremIpsum ORDER BY NEWID()), @PushTitleLength);
        SET @PushMessage = LEFT(@SMSMessage, @PushMessageLength);
        SET @Summary = @PushMessage;

        SET @SMSMessage = NULL; -- Clear it, as we don't want push notifications to have an SMS message.
    END

    -- Add the [Communication].
    INSERT INTO [Communication]
    (
        [Subject]
        , [FutureSendDateTime]
        , [Status]
        , [Guid]
        , [CreatedDateTime]
        , [ModifiedDateTime]
        , [IsBulkCommunication]
        , [SenderPersonAliasId]
        , [ReviewerPersonAliasId]
        , [Name]
        , [CommunicationType]
        , [FromName]
        , [FromEmail]
        , [Message]
        , [SMSMessage]
        , [PushTitle]
        , [PushMessage]
        , [SendDateTime]
        , [SmsFromSystemPhoneNumberId]
        , [CommunicationTopicValueId]
        , [Summary]
    )
    VALUES
    (
        @Subject
        , @FutureSendDateTime
        , @CommStatus
        , NEWID()
        , @CommCreatedDateTime
        , @CommCreatedDateTime
        , @IsBulk
        , @SenderId
        , @ReviewerId
        , @Name
        , @CommType
        , 'Rock Solid Church'
        , 'noreply+' + CAST(NEWID() AS NVARCHAR(36)) + '@rocksolidchurchdemo.com'
        , @Message
        , @SMSMessage
        , @PushTitle
        , @PushMessage
        , @SendDateTime
        , CASE
            WHEN @CommType IN (0, 2) THEN @SystemPhoneNumberId -- Recipient Preference, SMS
            ELSE NULL
          END
        , @TopicId
        , @Summary
    );

    -- Skip the rest of the loop if the insert fails (unlikely, but we'll play it safe).
    IF @@ROWCOUNT = 0
    BEGIN
        GOTO NextCommunication;
    END

    SET @CommId = SCOPE_IDENTITY();

    -- Create some links to be used for interactions tied to this communication.
    TRUNCATE TABLE #LinkPool;
    SET @LinkPoolCount = 2 + ABS(CHECKSUM(NEWID())) % 14; -- 2-15 links.
    SET @LinkIndex = 1;

    WHILE @LinkIndex <= @LinkPoolCount
    BEGIN
        INSERT INTO #LinkPool
        VALUES ('https://www.rocksolidchurchdemo.com/link' + CAST(@LinkIndex AS NVARCHAR(10)));

        SET @LinkIndex += 1;
    END

    -- Add the [InteractionComponent] for all [Interactions] of this communication to reference.
    INSERT INTO [InteractionComponent]
    (
        [Name]
        , [EntityId]
        , [InteractionChannelId]
        , [CreatedDateTime]
        , [Guid]
    )
    VALUES
    (
        'Comm #' + CAST(@CommId AS NVARCHAR(10))
        , @CommId
        , @InteractionChannelId
        , GETDATE()
        , NEWID()
    );

    SET @InteractionComponentId = SCOPE_IDENTITY();

    -- Assign a random count of recipients up to the max recipient count.
    SET @RecipientCount = 1 + ABS(CHECKSUM(NEWID())) % @MaxRecipientCount;
    SET @RecipientIndex = 1;

    -- Select random people to receive this communication.
    TRUNCATE TABLE #AvailableRecipients;
    TRUNCATE TABLE #AlreadyAddedPersonIds;
    TRUNCATE TABLE #AlreadyAddedPersonAliasIds;

    INSERT INTO #AvailableRecipients (
        [PersonId]
        , [PersonAliasId]
    )
    SELECT TOP (@RecipientCount)
        [PersonId]
        , [PersonAliasId]
    FROM @PersonAndAliasIds
    ORDER BY NEWID();

    /* ===============
        Recipient Loop
        ===============*/

    WHILE @RecipientIndex <= @RecipientCount
    BEGIN
        SELECT @RecipientPersonId = [PersonId]
            , @RecipientPersonAliasId = [PersonAliasId]
        FROM #AvailableRecipients
        WHERE [RowNum] = @RecipientIndex;

        -- Failsafe to protect against bad data.
        IF @RecipientPersonId IS NULL
            OR @RecipientPersonAliasId IS NULL
            OR EXISTS (
                SELECT 1
                FROM #AlreadyAddedPersonIds
                WHERE [Id] = @RecipientPersonId
            )
            OR EXISTS (
                SELECT 1
                FROM #AlreadyAddedPersonAliasIds
                WHERE [Id] = @RecipientPersonAliasId
            )
        BEGIN
            -- This person has already been added to this communication; skip them.
            GOTO NextRecipient;
        END

        INSERT INTO #AlreadyAddedPersonIds VALUES (@RecipientPersonId);
        INSERT INTO #AlreadyAddedPersonAliasIds VALUES (@RecipientPersonAliasId);

        SET @RecipientStatus = 0; -- Pending by default.

        IF @CommStatus = 3 -- Approved
        BEGIN
            SET @RandomizeRecipStatus = ABS(CHECKSUM(NEWID())) % 100;

            IF @CommunicationAgeInMinutes > 2880 -- Sent more than 2 days ago.
            BEGIN
                SET @RecipientStatus = CASE
                    WHEN @RandomizeRecipStatus < 60 THEN 1  -- 60% Delivered
                    WHEN @RandomizeRecipStatus < 95 THEN 4  -- 35% Opened
                    WHEN @RandomizeRecipStatus < 97 THEN 2  -- 2% Failed
                    ELSE 3                                  -- 3% Cancelled
                END;
            END
            ELSE -- Sent within the last 2 days.
            BEGIN
                SET @RecipientStatus = CASE
                    WHEN @RandomizeRecipStatus < 50 THEN 0  -- 50% Pending
                    WHEN @RandomizeRecipStatus < 90 THEN 1  -- 40% Delivered
                    WHEN @RandomizeRecipStatus < 95 THEN 4  -- 5% Opened
                    WHEN @RandomizeRecipStatus < 99 THEN 3  -- 4% Cancelled
                    ELSE 5                                  -- 1% Sending
                END;
            END
        END

        SET @MediumEntityTypeId = CASE
            WHEN @CommType = 0 THEN -- Recipient Preference
                CASE
                    WHEN ABS(CHECKSUM(NEWID())) % 2 = 0 THEN @EmailMediumEntityTypeId
                    ELSE @SmsMediumEntityTypeId
                END
            WHEN @CommType = 1 THEN @EmailMediumEntityTypeId
            WHEN @CommType = 2 THEN @SmsMediumEntityTypeId
            ELSE @PushMediumEntityTypeId
        END;

        SET @UnsubscribeDateTime = NULL;
        SET @UnsubscribeLevel  = NULL;

        -- Some recipients will unsubscribe as a result of receiving this communication, as follows:
        IF @MediumEntityTypeId = @EmailMediumEntityTypeId   -- Must be an email
            AND @CommStatus = 3                             -- Communication must be approved
            AND @SendDateTime IS NOT NULL                   -- Communication must have a send date time
            AND ABS(CHECKSUM(NEWID())) % 100 < 3            -- 3% of these recipients will unsubscribe
        BEGIN
            IF @UseRealisticInteractionDateRange = 1
            BEGIN
                SET @RandomizeUnsubscribeOffsetMinutes = ABS(CHECKSUM(NEWID())) % 100; -- Weighted 0-to-180-day offset
                SET @UnsubscribeOffsetMinutes =
                    CASE
                        WHEN @RandomizeUnsubscribeOffsetMinutes < 80                -- ~80 % happen in first 7 days
                            THEN ABS(CHECKSUM(NEWID())) % (7 * 1440)                -- 0-7 d
                        WHEN @RandomizeUnsubscribeOffsetMinutes < 90                -- ~10 % in days 8-45
                            THEN 7 * 1440 + ABS(CHECKSUM(NEWID())) % (38 * 1440)    -- 7-45 d
                        ELSE                                                        -- ~10 % outliers up to ~6 months
                            45 * 1440 + ABS(CHECKSUM(NEWID())) % (135 * 1440)       -- 45-180 d
                    END;
            END
            ELSE
            BEGIN
                SET @UnsubscribeOffsetMinutes = ABS(CHECKSUM(NEWID())) % 2880;      -- Within the first 2 days
            END

            SET @UnsubscribeDateTime = DATEADD(MINUTE, @UnsubscribeOffsetMinutes, @SendDateTime);
            IF @UnsubscribeDateTime > GETDATE()
            BEGIN
                SET @UnsubscribeDateTime = GETDATE();
            END

            SET @UnsubscribeLevel = CASE ABS(CHECKSUM(NEWID())) % 4
                WHEN 0 THEN 1
                WHEN 1 THEN 2
                WHEN 2 THEN 3
                ELSE 4
            END;
        END

        -- Add the [CommunicationRecipient].
        INSERT INTO [CommunicationRecipient]
        (
            [CommunicationId]
            , [Status]
            , [Guid]
            , [CreatedDateTime]
            , [ModifiedDateTime]
            , [PersonAliasId]
            , [MediumEntityTypeId]
            , [PersonalDeviceId]
            , [UnsubscribeDateTime]
            , [UnsubscribeLevel]
        )
        VALUES
        (
            @CommId
            , @RecipientStatus
            , NEWID()
            , @CommCreatedDateTime
            , @CommCreatedDateTime
            , @RecipientPersonAliasId
            , @MediumEntityTypeId
            , CASE
                WHEN @CommType = 3 THEN @PersonalDeviceId
                ELSE NULL
              END
            , @UnsubscribeDateTime
            , @UnsubscribeLevel
        );

        -- Skip the rest of the loop if the insert fails (unlikely, but we'll play it safe).
        IF @@ROWCOUNT = 0
        BEGIN
            GOTO NextRecipient;
        END

        SET @RecipientId = SCOPE_IDENTITY();

        /* ==========================
           'Opened' Interaction Loop
           ==========================*/

        SET @ShouldGenerateOpenedInteractions = CASE
            WHEN @CommStatus = 3 -- Approved
                AND @SendDateTime IS NOT NULL
                AND @MediumEntityTypeId <> @SmsMediumEntityTypeId
                AND @RecipientStatus IN (1, 4) -- Delivered, Opened
                AND ABS(CHECKSUM(NEWID())) % 100 < 80 -- 80 % of qualifying recipients will get >= 1 open (20% won't open at all)
            THEN 1
            ELSE 0 END;

        IF @ShouldGenerateOpenedInteractions = 0
        BEGIN
            GOTO NextRecipient;
        END

        -- Decide HOW MANY opens this recipient gets.
        SET @RandomizeOpenCount = ABS(CHECKSUM(NEWID())) % 100;
        SET @OpenCount =
        CASE
            WHEN @MediumEntityTypeId = @PushMediumEntityTypeId THEN 1   -- Push notifications can only be opened once
            WHEN @RandomizeOpenCount < 50 THEN 1                        -- 50% of email recipients: 1 open
            WHEN @RandomizeOpenCount < 80 THEN 2                        -- 30% of email recipients: 2 opens
            WHEN @RandomizeOpenCount < 95 THEN 3                        -- 15% of email recipients: 3 opens
            ELSE 4                                                      -- 5% of email recipients: 4 opens
        END;

        SET @OpenIndex = 0;
        SET @MinOpenOffsetMinutes = 2147483647; -- We'll use this to ensure clicks don't happen before opens.
        SET @InteractionSessionId = NULL;

        WHILE @OpenIndex < @OpenCount
        BEGIN
            SET @RandomizeOpenOnDayCount = ABS(CHECKSUM(NEWID())) % 100; -- weighted 0-to-180-day offset

            IF @UseRealisticInteractionDateRange = 1
            BEGIN
                SET @InteractionOffsetMinutes =
                    CASE
                        WHEN @RandomizeOpenOnDayCount < 80                          -- ~80 % happen in first 7 days
                            THEN ABS(CHECKSUM(NEWID())) % (7 * 1440)                -- 0-7 d
                        WHEN @RandomizeOpenOnDayCount < 90                          -- ~10 % in days 8-45
                            THEN 7 * 1440 + ABS(CHECKSUM(NEWID())) % (38 * 1440)    -- 7-45 d
                        ELSE                                                        -- ~10 % outliers up to ~6 months
                            45 * 1440 + ABS(CHECKSUM(NEWID())) % (135 * 1440)       -- 45-180 d
                    END;
            END
            ELSE
            BEGIN
                SET @InteractionOffsetMinutes = ABS(CHECKSUM(NEWID())) % 1440;      -- Within the first day
            END

            -- Keep track of the earliest open so we can ensure clicks don't happen before this takes place
            SET @MinOpenOffsetMinutes = IIF(@InteractionOffsetMinutes < @MinOpenOffsetMinutes,
                @InteractionOffsetMinutes,
                @MinOpenOffsetMinutes);

            SET @InteractionDateTime = DATEADD(MINUTE, @InteractionOffsetMinutes, @SendDateTime);
            IF @InteractionDateTime > GETDATE()
            BEGIN
                SET @InteractionDateTime = GETDATE();
            END

            IF @InteractionSessionId IS NULL
            BEGIN
                SET @DeviceTypeId = NULL
                IF @MediumEntityTypeId <> @SmsMediumEntityTypeId AND ABS(CHECKSUM(NEWID())) % 100 < 90       -- 90 % have a known device-type
                BEGIN
                    SET @DeviceTypeId = CASE
                        WHEN @MediumEntityTypeId = @EmailMediumEntityTypeId THEN (SELECT TOP 1 [Id] FROM @EmailDeviceTypeIds ORDER BY NEWID())
                        WHEN @MediumEntityTypeId = @PushMediumEntityTypeId THEN (SELECT TOP 1 [Id] FROM @PushDeviceTypeIds ORDER BY NEWID())
                    END
                END

                SET @ClientIpAddress =
                    CONCAT(
                            ABS(CHECKSUM(NEWID())) % 256, '.',
                            ABS(CHECKSUM(NEWID())) % 256, '.',
                            ABS(CHECKSUM(NEWID())) % 256, '.',
                            ABS(CHECKSUM(NEWID())) % 256
                        );

                INSERT INTO [InteractionSession]
                (
                    [DeviceTypeId]
                    , [IpAddress]
                    , [CreatedDateTime]
                    , [Guid]
                    , [InteractionChannelId]
                )
                VALUES
                (
                    @DeviceTypeId
                    , @ClientIpAddress
                    , @InteractionDateTime
                    , NEWID()
                    , @InteractionChannelId
                );

                SET @InteractionSessionId = SCOPE_IDENTITY();
            END

            INSERT INTO [Interaction]
            (
                [InteractionDateTime]
                , [Operation]
                , [InteractionComponentId]
                , [EntityId]
                , [PersonAliasId]
                , [InteractionSessionId]
                , [Guid]
            )
            VALUES
            (
                @InteractionDateTime
                , 'Opened'
                , @InteractionComponentId
                , @RecipientId
                , @RecipientPersonAliasId
                , @InteractionSessionId
                , NEWID()
            );

            SET @OpenIndex += 1;
        END -- 'Opened' Interaction Loop

        /* =========================
           'Click' Interaction Loop
           =========================*/

        SET @ShouldGenerateClickInteractions = CASE
            WHEN @MediumEntityTypeId = @EmailMediumEntityTypeId
                AND ABS(CHECKSUM(NEWID())) % 100 < 25 -- 25% of email recipients who open will have clicks
            THEN 1
            ELSE 0 END;

        IF @ShouldGenerateClickInteractions = 0
        BEGIN
            GOTO NextRecipient;
        END

        SET @ClickCount = 1 + ABS(CHECKSUM(NEWID())) % 4; -- 1–4 clicks
        SET @ClickIndex = 0;

        -- 50% chance to repeat the same link for all clicks
        SET @UseSameLink = CASE WHEN ABS(CHECKSUM(NEWID())) % 100 < 50 THEN 1 ELSE 0 END;
        SET @RepeatedLink = NULL;

        IF @UseSameLink = 1
        BEGIN
            SELECT TOP 1 @RepeatedLink = [Link] FROM #LinkPool ORDER BY NEWID();
        END

        WHILE @ClickIndex < @ClickCount
        BEGIN
            SET @RandomizeClickOnDayCount = ABS(CHECKSUM(NEWID())) % 100; -- weighted 0-to-180-day offset

            IF @UseRealisticInteractionDateRange = 1
            BEGIN
                SET @InteractionOffsetMinutes =
                    CASE
                        WHEN @RandomizeClickOnDayCount < 80                         -- ~80 % happen in first 7 days
                            THEN ABS(CHECKSUM(NEWID())) % (7 * 1440)                -- 0-7 d
                        WHEN @RandomizeClickOnDayCount < 90                         -- ~10 % in days 8-45
                            THEN 7 * 1440 + ABS(CHECKSUM(NEWID())) % (38 * 1440)    -- 7-45 d
                        ELSE                                                        -- ~10 % outliers up to ~6 months
                            45 * 1440 + ABS(CHECKSUM(NEWID())) % (135 * 1440)       -- 45-180 d
                    END;
            END
            ELSE
            BEGIN
                SET @InteractionOffsetMinutes = ABS(CHECKSUM(NEWID())) % 2880;      -- Within the first 2 days
            END

            -- Ensure clicks don't happen before the first open.
            SET @InteractionOffsetMinutes = IIF(@InteractionOffsetMinutes < @MinOpenOffsetMinutes + 1,
                @MinOpenOffsetMinutes + 1,
                @InteractionOffsetMinutes);

            SET @InteractionDateTime = DATEADD(MINUTE, @InteractionOffsetMinutes, @SendDateTime);
            IF @InteractionDateTime > GETDATE()
            BEGIN
                SET @InteractionDateTime = GETDATE();
            END

            INSERT INTO [Interaction]
            (
                [InteractionDateTime]
                , [Operation]
                , [InteractionComponentId]
                , [EntityId]
                , [PersonAliasId]
                , [InteractionSessionId]
                , [InteractionData]
                , [Guid]
            )
            VALUES
            (
                @InteractionDateTime
                , 'Click'
                , @InteractionComponentId
                , @RecipientId
                , @RecipientPersonAliasId
                , @InteractionSessionId
                , CASE
                    WHEN @UseSameLink = 1 THEN @RepeatedLink
                    ELSE (SELECT TOP 1 [Link] FROM #LinkPool ORDER BY NEWID())
                    END
                , NEWID()
            );

            SET @ClickIndex += 1;
        END -- 'Click' Interaction Loop

NextRecipient:
        SET @RecipientIndex += 1;
    END -- Recipient Loop

NextCommunication:
    SET @CommIndex += 1;
END -- Communication Loop

DROP TABLE IF EXISTS #LinkPool;
DROP TABLE IF EXISTS #AvailableRecipients;
DROP TABLE IF EXISTS #AlreadyAddedPersonIds;
DROP TABLE IF EXISTS #AlreadyAddedPersonAliasIds;
