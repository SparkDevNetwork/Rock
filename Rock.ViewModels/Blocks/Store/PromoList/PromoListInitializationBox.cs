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

namespace Rock.ViewModels.Blocks.Store.PromoList
{
    /// <summary>
    /// The box that contains the initialization information for the Promo List block.
    /// </summary>
    public class PromoListInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the display style. Either "Card List" or "Rotator".
        /// </summary>
        public string DisplayStyle { get; set; }

        /// <summary>
        /// Gets or sets the promos to display.
        /// </summary>
        public List<PromoBag> Promos { get; set; }

        /// <summary>
        /// Gets or sets the panel title for the card list display style.
        /// Derived server-side from the PromoType block setting.
        /// Empty string when no heading should be shown (e.g. "All" promo type).
        /// </summary>
        public string PanelTitle { get; set; }

        /// <summary>
        /// Gets or sets the store error message. When non-empty, the store could not be
        /// reached and the component renders the "Store Currently Not Available" panel
        /// instead of the promo list.
        /// </summary>
        public string StoreErrorMessage { get; set; }
    }
}
