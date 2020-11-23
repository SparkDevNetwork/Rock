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
using System.Linq;

namespace Rock.Bus.Statistics
{
    /// <summary>
    /// Statistics Log
    /// </summary>
    public sealed class StatLog
    {
        /// <summary>
        /// The maximum log length
        /// </summary>
        private static readonly int _maxLogLength = 10000;

        /// <summary>
        /// When the log was started
        /// </summary>
        private readonly DateTime _started = RockDateTime.Now;

        /// <summary>
        /// The consume log.
        /// Specific to one Rock instance.
        /// </summary>
        private ConcurrentQueue<DateTime?> _consumeLog = new ConcurrentQueue<DateTime?>();

        /// <summary>
        /// Logs the consume.
        /// </summary>
        public void LogConsume()
        {
            _consumeLog.Enqueue( RockDateTime.Now );

            while ( _consumeLog.Count > _maxLogLength && _consumeLog.TryDequeue( out var _ ) )
            {
                // Remove elements until at the correct length
            }
        }

        /// <summary>
        /// Gets the messages consumed last minute.
        /// Specific to one Rock instance.
        /// </summary>
        /// <value>
        /// The messages consumed last hour.
        /// </value>
        public int? MessagesConsumedLastMinute
        {
            get
            {
                var minDateTime = RockDateTime.Now.AddMinutes( -1 );
                return GetMessagesConsumed( minDateTime );
            }
        }

        /// <summary>
        /// Gets the messages consumed last hour.
        /// Specific to one Rock instance.
        /// </summary>
        /// <value>
        /// The messages consumed last hour.
        /// </value>
        public int? MessagesConsumedLastHour
        {
            get
            {
                var minDateTime = RockDateTime.Now.AddHours( -1 );
                return GetMessagesConsumed( minDateTime );
            }
        }

        /// <summary>
        /// Gets the messages consumed last day.
        /// Specific to one Rock instance.
        /// </summary>
        /// <value>
        /// The messages consumed last hour.
        /// </value>
        public int? MessagesConsumedLastDay
        {
            get
            {
                var minDateTime = RockDateTime.Now.AddDays( -1 );
                return GetMessagesConsumed( minDateTime );
            }
        }

        /// <summary>
        /// Is there enough data to report a rate based on the given min.
        /// Specific to one Rock instance.
        /// </summary>
        /// <returns></returns>
        private bool HasCompleteData( DateTime minDateTime )
        {
            return _started <= minDateTime;
        }

        /// <summary>
        /// Gets the count of messages consumed since the minDateTime.
        /// </summary>
        /// <param name="minDateTime">The minimum date time.</param>
        /// <returns></returns>
        private int? GetMessagesConsumed( DateTime minDateTime )
        {
            try
            {
                if ( !HasCompleteData( minDateTime ) )
                {
                    return null;
                }

                var count = 0;
                var i = _consumeLog.Count - 1;

                while ( i >= 0 )
                {
                    var date = _consumeLog.ElementAtOrDefault( i );

                    if ( !date.HasValue )
                    {
                        continue;
                    }

                    if ( date.Value < minDateTime )
                    {
                        break;
                    }

                    count++;
                    i--;
                }

                return count;
            }
            catch
            {
                // Concurrency can cause the size of the queue to change while executing
                // and out of bounds exceptions will occur normally
                return null;
            }
        }
    }
}
