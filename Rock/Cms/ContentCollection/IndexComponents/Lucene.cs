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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;

using Newtonsoft.Json.Linq;

using Rock.Cms.ContentCollection.Attributes;
using Rock.Cms.ContentCollection.IndexDocuments;
using Rock.Cms.ContentCollection.Search;
using Rock.Model;

using Document = Lucene.Net.Documents.Document;

namespace Rock.Cms.ContentCollection.IndexComponents
{
    /// <summary>
    /// Lucene Search Index Provider
    /// </summary>
    /// <seealso cref="Rock.Cms.ContentCollection.ContentIndexComponent" />
    [Description( "Lucene.Net Content Collection Index (v4.8)" )]
    [Export( typeof( ContentIndexComponent ) )]
    [ExportMetadata( "ComponentName", "Lucene.Net 4.8" )]

    [Rock.SystemGuid.EntityTypeGuid( "B530717C-85C3-4FA5-920F-EFBF38F0587A" )]
    internal sealed class Lucene : ContentIndexComponent
    {
        #region Private Fields

        private const string LuceneIdField = "id";
        private const string LuceneIndexField = "index";
        private const string LuceneTypeField = "type";

        private static readonly LuceneVersion _matchVersion = LuceneVersion.LUCENE_48;
        private static readonly ConcurrentDictionary<Type, Index> _indexes = new ConcurrentDictionary<Type, Index>();
        private static IndexWriterConfig _indexWriterConfig = null;
        private static IndexWriter _writer = null;
        private static int _activeWriterCount = 0;
        private static DirectoryReader _reader = null;
        private static IndexSearcher _indexSearcher = null;
        private static FSDirectory _directory;
        private static Timer _timer = null;
        private static readonly string _path = System.Web.Hosting.HostingEnvironment.MapPath( "~/App_Data/LuceneContentCollectionIndex" );
        private static readonly object _lockWriter = new object();

        #endregion

        #region Properties

        /// <inheritdoc/>
        public override bool IsConnected => true;

        /// <inheritdoc/>
        public override string IndexLocation => _path;

