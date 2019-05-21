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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Snowball;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;

using Newtonsoft.Json.Linq;

using Rock.Data;
using Rock.Model;
using Rock.UniversalSearch.IndexModels;
using Rock.UniversalSearch.IndexModels.Attributes;
using Rock.Web.Cache;

namespace Rock.UniversalSearch.IndexComponents
{
    /// <summary>
    /// Lucene Search Index Provider
    /// </summary>
    /// <seealso cref="Rock.UniversalSearch.IndexComponent" />
    [Description( "Lucene.Net Universal Search Index (v4.8)" )]
    [Export( typeof( IndexComponent ) )]
    [ExportMetadata( "ComponentName", "Lucene.Net 4.8" )]

    public class Lucene : IndexComponent
    {
        #region Private Fields
        private static readonly LuceneVersion _matchVersion = LuceneVersion.LUCENE_48;
        private static ConcurrentDictionary<string, Index> _indexes = new ConcurrentDictionary<string, Index>();
        private static IndexWriterConfig _indexWriterConfig = null;
        private static IndexWriter _writer = null;
        private static DirectoryReader _reader = null;
        private static IndexSearcher _indexSearcher = null;
        private static FSDirectory _directory;
        private static Timer _timer = null;
        private static readonly string _path = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "App_Data", "LuceneSearchIndex" );
        private static readonly object _lockWriter = new object();
        #endregion

        #region Internal Methods
        /// <summary>
        /// Id used by Lucene.
        /// Note: Changing the formula result in the data not being able to be deleted / updated. When changing the formula, delete the files from the LuceneSearchIndex folder and then re-index.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mappingType">Type of the mapping.</param>
        /// <param name="id">The entity identifier.</param>
        /// <returns>Lucene Id</returns>
        private string LuceneID<T>( string mappingType, T id )
        {
            return $"{mappingType}_{id.ToString().PadLeft( 7, '0' )}";
        }

