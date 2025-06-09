
SET NOCOUNT ON;

----------------------------------------------
-- README
-- Generates [Communication], [CommunicationRecipient], [Interaction] (and associated) records.
-- This generates mostly garbage data, so it is really only useful for performance and UI testing.
-- Will likely take several (5+) minutes to run with default configuration.
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

DECLARE @SeedDeviceTypes TABLE
(
    [Name]         NVARCHAR(100),
    [ClientType]   NVARCHAR(25),
    [Application]  NVARCHAR(100),
    [Guid]         UNIQUEIDENTIFIER
);
INSERT INTO @SeedDeviceTypes VALUES
('Android 6.0 - Chrome Mobile 59.0.3324',           'Mobile',   'Chrome Mobile',    '6F11745E-70CE-41E7-AD4F-00D2C5E133C1'),
('iOS 18.3.2 - Chrome Mobile iOS 137.0.7151',       'Mobile',   'Chrome Mobile',    'B9BE2586-0EFC-42F0-8FB1-2A44F1F2A522'),
('Mac OS X 10.14.2 - Chrome 118.0.5993',            'Desktop',  'Chrome',           '15A08E1F-6E1F-4F7F-B202-11505CF86A33'),
('OS X - Outlook',                                  'Desktop',  'Outlook',          'C9B1D3F1-37A5-4BB6-BFFC-893E561F7C44'),
('iOS - Outlook-iOS 3.24.0',                        'Mobile',   'Outlook-iOS',      'E1B63C47-3B66-4F34-B2C1-F4DA0607F855'),
('Mac OS X - Apple Mail 601.3.9',                   'Desktop',  'Apple Mail',       '5DC2A435-819F-4562-870D-302783AC3466'),
('Android 9 - Chrome Mobile WebView 103.0.5060',    'Tablet',   'Android',          'A1FDD8E8-6346-44DF-AC23-9731139EFD77'),
('Windows 10 - Firefox 140.0',                      'Desktop',  'Firefox',          '55A3C8E0-A169-4F09-9F16-2E833F7DEE88'),
('Windows 10 - Edge 137.0.0',                       'Desktop',  'Edge',             '8ED7E8B3-970B-4060-B88F-89377E8D1DF9'),
('unknown - bingbot',                               'robot',    'bingbot',          '0C41F4F9-BBE8-4EF4-ADAF-179F9B7AE1AA');

INSERT INTO [InteractionDeviceType] ([Name], [ClientType], [Application], [Guid])
SELECT s.[Name], s.[ClientType], s.[Application], s.[Guid]
FROM   @SeedDeviceTypes s
WHERE  NOT EXISTS (SELECT 1 FROM [InteractionDeviceType] d WHERE d.[Guid] = s.[Guid]);

DECLARE @DeviceTypeIds TABLE ([Guid] UNIQUEIDENTIFIER, [Id] INT);
INSERT INTO @DeviceTypeIds ([Guid], [Id])
SELECT [Guid], [Id]
FROM   [InteractionDeviceType]
WHERE  [Guid] IN (SELECT [Guid] FROM @SeedDeviceTypes);

-- Insert a reusable [SystemPhoneNumber] for SMS
DECLARE @SystemPhoneNumberGuid UNIQUEIDENTIFIER = NEWID();
INSERT INTO [SystemPhoneNumber]
([Name], [Number], [IsActive], [IsSmsEnabled], [IsSmsForwardingEnabled], [Order], [Guid])
VALUES
('Test SMS Number', '+15555555555', 1, 1, 0, 0, @SystemPhoneNumberGuid);

DECLARE @SystemPhoneNumberId INT = SCOPE_IDENTITY();

-- Insert a reusable [PersonalDevice] for push
DECLARE @PersonalDeviceGuid UNIQUEIDENTIFIER = NEWID();
INSERT INTO [PersonalDevice]
([DeviceRegistrationId], [NotificationsEnabled], [IsActive], [Guid],
  [Name], [Model], [Manufacturer], [DeviceVersion])
