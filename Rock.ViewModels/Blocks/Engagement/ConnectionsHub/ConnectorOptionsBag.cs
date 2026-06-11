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

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// Represents the detail data for a connection opportunity that is used to populate the request panel when an opportunity is selected.
    /// </summary>
    public class ConnectorOptionsBag
    {
        /// <summary>
        /// Gets or sets the GUID of the default connector's person alias for this opportunity.
        /// </summary>
        public string DefaultConnectorPersonAliasGuid { get; set; }

        /// <summary>
        /// Gets or sets the list of connectors available for assignment within this opportunity.
        /// </summary>
        public List<ListItemBag> Connectors { get; set; }
    }
}
