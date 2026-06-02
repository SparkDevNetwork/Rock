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

namespace Rock.ViewModels.Blocks.Group.GroupDetail
{
    /// <summary>
    /// Response payload for the <c>GetGroupRequirementOptions</c> block action, returned when
    /// the user opens the Add / Edit Requirement modal.
    /// </summary>
    public class GroupRequirementOptionsBag
    {
        /// <summary>
        /// Gets or sets the GroupRequirementType dropdown options. Each entry carries the
        /// type's <c>DueDateType</c> so the modal's Due Date well knows which sub-control to render.
        /// </summary>
        public List<GroupRequirementTypeBag> GroupRequirementTypes { get; set; }

        /// <summary>
        /// Gets or sets the date-typed group-attribute dropdown options for the modal's
        /// Due Date Attribute well.
        /// </summary>
        public List<ListItemBag> GroupAttributes { get; set; }
    }
}
