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

namespace Rock.ViewModels.Blocks.Lms.PublicLearningClassWorkspace
{
    /// <summary>
    /// Gets or sets the information required to render the notifications content within a Public Learning Class Workspace block.
    /// </summary>
    public class PublicLearningClassWorkspaceNotificationBag
    {
        /// <summary>
        /// Gets or sets the main content text that should be shown for this notification.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the label text that should be used for this notification.
        /// </summary>
        public string LabelText { get; set; }

        /// <summary>
        /// Gets or sets the label type that should be used for this notification.
        /// </summary>
        public string LabelType { get; set; }

        /// <summary>
        /// Gets or sets the DateTime of the notification.
        /// </summary>
        public DateTime NotificationDateTime { get; set; }

        /// <summary>
        /// Gets or sets the title that should be used for this notification.
        /// </summary>
        public string Title { get; set; }
    }
}
