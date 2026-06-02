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
    /// The payload supplied when reordering a group among its siblings.
    /// </summary>
    public class ReorderGroupRequestBag
    {
        /// <summary>
        /// Gets or sets the hashed identifier of the group being moved.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the hashed identifier of the sibling group the moved group should be placed before, or
        /// <c>null</c> when the moved group should be placed at the end of its sibling list.
        /// </summary>
        public string BeforeIdKey { get; set; }

        /// <summary>
        /// Gets or sets the hashed identifier of the parent node that scopes the sibling list. Resolves to a
        /// group type when <see cref="ParentNodeType"/> is <see cref="CheckInTreeNodeType.Area"/>
        /// or to a group when it is <see cref="CheckInTreeNodeType.Group"/>.
        /// </summary>
        public string ParentIdKey { get; set; }

        /// <summary>
        /// Gets or sets the kind of node that <see cref="ParentIdKey"/> identifies, which determines how the
        /// sibling list is resolved on the server.
        /// </summary>
        public CheckInTreeNodeType ParentNodeType { get; set; }
    }
}
