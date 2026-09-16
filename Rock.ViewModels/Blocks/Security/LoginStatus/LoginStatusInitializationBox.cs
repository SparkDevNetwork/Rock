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

namespace Rock.ViewModels.Blocks.Security.LoginStatus
{
    /// <summary>
    /// The bag containing the data required to render the Login Status block.
    /// </summary>
    public class LoginStatusInitializationBox
    {
        /// <summary>
        /// Gets or sets the current person's nickname.
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the URL for the current person's profile photo.
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user is logged in.
        /// </summary>
        public bool IsLoggedIn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the block is in minimal mode
        /// (profile photo only, no name or caret).
        /// </summary>
        public bool IsMinimalMode { get; set; }

        /// <summary>
        /// Gets or sets the URL for the My Account page, or null if not configured.
        /// </summary>
        public string MyAccountUrl { get; set; }

        /// <summary>
        /// Gets or sets the URL for the My Profile page, or null if not configured.
        /// </summary>
        public string MyProfileUrl { get; set; }

        /// <summary>
        /// Gets or sets the URL for the My Settings page, or null if not configured.
        /// </summary>
        public string MySettingsUrl { get; set; }

        /// <summary>
        /// Gets or sets the URL for the site's login page, including a return URL parameter.
        /// </summary>
        public string LoginPageUrl { get; set; }

        /// <summary>
        /// Gets or sets the list of custom navigation page items configured via
        /// the Logged In Page List block setting.
        /// </summary>
        public List<LoginStatusNavPageBag> CustomNavPages { get; set; }
    }
}
