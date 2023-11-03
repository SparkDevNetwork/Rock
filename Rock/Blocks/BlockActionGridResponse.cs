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

using System;

namespace Rock.Blocks
{
    /// <summary>
    /// Describes the response for a server-side data sourced grid.
    /// </summary>
    [Obsolete( "This is unused and will be removed in a future version of Rock.")]
    [RockObsolete( "1.16" )]
    public class BlockActionGridResponse
    {
        /// <summary>
        /// Gets or sets the total row count.
        /// </summary>
        /// <value>
        /// The page count.
        /// </value>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the current page data.
        /// </summary>
        /// <value>
        /// The current page data.
        /// </value>
        public object CurrentPageData { get; set; }
    }
}
