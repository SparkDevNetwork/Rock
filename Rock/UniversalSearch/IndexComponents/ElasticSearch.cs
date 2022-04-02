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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;

using Elasticsearch.Net;

using Nest;
using Nest.JsonNetSerializer;

using Newtonsoft.Json.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.UniversalSearch.IndexModels;
using Rock.UniversalSearch.IndexModels.Attributes;
using Rock.Web.Cache;

namespace Rock.UniversalSearch.IndexComponents
{
    /// <summary>
    /// Elastic Search Index Provider
    /// </summary>
    /// <seealso cref="Rock.UniversalSearch.IndexComponent" />
    [Description( "Elasticsearch Universal Search Index (v8.x)" )]
    [Export( typeof( IndexComponent ) )]
    [ExportMetadata( "ComponentName", "Elasticsearch 8.x" )]

    [TextField(
        "Node URL",
        Key = AttributeKey.NodeURL,
        Description = "The URL of the ElasticSearch node (http://myserver:9200)",
        IsRequired = true,
        Order = 0 )]

    [TextField(
        "UserName",
        Key = AttributeKey.UserName,
        Description = "If security is enabled, the username for login.",
        IsRequired = false,
        Order = 1 )]

    [TextField(
        "Password",
        Key = AttributeKey.Password,
        Description = "If security is enabled, the password for login.",
        IsPassword = true,
        IsRequired = false,
        Order = 2 )]

    [TextField(
        "Certificate Fingerprint",
        Key = AttributeKey.CertificateFingerprint,
        Description = "The Certificate Fingerprint (if required by server).",
        IsRequired = false,
        Order = 3 )]

    [IntegerField( "Shard Count",
        Key = AttributeKey.ShardCount,
        Description = "The number of shards to use for each index. More shards support larger databases, but can make the results less accurate. We recommend using 1 unless your database get's very large (> 50GB).",
        IsRequired = true,
        Order = 5 )]
    public class Elasticsearch : IndexComponent
    {
        private static class AttributeKey
        {
            public const string NodeURL = "NodeURL";
            public const string ShardCount = "ShardCount";
            public const string UserName = "UserName";
            public const string Password = "Password";
            public const string CertificateFingerprint = "CertificateFingerprint";
        }

        /// <summary>
        /// The Client
        /// </summary>
        protected ElasticClient _client;

        /// <summary>
        /// Gets a value indicating whether this instance is connected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </value>
        public override bool IsConnected
        {
            get
            {
                if ( _client == null )
                {
                    ConnectToServer();
                    if ( _client == null )
                    {
                        return false;
                    }
                }

                var pingResult = _client.Ping();

                return pingResult.IsValid;
            }
        }

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        public ElasticClient Client
        {
            get
            {
                return _client;
            }
        }

