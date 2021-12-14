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

namespace Rock.Security.Options
{
    /// <summary>
    /// A set of options that specify the requirements of a password when generating
    /// or verifying passwords.
    /// </summary>
    internal class PasswordRequirementOptions
    {
        /// <summary>
        /// Gets or sets the minimum length the password must be.
        /// </summary>
        /// <value>The minimum length the password must be.</value>
        public int MinimumLength { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a non-alpha numeric character is required.
        /// </summary>
        /// <value><c>true</c> if a non-alpha numeric character is required; otherwise, <c>false</c>.</value>
        public bool RequireNonAlphanumeric { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a digit character is required.
        /// </summary>
        /// <value><c>true</c> if a digit character is required; otherwise, <c>false</c>.</value>
        public bool RequireDigit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a lower case character is required.
        /// </summary>
        /// <value><c>true</c> if a lower case character is required; otherwise, <c>false</c>.</value>
        public bool RequireLowercase { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an upper case character is required.
        /// </summary>
        /// <value><c>true</c> if an upper case character is required; otherwise, <c>false</c>.</value>
        public bool RequireUppercase { get; set; }
    }
}
