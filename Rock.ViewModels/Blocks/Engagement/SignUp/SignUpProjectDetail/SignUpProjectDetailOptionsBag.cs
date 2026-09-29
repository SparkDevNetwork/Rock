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

using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.SignUp.SignUpProjectDetail
{
    /// <summary>
    /// The additional configuration options for the Sign-Up Project Detail block.
    /// </summary>
    public class SignUpProjectDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets the sign-up group types the parent group allows as child groups. The value
        /// of each item is the group type identifier. Only populated when adding a project.
        /// </summary>
        public List<ListItemBag> AllowedGroupTypes { get; set; }

        /// <summary>
        /// Gets or sets the warning displayed when adding a project whose parent group does not
        /// allow any sign-up group types as child groups.
        /// </summary>
        public string AllowedGroupTypesWarning { get; set; }

        /// <summary>
        /// Gets or sets the project type defined values. The value of each item is the defined
        /// value unique identifier.
        /// </summary>
        public List<ListItemBag> ProjectTypes { get; set; }

        /// <summary>
        /// Gets or sets the system communications that may be used to send reminders.
        /// </summary>
        public List<ListItemBag> ReminderSystemCommunications { get; set; }

        /// <summary>
        /// Gets or sets the URL to navigate to when cancelling the addition of a new project.
        /// </summary>
        public string AddModeCancelUrl { get; set; }

        /// <summary>
        /// Gets or sets the options that depend on the project's current group type.
        /// </summary>
        public SignUpProjectGroupTypeOptionsBag GroupTypeOptions { get; set; }
    }
}
