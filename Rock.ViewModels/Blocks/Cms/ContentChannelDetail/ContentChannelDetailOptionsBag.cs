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

namespace Rock.ViewModels.Blocks.Cms.ContentChannelDetail
{
    /// <summary>
    /// The additional configuration options for the Content Channel Detail block.
    /// </summary>
    public class ContentChannelDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the server has index enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the server has index enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsIndexEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether there is any approver configured.
        /// </summary>
        /// <value>
        ///   <c>true</c> if an approver is configured; otherwise, <c>false</c>.
        /// </value>
        public bool IsApproverConfigured { get; set; }

        /// <summary>
        /// Gets or sets the content channel types.
        /// </summary>
        /// <value>
        /// The content channel types.
        /// </value>
        public List<ListItemBag> ContentChannelTypes { get; set; }

        /// <summary>
        /// Gets or sets the content control types.
        /// </summary>
        /// <value>
        /// The content control types.
        /// </value>
        public List<ListItemBag> ContentControlTypes { get; set; }

        /// <summary>
        /// Gets or sets the settings options.
        /// </summary>
        /// <value>
        /// The settings options.
        /// </value>
        public List<ListItemBag> SettingsOptions { get; set; }

        /// <summary>
        /// Gets or sets the available licenses.
        /// </summary>
        /// <value>
        /// The available licenses.
        /// </value>
        public List<ListItemBag> AvailableLicenses { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the organization's store is configured
        /// </summary>
        /// <value>
        ///   <c>true</c> if the store is configured; otherwise, <c>false</c>.
        /// </value>
        public bool IsOrganizationConfigured { get; set; }

        /// <summary>
        /// Gets or sets the content library attributes.
        /// </summary>
        /// <value>
        /// The content library attributes.
        /// </value>
        public List<ListItemBag> ContentLibraryAttributes { get; set; }

        /// <summary>
        /// Gets or sets the content channel list.
        /// </summary>
        /// <value>
        /// The content channel list.
        /// </value>
        public List<ListItemBag> ContentChannelList { get; set; }

        /// <summary>
        /// Gets or sets the current page URL.
        /// </summary>
        /// <value>
        /// The current page URL.
        /// </value>
        public string CurrentPageUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [disable content field].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [disable content field]; otherwise, <c>false</c>.
        /// </value>
        public bool DisableContentField { get; set; }
    }
}
