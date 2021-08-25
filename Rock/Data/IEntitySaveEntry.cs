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

namespace Rock.Data
{
    /// <summary>
    /// A single entity entry that is in the process of being saved.
    /// </summary>
    /// <remarks>
    /// This is an internal API that supports the Rock infrastructure and not
    /// subject to the same compatibility standards as public APIs. It may be
    /// changed or removed without notice in any release. You should only use
    /// it directly in your code with extreme caution and knowing that doing so
    /// can result in application failures when updating to a new Rock release.
    /// </remarks>
    public interface IEntitySaveEntry
    {
        /// <summary>
        /// Gets the entity that is being saved.
        /// </summary>
        /// <value>
        /// The entity that is being saved.
        /// </value>
        object Entity { get; }

        /// <summary>
        /// Gets the original values the entity was loaded with. Only valid
        /// if <see cref="PreSaveState"/> has the value <see cref="EntityContextState.Modified"/>.
        /// </summary>
        /// <value>
        /// The original values the entity was loaded with.
        /// </value>
        IReadOnlyDictionary<string, object> OriginalValues { get; }

        /// <summary>
        /// Gets the database context that is processing the save operation for
        /// this entity.
        /// </summary>
        /// <value>
        /// The database context that is processing the save operation for
        /// this entity.
        /// </value>
        /// <remarks>
        /// Declare as object to not force a dependency on RockContext which
        /// in turn force a direct dependency on Entity Framework.
        /// </remarks>
        object DataContext { get; }

        /// <summary>
        /// Gets the state of the entity just before the save operation started.
        /// Useful in PostSave methods to determine what type of operation was
        /// performed.
        /// </summary>
        /// <value>
        /// The state of the entity just before the save operation started.
        /// </value>
        EntityContextState PreSaveState { get; }

        /// <summary>
        /// Gets the current state of the entity. Inside the PostSave methods
        /// this will probably not contain the value you expect as it would
        /// be updated to reflect the new state of the entity. Use the
        /// <see cref="PreSaveState"/> property in those cases.
        /// </summary>
        /// <value>
        /// The current state of the entity.
        /// </value>
        EntityContextState State { get; }
    }
}
