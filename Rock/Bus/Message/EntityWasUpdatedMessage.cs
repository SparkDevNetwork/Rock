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
#if WEBFORMS
using System.Data.Entity;
using EFEntityState = System.Data.Entity.EntityState;
#else
using EFEntityState = Microsoft.EntityFrameworkCore.EntityState;
#endif
using Rock.Bus.Queue;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Bus.Message
{
    /// <summary>
    /// Entity Update Message Interface
    /// </summary>
    public interface IEntityWasUpdatedMessage : IEventMessage<EntityUpdateQueue>
    {
        /// <summary>
        /// Gets the entity type identifier.
        /// </summary>
        int EntityTypeId { get; set; }

        /// <summary>
        /// Gets the entity identifier.
        /// </summary>
        int EntityId { get; set; }

        /// <summary>
        /// Gets the state of the entity.
        /// </summary>
        string EntityState { get; set; }
    }

    /// <summary>
    /// Typed Entity Update Message Interface
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IEntityWasUpdatedMessage<TEntity> : IEntityWasUpdatedMessage
        where TEntity : IEntity
    { }

    /// <summary>
    /// Entity Update Message Class
    /// </summary>
    public class EntityWasUpdatedMessage : IEntityWasUpdatedMessage
    {
        /// <summary>
        /// Gets the entity type identifier.
        /// </summary>
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets the entity identifier.
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Gets the state of the entity.
        /// </summary>
        public string EntityState { get; set; }

        /// <summary>
        /// Gets or sets the sender rock instance unique identifier.
        /// </summary>
        public string SenderNodeName { get; set; }

        /// <summary>
        /// The of an entity that will cause publishing a message on the <see cref="RockMessageBus"/>
        /// </summary>
        private static readonly HashSet<EntityState> _statesToPublishOnBus = new HashSet<EntityState> {
            EFEntityState.Added,
            EFEntityState.Modified,
            EFEntityState.Deleted
        };

        /// <summary>
        /// Should entity updates be published for this entity type.
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <returns></returns>
        public static bool ShouldPublish( int entityTypeId, EntityState entityState )
        {
            return
                _statesToPublishOnBus.Contains( entityState ) &&
                ( EntityTypeCache.Get( entityTypeId )?.IsMessageBusEventPublishEnabled ?? false );
        }

        /// <summary>
        /// Publishes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="entityState">State of the entity.</param>
        public static void Publish( IEntity entity, EntityState entityState )
        {
            var entityTypeId = entity.TypeId;
            var entityType = EntityTypeCache.Get( entityTypeId );

            var messageType = typeof( EntityWasUpdatedMessage<> ).MakeGenericType( entityType.GetEntityType() );
            var message = Activator.CreateInstance( messageType ) as IEntityWasUpdatedMessage;

            message.EntityId = entity.Id;
            message.EntityTypeId = entity.TypeId;
            message.EntityState = entityState.ToString();

            _ = RockMessageBus.PublishAsync( message, messageType );
        }

        /// <summary>
        /// Publishes the entity update if the entity type is configured to do so.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="entityState">State of the entity.</param>
        /// <returns></returns>
        public static bool PublishIfShould( IEntity entity, EntityState entityState )
        {
            if ( !ShouldPublish( entity.TypeId, entityState ) )
            {
                return false;
            }

            Publish( entity, entityState );
            return true;
        }
    }

    /// <summary>
    /// Typed Entity Update Message Class
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <seealso cref="Rock.Bus.Message.IEntityWasUpdatedMessage" />
    public class EntityWasUpdatedMessage<TEntity> : EntityWasUpdatedMessage, IEntityWasUpdatedMessage<TEntity>
        where TEntity : IEntity
    { }
}
