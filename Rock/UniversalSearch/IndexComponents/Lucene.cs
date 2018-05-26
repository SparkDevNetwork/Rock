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
using System.Text;
using System.Threading.Tasks;
using Rock.UniversalSearch.IndexModels;


using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.UniversalSearch.IndexModels.Attributes;
using Rock.Cache;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Analysis.Snowball;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Directory = Lucene.Net.Store.Directory;
using Lucene.Net.Store;
using System.IO;

namespace Rock.UniversalSearch.IndexComponents
{
    /// <summary>
    /// Elastic Search Index Provider
    /// </summary>
    /// <seealso cref="Rock.UniversalSearch.IndexComponent" />
    [Description( "Lucene.Net Universal Search Index (v4.8)" )]
    [Export( typeof( IndexComponent ) )]
    [ExportMetadata( "ComponentName", "Lucene.Net 4.8" )]

    [TextField( "Node URL", "The URL of the ElasticSearch node (http://myserver:9200)", true, key: "NodeUrl" )]
    [IntegerField( "Shard Count", "The number of shards to use for each index. More shards support larger databases, but can make the results less accurate. We recommend using 1 unless your database get's very large (> 50GB).", true, 1 )]
    public class Lucene : IndexComponent
    {
        private static readonly LuceneVersion MatchVersion = LuceneVersion.LUCENE_48;
        private static Dictionary<string, Index> Indexes = new Dictionary<string, Index>();
        private static IndexWriterConfig indexWriterConfig = null;
        private static IndexWriter writer = null;
        private static SearcherManager searcherManager = null;
        private static SearcherFactory searcherFactory = null;
        private static string path = null;
        private static FSDirectory _directory;

        public Lucene()
        {
            if (writer == null)
            {
                path = System.Web.HttpContext.Current.Server.MapPath( "~/App_Data/LuceneSearchIndex" );
                indexWriterConfig = new IndexWriterConfig( MatchVersion, null ); // No default Analyzer. Have to explicitly specify it when using IndexReader/IndexWriter
                indexWriterConfig.OpenMode = OpenMode.CREATE_OR_APPEND;
                writer = new IndexWriter( directory, indexWriterConfig );
                searcherFactory = new SearcherFactory();
                searcherManager = new SearcherManager( writer, true, searcherFactory );
            }
            
        }

        private static FSDirectory directory
        {
            get
            {
                if ( _directory == null )
                {
                    _directory = FSDirectory.Open( new DirectoryInfo( path ) );
                }

                return _directory;
            }
        }

        public override bool IsConnected
        {
            get
            {
                return writer != null;
            }
        }

        public override string IndexLocation
        {
            get
            {
                return path;

            }
        }