        /// <summary>
        /// Gets the index location.
        /// </summary>
        /// <value>
        /// The index location.
        /// </value>
        public override string IndexLocation
        {
            get
            {
                return GetAttributeValue( AttributeKey.NodeURL );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Elasticsearch" /> class.
        /// </summary>
        public Elasticsearch()
        {
            ConnectToServer();
        }

        /// <summary>
        /// Method that is called when attribute values are updated. Components can
        /// override this to perform any needed setup/validation based on current attribute
        /// values.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool ValidateAttributeValues( out string errorMessage )
        {
            // reset the connection when the component settings are changed
            ConnectToServer();

            return base.ValidateAttributeValues( out errorMessage );
        }

        /// <summary>
        /// Connects to server.
        /// </summary>
        protected virtual void ConnectToServer()
        {
            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.NodeURL ) ) )
            {
                try
                {
                    var node = new Uri( GetAttributeValue( AttributeKey.NodeURL ) );
                    var pool = new SingleNodeConnectionPool( new Uri( "http://localhost:9200" ) );

                    /* 04-01-2022 MDP

                       Make sure to use JsonNetSerializer. NEST's default serializer doesn't support inheritance on POCOs.

                    */

                    var config = new ConnectionSettings( new SingleNodeConnectionPool( node ), JsonNetSerializer.Default );

                    // use same casing as CLR Property Names
                    config.DefaultFieldNameInferrer( s => s );
                    config.DisableDirectStreaming();

                    var userName = GetAttributeValue( AttributeKey.UserName );
                    var password = GetAttributeValue( AttributeKey.Password );
                    if ( userName.IsNotNullOrWhiteSpace() )
                    {
                        config.BasicAuthentication( userName, password );
                    }

                    var certificateFingerprint = GetAttributeValue( AttributeKey.CertificateFingerprint );
                    if ( certificateFingerprint.IsNotNullOrWhiteSpace() )
                    {
                        config.CertificateFingerprint( certificateFingerprint );
                    }

                    _client = new ElasticClient( config );
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                }
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
            if ( indexName == null )
            {
                indexName = document.GetType().Name.ToLower();
            }

            if ( mappingType == null )
            {
                mappingType = document.GetType().Name.ToLower();
            }
            
            var indexResult = _client.IndexAsync( document, s => s.Index( indexName ) );
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

            // v2.3
            //_client.DeleteByQueryAsync<T>( indexName, typeof( T ).Name.ToLower(), d => d.MatchAll() );

            // v7.x ??
            _client.DeleteByQueryAsync<T>( d => d.Index( indexName ).MatchAll() );
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
        /// Creates the index.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        /// <param name="deleteIfExists">if set to <c>true</c> [delete if exists].</param>
        public override void CreateIndex( Type documentType, bool deleteIfExists = true )
        {
            // make sure this is an index document
            if ( !typeof( IndexModelBase ).IsAssignableFrom( documentType ) )
            {
                return;
            }

            var indexName = documentType.Name.ToLower();

            // check if index already exists
            var existsResponse = _client.Indices.Exists( indexName );

            if ( existsResponse.Exists )
            {
                if ( deleteIfExists )
                {
                    this.DeleteIndex( documentType );
                }
                else
                {
                    return;
                }
            }

            // create a new index request
            var createIndexRequest = new CreateIndexRequest( indexName );
            createIndexRequest.Settings = new IndexSettings();
            createIndexRequest.Settings.NumberOfShards = GetAttributeValue( "ShardCount" ).AsInteger();

            var typeMapping = new TypeMapping();
            typeMapping.Dynamic = true;
            typeMapping.Properties = new Properties();

            createIndexRequest.Mappings = typeMapping;

            // get properties from the model and add them to the index (hint: attributes will be added dynamically as the documents are loaded)
            var modelProperties = documentType.GetProperties();

            foreach ( var propertyInfo in modelProperties )
            {
                var indexAttributes = propertyInfo.GetCustomAttributes( false );
                var indexAttribute = propertyInfo.GetCustomAttributes( typeof( RockIndexField ), false );
                if ( indexAttribute.Length > 0 )
                {
                    var attribute = ( RockIndexField ) indexAttribute[0];

                    //var propertyName = Char.ToLowerInvariant( property.Name[0] ) + property.Name.Substring( 1 );

                    // rewrite non-string index option (would be nice if they made the enums match up...)
                    bool nsIndexOption = true;
                    if ( attribute.Type != IndexFieldType.String )
                    {
                        if ( attribute.Index == IndexType.NotIndexed )
                        {
                            nsIndexOption = false;
                        }
                    }

                    switch ( attribute.Type )
                    {
                        case IndexFieldType.Boolean:
                            {
                                typeMapping.Properties.Add( propertyInfo, new BooleanProperty()
                                {
                                    Name = propertyInfo,
                                    //Boost = attribute.Boost,
                                    Index = nsIndexOption
                                } );
                                break;
                            }
                        case IndexFieldType.Date:
                            {
                                typeMapping.Properties.Add( propertyInfo, new DateProperty()
                                {
                                    Name = propertyInfo,
                                    //Boost = attribute.Boost,
                                    Index = nsIndexOption
                                } );
                                break;
                            }
                        case IndexFieldType.Number:
                            {
                                typeMapping.Properties.Add( propertyInfo, new NumberProperty()
                                {
                                    Name = propertyInfo,
                                    //  Boost = attribute.Boost,
                                    Index = nsIndexOption
                                } );
                                break;
                            }
                        default:
                            {
                                var stringProperty = new TextProperty();
                                stringProperty.Name = propertyInfo;
                                //stringProperty.Boost = attribute.Boost;
                                stringProperty.Index = true;

                                if ( !string.IsNullOrWhiteSpace( attribute.Analyzer ) )
                                {
                                    stringProperty.Analyzer = attribute.Analyzer;
                                }

                                typeMapping.Properties.Add( propertyInfo, stringProperty );
                                break;
                            }
                    }
                }
            }

            var response = _client.Indices.Create( createIndexRequest );
        }

        /// <summary>
        /// Deletes the index.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        public override void DeleteIndex( Type documentType )
        {
            _client.Indices.Delete( documentType.Name.ToLower() );
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
            return Search( query, searchType, entities, fieldCriteria, size, from, out _ );
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
                    return ( bool ) bulkItemsMethod.Invoke( classInstance, null );
                }
            }

