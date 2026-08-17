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

namespace Rock.ViewModels.Blocks.Finance.ScheduledTransactionEditV2
{
    /// <summary>
    /// The configuration options for the Scheduled Transaction Edit (V2) block.
    /// </summary>
    public class ScheduledTransactionEditV2OptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether ACH bank transfers are available as a
        /// payment method.
        /// </summary>
        public bool IsAchEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether credit card payments are available as a
        /// payment method.
        /// </summary>
        public bool IsCreditCardEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an optional end date can be set.
        /// </summary>
        public bool IsEndDateEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the campus should be prompted for even
        /// when it is already known.
        /// </summary>
        public bool IsCampusPrompted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether titled section headers are shown.
        /// </summary>
        public bool IsSectionHeaderShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether section descriptions are shown.
        /// </summary>
        public bool IsSectionDescriptionShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the block header is shown.
        /// </summary>
        public bool IsBlockHeaderShown { get; set; }

        /// <summary>
        /// Gets or sets the word used to refer to a financial contribution (e.g. "Gift").
        /// </summary>
        public string TransactionTerm { get; set; }

        /// <summary>
        /// Gets or sets the panel title.
        /// </summary>
        public string PanelTitle { get; set; }

        /// <summary>
        /// Gets or sets the block header title.
        /// </summary>
        public string HeaderTitle { get; set; }

        /// <summary>
        /// Gets or sets the block header description.
        /// </summary>
        public string HeaderDescription { get; set; }

        /// <summary>
        /// Gets or sets the block header icon CSS class.
        /// </summary>
        public string HeaderIcon { get; set; }

        /// <summary>
        /// Gets or sets the Campus Information section title.
        /// </summary>
        public string CampusSectionTitle { get; set; }

        /// <summary>
        /// Gets or sets the Campus Information section icon CSS class.
        /// </summary>
        public string CampusSectionIcon { get; set; }

        /// <summary>
        /// Gets or sets the Campus Information section description.
        /// </summary>
        public string CampusSectionDescription { get; set; }

        /// <summary>
        /// Gets or sets the Gift Information section title.
        /// </summary>
        public string GiftSectionTitle { get; set; }

        /// <summary>
        /// Gets or sets the Gift Information section icon CSS class.
        /// </summary>
        public string GiftSectionIcon { get; set; }

        /// <summary>
        /// Gets or sets the Gift Information section description.
        /// </summary>
        public string GiftSectionDescription { get; set; }

        /// <summary>
        /// Gets or sets the Payment Information section title.
        /// </summary>
        public string PaymentSectionTitle { get; set; }

        /// <summary>
        /// Gets or sets the Payment Information section icon CSS class.
        /// </summary>
        public string PaymentSectionIcon { get; set; }

        /// <summary>
        /// Gets or sets the Payment Information section description.
        /// </summary>
        public string PaymentSectionDescription { get; set; }

        /// <summary>
        /// Gets or sets the label for the button that adds another account row.
        /// </summary>
        public string AddAccountButtonLabel { get; set; }
    }
}
