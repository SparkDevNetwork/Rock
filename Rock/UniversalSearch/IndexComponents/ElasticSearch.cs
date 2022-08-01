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

    [BooleanField( "Include Explain",
        Key = AttributeKey.IncludeExplain,
        Description = "Set this to true if debugging and what to an Explain in the search results.",
        Category = "Advanced Settings",
        Order = 6 )]

    [Rock.SystemGuid.EntityTypeGuid( "97DACCE9-F397-4E7B-9596-783A233FCFCF" )]
    public class Elasticsearch : IndexComponent
    {
        // These attribute keys are also used by the Elasticsearch content collection
        // index component. If any changes are made here, they need to be updated
        // in that class too.
        private static class AttributeKey
        {
            public const string NodeURL = "NodeURL";
            public const string ShardCount = "ShardCount";
            public const string UserName = "UserName";
            public const string Password = "Password";
            public const string CertificateFingerprint = "CertificateFingerprint";
            public const string IncludeExplain = "IncludeExplain";
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
            // Reset the connection when the component settings are changed.
            ConnectToServer();

            // Notify the content collection index component that our settings
            // have been updated, since they share settings with us.
            Cms.ContentCollection.ContentIndexContainer.Instance
                .Components
                .Values
                .Select( c => c.Value )
                .Where( c => c.GetType() == typeof( Cms.ContentCollection.IndexComponents.Elasticsearch ) )
                .Cast<Cms.ContentCollection.IndexComponents.Elasticsearch>()
                .FirstOrDefault()
                ?.SettingsUpdated();

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

                    // Use same casing as CLR Property Names.
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

            // Check if index already exists.
            var existsResponse = _client.Indices.Exists( indexName );
            if ( !existsResponse.Exists )
            {
                CreateIndex( document.GetType() );
            }

            var indexResult = _client.IndexAsync( document, s => s.Index( indexName ) ).ContinueWith( a =>
            {
                if ( a.Exception != null )
                {
                    ExceptionLogService.LogException( a.Exception );
                }
            } );
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
            // Make sure this is an index document.
            if ( !typeof( IndexModelBase ).IsAssignableFrom( documentType ) )
            {
                return;
            }

            var indexName = documentType.Name.ToLower();

            // Check if index already exists.
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

            // Create a new index request.
            var createIndexRequest = new CreateIndexRequest( indexName );
            createIndexRequest.Settings = new IndexSettings();
            createIndexRequest.Settings.Analysis = new Analysis
            {
                Normalizers = new Normalizers(),
                Analyzers = new Analyzers()
            };

            /* 04/19/2022 MDP 
             
             ElasticSearch's 'whitespace' is now 'case-sensitive'. It was not case-senstive in 2.x. https://www.elastic.co/guide/en/elasticsearch/reference/master/analysis-analyzers.html.
             To make 'whitespace' case-insensitive, the docs say to create a custom analyser. https://www.elastic.co/guide/en/elasticsearch/reference/master/analysis-custom-analyzer.html

             So, we'll make an analyzer called "whitespace_lowercase" which is based on the 'whitespace' analyser with a 'lowercase' filter.
             
             */

            createIndexRequest.Settings.Analysis.Analyzers.Add( "whitespace_lowercase", new CustomAnalyzer { Tokenizer = "whitespace", Filter = new string[1] { "lowercase" } } );

            createIndexRequest.Settings.NumberOfShards = GetAttributeValue( AttributeKey.ShardCount ).AsInteger();

            var typeMapping = new TypeMapping();
            typeMapping.Dynamic = true;
            typeMapping.Properties = new Properties();

            createIndexRequest.Mappings = typeMapping;

            // Get properties from the model and add them to the index.
            // Attributes will be added dynamically as the documents are loaded.
            var modelProperties = documentType.GetProperties();

            foreach ( var propertyInfo in modelProperties )
            {
                var indexAttribute = propertyInfo.GetCustomAttribute<RockIndexField>();
                if ( indexAttribute != null )
                {
                    bool nonStringIndexOption = true;
                    if ( indexAttribute.Type != IndexFieldType.String )
                    {
                        if ( indexAttribute.Index == IndexType.NotIndexed )
                        {
                            nonStringIndexOption = false;
                        }
                    }

                    switch ( indexAttribute.Type )
                    {
                        case IndexFieldType.Boolean:
                            {
                                typeMapping.Properties.Add( propertyInfo, new BooleanProperty()
                                {
                                    Name = propertyInfo,
                                    Index = nonStringIndexOption
                                } );
                                break;
                            }
                        case IndexFieldType.Date:
                            {
                                typeMapping.Properties.Add( propertyInfo, new DateProperty()
                                {
                                    Name = propertyInfo,
                                    Index = nonStringIndexOption,

                                    // Our DateTime data gets serialized as '2022-04-11T16:40:47.4070819'
                                    // But by default ElasticSearch wants the Z portion (DateTimeOffset)
                                    // Since we don't use DateTimeOffset, we'll add a couple more options for how query DateTime terms are processed
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
                                    Index = nonStringIndexOption
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

            if ( query.IsNullOrWhiteSpace() )
            {
                // Empty string shouldn't return any results.
                return documents;
            }

            // Leading/trailing space should not affect the query, so trim any.
            query = query.Trim();

            ISearchResponse<dynamic> results = null;
            List<SearchResultModel> searchResults = new List<SearchResultModel>();
            QueryContainer queryContainer = new QueryContainer();
            var searchDescriptor = new SearchDescriptor<IndexModelBase>().AllIndices();

            List<Type> indexModelTypes;
            List<EntityTypeCache> entityTypeList;

            if ( entities == null || entities.Count == 0 )
            {
                searchDescriptor = searchDescriptor.AllIndices();
                entityTypeList = EntityTypeCache.All().Where( e => e.IsIndexingSupported && e.IsIndexingEnabled && e.FriendlyName != "Site" ).ToList();
                indexModelTypes = entityTypeList.Select( a => a.IndexModelType ).ToList();
            }
            else
            {
                entityTypeList = new List<EntityTypeCache>();
                indexModelTypes = new List<Type>();
                var indexNameList = new List<IndexName>();
                foreach ( var entityId in entities )
                {
                    // Get entities search model name.
                    var entityType = EntityTypeCache.Get( entityId );
                    entityTypeList.Add( entityType );
                    indexModelTypes.Add( entityType.IndexModelType );
                    var indexName = entityType.IndexModelType.Name.ToLower();

                    if ( _client.Indices.Exists( indexName ).Exists )
                    {
                        indexNameList.Add( indexName );
                    }

                    // Check if this is a person model, if so we need to add two model types one for person and the other for businesses.
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

            var searchFields = GetSearchFields( query, indexModelTypes, entityTypeList );
            var emailSearchField = searchFields.FindBySearchFieldByName( nameof( PersonIndex.Email ) );
            var phoneNumbersSearchField = searchFields.FindBySearchFieldByName( nameof( PersonIndex.PhoneNumbers ) );
            var firstNameSearchField = searchFields.FindBySearchFieldByName( nameof( PersonIndex.FirstName ) );
            var nickNameSearchField = searchFields.FindBySearchFieldByName( nameof( PersonIndex.NickName ) );
            var lastNameSearchField = searchFields.FindBySearchFieldByName( nameof( PersonIndex.LastName ) );

            QueryContainer matchQuery = null;
            if ( fieldCriteria != null && fieldCriteria.FieldValues?.Count > 0 )
            {
                foreach ( var match in fieldCriteria.FieldValues )
                {
                    var searchField = searchFields.FindBySearchFieldByName( match.Field );
                    if ( searchField == null )
                    {
                        continue;
                    }

                    if ( fieldCriteria.SearchType == CriteriaSearchType.Or )
                    {
                        matchQuery |= new QueryStringQuery { Fields = searchField, Analyzer = "whitespace_lowercase", Query = match.Value, Boost = match.Boost };
                    }
                    else
                    {
                        matchQuery &= new QueryStringQuery { Fields = searchField, Analyzer = "whitespace_lowercase", Query = match.Value };
                    }
                }
            }

            switch ( searchType )
            {
                case SearchType.ExactMatch:
                    {
                        bool enablePhraseSearch = true;
                        QueryContainer exactMatchQuery = null;

                        var queryTerms = query.Split( ' ' ).Select( p => p.Trim() ).Where( a => a.IsNotNullOrWhiteSpace() ).ToList();

                        // Special logic to support emails.
                        if ( queryTerms.Count == 1 && query.Contains( "@" ) && emailSearchField != null )
                        {
                            exactMatchQuery |= new QueryStringQuery
                            {
                                // Do a exact search so that 'ted@rocksolidchurchdemo' does not find 'ted@rocksolidchurchdemo.com', but will only find if search is 'ted@rocksolidchurchdemo.com'.
                                Query = query,

                                // Use whitespace_lowercase to keep the email from being parsed into 3 variables because the @ will act as a delimiter by default
                                Analyzer = "whitespace_lowercase",
                                Fields = emailSearchField
                            };

                            enablePhraseSearch = false;
                        }
                        else
                        {
                            // We want to require an exact match of each of the terms to exists for a result to be returned.
                            var searchString = "+" + queryTerms.JoinStrings( " +" );

                            // Main ExactMatch Query, search all indexable fields
                            exactMatchQuery &= new QueryStringQuery { Query = searchString, Analyzer = "whitespace_lowercase", MinimumShouldMatch = "100%", Rewrite = MultiTermQueryRewrite.ScoringBoolean, Fields = searchFields, Fuzziness = Fuzziness.Auto };

                            // Add an additional 'OR' query if the query is just numeric. We'll see if there is a phone number that matches
                            if ( query.IsDigitsOnly() && phoneNumbersSearchField != null )
                            {
                                // Note that we store phone numbers in the form of Mobile^6235553322|Work^6235552444 in ElasticSearch, but
                                // ExactMatch will still work without wildcards because ^ and | are special chars.
                                exactMatchQuery |= new QueryStringQuery
                                {
                                    // Since this is 'Exact Match' search for exact matches.
                                    Query = query,
                                    AnalyzeWildcard = true,
                                    Fields = phoneNumbersSearchField
                                };
                            }
                        }

                        queryContainer &= exactMatchQuery;

                        // Add a search for all the words as one single search term.
                        if ( enablePhraseSearch )
                        {
                            queryContainer |= new QueryStringQuery
                            {
                                Query = query,
                                AnalyzeWildcard = true,
                                PhraseSlop = 0
                            };
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

                        if ( GetAttributeValue( AttributeKey.IncludeExplain ).AsBoolean() )
                        {
                            searchDescriptor = searchDescriptor.Explain();
                        }

                        results = _client.Search<dynamic>( searchDescriptor );
                        break;
                    }

                case SearchType.Fuzzy:
                case SearchType.Wildcard:
                    {
                        bool enablePhraseSearch = true;

                        /*  04/20/2022 MDP
                          
                          If this is SearchType.Fuzzy, it is the exact same thing as a wildcard query
                          except for setting some Fuzziness options and appending the special fuzzy indicator '~' to each term

                          see https://stackoverflow.com/a/58578354/1755417

                          Note that there is also a FuzzyQuery, but that only works for single terms, but we want a misspelled 'Ted Dekker' to find 'Ted Decker',
                        */

                        Fuzziness wildCardFuzziness;
                        MultiTermQueryRewrite fuzzyRewrite;
                        string fuzzyIndicator;
                        if ( searchType == SearchType.Fuzzy )
                        {
                            wildCardFuzziness = Fuzziness.Auto;
                            fuzzyRewrite = MultiTermQueryRewrite.TopTerms( size ?? 10 );
                            fuzzyIndicator = "~";
                        }
                        else
                        {
                            wildCardFuzziness = null;
                            fuzzyRewrite = null;
                            fuzzyIndicator = "";
                        }

                        QueryContainer wildcardQuery = null;

                        // Break each search term into a separate query terms.
                        // If this is a wildcard we'll append a * to the search term.
                        // If this is a fuzzy search, we'll also append a ~ to the search term.
                        var queryTerms = query.Split( ' ' ).Select( p => p.Trim() ).Where( a => a.IsNotNullOrWhiteSpace() ).ToList();

                        // special logic to support emails
                        if ( queryTerms.Count == 1 && query.Contains( "@" ) && emailSearchField != null )
                        {
                            wildcardQuery |= new QueryStringQuery
                            {
                                // Do a trailing wildcard search so that 'ted@rocksolidchurchdemo' finds 'ted@rocksolidchurchdemo.com'
                                Query = query + "*" + fuzzyIndicator,
                                Analyzer = "whitespace_lowercase",
                                Fields = emailSearchField,
                                Fuzziness = wildCardFuzziness,
                                FuzzyRewrite = fuzzyRewrite
                            };

                            enablePhraseSearch = false;
                        }
                        else
                        {
                            // We want to require each of the terms to exists for a result to be returned.
                            var searchString = "+" + queryTerms.JoinStrings( $"*{fuzzyIndicator} +" ) + "*" + fuzzyIndicator;

                            // Main WildCard Query, search all indexable fields.
                            wildcardQuery &= new QueryStringQuery { Query = searchString, Analyzer = "whitespace_lowercase", MinimumShouldMatch = "100%", Rewrite = MultiTermQueryRewrite.ScoringBoolean, Fields = searchFields };

                            // Add an additional 'OR' query with special logic to help boost last names if there are at least 2 terms.
                            // The boost=30 doesn't seem influence the score when it is a wildcard or fuzzy query. Open question on how we want that to work.
                            if ( queryTerms.Count > 1 )
                            {
                                QueryContainer nameQuery = null;

                                if ( lastNameSearchField != null && nickNameSearchField != null && firstNameSearchField != null )
                                {
                                    string wildcardIndicator = "*";
                                    var extraBoostedLastNameField = new Nest.Field( lastNameSearchField.Property, 30 );
                                    var assumedFirstNameTerm = queryTerms.First();
                                    var assumedLastNameTerm = queryTerms.Last();
                                    nameQuery &= new QueryStringQuery
                                    {
                                        Query = $"{assumedLastNameTerm}{wildcardIndicator}" + fuzzyIndicator,
                                        Analyzer = "whitespace_lowercase",
                                        Fields = extraBoostedLastNameField,
                                        Fuzziness = wildCardFuzziness,
                                        FuzzyRewrite = fuzzyRewrite
                                    };

                                    var firstNameSearchFields = new Nest.Field[2] { firstNameSearchField, nickNameSearchField };
                                    nameQuery &= new QueryStringQuery
                                    {
                                        Query = $"{assumedFirstNameTerm}{wildcardIndicator}" + fuzzyIndicator,
                                        Analyzer = "whitespace_lowercase",
                                        Fields = firstNameSearchFields,
                                        Fuzziness = wildCardFuzziness,
                                        FuzzyRewrite = fuzzyRewrite
                                    };

                                    wildcardQuery |= nameQuery;
                                }
                            }

                            // Add an additional 'OR' query if the query is just numeric. We'll see if there is a phone number that matches.
                            if ( query.IsDigitsOnly() && phoneNumbersSearchField != null )
                            {
                                wildcardQuery |= new QueryStringQuery
                                {
                                    // Find numbers that end with query term.
                                    // Note we store phone numbers in the form of "Mobile^6235553322|Work^6235552444",
                                    // However, the leading '*' wildcard will match the mobile '6235553322' and '3322'
                                    // cause ^ and | are treated as delimiters
                                    Query = $"*" + query + fuzzyIndicator,
                                    Analyzer = "whitespace_lowercase",
                                    Fields = phoneNumbersSearchField,
                                    Fuzziness = wildCardFuzziness,
                                    FuzzyRewrite = fuzzyRewrite
                                };
                            }
                        }

                        queryContainer &= wildcardQuery;

                        // Add an additional 'OR' search for all the words as one single search term.
                        if ( enablePhraseSearch )
                        {
                            var searchString = "+" + queryTerms.JoinStrings( fuzzyIndicator + " +" ) + fuzzyIndicator;
                            queryContainer |= new QueryStringQuery
                            {
                                Query = searchString,
                                AnalyzeWildcard = true,
                                PhraseSlop = 0,
                                Fields = searchFields,
                                Fuzziness = wildCardFuzziness,
                                FuzzyRewrite = fuzzyRewrite
                            };
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

                        if ( GetAttributeValue( AttributeKey.IncludeExplain ).AsBoolean() )
                        {
                            searchDescriptor = searchDescriptor.Explain();
                        }

                        results = _client.Search<dynamic>( searchDescriptor );

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
                IndexModelBase document = GetStrongTypedIndexModel( hit );
                if ( document == null )
                {
                    continue;
                }

                // If AttributeKey.IncludeExplain is enabled, we can get a bunch of info explaining why we got the scores and results that we did.
                document["Explain"] = hit.Explanation.ToJson();
                document.Score = hit.Score ?? 0.00;
                documents.Add( document );
            }

            if ( !results.IsValid && results.OriginalException != null )
            {
                ExceptionLogService.LogException( results.OriginalException );
                throw results.OriginalException;
            }

            return documents;
        }

        /// <summary>
        /// Gets the strongly typed object to represent the hit result. This
        /// will attempt to convert the object back to it's original object
        /// type if possible, otherwise as a generic IndexModelBase.
        /// </summary>
        /// <param name="hit">The hit result from a query.</param>
        /// <returns>An instance of <see cref="IndexModelBase"/> or a subclass of it, or <c>null</c> if the result could not be parsed.</returns>
        private static IndexModelBase GetStrongTypedIndexModel( IHit<dynamic> hit )
        {
            if ( !( hit.Source is JObject source ) )
            {
                return null;
            }

            try
            {
                var indexModelType = Type.GetType( $"{( string ) hit.Source["IndexModelType"]}, {( string ) hit.Source["IndexModelAssembly"]}" );

                if ( indexModelType != null )
                {
                    return ( IndexModelBase ) source.ToObject( indexModelType ); // return the source document as the derived type
                }
                else
                {
                    return source.ToObject<IndexModelBase>(); // return the source document as the base type
                }
            }
            catch
            {
                // ignore if the result if an exception resulted (most likely cause is getting a result from a non-rock index)
                return null;
            }
        }

        /// <summary>
        /// Gets the search fields. We specify them explicitly so that different boost levels per field can be specified.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="indexModelTypes">The index model types.</param>
        /// <param name="entityTypeList">The entity type list.</param>
        /// <returns>Nest.Field[].</returns>
        private static Nest.Field[] GetSearchFields( string query, List<Type> indexModelTypes, List<EntityTypeCache> entityTypeList )
        {
            // if the query is a single term, see if it can be interpreted as a Date, Number or Boolean. If so, include fields of that type.
            var queryIsDateTime = query.AsDateTime().HasValue;
            var queryIsNumber = query.AsIntegerOrNull().HasValue;
            var queryIsBoolean = query == "true" || query == "false";
            List<Nest.Field> searchFields = new List<Nest.Field>();
            foreach ( var indexModelType in indexModelTypes )
            {
                var properties = indexModelType.GetProperties().ToList();
                foreach ( var property in properties )
                {
                    var rockIndexFieldAttribute = property.GetCustomAttribute<RockIndexField>();
                    if ( rockIndexFieldAttribute != null )
                    {
                        if ( rockIndexFieldAttribute.Index == IndexType.Indexed )
                        {
                            // Only add search field if the query is a single term and can be compared as that data type.
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

            foreach ( var entityType in entityTypeList )
            {
                var indexableAttributes = AttributeCache.AllForEntityType( entityType.Id ).Where( a => a.IsIndexEnabled ).ToList();
                foreach ( var attribute in indexableAttributes )
                {
                    searchFields.Add( new Nest.Field( attribute.Key ) );
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

    internal static class ElasticSearchExtensions
    {
        internal static Nest.Field FindBySearchFieldByName( this IEnumerable<Nest.Field> searchFields, string fieldName )
        {
            return searchFields.Where( a => ( a.Property != null && a.Property.Name.Equals( fieldName, StringComparison.OrdinalIgnoreCase ) ) || ( a.Name.IsNotNullOrWhiteSpace() && a.Name.Equals( fieldName, StringComparison.OrdinalIgnoreCase ) ) ).FirstOrDefault();
        }

        internal static string GetFieldName( this Nest.Field nestField )
        {
            return nestField?.Property?.Name ?? nestField?.Name;
        }
    }
}

/* Developer Notes
  
   Forbidden characters in field names are shown in code at https://github.com/elastic/elasticsearch/blob/master/server/src/main/java/org/elasticsearch/cluster/metadata/MetadataCreateIndexService.java#L216
   They are:
       _ . , # : +

    Cluster APIs are at https://www.elastic.co/guide/en/elasticsearch/reference/current/cluster.html
        Cluster State API is https://www.elastic.co/guide/en/elasticsearch/reference/current/cluster-state.html
        Example: https://localhost:9200/_cluster/state/metadata,routing_table,blocks,version
*/