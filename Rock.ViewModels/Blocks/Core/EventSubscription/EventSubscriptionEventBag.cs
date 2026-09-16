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

namespace Rock.ViewModels.Blocks.Core.EventSubscription
{
    /// <summary>
    /// A following event that the current person may subscribe to.
    /// </summary>
    public class EventSubscriptionEventBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the following event type.
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// Gets or sets the name of the following event.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the following event. May contain
        /// basic HTML.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether notification for this event
        /// is required. Required events are always subscribed and cannot be
        /// unchecked.
        /// </summary>
        public bool IsNoticeRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current person is
        /// subscribed to this event.
        /// </summary>
        public bool IsSubscribed { get; set; }
    }
}
