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

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// Represents the combined payload returned when opening a connection request detail panel, containing the request data and its configuration options.
    /// </summary>
    public class ConnectionRequestDetailBox
    {
        /// <summary>
        /// Gets or sets the entity data for the connection request being viewed or edited.
        /// </summary>
        public ConnectionRequestDetailsBag Entity { get; set; }

        /// <summary>
        /// Gets or sets the configuration options used to render the connection request detail panel.
        /// </summary>
        public ConnectionRequestDetailOptionsBag Options { get; set; }
    }
}
