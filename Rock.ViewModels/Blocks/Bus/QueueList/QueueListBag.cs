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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Bus.QueueList
{
    /// <summary>
    /// 
    /// </summary>
    public class QueueListBag
    {
        /// <summary>
        /// Gets or sets the IdKey.
        /// </summary>
        /// <value>
        /// The Queue Type IdKey.
        /// </value>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the message TTL.
        /// </summary>
        /// <value>
        /// The message TTL.
        /// </value>
        public int? TimeToLiveSeconds { get; set; }

        /// <summary>
        /// Gets or sets the name of the queue.
        /// </summary>
        /// <value>
        /// The name of the queue.
        /// </value>
        public string QueueName { get; set; }

        /// <summary>
        /// Gets or sets the type of the queue.
        /// </summary>
        /// <value>
        /// The type of the queue.
        /// </value>
        public string QueueType { get; set; }

        /// <summary>
        /// Gets or sets the rate per minute.
        /// </summary>
        /// <value>
        /// The rate per minute.
        /// </value>
        public int? RatePerMinute { get; set; }

        /// <summary>
        /// Gets or sets the rate per hour.
        /// </summary>
        /// <value>
        /// The rate per hour.
        /// </value>
        public int? RatePerHour { get; set; }

        /// <summary>
        /// Gets or sets the rate per day.
        /// </summary>
        /// <value>
        /// The rate per day.
        /// </value>
        public int? RatePerDay { get; set; }

        /// <summary>
        /// Gets or sets the name of the queue type.
        /// </summary>
        /// <value>
        /// The name of the queue type.
        /// </value>
        public string QueueTypeName { get; set; }
    }
}