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

namespace Rock.ViewModels.Blocks.Security.BackgroundCheck.CheckrSettings
{
    /// <summary>
    /// The initialization options for the Checkr Settings block.
    /// </summary>
    public class CheckrSettingsOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether an access token has been configured.
        /// </summary>
        public bool IsConfigured { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Checkr is the current default background check provider.
        /// </summary>
        public bool IsDefaultProvider { get; set; }

        /// <summary>
        /// Gets or sets the decrypted access token for display.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the list of active Checkr package names (with the type prefix stripped).
        /// </summary>
        public List<string> Packages { get; set; }

        /// <summary>
        /// Gets or sets the organization name used to construct the Checkr signup URL.
        /// </summary>
        public string OrganizationName { get; set; }

        /// <summary>
        /// Gets or sets an optional warning message to display after a successful action
        /// that encountered non-fatal errors (e.g., package sync failure).
        /// </summary>
        public string WarningMessage { get; set; }
    }
}
