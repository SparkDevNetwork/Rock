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

using Rock.Enums.Connection;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Connection.ConnectionTypeNavigation
{
    /// <summary>
    /// A bag that contains information about a connection type summary for the Connection Type Navigation block.
    /// </summary>
    public class ConnectionTypeSummaryBag : ITranslateIdKey
    {
        /// <inheritdoc />
        public int? Id { get; set; }

        /// <inheritdoc />
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class for this connection type.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the name for this connection type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description for this connection type.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the order for this connection type.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the count of active connection requests for all connection opportunities of this type.
        /// </summary>
        public int ActiveRequestCount { get; set; }

        /// <summary>
        /// Gets or sets the count of "due soon" connection requests for all connection opportunities of this type.
        /// </summary>
        public int DueSoonRequestCount { get; set; }

        /// <summary>
        /// Gets or sets the count of overdue connection requests for all connection opportunities of this type.
        /// </summary>
        public int OverdueRequestCount { get; set; }

        /// <summary>
        /// Gets or sets the count of unassigned connection requests for all connection opportunities of this type.
        /// </summary>
        public int UnassignedRequestCount { get; set; }

        /// <summary>
        /// Gets or sets the count of connection requests assigned to the current person for all connection opportunities of this type.
        /// </summary>
        public int AssignedToYouRequestCount { get; set; }

        /// <summary>
        /// Gets or sets the enabled views for this connection type.
        /// </summary>
        public EnabledViewFlags EnabledViews { get; set; }

        /// <summary>
        /// Gets whether list view is enabled for this connection type.
        /// </summary>
        public bool IsListViewEnabled => EnabledViews.HasFlag( EnabledViewFlags.List );

        /// <summary>
        /// Gets whether board view is enabled for this connection type.
        /// </summary>
        public bool IsBoardViewEnabled => EnabledViews.HasFlag( EnabledViewFlags.Board );

        /// <summary>
        /// Gets whether board view is enabled for this connection type.
        /// </summary>
        public bool IsGridViewEnabled => EnabledViews.HasFlag( EnabledViewFlags.Grid );

        /// <summary>
        /// Gets whether snapshot view is enabled for this connection type.
        /// </summary>
        public bool IsSnapshotViewEnabled => EnabledViews.HasFlag( EnabledViewFlags.Snapshot );
    }
}
