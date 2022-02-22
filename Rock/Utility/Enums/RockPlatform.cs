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

using System;

namespace Rock.Utility
{
    /// <summary>
    /// The front-end platforms that are known and supported by Rock. This enum
    /// allows different types to specify what platforms they have built-in
    /// support for.
    /// </summary>
    /// <seealso cref="Rock.Attribute.RockPlatformSupportAttribute"/>
    [Flags]
    public enum RockPlatform
    {
        #region Web Platform Flags 0x0001 - 0x0080

        /// <summary>
        /// Available and supported on the WebForms web platform.
        /// </summary>
        WebForms = 0x0001,

        /// <summary>
        /// Available and supported on the Obsidian web platform.
        /// </summary>
        Obsidian = 0x0002,

        #endregion
    }
}
