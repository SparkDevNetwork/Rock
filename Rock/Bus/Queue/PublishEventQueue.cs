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

namespace Rock.Bus.Queue
{
    /// <summary>
    /// Publish Event Queue Interface.
    /// Publish Event Queues are intended for publishing event messages.
    /// </summary>
    public interface IPublishEventQueue : IRockQueue
    {
    }

    /// <summary>
    /// Publish Event Queue Interface.
    /// Publish Event Queues are intended for publishing event messages.
    /// </summary>
    public abstract class PublishEventQueue : RockQueue, IPublishEventQueue
    {
        /// <summary>
        /// Gets the name for configuration. On a publish queue we want the name to be specific to the Rock
        /// instance. This is so that each Rock instance receives all the events published on this queue.
        /// If the queue names are the same, then MassTransit assumes only one Rock instance needs to
        /// receive the messages.
        /// </summary>
        /// <value>
        /// The name for configuration.
        /// </value>
        public override string NameForConfiguration => $"{Name}_{RockMessageBus.NodeName}";
    }
}
