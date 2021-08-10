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
    /// Defines the methods that must be implemented in order for a type to be
    /// considered a save hook that can be called during SaveChanges().
    /// </summary>
    /// <remarks>
    /// This is an internal API that supports the Rock infrastructure and not
    /// subject to the same compatibility standards as public APIs. It may be
    /// changed or removed without notice in any release. You should only use
    /// it directly in your code with extreme caution and knowing that doing so
    /// can result in application failures when updating to a new Rock release.
    /// </remarks>
    public interface IEntitySaveHook
    {
        /// <summary>
        /// Called before the save operation is executed.
        /// </summary>
        /// <param name="entry">The entity entry that identifies the entity that is about to be saved.</param>
        void PreSave( IEntitySaveEntry entry );

        /// <summary>
        /// Called if the save operation failed or was otherwise aborted.
        /// </summary>
        /// <param name="entry">The entity entry that identifies the entity that failed to be saved.</param>
        /// <remarks>
        /// This method is only called if <see cref="PreSave(IEntitySaveEntry)"/>
        /// returns without error.
        /// </remarks>
        void SaveFailed( IEntitySaveEntry entry );

        /// <summary>
        /// Called after the save operation has been executed.
        /// </summary>
        /// <param name="entry">The entity entry that identifies the entity that was saved.</param>
        /// <remarks>
        /// This method is only called if <see cref="PreSave(IEntitySaveEntry)"/>
        /// returns without error.
        /// </remarks>
        void PostSave( IEntitySaveEntry entry );
    }
}
