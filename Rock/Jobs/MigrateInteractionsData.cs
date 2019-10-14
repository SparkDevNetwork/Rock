// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Linq;

using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [IntegerField( "Command Timeout", "Maximum amount of time (in seconds) to wait for the SQL Query to complete. Leave blank to use the default for this job (3600). Note, it could take several minutes, so you might want to set it at 3600 (60 minutes) or higher", false, 60 * 60, "General", 1, "CommandTimeout" )]
    public class MigrateInteractionsData : IJob
    {
        private int _commandTimeout = 0;
        private int _channelsInserted = 0;
        private int _componentsInserted = 0;
        private int _deviceTypesInserted = 0;
        private int _sessionsInserted = 0;
        private int _pageViewsMoved = 0;
        private int _pageViewsTotal = 0;
        private int _communicationRecipientActivityMoved = 0;
        private int _communicationRecipientActivityTotal = 0;

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="System.NotImplementedException"></exception>

        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            _commandTimeout = dataMap.GetString( "CommandTimeout" ).AsIntegerOrNull() ?? 3600;

            var deleteJob = dataMap.Get( "DeleteJob" ).ToStringSafe().AsBoolean();

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = _commandTimeout;
                _pageViewsTotal = rockContext.Database.SqlQuery<int>( "SELECT COUNT(*) FROM PageView" ).First();


                rockContext.Database.ExecuteSqlCommand( @"
IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_InteractionForeignGuid' AND object_id = OBJECT_ID('Interaction'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_InteractionForeignGuid]
	ON [dbo].[Interaction] ([ForeignGuid])
    where ForeignGuid is not null
END
" );

                _communicationRecipientActivityTotal = rockContext.Database.SqlQuery<int>( @"SELECT COUNT(*)
FROM CommunicationRecipientActivity
WHERE [Guid] NOT IN (
		SELECT ForeignGuid
		FROM Interaction
		WHERE ForeignGuid IS NOT NULL
		)
" ).First();

                if ( _pageViewsTotal == 0 && _communicationRecipientActivityTotal == 0 )
                {
                    // drop the tables
                    rockContext.Database.ExecuteSqlCommand( @"
IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'PageView'))
BEGIN
    DROP TABLE PageView;
END

IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'PageView'))
BEGIN
    DROP TABLE PageView;
END

IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'PageViewSession'))
BEGIN
    DROP TABLE PageViewSession;
END

IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'PageViewUserAgent'))
BEGIN
    DROP TABLE PageViewUserAgent;
END


IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'CommunicationRecipientActivity'))
BEGIN
    DROP TABLE CommunicationRecipientActivity;
END
" );
                    
                    
                    // delete job if there are no PageView or CommunicationRecipientActivity rows  left
                    var jobId = context.GetJobId();
                    var jobService = new ServiceJobService( rockContext );
                    var job = jobService.Get( jobId );
                    if ( job != null )
                    {
                        jobService.Delete( job );
                        rockContext.SaveChanges();
                        return;
                    }
                }
            }

            MigratePageViewsData( context );
            MigrateCommunicationRecipientActivityData( context );

            context.UpdateLastStatusMessage( $@"Channels Inserted: {_channelsInserted}, 
Components Inserted: {_componentsInserted}, 
DeviceTypes Inserted: {_deviceTypesInserted},
Sessions Inserted: {_sessionsInserted},
PageViews Moved: {_pageViewsMoved}/{_pageViewsTotal},
CommunicationRecipientActivity Moved: {_communicationRecipientActivityMoved}/{_communicationRecipientActivityTotal}
" );
        }

        /// <summary>
        /// Migrates the page views data.
        /// </summary>
        /// <param name="context">The context.</param>
        private void MigratePageViewsData( IJobExecutionContext context )
        {
            using ( var rockContext = new RockContext() )
            {
                var componentEntityTypePage = EntityTypeCache.Get<Rock.Model.Page>();
                var channelMediumWebsite = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE );
                var sqlInsertSitesToChannels = $@"
-- Insert Websites
INSERT INTO [InteractionChannel] (
    [Name]
    ,[ComponentEntityTypeId]
    ,[ChannelTypeMediumValueId]
    ,[ChannelEntityId]
    ,[Guid]
    )
SELECT s.[Name] [Site.Name]
    ,{componentEntityTypePage.Id}
    ,'{channelMediumWebsite.Id}'
    ,s.[Id] [SiteId]
    ,NEWID() AS NewGuid
FROM [PageView] pv
INNER JOIN [Site] s ON pv.[SiteId] = s.[Id]
WHERE s.Id NOT IN (
        SELECT ChannelEntityId
        FROM InteractionChannel where ChannelEntityId is not null
        )
GROUP BY s.[Id]
    ,s.[Name]";

                var sqlInsertPagesToComponents = @"
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
";

                var insertUserAgentToDeviceTypes = @"
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
";

                // Clean up unused PageViewSession data in chunks
                // delete the indexes so they don't have to be updated as the rows are deleted
                rockContext.Database.ExecuteSqlCommand( @"
IF EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE NAME = 'IX_Guid'
			AND object_id = OBJECT_ID('PageViewSession')
		)
BEGIN
	DROP INDEX [IX_Guid] ON [dbo].[PageViewSession]
END

IF EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE NAME = 'IX_PageViewUserAgentId'
			AND object_id = OBJECT_ID('PageViewSession')
		)
