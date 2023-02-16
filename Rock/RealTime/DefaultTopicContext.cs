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

namespace Rock.RealTime
{
    /// <summary>
    /// Default implementation for <see cref="ITopicContext{T}"/> to provide
    /// access to communication with clients from outside a topic message handler.
    /// </summary>
    /// <typeparam name="T">
    /// An interface that describes the methods that will be recognized by the
    /// clients connected to the topic.
    /// </typeparam>
    internal class DefaultTopicContext<T> : ITopicContext<T>, ITopicContextInternal
        where T : class
    {
        #region Fields

        /// <summary>
        /// The engine that this topic is owned by.
        /// </summary>
        private Engine _engine;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public ITopicChannelManager Channels { get; internal set; }

        /// <inheritdoc/>
        public ITopicClients<T> Clients { get; internal set; }

        #endregion

        #region ITopicContextInternal

        /// <inheritdoc/>
        Engine ITopicContextInternal.Engine
        {
            get => _engine;
            set => _engine = value;
        }

        /// <inheritdoc/>
        ITopicChannelManager ITopicContextInternal.Channels
        {
            get => Channels;
            set => Channels = value;
        }

        /// <inheritdoc/>
        object ITopicContextInternal.Clients
        {
            get => Clients;
            set => Clients = ( ITopicClients<T> ) value;
        }

        #endregion
    }
}
