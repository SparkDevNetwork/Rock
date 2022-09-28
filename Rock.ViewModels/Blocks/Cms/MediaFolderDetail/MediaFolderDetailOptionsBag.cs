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
using System;

namespace Rock.ViewModels.Blocks.Cms.MediaFolderDetail
{
    /// <summary>
    /// Class MediaFolderDetailOptionsBag.
    /// </summary>
    public class MediaFolderDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets the content channels.
        /// </summary>
        /// <value>
        /// The content channels.
        /// </value>
        public List<ListItemBag> ContentChannels { get; set; }

        /// <summary>
        /// Gets or sets the content channel attributes.
        /// </summary>
        /// <value>
        /// The content channel attributes.
        /// </value>
        public Dictionary<Guid, List<ListItemBag>> ContentChannelAttributes { get; set; }
    }
}
