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

namespace Rock.ViewModels.Blocks.Finance.UtilityPaymentEntry
{
    /// <summary>
    /// The resolved content for the confirmation step.
    /// </summary>
    public class UtilityPaymentEntryConfirmationResponseBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the entry passed validation and the confirmation was
        /// built.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets the resolved Confirmation Body Lava, the gift summary shown on the confirmation
        /// step.
        /// </summary>
        public string BodyHtml { get; set; }

        /// <summary>
        /// Gets or sets the messages shown to the giver when the entry could not be confirmed.
        /// </summary>
        public List<string> ErrorMessages { get; set; }
    }
}
