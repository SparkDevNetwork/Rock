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
    /// Defines the internal structure of a topic context for use by Rock.
    /// </summary>
    internal interface ITopicContextInternal
    {
        /// <summary>
        /// Gets or sets the engine that this topic belongs to.
        /// </summary>
        /// <value>The engine that this topic belongs to.</value>
        Engine Engine { get; set; }

        /// <summary>
        /// Gets or sets the channel manager that will provide functionality
        /// to add and remove client connections from various channels.
        /// </summary>
        /// <value>The channel manager.</value>
        ITopicChannelManager Channels { get; set; }

        /// <summary>
        /// Gets or sets the helper object to access client connections by various
        /// filtering options.
        /// </summary>
        /// <value>The helper object to access client connections.</value>
        object Clients { get; set; }
    }
}
