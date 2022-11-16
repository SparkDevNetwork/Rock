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
    /// Authorization for a special group of users not defined by a specific role or person
    /// </summary>
    [Enums.EnumDomain( "Core" )]
    public enum SpecialRole
    {
        /// <summary>
        /// No special role
        /// </summary>
        None = 0,

        /// <summary>
        /// Authorize all users
        /// </summary>
        AllUsers = 1,

        /// <summary>
        /// Authorize all authenticated users
        /// </summary>
        AllAuthenticatedUsers = 2,

        /// <summary>
        /// Authorize all un-authenticated users
        /// </summary>
        AllUnAuthenticatedUsers = 3,
    }
}
