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
using System.Linq;

using Rock.Bus.Statistics;
using Rock.Utility.ExtensionMethods;

namespace Rock.Bus.Queue
{
    /// <summary>
    /// Queue Interface
    /// </summary>
    public interface IRockQueue
    {
        /// <summary>
        /// Gets the name. Each instance of Rock shares this name for this queue.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets the name for configuration.
        /// </summary>
        /// <value>
        /// The name for configuration.
        /// </value>
        string NameForConfiguration { get; }

        /// <summary>
        /// Gets or sets the time to live seconds.
        /// A setting of null or less than 1 means there is no expiration.
        /// </summary>
        /// <value>
        /// The time to live seconds.
        /// </value>
        int? TimeToLiveSeconds { get; set; }

        /// <summary>
        /// Gets the log.
        /// </summary>
        /// <value>
        /// The log.
        /// </value>
        StatLog StatLog { get; }
    }

    /// <summary>
    /// Queue Abstract Class
    /// </summary>
    public abstract class RockQueue : IRockQueue
    {
        private static readonly Type _interfaceType = typeof( IRockQueue );

        /// <summary>
        /// Gets the queue name. Each instance of Rock shares this name for this queue.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public virtual string Name => GetType().Name;

        /// <summary>
        /// Gets the name for configuration.
        /// </summary>
        /// <value>
        /// The name for configuration.
        /// </value>
        public virtual string NameForConfiguration => Name;

        /// <summary>
        /// Gets the time to live seconds.
        /// </summary>
        /// <value>
        /// The time to live seconds.
        /// </value>
        public virtual int? TimeToLiveSeconds { get; set; } = 300; // 5 minutes * 60 seconds

        /// <summary>
        /// Gets the log.
        /// </summary>
        /// <value>
        /// The log.
        /// </value>
        public StatLog StatLog { get; } = new StatLog();

        /// <summary>
        /// Gets the time to live header value for a new message.
        /// </summary>
        /// <returns></returns>
        public static TimeSpan? GetTimeToLive<TQueue>()
            where TQueue : IRockQueue, new()
        {
            var queue = Get<TQueue>();
            return GetTimeToLive( queue );
        }

        /// <summary>
        /// Gets the time to live header value for a new message.
        /// </summary>
        /// <param name="queue">The queue.</param>
        /// <returns></returns>
        public static TimeSpan? GetTimeToLive( IRockQueue queue )
        {
            var canHaveValue = queue.TimeToLiveSeconds.HasValue && queue.TimeToLiveSeconds > 0;
            return canHaveValue ? ( TimeSpan? ) TimeSpan.FromSeconds( queue.TimeToLiveSeconds.Value ) : null;
        }

        /// <summary>
        /// Gets this instance.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns></returns>
        public static IRockQueue Get( string typeName )
        {
            var queueType = Type.GetType( typeName );
            return Get( queueType );
        }

        /// <summary>
        /// Gets this instance.
        /// </summary>
        /// <typeparam name="TQueue">The type of the queue.</typeparam>
        /// <returns></returns>
        public static IRockQueue Get<TQueue>()
            where TQueue : IRockQueue, new()
        {
            return Get( typeof( TQueue ) );
        }

        /// <summary>
        /// Gets this instance.
        /// </summary>
        /// <param name="queueType">Type of the queue.</param>
        /// <returns></returns>
        public static IRockQueue Get( Type queueType )
        {
            if ( queueType == null )
            {
                return null;
            }

            var key = queueType.FullName;

            return _queues.GetOrAdd( key, _key => Activator.CreateInstance( queueType ) as IRockQueue );
        }
        private static ConcurrentDictionary<string, IRockQueue> _queues = new ConcurrentDictionary<string, IRockQueue>();

        /// <summary>
        /// Gets the queue types.
        /// </summary>
        /// <returns></returns>
        public static List<Type> GetQueueTypes()
        {
            var queueTypes = new Dictionary<string, Type>();
            var assemblies = Reflection.GetRockAndPluginAssemblies();
            var types = assemblies
                .SelectMany( a => a.GetTypesSafe()
                .Where( t => t.IsClass && !t.IsNestedPrivate && !t.IsAbstract ) );

            foreach ( var type in types )
            {
                if ( IsRockQueue( type ) )
                {
                    queueTypes.TryAdd( type.FullName, type );
                }
            }

            var queueTypeList = queueTypes.Select( kvp => kvp.Value ).ToList();
            return queueTypeList;
        }

        /// <summary>
        /// Determines if the type is a Rock queue type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if [is rock queue] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsRockQueue( Type type )
        {
            if ( type.IsAbstract || type.ContainsGenericParameters )
            {
                return false;
            }

            return _interfaceType.IsAssignableFrom( type );
        }
    }
}
