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

using Rock.Data;
using Rock.Model;

namespace Rock.Core.Automation
{
    /// <summary>
    /// Represents a single value type that can exist in an <see cref="AutomationRequest"/>.
    /// </summary>
    internal class AutomationValueDefinition
    {
        /// <summary>
        /// The key in the values dictionary for an automation request that
        /// this entry represents.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The description of what this key is and how it is used. This should
        /// be written for an end-user and contain language they would understand.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The C# type that represents the most basic type of this value. For
        /// example, if the value could either be a <see cref="Person"/> or a
        /// <see cref="Group"/> then it should be an <see cref="IEntity"/>. If
        /// it can only ever be a <see cref="Person"/> then it should be that
        /// type.
        /// </summary>
        public Type Type { get; set; }
    }
}
