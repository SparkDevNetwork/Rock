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

namespace Rock.Enums.Cms
{
    /// <summary>
    /// The captcha mode.
    /// </summary>
    public enum CaptchaMode
    {
        /// <summary>
        /// CAPTCHA should be visible and requires manual interaction.
        /// </summary>
        Visible = 0,

        /// <summary>
        /// CAPTCHA should be invisible and run automatically.
        /// </summary>
        Invisible = 1,

        /// <summary>
        /// CAPTCHA is disabled.
        /// </summary>
        Disabled = 2
    }
}
