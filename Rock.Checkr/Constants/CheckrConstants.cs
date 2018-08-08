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
namespace Rock.Checkr.Constants
{
    /// <summary>
    /// This class holds Checkr settings.
    /// </summary>
    public static class CheckrConstants
    {

        /// <summary>
        /// The URL where the token for the account is retrieved
        /// </summary>
        public const string CHECKR_TOKEN_URL = "oauth/tokens";

        /// <summary>
        /// The typename prefix
        /// </summary>
        public const string CHECKR_TYPENAME_PREFIX = "Checkr - ";

        /// <summary>
        /// The login URL
        /// </summary>
        public const string CHECKR_APISERVER = "https://api.checkr.com";

        /// <summary>
        /// The candidates URL
        /// </summary>
        public const string CHECKR_CANDIDATES_URL = "v1/candidates";

        /// <summary>
        /// The invitations URL
        /// </summary>
        public const string CHECKR_INVITATIONS_URL = "v1/invitations";

        /// <summary>
        /// The report URL
        /// </summary>
        public const string CHECKR_REPORT_URL = "v1/reports";

        /// <summary>
        /// The packages URL
        /// </summary>
        public const string CHECKR_PACKAGES_URL = "v1/packages";

        /// <summary>
        /// The document URL
        /// </summary>
        public const string CHECKR_DOCUMENT_URL = "v1/documents";

        /// <summary>
        /// The default checkr workflow type name
        /// </summary>
        public const string CHECKR_WORKFLOW_TYPE_NAME = "Background Check (Checkr)";
    }
}