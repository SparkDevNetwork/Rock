/*
 Script to populate a Rock RMS database with sample interaction data quickly. The data is not very random, but quickly generates interaction
 combinations of components and person aliases sequentially.

 This script assumes the following:
 - The Person table is populated.
*/
SET NOCOUNT ON

/*
DELETE
FROM Interaction
WHERE ForeignKey = 'Interactions Sample Data'

DELETE
FROM InteractionSession
WHERE Id NOT IN (
        SELECT InteractionSessionId
        FROM Interaction
        )
        */
DECLARE @populateStartDateTimeLastHour DATETIME = DateAdd(hour, - 1, GetDate())
    , @populateStartDateTimeLast3Weeks DATETIME = DateAdd(WEEK, - 3, GetDate())
    , @populateStartDateTimeLast12Months DATETIME = DateAdd(MONTH, - 12, GetDate())
    , @populateStartDateTimeLast5Years DATETIME = DateAdd(YEAR, - 5, GetDate())
DECLARE
    -- set this to @populateStartDateTimeLastHour or @populateStartDateTimeLast12Months (or custom), depending on what you need
    @populateStartDateTime DATETIME = @populateStartDateTimeLast12Months
    , @populateEndDateTime DATETIME = DateAdd(hour, 0, GetDate())
    , @maxInteractionCount INT = 2000
    , @avgInteractionsPerSession INT = 10
    , @personSampleSize INT = 2500 -- number of people to use when randomly assigning a person to each interaction. You might want to set this lower or higher depending on what type of data you want
	,@forceIncludeAnonymousVisitors bit = 1 -- leave this true to add anonymous visitors to the set of PersonAliasIds
    
    -- Parameters
DECLARE
    -- Set this value to place a tag in the ForeignKey field of the sample data records for easier identification.
    @foreignKey NVARCHAR(100) = 'Interactions Sample Data'
/* Populate Interaction Channels for all the Pages on the Internal and External Websites */
DECLARE @externalWebSiteChannelId INT = (
        SELECT TOP 1 Id
        FROM InteractionChannel
        WHERE [Guid] = '363cc633-1416-4020-a5e9-3f5d5c77fe8c'
        )
    , @internalWebSiteChannelId INT = (
        SELECT TOP 1 Id
        FROM InteractionChannel
        WHERE [Guid] = 'c9cef6c4-e1ea-4e1f-b8d7-0172236a3f09'
        )
    , @externalWebSiteId INT = (
        SELECT TOP 1 Id
        FROM [Site]
        WHERE [Guid] = 'f3f82256-2d66-432b-9d67-3552cd2f4c2b'
        )
    , @internalWebSiteId INT = (
        SELECT TOP 1 Id
        FROM [Site]
        WHERE [Guid] = 'c2d29296-6a87-47a9-a753-ee4e9159c4c4'
        )

INSERT INTO [InteractionComponent] (
    [Name]
    , [EntityId]
    , [InteractionChannelId]
    , [Guid]
    )
SELECT p.[InternalName]
    , p.[Id]
    , @externalWebSiteChannelId
    , newid()
FROM [Page] p
WHERE LayoutId IN (
        SELECT Id
        FROM Layout
        WHERE SiteId = @externalWebSiteId
        )
    AND p.Id NOT IN (
        SELECT EntityId
        FROM InteractionComponent
        WHERE InteractionChannelId = @externalWebSiteChannelId
        )

UNION

SELECT p.[InternalName]
    , p.[Id]
    , @externalWebSiteChannelId
    , newid()
FROM [Page] p
WHERE LayoutId IN (
        SELECT Id
        FROM Layout
        WHERE SiteId = @internalWebSiteId
        )
    AND p.Id NOT IN (
        SELECT EntityId
        FROM InteractionComponent
        WHERE InteractionChannelId = @internalWebSiteChannelId
        )

PRINT 'Create Components for Pages'
PRINT N'Create Interactions Sample Data: started.';

--
-- Create Interactions.
--
DECLARE @interactionCounter INT = 0
    , @interactionPersonAliasId INT
    , @millisecondsPerDay INT = 86400000
    , @interactionComponentId INT
    , @interactionDateTime DATETIME = @populateStartDateTime
    , @millsecondsIncrement INT = NULL
    , @interactionSessionId INT
DECLARE @componentIds TABLE (id INT NOT NULL)
DECLARE @interactionsPerDay INT = @maxInteractionCount / (DateDiff(day, @populateStartDateTime, @populateEndDateTime) + 1)

IF (DateDiff(DAY, @populateStartDateTime, @populateEndDateTime) < 2)
BEGIN
    SET @millsecondsIncrement = (DateDiff(ms, @populateStartDateTime, @populateEndDateTime)) / @interactionsPerDay
END
ELSE
BEGIN
    SET @millsecondsIncrement = @millisecondsPerDay / @interactionsPerDay
END

INSERT INTO @componentIds
SELECT Id
FROM InteractionComponent

IF CURSOR_STATUS('global', 'interactionPersonAliasIdCursor') >= - 1
BEGIN
    DEALLOCATE interactionPersonAliasIdCursor;
