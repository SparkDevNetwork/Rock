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

namespace Rock.ViewModels.Blocks.Security.ChangePassword
{
    /// <summary>
    /// A box containing the required information to render the Change Password block.
    /// </summary>
    public class ChangePasswordBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the change password form should be shown.
        /// </summary>
        public bool IsChangePasswordVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether captcha verification should be disabled.
        /// </summary>
        public bool DisableCaptchaSupport { get; set; }

        /// <summary>
        /// The optional message to display on first render.
        /// </summary>
        public string AlertMessage { get; set; }

        /// <summary>
        /// The alert type for <see cref="AlertMessage"/>.
        /// </summary>
        public string AlertType { get; set; }
    }
}
