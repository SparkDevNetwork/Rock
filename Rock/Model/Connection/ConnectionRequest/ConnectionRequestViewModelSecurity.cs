﻿// <copyright>
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

namespace Rock.Model
{
    /// <summary>
    /// This model is used to include the ConnectionRequest and ConnectionType so security can be checked if needed.
    /// </summary>
    /// <seealso cref="Rock.Model.ConnectionRequestViewModel" />
    internal class ConnectionRequestViewModelSecurity : ConnectionRequestViewModel
    {
        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ConnectionRequest"/>.
        /// </summary>
        /// <value>
        /// The connection request.
        /// </value>
        public ConnectionRequest ConnectionRequest { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ConnectionType"/>.
        /// </summary>
        /// <value>
        /// The connection type.
        /// </value>
        public ConnectionType ConnectionType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether user has direct access.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [user has direct access]; otherwise, <c>false</c>.
        /// </value>
        public bool UserHasDirectAccess { get; set; }
    }
}
