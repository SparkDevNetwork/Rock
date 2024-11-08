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
    /// Provides the functionality to communicate with real-time client connections
    /// from outside the topic message handlers.
    /// </summary>
    public interface ITopic
    {
        /// <summary>
        /// Gets the channel manager that will provide functionality to add and
        /// remove client connections from various channels.
        /// </summary>
        ITopicChannelManager Channels { get; }
    }

    /// <summary>
    /// Defines the structure of a RealTime topic area that will facilitate
    /// communication between the server and remote devices.
    /// </summary>
    /// <typeparam name="T">
    /// An interface that describes the methods that will be recognized by the
    /// clients connected to the topic.
    /// </typeparam>
    public interface ITopic<T> : ITopic
        where T : class
    {
        /// <summary>
        /// Gets a helper object to access client connections by various
        /// filtering options.
        /// </summary>
        ITopicCallerClients<T> Clients { get; }

        /// <summary>
        /// Gets the context that describes the current request.
        /// </summary>
        /// <value>The context that describes the current request.</value>
        IContext Context { get; }
    }
}
