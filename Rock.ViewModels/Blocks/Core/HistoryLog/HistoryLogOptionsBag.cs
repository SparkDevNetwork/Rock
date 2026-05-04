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

namespace Rock.ViewModels.Blocks.Core.HistoryLog
{
    /// <summary>
    /// The additional configuration options for the History Log block.
    /// </summary>
    public class HistoryLogOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether a valid context entity is available.
        /// </summary>
        public bool HasContextEntity { get; set; }

        /// <summary>
        /// Gets or sets the panel title displayed above the grid.
        /// </summary>
        public string PanelTitle { get; set; }

        /// <summary>
        /// Gets or sets the export file name for the grid.
        /// </summary>
        public string ExportFileName { get; set; }

        /// <summary>
        /// Gets or sets the created date text displayed in the panel header.
        /// </summary>
        public string CreatedDateText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Category column should be visible.
        /// </summary>
        public bool IsCategoryColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Category filter should be visible.
        /// </summary>
        public bool IsCategoryFilterVisible { get; set; }
    }
}