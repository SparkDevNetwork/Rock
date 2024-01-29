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
namespace Rock.ViewModels.Blocks.Security.Login
{
    /// <summary>
    /// A bag that contains the passwordless login start request information.
    /// </summary>
    public class PasswordlessLoginStartRequestBag
    {
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>The email.</value>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        /// <value>The phone number.</value>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to send email link.
        /// </summary>
        /// <value><c>true</c> to send email link; otherwise, <c>false</c>.</value>
        public bool ShouldSendEmailLink { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to send email code.
        /// </summary>
        /// <value><c>true</c> to send email code; otherwise, <c>false</c>.</value>
        public bool ShouldSendEmailCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to send SMS code.
        /// </summary>
        /// <value><c>true</c> to send SMS code; otherwise, <c>false</c>.</value>
        public bool ShouldSendSmsCode { get; set; }

        /// <summary>
        /// Gets or sets the matching person value.
        /// </summary>
        /// <value>
        /// The matching person value.
        /// </value>
        public string MatchingPersonValue { get; set; }

        /// <summary>
        /// Gets or sets the MFA details.
        /// </summary>
        /// <value>
        /// The MFA details.
        /// </value>
        public string MfaTicket { get; set; }
    }
}
