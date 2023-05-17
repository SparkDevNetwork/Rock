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
    /// Enum denoting which merge template to query for.
    /// **NOTE**
    /// This is copied from Rock.Model.MergeTemplateOwnerShip to make accessible to other
    /// areas of the codebase. If you make changes here, also update the original if applicable.
    /// </summary>
    public enum MergeTemplateOwnership
    {
        /// <summary>
        /// Show only global merge templates
        /// </summary>
        Global,

        /// <summary>
        /// Only show personal merge templates
        /// </summary>
        Personal,

        /// <summary>
        /// Show both personal and global merge templates
        /// </summary>
        PersonalAndGlobal
    }
}
