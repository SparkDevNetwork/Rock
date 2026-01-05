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

using Rock.AI.Agent.Classes.Common;

namespace Rock.AI.Agent.Classes.Entity
{
    /// <summary>
    /// Represents a note type.
    /// </summary>
    internal class NoteTypeResult : EntityResultBase
    {
        /// <summary>
        /// The name of the note type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The entity type that this note type is associated with.
        /// </summary>
        public KeyNameResult EntityType { get; set; }
    }
}