            return false;
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

            if ( _client != null )
            {
                ISearchResponse<dynamic> results = null;
                List<SearchResultModel> searchResults = new List<SearchResultModel>();

                QueryContainer queryContainer = new QueryContainer();

                // add and field constraints
                var searchDescriptor = new SearchDescriptor<dynamic>().AllIndices();

                List<Type> indexModelTypes;

                if ( entities == null || entities.Count == 0 )
                {
                    searchDescriptor = searchDescriptor.AllIndices();
                    indexModelTypes = EntityTypeCache.All().Where( e => e.IsIndexingSupported && e.IsIndexingEnabled && e.FriendlyName != "Site" ).Select( a => a.IndexModelType ).ToList();
                }
                else
                {
                    indexModelTypes = new List<Type>();
                    var indexNameList = new List<IndexName>();
                    foreach ( var entityId in entities )
                    {
                        // get entities search model name
                        var entityType = EntityTypeCache.Get( entityId );
                        indexModelTypes.Add( entityType.IndexModelType );
                        var indexName = entityType.IndexModelType.Name.ToLower();

                        if ( _client.Indices.Exists( indexName ).Exists )
                        {
                            indexNameList.Add( indexName );
                        }

                        // check if this is a person model, if so we need to add two model types one for person and the other for businesses
                        // wish there was a cleaner way to do this
                        if ( entityType.Guid == SystemGuid.EntityType.PERSON.AsGuid() )
                        {
                            if ( _client.Indices.Exists( "businessindex" ).Exists )
                            {
                                indexNameList.Add( "businessindex" );
                            }
                        }
                    }

                    searchDescriptor = searchDescriptor.Index( indexNameList.ToArray() );
                }

                QueryContainer matchQuery = null;
                if ( fieldCriteria != null && fieldCriteria.FieldValues?.Count > 0 )
                {
                    foreach ( var match in fieldCriteria.FieldValues )
                    {
                        if ( fieldCriteria.SearchType == CriteriaSearchType.Or )
                        {
                            matchQuery |= new MatchQuery { Field = match.Field, Query = match.Value, Boost = match.Boost };
                        }
                        else
                        {
                            matchQuery &= new MatchQuery { Field = match.Field, Query = match.Value };
                        }
                    }
                }
                /*
                else
                {
                    List<Nest.Field> fieldList = new List<Nest.Field>();
                    foreach ( var indexModelType in indexModelTypes )
                    {
                        foreach ( var property in indexModelType.GetProperties() )
                        {
                            var rockIndexFieldAttribute = property.GetCustomAttribute<RockIndexField>();
                            if ( rockIndexFieldAttribute != null )
                            {
                                if ( rockIndexFieldAttribute?.Index == IndexType.Indexed )
                                {
                                    var propertyName = Char.ToLowerInvariant( property.Name[0] ) + property.Name.Substring( 1 );
                                    fieldList.Add( new Nest.Field( propertyName, boost: rockIndexFieldAttribute.Boost ) );
                                }
                            }
                        }
                    }

                    matchQuery |= new MultiMatchQuery() { Fields = fieldList.ToArray() };
                }
                */


                switch ( searchType )
                {
                    case SearchType.ExactMatch:
                        {
                            if ( !string.IsNullOrWhiteSpace( query ) )
                            {
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

                            results = _client.Search<dynamic>( searchDescriptor );
                            break;
                        }

                    case SearchType.Fuzzy:
                        {
                            results = _client.Search<dynamic>( d =>
                                        d.AllIndices()
                                        .Query( q =>
                                            q.Fuzzy( f => f.Value( query )
                                                .Rewrite( MultiTermQueryRewrite.TopTerms( size ?? 10 ) ) ) ) );
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
                                    // We want to require each of the terms to exists for a result to be returned.
                                    var searchString = "+" + queryTerms.JoinStrings( "* +" ) + "*";
                                    wildcardQuery &= new QueryStringQuery { Query = searchString, Analyzer = "whitespace", MinimumShouldMatch = "100%", FuzzyRewrite = MultiTermQueryRewrite.ScoringBoolean };

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

                                // add a search for all the words as one single search term
                                if ( enablePhraseSearch )
                                {
                                    var searchString = "+" + queryTerms.JoinStrings( " +" );
                                    queryContainer |= new QueryStringQuery { Query = searchString, AnalyzeWildcard = true, PhraseSlop = 0 };
                                }
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

                            var globalIndexBoost = GlobalAttributesCache.Value( "UniversalSearchIndexBoost" );

                            if ( globalIndexBoost.IsNotNullOrWhiteSpace() )
                            {
                                var boostItems = globalIndexBoost.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
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

                            var resultsAsIndexModelBase = _client.Search<IndexModelBase>( searchDescriptor );
                            var resultsAsObject = _client.Search<object>( searchDescriptor );
                            var resultsAsDynamic = _client.Search<dynamic>( searchDescriptor );
                            results = _client.Search<dynamic>( searchDescriptor );
                            break;
                        }
                }

                if ( results == null )
                {
                    return documents;
                }

                totalResultsAvailable = results.Total;

                foreach ( var hit in results.Hits )
                {
                    IndexModelBase document = hit.Source;
                    if ( document == null )
                    {
                        continue;
                    }

                    document["Explain"] = hit.Explanation.ToJson();
                    document.Score = hit.Score ?? 0.00;
                    documents.Add( document );
                }
            }

            return documents;
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


            // v2.3
            //var response = _client.DeleteByQuery<IndexModelBase>( documentType.Name.ToLower(), documentType.Name.ToLower(), qd => qd.Query( q => q.Raw( jsonSearch ) ) );

            // v7
            var response = _client.DeleteByQuery<IndexModelBase>( qd =>
                qd.Index( documentType.Name.ToLower() ).Query( q => q.Raw( jsonSearch ) ) );
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
            var indexName = documentType.Name.ToLower();

            var request = new GetRequest( indexName, id ) { };

            var result = _client.Get<dynamic>( request );

            IndexModelBase document = new IndexModelBase();

            if ( result.Source != null )
            {
                Type indexModelType = Type.GetType( ( string ) ( ( JObject ) result.Source )["indexModelType"] );

                if ( indexModelType != null )
                {
                    document = ( IndexModelBase ) ( ( JObject ) result.Source ).ToObject( indexModelType ); // return the source document as the derived type
                }
                else
                {
                    document = ( ( JObject ) result.Source ).ToObject<IndexModelBase>(); // return the source document as the base type
                }
            }

            return document;
        }
    }
}

// forbidden characters in field names _ . , #
// cluster state: http://localhost:9200/_cluster/state?filter_nodes=false&filter_metadata=true&filter_routing_table=true&filter_blocks=true&filter_indices=true