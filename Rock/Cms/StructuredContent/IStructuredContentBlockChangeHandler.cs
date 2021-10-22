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
using Rock.Data;

namespace Rock.Cms.StructuredContent
{
    /// <summary>
    /// Adds ability to detect and respond to changes in structured content.
    /// </summary>
    /// <remarks>
    /// This is an internal API that supports the Rock infrastructure and not
    /// subject to the same compatibility standards as public APIs. It may be
    /// changed or removed without notice in any release. You should only use
    /// it directly in your code with extreme caution and knowing that doing so
    /// can result in application failures when updating to a new Rock release.
    /// </remarks>
    public interface IStructuredContentBlockChangeHandler
    {
        /// <summary>
        /// Detects the content changes from the previous version. Any data
        /// that needs to be stored for later use should be stored in
        /// <see cref="StructuredContentChanges.AddOrReplaceData{TData}(TData)"/>.
        /// This method is called once for each block.
        /// </summary>
        /// <remarks></remarks>
        /// <param name="newData">The new data for the block; or <c>null</c> if the block was removed.</param>
        /// <param name="oldData">The old data for the block; or <c>null</c> if the block is new.</param>
        /// <param name="changes">The changes.</param>
        void DetectChanges( dynamic newData, dynamic oldData, StructuredContentChanges changes );

        /// <summary>
        /// Apply changes to the database that the component needs to make from
        /// the changes specified. This method is called once for the entire
        /// save operation.
        /// </summary>
        /// <remarks>
        /// SaveChanges() should not be called, it will be called automatically.
        /// </remarks>
        /// <param name="helper">The helper that is running the operation.</param>
        /// <param name="changes">The changes previously detected.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if a call to SaveChanges() is now required; <c>false</c> otherwise.</returns>
        bool ApplyDatabaseChanges( StructuredContentHelper helper, StructuredContentChanges changes, RockContext rockContext );
    }
}
