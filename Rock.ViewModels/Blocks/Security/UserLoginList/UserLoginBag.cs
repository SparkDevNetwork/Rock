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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Security.UserLoginList
{
    /// <summary>
    /// Contains details on the User Login.
    /// </summary>
    public class UserLoginBag
    {
        /// <summary>
        /// Gets or sets the identifier key of this entity.
        /// </summary>
        /// <value>
        /// The identifier key of this entity.
        /// </value>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>
        /// The username.
        /// </value>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the authentication provider.
        /// </summary>
        /// <value>
        /// The authentication provider.
        /// </value>
        public ListItemBag AuthenticationProvider { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user account is confirmed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the user account is confirmed; otherwise, <c>false</c>.
        /// </value>
        public bool? IsConfirmed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user locked out.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the user is locked out; otherwise, <c>false</c>.
        /// </value>
        public bool? IsLockedOut { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a password change is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if a is password change is required; otherwise, <c>false</c>.
        /// </value>
        public bool? IsPasswordChangeRequired { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the confirm password.
        /// </summary>
        /// <value>
        /// The confirm password.
        /// </value>
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        public ListItemBag PersonAlias { get; set; }
    }
}
