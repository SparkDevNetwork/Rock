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

namespace Rock.ViewModels.Blocks.Core.SmartSearch
{
    /// <summary>
    /// Represents a single search filter available in the Smart Search block.
    /// </summary>
    public class SearchFilterBag
    {
        /// <summary>
        /// Gets or sets the search component key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the display label for this search filter.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the result URL template for this search filter.
        /// The placeholder {0} is replaced with the search term.
        /// </summary>
        public string ResultUrl { get; set; }
    }
}
