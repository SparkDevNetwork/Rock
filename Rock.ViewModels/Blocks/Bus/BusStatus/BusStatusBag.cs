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

namespace Rock.ViewModels.Blocks.Bus.BusStatus
{
    /// <summary>
    /// 
    /// </summary>
    public class BusStatusBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is ready.
        /// </summary>
        public bool IsReady { get; set; }
        /// <summary>
        /// Gets or sets the name of the transport.
        /// </summary>
        public string TransportName { get; set; }
        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        public string NodeName { get; set; }
        /// <summary>
        /// Gets or sets the messages per minute.
        /// </summary>
        public int? MessagesPerMinute { get; set; }
        /// <summary>
        /// Gets or sets the messages per hour.
        /// </summary>
        public int? MessagesPerHour { get; set; }
        /// <summary>
        /// Gets or sets the messages per day.
        /// </summary>
        public int? MessagesPerDay { get; set; }
    }
}
