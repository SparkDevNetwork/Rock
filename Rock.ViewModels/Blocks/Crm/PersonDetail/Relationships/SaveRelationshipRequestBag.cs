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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Crm.PersonDetail.Relationships
{
    /// <summary>
    /// The information needed to add or update a relationship in the
    /// Relationships block.
    /// </summary>
    public class SaveRelationshipRequestBag
    {
        /// <summary>
        /// Gets or sets the identifier key of the group member record being
        /// updated. Null when adding a new relationship.
        /// </summary>
        public string GroupMemberIdKey { get; set; }

        /// <summary>
        /// Gets or sets the related person. The value is a person alias
        /// unique identifier as emitted by the person picker.
        /// </summary>
        public ListItemBag Person { get; set; }

        /// <summary>
        /// Gets or sets the relationship role. The value is the role's unique
        /// identifier.
        /// </summary>
        public ListItemBag Role { get; set; }
    }
}
