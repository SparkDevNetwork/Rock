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
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Special Class to use when posting/putting a UserLogin record through the Rest API.
    /// The Rest Client can't be given access to the DataEncryptionKey, so they'll upload it (using SSL)
    /// with the PlainTextPassword and the Rock server will encrypt prior to saving to database
    /// </summary>
    [DataContract]
    [NotMapped]
    [RockClientInclude("Use Rock.Client.UserLoginWithPlainTextPassword and set PlainTextPassword to set a new password as part of a api/UserLogins POST or PUT")]
    public class UserLoginWithPlainTextPassword : UserLogin
    {
        /// <summary>
        /// Gets or sets the plain text password.
        /// </summary>
        /// <value>
        /// The plain text password.
        /// </value>
        [DataMember]
        public string PlainTextPassword { get; set; }
    }
}
