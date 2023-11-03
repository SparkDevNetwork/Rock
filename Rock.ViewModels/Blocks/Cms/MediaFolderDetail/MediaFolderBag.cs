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

namespace Rock.ViewModels.Blocks.Cms.MediaFolderDetail
{
    /// <summary>
    /// Class MediaFolderBag.
    /// Implements the <see cref="Rock.ViewModels.Utility.EntityBagBase" />
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class MediaFolderBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the content channel.
        /// </summary>
        public ListItemBag ContentChannel { get; set; }

        /// <summary>
        /// Gets or sets the content channel item status.
        /// </summary>
        /// <value>
        /// The content channel item status.
        /// </value>
        public string ContentChannelItemStatus { get; set; }

        /// <summary>
        /// Gets or sets the content channel attribute.
        /// </summary>
        public ListItemBag ContentChannelAttribute { get; set; }

        /// <summary>
        /// Gets or sets a description of the MediaFolder.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the content channel sync is enabled.
        /// </summary>
        public bool IsContentChannelSyncEnabled { get; set; }

        /// <summary>
        /// Gets or sets the Media Account that this MediaFolder belongs to.
        /// </summary>
        public ListItemBag MediaAccount { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the Elements that belong to this Folder.
        /// It is used to display the folder's attributes when in view mode.
        /// </summary>
        public List<ListItemBag> ContentChannelItemAttributes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if this Media Folder is public.
        /// </summary>
        public bool? IsPublic { get; set; }

        /// <summary>
        /// Gets or sets the Name of the MediaFolder. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the workflow that will be launched when
        /// a new Rock.Model.MediaElement is added.
        /// </summary>
        public ListItemBag WorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the metric data.
        /// </summary>
        /// <value>
        /// The metric data.
        /// </value>
        public string MetricData { get; set; }
    }
}