        public override void CreateIndex( Type documentType, bool deleteIfExists = true )
        {
            var indexName = documentType.Name.ToLower();

            object instance = Activator.CreateInstance( documentType );

            // check if index already exists, and delete existing copy if it exist
            if ( Indexes.ContainsKey( indexName ) )
            {
                Indexes.Remove( indexName );
            }

            Index index = new Index();

            // make sure this is an index document
            if ( instance is IndexModelBase )
            {
                // create a new index request
                Dictionary<string, TypeMappingProperties> typeMapping = new Dictionary<string, TypeMappingProperties>();

                // get properties from the model and add them to the index (hint: attributes will be added dynamically as the documents are loaded)
                var modelProperties = documentType.GetProperties();

                foreach ( var property in modelProperties )
                {
                    var indexAttributes = property.GetCustomAttributes( false );
                    var indexAttribute = property.GetCustomAttributes( typeof( RockIndexField ), false );
                    if ( indexAttribute.Length > 0 )
                    {
                        var attribute = (RockIndexField)indexAttribute[0];

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
                        typeMappingProperty.Boost = (float)attribute.Boost;

                        switch ( attribute.Type )
                        {
                            case IndexFieldType.Boolean:
                            case IndexFieldType.Date:
                            case IndexFieldType.Number:
                                {
                                    typeMappingProperty.IndexType = IndexType.NotAnalyzed;
                                    typeMappingProperty.Analyzer = null;
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
                    }
                }

                index.MappingProperties = typeMapping;
                Indexes[indexName] = index;
            }
        }

        /// <summary>
        /// Deletes the type of the documents by.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="indexName">Name of the index.</param>
        public override void DeleteDocumentsByType<T>( string indexName = null )
        {
            if ( indexName == null )
            {
                indexName = typeof( T ).Name.ToLower();
            }

            //Analyzer analyzer = (new StandardAnalyzer()).  SetupAnalyzer(); new Analyzer(); //new StandardAnalyzer();
            //IndexWriterConfig indexWriterConfig = new IndexWriterConfig( MatchVersion, analyzer );
            //using ( var writer = new IndexWriter( _directory, indexWriterConfig ) )
            //{
                IndexModelBase docIndexModelBase = document as IndexModelBase;
                writer.DeleteDocuments(new Query() );
            //}

            _client.DeleteByQueryAsync<T>( indexName, typeof( T ).Name.ToLower(), d => d.MatchAll() );
        }

        /// <summary>
        /// Deletes the document.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document">The document.</param>
        /// <param name="indexName">Name of the index.</param>
        public override void DeleteDocument<T>( T document, string indexName = null )
        {
            if ( indexName == null )
            {
                indexName = document.GetType().Name.ToLower();
            }

            _client.Delete<T>( document, d => d.Index( indexName ) );
        }

        /// <summary>
        /// Deletes the document by identifier.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        /// <param name="id">The identifier.</param>
        public override void DeleteDocumentById( Type documentType, int id )
        {
            this.DeleteDocumentByProperty( documentType, "id", id );
        }

        /// <summary>
        /// Deletes the document by property.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">The property value.</param>
        public override void DeleteDocumentByProperty( Type documentType, string propertyName, object propertyValue )
        {

            string jsonSearch = string.Format( @"{{
""term"": {{
      ""{0}"": {{
                ""value"": ""{1}""
      }}
        }}
}}", Char.ToLowerInvariant( propertyName[0] ) + propertyName.Substring( 1 ), propertyValue );

            var response = _client.DeleteByQuery<IndexModelBase>( documentType.Name.ToLower(), documentType.Name.ToLower(), qd => qd.Query( q => q.Raw( jsonSearch ) ) );
        }

        public override void DeleteIndex( Type documentType )
        {
            if ( typeMappings.ContainsKey( documentType.Name.ToLower() ) )
            {
                typeMappings.Remove( documentType.Name.ToLower() );
            }
        }

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
            throw new NotImplementedException();
        }
        public override void IndexDocument<T>( T document, string indexName = null, string mappingType = null )
        {
            if ( indexName == null )
            {
                indexName = document.GetType().Name.ToLower();
            }

            if ( mappingType == null )
            {
                mappingType = document.GetType().Name.ToLower();
            }

            if ( !Indexes.ContainsKey( mappingType ) )
            {
                CreateIndex( document.GetType() );
            }

            var index = Indexes[mappingType];

            Document doc = new Document();
            Dictionary<string, Analyzer> fieldAnalyzers = new Dictionary<string, Analyzer>();
            foreach ( var typeMappingProperty in index.MappingProperties.Values )
            {
                TextField textField = new TextField( typeMappingProperty.Name, document.GetType().GetProperty( typeMappingProperty.Name ).GetValue( document, null ).ToString(), global::Lucene.Net.Documents.Field.Store.YES );
                textField.Boost = typeMappingProperty.Boost;
                doc.Add( textField );
                if ( typeMappingProperty.Analyzer.ToLowerInvariant() == "snowball" )
                {
                    fieldAnalyzers[typeMappingProperty.Name] = Analyzer.NewAnonymous( createComponents: ( fieldName, reader ) =>
                    {
                        var tokenizer = new StandardTokenizer( MatchVersion, reader );
                        var sbpff = new SnowballPorterFilterFactory( new Dictionary<string, string>() { { "language", "English" } } );
                        TokenStream result = sbpff.Create( new StandardTokenizer( MatchVersion, reader ) );
                        return new TokenStreamComponents( tokenizer, result ); // https://github.com/apache/lucenenet/blob/master/src/Lucene.Net.Analysis.Common/Analysis/Snowball/SnowballAnalyzer.cs
                        } );
                }
                else if ( typeMappingProperty.Analyzer.ToLowerInvariant() == "whitespace" )
                {
                    fieldAnalyzers[typeMappingProperty.Name] = Analyzer.NewAnonymous( createComponents: ( fieldName, reader ) =>
                    {
                        var tokenizer = new WhitespaceTokenizer( MatchVersion, reader );
                        TokenStream result = new StandardFilter( MatchVersion, tokenizer );
                        return new TokenStreamComponents( tokenizer, result );
                    } );
                }
            }

            // Stores all the properties as JSON to retreive object on lookup
            doc.AddStoredField( "JSON", document.ToJson());

            // Use the analyzer in fieldAnalyzers if that field is in that dictionary, otherwise use StandardAnalyzer.
            PerFieldAnalyzerWrapper analyzer = new PerFieldAnalyzerWrapper( defaultAnalyzer: new StandardAnalyzer( MatchVersion ), fieldAnalyzers: fieldAnalyzers );

            IndexModelBase docIndexModelBase = document as IndexModelBase;
            writer.UpdateDocument( new Term( indexName, docIndexModelBase.Id.ToString() ), doc, analyzer ); // Must specify analyzer because the default analyzer that is specified in indexWriterConfig is null.
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
        /// <returns></returns>
        public override List<IndexModelBase> Search( string query, SearchType searchType = SearchType.Wildcard, List<int> entities = null, SearchFieldCriteria fieldCriteria = null, int? size = null, int? from = null )
        {
            long totalResultsAvailable = 0;
            return Search( query, searchType, entities, fieldCriteria, size, from, out totalResultsAvailable );
        }

        /// <summary>
        /// Supportses the index field filtering.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        protected bool SupportsIndexFieldFiltering( Type entityType )
        {
            if ( entityType != null )
            {
                object classInstance = Activator.CreateInstance( entityType, null );
                MethodInfo bulkItemsMethod = entityType.GetMethod( "SupportsIndexFieldFiltering" );

                if ( classInstance != null && bulkItemsMethod != null )
                {
                    return (bool)bulkItemsMethod.Invoke( classInstance, null );
                }
            }

            return false;
        }

        private void AddOrAndString( ref StringBuilder subQueryString, CriteriaSearchType SearchType )
        {
            if ( SearchType == CriteriaSearchType.Or )
            {
                if ( subQueryString.Length != 0 )
                {
                    subQueryString.Append( " OR " );
                }
            }
            else
            {
                if ( subQueryString.Length != 0 )
                {
                    subQueryString.Append( " AND " );
                }
            }
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

            if ( writer != null )
            {
                    IndexSearcher indexSearcher = searcherManager.Acquire();
                    QueryParser queryParser = new QueryParser( MatchVersion, f, a );
                    QueryParser queryParser2 = new MultiFieldQueryParser( MatchVersion, new[] { "name", "description", "readme" }, analyzer );
                var _query = queryParser.Parse( queryString );
                var topDocs = indexSearcher.Search( _query, 10 );
                int _totalHits = topDocs.TotalHits;
                foreach ( var result in topDocs.ScoreDocs )
                {
                    var doc = indexSearcher.Doc( result.Doc );
                    l.Add( new SearchResult
                    {
                        Name = doc.GetField( "name" )?.GetStringValue(),
                        Description = doc.GetField( "description" )?.GetStringValue(),
                        Url = doc.GetField( "url" )?.GetStringValue(),

                        // Results are automatically sorted by relevance
                        Score = result.Score,
                    } );
                }
                BooleanQuery booleanQuery = new BooleanQuery();




                List<SearchResultModel> searchResults = new List<SearchResultModel>();
                string queryString = string.Empty;
                //QueryContainer queryContainer = new QueryContainer(); -> Elasticsearch

                // add and field constraints
                // var searchDescriptor = new SearchDescriptor<dynamic>().AllIndices(); -> Elasticsearch

                if ( entities == null || entities.Count == 0 )
                {
                    searchDescriptor = searchDescriptor.AllTypes();
                }
                else
                {
                    var entityTypes = new List<string>();
                    foreach ( var entityId in entities )
                    {
                        // get entities search model name
                        var entityType = new EntityTypeService( new RockContext() ).Get( entityId );
                        entityTypes.Add( entityType.IndexModelType.Name.ToLower() );

                        // check if this is a person model, if so we need to add two model types one for person and the other for businesses
                        // wish there was a cleaner way to do this
                        if ( entityType.Guid == SystemGuid.EntityType.PERSON.AsGuid() )
                        {
                            entityTypes.Add( "businessindex" );
                        }
                    }

                    searchDescriptor = searchDescriptor.Type( string.Join( ",", entityTypes ) ); // todo: consider adding indexmodeltype to the entity cache
                }

                // QueryContainer matchQuery = null; -> Elasticsearch
                StringBuilder matchQuery = new StringBuilder();
                if ( fieldCriteria != null && fieldCriteria.FieldValues?.Count > 0 )
                {
                    foreach ( var match in fieldCriteria.FieldValues )
                    {
                        AddOrAndString( ref matchQuery, fieldCriteria.SearchType );
                        if ( match.Boost == 1 )
                        {
                            matchQuery.Append( string.Format( "{0}:\"{1}\"", match.Field, match.Value ) );
                        }
                        else
                        {
                            matchQuery.Append( string.Format( "{0}:\"{1}\"^{2}", match.Field, match.Value, match.Boost ) );
                        }

                    }
                }

                StringBuilder queryContainer = new StringBuilder();
                switch ( searchType )
                {
                    case SearchType.ExactMatch:
                        {
                            if ( !string.IsNullOrWhiteSpace( query ) )
                            {
                                AddOrAndString( ref queryContainer, fieldCriteria.SearchType );
                                queryContainer.Append( query );
                                // https://stackoverflow.com/questions/15170097/how-to-search-across-all-the-fields
                                // https://stackoverflow.com/questions/1186790/in-lucene-net-can-we-search-for-a-content-without-giving-field-name-and-it-will/10910110#10910110
                                queryContainer &= new QueryStringQuery { Query = query, AnalyzeWildcard = true };
                            }

                            // special logic to support emails
                            if ( query.Contains( "@" ) )
                            {
                                queryContainer |= new QueryStringQuery { Query = "email:" + query, Analyzer = "whitespace" }; // analyzer = whitespace to keep the email from being parsed into 3 variables because the @ will act as a delimitor by default
                            }

                            // special logic to support phone search
                            if ( query.IsDigitsOnly() )
                            {
                                queryContainer |= new QueryStringQuery { Query = "phone:*" + query + "*", AnalyzeWildcard = true };
                            }

                            // add a search for all the words as one single search term
                            queryContainer |= new QueryStringQuery { Query = query, AnalyzeWildcard = true, PhraseSlop = 0 };

                            if ( matchQuery != null )
                            {
                                queryContainer &= matchQuery; // queryContainer = queryContainer AND ( subquery )
                            }

                            if ( size.HasValue )
                            {
                                searchDescriptor.Size( size.Value );
                            }

                            if ( from.HasValue )
                            {
                                searchDescriptor.From( from.Value );
                            }

                            searchDescriptor.Query( q => queryContainer );

                            topDocs = _client.Search<dynamic>( searchDescriptor ); / / indexSearcher.Search( _query, 10 );
                            break;
                        }
                    case SearchType.Fuzzy:
                        {
                            topDocs = _client.Search<dynamic>( d =>
                                        d.AllIndices().AllTypes()
                                        .Query( q =>
                                            q.Fuzzy( f => f.Value( query )
                                            .Rewrite( RewriteMultiTerm.TopTermsN ) )
                                        )
                                    ); / / indexSearcher.Search( _query, 10 )
                            break;
                        }
                    case SearchType.Wildcard:
                        {
                            bool enablePhraseSearch = true;

                            if ( !string.IsNullOrWhiteSpace( query ) )
                            {
                                QueryContainer wildcardQuery = null;

                                // break each search term into a separate query and add the * to the end of each
                                var queryTerms = query.Split( ' ' ).Select( p => p.Trim() ).ToList();

                                // special logic to support emails
                                if ( queryTerms.Count == 1 && query.Contains( "@" ) )
                                {
                                    wildcardQuery |= new QueryStringQuery { Query = "email:*" + query + "*", Analyzer = "whitespace" };
                                    enablePhraseSearch = false;
                                }
                                else
                                {
                                    foreach ( var queryTerm in queryTerms )
                                    {
                                        if ( !string.IsNullOrWhiteSpace( queryTerm ) )
                                        {
                                            wildcardQuery &= new QueryStringQuery { Query = queryTerm + "*", Analyzer = "whitespace", Rewrite = RewriteMultiTerm.ScoringBoolean }; // without the rewrite all results come back with the score of 1; analyzer of whitespaces says don't fancy parse things like check-in to 'check' and 'in'
                                        }
                                    }

                                    // add special logic to help boost last names
                                    if ( queryTerms.Count > 1 )
                                    {
                                        QueryContainer nameQuery = null;
                                        nameQuery &= new QueryStringQuery { Query = "lastName:" + queryTerms.Last() + "*", Analyzer = "whitespace", Boost = 30 };
                                        nameQuery &= new QueryStringQuery { Query = "firstName:" + queryTerms.First() + "*", Analyzer = "whitespace" };
                                        wildcardQuery |= nameQuery;
                                    }

                                    // special logic to support phone search
                                    if ( query.IsDigitsOnly() )
                                    {
                                        wildcardQuery |= new QueryStringQuery { Query = "phoneNumbers:*" + query, Analyzer = "whitespace" };
                                    }
                                }

                                queryContainer &= wildcardQuery;
                            }

                            // add a search for all the words as one single search term
                            if ( enablePhraseSearch )
                            {
                                queryContainer |= new QueryStringQuery { Query = query, AnalyzeWildcard = true, PhraseSlop = 0 };
                            }

                            if ( matchQuery != null )
                            {
                                queryContainer &= matchQuery;
                            }

                            if ( size.HasValue )
                            {
                                searchDescriptor.Size( size.Value );
                            }

                            if ( from.HasValue )
                            {
                                searchDescriptor.From( from.Value );
                            }

                            searchDescriptor.Query( q => queryContainer );

                            var indexBoost = CacheGlobalAttributes.Value( "UniversalSearchIndexBoost" );

                            if ( indexBoost.IsNotNullOrWhitespace() )
                            {
                                var boostItems = indexBoost.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                                foreach ( var boostItem in boostItems )
                                {
                                    var boostParms = boostItem.Split( new char[] { '^' } );

                                    if ( boostParms.Length == 2 )
                                    {
                                        int boost = 1;
                                        Int32.TryParse( boostParms[1], out boost );
                                        searchDescriptor.IndicesBoost( b => b.Add( boostParms[0], boost ) );
                                    }
                                }
                            }

                            results = _client.Search<dynamic>( searchDescriptor ); / / indexSearcher.Search( _query, 10 );
                            break;
                        }
                }

                totalResultsAvailable = topDocs.TotalHits;

                // normallize the results to rock search results
                if ( topDocs != null )
                {
                    foreach ( var hit in topDocs.ScoreDocs )
                    {
                        var doc = indexSearcher.Doc( hit.Doc );

                        IndexModelBase document = new IndexModelBase();

                        try
                        {
                            var hitJsonField = doc.GetField( "JSON" );
                            if ( hitJsonField != null )
                            {
                                string hitJson = hitJsonField.GetStringValue();
                                Type indexModelType = Type.GetType( $"{ ( (string)( (JObject)hitJson )["indexModelType"] )}, { ( (string)( (JObject)hitJson )["indexModelAssembly"] )}" );

                                if ( indexModelType != null )
                                {
                                    document = (IndexModelBase)( (JObject)hitJson ).ToObject( indexModelType ); // return the source document as the derived type
                                }
                                else
                                {
                                    document = ( (JObject)hitJson ).ToObject<IndexModelBase>(); // return the source document as the base type
                                }
                            }

                            // Todo: See if there is a value to assign to document["Explain"]

                            document.Score = hit.Score;

                            documents.Add( document );
                        }
                        catch { } // ignore if the result if an exception resulted (most likely cause is getting a result from a non-rock index)
                    }
                }
            }

            return documents;
        }

        private class Index
        {
            public Dictionary<string, TypeMappingProperties> MappingProperties { get; set; } = new Dictionary<string, TypeMappingProperties>();
        }

        private class TypeMappingProperties
        {
            public string Name { get; set; }

            public float Boost { get; set; }

            public IndexType IndexType { get; set; }

            public string Analyzer { get; set; }
        }
    }
}
