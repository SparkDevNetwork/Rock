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

using Rock.Enums.CheckIn;

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInAreasAndGroups
{
    /// <summary>
    /// A single node in the check-in areas-and-groups tree. The tree is a flattened blend of two underlying
    /// hierarchies: child areas (group types) and the groups that belong to those areas.
    /// </summary>
    public class CheckInTreeNodeBag
    {
        /// <summary>
        /// Gets or sets the kind of node this entry represents.
        /// </summary>
        public CheckInTreeNodeType NodeType { get; set; }

        /// <summary>
        /// Gets or sets the hashed identifier of the underlying entity, suitable for use in client-side
        /// requests back to the server.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the display name of the node.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the node is active. Inactive nodes render at reduced opacity
        /// and are filtered out unless the "Show Inactive" preference is enabled.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the node is a system entity that should not be deleted.
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the sort order assigned to the underlying entity.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the child nodes nested under this node.
        /// </summary>
        public List<CheckInTreeNodeBag> Children { get; set; }
    }
}
