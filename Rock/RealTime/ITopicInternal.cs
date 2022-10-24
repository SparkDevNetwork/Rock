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
    /// Defines the structure of a RealTime topic area that will facilitate
    /// communication between the server and remote devices.
    /// </summary>
    internal interface ITopicInternal : ITopicContextInternal
    {
        /// <summary>
        /// Gets or sets the context that describes the current request.
        /// </summary>
        /// <value>The context that describes the current request.</value>
        IContext Context { get; set; }

        /// <summary>
        /// Called when a new connection is established with the topic.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous connect.</returns>
        Task OnConnectedAsync();

        /// <summary>
        /// Called when a connection with the topic is terminated.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous disconnect.</returns>
        Task OnDisconnectedAsync();
    }
}
