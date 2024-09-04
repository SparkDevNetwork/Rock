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

namespace Rock.ViewModels.Blocks.Security.ForgotUserName
{
    /// <summary>
    /// A bag containing the required information to render a Forgot UserName block.
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Blocks.BlockBox" />
    public class ForgotUserNameInitializationBox : BlockBox
    {
        /// <summary>
        /// A bag containing the required information to display various block captions.
        /// </summary>
        public ForgotUserNameCaptionsBag Captions { get; set; }

        /// <summary>
        /// If set to true if the Captcha verification step should not be performed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if Captcha is disabled; otherwise, <c>false</c>.
        /// </value>
        public bool DisableCaptchaSupport { get; set; }
    }
}
