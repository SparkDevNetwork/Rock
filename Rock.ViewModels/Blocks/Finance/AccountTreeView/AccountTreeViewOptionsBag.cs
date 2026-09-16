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
    /// The configuration options the Account Tree View block ships to its Obsidian component.
    /// </summary>
    public class AccountTreeViewOptionsBag
    {
        /// <summary>
        /// Gets or sets the configured block settings describing how the tree is displayed and scoped.
        /// </summary>
        public AccountTreeViewBlockAttributesBag BlockProperties { get; set; }
    }
}
