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

namespace Rock.ViewModels.Blocks.Utility.DefinedTypeCheckList
{
    /// <summary>
    /// The data needed to render the Defined Type Check List block.
    /// </summary>
    public class DefinedTypeCheckListBag
    {
        /// <summary>
        /// Gets or sets the checklist title. Rendered as plain text; blank to omit.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the checklist description HTML. Blank to omit.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the checklist items, ordered for display.
        /// </summary>
        public List<DefinedTypeCheckListItemBag> Items { get; set; }
    }
}
