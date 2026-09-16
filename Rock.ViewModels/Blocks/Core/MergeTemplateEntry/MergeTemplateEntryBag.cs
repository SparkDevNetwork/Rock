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

namespace Rock.ViewModels.Blocks.Core.MergeTemplateEntry
{
    /// <summary>
    /// The state used to render the Merge Template Entry block.
    /// </summary>
    public class MergeTemplateEntryBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the merge entry panel should be shown.
        /// This is <c>false</c> when no entity set was provided or it could not be found.
        /// </summary>
        public bool IsEntryPanelVisible { get; set; }

        /// <summary>
        /// Gets or sets a warning message to display in place of the entry panel
        /// (for example, when the requested merge records could not be found).
        /// </summary>
        public string WarningMessage { get; set; }

        /// <summary>
        /// Gets or sets the number of rows that will be merged.
        /// </summary>
        public int RecordCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the "Combine Family Members" option
        /// should be shown. This is only relevant for Person and Group Member entity sets.
        /// </summary>
        public bool IsCombineFamilyMembersVisible { get; set; }
    }
}
