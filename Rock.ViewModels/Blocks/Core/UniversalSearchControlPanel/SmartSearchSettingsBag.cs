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

namespace Rock.ViewModels.Blocks.Core.UniversalSearchControlPanel
{
    /// <summary>
    /// Contains the current smart search configuration settings for display in view mode.
    /// </summary>
    public class SmartSearchSettingsBag
    {
        /// <summary>
        /// Gets or sets the list of selected entity type ID strings for smart search.
        /// </summary>
        public List<string> SelectedEntityIds { get; set; }

        /// <summary>
        /// Gets or sets a comma-separated display string of the selected entity names.
        /// </summary>
        public string SelectedEntityNames { get; set; }

        /// <summary>
        /// Gets or sets the current search type value as a string.
        /// </summary>
        public string SearchType { get; set; }

        /// <summary>
        /// Gets or sets the display text for the current search type.
        /// </summary>
        public string SearchTypeText { get; set; }

        /// <summary>
        /// Gets or sets the field criteria filter string for smart search.
        /// </summary>
        public string FieldCriteria { get; set; }
    }
}
