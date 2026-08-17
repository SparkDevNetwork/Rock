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

using Rock.Model;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Reminders.ReminderTypeList
{
    /// <summary>
    /// The item details for the Reminder Type List block's inline edit modal.
    /// </summary>
    public class ReminderTypeBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the unique identifier of the reminder type.
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// Gets or sets the name of the reminder type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the reminder type.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this reminder type is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the notification type for this reminder type.
        /// Null when no selection has been made.
        /// </summary>
        public ReminderNotificationType? NotificationType { get; set; }

        /// <summary>
        /// Gets or sets the notification workflow type selected for this reminder type.
        /// </summary>
        public ListItemBag NotificationWorkflowType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the note should be shown for this reminder type.
        /// </summary>
        public bool ShouldShowNote { get; set; }

        /// <summary>
        /// Gets or sets the entity type that this reminder type applies to.
        /// </summary>
        public ListItemBag EntityType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether reminders of this type
        /// should auto-complete when the individual is notified.
        /// </summary>
        public bool ShouldAutoCompleteWhenNotified { get; set; }

        /// <summary>
        /// Gets or sets the highlight color used for display purposes.
        /// </summary>
        public string HighlightColor { get; set; }
    }
}
