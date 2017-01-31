-- Channel is just one hardcoded thing called "Communication" which is for any Email, SMS, etc
DECLARE @InteractionChannel_COMMUNICATION UNIQUEIDENTIFIER = 'C88A187F-0343-4E7C-AF3F-79A8989DFA65'
	,@entityTypeIdCommunication INT = (
		SELECT TOP 1 Id
		FROM EntityType
		WHERE NAME = 'Rock.Model.Communication'
		)
	,@entityTypeIdCommunicationRecipient INT = (
		SELECT TOP 1 Id
		FROM EntityType
		WHERE NAME = 'Rock.Model.CommunicationRecipient'
		);

IF NOT EXISTS (
		SELECT *
		FROM InteractionChannel
		WHERE Guid = @InteractionChannel_COMMUNICATION
		)
BEGIN
	INSERT INTO InteractionChannel (
		[NAME]
		,ComponentEntityTypeId
		,InteractionEntityTypeId
		,[Guid]
		)
	VALUES (
		'Communication'
		,@entityTypeIdCommunication
		,@entityTypeIdCommunicationRecipient
		,@InteractionChannel_COMMUNICATION
		);
END;

-- insert all communications that have CommunicationRecipientActivity
DECLARE @ChannelId INT = (
		SELECT TOP 1 Id
		FROM InteractionChannel
		WHERE [Guid] = @InteractionChannel_COMMUNICATION
		)

INSERT INTO [dbo].[InteractionComponent] (
	[Name]
	,[EntityId]
	,[ChannelId]
	,[Guid]
	)
SELECT c.[Subject]
	,c.Id
	,@ChannelId
	,NEWID()
FROM Communication c
WHERE (
		c.Id IN (
			SELECT cr.CommunicationId
			FROM CommunicationRecipient cr
			WHERE cr.Id IN (
					SELECT DISTINCT cra.CommunicationRecipientId
					FROM CommunicationRecipientActivity cra
					)
			)
		)
	AND c.Id NOT IN (
		SELECT EntityId
		FROM InteractionComponent
		WHERE ChannelId = @ChannelId
		)

BEGIN
	DECLARE @ipaddressPatternSendGridMandrill NVARCHAR(max) = '%([0-9]%.%[0-9]%.%[0-9]%.%[0-9]%)%'
	DECLARE @ipaddressPatternMailgun NVARCHAR(max) = '%Opened from [0-9]%.%[0-9]%.%[0-9]%.%[0-9]% using %'
	DECLARE @ipaddressPatternMailgun_start NVARCHAR(max) = '%[0-9]%.%[0-9]%.%[0-9]%.%[0-9]%'
	DECLARE @ipaddressPatternMailgun_end NVARCHAR(max) = '% using %'
	DECLARE @ipaddressPatternClickStart NVARCHAR(max) = '% from %[0-9]%.%[0-9]%.%[0-9]%.%[0-9]% using%'
	DECLARE @ipaddressPatternClickEnd NVARCHAR(max) = '% using %'

	INSERT INTO [dbo].[InteractionDeviceType] (
		[Name]
		,[DeviceTypeData]
		,[Guid]
		)
	SELECT CASE 
			WHEN x.DeviceTypeInfo LIKE '%Apple Mail%'
				AND x.DeviceTypeInfo LIKE '%OS X%'
				THEN 'Apple Mail on OS X'
			WHEN x.DeviceTypeInfo LIKE '%Safari%'
				AND x.DeviceTypeInfo LIKE '%iOS%'
				THEN 'Safari on iOS'
			WHEN x.DeviceTypeInfo LIKE '%IE %'
				AND x.DeviceTypeInfo LIKE '%Windows%'
				THEN 'IE on Windows'
			WHEN x.DeviceTypeInfo LIKE '%Firefox browser%'
				AND x.DeviceTypeInfo LIKE '%Windows%'
				THEN 'Firefox browser on Windows'
			WHEN x.DeviceTypeInfo LIKE '%Chrome browser%'
				AND x.DeviceTypeInfo LIKE '%Windows%'
				THEN 'Chrome browser on Windows'
			WHEN x.DeviceTypeInfo LIKE '%Gmail%'
				AND x.DeviceTypeInfo LIKE '%Linux%'
				THEN 'Gmail on Linux'
			WHEN x.DeviceTypeInfo LIKE '%Android%mobile%'
				THEN 'Android Mobile'
			WHEN x.DeviceTypeInfo LIKE '%Android%browser%'
				THEN 'Android Browser'
			WHEN x.DeviceTypeInfo LIKE '%Outlook% on %Windows%'
				THEN 'Outlook on Windows'
			WHEN x.DeviceTypeInfo LIKE '%Outlook%'
				AND x.DeviceTypeInfo LIKE '%Windows%'
				THEN 'Outlook on Windows'
			ELSE 'Other'
			END [Name]
		,DeviceTypeInfo
		,NEWID()
	FROM (
		SELECT rtrim(ltrim(x.DeviceTypeInfo)) [DeviceTypeInfo]
		FROM (
			-- get just the UserAgent, etc stuff  (SendGrid or Mandrill): examples
			--   * Opened from Outlook 2013 on Windows 8 (70.209.106.108)
			--   * Opened from IE Mobile 7.0 on Windows Phone 7 (203.210.7.152)
			SELECT replace(substring([ActivityDetail], 0, PATINDEX(@ipaddressPatternSendGridMandrill, [ActivityDetail])), 'Opened from', '') [DeviceTypeInfo]
			FROM [CommunicationRecipientActivity]
			WHERE ActivityType = 'Opened'
				AND [ActivityDetail] NOT LIKE @ipaddressPatternMailgun
			
			UNION ALL
			
			-- get just the UserAgent, etc stuff  (Mailgun): examples
			--   * Opened from 207.91.187.194 using OS X desktop Apple Mail email client
			--   * Opened from 66.102.7.142 using Windows desktop Firefox browser
			SELECT replace(replace(substring([ActivityDetail], PATINDEX(@ipaddressPatternMailgun_end, [ActivityDetail]), 8000), 'Opened from', ''), ' using ', '') [DeviceTypeInfo]
			FROM [CommunicationRecipientActivity]
			WHERE ActivityType = 'Opened'
				AND [ActivityDetail] LIKE @ipaddressPatternMailgun
			
			UNION ALL
			
			SELECT ltrim(rtrim(replace(substring([Parsed], PATINDEX(@ipaddressPatternClickEnd, [Parsed]), 8000), ' using ', ''))) [DeviceTypeData]
			FROM (
				SELECT substring(ActivityDetail, PATINDEX(@ipaddressPatternClickStart, [ActivityDetail]) + len(' from '), 8000) [Parsed]
				FROM [CommunicationRecipientActivity]
				WHERE ActivityType = 'Click'
				) x
			) x
		GROUP BY rtrim(ltrim(x.DeviceTypeInfo))
		) x
	WHERE x.DeviceTypeInfo NOT IN (
			SELECT DeviceTypeData
			FROM InteractionDeviceType
			WHERE DeviceTypeData IS NOT NULL
			)
