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
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Security
{
    /// <summary>
    /// 
    /// </summary>
    [RockClientInclude("Use this as the Content of a api/Auth/Login POST")]
    public class LoginParameters
    {
        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>
        /// The username.
        /// </value>
        [DataMember]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        [DataMember]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LoginParameters"/> information is going to be persisted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if persisted; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool Persisted { get; set; }
    }
}
