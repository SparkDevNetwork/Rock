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

namespace Rock.ViewModels.Blocks.Cms.UniversalSearch
{
    /// <summary>
    /// The settings that will be edited in the custom settings panel for the Universal Search block.
    /// </summary>
    public class CustomSettingsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the model filter should be displayed.
        /// </summary>
        public bool ShowFilters { get; set; }

        /// <summary>
        /// Gets or sets the entity type identifiers that should be enabled for searching.
        /// </summary>
        public List<string> EnabledModels { get; set; }

        /// <summary>
        /// Gets or sets the number of results to display per page.
        /// </summary>
        public int? ResultsPerPage { get; set; }

        /// <summary>
        /// Gets or sets the search type value.
        /// </summary>
        public string SearchType { get; set; }

        /// <summary>
        /// Gets or sets the base field filters that are always applied.
        /// </summary>
        public string BaseFieldFilters { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the refined search options should be displayed.
        /// </summary>
        public bool ShowRefinedSearch { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether result scores should be displayed.
        /// </summary>
        public bool ShowScores { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the custom results template should be used.
        /// </summary>
        public bool UseCustomResults { get; set; }

        /// <summary>
        /// Gets or sets the custom Lava template used to render results.
        /// </summary>
        public string LavaResultTemplate { get; set; }

        /// <summary>
        /// Gets or sets the custom Lava commands that are enabled for the results template.
        /// </summary>
        public List<string> CustomResultsCommands { get; set; }

        /// <summary>
        /// Gets or sets the Lava that should be rendered before the search input.
        /// </summary>
        public string PreHtml { get; set; }

        /// <summary>
        /// Gets or sets the Lava that should be rendered after the search input.
        /// </summary>
        public string PostHtml { get; set; }
    }
}
