using System;
using System.Collections.Generic;

using Rock.Bus;
using Rock.Bus.Consumer;
using Rock.Bus.Message;
using Rock.Bus.Queue;
using Rock.Logging;

namespace Rock.Web.Cache
{
    /// <summary>
    /// The RockConsumer to update the AllKeys in the Cache when a new item is added in another Rock Node.
    /// </summary>
    public sealed class RockCacheAllIdUpdater : RockConsumer<CacheEventQueue, NewItemCacheAddedMessage>
    {
        /// <summary>
        /// 
        /// </summary>
        public RockCacheAllIdUpdater()
        {
        }

        /// <summary>
        /// For every Entity, the Cache has an entry which stores all the ids for the entity.
        /// The purpose of this Consumer is add the new id to the entry every time a new entity gets created in another node.
        /// </summary>
        /// <param name="message"></param>
        public override void Consume( NewItemCacheAddedMessage message )
        {
            if ( RockMessageBus.IsFromSelf( message ) )
            {
                RockLogger.Log.Debug( RockLogDomains.Bus, $"Cache NewItemCacheAddedMessage was from ourselves( {message.SenderNodeName} ). Skipping. {message.ToDebugString()}." );
                return;
            }

            RockLogger.Log.Debug( RockLogDomains.Bus, $"Consumed NewItemCacheAddedMessage from {message.SenderNodeName} node. {message.ToDebugString()}." );

            var newEntityIdKey = message.IdKey;
            var cacheTypeName = message.CacheTypeName;

            var itemCacheType = typeof( ItemCache<> ).MakeGenericType( Type.GetType( cacheTypeName ) );
            itemCacheType?.GetMethod( "ReceiveAddIdMessage", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic )
                ?.Invoke( this, new[] { newEntityIdKey } );
        }
    }
}
