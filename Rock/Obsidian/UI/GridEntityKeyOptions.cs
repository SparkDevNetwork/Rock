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

namespace Rock.Obsidian.UI
{
    /// <summary>
    /// Describes which values in a list block's grid rows identify the records
    /// that the grid entity set and communication actions may use.
    /// </summary>
    public class GridEntityKeyOptions
    {
        /// <summary>
        /// Gets or sets the identifier of the entity type that the row keys
        /// identify. When <c>null</c>, the row keys are not tied to an entity
        /// type and are accepted for every entity type, including as people.
        /// </summary>
        /// <value>The entity type identifier, or <c>null</c> if the rows do not identify one entity type.</value>
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the dot separated paths of the row values that hold the
        /// row keys. When <c>null</c>, the <c>idKey</c>, <c>id</c> and <c>guid</c>
        /// values are used if <see cref="EntityTypeId"/> is set, and every key
        /// property in the row is used if it is not.
        /// </summary>
        /// <value>The paths of the row key values.</value>
        public IEnumerable<string> RowKeyFieldPaths { get; set; }

        /// <summary>
        /// Gets or sets the dot separated paths of the row values that hold
        /// person keys. Person fields in the row are always included, so this
        /// is only needed for person keys stored as plain values.
        /// </summary>
        /// <value>The paths of the person key values.</value>
        public IEnumerable<string> PersonKeyFieldPaths { get; set; }
    }
}
