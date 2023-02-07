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

using System.Threading.Tasks;

namespace Rock.RealTime
{
    /// <summary>
    /// A topic provides a way to target your messages to client connections in
    /// a way that prevents conflicts in message names. Subscribing a connection
    /// to a channel on one topic will subscribe them to that channel across
    /// all topics. However, messages sent on this topic will be ignored by the
    /// client if they do not support this topic.
    /// </summary>
    /// <typeparam name="T">
    /// An interface that describes the methods that will be recognized by the
    /// clients connected to the topic.
    /// </typeparam>
    public abstract class Topic<T> : ITopicInternal, ITopic<T>
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
        public ITopicChannelManager Channels { get; private set; }

        /// <inheritdoc/>
        public ITopicCallerClients<T> Clients { get; private set; }

        /// <inheritdoc/>
        public IContext Context { get; private set; }

        #endregion

        #region ITopicInternal

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
            set => Clients = ( ITopicCallerClients<T> ) value;
        }

        /// <inheritdoc/>
        IContext ITopicInternal.Context
        {
            get => Context;
            set => Context = value;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public virtual Task OnConnectedAsync()
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual Task OnDisconnectedAsync()
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}
