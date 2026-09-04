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

namespace Rock.ViewModels.Blocks.Tv.RemoteAuthentication
{
    /// <summary>
    /// The initialization options for the Remote Authentication block.
    /// </summary>
    public class RemoteAuthenticationOptionsBag
    {
        /// <summary>
        /// Gets or sets a warning message shown when the person is not authenticated
        /// or another blocking condition is present.
        /// </summary>
        public string WarningMessage { get; set; }

        /// <summary>
        /// Gets or sets the resolved Lava HTML for the form header.
        /// </summary>
        public string HeaderHtml { get; set; }

        /// <summary>
        /// Gets or sets the resolved Lava HTML for the form footer.
        /// </summary>
        public string FooterHtml { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether authentication already succeeded
        /// during block initialization (for example, via an AuthCode page parameter).
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets the resolved Lava HTML shown after a successful authentication.
        /// </summary>
        public string SuccessHtml { get; set; }

        /// <summary>
        /// Gets or sets an error message from an initialization-time authentication
        /// attempt. When set, the form remains available so the person can retry.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
