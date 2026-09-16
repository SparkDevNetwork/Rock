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

using Rock.ViewModels.Core.Grid;

namespace Rock.ViewModels.Blocks.Event.CalendarContentChannelItemList
{
    /// <summary>
    /// Describes a single content channel section (collapsible grid) within the
    /// Calendar Content Channel Item List block.
    /// </summary>
    public class CalendarContentChannelItemListContentChannelBag
    {
        /// <summary>
        /// Gets or sets the content channel identifier key.
        /// </summary>
        /// <value>
        /// The content channel IdKey.
        /// </value>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the content channel name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the content channel icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this panel starts expanded.
        /// </summary>
        /// <value>
        /// <c>true</c> if expanded; otherwise, <c>false</c>.
        /// </value>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether add is enabled for this channel.
        /// </summary>
        /// <value>
        /// <c>true</c> if add is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsAddEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether delete is enabled for this channel.
        /// </summary>
        /// <value>
        /// <c>true</c> if delete is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsDeleteEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the start / active date column is shown.
        /// </summary>
        /// <value>
        /// <c>true</c> if the start date column is shown; otherwise, <c>false</c>.
        /// </value>
        public bool ShowStartDateTimeColumn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the expire date column is shown.
        /// </summary>
        /// <value>
        /// <c>true</c> if the expire date column is shown; otherwise, <c>false</c>.
        /// </value>
        public bool ShowExpireDateTimeColumn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether date columns include time.
        /// </summary>
        /// <value>
        /// <c>true</c> if times are included; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the priority column is shown.
        /// </summary>
        /// <value>
        /// <c>true</c> if the priority column is shown; otherwise, <c>false</c>.
        /// </value>
        public bool ShowPriorityColumn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the status column is shown.
        /// </summary>
        /// <value>
        /// <c>true</c> if the status column is shown; otherwise, <c>false</c>.
        /// </value>
        public bool ShowStatusColumn { get; set; }

        /// <summary>
        /// Gets or sets the grid definition for this channel's grid.
        /// </summary>
        /// <value>
        /// The grid definition.
        /// </value>
        public GridDefinitionBag GridDefinition { get; set; }

        /// <summary>
        /// Gets or sets the grid row data for this channel.
        /// </summary>
        /// <value>
        /// The grid data.
        /// </value>
        public GridDataBag GridData { get; set; }
    }
}
