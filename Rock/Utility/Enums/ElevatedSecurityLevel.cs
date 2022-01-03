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

using Rock.Model;

namespace Rock.Utility.Enums
{
    /// <summary>
    /// Used by the group to determine what <see cref="AccountProtectionProfile"/> a <see cref="Person"/> should assigned.
    /// </summary>
    public enum ElevatedSecurityLevel
    {
        /// <summary>
        /// The group members of this type will get an AccountProtectionProfile of Low.
        /// </summary>
        None = 0,

        /// <summary>
        /// The group members of this type will get an AccountProtectionProfile of High.
        /// </summary>
        High = 1,

        /// <summary>
        /// The group members of this type will get an AccountProtectionProfile of Extreme.
        /// </summary>
        Extreme = 2,
    }
}
