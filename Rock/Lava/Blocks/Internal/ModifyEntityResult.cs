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

using System.Collections.Generic;

namespace Rock.Lava.Blocks.Internal
{
    /// <summary>
    /// Describes the result of a modify entity operation in Lava.
    /// </summary>
    internal class ModifyEntityResult : LavaDataObject
    {
        /// <summary>
        /// Whether the modification was a success or not.
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Message about the success for failure of the modification.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// List of errors in saving the entity.
        /// </summary>
        public List<ValidationError> ValidationErrors { get; set; } = new List<ValidationError>();
    }
}
