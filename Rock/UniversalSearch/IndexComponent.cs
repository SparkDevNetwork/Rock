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
using System.Linq;

using Rock.Extension;
using Rock.UniversalSearch.IndexModels;

namespace Rock.UniversalSearch
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class IndexComponent : Component
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
        /// Indexes the document.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document">The document.</param>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="mappingType">Type of the mapping.</param>
        [Obsolete( "Use the method that takes only the document parameter, instead." )]
        [RockObsolete( "17.1" )]
        public void IndexDocument<T>( T document, string indexName = null, string mappingType = null ) where T : class, new()
        {
            if ( document != null )
            {
                if ( !string.IsNullOrEmpty( indexName ) )
                {
                    if ( indexName.ToLower() != document.GetType().Name.ToLower() )
                    {
                        throw new ArgumentOutOfRangeException( "indexName", "Index Name must be the type name." );
                    }
                }

                if ( !string.IsNullOrEmpty( mappingType ) )
                {
                    if ( mappingType.ToLower() != document.GetType().Name.ToLower() )
                    {
                        throw new ArgumentOutOfRangeException( "mappingType", "Mapping Type must be the type name." );
                    }
                }
            }

            IndexDocument( document );
        }

        /// <summary>
        /// Indexes multiple documents.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="documents">The documents.</param>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="mappingType">Type of the mapping.</param>
        [Obsolete( "Use the method that takes only the documents parameter, instead." )]
        [RockObsolete( "17.1" )]
        public void IndexDocuments<T>( IEnumerable<T> documents, string indexName = null, string mappingType = null ) where T : class, new()
        {
            if ( !string.IsNullOrEmpty( indexName ) && documents != null && documents.Any() )
            {
                if ( indexName.ToLower() != documents.First().GetType().Name.ToLower() )
                {
                    throw new ArgumentOutOfRangeException( "indexName", "Index Name must be the type name." );
                }
            }

            if ( !string.IsNullOrEmpty( mappingType ) && documents != null && documents.Any() )
            {
                if ( mappingType.ToLower() != documents.First().GetType().Name.ToLower() )
                {
                    throw new ArgumentOutOfRangeException( "mappingType", "Mapping Type must be the type name." );
                }
            }

            IndexDocuments( documents );
        }

        /// <summary>
        /// Indexes the document.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document">The document.</param>
        public abstract void IndexDocument<T>( T document ) where T : class, new();

        /// <summary>
        /// Indexes multiple documents.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="documents">The documents.</param>
        public virtual void IndexDocuments<T>( IEnumerable<T> documents ) where T : class, new()
        {
            foreach ( var document in documents )
            {
                IndexDocument( document );
            }
        }

        /// <summary>
        /// Deletes the type of the documents by.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="indexName">Name of the index.</param>
        [Obsolete( "Use the method that does not take a parameter, instead." )]
        [RockObsolete( "17.1" )]
        public void DeleteDocumentsByType<T>( string indexName = null ) where T : class, new()
        {
            if ( !string.IsNullOrEmpty( indexName ) )
            {
                if ( indexName.ToLower() != typeof( T ).Name.ToLower() )
                {
                    throw new ArgumentOutOfRangeException( "indexName", "Index Name must be the type name." );
                }
            }

            DeleteDocumentsByType<T>();
        }

        /// <summary>
        /// Deletes the type of the documents by.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public abstract void DeleteDocumentsByType<T>() where T : class, new();

        /// <summary>
        /// Creates the index.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        /// <param name="deleteIfExists">if set to <c>true</c> [delete if exists].</param>
        public abstract void CreateIndex( Type documentType, bool deleteIfExists = true );

        /// <summary>
        /// Deletes the index.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        public abstract void DeleteIndex( Type documentType );

        /// <summary>
        /// Deletes the document.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document">The document.</param>
        /// <param name="indexName">Name of the index.</param>
        [Obsolete( "Use the method that does not take a parameter, instead." )]
        [RockObsolete( "17.1" )]
        public void DeleteDocument<T>( T document, string indexName = null ) where T : class, new()
        {
            if ( !string.IsNullOrEmpty( indexName ) )
            {
                if ( indexName.ToLower() != document.GetType().Name.ToLower() )
                {
                    throw new ArgumentOutOfRangeException( "indexName", "Index Name must be the type name." );
                }
            }

            DeleteDocument<T>( document );
        }

        /// <summary>
        /// Deletes the document.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document">The document.</param>
        public abstract void DeleteDocument<T>( T document ) where T : class, new();

        /// <summary>
        /// Deletes the document by identifier.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        /// <param name="id">The identifier.</param>
        public abstract void DeleteDocumentById( Type documentType, int id );

        /// <summary>
        /// Deletes the document by property.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">The property value.</param>
        public abstract void DeleteDocumentByProperty( Type documentType, string propertyName, object propertyValue );

        /// <summary>
        /// Gets the document by identifier.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public abstract IndexModelBase GetDocumentById( Type documentType, int id );

        /// <summary>
        /// Gets the document by identifier.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public abstract IndexModelBase GetDocumentById( Type documentType, string id );

        /// <summary>
        /// Searches the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="searchType">Type of the search.</param>
        /// <param name="entities">The entities.</param>
        /// <param name="criteria">The criteria.</param>
        /// <param name="size">The size.</param>
        /// <param name="from">From.</param>
        /// <returns></returns>
        public abstract List<IndexModelBase> Search( string query, SearchType searchType = SearchType.Wildcard, List<int> entities = null, SearchFieldCriteria criteria = null, int? size = null, int? from = null );

        /// <summary>
        /// Searches the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="searchType">Type of the search.</param>
        /// <param name="entities">The entities.</param>
        /// <param name="criteria">The criteria.</param>
        /// <param name="size">The size.</param>
        /// <param name="from">From.</param>
        /// <param name="totalResultsAvailable">The total results available.</param>
        /// <returns></returns>
        public abstract List<IndexModelBase> Search( string query, SearchType searchType, List<int> entities, SearchFieldCriteria criteria, int? size, int? from, out long totalResultsAvailable );
    }

    /// <summary>
    /// Type of Search
    /// </summary>
    public enum SearchType
    {
        /// <summary>
        /// Exact Match
        /// </summary>
        ExactMatch = 0,

        /// <summary>
        /// Fuzzy Match
        /// </summary>
        Fuzzy = 1,

        /// <summary>
        /// Wildcard match
        /// </summary>
        Wildcard = 2
    }
}