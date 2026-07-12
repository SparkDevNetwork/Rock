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

namespace Rock.ViewModels.Blocks.Mobile.MobilePageDetail
{
    /// <summary>
    /// The response returned by the Mobile Page Detail block's Deploy action.
    /// Carries the refreshed deploy-badge values so the view-mode UI can reflect
    /// the new deploy timestamp without a full block reload.
    /// </summary>
    public class MobilePageDeployResponseBag
    {
        /// <summary>
        /// Gets or sets the friendly deploy text ("Last Deploy: 5 minutes ago").
        /// Null when the application has never been deployed.
        /// </summary>
        public string LastDeployText { get; set; }

        /// <summary>
        /// Gets or sets the long-form, locale-friendly tooltip text shown on
        /// hover (e.g. "Sunday, March 2, 2025 5:42 PM"). Null when the
        /// application has never been deployed.
        /// </summary>
        public string LastDeployTooltip { get; set; }
    }
}
