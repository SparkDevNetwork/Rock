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

using Rock.Attribute;

namespace Rock.RealTime
{
    /// <summary>
    /// Defines the methods and properties used to gain access to clients on
    /// a specific topic.
    /// </summary>
    /// <typeparam name="T">
    /// An interface that describes the methods that will be recognized by the
    /// clients connected to the topic.
    /// </typeparam>
    public interface ITopicClients<T>
        where T : class
    {
        /// <summary>
        /// Gets a <typeparamref name="T"/> that can be used to invoke methods
        /// on all clients connected to the topic.
        /// </summary>
        T All { get; }

        /// <summary>
        /// Gets a <typeparamref name="T"/> that can be used to invoke methods
        /// on all clients connected to the topic excluding the specified client
        /// connections.
        /// </summary>
        /// <param name="excludedConnectionIds">A collection of connection identifiers to exclude.</param>
        /// <returns>A proxy of <typeparamref name="T"/> that can be used to invoke methods.</returns>
        T AllExcept( IReadOnlyList<string> excludedConnectionIds );

        /// <summary>
        /// Gets a <typeparamref name="T"/> that can be used to invoke methods
        /// on the specified client connection.
        /// </summary>
        /// <param name="connectionId">The identifier of the connection to target.</param>
        /// <returns>A proxy of <typeparamref name="T"/> that can be used to invoke methods.</returns>
        T Client( string connectionId );

        /// <summary>
        /// Gets a <typeparamref name="T"/> that can be used to invoke methods
        /// on the specified client connections.
        /// </summary>
        /// <param name="connectionIds">A collection of connection identifiers to include.</param>
        /// <returns>A proxy of <typeparamref name="T"/> that can be used to invoke methods.</returns>
        T Clients( IReadOnlyList<string> connectionIds );

        /// <summary>
        /// Gets a <typeparamref name="T"/> that can be used to invoke methods
        /// on all connections in the specified channel.
        /// </summary>
        /// <param name="channelName">The name of the channel to traget.</param>
        /// <returns>A proxy of <typeparamref name="T"/> that can be used to invoke methods.</returns>
        T Channel( string channelName );

        /// <summary>
        /// Gets a <typeparamref name="T"/> that can be used to invoke methods
        /// on all connections in the specified channel excluding the specified
        /// connections.
        /// </summary>
        /// <param name="channelName">The name of the channel to target.</param>
        /// <param name="excludedConnectionIds">A collection of connection identifiers to exclude.</param>
        /// <returns>A proxy of <typeparamref name="T"/> that can be used to invoke methods.</returns>
        T ChannelExcept( string channelName, IReadOnlyList<string> excludedConnectionIds );

        /// <summary>
        /// Gets a <typeparamref name="T"/> that can be used to invoke methods
        /// on all connections in all of the specified channels.
        /// </summary>
        /// <param name="channelNames">A collection of channel names to target.</param>
        /// <returns>A proxy of <typeparamref name="T"/> that can be used to invoke methods.</returns>
        T Channels( IReadOnlyList<string> channelNames );

        /// <summary>
        /// Gets a <typeparamref name="T"/> that can be used to invoke methods
        /// on all connections associated with the specified person.
        /// </summary>
        /// <param name="personId">The identifier of the person to target.</param>
        /// <returns>A proxy of <typeparamref name="T"/> that can be used to invoke methods.</returns>
        T Person( int personId );

        /// <summary>
        /// Gets a <typeparamref name="T"/> that can be used to invoke methods
        /// on all connections associated with all of the specified people.
        /// </summary>
        /// <param name="personIds">A collection of person identifiers to target all connections belonging to these people.</param>
        /// <returns>A proxy of <typeparamref name="T"/> that can be used to invoke methods.</returns>
        T People( IReadOnlyList<int> personIds );

        /// <summary>
        /// Gets a <typeparamref name="T"/> that can be used to invoke methods
        /// on all connections associated with the specified person.
        /// </summary>
        /// <param name="visitorAliasId">The identifier of the visitor person alias to target.</param>
        /// <returns>A proxy of <typeparamref name="T"/> that can be used to invoke methods.</returns>
        T Visitor( int visitorAliasId );
    }
}
