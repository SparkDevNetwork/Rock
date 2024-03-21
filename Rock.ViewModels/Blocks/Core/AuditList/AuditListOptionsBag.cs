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
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Core.AuditList
{
    /// <summary>
    /// The additional configuration options for the Audit List block.
    /// </summary>
    public class AuditListOptionsBag
    {
        /// <summary>
        /// Gets or sets available entity types for the filter.
        /// </summary>
        /// <value>
        /// The entity type items.
        /// </value>
        public List<ListItemBag> EntityTypeItems { get; set; }
    }
}
