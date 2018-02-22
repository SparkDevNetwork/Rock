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

namespace Rock.Security.Authentication.Auth0
{
    /// <summary>
    /// see https://auth0.com/docs/user-profile/normalized/oidc 
    /// </summary>
    public class Auth0UserInfo
    {
        /// <summary>
        /// unique identifier for the user
        /// </summary>
        /// <value>
        /// The sub.
        /// </value>
        public string sub { get; set; }

        /// <summary>
        /// name of the user
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string name { get; set; }

        /// <summary>
        /// the first/given name of the user
        /// </summary>
        /// <value>
        /// The name of the given.
        /// </value>
        public string given_name { get; set; }

        /// <summary>
        /// the surname/last name of the use
        /// </summary>
        /// <value>
        /// The name of the family.
        /// </value>
        public string family_name { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        public string email { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [email verified].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [email verified]; otherwise, <c>false</c>.
        /// </value>
        public bool email_verified { get; set; }

        /// <summary>
        /// casual name of the user that may/may not be the same as the given_name
        /// </summary>
        /// <value>
        /// The nickname.
        /// </value>
        public string nickname { get; set; }

        /// <summary>
        /// URL of the user's profile picture
        /// </summary>
        /// <value>
        /// The picture.
        /// </value>
        public string picture { get; set; }

        /// <summary>
        /// gender of the user
        /// </summary>
        /// <value>
        /// The gender.
        /// </value>
        public string gender { get; set; }

        /// <summary>
        /// location where the user is located
        /// </summary>
        /// <value>
        /// The locale.
        /// </value>
        public string locale { get; set; }

        /// <summary>
        /// time when the user's profile was last updated
        /// </summary>
        /// <value>
        /// The updated at.
        /// </value>
        public DateTime updated_at { get; set; }
    }
}
