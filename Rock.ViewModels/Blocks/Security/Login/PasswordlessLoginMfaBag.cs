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
    /// A bag that contains the passwordless login MFA information.
    /// </summary>
    public class PasswordlessLoginMfaBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether email and mobile phone are missing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if email and mobile phone are missing; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmailAndMobilePhoneMissing { get; set; }

        /// <summary>
        /// Gets a value indicating whether there is an error.
        /// </summary>
        /// <value>
        ///   <c>true</c> if there is an error; otherwise, <c>false</c>.
        /// </value>
        public bool IsError => IsEmailAndMobilePhoneMissing;

        /// <summary>
        /// Gets or sets the MFA ticket.
        /// </summary>
        /// <value>
        /// The MFA ticket.
        /// </value>
        public string Ticket { get; set; }
    }
}