BEGIN
	DROP INDEX [IX_PageViewUserAgentId] ON [dbo].[PageViewSession]
END

IF EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE NAME = 'IX_SessionId'
			AND object_id = OBJECT_ID('PageViewSession')
		)
BEGIN
	DROP INDEX [IX_SessionId] ON [dbo].[PageViewSession]
END" );

                int unUsedSessionsTotal = rockContext.Database.SqlQuery<int>( @"SELECT COUNT(*) FROM PageViewSession where Id not in ( select distinct PageViewSessionId from PageView )" ).First();
                int unUsedSessionsDeleted = 0;

                bool keepMoving = true;
                int chunkSize = 25000;
                var deleteUnusedSessions = $@"delete top ({chunkSize}) from PageViewSession where Id not in (
select distinct PageViewSessionId from PageView)";
                while ( keepMoving )
                {
                    int rowsDeleted = rockContext.Database.ExecuteSqlCommand( deleteUnusedSessions );
                    keepMoving = rowsDeleted > 0;
                    unUsedSessionsDeleted += rowsDeleted;
                    if ( unUsedSessionsTotal > 0 )
                    {
                        var percentDone = Math.Round( ( decimal ) unUsedSessionsDeleted * 100 / unUsedSessionsTotal, 2 );
                        context.UpdateLastStatusMessage( $"Cleaning up unused PageViewSession data ({unUsedSessionsDeleted} of {unUsedSessionsTotal}, {percentDone}%) " );
                    }
                }

                var insertSessions = $@"
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
        );";

                rockContext.Database.CommandTimeout = _commandTimeout;
                _channelsInserted += rockContext.Database.ExecuteSqlCommand( sqlInsertSitesToChannels );
                _componentsInserted = rockContext.Database.ExecuteSqlCommand( sqlInsertPagesToComponents );
                _deviceTypesInserted = rockContext.Database.ExecuteSqlCommand( insertUserAgentToDeviceTypes );
                _sessionsInserted = rockContext.Database.ExecuteSqlCommand( insertSessions );
                var interactionService = new InteractionService( rockContext );

                rockContext.Database.ExecuteSqlCommand( @"
IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_InteractionSessionForeignId' AND object_id = OBJECT_ID('InteractionSession'))
BEGIN
    CREATE unique NONCLUSTERED INDEX [IX_InteractionSessionForeignId]
	ON [dbo].[InteractionSession] ([ForeignId])
	INCLUDE ([Id])
	where ForeignId is not null
END
" );

                var interactionCommunicationChannel = new InteractionChannelService( rockContext ).Get( Rock.SystemGuid.InteractionChannel.COMMUNICATION.AsGuid() );
                var interactionCommunicationChannelId = interactionCommunicationChannel.Id;

                // move PageView data in chunks (see http://dba.stackexchange.com/questions/1750/methods-of-speeding-up-a-huge-delete-from-table-with-no-clauses)

                // delete the indexes so they don't have to be updated as the rows are deleted
                rockContext.Database.ExecuteSqlCommand( @"
IF EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE NAME = 'IX_DateTimeViewed'
			AND object_id = OBJECT_ID('PageView')
		)
BEGIN
	DROP INDEX [IX_DateTimeViewed] ON [dbo].[PageView]
END

IF EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE NAME = 'IX_PageId'
			AND object_id = OBJECT_ID('PageView')
		)
