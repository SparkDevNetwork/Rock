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
