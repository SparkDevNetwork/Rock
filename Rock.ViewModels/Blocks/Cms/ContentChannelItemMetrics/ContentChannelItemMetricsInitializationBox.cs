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
using Rock.ViewModels.Core.Grid;

namespace Rock.ViewModels.Blocks.Cms.ContentChannelItemMetrics
{
    /// <summary>
    /// The initialization data for the Content Channel Item Metrics block.
    /// </summary>
    public class ContentChannelItemMetricsInitializationBox
    {
        /// <summary>
        /// Gets or sets the identifier key of the content channel item whose metrics are shown.
        /// </summary>
        public string ContentChannelItemIdKey { get; set; }

        /// <summary>
        /// Gets or sets the title of the content channel item whose metrics are shown.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the name of the content channel the item belongs to.
        /// </summary>
        public string ContentChannelName { get; set; }

        /// <summary>
        /// Gets or sets the approval status of the content channel item.
        /// </summary>
        public ContentChannelItemStatus ItemStatus { get; set; }

        /// <summary>
        /// Gets or sets whether the approval status label should be shown. This is only
        /// true when the content channel requires approval and status is not disabled.
        /// </summary>
        public bool IsStatusVisible { get; set; }

        /// <summary>
        /// Gets or sets whether any interaction data has ever been collected for the item. When
        /// false the block shows a single "tracking is off" notice instead of the metrics UI.
        /// </summary>
        public bool IsCollectingData { get; set; }

        /// <summary>
        /// Gets or sets the grid definition for the Viewer Details grid, including the built-in
        /// person grid action URLs (communicate, merge, bulk update, launch workflow, export).
        /// </summary>
        public GridDefinitionBag ViewerGridDefinition { get; set; }

        /// <summary>
        /// Gets or sets the option lists for the Viewer Details grid filters.
        /// </summary>
        public ViewerFilterOptionsBag ViewerFilterOptions { get; set; }
    }
}
