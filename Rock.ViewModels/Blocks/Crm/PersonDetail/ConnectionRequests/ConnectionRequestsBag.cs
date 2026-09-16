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

namespace Rock.ViewModels.Blocks.Crm.PersonDetail.ConnectionRequests
{
    /// <summary>
    /// The data required to render the Connection Requests block.
    /// </summary>
    public class ConnectionRequestsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the block resolved a person
        /// and should be rendered. When <c>false</c> the block displays nothing.
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets the connection requests to display, ordered by
        /// connection type order, connection type name and then opportunity name.
        /// </summary>
        public List<ConnectionRequestItemBag> ConnectionRequests { get; set; }
    }
}
