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

using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNet.SignalR;

namespace Rock.RealTime.AspNet
{
    /// <summary>
    /// The channel manager implementation used in the ASP.Net environment.
    /// </summary>
    internal class TopicChannelManager : ITopicChannelManager
    {
        #region Fields

        /// <summary>
        /// The SignalR group manager that will handle the groups.
        /// </summary>
        private readonly IGroupManager _groupManager;

        /// <summary>
        /// The topic identifier to use when prefixing channel names.
        /// </summary>
        private readonly string _topicIdentifier;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="TopicChannelManager"/> designed
        /// to work with the ASP.Net environment.
        /// </summary>
        /// <param name="groupManager">The SignalR group manager.</param>
        /// <param name="topicIdentifier">The topic identifier to use when prefixing channel names.</param>
        public TopicChannelManager( IGroupManager groupManager, string topicIdentifier )
        {
            _groupManager = groupManager;
            _topicIdentifier = topicIdentifier;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public Task AddToChannelAsync( string connectionId, string channelName, CancellationToken cancellationToken = default )
        {
            return _groupManager.Add( connectionId, $"{_topicIdentifier}-{channelName}" );
        }

        /// <inheritdoc/>
        public Task RemoveFromChannelAsync( string connectionId, string channelName, CancellationToken cancellationToken = default )
        {
            return _groupManager.Remove( connectionId, $"{_topicIdentifier}-{channelName}" );
        }

        #endregion
    }
}
