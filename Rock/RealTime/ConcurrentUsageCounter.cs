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

using System.Collections.Generic;

namespace Rock.RealTime
{
    /// <summary>
    /// Thread-safe increment and decrement counter for keyed values.
    /// </summary>
    /// <typeparam name="TKey">The type to be used as the key when accessing usage counts.</typeparam>
    /// <remarks>
    /// <para>
    /// Performance testing was done between this method, a lock per key and
    /// a ConcurrentDictionary. This method was the fastest. Test was performed
    /// by simulating a multi-threaded environment with 25 keys, 250 clients per
    /// key and 10,000 increments per client. Meaning, each key was incremented
    /// 2,500,000 times.
    /// </para>
    /// <para>
    /// The results showed that ConcurrentDictionary took 35.7 seconds, one lock
    /// per key took 11.3 seconds. This method took 4.7 seconds.
    /// </para>
    /// <para>
    /// In other words, on the test hardware each increment took 0.0019 milliseconds.
    /// </para>
    /// </remarks>
    internal class ConcurrentUsageCounter<TKey>
    {
        #region Fields

        /// <summary>
        /// The object used for synchronization.
        /// </summary>
        protected readonly object _lock = new object();

        /// <summary>
        /// The counter values held by this class.
        /// </summary>
        private readonly Dictionary<TKey, int> _counters = new Dictionary<TKey, int>();

        #endregion

        #region Methods

        /// <summary>
        /// Increments the specified key counter value by one.
        /// </summary>
        /// <param name="key">The key whose value will be incremented.</param>
        /// <returns>The new value after the operation has completed.</returns>
        public int Increment( TKey key )
        {
            lock ( _lock )
            {
                if ( _counters.TryGetValue( key, out var value ) )
                {
                    _counters[key] = value + 1;

                    return value + 1;
                }
                else
                {
                    _counters[key] = 1;

                    return 1;
                }
            }
        }

        /// <summary>
        /// Decrements the specified key counter value by one. This will never
        /// decrement the value below <c>0</c>.
        /// </summary>
        /// <param name="key">The key whose value will be decremented.</param>
        /// <returns>The new value after the operation has completed.</returns>
        public int Decrement( TKey key )
        {
            lock ( _lock )
            {
                if ( _counters.TryGetValue( key, out var value ) )
                {
                    if ( value > 0 )
                    {
                        _counters[key] = value - 1;

                        return value - 1;
                    }
                    else
                    {
                        _counters[key] = 0;

                        return 0;
                    }
                }
                else
                {
                    _counters[key] = 0;

                    return 0;
                }
            }
        }

        #endregion
    }
}
