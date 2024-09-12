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

using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Reporting.PageParameterFilter
{
    /// <summary>
    /// The settings options and other supporting values for the custom settings panel for the page parameter filter block.
    /// </summary>
    public class PageParameterFilterCustomSettingsOptionsBag
    {
        /// <summary>
        /// Gets or sets the filter button size items.
        /// </summary>
        public List<ListItemBag> FilterButtonSizeItems { get; set; }

        /// <summary>
        /// Gets or sets the filter selection action items.
        /// </summary>
        public List<ListItemBag> FilterSelectionActionItems { get; set; }

        /// <summary>
        /// Gets or sets the filters grid definition.
        /// </summary>
        public GridDefinitionBag FiltersGridDefinition { get; set; }

        /// <summary>
        /// Gets or sets the filters reserved key names.
        /// </summary>
        public List<string> FiltersReservedKeyNames { get; set; }
    }
}
