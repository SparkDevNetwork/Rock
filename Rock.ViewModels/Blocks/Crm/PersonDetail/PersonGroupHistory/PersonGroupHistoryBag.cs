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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Crm.PersonDetail.PersonGroupHistory
{
    /// <summary>
    /// The per-person initialization data for the Person Group History block.
    /// </summary>
    public class PersonGroupHistoryBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether a person is in context and the block should render.
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets the group types currently selected in the person's saved filter preference.
        /// Each item's value is a Group Type unique identifier.
        /// </summary>
        public List<ListItemBag> SelectedGroupTypes { get; set; }
    }
}
