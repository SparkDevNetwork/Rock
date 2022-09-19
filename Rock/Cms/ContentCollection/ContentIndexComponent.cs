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
using System.Threading.Tasks;

using Rock.Cms.ContentCollection.IndexDocuments;
using Rock.Cms.ContentCollection.Search;
using Rock.Extension;

namespace Rock.Cms.ContentCollection
{
    /// <summary>
    /// The base class that all content collection index components must inherit from.
    /// </summary>
    internal abstract class ContentIndexComponent : Component
    {
        /// <summary>
        /// Gets a value indicating whether this instance is connected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsConnected { get; }

        /// <summary>
        /// Gets the index location.
        /// </summary>
        /// <value>
        /// The index location.
        /// </value>
        public abstract string IndexLocation { get; }

        /// <summary>
        /// Creates the index for the specified document type.
        /// </summary>
        /// <param name="deleteIfExists">if set to <c>true</c> then any existing index will be deleted.</param>
        public virtual Task CreateIndexAsync<TDocument>( bool deleteIfExists = true )
            where TDocument : IndexDocumentBase
        {
            return CreateIndexAsync( typeof( TDocument ), deleteIfExists );
        }

        /// <summary>
        /// Deletes the index for the specified document type.
        /// </summary>
        /// <param name="documentType">The type of document whose index will be deleted.</param>
        /// <param name="deleteIfExists">if set to <c>true</c> then any existing index will be deleted.</param>
        public abstract Task CreateIndexAsync( Type documentType, bool deleteIfExists = true );

        /// <summary>
        /// Deletes the index for the specified document type.
        /// </summary>
        public virtual Task DeleteIndexAsync<TDocument>()
            where TDocument : IndexDocumentBase
        {
            return DeleteIndexAsync( typeof( TDocument ) );
        }

        /// <summary>
        /// Deletes the index for the specified document type.
        /// </summary>
        /// <param name="documentType">The type of document whose index will be deleted.</param>
        public abstract Task DeleteIndexAsync( Type documentType );

        /// <summary>
        /// Indexes the document.
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="document">The document.</param>
        public abstract Task IndexDocumentAsync<TDocument>( TDocument document )
            where TDocument : IndexDocumentBase;

        /// <summary>
        /// Deletes all matching documents across all document types.
        /// </summary>
        /// <param name="query">The query to use when matching documents.</param>
        public abstract Task DeleteMatchingDocumentsAsync( SearchQuery query );

        /// <summary>
        /// Deletes all the matching documents of the specific document type.
        /// </summary>
        /// <typeparam name="TDocument">The document types to be deleted.</typeparam>
        /// <param name="query">The query to use when matching documents.</param>
        public virtual Task DeleteMatchingDocumentsAsync<TDocument>( SearchQuery query )
            where TDocument : IndexDocumentBase
        {
            return DeleteMatchingDocumentsAsync( typeof( TDocument ), query );
        }

        /// <summary>
        /// Deletes all the matching documents of the specific document type.
        /// </summary>
        /// <param name="documentType">The document type to be deleted.</param>
        /// <param name="query">The query to use when matching documents.</param>
        public abstract Task DeleteMatchingDocumentsAsync( Type documentType, SearchQuery query );

        /// <summary>
        /// Searches for the specified query.
        /// </summary>
        /// <param name="query">The query to use when searching for documents.</param>
        /// <param name="options">Optional options that describe the query in more detail.</param>
        /// <returns>An instance of <see cref="SearchResults"/> that represents the matching documents.</returns>
        public abstract Task<SearchResults> SearchAsync( SearchQuery query, SearchOptions options = null );

        /// <summary>
        /// Gets the name of the index from the type. This takes into account
        /// any custom overrides that have been applied.
        /// </summary>
        /// <param name="type">The type whose index name is being requested.</param>
        /// <returns>The name to use for the index.</returns>
        protected virtual string GetIndexName( Type type )
        {
            return type.Name.ToLower();
        }
    }
}