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

namespace Rock.ViewModels.Blocks.Connection.CelebrationsReport
{
    /// <summary>
    /// The additional configuration options for the Celebrations Report block.
    /// </summary>
    public class CelebrationsReportOptionsBag
    {
        /// <summary>
        /// Gets or sets the available connection types for the filter dropdown.
        /// Only connection types with celebrations enabled are included.
        /// </summary>
        public List<ListItemBag> ConnectionTypes { get; set; }

        /// <summary>
        /// Gets or sets the connection type that should be pre-selected on load,
        /// populated when the ConnectionTypeId page parameter is present.
        /// Null when no page parameter is provided.
        /// </summary>
        public ListItemBag InitialConnectionType { get; set; }

        /// <summary>
        /// Gets or sets the name of the connection type specified via the
        /// <c>ConnectionTypeId</c> page parameter. When non-null, the block
        /// title is set to "{Name} Celebrations" and the connection type
        /// filter in the grid settings is hidden.
        /// </summary>
        public string PageConnectionTypeName { get; set; }
    }
}