        /// <summary>
        /// Opens the index writer.
        /// When the writer is idle, then it will flush and close the index writer
        /// </summary>
        private void OpenWriter()
        {
            lock ( _lockWriter )
            {
                if ( _writer == null )
                {
                    // path = System.Web.HttpContext.Current.Server.MapPath( "~/App_Data/LuceneSearchIndex" ); // Do not work with jobs
                    _indexWriterConfig = new IndexWriterConfig( _matchVersion, null ) // No default Analyzer. Have to explicitly specify it when using IndexReader/IndexWriter
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

                if ( _timer == null )
                {
                    _timer = new Timer( new TimerCallback( CloseWriter ), null, 10000, 0 );
                }
                else
                {
                    _timer.Change( 10000, 0 );
                }
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
        /// Flush and closes the index writer.
        /// </summary>
        /// <param name="state">Timer state. Unused.</param>
        private void CloseWriter( object state )
        {
            lock ( _lockWriter )
            {
                FlushWriter();
                if ( _writer != null )
                {
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
        }

        /// <summary>
        /// Opens the index reader.
        /// </summary>
        private void OpenReader()
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
        }

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

        /// <summary>
        /// Combines the tags and analyzers of the different index types.
        /// </summary>
        /// <param name="entityTypes">The entity types.</param>
        /// <param name="combinedTags">The combined tags.</param>
        /// <param name="combinedFieldAnalyzers">The combined field analyzers.</param>
        private void CombineIndexTypes( List<Type> entityTypes, out List<string> combinedTags, out Dictionary<string, Analyzer> combinedFieldAnalyzers )
        {
            Dictionary<string, bool> combinedTagsDictionary = new Dictionary<string, bool>();
            combinedFieldAnalyzers = new Dictionary<string, Analyzer>();

            foreach ( var mappingType in entityTypes )
            {
                string mappingTypeName = mappingType.Name.ToLower();
                if ( !_indexes.ContainsKey( mappingTypeName ) )
                {
                    CreateIndex( mappingType );
                }

                var index = _indexes[mappingTypeName];

                foreach ( var typeMappingProperty in index.MappingProperties.Values )
                {
                    combinedTagsDictionary[typeMappingProperty.Name] = true;
                }

                foreach ( var fieldAnalyzer in index.FieldAnalyzers )
                {
                    combinedFieldAnalyzers[fieldAnalyzer.Key] = fieldAnalyzer.Value;
                }
            }

            combinedTags = combinedTagsDictionary.Keys.ToList();
        }

        /// <summary>
        /// Converts Lucene document to index model.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="hit">The scoredoc.</param>
        /// <returns>Index model</returns>
        private IndexModelBase LuceneDocToIndexModel( Query query, ScoreDoc hit )
        {
            var doc = _indexSearcher.Doc( hit.Doc );

            IndexModelBase document = new IndexModelBase();

            try
            {
                var hitJsonField = doc.GetField( "JSON" );
                if ( hitJsonField != null )
                {
                    string hitJson = hitJsonField.GetStringValue();
                    JObject jObject = JObject.Parse( hitJson );
                    Type indexModelType = Type.GetType( $"{ jObject["IndexModelType"].ToStringSafe() }, { jObject["IndexModelAssembly"].ToStringSafe() }" );

                    if ( indexModelType != null )
                    {
                        document = (IndexModelBase)jObject.ToObject( indexModelType ); // return the source document as the derived type
                    }
                    else
                    {
                        document = jObject.ToObject<IndexModelBase>(); // return the source document as the base type
                    }
                }

                Explanation explanation = _indexSearcher.Explain( query, hit.Doc );
                document["Explain"] = explanation.ToString();
                document.Score = hit.Score;

                return document;
            }
            catch { } // ignore if the result if an exception resulted (most likely cause is getting a result from a non-rock index)
            return null;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Lucene is connected when needed, so not needed
        /// </summary>
        /// <value>
        ///   Always return <c>true</c> because it connects as needed.
        /// </value>
        public override bool IsConnected
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the lucene path.
        /// </summary>
        /// <value>
        /// The lucene path.
        /// </value>
        public override string IndexLocation
        {
            get
            {
                return _path;

            }
        }
        #endregion

        #region Document Deletion
        /// <summary>
        /// Deletes the type of the documents by.
        /// </summary>
        /// <typeparam name="T">IndexModelBase</typeparam>
        /// <param name="indexName">Name of the index.</param>
        public override void DeleteDocumentsByType<T>( string indexName = null )
        {
            if ( indexName == null )
            {
                indexName = typeof( T ).Name.ToLower();
            }

            OpenWriter();
            lock ( _lockWriter )
            {
                if ( _writer != null )
                {
                    _writer.DeleteDocuments( new Term( "type", typeof( T ).Name.ToLower() ) );
                }
            }
        }

        /// <summary>
        /// Deletes the document.
        /// </summary>
        /// <typeparam name="T">IndexModelBase</typeparam>
        /// <param name="document">The document.</param>
        /// <param name="indexName">Name of the index.</param>
        public override void DeleteDocument<T>( T document, string indexName = null )
        {
            if ( indexName == null )
            {
                indexName = document.GetType().Name.ToLower();
            }

            IndexModelBase docIndexModelBase = document as IndexModelBase;
            OpenWriter();
            lock ( _lockWriter )
            {
                if ( _writer != null )
                {
                    _writer.DeleteDocuments( new Term( "index", LuceneID( document.GetType().Name.ToLower(), docIndexModelBase.Id ) ) );
                }
            }
        }

        /// <summary>
        /// Deletes the document by identifier.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        /// <param name="id">The identifier.</param>
        public override void DeleteDocumentById( Type documentType, int id )
        {
            OpenWriter();
            lock ( _lockWriter )
            {
                if ( _writer != null )
                {
                    _writer.DeleteDocuments( new Term( "index", LuceneID( documentType.Name.ToLower(), id ) ) );
                    _writer.Flush( true, true );
                    _writer.Commit();
                }
            }
        }

        /// <summary>
        /// Deletes the document by property.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">The property value.</param>
        public override void DeleteDocumentByProperty( Type documentType, string propertyName, object propertyValue )
        {
            OpenWriter();
            lock ( _lockWriter )
            {
                if ( _writer != null )
                {
                    BooleanQuery query = new BooleanQuery
                    {
                        { new TermQuery( new Term( "type", documentType.Name.ToLower() ) ), Occur.MUST },
                        { new TermQuery( new Term( propertyName, propertyValue.ToStringSafe() ) ), Occur.MUST }
                    };

                    _writer.DeleteDocuments( query );
                }
            }
        }

        /// <summary>
        /// Delete all documents by type.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        public override void DeleteIndex( Type documentType )
        {
            OpenWriter();
            lock ( _lockWriter )
            {
                if ( _writer != null )
                {
                    _writer.DeleteDocuments( new Term( "type", documentType.Name.ToLower() ) );
                }
            }
        }
        #endregion

        #region Document Retrieval and Searching
        /// <summary>
        /// Gets the document by identifier.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override IndexModelBase GetDocumentById( Type documentType, int id )
        {
            return GetDocumentById( documentType, id.ToString() );
        }

        /// <summary>
        /// Gets the document by identifier.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override IndexModelBase GetDocumentById( Type documentType, string id )
        {
            OpenReader();
            string mappingType = documentType.Name.ToLower();
            var query = new TermQuery( new Term( "index", LuceneID( mappingType, id ) ) );
            var docs = _indexSearcher.Search( query, 1 );

            if ( docs.TotalHits >= 1 )
            {
                return LuceneDocToIndexModel( query, docs.ScoreDocs[0] );
            }

            return null;
        }

        /// <summary>
        /// Searches the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="searchType">Type of the search.</param>
        /// <param name="entities">The entities.</param>
        /// <param name="fieldCriteria">The field criteria.</param>
        /// <param name="size">The size.</param>
        /// <param name="from">From.</param>
        /// <returns>Search results.</returns>
        public override List<IndexModelBase> Search( string query, SearchType searchType = SearchType.Wildcard, List<int> entities = null, SearchFieldCriteria fieldCriteria = null, int? size = null, int? from = null )
        {
            long totalResultsAvailable = 0;
            return Search( query, searchType, entities, fieldCriteria, size, from, out totalResultsAvailable );
        }

        /// <summary>
        /// Searches the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="searchType">Type of the search.</param>
        /// <param name="entities">The entities.</param>
        /// <param name="fieldCriteria">The field criteria.</param>
        /// <param name="size">The size.</param>
        /// <param name="from">From.</param>
        /// <param name="totalResultsAvailable">The total results available.</param>
        /// <returns></returns>
        public override List<IndexModelBase> Search( string query, SearchType searchType, List<int> entities, SearchFieldCriteria fieldCriteria, int? size, int? from, out long totalResultsAvailable )
        {
            List<IndexModelBase> documents = new List<IndexModelBase>();
            totalResultsAvailable = 0;
            bool allEntities = false;

            BooleanQuery queryContainer = new BooleanQuery();
            List<string> combinedFields = new List<string>();
            List<Type> indexModelTypes = new List<Type>();
            Dictionary<string, Analyzer> combinedFieldAnalyzers = new Dictionary<string, Analyzer>();

            using ( RockContext rockContext = new RockContext() )
            {
                var entityTypeService = new EntityTypeService( rockContext );
                if ( entities == null || entities.Count == 0 )
                {
                    //add all entities
                    allEntities = true;
                    var selectedEntityTypes = EntityTypeCache.All().Where( e => e.IsIndexingSupported && e.IsIndexingEnabled && e.FriendlyName != "Site" );

                    foreach ( var entityTypeCache in selectedEntityTypes )
                    {
                        entities.Add( entityTypeCache.Id );
                    }
                }

                foreach ( var entityId in entities )
                {
                    // get entities search model name
                    var entityType = entityTypeService.GetNoTracking( entityId );
                    indexModelTypes.Add( entityType.IndexModelType );

                    // check if this is a person model, if so we need to add two model types one for person and the other for businesses
                    // wish there was a cleaner way to do this
                    if ( entityType.Guid == SystemGuid.EntityType.PERSON.AsGuid() )
                    {
                        indexModelTypes.Add( typeof( BusinessIndex ) );
                    }
                }

                indexModelTypes = indexModelTypes.Distinct().ToList();
                CombineIndexTypes( indexModelTypes, out combinedFields, out combinedFieldAnalyzers );

                if ( entities != null && entities.Count != 0 && !allEntities )
                {
                    var indexModelTypesQuery = new BooleanQuery();
                    indexModelTypes.ForEach( f => indexModelTypesQuery.Add( new TermQuery( new Term( "type", f.Name.ToLower() ) ), Occur.SHOULD ) );
                    queryContainer.Add( indexModelTypesQuery, Occur.MUST );
                }
            }

            TopDocs topDocs = null;
            // Use the analyzer in fieldAnalyzers if that field is in that dictionary, otherwise use StandardAnalyzer.
            PerFieldAnalyzerWrapper analyzer = new PerFieldAnalyzerWrapper( defaultAnalyzer: new StandardAnalyzer( _matchVersion ), fieldAnalyzers: combinedFieldAnalyzers );

            if ( fieldCriteria != null && fieldCriteria.FieldValues?.Count > 0 )
            {
                Occur occur = fieldCriteria.SearchType == CriteriaSearchType.And ? Occur.MUST : Occur.SHOULD;
                foreach ( var match in fieldCriteria.FieldValues )
                {
                    BooleanClause booleanClause = new BooleanClause( new TermQuery( new Term( match.Field, match.Value ) ), occur );
                    booleanClause.Query.Boost = match.Boost;
                    queryContainer.Add( booleanClause );
                }
            }

            switch ( searchType )
            {
                case SearchType.ExactMatch:
                {
                    var wordQuery = new BooleanQuery();

                    if ( !string.IsNullOrWhiteSpace( query ) )
                    {
                        var words = query.Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
                        foreach ( var word in words )
                        {
                            var innerQuery = new BooleanQuery();
                            combinedFields.ForEach( f => innerQuery.Add( new PrefixQuery( new Term( f, word.ToLower() ) ), Occur.SHOULD ) );
                            wordQuery.Add( innerQuery, Occur.SHOULD );
                        }
                    }

                    if ( wordQuery.Count() != 0 )
                    {
                        queryContainer.Add( wordQuery, Occur.MUST );
                    }

                    // special logic to support emails
                    if ( query.Contains( "@" ) )
                    {
                        queryContainer.Add( new BooleanClause( new TermQuery( new Term( "Email", query ) ), Occur.SHOULD ) );
                    }

                    // special logic to support phone search
                    if ( query.IsDigitsOnly() )
                    {
                        queryContainer.Add( new BooleanClause( new WildcardQuery( new Term( "PhoneNumbers", "*" + query + "*" ) ), Occur.SHOULD ) );
                    }

                    // add a search for all the words as one single search term
                    foreach ( var field in combinedFields )
                    {
                        var phraseQuery = new PhraseQuery();
                        phraseQuery.Add( new Term( field, query.ToLower() ) );
                        queryContainer.Add( phraseQuery, Occur.SHOULD );
                    }

                    break;
                }
                case SearchType.Fuzzy:
                {
                    foreach ( var field in combinedFields )
                    {
                        queryContainer.Add( new FuzzyQuery( new Term( field, query.ToLower() ) ), Occur.SHOULD );
                    }

                    break;
                }
                case SearchType.Wildcard:
                {
                    bool enablePhraseSearch = true;

                    if ( !string.IsNullOrWhiteSpace( query ) )
                    {
                        BooleanQuery wildcardQuery = new BooleanQuery();

                        // break each search term into a separate query and add the * to the end of each
                        var queryTerms = query.Split( ' ' ).Select( p => p.Trim() ).ToList();

                        // special logic to support emails
                        if ( queryTerms.Count == 1 && query.Contains( "@" ) )
                        {
                            wildcardQuery.Add( new WildcardQuery( new Term( "Email", "*" + query.ToLower() + "*" ) ), Occur.SHOULD );
                            enablePhraseSearch = false;
                        }
                        else
                        {
                            foreach ( var queryTerm in queryTerms )
                            {
                                if ( !string.IsNullOrWhiteSpace( queryTerm ) )
                                {
                                    var innerQuery = new BooleanQuery();
                                    combinedFields.ForEach( f => innerQuery.Add( new PrefixQuery( new Term( f, queryTerm.ToLower() ) ), Occur.SHOULD ) );
                                    wildcardQuery.Add( innerQuery, Occur.MUST );
                                }
                            }

                            // add special logic to help boost last names
                            if ( queryTerms.Count() > 1 && ( indexModelTypes.Contains( typeof( PersonIndex ) ) || indexModelTypes.Contains( typeof( BusinessIndex ) ) ) )
                            {
                                BooleanQuery nameQuery = new BooleanQuery
                                    {
                                        { new PrefixQuery( new Term( "FirstName", queryTerms.First().ToLower() ) ), Occur.MUST },
                                        { new PrefixQuery( new Term( "LastName", queryTerms.Last().ToLower() ) ) { Boost = 30 }, Occur.MUST }
                                    };
                                wildcardQuery.Add( nameQuery, Occur.SHOULD );

                                nameQuery = new BooleanQuery
                                    {
                                        { new PrefixQuery( new Term( "NickName", queryTerms.First().ToLower() ) ), Occur.MUST },
                                        { new PrefixQuery( new Term( "LastName", queryTerms.Last().ToLower() ) ) { Boost = 30 }, Occur.MUST }
                                    };
                                wildcardQuery.Add( nameQuery, Occur.SHOULD );
                            }

                            // special logic to support phone search
                            if ( query.IsDigitsOnly() )
                            {
                                wildcardQuery.Add( new PrefixQuery( new Term( "PhoneNumbers", queryTerms.First().ToLower() ) ), Occur.SHOULD );
                            }
                        }

                        queryContainer.Add( wildcardQuery, Occur.MUST );
                    }

                    // add a search for all the words as one single search term
                    if ( enablePhraseSearch )
                    {
                        // add a search for all the words as one single search term
                        foreach ( var field in combinedFields )
                        {
                            var phraseQuery = new PhraseQuery();
                            phraseQuery.Add( new Term( field, query.ToLower() ) );
                            queryContainer.Add( phraseQuery, Occur.SHOULD );
                        }
                    }

                    break;
                }
            }

            int returnSize = 10;
            if ( size.HasValue )
            {
                returnSize = size.Value;
            }

            OpenReader();

            if ( from.HasValue )
            {
                TopScoreDocCollector collector = TopScoreDocCollector.Create( returnSize * 10, true ); // Search for 10 pages with returnSize entries in each page
                _indexSearcher.Search( queryContainer, collector );
                topDocs = collector.GetTopDocs( from.Value, returnSize );
            }
            else
            {
                topDocs = _indexSearcher.Search( queryContainer, returnSize );
            }

            totalResultsAvailable = topDocs.TotalHits;

            if ( topDocs != null )
            {
                foreach ( var hit in topDocs.ScoreDocs )
                {
                    var document = LuceneDocToIndexModel( queryContainer, hit );
                    if ( document != null )
                    {
                        documents.Add( document );
                    }
                }
            }

            return documents;
        }
        #endregion

        #region Document Indexing
        /// <summary>
        /// Creates the cache type mapping and analyzer.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        /// <param name="deleteIfExists">if set to <c>true</c> [delete if exists].</param>
        public override void CreateIndex( Type documentType, bool deleteIfExists = true )
        {
            try
            {
                var indexName = documentType.Name.ToLower();

                object instance = Activator.CreateInstance( documentType );

                // Check if index already exists. If it exists, no need to create it again
                if ( _indexes.ContainsKey( indexName ) )
                {
                    return;
                }

                Index index = new Index();

                // make sure this is an index document
                if ( instance is IndexModelBase )
                {
                    Dictionary<string, TypeMappingProperties> typeMapping = new Dictionary<string, TypeMappingProperties>();
                    Dictionary<string, Analyzer> fieldAnalyzers = new Dictionary<string, Analyzer>();

                    // get properties from the model and add them to the index (hint: attributes will be added dynamically as the documents are loaded)
                    var modelProperties = documentType.GetProperties();

                    foreach ( var property in modelProperties )
                    {
                        var indexAttribute = property.GetCustomAttributes( typeof( RockIndexField ), false );
                        if ( indexAttribute.Length > 0 )
                        {
                            var attribute = ( RockIndexField ) indexAttribute[0];

                            var propertyName = property.Name;

                            // rewrite non-string index option (would be nice if they made the enums match up...)
                            if ( attribute.Type != IndexFieldType.String )
                            {
                                if ( attribute.Index == IndexType.NotIndexed )
                                {
                                    continue;
                                }
                            }

                            var typeMappingProperty = new TypeMappingProperties();
                            typeMappingProperty.Name = propertyName;
                            typeMappingProperty.Boost = ( float ) attribute.Boost;

                            switch ( attribute.Type )
                            {
                                case IndexFieldType.Boolean:
                                case IndexFieldType.Date:
                                case IndexFieldType.Number:
                                    {
                                        typeMappingProperty.IndexType = IndexType.NotAnalyzed;
                                        typeMappingProperty.Analyzer = string.Empty;
                                        break;
                                    }
                                default:
                                    {
                                        typeMappingProperty.IndexType = attribute.Index;
                                        if ( !string.IsNullOrWhiteSpace( attribute.Analyzer ) )
                                        {
                                            typeMappingProperty.Analyzer = attribute.Analyzer;
                                        }

                                        break;
                                    }
                            }

                            typeMapping.Add( propertyName, typeMappingProperty );

                            if ( typeMappingProperty.Analyzer?.ToLowerInvariant() == "snowball" )
                            {
                                fieldAnalyzers[typeMappingProperty.Name] = Analyzer.NewAnonymous( createComponents: ( fieldName, reader ) =>
                                {
                                    var tokenizer = new StandardTokenizer( _matchVersion, reader );
                                    var sbpff = new SnowballPorterFilterFactory( new Dictionary<string, string>() { { "language", "English" } } );
                                    sbpff.Inform(new ClasspathResourceLoader( documentType ) );
                                    TokenStream result = sbpff.Create( new StandardTokenizer( _matchVersion, reader ) );
                                    return new TokenStreamComponents( tokenizer, result ); // https://github.com/apache/lucenenet/blob/master/src/Lucene.Net.Analysis.Common/Analysis/Snowball/SnowballAnalyzer.cs
                                } );
                            }
                            else if ( typeMappingProperty.Analyzer?.ToLowerInvariant() == "whitespace" )
                            {
                                fieldAnalyzers[propertyName] = Analyzer.NewAnonymous( createComponents: ( fieldName, reader ) =>
                                {
                                    var tokenizer = new WhitespaceTokenizer( _matchVersion, reader );
                                    TokenStream result = new StandardFilter( _matchVersion, tokenizer );
                                    return new TokenStreamComponents( tokenizer, result );
                                } );
                            }
                        }
                    }

                    index.MappingProperties = typeMapping;
                    index.FieldAnalyzers = fieldAnalyzers;
                    _indexes[indexName] = index;
                }
            }
            catch ( Exception ex )
            {
                HttpContext context2 = HttpContext.Current;
                ExceptionLogService.LogException( ex, context2 );
            }
        }

        /// <summary>
        /// Indexes the document.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document">The document.</param>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="mappingType">Type of the mapping.</param>
        public override void IndexDocument<T>( T document, string indexName = null, string mappingType = null )
        {
            try
            {
                Type documentType = document.GetType();
                if ( indexName == null )
                {
                    indexName = documentType.Name.ToLower();
                }

                if ( mappingType == null )
                {
                    mappingType = documentType.Name.ToLower();
                }

                if ( !_indexes.ContainsKey( mappingType ) )
                {
                    CreateIndex( documentType );
                }

                var index = _indexes[mappingType];

                Document doc = new Document();
                foreach ( var typeMappingProperty in index.MappingProperties.Values )
                {
                    TextField textField = new TextField( typeMappingProperty.Name, documentType.GetProperty( typeMappingProperty.Name ).GetValue( document, null ).ToStringSafe().ToLower(), global::Lucene.Net.Documents.Field.Store.YES );
                    textField.Boost = typeMappingProperty.Boost;
                    doc.Add( textField );
                }

                IndexModelBase docIndexModelBase = document as IndexModelBase;
                string indexValue = LuceneID( mappingType, docIndexModelBase.Id );
                doc.AddStringField( "type", mappingType, global::Lucene.Net.Documents.Field.Store.YES );
                doc.AddStringField( "id", docIndexModelBase.Id.ToString(), global::Lucene.Net.Documents.Field.Store.YES );
                doc.AddStringField( "index", indexValue, global::Lucene.Net.Documents.Field.Store.YES );
                doc.AddStoredField( "JSON", document.ToJson() ); // Stores all the properties as JSON to retreive object on lookup

                // Use the analyzer in fieldAnalyzers if that field is in that dictionary, otherwise use StandardAnalyzer.
                PerFieldAnalyzerWrapper analyzer = new PerFieldAnalyzerWrapper( defaultAnalyzer: new StandardAnalyzer( _matchVersion ), fieldAnalyzers: index.FieldAnalyzers );

                OpenWriter();
                lock ( _lockWriter )
                {
                    if ( _writer != null )
                    {
                        _writer.UpdateDocument( new Term( "index", indexValue ), doc, analyzer ); // Must specify analyzer because the default analyzer that is specified in indexWriterConfig is null.
                    }
                }
            }
            catch ( Exception ex )
            {
                HttpContext context2 = HttpContext.Current;
                ExceptionLogService.LogException( ex, context2 );
            }
        }
#endregion

/// <summary>
/// Dispose of the index from memory and close all files.
/// </summary>

#region Constructors and Destructors
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
        }
        #endregion

        #region Classes
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
            public Dictionary<string, TypeMappingProperties> MappingProperties { get; set; } = new Dictionary<string, TypeMappingProperties>();

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
            /// Gets or sets the type of the index.
            /// </summary>
            /// <value>
            /// The type of the index.
            /// </value>
            public IndexType IndexType { get; set; }

            /// <summary>
            /// Gets or sets the analyzer.
            /// </summary>
            /// <value>
            /// The analyzer.
            /// </value>
            public string Analyzer { get; set; }
        }
        #endregion
    }
}
