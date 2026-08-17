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

namespace Rock.ViewModels.Blocks.Cms.ContentChannelItemMetrics
{
    /// <summary>
    /// The option lists for the Viewer Details grid filters.
    /// </summary>
    public class ViewerFilterOptionsBag
    {
        /// <summary>
        /// Gets or sets the campus options, keyed by campus guid.
        /// </summary>
        public List<ListItemBag> Campuses { get; set; }

        /// <summary>
        /// Gets or sets the connection status options, keyed by defined value guid.
        /// </summary>
        public List<ListItemBag> ConnectionStatuses { get; set; }

        /// <summary>
        /// Gets or sets the original source options, keyed by source label.
        /// </summary>
        public List<ListItemBag> Sources { get; set; }
    }
}
