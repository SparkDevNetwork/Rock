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
using System.Collections.Generic;

namespace Rock.Common.Tv
{
    /// <summary>
    /// POCO for TV Interaction Session
    /// </summary>
    public class TvInteractionSession
    {
        /// <summary>
        /// Gets or sets the GUID.
        /// </summary>
        /// <value>The GUID.</value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the type of the client.
        /// </summary>
        /// <value>The type of the client.</value>
        public string ClientType { get; set; } = "TV";

        /// <summary>
        /// Gets or sets the operating system.
        /// </summary>
        /// <value>The operating system.</value>
        public string OperatingSystem { get; set; }

        /// <summary>
        /// Gets or sets the application.
        /// </summary>
        /// <value>The application.</value>
        public string Application { get; set; }

        /// <summary>
        /// Gets or sets the interactions.
        /// </summary>
        /// <value>The interactions.</value>
        public List<TvInteraction> Interactions { get; set; } = new List<TvInteraction>();
    }
}
