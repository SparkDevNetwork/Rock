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

using System;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Core.FollowingEventTypeDetail
{
    /// <summary>
    /// Following Event Type View Model
    /// </summary>
    public class FollowingEventTypeBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the user defined description of the FollowingEvent.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets how an entity should be formatted when included in the event notification to follower.
        /// </summary>
        public string EntityNotificationFormatLava { get; set; }

        /// <summary>
        /// Gets or sets the type of the event entity.
        /// </summary>
        public ListItemBag EntityType { get; set; }

        /// <summary>
        /// Gets or sets the type of the followed entity.
        /// </summary>
        public ListItemBag FollowedEntityType { get; set; }

        /// <summary>
        /// Gets or sets the followed entity type identifier.
        /// </summary>
        public int? FollowedEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include non public requests].
        /// </summary>
        public bool IncludeNonPublicRequests { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this event is required. If not, followers will be able to optionally select if they want to be notified of this event
        /// </summary>
        public bool IsNoticeRequired { get; set; }

        /// <summary>
        /// Gets or sets the last check.
        /// </summary>
        public DateTime? LastCheckDateTime { get; set; }

        /// <summary>
        /// Gets or sets the (internal) Name of the FollowingEvent. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [send on weekends].
        /// </summary>
        public bool SendOnWeekends { get; set; }
    }
}
