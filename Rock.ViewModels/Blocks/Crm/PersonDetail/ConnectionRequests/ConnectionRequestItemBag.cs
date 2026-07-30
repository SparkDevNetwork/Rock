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

namespace Rock.ViewModels.Blocks.Crm.PersonDetail.ConnectionRequests
{
    /// <summary>
    /// A single connection request row displayed by the Connection Requests block.
    /// </summary>
    public class ConnectionRequestItemBag
    {
        /// <summary>
        /// Gets or sets the name of the connection type the request belongs to.
        /// Rows are grouped under a sub-header with this name.
        /// </summary>
        public string ConnectionTypeName { get; set; }

        /// <summary>
        /// Gets or sets the display name of the request, which is the opportunity
        /// name followed by the campus name in parentheses when a campus is set.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the status text displayed for the request.
        /// </summary>
        public string StatusText { get; set; }

        /// <summary>
        /// Gets or sets the URL of the detail page for the request, or
        /// <c>null</c> when no detail page is configured.
        /// </summary>
        public string DetailUrl { get; set; }
    }
}
