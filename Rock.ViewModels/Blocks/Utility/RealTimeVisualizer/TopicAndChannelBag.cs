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

namespace Rock.ViewModels.Blocks.Utility.RealTimeVisualizer
{
    /// <summary>
    /// A pair that describes a topic and channel to be monitored.
    /// </summary>
    public class TopicAndChannelBag
    {
        /// <summary>
        /// Gets or sets the topic identifier.
        /// </summary>
        /// <value>The topic identifer.</value>
        public string Topic { get; set; }

        /// <summary>
        /// Gets or sets the channel name.
        /// </summary>
        /// <value>The channel name.</value>
        public string Channel { get; set; }
    }
}
