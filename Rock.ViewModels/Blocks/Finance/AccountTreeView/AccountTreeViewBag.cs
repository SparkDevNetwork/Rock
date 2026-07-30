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

using System;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Finance.AccountTreeView
{
    /// <summary>
    /// The runtime data the Account Tree View block ships to its Obsidian component.
    /// </summary>
    public class AccountTreeViewBag
    {
        /// <summary>
        /// Gets or sets the accounts selected on load, resolved from the page parameter for deep-linking.
        /// </summary>
        public List<Guid> SelectedAccountGuids { get; set; }

        /// <summary>
        /// Gets or sets the accounts to expand on load.
        /// </summary>
        public List<Guid> ExpandedAccountGuids { get; set; }

        /// <summary>
        /// Gets or sets an error message to display in place of the tree.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets whether the add-account chrome is shown (block edit or elevated child-add auth).
        /// </summary>
        public bool IsAddAccountVisible { get; set; }

        /// <summary>
        /// Gets or sets whether the Add Top-Level action is enabled.
        /// </summary>
        public bool IsAddRootEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether the Add Child To Selected action is enabled for the current selection.
        /// </summary>
        public bool IsAddChildEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether inactive accounts are hidden, reflecting the person's saved preference
        /// (or the block's initial active setting when no preference is stored).
        /// </summary>
        public bool HideInactiveAccounts { get; set; }
    }
}
