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

using Rock.Enums.Crm;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Crm.BulkUpdate
{
    /// <summary>
    /// Represents a tag update operation in a bulk update save request.
    /// </summary>
    public class BulkUpdateTagBag
    {
        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        public BulkUpdateActionSpecifier Action { get; set; }

        /// <summary>
        /// Gets or sets the tag. The <see cref="ListItemBag.Value"/> carries the tag
        /// unique identifier.
        /// </summary>
        public ListItemBag Tag { get; set; }
    }
}
