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

using System.ComponentModel;

namespace Rock.Enums.Controls
{
    /// <summary>
    /// Type representing the cache strategy for this item.
    /// </summary>
    public enum RockCacheabilityType
    {
        /// <summary>
        /// Represents the public Cache-Control header.
        /// </summary>
        Public = 0,

        /// <summary>
        /// Represents the private Cache-Control header.
        /// </summary>
        Private = 1,

        /// <summary>
        /// Represents the no-cache Cache-Control header.
        /// </summary>
        [Description( "No-Cache" )]
        NoCache = 2,

        /// <summary>
        /// Represents the no-store Cache-Control header.
        /// </summary>
        [Description( "No-Store" )]
        NoStore = 3
    }
}
