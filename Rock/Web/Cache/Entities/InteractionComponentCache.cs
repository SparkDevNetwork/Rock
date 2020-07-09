﻿// <copyright>
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
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a interactionComponent that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class InteractionComponentCache : ModelCache<InteractionComponentCache, InteractionComponent>
    {

        #region Static Fields

        private static ConcurrentDictionary<string, int> _interactionComponentIdLookupFromForeignKey = new ConcurrentDictionary<string, int>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        [DataMember]
        public int? EntityId { get; private set; }

        /// <summary>
        /// Gets or sets the channel identifier.
        /// </summary>
        /// <value>
        /// The channel identifier.
        /// </value>
        [DataMember]
        public int ChannelId { get; private set; }

        /// <summary>
        /// Gets the interaction channel.
        /// </summary>
        /// <value>
        /// The interaction channel.
        /// </value>
        public InteractionChannelCache InteractionChannel => InteractionChannelCache.Get( ChannelId );

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies the model property values to the DTO properties
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var interactionComponent = entity as InteractionComponent;
            if ( interactionComponent == null )
            {
                return;
            }

            Name = interactionComponent.Name;
            EntityId = interactionComponent.EntityId;
            ChannelId = interactionComponent.ChannelId;

            if ( interactionComponent.ForeignKey.IsNotNullOrWhiteSpace() )
            {
                var lookupKey = $"{interactionComponent.ForeignKey}|interactionChannelId:{interactionComponent.ChannelId}";
                _interactionComponentIdLookupFromForeignKey.AddOrUpdate( lookupKey, interactionComponent.Id, ( k, v ) => interactionComponent.Id );
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Reads the specified unique identifier.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns></returns>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use Get Instead" )]
        public static InteractionComponentCache Read( string guid )
        {
            Guid realGuid = guid.AsGuid();
            if ( realGuid.Equals( Guid.Empty ) )
            {
                return null;
            }

            return Get( realGuid );
        }

        /// <summary>
        /// Gets the component identifier by foreign key and ChannelId, and creates it if it doesn't exist.
        /// If foreignKey is blank, this will throw a <seealso cref="ArgumentNullException" />
        /// If creating a new InteractionComponent with this, componentName must be specified
        /// </summary>
        /// <param name="foreignKey">The foreign key.</param>
        /// <param name="interactionChannelId">The interaction channel identifier.</param>
        /// <param name="componentName">Name of the component.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">ForeignKey must be specified when using GetComponentIdByForeignKey</exception>
        public static int GetComponentIdByForeignKeyAndChannelId( string foreignKey, int interactionChannelId, string componentName )
        {
            if ( foreignKey.IsNullOrWhiteSpace() )
            {
                throw new ArgumentNullException( "ForeignKey must be specified when using GetComponentIdByForeignKey" );
            }

            var lookupKey = $"{foreignKey}|interactionChannelId:{interactionChannelId}";

            if ( _interactionComponentIdLookupFromForeignKey.TryGetValue( lookupKey, out int channelId ) )
            {
                return channelId;
            }

            using ( var rockContext = new RockContext() )
            {
                var interactionComponentService = new InteractionComponentService( rockContext );
                var interactionComponent = interactionComponentService.Queryable()
                        .Where( a => a.ForeignKey == foreignKey && a.ChannelId == interactionChannelId ).FirstOrDefault();

                if ( interactionComponent == null )
                {
                    interactionComponent = new InteractionComponent();
                    interactionComponent.Name = componentName;
                    interactionComponent.ForeignKey = foreignKey;
                    interactionComponent.ChannelId = interactionChannelId;
                    interactionComponentService.Add( interactionComponent );
                    rockContext.SaveChanges();
                }

                var interactionComponentId = Get( interactionComponent ).Id;
                _interactionComponentIdLookupFromForeignKey.AddOrUpdate( lookupKey, interactionComponentId, ( k, v ) => interactionComponentId );

                return interactionComponentId;
            }
        }

        #endregion

    }
}