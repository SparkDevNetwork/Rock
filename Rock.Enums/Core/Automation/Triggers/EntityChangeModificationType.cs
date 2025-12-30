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

namespace Rock.Enums.Core.Automation.Triggers
{
    /// <summary>
    /// The type of modification that occurred to the entity.
    /// </summary>
    [Flags]
    public enum EntityChangeModificationType
    {
        /// <summary>
        /// The entity was added to the database.
        /// </summary>
        Added = 0x01,

        /// <summary>
        /// The entity was modified in the database.
        /// </summary>
        Modified = 0x02,

        /// <summary>
        /// The entity was deleted from the database.
        /// </summary>
        Deleted = 0x04,
    }
}
