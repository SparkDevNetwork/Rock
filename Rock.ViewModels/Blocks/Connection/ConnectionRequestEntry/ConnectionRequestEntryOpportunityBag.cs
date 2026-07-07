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

namespace Rock.ViewModels.Blocks.Connection.ConnectionRequestEntry
{
    /// <summary>
    /// A single connection opportunity offered for selection on the Connection Request Entry form.
    /// </summary>
    public class ConnectionRequestEntryOpportunityBag
    {
        /// <summary>
        /// Gets or sets the opportunity identifier key.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the displayed opportunity name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the displayed opportunity description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the displayed icon CSS class.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the name of the connection type the opportunity belongs to.
        /// </summary>
        public string ConnectionTypeName { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class of the connection type the opportunity belongs to.
        /// </summary>
        public string ConnectionTypeIconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the public connection request attributes revealed when the opportunity is selected.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> Attributes { get; set; }
    }
}
