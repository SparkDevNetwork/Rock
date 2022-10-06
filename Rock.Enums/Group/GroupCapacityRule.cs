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
    /// Group Capacity Rule
    /// </summary>
    [Enums.EnumDomain( "Group" )]
    public enum GroupCapacityRule
    {
        /// <summary>
        /// The group does not have capacity limitations
        /// </summary>
        None = 0,

        /// <summary>
        /// The group can not go over capacity
        /// </summary>
        Hard = 1,

        /// <summary>
        /// A warning will be shown if a group is going to go over capacity
        /// </summary>
        Soft = 2
    }
}
