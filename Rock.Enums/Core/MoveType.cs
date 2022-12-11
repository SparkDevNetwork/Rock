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
    /// Represents the move type for NCOA.
    /// </summary>
    [Enums.EnumDomain( "Core" )]
    public enum MoveType
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// Family
        /// </summary>
        Family = 1,

        /// <summary>
        /// Individual
        /// </summary>
        Individual = 2,

        /// <summary>
        /// Business
        /// </summary>
        Business = 3
    }
}
