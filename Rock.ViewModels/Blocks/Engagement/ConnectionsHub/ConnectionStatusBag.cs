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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// Represents a connection status and its display properties for use in the Connections Hub.
    /// </summary>
    public class ConnectionStatusBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of this connection status.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the sort order of this connection status relative to others in the same Connection Type.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the display name of this connection status.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the highlight color used to visually distinguish this status in the UI.
        /// </summary>
        public string HighlightColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a note is required when completing a request with this status.
        /// </summary>
        public bool IsNoteRequiredOnCompletion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this status is the default status.
        /// </summary>
        public bool IsDefaultStatus { get; set; }

        /// <summary>
        /// Gets or sets whether this status is disabled. (This is used when editing a request in sequential status mode)
        /// </summary>
        public bool Disabled { get; set; }
    }
}
