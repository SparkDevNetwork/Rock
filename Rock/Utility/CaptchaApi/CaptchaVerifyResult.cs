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

namespace Rock.Utility.CaptchaApi
{
    /// <summary>
    /// Represents the result of a CAPTCHA verification process.
    /// </summary>
    internal sealed class CaptchaVerifyResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the captcha has been verified.
        /// </summary>
        /// <value>
        /// If <see langword="true"/>, verification was successful and the <see cref="Token"/> and <see cref="Expires"/> will be set;
        /// otherwise, the <see cref="Error"/> will be set.
        /// </value>
        internal bool IsVerified { get; set; }

        /// <summary>
        /// Gets or sets the error message associated if the captcha was not verified.
        /// </summary>
        internal string Error { get; set; }

        /// <summary>
        /// Gets or sets the captcha token if the catpcha was verified.
        /// </summary>
        internal string Token { get; set; }

        /// <summary>
        /// Gets the expiration time of the captcha token in Unix time milliseconds.
        /// </summary>
        internal long? Expires
        {
            get
            {
                if ( TokenIssueDate.HasValue && TokenLifetime.HasValue )
                {
                    var expiration = new DateTimeOffset( TokenIssueDate.Value.Add( TokenLifetime.Value ) );
                    return expiration.ToUnixTimeMilliseconds();
                }

                return null;
            }
        }

        /// <summary>
        /// Gets or sets the token issue date.
        /// </summary>
        internal DateTime? TokenIssueDate { get; set; }

        /// <summary>
        /// Gets or sets the token lifetime.
        /// </summary>
        internal TimeSpan? TokenLifetime { get; set; }

        /// <summary>
        /// Creates a <see cref="CaptchaVerifyResult"/> indicating a successful verification.
        /// </summary>
        /// <param name="token">The token associated with the captcha verification.</param>
        /// <param name="issueDate">The date and time when the token was issued.</param>
        /// <param name="lifetime">The lifetime of the token.</param>
        internal static CaptchaVerifyResult Pass( string token, DateTime issueDate, TimeSpan lifetime )
        {
            return new CaptchaVerifyResult
            {
                IsVerified = true,
                Token = token,
                TokenIssueDate = issueDate,
                TokenLifetime = lifetime
            };
        }

        /// <summary>
        /// Creates a <see cref="CaptchaVerifyResult"/> indicating a failed verification.
        /// </summary>
        /// <param name="errorMessage">The error message describing the reason for the failure. Cannot be null.</param>
        internal static CaptchaVerifyResult Fail( string errorMessage )
        {
            return new CaptchaVerifyResult
            {
                IsVerified = false,
                Error = errorMessage
            };
        }
    }
}
