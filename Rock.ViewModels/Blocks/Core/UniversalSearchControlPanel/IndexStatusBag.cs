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

namespace Rock.ViewModels.Blocks.Core.UniversalSearchControlPanel
{
    /// <summary>
    /// Contains the status information for the active universal search index component.
    /// </summary>
    public class IndexStatusBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether a search index component is active.
        /// </summary>
        public bool IsSearchEnabled { get; set; }

        /// <summary>
        /// Gets or sets the friendly name of the active index component.
        /// </summary>
        public string ComponentName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the active component is connected to its server.
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// Gets or sets the URL or location of the search index server.
        /// </summary>
        public string IndexLocation { get; set; }

        /// <summary>
        /// Gets or sets a warning message to display when the component is not connected
        /// or no component is enabled. Null if there are no warnings.
        /// </summary>
        public string WarningMessage { get; set; }
    }
}
