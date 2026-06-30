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

using Rock.Model;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Finance.GivingAutomationConfiguration
{
    /// <summary>
    /// The editable fields of a single financial transaction alert type, shown in the alert detail modal.
    /// </summary>
    public class FinancialTransactionAlertTypeBag
    {
        /// <summary>
        /// Gets or sets the identifier key of the alert type. Empty for a new alert type.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the alert type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the optional campus used to filter people.
        /// </summary>
        public ListItemBag Campus { get; set; }

        /// <summary>
        /// Gets or sets the optional financial account used to filter gifts.
        /// </summary>
        public ListItemBag Account { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether child accounts of the selected account are included.
        /// </summary>
        public bool IsIncludeChildAccounts { get; set; }

        /// <summary>
        /// Gets or sets the alert type (Gratitude or Follow-up).
        /// </summary>
        public AlertType AlertType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether additional rules are considered after this one matches.
        /// </summary>
        public bool IsContinueIfMatched { get; set; }

        /// <summary>
        /// Gets or sets the days of the week (as <see cref="System.DayOfWeek"/> numbers) on which this alert runs.
        /// </summary>
        public List<string> RunDays { get; set; }

        /// <summary>
        /// Gets or sets the number of days between triggering the same alert.
        /// </summary>
        public int? RepeatPreventionDuration { get; set; }

        /// <summary>
        /// Gets or sets the number of interquartile ranges from the median amount required to trigger the alert.
        /// </summary>
        public decimal? AmountSensitivityScale { get; set; }

        /// <summary>
        /// Gets or sets the number of standard deviations from the mean frequency required to trigger the alert.
        /// </summary>
        public decimal? FrequencySensitivityScale { get; set; }

        /// <summary>
        /// Gets or sets the minimum amount the specific gift must be to be considered a match.
        /// </summary>
        public decimal? MinimumGiftAmount { get; set; }

        /// <summary>
        /// Gets or sets the maximum amount the specific gift must be to be considered a match.
        /// </summary>
        public decimal? MaximumGiftAmount { get; set; }

        /// <summary>
        /// Gets or sets the minimum median gift amount for the giver to be considered a match.
        /// </summary>
        public decimal? MinimumMedianGiftAmount { get; set; }

        /// <summary>
        /// Gets or sets the maximum median gift amount for the giver to be considered a match.
        /// </summary>
        public decimal? MaximumMedianGiftAmount { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of days since the giver's last gift.
        /// </summary>
        public int? MaximumDaysSinceLastGift { get; set; }

        /// <summary>
        /// Gets or sets the optional person data view used to further filter who triggers the alert.
        /// </summary>
        public ListItemBag PersonDataView { get; set; }

        /// <summary>
        /// Gets or sets the optional workflow type launched when the alert is matched.
        /// </summary>
        public ListItemBag WorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the selected connection type, used to filter connection opportunities.
        /// </summary>
        public string ConnectionTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the connection opportunity used to create a connection request when matched.
        /// </summary>
        public string ConnectionOpportunityGuid { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the system communication template sent to the donor when matched.
        /// </summary>
        public string DonorSystemCommunicationGuid { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the system communication template sent to account participants when matched.
        /// </summary>
        public string AccountParticipantSystemCommunicationGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an event-bus message is sent when matched.
        /// </summary>
        public bool IsSendBusEvent { get; set; }

        /// <summary>
        /// Gets or sets the group that receives a summary email when an alert of this type is created.
        /// </summary>
        public ListItemBag NotificationGroup { get; set; }
    }
}
