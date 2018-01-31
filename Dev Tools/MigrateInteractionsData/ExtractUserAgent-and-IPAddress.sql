DECLARE @ipaddressPatternSendGridMandrill NVARCHAR(max) = '%([0-9]%.%[0-9]%.%[0-9]%.%[0-9]%)%'
DECLARE @ipaddressPatternMailgun NVARCHAR(max) = '%Opened from [0-9]%.%[0-9]%.%[0-9]%.%[0-9]% using %'
DECLARE @ipaddressPatternMailgun_start NVARCHAR(max) = '%[0-9]%.%[0-9]%.%[0-9]%.%[0-9]%'
DECLARE @ipaddressPatternMailgun_end NVARCHAR(max) = '% using %'
DECLARE @ipaddressPatternClick NVARCHAR(max) = '%Clicked the address % from [0-9]%.%[0-9]%.%[0-9]%.%[0-9]% using %'
DECLARE @ipaddressPatternClickStart NVARCHAR(max) = '% from %[0-9]%.%[0-9]%.%[0-9]%.%[0-9]% using%'
DECLARE @ipaddressPatternClickEnd NVARCHAR(max) = '% using %'

-- populate InteractionDeviceType
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
	,UsageCount
FROM (
	SELECT rtrim(ltrim(x.DeviceTypeInfo)) [DeviceTypeInfo]
		,count(*) [UsageCount]
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
ORDER BY x.UsageCount DESC




-- populate InteractionSession and Interaction
SELECT rtrim(ltrim(x.IPAddress)) [IPAddress]
	,rtrim(ltrim(x.DeviceTypeData)) [DeviceTypeData]
	,InteractionData
	,dt.Id [DeviceType.Id]
	,cra.*
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
