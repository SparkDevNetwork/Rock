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

using Rock.Enums.CheckIn;

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInAreasAndGroups
{
    /// <summary>
    /// The request payload for saving a new or existing check-in group. The server treats
    /// <see cref="GroupDetailBag.IdKey"/> as the create-vs-update discriminator: empty/null means create a new
    /// group under the parent identified by <see cref="ParentIdKey"/> and <see cref="ParentNodeType"/>.
    /// </summary>
    public class SaveGroupRequestBag
    {
        /// <summary>
        /// Gets or sets the editable group detail.
        /// </summary>
        public GroupDetailBag Group { get; set; }

        /// <summary>
        /// Gets or sets the hashed identifier of the parent node under which to create this group when
        /// <see cref="GroupDetailBag.IdKey"/> is empty/null. The parent may be an area (<c>GroupType</c>) or
        /// another group; see <see cref="ParentNodeType"/>. Ignored on update.
        /// </summary>
        public string ParentIdKey { get; set; }

        /// <summary>
        /// Gets or sets the parent node's type, disambiguating the <see cref="ParentIdKey"/> hashed identifier
        /// between a <c>GroupType</c> (area) and a <c>Group</c>. Ignored on update.
        /// </summary>
        public CheckInTreeNodeType ParentNodeType { get; set; }
    }
}
