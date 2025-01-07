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

namespace Rock.ViewModels.Blocks.Reporting.PageParameterFilter
{
    /// <summary>
    /// The box that contains all the initialization information for the page parameter filter block.
    /// </summary>
    public class PageParameterFilterInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the block title text.
        /// </summary>
        public string BlockTitleText { get; set; }

        /// <summary>
        /// Gets or sets the block title icon CSS class.
        /// </summary>
        public string BlockTitleIconCssClass { get; set; }

        /// <summary>
        /// Gets or sets whether the block title is visible.
        /// </summary>
        public bool IsBlockTitleVisible { get; set; }

        /// <summary>
        /// Gets or sets the filter button text.
        /// </summary>
        public string FilterButtonText { get; set; }

        /// <summary>
        /// Gets or sets the filter button size.
        /// </summary>
        public string FilterButtonSize { get; set; }

        /// <summary>
        /// Gets or sets whether the filter button is visible.
        /// </summary>
        public bool IsFilterButtonVisible { get; set; }

        /// <summary>
        /// Gets or sets whether the reset filters button is visible.
        /// </summary>
        public bool IsResetFiltersButtonVisible { get; set; }

        /// <summary>
        /// Gets or sets how many filters to display per row.
        /// </summary>
        public int FiltersPerRow { get; set; }

        /// <summary>
        /// Gets or sets the public filters available for selection.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> PublicFilters { get; set; }

        /// <summary>
        /// Gets or sets the public filter values.
        /// </summary>
        public Dictionary<string, string> PublicFilterValues { get; set; }

        /// <summary>
        /// Gets or sets the filter page parameters.
        /// </summary>
        public Dictionary<string, string> FilterPageParameters { get; set; }

        /// <summary>
        /// Gets or sets the keys of any filters whose default or startup values will need to be overridden if empty
        /// values are desired (e.g. if a textbox value is cleared in the UI that would otherwise have a default value
        /// if not overridden on the server, or a startup query string value that the server won't know has been cleared
        /// on subsequent block action requests). Identifying these specific keys up front allows us to avoid having to
        /// send every key/value filter pair on every request, and instead allows us to ONLY send these keys with empty
        /// values when needed.
        /// </summary>
        public List<string> FilterKeysWithStartupValues { get; set; }

        /// <summary>
        /// Gets or sets the filter selection action.
        /// </summary>
        public string FilterSelectionAction { get; set; }

        /// <summary>
        /// Gets or sets legacy reload is enabled.
        /// </summary>
        public bool IsLegacyReloadEnabled { get; set; }
    }
}
