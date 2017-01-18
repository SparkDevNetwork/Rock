/*DELETE FROM [Interaction]
DELETE FROM [InteractionSession]
DELETE FROM [InteractionDeviceType]
DELETE FROM [InteractionComponent]
DELETE FROM [InteractionChannel]*/
/*IF OBJECT_ID('tempdb.dbo.#tempsession', 'U') IS NOT NULL
	DROP TABLE #tempsession*/
DECLARE @ChannelMediumValue INT;

SELECT @ChannelMediumValue = a.[Id]
FROM [DefinedValue] a
WHERE a.[Guid] = 'E503E77D-CF35-E09F-41A2-B213184F48E8'

-- Insert Websites
INSERT INTO [InteractionChannel] (
    [Name]
    ,[ComponentEntityTypeId]
    ,[ChannelTypeMediumValueId]
    ,[ChannelEntityId]
    ,[Guid]
    )
SELECT s.[Name] [Site.Name]
    ,2 -- Rock.Model.Page
    ,@channelMediumValue
    ,s.[Id] [SiteId]
    ,NEWID() AS NewGuid
FROM [PageView] pv
INNER JOIN [Site] s ON pv.[SiteId] = s.[Id]
WHERE s.Id NOT IN (
        SELECT ChannelEntityId
        FROM InteractionChannel
        )
GROUP BY s.[Id]
    ,s.[Name]

-- Insert Pages
INSERT INTO [InteractionComponent] (
    [Name]
    ,[EntityId]
    ,[Guid]
    ,[ChannelId]
    )
SELECT pv.[PageTitle]
    ,pv.[PageId]
    ,NEWID() AS NewGuid
    ,c.[Id]
FROM [PageView] pv
INNER JOIN [Site] s ON pv.SiteId = s.Id
INNER JOIN [InteractionChannel] c ON s.[Id] = c.[ChannelEntityId]
WHERE CONCAT (
        pv.PageId
        ,'_'
        ,c.Id
        ) NOT IN (
        SELECT CONCAT (
                EntityId
                ,'_'
                ,ChannelId
                )
        FROM InteractionComponent
        )
GROUP BY pv.[PageId]
    ,pv.[PageTitle]
    ,c.[Id]

-- Insert Devices
INSERT INTO [InteractionDeviceType] (
    [Name]
    ,[Application]
    ,[DeviceTypeData]
    ,[ClientType]
    ,[OperatingSystem]
    ,[Guid]
    ,[ForeignId]
    )
SELECT [OperatingSystem] + ' - ' + [Browser]
    ,[Browser]
    ,[UserAgent]
    ,[ClientType]
    ,[OperatingSystem]
    ,NEWID()
    ,[Id]
FROM [PageViewUserAgent]
WHERE Id NOT IN (
        SELECT ForeignId
        FROM InteractionDeviceType
        )

-- Insert Sessions
INSERT INTO [InteractionSession] (
    [DeviceTypeId]
    ,[IpAddress]
    ,[Guid]
    ,[ForeignId]
    )
SELECT c.[Id]
    ,a.[IpAddress]
    ,a.[Guid]
    ,a.[Id]
FROM [PageViewSession] a
INNER JOIN [PageViewUserAgent] AS b ON a.[PageViewUserAgentId] = b.[Id]
INNER JOIN [InteractionDeviceType] AS c ON c.[ForeignId] = a.[PageViewUserAgentId]
WHERE a.Id NOT IN (
        SELECT ForeignId
        FROM InteractionSession
        );

-- Insert Page Views
DECLARE @rowsInserted INT = NULL;

WHILE @rowsInserted is NULL
    OR @rowsInserted > 0
BEGIN
    INSERT INTO [Interaction] (
        [InteractionDateTime]
        ,[Operation]
        ,[InteractionComponentId]
        ,[InteractionSessionId]
        ,[InteractionData]
        ,[PersonAliasId]
        ,[Guid]
        ,[ForeignId]
        )
    SELECT TOP 100000 [DateTimeViewed]
        ,'View' [Operation]
        ,cmp.[Id] [InteractionComponentId]
        ,s.[Id] [InteractionSessionId]
        ,pv.[Url] [InteractionData]
        ,pv.[PersonAliasId]
        ,NEWID() [Guid]
        ,pv.Id
    FROM [PageView] pv
    INNER JOIN [InteractionComponent] cmp ON pv.[PageId] = cmp.[EntityId]
        AND pv.[PageTitle] = cmp.[Name]
    INNER JOIN [InteractionSession] s ON s.[ForeignId] = pv.[PageViewSessionId]
    WHERE pv.Id NOT IN (
            SELECT ForeignId
            FROM Interaction
            )

    SET @rowsInserted = @@ROWCOUNT
END

SELECT count(*)
FROM Interaction

SELECT count(*)
FROM PageView

SELECT count(*)
FROM PageView
WHERE Id IN (
        SELECT ForeignId
        FROM Interaction
        )

SELECT count(*)
FROM PageView
WHERE Id NOT IN (
        SELECT ForeignId
        FROM Interaction
        )
