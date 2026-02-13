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

namespace Rock.ViewModels.Blocks.Core.CampusContextSetter
{
    /// <summary>
    /// The initialization options for the Campus Context Setter block.
    /// </summary>
    public class CampusContextSetterOptionsBag
    {
        /// <summary>
        /// The alignment of the dropdown. 1 = Left, 2 = Right.
        /// </summary>
        public int Alignment { get; set; }

        /// <summary>
        /// The list of campuses to display in the dropdown.
        /// </summary>
        public List<ListItemBag> Campuses { get; set; }

        /// <summary>
        /// The text to display as the current selection.
        /// </summary>
        public string CurrentSelectionText { get; set; }
    }
}
