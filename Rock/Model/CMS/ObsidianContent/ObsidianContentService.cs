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

using System.Linq;

namespace Rock.Model
{
    /// <summary>
    /// Data access and service class for <see cref="Rock.Model.ObsidianContent"/> entity objects.
    /// </summary>
    public partial class ObsidianContentService
    {
        /// <summary>
        /// Returns the <see cref="Rock.Model.ObsidianContent"/> owned by the specified <see cref="Rock.Model.Block"/> placement.
        /// </summary>
        /// <param name="blockId">A <see cref="System.Int32"/> representing the Id of the owning <see cref="Rock.Model.Block"/>.</param>
        /// <returns>The <see cref="Rock.Model.ObsidianContent"/> for the specified block, or <c>null</c> if none exists yet.</returns>
        public ObsidianContent GetByBlockId( int blockId )
        {
            return Queryable().FirstOrDefault( c => c.BlockId == blockId );
        }

        /// <summary>
        /// Returns the <see cref="Rock.Model.ObsidianContent"/> owned by the specified <see cref="Rock.Model.Block"/> placement,
        /// creating and adding a new (unsaved) record to the context when one does not yet exist.
        /// </summary>
        /// <remarks>
        /// The returned record, when newly created, has already been added to the context. The caller is
        /// responsible for populating its content and calling <see cref="Rock.Data.RockContext.SaveChanges()"/>.
        /// </remarks>
        /// <param name="blockId">A <see cref="System.Int32"/> representing the Id of the owning <see cref="Rock.Model.Block"/>.</param>
        /// <returns>The existing or newly created <see cref="Rock.Model.ObsidianContent"/> for the specified block.</returns>
        public ObsidianContent GetOrCreateByBlockId( int blockId )
        {
            var content = GetByBlockId( blockId );

            if ( content == null )
            {
                content = new ObsidianContent
                {
                    BlockId = blockId,
                    IsActive = true
                };

                Add( content );
            }

            return content;
        }
    }
}
