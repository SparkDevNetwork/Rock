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
using System.Security.Cryptography;

namespace Rock.Oidc.Configuration
{
    internal class SerializedRockRsaKey
    {
        /// <summary>
        /// Gets or sets the key identifier.
        /// </summary>
        /// <value>
        /// The key identifier.
        /// </value>
        public string KeyId { get; set; } = System.Guid.NewGuid().ToString();
        /// <summary>
        /// Gets or sets the key created date.
        /// </summary>
        /// <value>
        /// The key created date.
        /// </value>
        public DateTime KeyCreatedDate { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public RSAParameters Parameters { get; set; }
    }
}
