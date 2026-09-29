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

namespace Rock.ViewModels.Blocks.Administration.DataAutomationSettings
{
    /// <summary>
    /// Settings that control when children who have become adults are moved to
    /// their own family.
    /// </summary>
    public class MoveAdultChildrenSettingsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether automatically moving adult children is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether only children who have graduated should be moved.
        /// </summary>
        public bool IsOnlyMoveGraduated { get; set; }

        /// <summary>
        /// Gets or sets the age at which a child is considered an adult.
        /// </summary>
        public int? AdultAge { get; set; }

        /// <summary>
        /// Gets or sets the optional known relationship added between the parent(s) and the new adult.
        /// The value is the group type role unique identifier.
        /// </summary>
        public ListItemBag ParentRelationship { get; set; }

        /// <summary>
        /// Gets or sets the optional known relationship added between the new adult and their sibling(s).
        /// The value is the group type role unique identifier.
        /// </summary>
        public ListItemBag SiblingRelationship { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the new adult's home address should match their current family.
        /// </summary>
        public bool UseSameHomeAddress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the new adult should reuse their parent's home phone when they have none.
        /// </summary>
        public bool UseSameHomePhone { get; set; }

        /// <summary>
        /// Gets or sets the workflow types launched for each processed person. Each value is a workflow type unique identifier.
        /// </summary>
        public List<ListItemBag> Workflows { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of records processed at a time.
        /// </summary>
        public int? MaximumRecords { get; set; }
    }
}
