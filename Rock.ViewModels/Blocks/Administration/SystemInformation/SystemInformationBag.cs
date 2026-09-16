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

namespace Rock.ViewModels.Blocks.Administration.SystemInformation
{
    /// <summary>
    /// The data shown on the Version Info tab of the System Information block.
    /// </summary>
    public class SystemInformationBag
    {
        /// <summary>
        /// Gets or sets the full Rock product version name (e.g. "Rock McKinley 20.0").
        /// </summary>
        public string RockVersion { get; set; }

        /// <summary>
        /// Gets or sets the numeric Rock product version (e.g. "20.0.3").
        /// </summary>
        public string RockVersionNumber { get; set; }

        /// <summary>
        /// Gets or sets the current client culture setting of the server.
        /// </summary>
        public string ClientCulture { get; set; }
    }
}
