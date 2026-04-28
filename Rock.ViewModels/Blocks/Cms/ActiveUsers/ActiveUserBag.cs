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

namespace Rock.ViewModels.Blocks.Cms.ActiveUsers
{
    /// <summary>
    /// Represents a single active user entry for the Active Users block.
    /// </summary>
    public class ActiveUserBag
    {
        /// <summary>
        /// Gets or sets the formatted full name of the person.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the URL to the person's profile page. When null or empty, the name is rendered as plain text.
        /// </summary>
        public string ProfileUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user's last activity was within the last 5 minutes.
        /// Drives the green (recent) vs. yellow (not-recent) indicator color.
        /// </summary>
        public bool IsRecent { get; set; }

        /// <summary>
        /// Gets or sets the recent page titles to display in the hover tooltip, ordered most-recent first.
        /// Limited to the latest interaction session and capped at the block's Page View Count setting.
        /// </summary>
        public List<string> PageTitles { get; set; }
    }
}