VALUES
(NEWID(), 1, 1, @PersonalDeviceGuid, 'Test Device', 'Model X', 'OpenAI', '1.0.0');
DECLARE @PersonalDeviceId INT = SCOPE_IDENTITY();

-- Setup reusable lookup values
DECLARE @EmailMediumTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '5A653EBE-6803-44B4-85D2-FB7B8146D55D');
DECLARE @SmsMediumTypeId  INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '4BC02764-512A-4A10-ACDE-586F71D8A8BD');
DECLARE @PushMediumTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '3638C6DF-4FF3-4A52-B4B8-AFB754991597');
DECLARE @InteractionChannelId INT = (SELECT TOP 1 [Id] FROM [InteractionChannel] WHERE [Guid] = 'C88A187F-0343-4E7C-AF3F-79A8989DFA65');

-- Preload all eligible [PersonAlias] Ids
DECLARE @PersonAlias TABLE ([Id] INT);
INSERT INTO @PersonAlias ([Id])
SELECT [Id] FROM [PersonAlias];

-- Preload eligible topic DefinedValue Ids
DECLARE @TopicValues TABLE ([Id] INT);
INSERT INTO @TopicValues ([Id])
SELECT dv.[Id]
FROM   [DefinedValue] dv
JOIN   [DefinedType] dt ON dt.[Id] = dv.[DefinedTypeId]
WHERE  dt.[Guid] = 'A798492C-F0A4-496E-9142-97D9336C3E99';

-- Utility: Generate pseudo-random strings
DECLARE @LoremIpsum TABLE ([Id] INT IDENTITY(1,1), [Text] NVARCHAR(MAX));
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
ORDER BY NEWID();

-- Date-range helpers
DECLARE @I INT            = 0;
DECLARE @LoginDateTime    DATETIME = @StartDate;
DECLARE @TimeSpanSeconds  BIGINT   = DATEDIFF(SECOND, @StartDate, @EndDate);
DECLARE @BaseIncrementSeconds INT  = CASE WHEN @CommCount = 0 THEN 1 ELSE @TimeSpanSeconds / @CommCount END;
IF @BaseIncrementSeconds < 1 SET @BaseIncrementSeconds = 1;

/* ===================
   Communication Loop
   ===================*/
