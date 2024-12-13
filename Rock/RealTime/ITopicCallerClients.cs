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
    public interface ITopicCallerClients<T> : ITopicClients<T>
        where T : class
    {
        /// <summary>
        /// Gets a <typeparamref name="T"/> that can be used to invoke methods
        /// on the current client connection.
        /// </summary>
        T Current { get; }

        /// <summary>
        /// Gets a <typeparamref name="T"/> that can be used to invoke methods
        /// on all client connections other than the <see cref="Current"/> connection.
        /// </summary>
        T Others { get; }

        /// <summary>
        /// Gets a <typeparamref name="T"/> that can be used to invoke methods
        /// on all client connections in the specified channel other than the
        /// <see cref="Current"/> connection.
        /// </summary>
        /// <param name="channelName">The name of the channel to traget.</param>
        /// <returns>A proxy of <typeparamref name="T"/> that can be used to invoke methods.</returns>
        T OthersInChannel( string channelName );
    }
}
