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

namespace Rock.ViewModels.Blocks.Connection.ConnectionTypeNavigation
{
    /// <summary>
    /// The box that contains all the initialization information for the connection type navigation block.
    /// </summary>
    public class ConnectionTypeNavigationInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the list of connection type visibility items the individual may select.
        /// </summary>
        public List<ListItemBag> TypeVisibilityItems { get; set; }

        /// <summary>
        /// Gets or sets whether to show the configure connection types button.
        /// </summary>
        public bool ShowConfigureConnectionTypesButton { get; set; }

        /// <summary>
        /// Gets or sets the list of connection type summaries to display.
        /// </summary>
        public List<ConnectionTypeSummaryBag> ConnectionTypeSummaries { get; set; }
    }
}
