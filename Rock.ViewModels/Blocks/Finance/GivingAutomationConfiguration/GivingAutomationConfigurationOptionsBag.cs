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

namespace Rock.ViewModels.Blocks.Finance.GivingAutomationConfiguration
{
    /// <summary>
    /// The read-only options used to render the Giving Automation Configuration block.
    /// </summary>
    public class GivingAutomationConfigurationOptionsBag
    {
        /// <summary>
        /// Gets or sets the active financial transaction types (value = DefinedValue GUID).
        /// </summary>
        public List<ListItemBag> TransactionTypes { get; set; }

        /// <summary>
        /// Gets or sets the account selection-mode options (All Tax Deductible / Custom).
        /// </summary>
        public List<ListItemBag> AccountTypeOptions { get; set; }

        /// <summary>
        /// Gets or sets the available connection types (value = ConnectionType GUID).
        /// </summary>
        public List<ListItemBag> ConnectionTypes { get; set; }

        /// <summary>
        /// Gets or sets the available connection opportunities (value = ConnectionOpportunity GUID,
        /// category = parent ConnectionType GUID). The client filters these by the selected connection type.
        /// </summary>
        public List<ListItemBag> ConnectionOpportunities { get; set; }

        /// <summary>
        /// Gets or sets the available system communication templates (value = SystemCommunication GUID),
        /// shared by both communication dropdowns in the alert detail modal.
        /// </summary>
        public List<ListItemBag> SystemCommunications { get; set; }

        /// <summary>
        /// Gets or sets the amount-sensitivity help text keyed by <see cref="Rock.Model.AlertType"/> value.
        /// </summary>
        public Dictionary<string, string> AmountSensitivityDescriptions { get; set; }

        /// <summary>
        /// Gets or sets the frequency-sensitivity help text keyed by <see cref="Rock.Model.AlertType"/> value.
        /// </summary>
        public Dictionary<string, string> FrequencySensitivityDescriptions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Campus column should be shown in the alert grid.
        /// This is false when only a single campus exists in the system.
        /// </summary>
        public bool IsCampusColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets the URL of the parent page, used when navigating away on Save or Cancel.
        /// </summary>
        public string ParentPageUrl { get; set; }
    }
}
