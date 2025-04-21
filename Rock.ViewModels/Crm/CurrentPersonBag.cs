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

using System;

namespace Rock.ViewModels.Crm
{
    /// <summary>
    /// Represents the currently logged in person.
    /// </summary>
    public class CurrentPersonBag
    {
        /// <summary>
        /// Gets or sets the identifier key of the person.
        /// </summary>
        /// <value>The identifier key of the person.</value>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or set the unique identifier of the person.
        /// </summary>
        /// <value>The unique identifier of the person</value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or set the identifier key of the person's primary alias.
        /// </summary>
        /// <value>The identifier key of the person's primary alias</value>
        public string PrimaryAliasIdKey { get; set; }

        /// <summary>
        /// Gets or set the unique identifier of the person's primary alias.
        /// </summary>
        /// <value>The unique identifier of the person's primary alias</value>
        public Guid PrimaryAliasGuid { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>The first name.</value>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the name of the nick.
        /// </summary>
        /// <value>The name of the nick.</value>
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>The last name.</value>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the full name.
        /// </summary>
        /// <value>The full name.</value>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>The email.</value>
        public string Email { get; set; }
    }
}
