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
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.BulkImport;
using Rock.Core;
using Rock.Data;
using Rock.Net.Geolocation;
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
        /// Creates a new interaction using the provided interaction info object
        /// and adds the new interaction to the context.
        /// <remarks>
        /// <para>
        /// The new interaction will not be saved until you call <see cref="DbContext.SaveChanges()"/>.
        /// </para>
        /// <para>
        /// The following entities will be looked up (or added and auto-saved to the database) when
        /// possible, with the session ID being added to the returned interaction instance:
        /// <see cref="InteractionSessionLocation" />,
        /// <see cref="InteractionDeviceType" />,
        /// <see cref="InteractionSession" />.
        /// </para>
        /// </remarks>
        /// </summary>
        /// <param name="info">The information that will be used to create the interaction.</param>
        /// <returns>A new interaction.</returns>
        internal Interaction AddInteraction( InteractionInfo info )
        {
            var interaction = CreateInteraction( info );
            this.Add( interaction );
            return interaction;
        }

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
            /*
                5/22/2024 - JPH

                We should prefer the `AddInteraction( InteractionInfo info )` overload of this method for future use,
                as this method signature is difficult to call and extend. If changes are needed to this method, you
                should instead take the opportunity to migrate to the simpler method signature and add properties to
                the `InteractionInfo` POCO as needed.

                #techdebt: We should consider deprecating this method in the future.

                Reason: Extend InteractionService to add geolocation support
             */

            Interaction interaction = CreateInteraction( interactionComponentId, entityId, operation, interactionSummary, interactionData, personAliasId, dateTime, deviceApplication, deviceOs, deviceClientType, deviceTypeData, ipAddress, browserSessionId );
            this.Add( interaction );
            return interaction;
        }

        /// <summary>
        /// Creates a new interaction using the provided interaction info object.
        /// <remarks>
        /// <para>
        /// The new interaction will not be added to the context or saved to the database.
        /// </para>
        /// <para>
        /// The following entities will be looked up (or added and auto-saved to the database) when
        /// possible, with the session ID being added to the returned interaction instance:
        /// <see cref="InteractionSessionLocation" />,
        /// <see cref="InteractionDeviceType" />,
        /// <see cref="InteractionSession" />.
        /// </para>
        /// </remarks>
        /// </summary>
        /// <param name="info">The information that will be used to create the interaction.</param>
        /// <returns>A new interaction.</returns>
        internal Interaction CreateInteraction( InteractionInfo info )
        {
            if ( info == null )
            {
                return new Interaction();
            }

            var interaction = new Interaction
            {
                Guid = info.InteractionGuid ?? Guid.NewGuid(),
                InteractionDateTime = info.InteractionDateTime,
                Operation = info.Operation.IsNotNullOrWhiteSpace() ? info.Operation.Trim() : "View",
                InteractionComponentId = info.InteractionComponentId,
                EntityId = info.EntityId,
                RelatedEntityTypeId = info.RelatedEntityTypeId,
                RelatedEntityId = info.RelatedEntityId,
                PersonAliasId = info.PersonAliasId,
                InteractionSummary = info.InteractionSummary?.Trim(),
                InteractionEndDateTime = info.InteractionEndDateTime,
                ChannelCustom1 = info.ChannelCustom1?.Trim(),
                ChannelCustom2 = info.ChannelCustom2?.Trim(),
                ChannelCustomIndexed1 = info.ChannelCustomIndexed1?.Trim(),
                InteractionLength = info.InteractionLength,
                InteractionTimeToServe = info.InteractionTimeToServe
            };

            interaction.SetInteractionData( info.InteractionData?.Trim() );

            // Try to set the UTM fields first from the provided URL, and only
            // fall back to using the provided info's values if they're not
            // already set from the URL. This is to maintain existing behavior
            // that this new method will be used to replace:
            // https://github.com/SparkDevNetwork/Rock/blob/df53f91052621f5e341fb6888a703254d575dfb8/Rock/Transactions/InteractionTransaction.cs#L300-L304
            interaction.SetUTMFieldsFromURL( info.InteractionData );

            if ( interaction.Source.IsNullOrWhiteSpace() )
            {
                interaction.Source = info.Source?.Trim();
            }

            if ( interaction.Medium.IsNullOrWhiteSpace() )
            {
                interaction.Medium = info.Medium?.Trim();
            }

            if ( interaction.Campaign.IsNullOrWhiteSpace() )
            {
                interaction.Campaign = info.Campaign?.Trim();
            }

            if ( interaction.Content.IsNullOrWhiteSpace() )
            {
                interaction.Content = info.Content?.Trim();
            }

            if ( interaction.Term.IsNullOrWhiteSpace() )
            {
                interaction.Term = info.Term?.Trim();
            }

            // If geolocation data hasn't yet been set, look it up from the cache or IPGeoLookup service
            // so we only have to make a single database call to lookup or add a session location instance.
            if ( !info.GeolocationLookupDateTime.HasValue && info.IpAddress.IsNotNullOrWhiteSpace() )
            {
                var geolocation = IpGeoLookup.Instance.GetGeolocation( info.IpAddress );
                if ( geolocation != null )
                {
                    info.GeolocationIpAddress = geolocation.IpAddress;
                    info.GeolocationLookupDateTime = geolocation.LookupDateTime;
                    info.City = geolocation.City;
                    info.RegionName = geolocation.RegionName;
                    info.RegionCode = geolocation.RegionCode;
                    info.RegionValueId = geolocation.RegionValueId;
                    info.CountryCode = geolocation.CountryCode;
                    info.CountryValueId = geolocation.CountryValueId;
                    info.PostalCode = geolocation.PostalCode;
                    info.Latitude = geolocation.Latitude;
                    info.Longitude = geolocation.Longitude;
                }
            }

            // If we have geolocation data, lookup or add a session location instance.
            var interactionSessionLocationId = GetInteractionSessionLocationId( info );

            // Get device info from user agent.
            ParseUserAgentString( info.UserAgent ?? string.Empty, out string deviceOs, out string deviceApplication, out string deviceClientType );

            // If all device values were returned (they should have been, since values of "Other" will be
            // returned if parsing is unsuccessful), lookup or add a device type instance.
            int? deviceTypeId = null;
            if ( deviceOs.IsNotNullOrWhiteSpace() && deviceApplication.IsNotNullOrWhiteSpace() && deviceClientType.IsNotNullOrWhiteSpace() )
            {
                deviceTypeId = GetInteractionDeviceTypeId( deviceApplication, deviceOs, deviceClientType, info.UserAgent );
            }

            // If we have at least one useful piece of info about the session, lookup or add a session instance.
            if ( info.BrowserSessionId.HasValue
                || info.IpAddress.IsNotNullOrWhiteSpace()
                || interactionSessionLocationId.HasValue
                || deviceTypeId.HasValue )
            {
                var interactionSessionId = GetInteractionSessionId(
                    info.BrowserSessionId ?? Guid.NewGuid(),
                    info.IpAddress,
                    deviceTypeId,
                    interaction.InteractionDateKey,
                    interactionSessionLocationId );

                interaction.InteractionSessionId = interactionSessionId;
            }

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
            /*
                5/22/2024 - JPH

                We should prefer the `CreateInteraction( InteractionInfo info )` overload of this method for future use,
                as this method signature is difficult to call and extend. If changes are needed to this method, you
                should instead take the opportunity to migrate to the simpler method signature and add properties to
                the `InteractionInfo` POCO as needed.

                #techdebt: We should consider deprecating this method in the future.

                Reason: Extend InteractionService to add geolocation support
             */

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
            /*
                5/22/2024 - JPH

                We should prefer the `CreateInteraction( InteractionInfo info )` overload of this method for future use,
                as this method signature is difficult to call and extend. If changes are needed to this method, you
                should instead take the opportunity to migrate to the simpler method signature and add properties to
                the `InteractionInfo` POCO as needed.

                #techdebt: We should consider deprecating this method in the future.

                Reason: Extend InteractionService to add geolocation support
             */

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
            /*
                5/22/2024 - JPH

                We should prefer the `AddInteraction( InteractionInfo info )` overload of this method for future use,
                as this method signature is difficult to call and extend. If changes are needed to this method, you
                should instead take the opportunity to migrate to the simpler method signature and add properties to
                the `InteractionInfo` POCO as needed.

                #techdebt: We should consider deprecating this method in the future.

                Reason: Extend InteractionService to add geolocation support
             */

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
            /*
                5/22/2024 - JPH

                We should prefer the `AddInteraction( InteractionInfo info )` overload of this method for future use,
                as this method signature is difficult to call and extend. If changes are needed to this method, you
                should instead take the opportunity to migrate to the simpler method signature and add properties to
                the `InteractionInfo` POCO as needed.

                #techdebt: We should consider deprecating this method in the future.

                Reason: Extend InteractionService to add geolocation support
             */

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
        /// <param name="interactionSessionLocationId">The interaction session location identifier.</param>
        /// <returns></returns>
        private int GetInteractionSessionId( Guid browserSessionId, string ipAddress, int? interactionDeviceTypeId, int? interactionDateKey = null, int? interactionSessionLocationId = null )
        {
            object deviceTypeId = DBNull.Value;
            if ( interactionDeviceTypeId != null )
            {
                deviceTypeId = interactionDeviceTypeId;
            }

            var currentDateTime = RockDateTime.Now;
            interactionDateKey = interactionDateKey ?? currentDateTime.ToString( "yyyyMMdd" ).AsInteger();

            object sessionLocationId = DBNull.Value;
            if ( interactionSessionLocationId != null )
            {
                sessionLocationId = interactionSessionLocationId;
            }

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
                            ,[InteractionSessionLocationId]
                            )
                        OUTPUT inserted.Id
                        VALUES (
                            @interactionDeviceTypeId
                            ,@ipAddress
                            ,@browserSessionId
                            ,@currentDateTime
                            ,@currentDateTime
                            ,@sessionStartDateKey
                            ,@interactionSessionLocationId
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
                new SqlParameter( "@sessionStartDateKey", interactionDateKey ),
                new SqlParameter( "@interactionSessionLocationId", sessionLocationId ) )
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
        internal static void RegisterPageInteraction( RegisterPageInteractionActionInfo interactionInfo, bool immediate = false )
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
                InteractionGuid = interactionInfo.InteractionGuid,
                GetValuesFromHttpRequest = false,
                PersonAliasId = personAliasId,
                InteractionData = interactionInfo.PageRequestUrl,
                InteractionTimeToServe = interactionInfo.PageRequestTimeToServe,
                InteractionChannelCustomIndexed1 = interactionInfo.UrlReferrerHostAddress,
                InteractionChannelCustom1 = interactionInfo.TraceId,
                InteractionChannelCustom2 = interactionInfo.UrlReferrerSearchTerms,
                InteractionSummary = title,
                UserAgent = interactionInfo.UserAgent,
                IPAddress = interactionInfo.UserHostAddress,
                BrowserSessionId = interactionInfo.BrowserSessionGuid,
                GeolocationIpAddress = interactionInfo.GeolocationIpAddress,
                GeolocationLookupDateTime = interactionInfo.GeolocationLookupDateTime,
                City = interactionInfo.City,
                RegionName = interactionInfo.RegionName,
                RegionCode = interactionInfo.RegionCode,
                RegionValueId = interactionInfo.RegionValueId,
                CountryCode = interactionInfo.CountryCode,
                CountryValueId = interactionInfo.CountryValueId,
                PostalCode = interactionInfo.PostalCode,
                Latitude = interactionInfo.Latitude,
                Longitude = interactionInfo.Longitude,
                InteractionSource = interactionInfo.InteractionSource,
                InteractionMedium = interactionInfo.InteractionMedium,
                InteractionCampaign = interactionInfo.InteractionCampaign,
                InteractionContent = interactionInfo.InteractionContent,
                InteractionTerm = interactionInfo.InteractionTerm
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

            RegisterIntentInteractions( page.InteractionIntentValueIds, immediate: immediate );
        }

        /// <summary>
        /// Creates interactions for the provided interaction intent defined values.
        /// </summary>
        /// <param name="interactionIntentValueIds">The interaction intent defined value identifiers.</param>
        /// <param name="interactionOperation">The optional interaction operation.</param>
        /// <param name="immediate">Whether the transaction should be written to the database immediately. If false, it will be added to the transaction queue.</param>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "1.16.4" )]
        public static void RegisterIntentInteractions( IEnumerable<int> interactionIntentValueIds, string interactionOperation = "View", bool immediate = false )
        {
            if ( interactionIntentValueIds?.Any() != true )
            {
                return;
            }

            var channelTypeMediumValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_INTERACTION_INTENTS )?.Id;
            var definedTypeEntityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.DEFINED_TYPE )?.Id;
            var interactionIntentDefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.INTERACTION_INTENT.AsGuid() )?.Id;

            if ( !channelTypeMediumValueId.HasValue
                || !definedTypeEntityTypeId.HasValue
                || !interactionIntentDefinedTypeId.HasValue )
            {
                // Missing required system values.
                return;
            }

            foreach ( var intentValueId in interactionIntentValueIds )
            {
                var intentValue = DefinedValueCache.Get( intentValueId );
                if ( intentValue == null
                    || intentValue.DefinedTypeId != interactionIntentDefinedTypeId.Value
                    || intentValue.Value.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                var info = new InteractionTransactionInfo
                {
                    ChannelTypeMediumValueId = channelTypeMediumValueId,
                    ChannelName = "Interaction Intents",
                    ComponentEntityTypeId = definedTypeEntityTypeId,
                    ChannelEntityId = interactionIntentDefinedTypeId,
                    ComponentEntityId = intentValue.Id,
                    ComponentName = intentValue.Value,
                    InteractionOperation = interactionOperation
                };

                var intentTransaction = new InteractionTransaction( info );

                if ( immediate )
                {
                    intentTransaction.Execute();
                }
                else
                {
                    intentTransaction.Enqueue();
                }
            }
        }

        /// <summary>
        /// Ensures there is a matching/updated interaction session location instance
        /// in the database and returns its ID.
        /// </summary>
        /// <param name="info">The information that will be used to lookup/add/update
        /// the interaction session location instance.</param>
        /// <returns>The ID of the interaction session location instance.</returns>
        private int? GetInteractionSessionLocationId( InteractionInfo info )
        {
            if ( info == null
                || info.GeolocationIpAddress.IsNullOrWhiteSpace()
                || !info.GeolocationLookupDateTime.HasValue )
            {
                return null;
            }

            object postalCode = DBNull.Value;
            if ( info.PostalCode.IsNotNullOrWhiteSpace() )
            {
                postalCode = info.PostalCode.Truncate( 50 );
            }

            object location = DBNull.Value;
            if ( info.City.IsNotNullOrWhiteSpace() || info.RegionName.IsNotNullOrWhiteSpace() )
            {
                var locationSb = new StringBuilder();
                if ( info.City.IsNullOrWhiteSpace() )
                {
                    locationSb.Append( info.RegionName );
                }
                else if ( info.RegionName.IsNullOrWhiteSpace() )
                {
                    locationSb.Append( info.City );
                }
                else
                {
                    locationSb.Append( $"{info.City}, {info.RegionName}" );
                }

                location = locationSb.ToString().Truncate( 250 );
            }

            object countryCode = DBNull.Value;
            if ( info.CountryCode.IsNotNullOrWhiteSpace() )
            {
                countryCode = info.CountryCode.Truncate( 2 );
            }

            object countryValueId = DBNull.Value;
            if ( info.CountryValueId.HasValue )
            {
                countryValueId = info.CountryValueId.Value;
            }

            object regionCode = DBNull.Value;
            if ( info.RegionCode.IsNotNullOrWhiteSpace() )
            {
                // TODO: Non-US region codes may be up to 3 characters in length.
                // We need to increase the db column's max length to allow more than 2.
                regionCode = info.RegionCode.Truncate( 2 );
            }

            object regionValueId = DBNull.Value;
            if ( info.RegionValueId.HasValue )
            {
                regionValueId = info.RegionValueId.Value;
            }

            var geographySqlParameter = new SqlParameter( "@GeoPoint", DBNull.Value );
            if ( info.Latitude.HasValue && info.Longitude.HasValue )
            {
                // We need to manually convert DbGeography to SqlGeography since we're sidestepping EF here.
                geographySqlParameter = Location.GetGeographySqlParameter( "@GeoPoint", info.Latitude.Value, info.Longitude.Value );
            }

            // To make this more thread safe and to avoid overhead of an extra database call, Etc.,
            // run a SQL block to get/create/update in one quick SQL round trip.
            int interactionSessionLocationId = this.Context.Database.SqlQuery<int>(
                @"BEGIN
                    DECLARE @InteractionSessionLocationId [int] =
                    (
                        SELECT TOP 1 [Id]
                        FROM [InteractionSessionLocation]
                        WHERE [IpAddress] = @IpAddress
                            AND ((@PostalCode IS NULL AND [PostalCode] IS NULL) OR [PostalCode] = @PostalCode)
                            AND ((@CountryCode IS NULL AND [CountryCode] IS NULL) OR [CountryCode] = @CountryCode)
                            AND ((@RegionCode IS NULL AND [RegionCode] IS NULL) OR [RegionCode] = @RegionCode)
                    );

                    IF @InteractionSessionLocationId IS NULL
                    BEGIN
                        INSERT [InteractionSessionLocation]
                        (
                            [IpAddress]
                            , [LookupDateTime]
                            , [PostalCode]
                            , [Location]
                            , [CountryCode]
                            , [CountryValueId]
                            , [RegionCode]
                            , [RegionValueId]
                            , [GeoPoint]
                            , [CreatedDateTime]
                            , [ModifiedDateTime]
                            , [Guid]
                        )
                        OUTPUT INSERTED.[Id]
                        VALUES
                        (
                            @IpAddress
                            , @LookupDateTime
                            , @PostalCode
                            , @Location
                            , @CountryCode
                            , @CountryValueId
                            , @RegionCode
                            , @RegionValueId
                            , @GeoPoint
                            , @Now
                            , @Now
                            , NEWID()
                        );
                    END
                    ELSE
                    BEGIN
                        IF @IpAddress NOT IN ('InvalidAddress', 'ReservedAddress')
                        BEGIN
                            UPDATE [InteractionSessionLocation]
                            SET [LookupDateTime] = @LookupDateTime
                                , [Location] = @Location
                                , [CountryValueId] = @CountryValueId
                                , [RegionValueId] = @RegionValueId
                                , [GeoPoint] = @GeoPoint
                                , [ModifiedDateTime] = @Now
                            WHERE [Id] = @InteractionSessionLocationId;
                        END

                        SELECT @InteractionSessionLocationId;
                    END
                END",
                new SqlParameter( "@IpAddress", info.GeolocationIpAddress.Truncate( 45 ) ),
                new SqlParameter( "@LookupDateTime", info.GeolocationLookupDateTime.Value ),
                new SqlParameter( "@PostalCode", postalCode ),
                new SqlParameter( "@Location", location ),
                new SqlParameter( "@CountryCode", countryCode ),
                new SqlParameter( "@CountryValueId", countryValueId ),
                new SqlParameter( "@RegionCode", regionCode ),
                new SqlParameter( "@RegionValueId", regionValueId ),
                new SqlParameter( "@Now", RockDateTime.Now ),
                geographySqlParameter )
                .FirstOrDefault();

            return interactionSessionLocationId;
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

                foreach ( var interactionId in interactionIds )
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
        /// <inheritdoc cref="IEntity.Guid"/>
        /// <remarks>
        /// If this is not specified then a new Guid will be created.
        /// </remarks>
        public Guid? Guid { get; set; }

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

        #region InteractionSessionLocation Properties

        /// <inheritdoc cref="IpGeolocation.IpAddress"/>
        public string GeolocationIpAddress { get; set; }

        /// <inheritdoc cref="InteractionSessionLocation.LookupDateTime"/>
        public DateTime? GeolocationLookupDateTime { get; set; }

        /// <inheritdoc cref="IpGeolocation.City"/>
        public string City { get; set; }

        /// <inheritdoc cref="IpGeolocation.RegionName"/>
        public string RegionName { get; set; }

        /// <inheritdoc cref="InteractionSessionLocation.RegionCode"/>
        public string RegionCode { get; set; }

        /// <inheritdoc cref="InteractionSessionLocation.RegionValueId"/>
        public int? RegionValueId { get; set; }

        /// <inheritdoc cref="InteractionSessionLocation.CountryCode"/>
        public string CountryCode { get; set; }

        /// <inheritdoc cref="InteractionSessionLocation.CountryValueId"/>
        public int? CountryValueId { get; set; }

        /// <inheritdoc cref="InteractionSessionLocation.PostalCode"/>
        public string PostalCode { get; set; }

        /// <inheritdoc cref="IpGeolocation.Latitude"/>
        public double? Latitude { get; set; }

        /// <inheritdoc cref="IpGeolocation.Longitude"/>
        public double? Longitude { get; set; }

        #endregion InteractionSessionLocation Properties
    }

    /// <summary>
    /// Describes a processing request to register a page interaction.
    /// </summary>
    internal class RegisterPageInteractionActionInfo
    {
        /// <summary>
        /// Create a new instance from a PageInteractionInfo object.
        /// </summary>
        /// <param name="interactionInfo"></param>
        /// <returns></returns>
        public static RegisterPageInteractionActionInfo FromPageInteraction( PageInteractionInfo interactionInfo )
        {
            var actionInfo = new RegisterPageInteractionActionInfo()
            {
                InteractionGuid = interactionInfo.Guid,
                PageId = interactionInfo.PageId,
                UserIdKey = interactionInfo.UserIdKey,
                PageRequestUrl = interactionInfo.PageRequestUrl,
                PageRequestTimeToServe = interactionInfo.PageRequestTimeToServe,
                UrlReferrerHostAddress = interactionInfo.UrlReferrerHostAddress,
                UrlReferrerSearchTerms = interactionInfo.UrlReferrerSearchTerms,
                UserAgent = interactionInfo.UserAgent,
                UserHostAddress = interactionInfo.UserHostAddress,
                BrowserSessionGuid = interactionInfo.BrowserSessionGuid,
                GeolocationIpAddress = interactionInfo.GeolocationIpAddress,
                GeolocationLookupDateTime = interactionInfo.GeolocationLookupDateTime,
                City = interactionInfo.City,
                RegionName = interactionInfo.RegionName,
                RegionCode = interactionInfo.RegionCode,
                RegionValueId = interactionInfo.RegionValueId,
                CountryCode = interactionInfo.CountryCode,
                CountryValueId = interactionInfo.CountryValueId,
                PostalCode = interactionInfo.PostalCode,
                Latitude = interactionInfo.Latitude,
                Longitude = interactionInfo.Longitude,
                TraceId = Activity.Current?.TraceId.ToString()
            };

            return actionInfo;
        }

        /// <inheritdoc cref="IEntity.Guid"/>
        /// <remarks>
        /// If this is not specified then a new Guid will be created.
        /// </remarks>
        public Guid? InteractionGuid { get; set; }

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
        /// The trace identifier from Observability. This allows correlation
        /// between page interactions and observability trace logs.
        /// </summary>
        public string TraceId { get; set; }

        /// <summary>
        /// The unique identifier of the user initiating this interaction.
        /// </summary>
        public string UserIdKey { get; set; }

        /// <inheritdoc cref="Interaction.Source"/>
        public string InteractionSource { get; set; }

        /// <inheritdoc cref="Interaction.Medium"/>
        public string InteractionMedium { get; set; }

        /// <inheritdoc cref="Interaction.Campaign"/>
        public string InteractionCampaign { get; set; }

        /// <inheritdoc cref="Interaction.Content"/>
        public string InteractionContent { get; set; }

        /// <inheritdoc cref="Interaction.Term"/>
        public string InteractionTerm { get; set; }

        #region InteractionSessionLocation Properties

        /// <inheritdoc cref="IpGeolocation.IpAddress"/>
        public string GeolocationIpAddress { get; set; }

        /// <inheritdoc cref="InteractionSessionLocation.LookupDateTime"/>
        public DateTime? GeolocationLookupDateTime { get; set; }

        /// <inheritdoc cref="IpGeolocation.City"/>
        public string City { get; set; }

        /// <inheritdoc cref="IpGeolocation.RegionName"/>
        public string RegionName { get; set; }

        /// <inheritdoc cref="InteractionSessionLocation.RegionCode"/>
        public string RegionCode { get; set; }

        /// <inheritdoc cref="InteractionSessionLocation.RegionValueId"/>
        public int? RegionValueId { get; set; }

        /// <inheritdoc cref="InteractionSessionLocation.CountryCode"/>
        public string CountryCode { get; set; }

        /// <inheritdoc cref="InteractionSessionLocation.CountryValueId"/>
        public int? CountryValueId { get; set; }

        /// <inheritdoc cref="InteractionSessionLocation.PostalCode"/>
        public string PostalCode { get; set; }

        /// <inheritdoc cref="IpGeolocation.Latitude"/>
        public double? Latitude { get; set; }

        /// <inheritdoc cref="IpGeolocation.Longitude"/>
        public double? Longitude { get; set; }

        #endregion InteractionSessionLocation Properties
    }

    #endregion
}