END
GO

-- populate interaction session for communications
BEGIN
	DECLARE @ipaddressPatternSendGridMandrill NVARCHAR(max) = '%([0-9]%.%[0-9]%.%[0-9]%.%[0-9]%)%'
	DECLARE @ipaddressPatternMailgun NVARCHAR(max) = '%Opened from [0-9]%.%[0-9]%.%[0-9]%.%[0-9]% using %'
	DECLARE @ipaddressPatternMailgun_start NVARCHAR(max) = '%[0-9]%.%[0-9]%.%[0-9]%.%[0-9]%'
	DECLARE @ipaddressPatternMailgun_end NVARCHAR(max) = '% using %'
	DECLARE @ipaddressPatternClickStart NVARCHAR(max) = '% from %[0-9]%.%[0-9]%.%[0-9]%.%[0-9]% using%'
	DECLARE @ipaddressPatternClickEnd NVARCHAR(max) = '% using %'

	-- populate InteractionSession
	INSERT INTO [InteractionSession] (
		IPAddress
		,DeviceTypeId
		,ForeignGuid
		,[Guid]
		)
	SELECT rtrim(ltrim(x.IPAddress)) [IPAddress]
		,dt.Id [DeviceType.Id]
		,cra.[Guid]
		,newid()
	FROM (
		-- get the IP Address and DeviceType from Opens (SendGrid or Mandrill)
		SELECT replace(replace(substring([ActivityDetail], PATINDEX(@ipaddressPatternSendGridMandrill, [ActivityDetail]), 8000), '(', ''), ')', '') [IPAddress]
			,replace(substring([ActivityDetail], 0, PATINDEX(@ipaddressPatternSendGridMandrill, [ActivityDetail])), 'Opened from', '') [DeviceTypeData]
			,NULL [InteractionData]
			,[Id]
		FROM [CommunicationRecipientActivity]
		WHERE ActivityType = 'Opened'
			AND [ActivityDetail] NOT LIKE @ipaddressPatternMailgun
		
		UNION ALL
		
		-- get the IP Address and DeviceType from Opens (Mailgun)
		SELECT substring(x.Parsed, 0, PATINDEX(@ipaddressPatternMailgun_end, x.Parsed)) [IPAddress]
			,[DeviceTypeData]
			,NULL [InteractionData]
			,[Id]
		FROM (
			SELECT [Id]
				,replace(replace(substring([ActivityDetail], PATINDEX(@ipaddressPatternMailgun_end, [ActivityDetail]), 8000), 'Opened from', ''), ' using ', '') [DeviceTypeData]
				,substring([ActivityDetail], PATINDEX(@ipaddressPatternMailgun_start, [ActivityDetail]), 8000) [Parsed]
			FROM [CommunicationRecipientActivity]
			WHERE ActivityType = 'Opened'
				AND [ActivityDetail] LIKE @ipaddressPatternMailgun
			) x
		
		UNION ALL
		
		-- get the IP Address and DeviceType from Clicks (all webhooks)
		SELECT ltrim(rtrim(substring([Parsed], 0, PATINDEX(@ipaddressPatternClickEnd, [Parsed])))) [IPAddress]
			,ltrim(rtrim(replace(substring([Parsed], PATINDEX(@ipaddressPatternClickEnd, [Parsed]), 8000), ' using ', ''))) [DeviceTypeData]
			,ltrim(rtrim(replace(replace(replace(ActivityDetail, Parsed, ''), 'Clicked the address', ''), ' from', ''))) [InteractionData]
			,Id
		FROM (
			SELECT substring(ActivityDetail, PATINDEX(@ipaddressPatternClickStart, [ActivityDetail]) + len(' from '), 8000) [Parsed]
				,replace(replace(substring([ActivityDetail], PATINDEX(@ipaddressPatternClickStart, [ActivityDetail]), 8000), '(', ''), ')', '') [IPAddress]
				,replace(substring([ActivityDetail], 0, PATINDEX('% from %', [ActivityDetail])), ' from ', '') [DeviceTypeData]
				,*
			FROM [CommunicationRecipientActivity]
			WHERE ActivityType = 'Click'
			) x
		) x
	INNER JOIN CommunicationRecipientActivity cra ON cra.Id = x.Id
	LEFT JOIN InteractionDeviceType dt ON dt.DeviceTypeData = rtrim(ltrim(x.DeviceTypeData))
	WHERE cra.[Guid] NOT IN (
			SELECT ForeignGuid
			FROM InteractionSession
			WHERE ForeignGuid IS NOT NULL
			)
