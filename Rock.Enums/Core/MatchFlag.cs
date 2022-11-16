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

namespace Rock.Model
{
    /// <summary>
    /// Represents the matching flag.
    /// </summary>
    [Enums.EnumDomain( "Core" )]
    public enum MatchFlag
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// Moved
        /// </summary>
        Moved = 1,

        /// <summary>
        /// PO Box Closed
        /// </summary>
        POBoxClosed = 2,

        /// <summary>
        /// Moved left no forwarding
        /// </summary>
        MovedNoForwarding = 3,

        /// <summary>
        /// Moved to foreign country
        /// </summary>
        MovedToForeignCountry = 4
    }
}
