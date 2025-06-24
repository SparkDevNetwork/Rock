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

namespace Rock.Enums.Group
{
    /// <summary>
    /// Represents the different Group Placement modes.
    /// </summary>
    public enum PlacementMode
    {
        /// <summary>
        /// The Group Placement Block is in Template Mode.
        /// </summary>
        TemplateMode = 0,

        /// <summary>
        /// The Group Placement Block is in Instance Mode.
        /// </summary>
        InstanceMode = 1,

        /// <summary>
        /// The Group Placement Block is in Group Mode.
        /// </summary>
        GroupMode = 2,

        /// <summary>
        /// The Group Placement Block is in Entity Set Mode.
        /// </summary>
        EntitySetMode = 3
    }
}
