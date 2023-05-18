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

namespace Rock.Enums.Controls
{
    /// <summary>
    /// Requirement level of an input field. Based off Rock.Field.Types.DataEntryRequirementLevelSpecifier
    /// for transfer to/from Obsidian controls.
    /// </summary>
    public enum RequirementLevel
    {
        /// <summary>
        /// No requirement level has been specified for this field.
        /// </summary>
        Unspecified = 0,
        /// <summary>
        /// The field is available but not required.
        /// </summary>
        Optional = 1,
        /// <summary>
        /// The field is available and required.
        /// </summary>
        Required = 2,
        /// <summary>
        /// The field is not available.
        /// </summary>
        Unavailable = 3
    }
}
