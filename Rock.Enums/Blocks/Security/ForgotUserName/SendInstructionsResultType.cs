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

namespace Rock.Enums.Blocks.Security.ForgotUserName
{
    /// <summary>
    /// The send instructions result type for the Forgot UserName block.
    /// </summary>
    public enum SendInstructionsResultType
    {
        /// <summary>
        /// Indicates that the instructions were sent successfully.
        /// </summary>
        InstructionsSent = 0,

        /// <summary>
        /// Indicates that the requested email is invalid.
        /// </summary>
        EmailInvalid = 1,

        /// <summary>
        /// Indicates that change password is not supported for account(s) associated with requested email.
        /// </summary>
        ChangePasswordNotSupported = 2,

        /// <summary>
        /// Indicates that the captcha was not solved successfully.
        /// </summary>
        [Obsolete( "No longer used and will be removed in the future." )]
        CaptchaInvalid = 3,
    }
}
