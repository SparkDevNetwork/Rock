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

namespace Rock.ViewModels.Blocks.Connection.ConnectionRequestEntry
{
    /// <summary>
    /// Data returned to the client after a successful Connection Request Entry submission.
    /// </summary>
    public class ConnectionRequestEntryResultBag
    {
        /// <summary>
        /// Gets or sets the URL the client should redirect to when an Optional Redirect URL is configured. When empty, the client shows the built-in success state.
        /// </summary>
        public string RedirectUrl { get; set; }
    }
}
