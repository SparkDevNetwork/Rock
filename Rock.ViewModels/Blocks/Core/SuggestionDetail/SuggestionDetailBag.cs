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
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Core.SuggestionDetail
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class SuggestionDetailBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the user defined description of the FollowingSuggestion.
        /// </summary>Y
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets how an entity should be formatted when included in the suggestion notification to follower.
        /// </summary>
        public string EntityNotificationFormatLava { get; set; }

        /// <summary>
        /// Gets or sets the type of the suggestion entity.
        /// </summary>
        public ListItemBag EntityType { get; set; }

        /// <summary>
        /// Gets or sets the suggestion entity type identifier.
        /// </summary>
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the (internal) Name of the FollowingSuggestion. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the reason note to use when suggesting an entity be followed
        /// </summary>
        public string ReasonNote { get; set; }

        /// <summary>
        /// Gets or sets the reminder days.
        /// </summary>
        public int? ReminderDays { get; set; }
    }
}
