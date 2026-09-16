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
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Crm.PersonDetail.Relationships
{
    /// <summary>
    /// The configuration options for the Relationships block.
    /// </summary>
    public class RelationshipsOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the block should render.
        /// This is false when no person could be resolved from the page context.
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets the title displayed in the card header. This is the
        /// pluralized relationship group name, or the block name when the
        /// person does not have a relationship group.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current person may add,
        /// edit, and remove relationships.
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether each member's role name is
        /// displayed with their name.
        /// </summary>
        public bool IsRoleNameVisible { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the group type whose roles
        /// are offered as relationship types when adding or editing.
        /// </summary>
        public Guid? GroupTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the owner role. The owner
        /// role is excluded from the relationship type choices.
        /// </summary>
        public Guid? OwnerRoleGuid { get; set; }

        /// <summary>
        /// Gets or sets the warning displayed when the current person is not
        /// allowed to view the relationship group. Null when the group can be
        /// viewed.
        /// </summary>
        public string AccessWarningMessage { get; set; }

        /// <summary>
        /// Gets or sets the relationships to display.
        /// </summary>
        public List<RelationshipBag> Relationships { get; set; }
    }
}
