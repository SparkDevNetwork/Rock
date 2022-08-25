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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

using Rock.Cms.ContentCollection.IndexDocuments;
using Rock.Cms.ContentCollection.Search;
using Rock.Extension;
using Rock.Web.Cache;

namespace Rock.Cms.ContentCollection
{
    /// <summary>
    /// MEF Container class for Binary File Search Components
    /// </summary>
    internal class ContentIndexContainer : Container<ContentIndexComponent, IComponentData>
    {
        #region Properties

        /// <summary>
        /// Singleton instance of this container.
        /// </summary>
        private static readonly Lazy<ContentIndexContainer> _instance =
            new Lazy<ContentIndexContainer>( () => new ContentIndexContainer() );

        /// <summary>
        /// Gets the instance of this container that will handle all requests.
        /// </summary>
        /// <value>
        /// The instance of this container that will handle all requests.
        /// </value>
        public static ContentIndexContainer Instance => _instance.Value;

        /// <summary>
        /// Gets a value indicating whether indexing is currently enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if indexing is currently enabled; otherwise, <c>false</c>.
        /// </value>
        public static bool IndexingEnabled => GetActiveComponent() != null;

        /// <summary>
        /// Gets or sets the MEF components that have been discovered.
        /// </summary>
        /// <value>
        /// The MEF components that have been discovered.
        /// </value>
        [ImportMany( typeof( ContentIndexComponent ) )]
        protected sealed override IEnumerable<Lazy<ContentIndexComponent, IComponentData>> MEFComponents { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Prevents a default instance of the <see cref="ContentIndexContainer"/> class from being created.
        /// </summary>
        private ContentIndexContainer()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the active component.
        /// </summary>
        /// <returns></returns>
        public static ContentIndexComponent GetActiveComponent()
        {
            var usComponent = Rock.UniversalSearch.IndexContainer.GetActiveComponent();

            if ( usComponent == null )
            {
                return null;
            }

            if ( usComponent.GetType() == typeof( Rock.UniversalSearch.IndexComponents.Lucene ) )
            {
                return Instance.Components.Values
                    .Select( c => c.Value )
                    .FirstOrDefault( c => c.GetType() == typeof( IndexComponents.Lucene ) );
            }
            else if ( usComponent.GetType() == typeof( Rock.UniversalSearch.IndexComponents.Elasticsearch ) )
            {
                return Instance.Components.Values
                    .Select( c => c.Value )
                    .FirstOrDefault( c => c.GetType() == typeof( IndexComponents.Elasticsearch ) );
            }

            return null;
        }

        /// <summary>
        /// Indexes the documents.
        /// </summary>
        /// <param name="document">The document.</param>
        public static async Task IndexDocumentAsync<TDocument>( TDocument document )
            where TDocument : IndexDocumentBase
        {
            var activeComponent = GetActiveComponent();

            if ( activeComponent == null )
            {
                return;
            }

            await activeComponent.IndexDocumentAsync( document );
        }

        /// <summary>
        /// Creates the index.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        /// <param name="deleteIfExists">if set to <c>true</c> and the index already exists it will be deleted first.</param>
        public static async Task CreateIndexAsync( Type documentType, bool deleteIfExists = true )
        {
            var activeComponent = GetActiveComponent();

            if ( activeComponent == null )
            {
                return;
            }

            await activeComponent.CreateIndexAsync( documentType, deleteIfExists );
        }

        /// <summary>
        /// Deletes the index.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        public static async Task DeleteIndexAsync( Type documentType )
        {
            var activeComponent = GetActiveComponent();

            if ( activeComponent == null )
            {
                return;
            }

            await activeComponent.DeleteIndexAsync( documentType );
        }

        /// <summary>
        /// Deletes all matching documents across all document types.
        /// </summary>
        /// <param name="query">The query to use when matching documents.</param>
        public static Task DeleteMatchingDocumentsAsync( SearchQuery query )
        {
            var activeComponent = GetActiveComponent();

            if ( activeComponent == null )
            {
                return Task.CompletedTask;
            }

            return activeComponent.DeleteMatchingDocumentsAsync( query );
        }

        /// <summary>
        /// Deletes all the matching documents of the specific document type.
        /// </summary>
        /// <typeparam name="TDocument">The document types to be deleted.</typeparam>
        /// <param name="query">The query to use when matching documents.</param>
        public static Task DeleteMatchingDocumentsAsync<TDocument>( SearchQuery query )
            where TDocument : IndexDocumentBase
        {
            var activeComponent = GetActiveComponent();

            if ( activeComponent == null )
            {
                return Task.CompletedTask;
            }

            return activeComponent.DeleteMatchingDocumentsAsync<TDocument>( query );
        }

        /// <summary>
        /// Deletes all the matching documents of the specific document type.
        /// </summary>
        /// <param name="documentType">The document type to be deleted.</param>
        /// <param name="query">The query to use when matching documents.</param>
        public static Task DeleteMatchingDocumentsAsync( Type documentType, SearchQuery query )
        {
            var activeComponent = GetActiveComponent();

            if ( activeComponent == null )
            {
                return Task.CompletedTask;
            }

            return activeComponent.DeleteMatchingDocumentsAsync( documentType, query );
        }

        /// <summary>
        /// Creates all indexes if they do not already exist.
        /// </summary>
        public static async Task CreateAllIndexesAsync()
        {
            var documentTypes = GetAllDocumentTypes();

            foreach ( var documentType in documentTypes )
            {
                await CreateIndexAsync( documentType, false );
            }
        }

        /// <summary>
        /// Gets all document types that are currently enabled in the system.
        /// </summary>
        /// <returns>An enumeration of all document <see cref="Type"/> objects.</returns>
        public static IEnumerable<Type> GetAllDocumentTypes()
        {
            return EntityTypeCache.All()
                .Where( et => et.IsContentCollectionIndexingEnabled )
                .Select( et => et.ContentCollectionDocumentType )
                .ToList();
        }

        #endregion
    }
}
