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

using System.Threading.Tasks;

using Rock.Attribute;

namespace Rock.Cms.ContentCollection
{
    /// <summary>
    /// The methods that must be implemented for an entity to be able to
    /// participate in the content collection system.
    /// </summary>
    internal interface IContentCollectionIndexer
    {
        /// <summary>
        /// Creates or updates an indexed document for the specified entity.
        /// </summary>
        /// <param name="id">The identifier of the entity to be indexed.</param>
        /// <param name="options">The options that describe this index operation request.</param>
        /// <returns>The number of documents that were indexed.</returns>
        Task<int> IndexContentCollectionDocumentAsync( int id, IndexDocumentOptions options );

        /// <summary>
        /// Deletes the specified entity from the index.
        /// </summary>
        /// <param name="id">The identifier of the entity to be deleted from the index.</param>
        Task DeleteContentCollectionDocumentAsync( int id );

        /// <summary>
        /// Creates or updates all documents that belong to the specified source.
        /// </summary>
        /// <param name="sourceId">The identifier of the source that should be indexed.</param>
        /// <param name="options">The options that describe this index operation request.</param>
        /// <returns>The number of documents that were indexed.</returns>
        Task<int> IndexAllContentCollectionSourceDocumentsAsync( int sourceId, IndexDocumentOptions options );

        /// <summary>
        /// Deletes all documents that belong to the specified source.
        /// </summary>
        /// <param name="sourceId">The identifier of the source whose documents should be deleted.</param>
        Task DeleteAllContentCollectionSourceDocumentsAsync( int sourceId );
    }
}
