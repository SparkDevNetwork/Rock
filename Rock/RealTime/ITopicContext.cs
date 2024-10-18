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
    /// <typeparam name="T">
    /// An interface that describes the methods that will be recognized by the
    /// clients connected to the topic.
    /// </typeparam>
    public interface ITopicContext<T> : ITopic
        where T : class
    {
        /// <summary>
        /// Gets a helper object to access client connections by various
        /// filtering options.
        /// </summary>
        ITopicClients<T> Clients { get; }
    }
}
