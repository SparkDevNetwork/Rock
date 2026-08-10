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

namespace Rock.ViewModels.Blocks.Finance.AccountTreeView
{
    /// <summary>
    /// The configured block settings the Account Tree View ships to its Obsidian component.
    /// </summary>
    public class AccountTreeViewBlockAttributesBag
    {
        /// <summary>
        /// Gets or sets the title shown on the tree panel.
        /// </summary>
        public string PanelTitle { get; set; }

        /// <summary>
        /// Gets or sets whether the settings drawer (active/all filter, ordering) is available.
        /// </summary>
        public bool ShowSettingsPanel { get; set; }

        /// <summary>
        /// Gets or sets whether the public name is displayed for accounts instead of the internal name.
        /// </summary>
        public bool UsePublicName { get; set; }
    }
}
