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

namespace Rock.Data
{
    /// <summary>
    /// The state in which an entity is being tracked by a context.
    /// </summary>
    public enum EntityContextState
    {
        /// <summary>
        /// The entity is not being tracked by the context.
        /// </summary>
        Detached,

        /// <summary>
        /// The entity is being tracked by the context and exists in the database.
        /// Its property values have not changed from the values in the database.
        /// </summary>
        Unchanged,

        /// <summary>
        /// The entity is being tracked by the context and exists in the database.
        /// It has been marked for deletion from the database.
        /// </summary>
        Deleted,

        /// <summary>
        /// The entity is being tracked by the context but does not yet exist in
        /// the database.
        /// </summary>
        Added,

        /// <summary>
        /// The entity is being tracked by the context and exists in the database.
        /// Some or all of its property values have been modified.
        /// </summary>
        Modified
    }
}
