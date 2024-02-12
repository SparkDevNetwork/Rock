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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Rock.BulkImport;
using Rock.Data;
using Rock.Transactions;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Data access and service class for <see cref="Rock.Model.Interaction"/> entity objects.
    /// </summary>
    public partial class InteractionService
    {
        /// <summary>
        /// Adds the interaction.
        /// </summary>
        /// <param name="interactionComponentId">The interaction component identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="operation">The operation.</param>
        /// <param name="interactionSummary">The interaction summary.</param>
        /// <param name="interactionData">The interaction data.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="deviceApplication">The device application.</param>
        /// <param name="deviceOs">The device os.</param>
        /// <param name="deviceClientType">Type of the device client.</param>
        /// <param name="deviceTypeData">The device type data.</param>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="browserSessionId">The browser session identifier.</param>
        /// <returns></returns>
        public Interaction AddInteraction(
            int interactionComponentId,
            int? entityId,
            string operation,
            string interactionSummary,
            string interactionData,
            int? personAliasId,
            DateTime dateTime,
            string deviceApplication,
            string deviceOs,
            string deviceClientType,
            string deviceTypeData,
            string ipAddress,
            Guid? browserSessionId )
        {
            Interaction interaction = CreateInteraction( interactionComponentId, entityId, operation, interactionSummary, interactionData, personAliasId, dateTime, deviceApplication, deviceOs, deviceClientType, deviceTypeData, ipAddress, browserSessionId );
            this.Add( interaction );
            return interaction;
        }

        /// <summary>
        /// Creates a new interaction.
        /// </summary>
        /// <param name="interactionComponentId">The interaction component identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="operation">The operation.</param>
        /// <param name="interactionSummary">The interaction summary.</param>
        /// <param name="interactionData">The interaction data.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="deviceApplication">The device application.</param>
        /// <param name="deviceOs">The device os.</param>
        /// <param name="deviceClientType">Type of the device client.</param>
        /// <param name="deviceTypeData">The device type data.</param>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="browserSessionId">The browser session identifier.</param>
        /// <returns></returns>
        public Interaction CreateInteraction( int interactionComponentId, int? entityId, string operation, string interactionSummary, string interactionData, int? personAliasId, DateTime dateTime, string deviceApplication, string deviceOs, string deviceClientType, string deviceTypeData, string ipAddress, Guid? browserSessionId )
        {
            Interaction interaction = new Interaction();
            interaction.InteractionComponentId = interactionComponentId;
            interaction.EntityId = entityId;
            interaction.Operation = operation;
            interaction.SetInteractionData( interactionData );
            interaction.InteractionDateTime = dateTime;
            interaction.PersonAliasId = personAliasId;
            interaction.InteractionSummary = interactionSummary;

            int? deviceTypeId = null;
            if ( deviceApplication.IsNotNullOrWhiteSpace() && deviceOs.IsNotNullOrWhiteSpace() && deviceClientType.IsNotNullOrWhiteSpace() )
            {
                deviceTypeId = this.GetInteractionDeviceTypeId( deviceApplication, deviceOs, deviceClientType, deviceTypeData );
            }

            // If we don't have an BrowserSessionId, IPAddress or a devicetype, there is nothing useful about the session
            // but at least one of these has a value, then we should lookup or create a session
            if ( browserSessionId.HasValue || ipAddress.IsNotNullOrWhiteSpace() || deviceTypeId.HasValue )
            {
                var interactionSessionId = GetInteractionSessionId( browserSessionId ?? Guid.NewGuid(), ipAddress, deviceTypeId, interaction.InteractionDateKey );
                interaction.InteractionSessionId = interactionSessionId;
            }

            return interaction;
        }

        /// <summary>
        /// The ua parser
        /// </summary>
        private static UAParser.Parser _uaParser = UAParser.Parser.GetDefault();

        /// <summary>
        /// Parse the user agent string from a HTTP Request to extract information about the client device.
        /// </summary>
        /// <param name="userAgent"></param>
        /// <param name="deviceOs"></param>
        /// <param name="deviceApplication"></param>
        /// <param name="deviceClientType"></param>
        public static void ParseUserAgentString( string userAgent, out string deviceOs, out string deviceApplication, out string deviceClientType )
        {
            userAgent = userAgent ?? string.Empty;

            deviceOs = _uaParser.ParseOS( userAgent ).ToString();
            deviceApplication = _uaParser.ParseUserAgent( userAgent ).ToString();
            deviceClientType = InteractionDeviceType.GetClientType( userAgent );
        }

        /// <summary>
        /// Creates the interaction.
        /// </summary>
        /// <param name="interactionComponentId">The interaction component identifier.</param>
        /// <param name="userAgent">The user agent.</param>
        /// <param name="url">The URL.</param>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="browserSessionId">The browser session identifier.</param>
        /// <returns></returns>
        public Interaction CreateInteraction( int interactionComponentId, string userAgent, string url, string ipAddress, Guid? browserSessionId )
        {
            userAgent = userAgent ?? string.Empty;

            string deviceOs;
            string deviceApplication;
            string deviceClientType;

            ParseUserAgentString( userAgent, out deviceOs, out deviceApplication, out deviceClientType );

            var interaction = CreateInteraction( interactionComponentId, null, null, string.Empty, null, null, RockDateTime.Now, deviceApplication, deviceOs, deviceClientType, userAgent, ipAddress, browserSessionId );

            interaction.SetUTMFieldsFromURL( url );

            return interaction;
        }

        /// <summary>
        /// Adds the interaction.
        /// </summary>
        /// <param name="interactionComponentId">The interaction component identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="operation">The operation.</param>
        /// <param name="interactionData">The interaction data.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="deviceApplication">The device application.</param>
        /// <param name="deviceOs">The device os.</param>
        /// <param name="deviceClientType">Type of the device client.</param>
        /// <param name="deviceTypeData">The device type data.</param>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="browserSessionId">The browser session identifier (RockSessionId).</param>
        /// <returns></returns>
        public Interaction AddInteraction(
            int interactionComponentId,
            int? entityId,
            string operation,
            string interactionData,
            int? personAliasId,
            DateTime dateTime,
            string deviceApplication,
            string deviceOs,
            string deviceClientType,
            string deviceTypeData,
            string ipAddress,
            Guid? browserSessionId )
        {
            return AddInteraction( interactionComponentId, entityId, operation, string.Empty, interactionData, personAliasId, dateTime, deviceApplication, deviceOs, deviceClientType, deviceTypeData, ipAddress, browserSessionId );
        }

        /// <summary>
        /// Adds the interaction.
        /// </summary>
        /// <param name="interactionComponentId">The interaction component identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="operation">The operation.</param>
        /// <param name="interactionData">The interaction data.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="deviceApplication">The device application.</param>
        /// <param name="deviceOs">The device os.</param>
        /// <param name="deviceClientType">Type of the device client.</param>
        /// <param name="deviceTypeData">The device type data.</param>
        /// <param name="ipAddress">The ip address.</param>
        /// <returns></returns>
        public Interaction AddInteraction(
            int interactionComponentId,
            int? entityId,
            string operation,
            string interactionData,
            int? personAliasId,
            DateTime dateTime,
            string deviceApplication,
            string deviceOs,
            string deviceClientType,
            string deviceTypeData,
            string ipAddress )
        {
            return AddInteraction( interactionComponentId, entityId, operation, string.Empty, interactionData, personAliasId, dateTime, deviceApplication, deviceOs, deviceClientType, deviceTypeData, ipAddress, null );
        }

        private const string DeviceTypeIdLookupCacheKey = "InteractionServiceDeviceTypeIdLookup";

        /// <summary>
        /// Gets the interaction device type identifier.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="operatingSystem">The operating system.</param>
        /// <param name="clientType">Type of the client.</param>
        /// <param name="deviceTypeData">The device type data.</param>
        /// <returns></returns>
        public int GetInteractionDeviceTypeId( string application, string operatingSystem, string clientType, string deviceTypeData )
        {
            var lookupTable = RockCacheManager<object>.Instance.Get( DeviceTypeIdLookupCacheKey ) as ConcurrentDictionary<string, int>;

            if ( lookupTable == null )
            {
                lookupTable = new ConcurrentDictionary<string, int>();
                RockCacheManager<object>.Instance.AddOrUpdate( DeviceTypeIdLookupCacheKey, lookupTable );
            }

            var lookupKey = $"{application}|{operatingSystem}|{clientType}";
            int? deviceTypeId = lookupTable.GetValueOrNull( lookupKey );
            if ( deviceTypeId == null )
            {
                deviceTypeId = GetOrCreateInteractionDeviceTypeId( application, operatingSystem, clientType, deviceTypeData );
                lookupTable.AddOrReplace( lookupKey, deviceTypeId.Value );
            }

            return deviceTypeId.Value;
        }

        /// <summary>
        /// Gets the interaction device type. If it can't be found, a new InteractionDeviceType record will be created and returned.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="operatingSystem">The operating system.</param>
        /// <param name="clientType">Type of the client.</param>
        /// <param name="deviceTypeData">The device type data (either a plain DeviceType name or the whole useragent string).</param>
        /// <returns></returns>
        [Obsolete]
        [RockObsolete( "1.11" )]
        public InteractionDeviceType GetInteractionDeviceType( string application, string operatingSystem, string clientType, string deviceTypeData )
        {
            /*
             * 2020-10-22 ETD
             * This method was used by GetInteractionDeviceTypeId(). Discussed with Mike and Nick and it was
             * decided to mark it as obsolete and create private method GetOrCreateInteractionDeviceTypeId()
             * instead.
             */

            var rockContext = new RockContext();
            InteractionDeviceTypeService interactionDeviceTypeService = new InteractionDeviceTypeService( rockContext );
            InteractionDeviceType interactionDeviceType = interactionDeviceTypeService.Queryable()
                .Where( a => a.Application == application && a.OperatingSystem == operatingSystem && a.ClientType == clientType )
                .FirstOrDefault();

            if ( interactionDeviceType == null )
            {
                interactionDeviceType = new InteractionDeviceType();
                interactionDeviceType.DeviceTypeData = deviceTypeData;
                interactionDeviceType.ClientType = clientType;
                interactionDeviceType.OperatingSystem = operatingSystem;
                interactionDeviceType.Application = application;
                interactionDeviceType.Name = string.Format( "{0} - {1}", operatingSystem, application );
                interactionDeviceTypeService.Add( interactionDeviceType );
                rockContext.SaveChanges();
            }

            return interactionDeviceType;
        }

        /// <summary>
        /// Gets the InteractionDeveiceTypeId or creates one if it does not exist.
        /// This method uses its own RockContext so it doesn't interfere with the Interaction.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="operatingSystem">The operating system.</param>
        /// <param name="clientType">Type of the client.</param>
        /// <param name="deviceTypeData">The device type data.</param>
        /// <returns>InteractionDeveiceType.Id</returns>
        private int GetOrCreateInteractionDeviceTypeId( string application, string operatingSystem, string clientType, string deviceTypeData )
        {
            var rockContext = new RockContext();
            InteractionDeviceTypeService interactionDeviceTypeService = new InteractionDeviceTypeService( rockContext );
            InteractionDeviceType interactionDeviceType = interactionDeviceTypeService.Queryable()
                .Where( a => a.Application == application && a.OperatingSystem == operatingSystem && a.ClientType == clientType )
                .FirstOrDefault();

            if ( interactionDeviceType == null )
            {
                interactionDeviceType = new InteractionDeviceType
                {
                    DeviceTypeData = deviceTypeData,
                    ClientType = clientType,
                    OperatingSystem = operatingSystem,
                    Application = application,
                    Name = string.Format( "{0} - {1}", operatingSystem, application )
                };

                interactionDeviceTypeService.Add( interactionDeviceType );
                rockContext.SaveChanges();
            }

            return interactionDeviceType.Id;
        }

        /// <summary>
        /// Gets the interaction session. If browserSessionId isn't specified, or it can't be found, a new InteractionSession record will be created and returned.
        /// </summary>
        /// <param name="browserSessionId">The browser session identifier (RockSessionId).</param>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="interactionDeviceTypeId">The interaction device type identifier.</param>
        /// <returns></returns>
        public InteractionSession GetInteractionSession( Guid? browserSessionId, string ipAddress, int? interactionDeviceTypeId )
        {
            var interactionSessionId = GetInteractionSessionId( browserSessionId ?? Guid.NewGuid(), ipAddress, interactionDeviceTypeId );
            return new InteractionSessionService( this.Context as RockContext ).GetNoTracking( interactionSessionId );
        }

        /// <summary>
        /// Ensures there is an InteractionSessionId for the specified browserSessionId and returns it
        /// </summary>
        /// <param name="browserSessionId">The browser session identifier.</param>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="interactionDeviceTypeId">The interaction device type identifier.</param>
        /// <param name="interactionDateKey">The interaction date key.</param>
        /// <returns></returns>
        private int GetInteractionSessionId( Guid browserSessionId, string ipAddress, int? interactionDeviceTypeId, int? interactionDateKey = null )
        {
            object deviceTypeId = DBNull.Value;
            if ( interactionDeviceTypeId != null )
            {
                deviceTypeId = interactionDeviceTypeId;
            }

            var currentDateTime = RockDateTime.Now;
            interactionDateKey = interactionDateKey ?? currentDateTime.ToString( "yyyyMMdd" ).AsInteger();

            // To make this more thread safe and to avoid overhead of an extra database call, etc, run a SQL block to Get/Create in one quick SQL round trip
            int interactionSessionId = this.Context.Database.SqlQuery<int>(
                @"BEGIN
                    DECLARE @InteractionSessionId INT;

                    SELECT @InteractionSessionId = Id
                    FROM InteractionSession
                    WHERE [Guid] = @browserSessionId

                    IF (@InteractionSessionId IS NULL)
                    BEGIN
                        INSERT [dbo].[InteractionSession] (
                            [DeviceTypeId]
                            ,[IpAddress]
                            ,[Guid]
                            ,[CreatedDateTime]
                            ,[ModifiedDateTime]
                            ,[SessionStartDateKey]
                            )
                        OUTPUT inserted.Id
                        VALUES (
                            @interactionDeviceTypeId
                            ,@ipAddress
                            ,@browserSessionId
                            ,@currentDateTime
                            ,@currentDateTime
                            ,@sessionStartDateKey
                            )
                    END
                    ELSE
                    BEGIN
                        SELECT @InteractionSessionId
                    END
                END",
                new SqlParameter( "@browserSessionId", browserSessionId ),
                new SqlParameter( "@ipAddress", ipAddress.Truncate( 45 ) ),
                new SqlParameter( "@interactionDeviceTypeId", deviceTypeId ),
                new SqlParameter( "@currentDateTime", currentDateTime ),
                new SqlParameter( "@sessionStartDateKey", interactionDateKey ) )
                .FirstOrDefault();

            return interactionSessionId;
        }

        /// <summary>
        /// Bulk updates null Interaction.PersonAliasId for the provided PersonalDeviceId
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="personalDeviceId">The personal device identifier.</param>
        /// <returns></returns>
        public int UpdateInteractionsWithPersonAliasIdForDeviceId( int personAliasId, int personalDeviceId )
        {
            var interactionsCount = Queryable()
                .Where( i => i.PersonalDeviceId == personalDeviceId )
                .Where( i => i.PersonAliasId == null ).Count();

            if ( interactionsCount > 0 )
            {
                var interactions = Queryable()
                    .Where( i => i.PersonalDeviceId == personalDeviceId )
                    .Where( i => i.PersonAliasId == null );

                // Use BulkUpdate to set the PersonAliasId
                new RockContext().BulkUpdate( interactions, i => new Interaction { PersonAliasId = personAliasId } );
            }

            return interactionsCount;
        }

        /// <summary>
        /// Create an interaction for a web page.
        /// </summary>
        /// <param name="interactionInfo"></param>
        /// <param name="immediate"></param>
        internal void RegisterPageInteraction( PageInteractionInfo interactionInfo, bool immediate = false )
        {
            // Get the Page.
            var page = PageCache.Get( interactionInfo.PageId );
            if ( page == null )
            {
                throw new Exception( $"Invalid page reference. [PageId={interactionInfo.PageId}]" );
            }

            // Get the Site.
            var site = SiteCache.Get( page.SiteId );

            // Get the Person.
            int? personAliasId = null;
            if ( !string.IsNullOrWhiteSpace( interactionInfo.UserIdKey ) )
            {
                personAliasId = IdHasher.Instance.GetId( interactionInfo.UserIdKey );
                if ( personAliasId == null )
                {
                    throw new Exception( $"Invalid user reference. [UserIdKey={interactionInfo.UserIdKey}]" );
                }
            }

            // Get the interaction channel and component for a page interaction.
            var dvWebsiteChannelType = DefinedValueCache.Get( SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE );

            // Use the Page Title as the InteractionSummary.
            var title = page.BrowserTitle ?? page.PageTitle ?? string.Empty;

            if ( title.Contains( "|" ) )
            {
                // Remove the site name.
                title = title.Substring( 0, title.LastIndexOf( '|' ) ).Trim();
            }

            // Create the interaction transaction.
            var interactionTransactionInfo = new InteractionTransactionInfo
            {
                GetValuesFromHttpRequest = false,
                PersonAliasId = personAliasId,
                InteractionData = interactionInfo.PageRequestUrl,
                InteractionTimeToServe = interactionInfo.PageRequestTimeToServe,
                InteractionChannelCustomIndexed1 = interactionInfo.UrlReferrerHostAddress,
                InteractionChannelCustom2 = interactionInfo.UrlReferrerSearchTerms,
                InteractionSummary = title,
                UserAgent = interactionInfo.UserAgent,
                IPAddress = interactionInfo.UserHostAddress,
                BrowserSessionId = interactionInfo.BrowserSessionGuid
            };

            var pageViewTransaction = new InteractionTransaction( dvWebsiteChannelType,
                site,
                page,
                interactionTransactionInfo );

            // Either queue the interaction to be sent or
            // immediately post it to the database.
            if ( immediate )
            {
                pageViewTransaction.Execute();
            }
            else
            {
                pageViewTransaction.Enqueue();
            }
        }

        #region Queryables that return Page Views

        /// <summary>
        /// Gets a queryable of Page View Interactions, limited to the specified sites and pages on those sites.
        /// </summary>
        /// <param name="siteIds">The site ids.</param>
        /// <param name="pageIds">The page ids.</param>
        /// <returns>IQueryable&lt;Interaction&gt;.</returns>
        public IQueryable<Interaction> GetPageViewsByPage( int[] siteIds, int[] pageIds )
        {
            var pageViewComponentIdsQry = new InteractionComponentService( this.Context as RockContext ).QueryByPages( siteIds, pageIds ).Select( a => a.Id );
            return this.Queryable().Where( a => pageViewComponentIdsQry.Contains( a.InteractionComponentId ) && a.Operation == "View" );
        }

        /// <summary>
        /// Gets a queryable of Page View Interactions, limited to the specified sites
        /// </summary>
        /// <param name="siteIds">The site ids.</param>
        /// <returns>IQueryable&lt;Interaction&gt;.</returns>
        public IQueryable<Interaction> GetPageViewsBySite( int[] siteIds )
        {
            var pageViewComponentIdsQry = new InteractionComponentService( this.Context as RockContext ).QueryBySites( siteIds ).Select( a => a.Id );
            return this.Queryable().Where( a => pageViewComponentIdsQry.Contains( a.InteractionComponentId ) && a.Operation == "View" );
        }

        #endregion Queryables that return Page Views

        #region BulkImport related

        /// <summary>
        /// BulkInserts Interaction Records
        /// </summary>
        /// <remarks>
        /// If any PersonAliasId references a PersonAliasId record that doesn't exist, the field value will be set to null.
        /// Also, if the InteractionComponent Id (or Guid) is specified, but references a Interaction Component record that doesn't exist
        /// the Interaction will not be recorded.
        /// </remarks>
        /// <param name="interactionsImport">The interactions import.</param>
        internal static void BulkInteractionImport( InteractionsImport interactionsImport )
        {
            if ( interactionsImport == null )
            {
                throw new Exception( "InteractionsImport must be assigned a value." );
            }

            var interactionImportList = interactionsImport.Interactions;

            if ( interactionImportList == null || !interactionImportList.Any() )
            {
                // if there aren't any return
                return;
            }

            /* 2020-05-14 MDP
             * Make sure that all the PersonAliasIds in the import exist in the database.
             * For performance reasons, look them up all at one and keep a list of valid ones.

             * If there are any PersonAliasIds that aren't valid,
             * we decided that just set the PersonAliasId to null (we want ignore bad data).
             */

            HashSet<int> validPersonAliasIds = interactionsImport.GetValidPersonAliasIds();

            List<Interaction> interactionsToInsert = new List<Interaction>();

            foreach ( InteractionImport interactionImport in interactionImportList )
            {
                if ( interactionImport.Interaction == null )
                {
                    throw new ArgumentNullException( "InteractionImport.Interaction can not be null" );
                }

                // Determine which Channel this should be set to
                if ( interactionImport.InteractionChannelId.HasValue )
                {
                    // make sure it is a valid Id
                    interactionImport.InteractionChannelId = InteractionChannelCache.Get( interactionImport.InteractionChannelId.Value )?.Id;
                }

                // Determine which Channel Type Medium this should be set to
                if ( interactionImport.InteractionChannelChannelTypeMediumValueId.HasValue )
                {
                    // make sure it is a valid Id
                    interactionImport.InteractionChannelChannelTypeMediumValueId = DefinedValueCache.Get( interactionImport.InteractionChannelChannelTypeMediumValueId.Value )?.Id;
                }

                if ( !interactionImport.InteractionChannelChannelTypeMediumValueId.HasValue )
                {
                    if ( interactionImport.InteractionChannelChannelTypeMediumValueGuid.HasValue )
                    {
                        interactionImport.InteractionChannelChannelTypeMediumValueId = DefinedValueCache.GetId( interactionImport.InteractionChannelChannelTypeMediumValueGuid.Value );
                    }
                }

                if ( !interactionImport.InteractionChannelId.HasValue )
                {
                    if ( interactionImport.InteractionChannelGuid.HasValue )
                    {
                        interactionImport.InteractionChannelId = InteractionChannelCache.GetId( interactionImport.InteractionChannelGuid.Value );
                    }

                    // if InteractionChannelId is still null, lookup (or create) an InteractionChannel from InteractionChannelForeignKey (if it is specified) 
                    if ( interactionImport.InteractionChannelId == null && interactionImport.InteractionChannelForeignKey.IsNotNullOrWhiteSpace() )
                    {
                        interactionImport.InteractionChannelId = InteractionChannelCache.GetCreateChannelIdByForeignKey( interactionImport.InteractionChannelForeignKey, interactionImport.InteractionChannelName, interactionImport.InteractionChannelChannelTypeMediumValueId );
                    }
                    else
                    {
                        /* 2020-05-14 MDP
                            Discussed this and decided that if we tried InteractionChannelId and InteractionChannelGuid, and InteractionChannelForeignKey was not specified,
                            we'll just skip over this record
                         */
                        continue;
                    }
                }

                // Determine which Component this should be set to
                if ( interactionImport.InteractionComponentId.HasValue )
                {
                    // make sure it is a valid Id
                    interactionImport.InteractionComponentId = InteractionComponentCache.Get( interactionImport.InteractionComponentId.Value )?.Id;
                }

                if ( !interactionImport.InteractionComponentId.HasValue )
                {
                    if ( interactionImport.InteractionComponentGuid.HasValue )
                    {
                        interactionImport.InteractionComponentId = InteractionComponentCache.GetId( interactionImport.InteractionComponentGuid.Value );
                    }

                    // if InteractionComponentId is still null, lookup (or create) an InteractionComponent from the ForeignKey and ChannelId
                    if ( interactionImport.InteractionComponentForeignKey.IsNotNullOrWhiteSpace() )
                    {
                        interactionImport.InteractionComponentId = InteractionComponentCache.GetComponentIdByForeignKeyAndChannelId(
                            interactionImport.InteractionComponentForeignKey,
                            interactionImport.InteractionChannelId.Value,
                            interactionImport.InteractionComponentName );
                    }
                    else
                    {
                        /* 2020-05-14 MDP
                            Discussed this and decided that and if we tried InteractionComponentId and InteractionComponentGuid, and InteractionComponentForeignKey was not specified,
                            we'll just skip over this record
                         */
                        continue;
                    }
                }
            }

            foreach ( InteractionImport interactionImport in interactionImportList.Where( a => a.InteractionComponentId.HasValue ) )
            {
                Interaction interaction = new Interaction
                {
                    InteractionComponentId = interactionImport.InteractionComponentId.Value
                };

                interaction.InteractionDateTime = interactionImport.Interaction.InteractionDateTime;

                // if operation is over 25, truncate it
                interaction.Operation = interactionImport.Interaction.Operation.Truncate( 25 );

                interaction.InteractionComponentId = interactionImport.InteractionComponentId.Value;
                interaction.EntityId = interactionImport.Interaction.EntityId;
                if ( interactionImport.Interaction.RelatedEntityTypeId.HasValue )
                {
                    /* 2020-05-14 MDP
                     * We want to ignore bad data, so first see if the RelatedEntityTypeId exists by looking it up in a cache.
                     * If it doesn't exist, it'll set RelatedEntityTypeId to null (so that we don't get a database constraint error)
                    */

                    interaction.RelatedEntityTypeId = EntityTypeCache.Get( interactionImport.Interaction.RelatedEntityTypeId.Value )?.Id;
                }

                interaction.RelatedEntityId = interactionImport.Interaction.RelatedEntityId;

                if ( interactionImport.Interaction.PersonAliasId.HasValue )
                {
                    /* 2020-05-14 MDP
                     * We want to ignore bad data, so see if the specified PersonAliasId exists in the validPersonAliasIds that we lookup up
                     * If it doesn't exist, we'll leave interaction.PersonAliasId null (so that we don't get a database constraint error)
                    */

                    if ( validPersonAliasIds.Contains( interactionImport.Interaction.PersonAliasId.Value ) )
                    {
                        interaction.PersonAliasId = interactionImport.Interaction.PersonAliasId.Value;
                    }
                }

                // BulkImport doesn't include Session information TODO???
                interaction.InteractionSessionId = null;

                // if the summary is over 500 chars, truncate with addEllipsis=true
                interaction.InteractionSummary = interactionImport.Interaction.InteractionSummary.Truncate( 500, true );

                interaction.InteractionData = interactionImport.Interaction.InteractionData;
                interaction.PersonalDeviceId = interactionImport.Interaction.PersonalDeviceId;

                interaction.InteractionEndDateTime = interactionImport.Interaction.InteractionEndDateTime;

                // Campaign related fields, we'll truncate those if they are too long
                interaction.Source = interactionImport.Interaction.Source.Truncate( 25 );
                interaction.Medium = interactionImport.Interaction.Medium.Truncate( 25 );
                interaction.Campaign = interactionImport.Interaction.Campaign.Truncate( 50 );
                interaction.Content = interactionImport.Interaction.Content.Truncate( 50 );
                interaction.Term = interactionImport.Interaction.Term.Truncate( 50 );
                interaction.ForeignId = interactionImport.Interaction.ForeignId;
                interaction.ForeignKey = interactionImport.Interaction.ForeignKey;
                interaction.ForeignGuid = interactionImport.Interaction.ForeignGuid;

                interaction.ChannelCustom1 = interactionImport.Interaction.ChannelCustom1.Truncate( 500, true );
                interaction.ChannelCustom2 = interactionImport.Interaction.ChannelCustom2.Truncate( 2000, true );
                interaction.ChannelCustomIndexed1 = interactionImport.Interaction.ChannelCustomIndexed1.Truncate( 500, true );
                interaction.InteractionLength = interactionImport.Interaction.InteractionLength;
                interaction.InteractionTimeToServe = interactionImport.Interaction.InteractionTimeToServe;

                interactionsToInsert.Add( interaction );
            }

            using ( var rockContext = new RockContext() )
            {
                rockContext.BulkInsert( interactionsToInsert );
            }

            // This logic is normally handled in the Interaction.PostSave method, but since the BulkInsert bypasses those
            // model hooks, streaks need to be updated here. Also, it is not necessary for this logic to complete before this
            // transaction can continue processing and exit, so update the streak using a task.

            // Only launch this task if there are StreakTypes configured that have interactions. Otherwise several
            // database calls are made only to find out there are no streak types defined.
            if ( StreakTypeCache.All().Any( s => s.IsInteractionRelated ) )
            {
                // Ids do not exit for the interactions in the collection since they were bulk imported.
                // Read their ids from their guids and append the id.
                var insertedGuids = interactionsToInsert.Select( i => i.Guid ).ToList();

                var interactionIds = new InteractionService( new RockContext() ).Queryable()
                                        .Where( i => insertedGuids.Contains( i.Guid ) )
                                        .Select( i => new { i.Id, i.Guid } )
                                        .ToList();

                foreach( var interactionId in interactionIds )
                {
                    var interaction = interactionsToInsert.Where( i => i.Guid == interactionId.Guid ).FirstOrDefault();
                    if ( interaction != null )
                    {
                        interaction.Id = interactionId.Id;
                    }
                }

                // Launch task
                interactionsToInsert.ForEach( i => Task.Run( () => StreakTypeService.HandleInteractionRecord( i.Id ) ) );
            }
            
        }
    }

    #endregion BulkImport related


    #region Support Classes

    /// <summary>
    /// Describes a web page interaction.
    /// </summary>
    public class PageInteractionInfo
    {
        /// <summary>
        /// The unique identifier of the page.
        /// </summary>
        public int PageId { get; set; }

        /// <summary>
        /// The name of the action being registered.
        /// </summary>
        public string ActionName { get; set; } = "View";

        /// <summary>
        /// The unique identifier for the browser session.
        /// </summary>
        public Guid BrowserSessionGuid { get; set; }

        /// <summary>
        /// The URL requested by the client browser.
        /// </summary>
        public string PageRequestUrl { get; set; }

        /// <summary>
        /// The server date and time on which the page was requested.
        /// </summary>
        public DateTime PageRequestDateTime { get; set; }

        /// <summary>
        /// The time in seconds required to serve the initial page request.
        /// </summary>
        public double? PageRequestTimeToServe { get; set; }

        /// <summary>
        /// Gets the raw user agent string of the client browser.
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// Gets the IP host address of the remote client.
        /// </summary>
        public string UserHostAddress { get; set; }

        /// <summary>
        /// Gets the DNS host name or IP address of the client's previous request that linked to the current URL.
        /// </summary>
        public string UrlReferrerHostAddress { get; set; }

        /// <summary>
        /// Gets the query search terms of the client's previous request that linked to the current URL.
        /// </summary>
        public string UrlReferrerSearchTerms { get; set; }

        /// <summary>
        /// The unique identifier of the user initiating this interaction.
        /// </summary>
        public string UserIdKey { get; set; }
    }

    #endregion
}