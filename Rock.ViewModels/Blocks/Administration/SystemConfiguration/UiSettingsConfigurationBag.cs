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

namespace Rock.ViewModels.Blocks.Administration.SystemConfiguration
{
    /// <summary>
    /// Contains the Ui Settings configuration details.
    /// </summary>
    public class UiSettingsConfigurationBag
    {
        /// <summary>
        /// Gets or sets the race label.
        /// </summary>
        /// <value>
        /// The race label.
        /// </value>
        public string RaceLabel { get; set; }

        /// <summary>
        /// Gets or sets the ethnicity label.
        /// </summary>
        /// <value>
        /// The ethnicity label.
        /// </value>
        public string EthnicityLabel { get; set; }

        /// <summary>
        /// Gets or sets the captcha site key.
        /// </summary>
        /// <value>
        /// The captcha site key.
        /// </value>
        public string CaptchaSiteKey { get; set; }

        /// <summary>
        /// Gets or sets the captcha secret key.
        /// </summary>
        /// <value>
        /// The captcha secret key.
        /// </value>
        public string CaptchaSecretKey { get; set; }

        /// <summary>
        /// Gets or sets the SMS opt in message.
        /// </summary>
        /// <value>
        /// The SMS opt in message.
        /// </value>
        public string SmsOptInMessage { get; set; }
    }
}
