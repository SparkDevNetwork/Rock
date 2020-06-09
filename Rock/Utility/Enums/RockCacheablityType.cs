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

namespace Rock.Utility
{
    /// <summary>
    /// The type of cache that a cache-header should be.
    /// </summary>
    public enum RockCacheablityType
    {
        /// <summary>
        /// Represents the public Cache-Control header
        /// </summary>
        Public,
        /// <summary>
        /// Represents the private Cache-Control header
        /// </summary>
        Private,
        /// <summary>
        /// Represents the no-cache Cache-Control header
        /// </summary>
        NoCache,
        /// <summary>
        /// Represents the no-store Cache-Control header
        /// </summary>
        NoStore,
    }
}
