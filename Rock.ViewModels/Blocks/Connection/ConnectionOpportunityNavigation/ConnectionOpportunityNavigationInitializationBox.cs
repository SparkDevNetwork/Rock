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

using Rock.Enums.Connection;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Connection.ConnectionOpportunityNavigation
{
    /// <summary>
    /// The box that contains all the initialization information for the connection opportunity navigation block.
    /// </summary>
    public class ConnectionOpportunityNavigationInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the list of connection type items the individual may select.
        /// </summary>
        public List<ListItemBag> ConnectionTypeItems { get; set; }

        /// <summary>
        /// Gets or sets the list of opportunity visibility items the individual may select.
        /// </summary>
        public List<ListItemBag> OpportunityVisibilityItems { get; set; }

        /// <summary>
        /// Gets or sets the connection opportunity metrics and summaries.
        /// </summary>
        public ConnectionOpportunityNavigationDetailsBag NavigationDetails { get; set; }
    }
}
