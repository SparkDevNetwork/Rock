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

using Rock.ViewModels.Cms;

namespace Rock.Blocks
{
    /// <summary>
    /// Identifies a block as having custom administrative actions that should
    /// be added to the block's configuration bar.
    /// </summary>
    public interface IHasCustomActions
    {
        /// <summary>
        /// Adds custom actions to the configuration area of a block instance.
        /// No other properties on the block instance are valid during this call.
        /// </summary>
        /// <param name="canEdit">A <see cref="bool" /> flag that indicates if the user can edit the block instance.</param>
        /// <param name="canAdministrate">A <see cref="bool" /> flag that indicates if the user can administrate the block instance.</param>
        /// <returns>
        /// A <see cref="List{ObsidianBlockCustomConfigActionBag}" /> containing all the
        /// custom actions that will be available to the individual in the configuration
        /// area of the block instance.
        /// </returns>
        List<BlockCustomActionBag> GetCustomActions( bool canEdit, bool canAdministrate );
    }
}
