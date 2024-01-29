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

namespace Rock.Enums.Blocks.Group.Scheduling
{
    /// <summary>
    /// The action types that can be performed within the group schedule toolbox.
    /// </summary>
    public enum ToolboxActionType
    {
        /// <summary>
        /// View the person's current schedule rows, including attendances and/or
        /// person schedule exclusions.
        /// </summary>
        ViewCurrent = 0,

        /// <summary>
        /// Update the person's schedule preferences.
        /// </summary>
        UpdatePreferences = 1,

        /// <summary>
        /// Sign the person up for additional occurrences.
        /// </summary>
        SignUp = 2
    }
}
