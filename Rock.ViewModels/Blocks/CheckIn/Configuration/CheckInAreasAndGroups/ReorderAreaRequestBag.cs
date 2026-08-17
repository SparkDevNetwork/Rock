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

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInAreasAndGroups
{
    /// <summary>
    /// The payload supplied when reordering an area among its siblings.
    /// </summary>
    public class ReorderAreaRequestBag
    {
        /// <summary>
        /// Gets or sets the hashed identifier of the area being moved.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the hashed identifier of the sibling area the moved area should be placed before, or
        /// <c>null</c> when the moved area should be placed at the end of its sibling list.
        /// </summary>
        public string BeforeIdKey { get; set; }

        /// <summary>
        /// Gets or sets the hashed identifier of the parent area that scopes the sibling list, or <c>null</c>
        /// when the moved area is a top-level child of the configuration.
        /// </summary>
        public string ParentAreaIdKey { get; set; }
    }
}