END

DECLARE @personAliasIdTable AS TABLE (Id INT NOT NULL PRIMARY KEY)

INSERT INTO @personAliasIdTable
SELECT TOP (@personSampleSize) pa.Id
FROM PersonAlias pa
INNER JOIN 
Person p on pa.PersonId = p.Id
WHERE  p.[Guid] not in ('7ebc167b-512d-4683-9d80-98b6bb02e1b9', '802235dc-3ca5-94b0-4326-aace71180f48') and pa.PersonId NOT IN (
        SELECT Id
        FROM Person
        WHERE (
                IsDeceased = 1
                AND RecordStatusValueId != 3
				AND [Guid] not in ('7ebc167b-512d-4683-9d80-98b6bb02e1b9', '802235dc-3ca5-94b0-4326-aace71180f48') 
                )
        )
ORDER BY newid();

IF (@forceIncludeAnonymousVisitors = 1)
BEGIN
	INSERT INTO @personAliasIdTable
    SELECT pa.Id
    FROM PersonAlias pa
    INNER JOIN Person p on pa.PersonId = p.Id
    WHERE p.[Guid] = '7ebc167b-512d-4683-9d80-98b6bb02e1b9'       
END

-- put all personIds in randomly ordered cursor to speed up getting a random personAliasId for each interaction
DECLARE interactionPersonAliasIdCursor CURSOR LOCAL FAST_FORWARD
FOR
SELECT Id
FROM @personAliasIdTable
ORDER BY newid();

OPEN interactionPersonAliasIdCursor;

-- Loop for each interaction
WHILE @interactionDateTime <= @populateEndDateTime
BEGIN
    FETCH NEXT
    FROM interactionPersonAliasIdCursor
    INTO @interactionPersonAliasId;

    IF (@@FETCH_STATUS != 0)
    BEGIN
        CLOSE interactionPersonAliasIdCursor;

        OPEN interactionPersonAliasIdCursor;

        FETCH NEXT
        FROM interactionPersonAliasIdCursor
        INTO @interactionPersonAliasId;
    END

    DECLARE @ipAddress NVARCHAR(45) = (
            SELECT CONCAT (
                    floor(RAND() * 245) + 10
                    , '.'
                    , floor(RAND() * 255)
                    , '.'
                    , floor(RAND() * 255)
                    , '.'
                    , floor(RAND() * 255)
                    )
            )

    INSERT INTO [dbo].[InteractionSession] (
        [InteractionMode]
        , [SessionData]
        , [DeviceTypeId]
        , [IpAddress]
        , [Guid]
        , [SessionStartDateKey]
        )
    VALUES (
        NULL
        , NULL
        , 1
        , @ipAddress
        , NEWID()
        , CAST(DATEPART(yyyy, @interactionDateTime) AS VARCHAR(4)) + CAST(DATEPART(mm, @interactionDateTime) AS VARCHAR(2)) + CAST(DATEPART(dd, @interactionDateTime) AS VARCHAR(2))
        )

    SET @interactionSessionId = SCOPE_IDENTITY()

    DECLARE @interactionCount INT = ROUND((RAND() * @avgInteractionsPerSession * 2), 0)
        , @interactionLoopCounter INT = 0;

    WHILE (@interactionLoopCounter < @interactionCount)
    BEGIN
        SET @interactionCounter += 1;
        SET @interactionLoopCounter = @interactionLoopCounter + 1;
        SET @interactionComponentId = (
                SELECT TOP 1 Id
                FROM @componentIds
                ORDER BY newid()
                )

        INSERT INTO Interaction (
            InteractionComponentId
            , Operation
            , InteractionData
            , PersonAliasId
            , InteractionDateTime
            , InteractionSessionId
            , [Guid]
            , ForeignKey
            , CreatedDateTime
            , CreatedByPersonAliasId
            , ModifiedDateTime
            , ModifiedByPersonAliasId
            , InteractionDateKey
            )
        VALUES (
            @interactionComponentId
            , 'View'
            , CONCAT (
                'http://localhost/page/'
                , @interactionComponentId
                )
            , @interactionPersonAliasId
            , @interactionDateTime
            , @interactionSessionId
            , NEWID()
            , @foreignKey
            , @interactionDateTime
            , @interactionPersonAliasId
            , @interactionDateTime
            , @interactionPersonAliasId
            , CAST(DATEPART(yyyy, @interactionDateTime) AS VARCHAR(4)) + CAST(DATEPART(mm, @interactionDateTime) AS VARCHAR(2)) + CAST(DATEPART(dd, @interactionDateTime) AS VARCHAR(2))
            );

        SET @interactionDateTime = DATEADD(ms, @millsecondsIncrement, @interactionDateTime)

        -- Print if a multiple of 1000
        IF (@interactionCounter % 1000 = 0)
        BEGIN
            PRINT @interactionDateTime
            PRINT @interactionCounter
        END
    END
END

CLOSE interactionPersonAliasIdCursor;

PRINT N'Create Interactions Sample Data: completed.';
