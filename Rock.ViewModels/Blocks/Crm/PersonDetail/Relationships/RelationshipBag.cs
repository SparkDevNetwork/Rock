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
    /// A single relationship displayed by the Relationships block.
    /// </summary>
    public class RelationshipBag
    {
        /// <summary>
        /// Gets or sets the identifier key of the group member record that
        /// represents this relationship.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the related person. The value is the person's primary
        /// alias unique identifier and the text is their full name, so it can
        /// be fed directly to a person picker.
        /// </summary>
        public ListItemBag Person { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the related person. This is the raw
        /// integer identifier because the PersonLink control requires it to
        /// build the profile link and hover card.
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the name of the relationship role.
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// Gets or sets the relationship role. The value is the role's unique
        /// identifier and the text is its name, so it can be fed directly to a
        /// group role picker.
        /// </summary>
        public ListItemBag Role { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the related person is
        /// deceased.
        /// </summary>
        public bool IsDeceased { get; set; }
    }
}
