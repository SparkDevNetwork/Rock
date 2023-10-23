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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The options that can be passed to the GetAccounts API action of
    /// the ampusAccountAmountPicker control.
    /// </summary>
    public class CampusAccountAmountPickerGetAccountsOptionsBag
    {
        /// <summary>
        /// List of GUIDs of accounts to include in the list
        /// </summary>
        public List<Guid> SelectableAccountGuids { get; set; }

        /// <summary>
        /// The Lava template for text label for each account
        /// </summary>
        public string AccountHeaderTemplate { get; set; } = "{{ Account.PublicName }}";

        /// <summary>
        /// Whether to order accounts by the order they were given as SelectableAccountGuids.
        /// If not, will be ordered by Order property.
        /// </summary>
        public bool OrderBySelectableAccountsIndex { get; set; }

        /// <summary>
        /// Gets or sets the security grant token to use when performing
        /// authorization checks.
        /// </summary>
        /// <value>The security grant token.</value>
        public string SecurityGrantToken { get; set; }
    }
}
