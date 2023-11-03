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

namespace Rock.ViewModels.Core.Grid
{
    /// <summary>
    /// Identifies the information about a communication that should be created
    /// for use by a Grid.
    /// </summary>
    public class GridCommunicationBag
    {
        /// <summary>
        /// Gets or sets the recipients.
        /// </summary>
        /// <value>The recipients.</value>
        public List<GridEntitySetItemBag> Recipients { get; set; }

        /// <summary>
        /// Gets or sets the merge field keys that are available for use.
        /// </summary>
        /// <value>The merge field keys that are available for use.</value>
        public List<string> MergeFields { get; set; }

        /// <summary>
        /// Gets or sets the URL of the page this request came from.
        /// </summary>
        /// <value>The URL of the page this request came from.</value>
        public string FromUrl { get; set; }
    }
}
