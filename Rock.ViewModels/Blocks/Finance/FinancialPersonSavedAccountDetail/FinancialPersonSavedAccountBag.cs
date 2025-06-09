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

using Rock.ViewModels.Utility;

using System;

namespace Rock.ViewModels.Blocks.Finance.FinancialPersonSavedAccountDetail
{
    /// <summary>
    /// Represents a bag for storing details of a financial person's saved account.
    /// </summary>
    public class FinancialPersonSavedAccountBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the name of the saved account.
        /// </summary>
        /// <value>
        /// The name of the saved account. This property is required.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the masked account number.
        /// </summary>
        /// <value>
        /// The masked account number, typically displaying only the last few digits for security.
        /// </value>
        public string AccountNumberMasked { get; set; }

        /// <summary>
        /// Gets or sets the expiration date of the saved account.
        /// </summary>
        /// <value>
        /// The expiration date of the saved account in MM/YY format.
        /// </value>
        public string ExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the description of the saved account.
        /// </summary>
        /// <value>
        /// The description or additional notes about the saved account.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the image source for the saved account.
        /// </summary>
        /// <value>
        /// The image source for the saved account.
        /// </value>
        public string ImageSource { get; set; }

        /// <summary>
        /// Gets or sets the currency type guid for this saved account.
        /// </summary>
        public Guid? CurrencyTypeValueGuid { get; set; }

        /// <summary>
        /// Gets or sets the GUID for this saved account.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Whether or not the saved account can be deleted.
        /// Note, this is false if the saved account is being used in a scheduled transaction.
        /// </summary>
        public bool IsUsedInScheduledTransaction { get; set; }
    }
}
