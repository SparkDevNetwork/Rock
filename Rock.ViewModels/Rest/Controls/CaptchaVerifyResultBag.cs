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

using System;

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The results of the CaptchaRedeemChallenge API action of the Captcha control.
    /// </summary>
    public class CaptchaVerifyResultBag
    {
        /// <summary>
        /// Gets or sets whether the challenge is successful or not.
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>
        /// Gets or sets the failure message.
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Gets or sets the token generated after the challenge is successfully completed.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the token expiration time.
        /// </summary>
        public long? Expires { get; set; }
    }
}
