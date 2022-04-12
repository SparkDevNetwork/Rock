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

using Rock.Attribute;
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
        Description = "The URL of the ElasticSearch node (https://myserver:9200)",
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
        Description = "The number of shards to use for each index. More shards support larger databases, but can make the results less accurate. We recommend using 1 unless your database gets very large (> 50GB).",
        DefaultIntegerValue = 1,
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
        /// Keep the created client around so we don't have to keep recreating it.
        /// Note that if any connection parameters change,
        /// ValidateAttributeValues will take care of re-creating the client.
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

                    /* 04-01-2022 MDP

                       Make sure to use JsonNetSerializer. NEST's default serializer doesn't support serializing/deserializing inherited classes.

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

                    var pingResult = _client.Ping();
                    if ( !pingResult.IsValid )
                    {
                        if ( pingResult.OriginalException != null )
                        {
                            ExceptionLogService.LogException( new Exception( "Error Connecting to ElasticSearch server", pingResult.OriginalException ) );
                        }
                    }
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
                var indexAttribute = propertyInfo.GetCustomAttribute<RockIndexField>();
                if ( indexAttribute != null )
                {
                    // rewrite non-string index option (would be nice if they made the enums match up...)
                    bool nsIndexOption = true;
                    if ( indexAttribute.Type != IndexFieldType.String )
                    {
                        if ( indexAttribute.Index == IndexType.NotIndexed )
                        {
                            nsIndexOption = false;
                        }
                    }

                    switch ( indexAttribute.Type )
                    {
                        case IndexFieldType.Boolean:
                            {
                                typeMapping.Properties.Add( propertyInfo, new BooleanProperty()
                                {
                                    Name = propertyInfo,
                                    Index = nsIndexOption
                                } );
                                break;
                            }
                        case IndexFieldType.Date:
                            {
                                typeMapping.Properties.Add( propertyInfo, new DateProperty()
                                {
                                    Name = propertyInfo,
                                    Index = nsIndexOption,

                                    // Our DateTime data gets serialized as '2022-04-11T16:40:47.4070819'
                                    // But by default ElasticSearch wants the Z portion (DateTimeOffset)
                                    // Since we don't use DateTimeOffset, we'll add a couple more options for how query terms are 
                                    // See all the formatting options here https://www.elastic.co/guide/en/elasticsearch/reference/current/mapping-date-format.html
                                    Format = $"{DateFormat.date_optional_time}||{DateFormat.epoch_millis}||MM-dd-yyyy||yyyy-MM-dd'T'HH:mm:ss.SSS"
                                } );
                                break;
                            }
                        case IndexFieldType.Number:
                            {
                                typeMapping.Properties.Add( propertyInfo, new NumberProperty()
                                {
                                    Name = propertyInfo,
                                    Index = nsIndexOption
                                } );
                                break;
                            }
                        default:
                            {
                                var stringProperty = new TextProperty();
                                stringProperty.Name = propertyInfo;
                                stringProperty.Index = true;

                                if ( !string.IsNullOrWhiteSpace( indexAttribute.Analyzer ) )
                                {
                                    stringProperty.Analyzer = indexAttribute.Analyzer;
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

            if ( _client == null )
            {
                return documents;
            }

            ISearchResponse<IndexModelBase> results = null;
            List<SearchResultModel> searchResults = new List<SearchResultModel>();

            QueryContainer queryContainer = new QueryContainer();

            // add and field constraints
            var searchDescriptor = new SearchDescriptor<IndexModelBase>().AllIndices();

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

            var searchFields = GetSearchFields( query, indexModelTypes );

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

            switch ( searchType )
            {
                case SearchType.ExactMatch:
                    {
                        if ( !string.IsNullOrWhiteSpace( query ) )
                        {
                            // Main ExactMatch Query, search all indexable fields
                            queryContainer &= new QueryStringQuery { Query = query, AnalyzeWildcard = true, Fields = searchFields };
                        }

                        // special logic to support emails
                        if ( query.Contains( "@" ) )
                        {
                            var emailSearchField = searchFields.Where( a => a.Property.Name == nameof( PersonIndex.Email ) ).FirstOrDefault();
                            queryContainer |= new QueryStringQuery
                            {
                                // exact match, no wildcard
                                Query = query,

                                // analyzer = whitespace to keep the email from being parsed into 3 variables because the @ will act as a delimiter by default
                                Analyzer = "whitespace",
                                Fields = emailSearchField,
                            };
                        }

                        // Add an additional 'OR' query if the query is just numeric. We'll see if there is a phone number that matches
                        if ( query.IsDigitsOnly() )
                        {
                            var phoneNumbersSearchField = searchFields.Where( a => a.Property.Name == nameof( PersonIndex.PhoneNumbers ) ).FirstOrDefault();
                            queryContainer |= new QueryStringQuery
                            {
                                // Find numbers that end with query term
                                Query = $"*" + query + "*",
                                AnalyzeWildcard = true,
                                Fields = phoneNumbersSearchField,
                            };
                        }

                        // add a search for all the words as one single search term
                        queryContainer |= new QueryStringQuery
                        {
                            Query = query,
                            AnalyzeWildcard = true,
                            PhraseSlop = 0
                        };

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

                        results = _client.Search<IndexModelBase>( searchDescriptor );
                        break;
                    }

                case SearchType.Fuzzy:
                    {
                        results = _client.Search<IndexModelBase>( d =>
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
                            var queryTerms = query.Split( ' ' ).Select( p => p.Trim() ).Where( a => a.IsNotNullOrWhiteSpace() ).ToList();

                            // special logic to support emails
                            if ( queryTerms.Count == 1 && query.Contains( "@" ) )
                            {
                                var emailSearchField = searchFields.Where( a => a.Property.Name == nameof( PersonIndex.Email ) ).FirstOrDefault();
                                wildcardQuery |= new QueryStringQuery
                                {
                                    Query = query + "*",
                                    Analyzer = "whitespace",
                                    Fields = emailSearchField,
                                };

                                enablePhraseSearch = false;
                            }
                            else
                            {
                                // We want to require each of the terms to exists for a result to be returned.
                                var searchString = "+" + queryTerms.JoinStrings( "* +" ) + "*";

                                // Main WildCard Query, search all indexable fields
                                wildcardQuery &= new QueryStringQuery { Query = searchString, Analyzer = "whitespace", MinimumShouldMatch = "100%", FuzzyRewrite = MultiTermQueryRewrite.ScoringBoolean, Fields = searchFields };


                                // add an additional 'OR' query with special logic to help boost last names if there are at least 2 terms
                                if ( queryTerms.Count > 1 )
                                {
                                    var firstNameSearchField = searchFields.Where( a => a.Property.Name == nameof( PersonIndex.FirstName ) ).FirstOrDefault();
                                    var lastNameSearchField = searchFields.Where( a => a.Property.Name == nameof( PersonIndex.LastName ) ).FirstOrDefault();
                                    QueryContainer nameQuery = null;

                                    if ( lastNameSearchField != null )
                                    {
                                        var extraBoostedLastNameField = new Nest.Field( lastNameSearchField.Property, 30 );
                                        nameQuery &= new QueryStringQuery
                                        {
                                            Query = $"{queryTerms.Last()}*",
                                            Analyzer = "whitespace",
                                            Fields = extraBoostedLastNameField,
                                        };
                                    }

                                    if ( firstNameSearchField != null )
                                    {
                                        nameQuery &= new QueryStringQuery
                                        {
                                            Query = $"{queryTerms.First()}*",
                                            Analyzer = "whitespace",
                                            Fields = firstNameSearchField
                                        };
                                    }

                                    wildcardQuery |= nameQuery;
                                }

                                // Add an additional 'OR' query if the query is just numeric. We'll see if there is a phone number that matches
                                if ( query.IsDigitsOnly() )
                                {
                                    var phoneNumbersSearchField = searchFields.Where( a => a.Property.Name == nameof( PersonIndex.PhoneNumbers ) ).FirstOrDefault();
                                    wildcardQuery |= new QueryStringQuery
                                    {
                                        // Find numbers that end with query term
                                        Query = $"*" + query,
                                        Analyzer = "whitespace",
                                        Fields = phoneNumbersSearchField,
                                    };
                                }
                            }

                            queryContainer &= wildcardQuery;

                            // add an additional 'OR' search for all the words as one single search term
                            if ( enablePhraseSearch )
                            {
                                var searchString = "+" + queryTerms.JoinStrings( " +" );
                                queryContainer |= new QueryStringQuery { Query = searchString, AnalyzeWildcard = true, PhraseSlop = 0, Fields = searchFields };
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

                        results = _client.Search<IndexModelBase>( searchDescriptor );

                        /* 04-12-2022 MDP

                        To see the RAW Json Request and Response that was POST'd to Elastic search
                        look at results.DebugInformation. This is useful to see the JSON of the query
                        that got constructed.

                        var debugInformation = results.DebugInformation;

                        */

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

            return documents;
        }

        /// <summary>
        /// Gets the search fields. We specify them explicitly so that different boost levels per field can be specified.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="indexModelTypes">The index model types.</param>
        /// <returns>Nest.Field[].</returns>
        private static Nest.Field[] GetSearchFields( string query, List<Type> indexModelTypes )
        {
            // if the query is a single term, see if it can be interpreted as a Date, Number or Boolean. If so, include fields of that type.
            var queryIsDateTime = query.AsDateTime().HasValue;
            var queryIsNumber = query.AsIntegerOrNull().HasValue;
            var queryIsBoolean = query == "true" || query == "false";
            List<Nest.Field> searchFields = new List<Nest.Field>();
            foreach ( var indexModelType in indexModelTypes )
            {
                foreach ( var property in indexModelType.GetProperties() )
                {
                    var rockIndexFieldAttribute = property.GetCustomAttribute<RockIndexField>();
                    if ( rockIndexFieldAttribute != null )
                    {
                        if ( rockIndexFieldAttribute.Index == IndexType.Indexed )
                        {
                            // only add search field if the query is a single term and can be compared as that data type
                            bool addField;
                            switch ( rockIndexFieldAttribute.Type )
                            {
                                case IndexFieldType.Date:
                                    {
                                        addField = queryIsDateTime;
                                    }
                                    break;

                                case IndexFieldType.Number:
                                    {
                                        addField = queryIsNumber;
                                    }
                                    break;
                                case IndexFieldType.Boolean:
                                    {
                                        addField = queryIsBoolean;
                                    }
                                    break;
                                default:
                                    {
                                        addField = true;
                                    }
                                    break;
                            }


                            if ( addField )
                            {
                                searchFields.Add( new Nest.Field( property, boost: rockIndexFieldAttribute.Boost ) );
                            }
                        }
                    }
                }
            }

            return searchFields.ToArray();
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
}}", propertyName, propertyValue );

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
            this.DeleteDocumentByProperty( documentType, $"{nameof( IndexModelBase.Id )}", id );
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

            var result = _client.Get<IndexModelBase>( request );

            var document = result.Source;

            return document;
        }
    }
}

// forbidden characters in field names _ . , #
// cluster state: http://localhost:9200/_cluster/state?filter_nodes=false&filter_metadata=true&filter_routing_table=true&filter_blocks=true&filter_indices=true