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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Rock.Data;
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
            Interaction interaction = new Interaction();
            interaction.InteractionComponentId = interactionComponentId;
            interaction.EntityId = entityId;
            interaction.Operation = operation;
            interaction.InteractionData = PersonToken.ObfuscateRockMagicToken( interactionData );
            interaction.InteractionDateTime = dateTime;
            interaction.PersonAliasId = personAliasId;

            var deviceType = this.GetInteractionDeviceType( deviceApplication, deviceOs, deviceClientType, deviceTypeData );
            var session = this.GetInteractionSession( browserSessionId, ipAddress, deviceType.Id );

            interaction.InteractionSessionId = session.Id;
            this.Add( interaction );

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
        /// <returns></returns>
        public Interaction AddInteraction( int interactionComponentId, int? entityId, string operation, string interactionData, int? personAliasId, DateTime dateTime,
            string deviceApplication, string deviceOs, string deviceClientType, string deviceTypeData, string ipAddress )
        {
            return AddInteraction( interactionComponentId, entityId, operation, interactionData, personAliasId, dateTime, deviceApplication, deviceOs, deviceClientType, deviceTypeData, ipAddress, null );
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
        /// Gets the interaction session. If it can't be found, a new InteractionSession record will be created and returned.
        /// </summary>
        /// <param name="browserSessionId">The browser session identifier (RockSessionId).</param>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="interactionDeviceTypeId">The interaction device type identifier.</param>
        /// <returns></returns>
        public InteractionSession GetInteractionSession( Guid? browserSessionId, string ipAddress, int interactionDeviceTypeId )
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
    }
}
