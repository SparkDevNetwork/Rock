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

namespace Rock.ViewModels.Blocks.Finance.UtilityPaymentEntry
{
    /// <summary>
    /// A per-account amount seeded from the URL account options, optionally locked so the giver
    /// cannot change it.
    /// </summary>
    public class UtilityPaymentEntryPresetAccountAmountBag
    {
        /// <summary>
        /// Gets or sets the Guid of the account this preset applies to.
        /// </summary>
        public Guid? AccountGuid { get; set; }

        /// <summary>
        /// Gets or sets the initial amount seeded for the account. Null when the URL specified no amount.
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the seeded amount is locked (the giver cannot change it),
        /// from the URL option's editable flag.
        /// </summary>
        public bool IsReadOnly { get; set; }
    }
}