END
GO

BEGIN
	DECLARE @InteractionChannel_COMMUNICATION UNIQUEIDENTIFIER = 'C88A187F-0343-4E7C-AF3F-79A8989DFA65'
	DECLARE @ipaddressPatternClickStart NVARCHAR(max) = '% from %[0-9]%.%[0-9]%.%[0-9]%.%[0-9]% using%'
	DECLARE @ChannelId INT = (
			SELECT TOP 1 Id
			FROM InteractionChannel
			WHERE [Guid] = @InteractionChannel_COMMUNICATION
			)

	-- populate Interaction
	INSERT INTO Interaction (
		[InteractionDateTime]
		,[Operation]
		,InteractionComponentId
		,PersonAliasId
		,EntityId
		,InteractionSessionId
		,InteractionData
		,ForeignGuid
		,[Guid]
		)
	SELECT top 1000 cra.ActivityDateTime [InteractionDateTime]
		,cra.ActivityType [Operation]
		,icmp.Id [ComponentId]
		,cr.PersonAliasId
		,cr.Id [EntityId]
		,iss.Id [SessionId]
		,cra.InteractionData [InteractionData]
		,cra.[Guid]
		,NEWID()
	FROM (
		SELECT ActivityDateTime
			,ActivityType
			,[Guid]
			,[CommunicationRecipientId]
			,NULL [InteractionData]
		FROM CommunicationRecipientActivity
		WHERE ActivityType = 'Opened'
		
		UNION ALL
		
		SELECT ActivityDateTime
			,ActivityType
			,[Guid]
			,[CommunicationRecipientId]
			,ltrim(rtrim(replace(replace(replace(ActivityDetail, Parsed, ''), 'Clicked the address', ''), ' from', ''))) [InteractionData]
		FROM (
			SELECT substring(ActivityDetail, PATINDEX(@ipaddressPatternClickStart, [ActivityDetail]) + len(' from '), 8000) [Parsed]
				,*
			FROM CommunicationRecipientActivity
			WHERE ActivityType = 'Click'
			) cc
		) cra
	INNER JOIN InteractionSession iss ON iss.[ForeignGuid] = cra.[Guid]
	INNER JOIN CommunicationRecipient cr ON cra.CommunicationRecipientId = cr.Id
	INNER JOIN Communication c ON cr.CommunicationId = c.Id
	INNER JOIN InteractionComponent icmp ON icmp.ChannelId = @ChannelId
		AND icmp.EntityId = c.Id
	WHERE cra.[Guid] NOT IN (
			SELECT ForeignGuid
			FROM Interaction
			WHERE ForeignGuid IS NOT NULL
			)
END

SELECT COUNT(*)
FROM CommunicationRecipientActivity
WHERE [Guid] NOT IN (
		SELECT ForeignGuid
		FROM Interaction
		WHERE ForeignGuid IS NOT NULL
		)

--delete from Interaction where ForeignGuid is not null and ForeignGuid in (select [Guid] from CommunicationRecipientActivity)
