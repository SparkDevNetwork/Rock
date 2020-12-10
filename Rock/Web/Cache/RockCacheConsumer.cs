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
using System.Reflection;
using Rock.Bus;
using Rock.Bus.Consumer;
using Rock.Bus.Message;
using Rock.Bus.Queue;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Rock Cache Consumer
    /// </summary>
    public sealed class RockCacheConsumer : RockConsumer<CacheEventQueue, CacheWasUpdatedMessage>
    {
        /// <summary>
        /// The apply method
        /// </summary>
        private readonly MethodInfo _applyMethod;

        /// <summary>
        /// Initializes a new instance of the <see cref="RockCacheConsumer"/> class.
        /// </summary>
        public RockCacheConsumer()
        {
            _applyMethod = GetType().GetMethod( nameof( ApplyCacheMessage ), BindingFlags.NonPublic | BindingFlags.Instance );
        }

        /// <summary>
        /// Consumes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void Consume( CacheWasUpdatedMessage message )
        {
            if ( !RockMessageBus.IsRockStarted || RockMessageBus.IsFromSelf( message ) )
            {
                return;
            }

            var type = Type.GetType( message.CacheTypeName );

            if ( type == null )
            {
                return;
            }

            _applyMethod.MakeGenericMethod( type ).Invoke( this, new[] { message.Key } );
        }

        /// <summary>
        /// Applies the cache message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        private void ApplyCacheMessage<T>( string key )
        {
            if ( key.IsNullOrWhiteSpace() )
            {
                RockCacheManager<T>.Instance.Cache.Clear();
            }
            else
            {
                RockCacheManager<T>.Instance.Cache.Remove( key );
            }
        }
    }
}