WHILE @I < @CommCount
BEGIN
    -- Choose comm type / status
    DECLARE @CommType   INT = ABS(CHECKSUM(NEWID())) % 4; -- 0=recipient preference,1=email,2=sms,3=push

    DECLARE @RndCommStatus INT = ABS(CHECKSUM(NEWID())) % 100;
    DECLARE @CommStatus INT = CASE
        WHEN @RndCommStatus < 75 THEN 3     -- 75% Approved
        WHEN @RndCommStatus < 85 THEN 2     -- 10% Pending Approval
        WHEN @RndCommStatus < 90 THEN 1     -- 5% Draft
        WHEN @RndCommStatus < 95 THEN 4     -- 5% Denied
        ELSE 0                              -- 5% Transient
    END;

    DECLARE @IsBulk BIT = CASE WHEN ABS(CHECKSUM(NEWID())) % 100 < 60 THEN 1 ELSE 0 END;

    -- Timestamp advance
    DECLARE @Jitter INT = (ABS(CHECKSUM(NEWID())) % (@BaseIncrementSeconds / 5 + 1))
                        - (@BaseIncrementSeconds / 10);
    SET @LoginDateTime = DATEADD(SECOND, @BaseIncrementSeconds + @Jitter, @LoginDateTime);
    IF @LoginDateTime > GETDATE() SET @LoginDateTime = GETDATE();

    DECLARE @CreatedDateTime     DATETIME = @LoginDateTime;
    DECLARE @FutureSendDateTime  DATETIME = NULL;
    DECLARE @SendDateTime        DATETIME = NULL;

    IF @CommStatus = 3 -- Approved
    BEGIN
        IF ABS(CHECKSUM(NEWID())) % 2 = 0
            -- Assign only a send date time
            SET @SendDateTime = DATEADD(MINUTE, ABS(CHECKSUM(NEWID())) % 60, @CreatedDateTime);

            -- Ensure send date time is never in the future
            IF @SendDateTime > GETDATE()
                SET @SendDateTime = GETDATE();
        ELSE
        BEGIN
            -- Assign both a future send date time and a send date time
            SET @FutureSendDateTime = DATEADD(MINUTE, ABS(CHECKSUM(NEWID())) % 1440, @CreatedDateTime);
            SET @SendDateTime       = DATEADD(MINUTE, ABS(CHECKSUM(NEWID())) % 60, @FutureSendDateTime);

            -- Ensure send date time is never in the future
            IF @SendDateTime > GETDATE()
            BEGIN
                SET @FutureSendDateTime = GETDATE();
                SET @SendDateTime = @FutureSendDateTime;
            END
        END
    END
    ELSE IF @CommStatus NOT IN (0,1) -- Transient, Draft
        -- Assign only a future send date time
        SET @FutureSendDateTime = DATEADD(MINUTE, ABS(CHECKSUM(NEWID())) % 1440, @CreatedDateTime);

    -- Misc IDs / text blobs
    DECLARE @SenderId   INT = (SELECT TOP 1 [Id] FROM @PersonAlias ORDER BY NEWID());
    DECLARE @ReviewerId INT = CASE WHEN ABS(CHECKSUM(NEWID())) % 2 = 0 THEN @SenderId
                                   ELSE (SELECT TOP 1 [Id] FROM @PersonAlias ORDER BY NEWID()) END;
    DECLARE @TopicId INT = NULL;
    IF ABS(CHECKSUM(NEWID())) % 100 < 30
        SET @TopicId = (SELECT TOP 1 [Id] FROM @TopicValues ORDER BY NEWID());

    DECLARE @Subject       NVARCHAR(1000),
            @SMS           NVARCHAR(MAX),
            @PushTitle     NVARCHAR(100),
            @PushMessage   NVARCHAR(MAX),
            @Name          NVARCHAR(100),
            @HtmlBody      NVARCHAR(MAX),
            @Summary       NVARCHAR(600);

    -- Randomize character lengths of the following fields
    DECLARE @NameLength INT = 10 + (ABS(CHECKSUM(NEWID())) % 101);
    DECLARE @SubjectLength INT = 30 + (ABS(CHECKSUM(NEWID())) % 1001);
    DECLARE @PushTitleLength INT = 10 + (ABS(CHECKSUM(NEWID())) % 101);
    DECLARE @PushMessageLength INT = 200 + (ABS(CHECKSUM(NEWID())) % 501);
    DECLARE @SummaryLength INT = 200 + (ABS(CHECKSUM(NEWID())) % 601);

    DECLARE @SMSLength INT = 100 + (ABS(CHECKSUM(NEWID())) % 1001);
    SET @SMS = (
            SELECT LEFT(STUFF((
                SELECT ' ' + [Text]
                FROM (SELECT TOP 10 [Text] FROM @LoremIpsum ORDER BY NEWID()) AS x
                FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, ''), @SMSLength)
        );

    SET @Name     = LEFT((SELECT TOP 1 [Text] FROM @LoremIpsum ORDER BY NEWID()), @NameLength);
    SET @HtmlBody = '<html><body><p>' + (SELECT TOP 1 [Text] FROM @LoremIpsum ORDER BY NEWID()) + '</p></body></html>';

    IF @CommType IN (0,1) -- Recipient Preference, Email
    BEGIN
        SET @Subject = (
            SELECT LEFT(STUFF((
                SELECT ' ' + [Text]
                FROM (SELECT TOP 10 [Text] FROM @LoremIpsum ORDER BY NEWID()) AS x
                FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, ''), @SubjectLength)
            );

        SET @Summary = (
            SELECT LEFT(STUFF((
                SELECT ' ' + [Text]
                FROM (SELECT TOP 6 [Text] FROM @LoremIpsum ORDER BY NEWID()) AS x
                FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, ''), @SummaryLength)
            );
    END
    ELSE IF @CommType = 2 -- SMS
    BEGIN
        SET @Summary = LEFT(@SMS, 600);
    END
    ELSE IF @CommType = 3 -- Push Notification
    BEGIN
        SET @PushTitle   = LEFT((SELECT TOP 1 [Text] FROM @LoremIpsum ORDER BY NEWID()), @PushTitleLength);
        SET @PushMessage = LEFT(@SMS, @PushMessageLength);
        SET @Summary     = @PushMessage;

        SET @SMS = NULL; -- Clear it, as we don't want push notifications to have an SMS message.
    END

    -- Insert Communication
    DECLARE @CommGuid UNIQUEIDENTIFIER = NEWID();
    INSERT INTO [Communication]
    (
        [Subject]
        , [FutureSendDateTime]
        , [Status]
        , [Guid]
        , [CreatedDateTime]
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
        --, [Summary]
    )
    VALUES
    (
        @Subject
        , @FutureSendDateTime
        , @CommStatus
        , @CommGuid
        , @CreatedDateTime
        , @IsBulk
        , @SenderId
        , @ReviewerId
        , @Name
        , @CommType
        , 'Rock Solid Church'
        , 'noreply+' + CAST(NEWID() AS NVARCHAR(36)) + '@rocksolidchurchdemo.com'
        , @HtmlBody
        , @SMS
        , @PushTitle
        , @PushMessage
        , @SendDateTime
        , CASE
            WHEN @CommType IN (0,2) THEN @SystemPhoneNumberId
            ELSE NULL
          END
        , @TopicId
        --, @Summary
    );
    DECLARE @CommId INT = SCOPE_IDENTITY();

    -- Create link pool (10-15 links) for this communication
    DECLARE @LinkPool TABLE ([Link] NVARCHAR(200));
    DECLARE @LinkPoolCount INT = 10 + ABS(CHECKSUM(NEWID())) % 6;
    DECLARE @LinkIndex INT = 1;
    WHILE @LinkIndex <= @LinkPoolCount
    BEGIN
        INSERT INTO @LinkPool ([Link])
        VALUES ('https://www.rocksolidchurchdemo.com/link' + CAST(@LinkIndex AS NVARCHAR(10)));
        SET @LinkIndex += 1;
    END

    INSERT INTO [InteractionComponent]
    (
        [Name], [EntityId], [InteractionChannelId], [CreatedDateTime], [Guid]
    )
    VALUES
    (
        'Comm #' + CAST(@CommId AS NVARCHAR(10)), @CommId, @InteractionChannelId, GETDATE(), NEWID()
    );
    DECLARE @InteractionComponentId INT = SCOPE_IDENTITY();

    /* ===============
       Recipient Loop
       ===============*/
    DECLARE @RecipientCount INT = 1 + ABS(CHECKSUM(NEWID())) % @MaxRecipientCount;
    DECLARE @R INT = 0;

    WHILE @R < @RecipientCount
    BEGIN
        DECLARE @RecipientPersonAliasId INT = (SELECT TOP 1 [Id] FROM @PersonAlias ORDER BY NEWID());

        DECLARE @InteractionSessionId INT;
        DECLARE @ClientIpAddress NVARCHAR(45) =
            CONCAT(
                    ABS(CHECKSUM(NEWID())) % 256, '.',
                    ABS(CHECKSUM(NEWID())) % 256, '.',
                    ABS(CHECKSUM(NEWID())) % 256, '.',
                    ABS(CHECKSUM(NEWID())) % 256
                );

        DECLARE @DeviceTypeId INT = NULL;
        IF ABS(CHECKSUM(NEWID())) % 100 < 90       -- 90 % have a known device-type
            SELECT TOP 1 @DeviceTypeId = [Id] FROM @DeviceTypeIds ORDER BY NEWID();

        SET @ClientIpAddress = 
            CONCAT(
                ABS(CHECKSUM(NEWID())) % 256, '.',
                ABS(CHECKSUM(NEWID())) % 256, '.',
                ABS(CHECKSUM(NEWID())) % 256, '.',
                ABS(CHECKSUM(NEWID())) % 256
            );

        INSERT INTO [InteractionSession]
        (
            [DeviceTypeId],
            [IpAddress],
            [CreatedDateTime],
            [Guid],
            [InteractionChannelId]
        )
        VALUES
        (
            @DeviceTypeId,
            @ClientIpAddress,
            ISNULL(@SendDateTime, @CreatedDateTime),
            NEWID(),
            @InteractionChannelId
        );
        SET @InteractionSessionId = SCOPE_IDENTITY();

        DECLARE @MediumId INT = CASE
            WHEN @CommType = 0 THEN
                CASE WHEN ABS(CHECKSUM(NEWID())) % 2 = 0 THEN @EmailMediumTypeId ELSE @SmsMediumTypeId END
            WHEN @CommType = 1 THEN @EmailMediumTypeId
            WHEN @CommType = 2 THEN @SmsMediumTypeId
            ELSE @PushMediumTypeId
        END;

        DECLARE @RecipientStatus INT = 0; -- Pending by default
        IF @CommStatus = 3 -- Approved
        BEGIN
            DECLARE @AgeMinutes INT = DATEDIFF(MINUTE, @SendDateTime, GETDATE());
            DECLARE @RndRecipStatus INT = ABS(CHECKSUM(NEWID())) % 100;
            IF @AgeMinutes > 2880 -- Older than 2 days
            BEGIN
                SET @RecipientStatus = CASE
                    WHEN @RndRecipStatus < 60 THEN 1    -- 60% Delivered
                    WHEN @RndRecipStatus < 95 THEN 4    -- 35% Opened
                    WHEN @RndRecipStatus < 97 THEN 2    -- 2% Failed
                    ELSE 3                              -- 3% Cancelled
                END;
            END
            ELSE
            BEGIN
                SET @RecipientStatus = CASE
                    WHEN @RndRecipStatus < 50 THEN 0    -- 50% Pending
                    WHEN @RndRecipStatus < 90 THEN 1    -- 40% Delivered
                    WHEN @RndRecipStatus < 95 THEN 4    -- 5% Opened
                    WHEN @RndRecipStatus < 99 THEN 3    -- 4% Cancelled
                    ELSE 5                              -- 1% Sending
                END;
            END
        END

        DECLARE @CausedUnsubscribe BIT          = 0;
        DECLARE @UnsubscribeDateTime DATETIME   = NULL;
        DECLARE @UnsubscribeLevel  INT          = NULL;
        DECLARE @UnsubscribeOffsetMinutes INT   = NULL

        -- Some recipients will unsubscribe as a result of receiving this communication, as follows:
        IF @MediumId = @EmailMediumTypeId           -- Must be an email
            AND @CommStatus = 3                     -- Communication must be approved
            AND @SendDateTime IS NOT NULL           -- Communication must have a send date time
            AND ABS(CHECKSUM(NEWID())) % 100 < 3    -- 3% of these recipients will unsubscribe
        BEGIN
            SET @CausedUnsubscribe   = 1;

            IF @UseRealisticInteractionDateRange = 1
            BEGIN
                DECLARE @RndUnsub INT = ABS(CHECKSUM(NEWID())) % 100; -- weighted 0-to-180-day offset
                SET @UnsubscribeOffsetMinutes =
                    CASE
                        WHEN @RndUnsub < 80                                         -- ~80 % happen in first 7 days
                            THEN ABS(CHECKSUM(NEWID())) % (7 * 1440)                -- 0-7 d
                        WHEN @RndUnsub < 90                                         -- ~10 % in days 8-45
                            THEN 7 * 1440 + ABS(CHECKSUM(NEWID())) % (38 * 1440)    -- 7-45 d
                        ELSE                                                        -- ~10 % outliers up to ~6 months
                            45 * 1440 + ABS(CHECKSUM(NEWID())) % (135 * 1440)       -- 45-180 d
                    END;
            END
            ELSE
                SET @UnsubscribeOffsetMinutes = ABS(CHECKSUM(NEWID())) % 2880;

            SET @UnsubscribeDateTime = DATEADD(MINUTE, @UnsubscribeOffsetMinutes, @SendDateTime);
            SET @UnsubscribeDateTime = CASE
                WHEN @UnsubscribeDateTime > GETDATE() THEN GETDATE()
                ELSE @UnsubscribeDateTime
            END;

            SET @UnsubscribeLevel    = CASE ABS(CHECKSUM(NEWID())) % 4
                WHEN 0 THEN 1
                WHEN 1 THEN 2
                WHEN 2 THEN 3
                ELSE 4
            END;
        END

        INSERT INTO [CommunicationRecipient]
        (
            [CommunicationId]
            , [Status]
            , [Guid]
            , [PersonAliasId]
            , [MediumEntityTypeId]
            , [PersonalDeviceId]
            , [CausedUnsubscribe]
            , [UnsubscribeDateTime]
            , [UnsubscribeLevel]
        )
        VALUES
        (
            @CommId
            , @RecipientStatus
            , NEWID()
            , @RecipientPersonAliasId
            , @MediumId
            , CASE
                WHEN @CommType = 3 THEN @PersonalDeviceId
                ELSE NULL
              END
            , @CausedUnsubscribe
            , @UnsubscribeDateTime
            , @UnsubscribeLevel
        );
        DECLARE @RecipientId INT = SCOPE_IDENTITY();

        DECLARE @ShouldGenerateInteractions BIT = CASE
            WHEN @CommStatus = 3 -- Approved
                AND @SendDateTime IS NOT NULL
                AND @MediumId <> @SmsMediumTypeId
                AND @RecipientStatus IN (1, 4) -- Delivered, Opened
            THEN 1
            ELSE 0 END;

        IF @ShouldGenerateInteractions = 1
        BEGIN
        -- Decide if this recipient gets any opens at all
            IF ABS(CHECKSUM(NEWID())) % 100 < 80  -- 80 % of recipients will get >= 1 open (20% won't open at all)
            BEGIN
                -- Decide HOW MANY opens each recipient gets
                DECLARE @RndOpenCount INT = ABS(CHECKSUM(NEWID())) % 100;
                DECLARE @OpenCount INT =
                CASE
                    WHEN @RndOpenCount < 50 THEN 1  -- 50% 1 open
                    WHEN @RndOpenCount < 80 THEN 2  -- 30% 2 opens
                    WHEN @RndOpenCount < 95 THEN 3  -- 15% 3 opens
                    ELSE 4                          -- 5% 4 opens
                END;

                DECLARE @InteractionOffsetMinutes INT, @InteractionDateTime DATETIME;
                DECLARE @MinOpenOffset INT = 2147483647; 

                DECLARE @OpenIndex INT = 0;
                WHILE @OpenIndex < @OpenCount
                BEGIN
                    DECLARE @RndOpen INT = ABS(CHECKSUM(NEWID())) % 100; -- weighted 0-to-180-day offset

                    IF @UseRealisticInteractionDateRange = 1
                    BEGIN
                        SET @InteractionOffsetMinutes =
                            CASE
                                WHEN @RndOpen < 80                                          -- ~80 % happen in first 7 days
                                    THEN ABS(CHECKSUM(NEWID())) % (7 * 1440)                -- 0-7 d
                                WHEN @RndOpen < 90                                          -- ~10 % in days 8-45
                                    THEN 7 * 1440 + ABS(CHECKSUM(NEWID())) % (38 * 1440)    -- 7-45 d
                                ELSE                                                        -- ~10 % outliers up to ~6 months
                                    45 * 1440 + ABS(CHECKSUM(NEWID())) % (135 * 1440)       -- 45-180 d
                            END;
                    END
                    ELSE
                        SET @InteractionOffsetMinutes = ABS(CHECKSUM(NEWID())) % 1440;

                    -- Keep track of the earliest open so we can ensure clicks don't happen before this takes place
                    SET @MinOpenOffset = IIF(@InteractionOffsetMinutes < @MinOpenOffset,
                         @InteractionOffsetMinutes,
                         @MinOpenOffset);

                    SET @InteractionDateTime = DATEADD(MINUTE, @InteractionOffsetMinutes, @SendDateTime);
                    IF @InteractionDateTime > GETDATE()
                        SET @InteractionDateTime = GETDATE();

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
                END

                -- Click(s) generation: 25% of those who open will have clicks
                IF ABS(CHECKSUM(NEWID())) % 100 < 25
                BEGIN
                    DECLARE @ClickCount INT = 1 + ABS(CHECKSUM(NEWID())) % 4; -- 1–4 clicks
                    DECLARE @ClickIndex INT = 0;

                    -- 50% chance to repeat the same link for all clicks
                    DECLARE @UseSameLink BIT = CASE WHEN ABS(CHECKSUM(NEWID())) % 100 < 50 THEN 1 ELSE 0 END;
                    DECLARE @RepeatedLink NVARCHAR(200) = NULL;

                    IF @UseSameLink = 1
                    BEGIN
                        SELECT TOP 1 @RepeatedLink = [Link] FROM @LinkPool ORDER BY NEWID();
                    END

                    WHILE @ClickIndex < @ClickCount
                    BEGIN
                        DECLARE @RndClick INT = ABS(CHECKSUM(NEWID())) % 100; -- weighted 0-to-180-day offset

                        If @UseRealisticInteractionDateRange = 1
                        BEGIN
                            SET @InteractionOffsetMinutes =
                                CASE
                                    WHEN @RndClick < 80                                         -- ~80 % happen in first 7 days
                                        THEN ABS(CHECKSUM(NEWID())) % (7 * 1440)                -- 0-7 d
                                    WHEN @RndClick < 90                                         -- ~10 % in days 8-45
                                        THEN 7 * 1440 + ABS(CHECKSUM(NEWID())) % (38 * 1440)    -- 7-45 d
                                    ELSE                                                        -- ~10 % outliers up to ~6 months
                                        45 * 1440 + ABS(CHECKSUM(NEWID())) % (135 * 1440)       -- 45-180 d
                                END;
                        END
                        ELSE
                            SET @InteractionOffsetMinutes = ABS(CHECKSUM(NEWID())) % 2880;

                        -- Ensure clicks don't happen before the first open.
                        SET @InteractionOffsetMinutes = IIF(@InteractionOffsetMinutes < @MinOpenOffset + 1,
                            @MinOpenOffset + 1,
                            @InteractionOffsetMinutes);

                        SET @InteractionDateTime = DATEADD(MINUTE, @InteractionOffsetMinutes, @SendDateTime);
                        IF @InteractionDateTime > GETDATE()
                            SET @InteractionDateTime = GETDATE();

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
                                ELSE (SELECT TOP 1 [Link] FROM @LinkPool ORDER BY NEWID())
                              END
                            , NEWID()
                        );
                        SET @ClickIndex += 1;
                    END
                END
            END
        END

        SET @R += 1;
    END  -- Recipient loop

    SET @I += 1;
END  -- Communication loop
