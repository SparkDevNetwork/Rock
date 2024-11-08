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

using Rock.Attribute;

namespace Rock.RealTime
{
    /// <summary>
    /// Defines a manager for channel membership on a topic in the RealTime system.
    /// </summary>
    public interface ITopicChannelManager
    {
        /// <summary>
        /// Adds a connection to the specified channel.
        /// </summary>
        /// <param name="connectionId">The identifier of the connection that should be added to the channel.</param>
        /// <param name="channelName">The name of the channel that the connection will be added to.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work if it has not yet started.</param>
        /// <returns>A <see cref="Task"/> that can be used to determine when the operation has completed.</returns>
        Task AddToChannelAsync( string connectionId, string channelName, CancellationToken cancellationToken = default );

        /// <summary>
        /// Removes a connection from the specified channel.
        /// </summary>
        /// <param name="connectionId">The identifier of the connection that should be removed from the channel.</param>
        /// <param name="channelName">The name of the channel that the connection will be removed from.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work if it has not yet started.</param>
        /// <returns>A <see cref="Task"/> that can be used to determine when the operation has completed.</returns>
        Task RemoveFromChannelAsync( string connectionId, string channelName, CancellationToken cancellationToken = default );
    }
}
