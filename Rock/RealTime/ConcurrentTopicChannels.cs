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

namespace Rock.RealTime
{
    /// <summary>
    /// Tracks which topics and channels a connection has joined.
    /// </summary>
    /// <remarks>
    /// See <see cref="ConcurrentUsageCounter{T}"/> for a discussion on performance.
    /// </remarks>
    internal class ConcurrentTopicChannels
    {
        #region Fields

        /// <summary>
        /// The object used for synchronization.
        /// </summary>
        protected readonly object _lock = new object();

        /// <summary>
        /// The counter values held by this class.
        /// </summary>
        private readonly Dictionary<string, int> _counters = new Dictionary<string, int>();

        /// <summary>
        /// The channels that have been joined by the related connection.
        /// </summary>
        private readonly Dictionary<string, List<string>> _channels = new Dictionary<string, List<string>>();

        #endregion

        #region Methods

        /// <summary>
        /// Increments the usage count for the topic identifier.
        /// </summary>
        /// <param name="topicIdentifier">The topic identifier.</param>
        /// <returns>The usage count of the topic after the operation has completed.</returns>
        public int IncrementTopic( string topicIdentifier )
        {
            lock ( _lock )
            {
                if ( _counters.TryGetValue( topicIdentifier, out var value ) )
                {
                    _counters[topicIdentifier] = value + 1;

                    return value + 1;
                }
                else
                {
                    _counters[topicIdentifier] = 1;

                    return 1;
                }
            }
        }

        /// <summary>
        /// Decrements the usage count for the topic identifier.
        /// </summary>
        /// <param name="topicIdentifier">The topic identifier.</param>
        /// <param name="channels">On return, contains the channels that the connection should be removed from.</param>
        /// <returns>The usage count of the topic after the operation has completed.</returns>
        public int DecrementTopic( string topicIdentifier, out IEnumerable<string> channels )
        {
            lock ( _lock )
            {
                if ( _counters.TryGetValue( topicIdentifier, out var value ) )
                {
                    if ( value > 0 )
                    {
                        _counters[topicIdentifier] = value - 1;
                        channels = Array.Empty<string>();

                        return value - 1;
                    }
                }

                if ( _channels.TryGetValue( topicIdentifier, out var channelList ) )
                {
                    // Make a copy to keep it thread safe.
                    channels = channelList.ToArray();
                }
                else
                {
                    channels = Array.Empty<string>();
                }

                _counters[topicIdentifier] = 0;
                _channels[topicIdentifier] = new List<string>();

                return 0;
            }
        }

        /// <summary>
        /// Determines whether the specified topic identifier is currently connected.
        /// </summary>
        /// <param name="topicIdentifier">The topic identifier.</param>
        /// <returns><c>true</c> if the specified topic identifier is connected; otherwise, <c>false</c>.</returns>
        public bool IsConnected( string topicIdentifier )
        {
            lock ( _lock )
            {
                return _counters.TryGetValue( topicIdentifier, out var value ) && value > 0;
            }
        }

        /// <summary>
        /// Adds the named channel to the list of known channels for the topic identifier.
        /// </summary>
        /// <param name="topicIdentifier">The topic identifier.</param>
        /// <param name="channelName">Name of the channel.</param>
        public void AddTopicChannel( string topicIdentifier, string channelName )
        {
            lock ( _lock )
            {
                if ( _channels.TryGetValue( topicIdentifier, out var channels ) )
                {
                    channels.Add( channelName );
                }
                else
                {
                    _channels[topicIdentifier] = new List<string> { channelName };
                }
            }
        }

        /// <summary>
        /// Removes the named channel to the list of known channels for the topic identifier.
        /// </summary>
        /// <param name="topicIdentifier">The topic identifier.</param>
        /// <param name="channelName">Name of the channel.</param>
        public void RemoveTopicChannel( string topicIdentifier, string channelName )
        {
            lock ( _lock )
            {
                if ( _channels.TryGetValue( topicIdentifier, out var channels ) )
                {
                    channels.Remove( channelName );
                }
            }
        }

        /// <summary>
        /// Gets all connected topics.
        /// </summary>
        /// <returns>An enumeration of all topic identifiers that are currently connected.</returns>
        public IEnumerable<string> GetAllConnectedTopics()
        {
            var topics = new List<string>();

            lock ( _lock )
            {
                foreach ( var pair in _counters )
                {
                    if ( pair.Value > 0 )
                    {
                        topics.Add( pair.Key );
                    }
                }
            }

            return topics;
        }

        /// <summary>
        /// Clears this instance and returns it to a default state.
        /// </summary>
        public void Clear()
        {
            lock ( _lock )
            {
                _counters.Clear();
                _channels.Clear();
            }
        }

        #endregion
    }
}
