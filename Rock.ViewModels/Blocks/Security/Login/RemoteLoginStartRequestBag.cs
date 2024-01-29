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
namespace Rock.ViewModels.Blocks.Security.Login
{
    /// <summary>
    /// A bag that contains the remote login start request information.
    /// </summary>
    public class RemoteLoginStartRequestBag
    {
        /// <summary>
        /// Gets or sets the authentication entity type guid.
        /// </summary>
        public string AuthenticationType { get; set; }

        /// <summary>
        /// Gets or sets the current URL path.
        /// </summary>
        /// <value>
        /// The current URL path.
        /// </value>
        public string Route { get; set; }
    }
}