        /// <summary>
        /// Gets the Lucene directory.
        /// </summary>
        /// <value>
        /// The Lucene directory.
        /// </value>
        private static FSDirectory Directory
        {
            get
            {
                if ( _directory == null )
                {
                    if ( !System.IO.Directory.Exists( _path ) )
                    {
                        System.IO.Directory.CreateDirectory( _path );
                    }

                    _directory = FSDirectory.Open( new DirectoryInfo( _path ) );
                }

                return _directory;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Dispose of the index from memory and close all files.
        /// </summary>
        public static void Dispose()
        {
            lock ( _lockWriter )
            {
                if ( _writer != null )
                {
                    try
                    {
                        FlushWriter();
                        _writer.Dispose();
                    }
                    finally
                    {
                        if ( IndexWriter.IsLocked( Directory ) )
                        {
                            IndexWriter.Unlock( Directory );
                        }

                        _writer = null;
                    }
                }
            }

            _reader?.Dispose();
            _reader = null;

            _timer?.Dispose();
            _timer = null;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Id used by Lucene.
        /// Note: Changing the formula result in the data not being able to be deleted / updated. When changing the formula, delete the files from the LuceneSearchIndex folder and then re-index.
        /// </summary>
        /// <param name="mappingType">Type of the mapping.</param>
        /// <param name="id">The entity identifier.</param>
        /// <returns>Lucene Id</returns>
        private static string LuceneID( string mappingType, string id )
        {
            return $"{mappingType}_{id}";
        }

        /// <summary>
        /// Opens the index writer.
        /// When the writer is idle, then it will flush and close the index writer
        /// </summary>
        private static ActiveIndexWriter OpenWriter()
        {
            lock ( _lockWriter )
            {
                if ( _writer == null )
                {
                    // No default Analyzer. So we will have to explicitly specify
                    // it when using IndexReader/IndexWriter.
                    _indexWriterConfig = new IndexWriterConfig( _matchVersion, null )
                    {
                        OpenMode = OpenMode.CREATE_OR_APPEND
                    };

                    _indexWriterConfig.OpenMode = OpenMode.CREATE_OR_APPEND;
                    if ( IndexWriter.IsLocked( Directory ) )
                    {
                        IndexWriter.Unlock( Directory );
                    }

                    _writer = new IndexWriter( Directory, _indexWriterConfig );
                    _writer.Flush( true, true );
                    _writer.Commit();
                }

                _activeWriterCount++;

                return new ActiveIndexWriter( _writer, CloseActiveWriter );
            }
        }

        /// <summary>
        /// Flushes the index writer.
        /// </summary>
        private static void FlushWriter()
        {
            if ( _writer != null )
            {
                _writer.Flush( true, true );
                _writer.Commit();
            }
        }

        /// <summary>
        /// Callback when the close writer timer has expired. Check if the
        /// writer is considered stale and then close it.
        /// </summary>
        /// <param name="state">Timer state. Unused.</param>
        private static void CloseWriterTimer( object state )
        {
            lock ( _lockWriter )
            {
                if ( _activeWriterCount > 0 || _writer == null )
                {
                    return;
                }

                FlushWriter();

                try
                {
                    _writer.Dispose( true );
                }
                finally
                {
                    if ( IndexWriter.IsLocked( Directory ) )
                    {
                        IndexWriter.Unlock( Directory );
                    }

                    _writer = null;
                }
            }
        }

        /// <summary>
        /// Closes the active writer. If there are no more writers in use then
        /// start or reset the real close timer.
        /// </summary>
        private static void CloseActiveWriter()
        {
            lock ( _lockWriter )
            {
                _activeWriterCount--;

                if ( _activeWriterCount == 0 )
                {
                    if ( _timer == null )
                    {
                        _timer = new Timer( new TimerCallback( CloseWriterTimer ), null, 10000, 0 );
                    }
                    else
                    {
                        _timer.Change( 10000, 0 );
                    }
                }
            }
        }

        /// <summary>
        /// Opens the index reader.
        /// </summary>
        private static bool OpenReader()
        {
            try
            {
                if ( _reader == null )
                {
                    _reader = DirectoryReader.Open( Directory );
                    _indexSearcher = new IndexSearcher( _reader );
                }
                else
                {
                    DirectoryReader newReader =
                        DirectoryReader.OpenIfChanged( _reader );
                    if ( newReader != null )
                    {
                        _reader.Dispose();
                        _reader = newReader;
                        _indexSearcher = new IndexSearcher( _reader );
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Converts Lucene document to index model.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="hit">The scoredoc.</param>
        /// <returns>Index model</returns>
        private static IndexDocumentBase LuceneDocToIndexModel( Query query, ScoreDoc hit )
        {
            var doc = _indexSearcher.Doc( hit.Doc );

            var document = new IndexDocumentBase();

            try
            {
                var hitJsonField = doc.GetField( "JSON" );
                if ( hitJsonField != null )
                {
                    string hitJson = hitJsonField.GetStringValue();
                    JObject jObject = JObject.Parse( hitJson );
                    Type indexModelType = Type.GetType( $"{jObject["IndexModelType"].ToStringSafe()}, {jObject["IndexModelAssembly"].ToStringSafe()}" );

                    if ( indexModelType != null )
                    {
                        // Return the source document as the derived type.
                        document = ( IndexDocumentBase ) jObject.ToObject( indexModelType );
                    }
                    else
                    {
                        // Return the source document as the generic type.
                        document = jObject.ToObject<IndexDocumentBase>();
                    }
                }

                Explanation explanation = _indexSearcher.Explain( query, hit.Doc );
                document["Explain"] = explanation.ToString();
                document.Score = hit.Score;

                return document;
            }
            catch
            {
                // Ignore if the result if an exception resulted (most likely
                // cause is getting a result from a non-rock index).
            }

            return null;
        }

        /// <summary>
        /// Gets the index of the document type. This contains the information
        /// about what properties should be searchable.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        /// <returns>The index that describes the document or <c>null</c> if it was not a valid document type.</returns>
        private static Index GetDocumentIndex( Type documentType )
        {
            if ( _indexes.TryGetValue( documentType, out var index ) )
            {
                return index;
            }

            if ( !typeof( IndexDocumentBase ).IsAssignableFrom( documentType ) )
            {
                return null;
            }

            index = new Index();

            var typeMapping = new List<TypeMappingProperties>();
            var fieldAnalyzers = new Dictionary<string, Analyzer>();

            // Get properties from the document type and add them to the index.
            // Attributes will be added dynamically as the documents are loaded.
            var documentProperties = documentType.GetProperties();

            foreach ( var property in documentProperties )
            {
                var indexAttribute = property.GetCustomAttribute<IndexFieldAttribute>( false );
                if ( indexAttribute == null )
                {
                    continue;
                }

                var propertyName = property.Name;

                var typeMappingProperty = new TypeMappingProperties
                {
                    Name = propertyName,
                    Boost = ( float ) indexAttribute.Boost,
                    IsSearched = indexAttribute.IsSearched,
                    FieldType = indexAttribute.FieldType
                };

                typeMapping.Add( typeMappingProperty );

                if ( typeMappingProperty.FieldType == IndexFieldType.Text )
                {
                    fieldAnalyzers[propertyName] = Analyzer.NewAnonymous( createComponents: ( fieldName, reader ) =>
                    {
                        var tokenizer = new WhitespaceTokenizer( _matchVersion, reader );
                        var result = new StandardFilter( _matchVersion, tokenizer );

                        return new TokenStreamComponents( tokenizer, result );
                    } );
                }
            }

            index.MappingProperties = typeMapping;
            index.FieldAnalyzers = fieldAnalyzers;

            _indexes.TryAdd( documentType, index );

            return index;
        }

        /// <summary>
        /// Adds the field to document after applying the proper formatting to it.
        /// </summary>
        /// <param name="doc">The document the field value will be added to.</param>
        /// <param name="mapping">The mapping that describes the field.</param>
        /// <param name="value">The field value.</param>
        private static void AddFieldToDocument( Document doc, TypeMappingProperties mapping, object value )
        {
            if ( mapping.FieldType == IndexFieldType.Boolean )
            {
                doc.Add( new StringField( mapping.Name, value.ToStringSafe().ToLower(), global::Lucene.Net.Documents.Field.Store.NO )
                {
                    Boost = mapping.Boost
                } );
            }
            else if ( mapping.FieldType == IndexFieldType.Integer )
            {
                doc.Add( new StringField( mapping.Name, value.ToStringSafe().ToLower(), global::Lucene.Net.Documents.Field.Store.NO )
                {
                    Boost = mapping.Boost
                } );
            }
            else if ( mapping.FieldType == IndexFieldType.DateTime )
            {
                var strValue = value is DateTime dt ? dt.ToString( "O" ).ToLower() : value.ToStringSafe().ToLower();

                doc.Add( new StringField( mapping.Name, strValue, global::Lucene.Net.Documents.Field.Store.NO )
                {
                    Boost = mapping.Boost
                } );
            }
            else if ( mapping.FieldType == IndexFieldType.Keyword )
            {
                doc.Add( new StringField( mapping.Name, value.ToStringSafe().ToLower(), global::Lucene.Net.Documents.Field.Store.NO )
                {
                    Boost = mapping.Boost
                } );
            }
            else
            {
                doc.Add( new TextField( mapping.Name, value.ToStringSafe().ToLower(), global::Lucene.Net.Documents.Field.Store.YES )
                {
                    Boost = mapping.Boost
                } );
            }
        }

        /// <summary>
        /// Gets the lucene query that represents the Rock query.
        /// </summary>
        /// <param name="query">The Rock search query.</param>
        /// <param name="index">The index information that describes additional field information.</param>
        /// <returns>The Lucene query object or null if the rock query was empty.</returns>
        private static Query GetLuceneQuery( SearchQuery query, Index index )
        {
            if ( query == null )
            {
                return null;
            }

            var luceneQuery = new BooleanQuery();
            var occur = query.IsAllMatching ? Occur.MUST : Occur.SHOULD;

            // Process all the items in the query.
            foreach ( var item in query )
            {
                if ( item is SearchTerm searchTerm )
                {
                    if ( searchTerm.Text.IsNotNullOrWhiteSpace() && index != null )
                    {
                        var wildcardQuery = new BooleanQuery();

                        // Determine which fields to search against.
                        var fieldsToSearch = index?.MappingProperties
                            .Where( p => p.IsSearched )
                            .ToList();

                        // Break each search term into component words.
                        var queryTerms = searchTerm.Text.Split( ' ' ).Select( p => p.Trim() ).ToList();

                        foreach ( var queryTerm in queryTerms )
                        {
                            var innerQuery = new BooleanQuery();
                            foreach ( var field in fieldsToSearch )
                            {
                                var prefixQuery = new PrefixQuery( new Term( field.Name, queryTerm.ToLower() ) )
                                {
                                    Boost = field.Boost
                                };

                                innerQuery.Add( prefixQuery, Occur.SHOULD );
                            }

                            wildcardQuery.Add( innerQuery, Occur.MUST );
                        }

                        luceneQuery.Add( wildcardQuery, Occur.MUST );

                        // Add a search for all the words as one single search term.
                        foreach ( var field in fieldsToSearch )
                        {
                            var phraseQuery = new PhraseQuery
                            {
                                new Term( field.Name, searchTerm.Text.ToLower() ),
                            };

                            phraseQuery.Boost = field.Boost;

                            luceneQuery.Add( phraseQuery, Occur.SHOULD );
                        }
                    }
                }
                else if ( item is SearchField searchField )
                {
                    // Add field filter
                    var phraseQuery = new PhraseQuery();

                    foreach ( var word in searchField.Value.Split( ' ' ) )
                    {
                        phraseQuery.Add( new Term( searchField.Name, word.ToLower() ) );
                    }

                    BooleanClause booleanClause = new BooleanClause( phraseQuery, occur );
                    booleanClause.Query.Boost = ( float ) searchField.Boost;
                    luceneQuery.Add( booleanClause );
                }
                else if ( item is SearchAnyMatch )
                {
                    luceneQuery.Add( new MatchAllDocsQuery(), occur );
                }
                else if ( item is SearchQuery subQuery )
                {
                    var luceneSubQuery = GetLuceneQuery( subQuery, index );

                    if ( luceneSubQuery != null )
                    {
                        luceneQuery.Add( luceneSubQuery, occur );
                    }
                }
            }

            return luceneQuery.Clauses.Count > 0 ? luceneQuery : null;
        }

        /// <summary>
        /// Gets the lucene sort that matches the requested sort order.
        /// </summary>
        /// <param name="sortOrder">The sort order definition.</param>
        /// <param name="isDescending">if set to <c>true</c> if the sort order is descending.</param>
        /// <returns>The sort object that will be used in the Lucene query.</returns>
        private static Sort GetLuceneSort( SearchSortOrder sortOrder, bool isDescending )
        {
            if ( sortOrder == SearchSortOrder.RelevantDate )
            {
                return new Sort( new SortField( nameof( IndexDocumentBase.RelevanceDateTime ), SortFieldType.STRING, isDescending ) );
            }
            else if ( sortOrder == SearchSortOrder.Trending )
            {
                return new Sort(
                    new SortField( nameof( IndexDocumentBase.IsTrending ), SortFieldType.STRING, true ),
                    new SortField( nameof( IndexDocumentBase.TrendingRank ), SortFieldType.INT32, isDescending ),
                    new SortField( null, SortFieldType.SCORE, isDescending )
                );
            }
            else if ( sortOrder == SearchSortOrder.Alphabetical )
            {
                return new Sort(
                    new SortField( nameof( IndexDocumentBase.NameSort ), SortFieldType.STRING, isDescending ),
                    new SortField( null, SortFieldType.SCORE, isDescending )
                );
            }
            else
            {
                return new Sort( new SortField( null, SortFieldType.SCORE, !isDescending ) );
            }
        }

        #endregion

        #region Document Deletion

        /// <inheritdoc/>
        public override Task DeleteMatchingDocumentsAsync( Type documentType, SearchQuery query )
        {
            var indexName = GetIndexName( documentType );
            var booleanQuery = new BooleanQuery();

            // Add the document type to the query.
            var documentQuery = new PhraseQuery
            {
                new Term( LuceneTypeField, indexName.ToLower() )
            };

            booleanQuery.Add( documentQuery, Occur.MUST );

            // Add in the custom query.
            var luceneQuery = GetLuceneQuery( query, null );

            if ( luceneQuery != null )
            {
                booleanQuery.Add( luceneQuery, Occur.MUST );
            }

            using ( var activeWriter = OpenWriter() )
            { 
                activeWriter.Writer.DeleteDocuments( booleanQuery );
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override Task DeleteMatchingDocumentsAsync( SearchQuery query )
        {
            // Add in the custom query.
            var luceneQuery = GetLuceneQuery( query, null );

            // Don't allow them to delete the entire index database.
            if ( luceneQuery == null )
            {
                return Task.CompletedTask;
            }

            using ( var activeWriter = OpenWriter() )
            {
                activeWriter.Writer.DeleteDocuments( luceneQuery );
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override Task DeleteIndexAsync( Type documentType )
        {
            var indexName = GetIndexName( documentType );

            // Add the document type to the query.
            var documentQuery = new PhraseQuery
            {
                new Term( LuceneTypeField, indexName.ToLower() )
            };

            using ( var activeWriter = OpenWriter() )
            {
                activeWriter.Writer.DeleteDocuments( documentQuery );
            }

            return Task.CompletedTask;
        }

        #endregion

        #region Document Retrieval and Searching

        /// <inheritdoc/>
        public override Task<SearchResults> SearchAsync( SearchQuery query, SearchOptions options = null )
        {
            options = options ?? new SearchOptions();

            // This isn't technically correct, but it will be close enough for
            // our purposes. Sub-classes are not currently allowed to define
            // their own searchable properties so this will give us the correct
            // boost information we need.
            var index = GetDocumentIndex( typeof( IndexDocumentBase ) );

            var queryContainer = GetLuceneQuery( query, index ) ?? new MatchAllDocsQuery();
            var returnSize = options.MaxResults ?? 10;

            if ( !OpenReader() )
            {
                // Issue opening index. Most likely cause is the index is empty so return an empty results set.
                return Task.FromResult( SearchResults.Empty );
            }

            TopDocs topDocs = null;
            if ( options.Offset.HasValue )
            {
                // Only support up to 10 pages worth of results.
                if ( options.Offset > returnSize * 10 )
                {
                    return Task.FromResult( SearchResults.Empty );
                }

                topDocs = _indexSearcher.Search( queryContainer, null, returnSize * 10, GetLuceneSort( options.Order, options.IsDescending ), true, false );
            }
            else
            {
                topDocs = _indexSearcher.Search( queryContainer, null, returnSize, GetLuceneSort( options.Order, options.IsDescending ), true, false );
            }

            if ( topDocs == null )
            {
                return Task.FromResult( SearchResults.Empty );
            }

            var docs = topDocs.ScoreDocs
                .Skip( options.Offset ?? 0 )
                .Take( returnSize )
                .Select( d => LuceneDocToIndexModel( queryContainer, d ) )
                .Where( d => d != null )
                .ToList();

            return Task.FromResult( new SearchResults
            {
                TotalResultsAvailable = topDocs.TotalHits,
                Documents = docs
            } );
        }

        #endregion

        #region Document Indexing

        /// <inheritdoc/>
        public override Task CreateIndexAsync( Type documentType, bool deleteIfExists = true )
        {
            using ( var activeWriter = OpenWriter() )
            {
                // Open the writer so it initializes the files.
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override Task IndexDocumentAsync<TDocument>( TDocument document )
        {
            try
            {
                Type documentType = document.GetType();
                var mappingType = GetIndexName( documentType );
                var index = GetDocumentIndex( documentType );
                var doc = new Document();

                // Index the known properties.
                foreach ( var typeMappingProperty in index.MappingProperties )
                {
                    var propertyValue = documentType.GetProperty( typeMappingProperty.Name ).GetValue( document, null );

                    if ( propertyValue is ICollection collectionProperty )
                    {
                        foreach ( var collectionValue in collectionProperty )
                        {
                            AddFieldToDocument( doc, typeMappingProperty, collectionValue );
                        }
                    }
                    else
                    {
                        AddFieldToDocument( doc, typeMappingProperty, propertyValue );
                    }
                }

                // Index any attributes or other non-property values that were defined.
                foreach ( var dynamicProperty in document.GetAdditionalMemberNames() )
                {
                    var propertyValue = document[dynamicProperty];

                    if ( propertyValue is ICollection collectionProperty )
                    {
                        foreach ( var collectionValue in collectionProperty )
                        {
                            var stringField = new StringField( dynamicProperty, collectionValue.ToStringSafe().ToLower(), global::Lucene.Net.Documents.Field.Store.YES );
                            doc.Add( stringField );
                        }
                    }
                    else
                    {
                        var stringField = new StringField( dynamicProperty, propertyValue.ToStringSafe().ToLower(), global::Lucene.Net.Documents.Field.Store.YES );
                        doc.Add( stringField );
                    }
                }

                string indexValue = LuceneID( mappingType, document.Id );
                doc.AddStringField( LuceneTypeField, mappingType, global::Lucene.Net.Documents.Field.Store.YES );
                doc.AddStringField( LuceneIdField, document.Id.ToString(), global::Lucene.Net.Documents.Field.Store.YES );
                doc.AddStringField( LuceneIndexField, indexValue, global::Lucene.Net.Documents.Field.Store.YES );

                // Stores all the properties as JSON to retrieve object on lookup.
                doc.AddStoredField( "JSON", document.ToJson() );

                // Use the analyzer in fieldAnalyzers if that field is in
                // that dictionary, otherwise use WhitespaceAnalyzer.
                var analyzer = new PerFieldAnalyzerWrapper( defaultAnalyzer: new WhitespaceAnalyzer( _matchVersion ), fieldAnalyzers: index.FieldAnalyzers );

                using ( var activeWriter = OpenWriter() )
                {
                    // Must specify analyzer because the default analyzer
                    // that is specified in indexWriterConfig is null.
                    activeWriter.Writer.UpdateDocument( new Term( LuceneIndexField, indexValue ), doc, analyzer );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
            }

            return Task.CompletedTask;
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// Property and Analyzer cache
        /// </summary>
        private class Index
        {
            /// <summary>
            /// Gets or sets the index type mapping properties.
            /// </summary>
            /// <value>
            /// The index type mapping properties.
            /// </value>
            public List<TypeMappingProperties> MappingProperties { get; set; } = new List<TypeMappingProperties>();

            /// <summary>
            /// Gets or sets the field analyzers.
            /// </summary>
            /// <value>
            /// The field analyzers.
            /// </value>
            public Dictionary<string, Analyzer> FieldAnalyzers { get; set; } = new Dictionary<string, Analyzer>();
        }

        /// <summary>
        /// Index type mapping properties
        /// </summary>
        private class TypeMappingProperties
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the boost.
            /// </summary>
            /// <value>
            /// The boost.
            /// </value>
            public float Boost { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this field property
            /// is used when performing general text searches.
            /// </summary>
            /// <value><c>true</c> if this field property is searched; otherwise, <c>false</c>.</value>
            public bool IsSearched { get; set; }

            /// <inheritdoc cref="IndexFieldAttribute.FieldType"/>
            public IndexFieldType FieldType { get; set; }
        }

        private class ActiveIndexWriter: IDisposable
        {
            public IndexWriter Writer { get; }

            private Action _disposeCallback;

            public ActiveIndexWriter( IndexWriter writer, Action disposeCallback )
            {
                Writer = writer;
                _disposeCallback = disposeCallback;
            }

            public void Dispose()
            {
                _disposeCallback();
            }
        }

        #endregion
    }
}