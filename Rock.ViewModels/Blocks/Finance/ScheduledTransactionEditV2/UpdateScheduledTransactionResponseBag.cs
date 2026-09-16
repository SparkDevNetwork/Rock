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
    /// The result of updating a scheduled transaction.
    /// </summary>
    public class UpdateScheduledTransactionResponseBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the update succeeded.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets the resolved success/confirmation Lava HTML to display.
        /// </summary>
        public string SuccessHtml { get; set; }

        /// <summary>
        /// Gets or sets a validation or error message to display when the update did not succeed.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
