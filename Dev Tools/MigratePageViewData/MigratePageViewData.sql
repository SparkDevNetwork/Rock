set statistics time on

/*
IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_Guid' AND object_id = OBJECT_ID('PageView'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Guid] ON [dbo].[PageView]
	(
		[Guid] ASC
	)
END

CREATE unique NONCLUSTERED INDEX [InteractionSessionForeignId]
ON [dbo].[InteractionSession] ([ForeignId])
INCLUDE ([Id])
 where ForeignId is not null
GO

*/

/*
SELECT count(*) [Total Interaction Rows]
FROM Interaction

SELECT count(*) [Total PageView Rows]
FROM PageView

SELECT count(*) [Total PageViews inserted into Interaction]
FROM PageView
WHERE [Guid] IN (
        SELECT [Guid]
        FROM Interaction
        )

SELECT count(*) [Total PageView Rows not in Interaction yet]
FROM PageView
WHERE [Guid] NOT IN (
        SELECT [Guid]
        FROM Interaction
        )

*/

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
        FROM InteractionChannel where ChannelEntityId is not null
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
SELECT isnull(pv.[PageTitle], '')
    ,pv.[PageId]
    ,NEWID() AS NewGuid
    ,c.[Id]
FROM [PageView] pv 
INNER JOIN [Site] s ON pv.SiteId = s.Id
INNER JOIN [InteractionChannel] c ON s.[Id] = c.[ChannelEntityId]
AND CONCAT (
        pv.[PageTitle]
		,'_'
		,pv.PageId
        ,'_'
        ,c.Id
        ) NOT IN (
        SELECT CONCAT (
				[Name]
				,'_'
                ,EntityId
                ,'_'
                ,ChannelId
                )
        FROM InteractionComponent
        )
GROUP BY pv.[PageId]
    ,isnull(pv.[PageTitle], '')
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
        FROM InteractionDeviceType where ForeignId is not null
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
        FROM InteractionSession where ForeignId is not null
        );

-- Insert Page Views

-- just in case
delete from PageView where [Guid] in (select [Guid] from Interaction) 

DECLARE @rowsInserted INT = NULL;
DECLARE @insertStartID INT;
WHILE @rowsInserted IS NULL
	OR @rowsInserted > 0
BEGIN
	SELECT @insertStartID = MAX(ID) FROM Interaction;
	
	INSERT INTO [Interaction]  WITH (TABLOCK) (
		[InteractionDateTime]
		,[Operation]
		,[InteractionComponentId]
		,[InteractionSessionId]
		,[InteractionData]
		,[PersonAliasId]
		,[Guid]
		)
	SELECT TOP (25000) *
	FROM (
		SELECT [DateTimeViewed]
			,'View' [Operation]
			,cmp.[Id] [InteractionComponentId]
			,s.[Id] [InteractionSessionId]
			,pv.[Url] [InteractionData]
			,pv.[PersonAliasId]
			,pv.[Guid] [Guid]
		FROM [PageView] pv
		CROSS APPLY (
			SELECT max(id) [Id]
			FROM [InteractionComponent] cmp
			WHERE ISNULL(pv.[PageId], 0) = ISNULL(cmp.[EntityId], 0)
				AND isnull(pv.[PageTitle], '') = isnull(cmp.[Name], '')
			) cmp
		CROSS APPLY (
			SELECT top 1 s.Id [Id]
			FROM [InteractionSession] s
			WHERE s.[ForeignId] = pv.[PageViewSessionId]
				AND s.ForeignId IS NOT NULL
			) s
		where cmp.Id is not null
		) x 

	SET @rowsInserted = @@ROWCOUNT

	delete from PageView with (tablock) where [Guid] in (select [Guid] from Interaction WHERE Id >= @insertStartID)
END

