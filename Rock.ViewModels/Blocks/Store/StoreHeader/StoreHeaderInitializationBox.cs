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

namespace Rock.ViewModels.Blocks.Store.StoreHeader
{
    /// <summary>
    /// The box that contains the initialization information for the Store Header block.
    /// </summary>
    public class StoreHeaderInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets a value indicating whether the Rock Shop organization has been
        /// configured. When <c>false</c>, the component renders the "Rock Shop Configuration
        /// Needed" panel instead of the organization card. This is <c>false</c> when no
        /// organization key is set or when the store returned no usable organization data.
        /// </summary>
        public bool IsConfigured { get; set; }

        /// <summary>
        /// Gets or sets the organization to display in the header card.
        /// Null when <see cref="IsConfigured"/> is <c>false</c>.
        /// </summary>
        public StoreOrganizationBag Organization { get; set; }
    }
}
