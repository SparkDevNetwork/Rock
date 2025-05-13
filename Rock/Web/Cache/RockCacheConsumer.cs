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
using System;
using System.Collections.Concurrent;
using System.Reflection;

using Microsoft.Extensions.Logging;

using Rock.Bus;
using Rock.Bus.Consumer;
using Rock.Bus.Message;
using Rock.Bus.Queue;
using Rock.Logging;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Rock Cache Consumer
    /// </summary>
    [RockLoggingCategory]
    public sealed class RockCacheConsumer : RockConsumer<CacheEventQueue, CacheWasUpdatedMessage>
    {
        /// <summary>
        /// The cache types
        /// </summary>
        private static readonly ConcurrentDictionary<string, MethodInfo> _cacheTypeMethodInfoLookup = new ConcurrentDictionary<string, MethodInfo>();

        /// <summary>
        /// Initializes a new instance of the <see cref="RockCacheConsumer"/> class.
        /// </summary>
        public RockCacheConsumer()
        {
        }

        /// <summary>
        /// Consumes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Consume( CacheWasUpdatedMessage message )
        {
            /* 06-07-2022 MP

            In the case of consuming a CacheWasUpdatedMessage, we don't need to check RockMessageBus.IsRockStarted. The Cache Update
            logic doesn't have a dependency on having Rock fully started.

            Also, we really need to consume these messages regardless of IsRockStarted to prevent the cache from getting stale.

            If we later discover that this isn't OK, we'll revisit this decision and make any updates to make it OK again.
            */

            Logger.LogDebug( $"Consumed Cache Update message from {message.SenderNodeName} node. {message.ToDebugString()}." );
            var applyCacheMessageMethodInfo = FindApplyCacheMessageMethodInfo( message.CacheTypeName );

            if ( applyCacheMessageMethodInfo == null )
            {
                var logMessage = $"Unable to resolve cache type when consuming cache update message. {message.ToDebugString()}.";
                Logger.LogDebug( logMessage );
                ExceptionLogService.LogException( new BusException( logMessage ) );

                return;
            }

            applyCacheMessageMethodInfo.Invoke( this, new[] { message } );
        }

        /// <summary>
        /// Finds the type of the cache.
        /// </summary>
        /// <param name="cacheTypeName">Name of the cache type.</param>
        /// <returns></returns>
        private MethodInfo FindApplyCacheMessageMethodInfo( string cacheTypeName )
        {
            MethodInfo applyCacheMessageMethodInfo;
            if ( _cacheTypeMethodInfoLookup.ContainsKey( cacheTypeName ) )
            {
                if ( _cacheTypeMethodInfoLookup.TryGetValue( cacheTypeName, out applyCacheMessageMethodInfo ) )
                {
                    if ( applyCacheMessageMethodInfo != null )
                    {
                        return applyCacheMessageMethodInfo;
                    }
                }
            }

            var cacheType = Type.GetType( cacheTypeName );
            applyCacheMessageMethodInfo = GetType().GetMethod( nameof( ApplyCacheMessage ), BindingFlags.NonPublic | BindingFlags.Instance ).MakeGenericMethod( cacheType );

            if ( applyCacheMessageMethodInfo != null )
            {
                _cacheTypeMethodInfoLookup.TryAdd( cacheTypeName, applyCacheMessageMethodInfo );
            }

            return applyCacheMessageMethodInfo;
        }

        /// <summary>
        /// Applies the cache message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message.</param>
        private void ApplyCacheMessage<T>( CacheWasUpdatedMessage message )
        {
            if ( message?.Key != null )
            {
                RockCacheManager<T>.Instance.ReceiveRemoveMessage( message );
            }
            else
            {
                RockCacheManager<T>.Instance.ReceiveClearMessage( message );
            }
        }
    }
}
