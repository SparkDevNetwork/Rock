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

using Rock.Data;

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
        public Interaction AddInteraction( int interactionComponentId, int? entityId, string operation, string interactionSummary, string interactionData, int? personAliasId, DateTime dateTime,
            string deviceApplication, string deviceOs, string deviceClientType, string deviceTypeData, string ipAddress, Guid? browserSessionId )
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
            interaction.InteractionData = interactionData.IsNotNullOrWhiteSpace() ? PersonToken.ObfuscateRockMagicToken( interactionData ) : string.Empty;
            interaction.InteractionDateTime = dateTime;
            interaction.PersonAliasId = personAliasId;
            interaction.InteractionSummary = interactionSummary;

            int? deviceTypeId = null;
            if ( deviceApplication.IsNotNullOrWhiteSpace() && deviceOs.IsNotNullOrWhiteSpace() && deviceClientType.IsNotNullOrWhiteSpace() )
            {
                var deviceType = this.GetInteractionDeviceType( deviceApplication, deviceOs, deviceClientType, deviceTypeData );
                deviceTypeId = deviceType != null ? deviceType.Id : ( int? ) null;
            }

            // If we don't have an BrowserSessionId, IPAddress or a devicetype, there is nothing useful about the session
            // but at least one of these has a value, then we should lookup or create a session
            if ( browserSessionId.HasValue || ipAddress.IsNotNullOrWhiteSpace() || deviceTypeId.HasValue )
            {
                var session = this.GetInteractionSession( browserSessionId, ipAddress, deviceTypeId );
                interaction.InteractionSessionId = session.Id;
            }

            return interaction;
        }

        /// <summary>
        /// The ua parser
        /// </summary>
        private static UAParser.Parser uaParser = UAParser.Parser.GetDefault();

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
            var deviceOs = uaParser.ParseOS( userAgent ).ToString();
            var deviceApplication = uaParser.ParseUserAgent( userAgent ).ToString();
            var deviceClientType = InteractionDeviceType.GetClientType( userAgent );

            var interaction = CreateInteraction( interactionComponentId, null, null, string.Empty, null, null, RockDateTime.Now, 
                deviceApplication, deviceOs, deviceClientType, userAgent, ipAddress, browserSessionId );

            if ( url.IsNotNullOrWhiteSpace() && url.IndexOf( "utm_", StringComparison.OrdinalIgnoreCase ) >= 0 )
            {
                var urlParams = System.Web.HttpUtility.ParseQueryString( url );
                interaction.Source = urlParams.Get( "utm_source" ).Truncate( 25 );
                interaction.Medium = urlParams.Get( "utm_medium" ).Truncate( 25 );
                interaction.Campaign = urlParams.Get( "utm_campaign" ).Truncate( 50 );
                interaction.Content = urlParams.Get( "utm_content" ).Truncate( 50 );
                interaction.Term = urlParams.Get( "utm_term" ).Truncate( 50 );
            }

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
        public Interaction AddInteraction( int interactionComponentId, int? entityId, string operation, string interactionData, int? personAliasId, DateTime dateTime,
            string deviceApplication, string deviceOs, string deviceClientType, string deviceTypeData, string ipAddress, Guid? browserSessionId )
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
        public Interaction AddInteraction( int interactionComponentId, int? entityId, string operation, string interactionData, int? personAliasId, DateTime dateTime,
            string deviceApplication, string deviceOs, string deviceClientType, string deviceTypeData, string ipAddress )
        {
            return AddInteraction( interactionComponentId, entityId, operation, string.Empty, interactionData, personAliasId, dateTime, deviceApplication, deviceOs, deviceClientType, deviceTypeData, ipAddress, null );
        }

        /// <summary>
        /// Gets the interaction device type. If it can't be found, a new InteractionDeviceType record will be created and returned.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="operatingSystem">The operating system.</param>
        /// <param name="clientType">Type of the client.</param>
        /// <param name="deviceTypeData">The device type data (either a plain DeviceType name or the whole useragent string).</param>
        /// <returns></returns>
        public InteractionDeviceType GetInteractionDeviceType( string application, string operatingSystem, string clientType, string deviceTypeData )
        {
            using ( var rockContext = new RockContext() )
            {
                InteractionDeviceTypeService interactionDeviceTypeService = new InteractionDeviceTypeService( rockContext );
                InteractionDeviceType interactionDeviceType = interactionDeviceTypeService.Queryable().Where( a => a.Application == application
                                                    && a.OperatingSystem == operatingSystem && a.ClientType == clientType ).FirstOrDefault();

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
            using ( var rockContext = new RockContext() )
            {
                InteractionSessionService interactionSessionService = new InteractionSessionService( rockContext );
                InteractionSession interactionSession = null;

                // if we have a browser session id, see if a session record was already created
                if ( browserSessionId.HasValue )
                {
                    interactionSession = interactionSessionService.Queryable().Where( a => a.Guid == browserSessionId.Value ).FirstOrDefault();
                }

                if ( interactionSession == null )
                {
                    interactionSession = new InteractionSession();
                    interactionSession.DeviceTypeId = interactionDeviceTypeId;
                    interactionSession.IpAddress = ipAddress;
                    interactionSession.Guid = browserSessionId ?? Guid.NewGuid();
                    interactionSessionService.Add( interactionSession );
                    rockContext.SaveChanges();
                }

                return interactionSession;
            }
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
    }
}