BEGIN
	DROP INDEX IX_PageId ON [dbo].[PageView]
END

IF EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE NAME = 'IX_PageViewSessionId'
			AND object_id = OBJECT_ID('PageView')
		)
BEGIN
	DROP INDEX IX_PageViewSessionId ON [dbo].[PageView]
END

IF EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE NAME = 'IX_PersonAliasId'
			AND object_id = OBJECT_ID('PageView')
		)
BEGIN
	DROP INDEX IX_PersonAliasId ON [dbo].[PageView]
END

IF EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE NAME = 'IX_SiteId'
			AND object_id = OBJECT_ID('PageView')
		)
BEGIN
	DROP INDEX IX_SiteId ON [dbo].[PageView]
END
" );

                keepMoving = true;

                while ( keepMoving )
                {
                    var dbTransaction = rockContext.Database.BeginTransaction();
                    try
                    {
                        // keep track of where to start so that SQL doesn't have to scan the whole table when deleting
                        int insertStartId = interactionService.Queryable().Max( a => (int?)a.Id ) ?? 1;

                        rockContext.Database.CommandTimeout = _commandTimeout;
                        int rowsMoved = rockContext.Database.ExecuteSqlCommand( $@"
                            INSERT INTO [Interaction]  WITH (TABLOCK) (
		                        [InteractionDateTime]
		                        ,[Operation]
		                        ,[InteractionComponentId]
		                        ,[InteractionSessionId]
		                        ,[InteractionData]
		                        ,[PersonAliasId]
		                        ,[Guid]
		                        )
	                        SELECT TOP ({chunkSize}) *
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
			                        WHERE ChannelId != {interactionCommunicationChannelId}
                                        AND ISNULL(pv.[PageId], 0) = ISNULL(cmp.[EntityId], 0)
				                        AND isnull(pv.[PageTitle], '') = isnull(cmp.[Name], '')
			                        ) cmp
		                        CROSS APPLY (
			                        SELECT top 1 s.Id [Id]
			                        FROM [InteractionSession] s
			                        WHERE s.[ForeignId] = pv.[PageViewSessionId]
				                        AND s.ForeignId IS NOT NULL
			                        ) s
		                        where cmp.Id is not null
		                        ) x " );

                        // delete PageViews that have been moved to the Interaction table
                        rockContext.Database.CommandTimeout = _commandTimeout;
                        rockContext.Database.ExecuteSqlCommand( $"delete from PageView with (tablock) where [Guid] in (select [Guid] from Interaction WHERE Id >= {insertStartId})"  );

                        keepMoving = rowsMoved > 0;
                        _pageViewsMoved += rowsMoved;
                    }
                    finally
                    {
                        dbTransaction.Commit();
                    }

                    var percentComplete = _pageViewsTotal > 0 ? ( _pageViewsMoved * 100.0 ) / _pageViewsTotal : 100.0;
                    var statusMessage = $@"Progress: {_pageViewsMoved} of {_pageViewsTotal} ({Math.Round( percentComplete, 1 )}%) PageViews data migrated to Interactions";
                    context.UpdateLastStatusMessage( statusMessage );
                }
            }
        }

        /// <summary>
        /// Migrates the communication recipient activity data.
        /// </summary>
        /// <param name="context">The context.</param>
        private void MigrateCommunicationRecipientActivityData( IJobExecutionContext context )
        {
            using ( var rockContext = new RockContext() )
            {
                var componentEntityTypeCommunicationRecipient = EntityTypeCache.Get<Rock.Model.CommunicationRecipient>();
                var channelMediumCommunication = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_COMMUNICATION );

                // InteractionChannel for Communications already exists (in a migration)
                var interactionChannel = new InteractionChannelService( rockContext ).Get( Rock.SystemGuid.InteractionChannel.COMMUNICATION.AsGuid() );
                var interactionChannelId = interactionChannel.Id;

                // InteractionComponent : Email Subject Line as Name and Communication.Id as EntityId for any communication that has CommunicationRecipientActivity
                var insertCommunicationsAsComponentsSQL = $@"
INSERT INTO [dbo].[InteractionComponent] (
	[Name]
	,[EntityId]
	,[ChannelId]
	,[Guid]
	)
SELECT c.[Subject]
	,c.Id
	,{interactionChannelId}
	,NEWID()
FROM Communication c
WHERE (
		c.Id IN (
			SELECT cr.CommunicationId
			FROM CommunicationRecipient cr
			WHERE cr.Id IN (
					SELECT cra.CommunicationRecipientId
					FROM CommunicationRecipientActivity cra
					)
			)
		)
	AND c.Id NOT IN (
		SELECT EntityId
		FROM InteractionComponent
		WHERE ChannelId = {interactionChannelId}
		)
";

                // InteractionDeviceType: CommunicationRecipientActivity puts IP address, ClientType, OS, etc all smooshed together into ActivityDetail, 
                var populateDeviceTypeFromActivityDetail = @"
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
";

                // InteractionSession: CommunicationRecipientActivity smooshed the IP address into ActivityDetail
                var insertSessions = @"
DECLARE @ipaddressPatternSendGridMandrill NVARCHAR(max) = '%([0-9]%.%[0-9]%.%[0-9]%.%[0-9]%)%'
	DECLARE @ipaddressPatternMailgun NVARCHAR(max) = '%Opened from [0-9]%.%[0-9]%.%[0-9]%.%[0-9]% using %'
	DECLARE @ipaddressPatternMailgun_start NVARCHAR(max) = '%[0-9]%.%[0-9]%.%[0-9]%.%[0-9]%'
	DECLARE @ipaddressPatternMailgun_end NVARCHAR(max) = '% using %'
	DECLARE @ipaddressPatternClickStart NVARCHAR(max) = '% from %[0-9]%.%[0-9]%.%[0-9]%.%[0-9]% using%'
	DECLARE @ipaddressPatternClickEnd NVARCHAR(max) = '% using %'
    DECLARE @alreadyInsertedCount INT = (select count(*) from InteractionSession where ForeignGuid is not null)

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
	CROSS APPLY (SELECT TOP 1 Id FROM InteractionDeviceType WHERE DeviceTypeData = rtrim(ltrim(x.DeviceTypeData))) dt
	WHERE (@alreadyInsertedCount = 0 or cra.[Guid] NOT IN (
			SELECT ForeignGuid
			FROM InteractionSession where ForeignGuid is not null
			))
";

                rockContext.Database.CommandTimeout = _commandTimeout;
                context.UpdateLastStatusMessage( "Migrating Communication Activity to Interaction Components" );

                rockContext.Database.ExecuteSqlCommand( @"
IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_EntityId_ChannelId' AND object_id = OBJECT_ID('InteractionComponent'))
BEGIN
CREATE INDEX [IX_EntityId_ChannelId] ON [dbo].[InteractionComponent] ([EntityId],[ChannelId])
END
" );

                _componentsInserted = rockContext.Database.ExecuteSqlCommand( insertCommunicationsAsComponentsSQL );

                rockContext.Database.CommandTimeout = _commandTimeout;
                context.UpdateLastStatusMessage( "Migrating Communication Activity to Interaction DeviceTypes" );
                _deviceTypesInserted = rockContext.Database.ExecuteSqlCommand( populateDeviceTypeFromActivityDetail );


                // Interaction 
                // Remove any CommunicationRecipientActivity records that can't be converted into Interactions (because an IP Address couldn't be extracted)
                var unconvertableInteractionsSQL = @"DECLARE @ipaddressPatternSendGridMandrill NVARCHAR(max) = '%([0-9]%.%[0-9]%.%[0-9]%.%[0-9]%)%'
	DECLARE @ipaddressPatternMailgun NVARCHAR(max) = '%Opened from [0-9]%.%[0-9]%.%[0-9]%.%[0-9]% using %'
	DECLARE @ipaddressPatternMailgun_start NVARCHAR(max) = '%[0-9]%.%[0-9]%.%[0-9]%.%[0-9]%'
	DECLARE @ipaddressPatternMailgun_end NVARCHAR(max) = '% using %'
	DECLARE @ipaddressPatternClickStart NVARCHAR(max) = '% from %[0-9]%.%[0-9]%.%[0-9]%.%[0-9]% using%'
	DECLARE @ipaddressPatternClickEnd NVARCHAR(max) = '% using %'

DELETE FROM CommunicationRecipientActivity where Id in (    
SELECT --rtrim(ltrim(x.IPAddress)) [IPAddress]
		x.Id [cra.Id]
	FROM (
		-- get the IP Address and DeviceType from Opens (SendGrid or Mandrill)
		SELECT replace(replace(substring([ActivityDetail], PATINDEX(@ipaddressPatternSendGridMandrill, [ActivityDetail]), 8000), '(', ''), ')', '') [IPAddress]
			,[Id]
		FROM [CommunicationRecipientActivity]
		WHERE ActivityType = 'Opened'
			AND [ActivityDetail] NOT LIKE @ipaddressPatternMailgun
		
		UNION ALL
		
		-- get the IP Address and DeviceType from Opens (Mailgun)
		SELECT substring(x.Parsed, 0, PATINDEX(@ipaddressPatternMailgun_end, x.Parsed)) [IPAddress]
			,[Id]
		FROM (
			SELECT [Id]
				,substring([ActivityDetail], PATINDEX(@ipaddressPatternMailgun_start, [ActivityDetail]), 8000) [Parsed]
			FROM [CommunicationRecipientActivity]
			WHERE ActivityType = 'Opened'
				AND [ActivityDetail] LIKE @ipaddressPatternMailgun
			) x
		
		UNION ALL
		
		-- get the IP Address and DeviceType from Clicks (all webhooks)
		SELECT ltrim(rtrim(substring([Parsed], 0, PATINDEX(@ipaddressPatternClickEnd, [Parsed])))) [IPAddress]
			,Id
		FROM (
			SELECT substring(ActivityDetail, PATINDEX(@ipaddressPatternClickStart, [ActivityDetail]) + len(' from '), 8000) [Parsed]
				,*
			FROM [CommunicationRecipientActivity]
			WHERE ActivityType = 'Click'
			) x
		) x
	WHERE IPAddress LIKE '%[a-z]%'
			or len(IPAddress) > 15)";

                context.UpdateLastStatusMessage( "Cleaning up unconvertable Communication Activity to Interaction Sessions" );
                rockContext.Database.ExecuteSqlCommand( unconvertableInteractionsSQL );

                rockContext.Database.ExecuteSqlCommand( @"
IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_InteractionSessionForeignGuid' AND object_id = OBJECT_ID('InteractionSession'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_InteractionSessionForeignGuid]
	ON [dbo].[InteractionSession] ([ForeignGuid])
    where ForeignGuid is not null
END
" );

                rockContext.Database.CommandTimeout = _commandTimeout;
                context.UpdateLastStatusMessage( "Migrating Communication Activity to Interaction Sessions" );
                _sessionsInserted = rockContext.Database.ExecuteSqlCommand( insertSessions );

                int chunkSize = 25000;
                var populateInteraction = $@"
BEGIN
	DECLARE @ipaddressPatternClickStart NVARCHAR(max) = '% from %[0-9]%.%[0-9]%.%[0-9]%.%[0-9]% using%'

	-- populate Interaction
	insert into Interaction (
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
	SELECT top {chunkSize} cra.ActivityDateTime [InteractionDateTime]
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
	CROSS APPLY (select top 1 icmp.Id from InteractionComponent icmp WHERE icmp.ChannelId = {interactionChannelId} AND icmp.EntityId = c.Id) icmp
     where cra.[Guid] not in (select ForeignGuid from Interaction where ForeignGuid is not null)
END
";

                bool keepMoving = true;

                while ( keepMoving )
                {
                    var dbTransaction = rockContext.Database.BeginTransaction();
                    try
                    {
                        rockContext.Database.CommandTimeout = _commandTimeout;
                        int rowsMoved = rockContext.Database.ExecuteSqlCommand( populateInteraction );
                        keepMoving = rowsMoved > 0;
                        _communicationRecipientActivityMoved += rowsMoved;
                    }
                    finally
                    {
                        dbTransaction.Commit();
                    }

                    var percentComplete = _communicationRecipientActivityTotal > 0 ? ( _communicationRecipientActivityMoved * 100.0 ) / _communicationRecipientActivityTotal : 100.0;
                    var statusMessage = $@"Progress: {_communicationRecipientActivityMoved} of {_communicationRecipientActivityTotal} ({Math.Round( percentComplete, 1 )}%) Communication Recipient Activity data migrated to Interactions";
                    context.UpdateLastStatusMessage( statusMessage );
                }
            }
        }
    }
}
