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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Finance
{
    /// <summary>
    /// A list item that describes an payment account the individual has
    /// previously setup that can be used again.
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.ListItemBag" />
    public class SavedFinancialAccountListItemBag : ListItemBag
    {
        /// <summary>
        /// Gets or sets the description of the card to help identify it to the
        /// individual.
        /// </summary>
        /// <value>The description of the card to help identify it to the individual..</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the image to be displayed to help identify the card.
        /// </summary>
        /// <value>The image to be displayed to help identify the card.</value>
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the masked account number.
        /// </summary>
        public string AccountNumberMasked { get; set; }

        /// <summary>
        /// The Guid for the currency type of this account.
        /// </summary>
        public Guid? CurrencyTypeGuid { get; set; }
    }
}
