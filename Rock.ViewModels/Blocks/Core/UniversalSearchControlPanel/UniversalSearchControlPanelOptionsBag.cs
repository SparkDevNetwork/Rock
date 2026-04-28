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

namespace Rock.ViewModels.Blocks.Core.UniversalSearchControlPanel
{
    /// <summary>
    /// Contains the configuration options for the Universal Search Control Panel block.
    /// </summary>
    public class UniversalSearchControlPanelOptionsBag
    {
        /// <summary>
        /// Gets or sets the list of search type options for the smart search dropdown.
        /// </summary>
        public List<ListItemBag> SearchTypeOptions { get; set; }

        /// <summary>
        /// Gets or sets the list of indexable entity types for the smart search entity checkbox list.
        /// </summary>
        public List<ListItemBag> IndexableEntityOptions { get; set; }
    }
}
