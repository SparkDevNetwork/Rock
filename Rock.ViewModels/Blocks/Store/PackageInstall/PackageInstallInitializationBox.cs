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

namespace Rock.ViewModels.Blocks.Store.PackageInstall
{
    /// <summary>
    /// The initial configuration sent to the Package Install block when it
    /// first renders.
    /// </summary>
    public class PackageInstallInitializationBox
    {
        /// <summary>
        /// Gets or sets a value indicating whether the store has an
        /// organization key configured. When <c>false</c> the block redirects
        /// to <see cref="LinkOrganizationPageUrl"/> so the user can configure
        /// the store before installing.
        /// </summary>
        public bool IsStoreConfigured { get; set; }

        /// <summary>
        /// Gets or sets the URL to navigate to when the store is not yet
        /// configured. Carries a ReturnUrl back to this page so the user lands
        /// here again after linking their organization.
        /// </summary>
        public string LinkOrganizationPageUrl { get; set; }

        /// <summary>
        /// Gets or sets the error detail to display when the store is currently
        /// unavailable. When set, the block shows the unavailable notice
        /// instead of the install form.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the name of the package being installed.
        /// </summary>
        public string PackageName { get; set; }

        /// <summary>
        /// Gets or sets the URL of the package icon image.
        /// </summary>
        public string PackageIconImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the HTML message shown above the login form describing
        /// what installing this package will do (free install, purchase,
        /// upgrade, or previously purchased).
        /// </summary>
        public string InstallMessage { get; set; }

        /// <summary>
        /// Gets or sets the label for the install button ("Install" for a new
        /// install or "Update" when a prior version is already installed).
        /// </summary>
        public string InstallButtonText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the installed version is
        /// already the latest version compatible with this version of Rock. When
        /// <c>true</c> the block hides the install form and shows an "up to date"
        /// notice instead, since there is nothing to install or update.
        /// </summary>
        public bool IsUpToDate { get; set; }
    }
}
