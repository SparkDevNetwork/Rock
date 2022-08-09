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
using System.Threading.Tasks;

using Elasticsearch.Net;

using Nest;
using Nest.JsonNetSerializer;

using Newtonsoft.Json.Linq;

using Rock.Cms.ContentCollection.Attributes;
using Rock.Cms.ContentCollection.IndexDocuments;
using Rock.Cms.ContentCollection.Search;
using Rock.Model;

namespace Rock.Cms.ContentCollection.IndexComponents
{
    /// <summary>
    /// Elastic Search Index Provider
    /// </summary>
    /// <seealso cref="Rock.Cms.ContentCollection.ContentIndexComponent" />
    [Description( "Elasticsearch Content Collection Index (v8.x)" )]
    [Export( typeof( ContentIndexComponent ) )]
    [ExportMetadata( "ComponentName", "Elasticsearch 8.x" )]

    [Rock.SystemGuid.EntityTypeGuid( "BFF0C2AF-A970-4452-80F8-75C6340F78CF" )]
    internal sealed class Elasticsearch : ContentIndexComponent
    {
        // These attribute keys are actually from the Elasticsearch component
        // in Universal Search.
        private static class AttributeKey
        {
            public const string NodeURL = "NodeURL";
            public const string ShardCount = "ShardCount";
            public const string UserName = "UserName";
            public const string Password = "Password";
            public const string CertificateFingerprint = "CertificateFingerprint";
            public const string IncludeExplain = "IncludeExplain";
        }

        #region Fields

        /// <summary>
        /// Keep the created client around so we don't have to keep recreating it.
        /// Note that if any connection parameters change,
        /// ValidateAttributeValues will take care of re-creating the client.
        /// </summary>
        private ElasticClient _client;

        /// <summary>
        /// The URL of the ElasticSearch node (https://myserver:9200)
        /// </summary>
        private string _nodeUrl;

        /// <summary>
        /// If security is enabled, the username for login.
        /// </summary>
        private string _userName;

        /// <summary>
        /// If security is enabled, the password for login.
        /// </summary>
        private string _password;

        /// <summary>
        /// The Certificate Fingerprint (if required by server).
        /// </summary>
        private string _certificateFingerprint;

        /// <summary>
        /// The number of shards to use for each index.
        /// </summary>
        private int _shardCount = 1;

        /// <summary>
        /// <c>true</c> if debugging and what an Explain field are included in the search results.
        /// </summary>
        private bool _includeExplain;

        #endregion

        #region Properties

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
        /// Gets the index location.
        /// </summary>
        /// <value>
        /// The index location.
        /// </value>
        public override string IndexLocation => _nodeUrl;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Elasticsearch" /> class.
        /// </summary>
        public Elasticsearch()
        {
            SettingsUpdated();

            ConnectToServer();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Called when the attribute value settings have been updated on the
        /// primary Elasticsearch component in Universal Search. Since we get
        /// our settings from there, we need to know when they change.
        /// </summary>
        internal void SettingsUpdated()
        {
            var component = UniversalSearch.IndexContainer
                .Instance
                .Components
                .Values
                .Select( c => c.Value )
                .Where( c => c.GetType() == typeof( UniversalSearch.IndexComponents.Elasticsearch ) )
                .Cast<UniversalSearch.IndexComponents.Elasticsearch>()
                .FirstOrDefault();

            if ( component == null )
            {
                return;
            }

            _nodeUrl = component.GetAttributeValue( AttributeKey.NodeURL );
            _userName = component.GetAttributeValue( AttributeKey.UserName );
            _password = component.GetAttributeValue( AttributeKey.Password );
            _certificateFingerprint = component.GetAttributeValue( AttributeKey.CertificateFingerprint );
            _shardCount = component.GetAttributeValue( AttributeKey.ShardCount ).AsInteger();
            _includeExplain = component.GetAttributeValue( AttributeKey.IncludeExplain ).AsBoolean();

            ConnectToServer();
        }

        /// <summary>
        /// Connects to server.
        /// </summary>
        private void ConnectToServer()
        {
            if ( _nodeUrl.IsNotNullOrWhiteSpace() )
            {
                try
                {
                    var node = new Uri( _nodeUrl );

                    /* 04-01-2022 MDP

                       Make sure to use JsonNetSerializer. NEST's default serializer doesn't support serializing/deserializing inherited classes.

                    */

                    var config = new ConnectionSettings( new SingleNodeConnectionPool( node ), JsonNetSerializer.Default );

                    // Use same casing as CLR Property Names.
                    config.DefaultFieldNameInferrer( s => s );
                    config.DisableDirectStreaming();

                    var userName = _userName;
                    var password = _password;
                    if ( userName.IsNotNullOrWhiteSpace() )
                    {
                        config.BasicAuthentication( userName, password );
                    }

                    if ( _certificateFingerprint.IsNotNullOrWhiteSpace() )
                    {
                        config.CertificateFingerprint( _certificateFingerprint );
                    }

                    var client = new ElasticClient( config );

                    var pingResult = client.Ping();
                    if ( !pingResult.IsValid )
                    {
                        if ( pingResult.OriginalException != null )
                        {
                            ExceptionLogService.LogException( new Exception( "Error Connecting to ElasticSearch server", pingResult.OriginalException ) );
                        }
                    }
                    else
                    {
                        _client = client;
                    }
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                }
            }
            else
            {
                _client = null;
            }
        }

        /// <inheritdoc/>
        protected override string GetIndexName( Type type )
        {
            return $"contentcollection_{base.GetIndexName( type )}";
        }

        /// <summary>
        /// Gets the search fields. We specify them explicitly so that
        /// different boost levels per field can be specified.
        /// </summary>
        /// <returns>An array of fields.</returns>
        private static Nest.Field[] GetSearchFields()
        {
            var searchFields = new List<Nest.Field>();

            // This isn't technically right, but since sub-classes are not currently
            // allowed to create their own searchable properties, it is close enough.
            var properties = typeof( IndexDocumentBase ).GetProperties().ToList();
            foreach ( var property in properties )
            {
                var indexAttribute = property.GetCustomAttribute<IndexFieldAttribute>();

                if ( indexAttribute == null || !indexAttribute.IsSearched )
                {
                    continue;
                }

                searchFields.Add( new Nest.Field( property, boost: indexAttribute.Boost ) );
            }

            return searchFields.ToArray();
        }

        /// <summary>
        /// Gets the strongly typed object to represent the hit result. This
        /// will attempt to convert the object back to it's original object
        /// type if possible, otherwise as a generic IndexDocumentBase.
        /// </summary>
        /// <param name="hit">The hit result from a query.</param>
        /// <returns>An instance of <see cref="IndexDocumentBase"/> or a subclass of it, or <c>null</c> if the result could not be parsed.</returns>
        private static IndexDocumentBase GetStrongTypedIndexModel( IHit<dynamic> hit )
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
                    // Return the source document as the derived type.
                    return ( IndexDocumentBase ) source.ToObject( indexModelType );
                }
                else
                {
                    // Return the source document as the base type.
                    return source.ToObject<IndexDocumentBase>();
                }
            }
            catch
            {
                // Ignore if the conversion resulted in an exception (most
                // likely cause is getting a result from a non-rock index).
                return null;
            }
        }

        /// <summary>
        /// Gets the Elasticsearch query that represents the Rock query.
        /// </summary>
        /// <param name="query">The Rock search query.</param>
        /// <param name="termFields">The fields that can be searched by common terms.</param>
        /// <returns>The query object or null if the rock query was empty.</returns>
        private static QueryContainer GetQuery( SearchQuery query, Nest.Field[] termFields )
        {
            if ( query == null )
            {
                return null;
            }

            QueryContainer queryContainer = null;

            // Process all the items in the query.
            foreach ( var item in query )
            {
                if ( item is SearchTerm searchTerm )
                {
                    if ( searchTerm.Text.IsNullOrWhiteSpace() || termFields == null )
                    {
                        continue;
                    }

                    var searchString = GetSearchString( searchTerm.Text, searchTerm.IsPhrase, searchTerm.IsWildcard );

                    // Main WildCard Query, search all indexable fields.
                    var qry = new QueryStringQuery
                    {
                        Query = searchString,
                        Analyzer = "whitespace_lowercase",
                        MinimumShouldMatch = "100%",
                        Rewrite = MultiTermQueryRewrite.ScoringBoolean,
                        Fields = termFields
                    };

                    if ( query.IsAllMatching )
                    {
                        queryContainer &= qry;
                    }
                    else
                    {
                        queryContainer |= qry;
                    }
                }
                else if ( item is SearchField searchField )
                {
                    if ( searchField.Name.IsNullOrWhiteSpace() || searchField.Value.IsNullOrWhiteSpace() )
                    {
                        continue;
                    }

                    // Break each search term into a separate query terms.
                    var searchString = GetSearchString( searchField.Value, searchField.IsPhrase, searchField.IsWildcard );

                    // Add field filter
                    var qry = new QueryStringQuery
                    {
                        Query = searchField.Value,
                        MinimumShouldMatch = "100%",
                        Rewrite = MultiTermQueryRewrite.ScoringBoolean,
                        Fields = new Nest.Field( searchField.Name, searchField.Boost )
                    };

                    if (!searchField.IsPhrase)
                    {
                        // If this is enabled on attribute value searches they fail.
                        qry.Analyzer = "whitespace_lowercase";
                    }

                    if ( query.IsAllMatching )
                    {
                        queryContainer &= qry;
                    }
                    else
                    {
                        queryContainer |= qry;
                    }
                }
                else if ( item is SearchAnyMatch )
                {
                    if ( query.IsAllMatching )
                    {
                        queryContainer &= new MatchAllQuery();
                    }
                    else
                    {
                        queryContainer |= new MatchAllQuery();
                    }
                }
                else if ( item is SearchQuery subQuery )
                {
                    var qry = GetQuery( subQuery, termFields );

                    if ( qry != null )
                    {
                        if ( query.IsAllMatching )
                        {
                            queryContainer &= qry;
                        }
                        else
                        {
                            queryContainer |= qry;
                        }
                    }
                }
            }

            return queryContainer;
        }

        /// <summary>
        /// Gets the search string to use in the QueryStringQuery object.
        /// </summary>
        /// <param name="needle">The needle text to search for.</param>
        /// <param name="isPhrase">if set to <c>true</c> the <paramref name="needle"/> is considered a whole phrase and will not be broken up by words.</param>
        /// <param name="isWildcard">if set to <c>true</c> the <paramref name="needle"/> is considered wildcard and will match words or phrases that begin with this value.</param>
        /// <returns>A string that is properly formatted for using in the query.</returns>
        private static string GetSearchString( string needle, bool isPhrase, bool isWildcard )
        {
            // Break each search term into a separate query terms.
            var queryTerms = needle.Split( ' ' )
                .Select( p => p.Trim() )
                .Where( a => a.IsNotNullOrWhiteSpace() )
                .ToList();

            if ( isPhrase )
            {
                return isWildcard
                    ? $"+\"{queryTerms.JoinStrings( " " )}\"*"
                    : $"+\"{queryTerms.JoinStrings( " " )}\"";
            }
            else
            {
                // We want to require each of the terms to exists for a
                // result to be returned.
                return isWildcard
                    ? $"+{queryTerms.JoinStrings( "* +" )}*"
                    : $"+{queryTerms.JoinStrings( " +" )}";
            }
        }

        #endregion

        #region Document Deletion

        /// <inheritdoc/>
        public override async Task DeleteMatchingDocumentsAsync( Type documentType, SearchQuery query )
        {
            var indexName = GetIndexName( documentType );
            var esQuery = GetQuery( query, null );

            var deleteRequest = new DeleteByQueryRequest( indexName );

            if ( esQuery != null )
            {
                deleteRequest.Query = esQuery;
            }

            await _client.DeleteByQueryAsync( deleteRequest );
        }

        /// <inheritdoc/>
        public override async Task DeleteMatchingDocumentsAsync( SearchQuery query )
        {
            var indexNames = new[]
            {
                GetIndexName( typeof( ContentChannelItemDocument ) )
            };

            var indices = Indices.Index( indexNames );
            var esQuery = GetQuery( query, null );

            // Don't allow them to delete the entire index database.
            if ( esQuery == null )
            {
                return;
            }

            var deleteRequest = new DeleteByQueryRequest( indices )
            {
                Query = esQuery
            };

            await _client.DeleteByQueryAsync( deleteRequest );
        }

        /// <inheritdoc/>
        public override async Task DeleteIndexAsync( Type documentType )
        {
            await _client.Indices.DeleteAsync( GetIndexName( documentType ) );
        }

        #endregion

        #region Document Retrieval and Searching

        /// <inheritdoc/>
        public override async Task<SearchResults> SearchAsync( SearchQuery query, SearchOptions options = null )
        {
            if ( _client == null )
            {
                return SearchResults.Empty;
            }

            options = options ?? new SearchOptions();

            // Get the base search descriptor and target all document types.
            var indexNames = ContentIndexContainer.GetAllDocumentTypes()
                .Select( t => GetIndexName( t ) )
                .ToArray();
            var searchDescriptor = new SearchDescriptor<IndexDocumentBase>()
                .Index( Indices.Index( indexNames ) );

            var queryContainer = GetQuery( query, GetSearchFields() );

            if ( options.MaxResults.HasValue )
            {
                searchDescriptor.Size( options.MaxResults.Value );
            }

            if ( options.Offset.HasValue )
            {
                searchDescriptor.From( options.Offset.Value );
            }

            if ( queryContainer != null )
            {
                searchDescriptor.Query( q => queryContainer );
            }

            // Apply the custom sorting options.
            searchDescriptor.Sort( sd =>
            {
                if ( options.Order == SearchSortOrder.Relevance )
                {
                    return options.IsDescending
                        ? sd.Descending( SortSpecialField.Score )
                        : sd.Ascending( SortSpecialField.Score );
                }
                else if ( options.Order == SearchSortOrder.RelevantDate )
                {
                    return options.IsDescending
                        ? sd.Descending( nameof( IndexDocumentBase.RelevanceDateTime ) )
                        : sd.Ascending( nameof( IndexDocumentBase.RelevanceDateTime ) );
                }
                else if ( options.Order == SearchSortOrder.Trending )
                {
                    return options.IsDescending
                        ? sd.Descending( nameof( IndexDocumentBase.IsTrending ) )
                            .Descending( nameof( IndexDocumentBase.TrendingRank ) )
                            .Descending( SortSpecialField.Score )
                        : sd.Descending( nameof( IndexDocumentBase.IsTrending ) )
                            .Ascending( nameof( IndexDocumentBase.TrendingRank ) )
                            .Descending( SortSpecialField.Score );
                }
                else if ( options.Order == SearchSortOrder.Alphabetical )
                {
                    return options.IsDescending
                        ? sd.Descending( nameof( IndexDocumentBase.NameSort ) ).Descending( SortSpecialField.Score )
                        : sd.Ascending( nameof( IndexDocumentBase.NameSort ) ).Descending( SortSpecialField.Score );
                }
                else
                {
                    return sd;
                }
            } );

            if ( _includeExplain )
            {
                searchDescriptor = searchDescriptor.Explain();
            }

            var results = await _client.SearchAsync<dynamic>( searchDescriptor );

            /* 04-12-2022 MDP

            To see the RAW Json Request and Response that was POST'd to Elastic search
            look at results.DebugInformation. This is useful to see the JSON of the query
            that got constructed.

            var debugInformation = results.DebugInformation;

            */

            if ( results == null )
            {
                return SearchResults.Empty;
            }

            if ( !results.IsValid && results.OriginalException != null )
            {
                ExceptionLogService.LogException( results.OriginalException );
                throw results.OriginalException;
            }

            var docs = results.Hits
                .Select( hit =>
                {
                    var document = GetStrongTypedIndexModel( hit );

                    if ( document == null )
                    {
                        return document;
                    }

                    // If AttributeKey.IncludeExplain is enabled, we can get a bunch of info explaining why we got the scores and results that we did.
                    document["Explain"] = hit.Explanation.ToJson();
                    document.Score = hit.Score ?? 0.00;

                    return document;
                } )
                .Where( d => d != null )
                .ToList();

            return new SearchResults
            {
                TotalResultsAvailable = ( int ) results.Total,
                Documents = docs
            };
        }

        #endregion

        #region Document Indexing

        /// <inheritdoc/>
        public override async Task CreateIndexAsync( Type documentType, bool deleteIfExists = true )
        {
            // Make sure this is an index document.
            if ( !typeof( IndexDocumentBase ).IsAssignableFrom( documentType ) )
            {
                return;
            }

            var indexName = GetIndexName( documentType );

            // Check if index already exists.
            var existsResponse = await _client.Indices.ExistsAsync( indexName );

            if ( existsResponse.Exists )
            {
                if ( !deleteIfExists )
                {
                    return;
                }

                await DeleteIndexAsync( documentType );
            }

            // Create a new index request.
            var createIndexRequest = new CreateIndexRequest( indexName )
            {
                Settings = new IndexSettings
                {
                    Analysis = new Analysis
                    {
                        Normalizers = new Normalizers(),
                        Analyzers = new Analyzers()
                    }
                }
            };

            /* 04/19/2022 MDP 
             
             ElasticSearch's 'whitespace' is now 'case-sensitive'. It was not case-senstive in 2.x. https://www.elastic.co/guide/en/elasticsearch/reference/master/analysis-analyzers.html.
             To make 'whitespace' case-insensitive, the docs say to create a custom analyser. https://www.elastic.co/guide/en/elasticsearch/reference/master/analysis-custom-analyzer.html

             So, we'll make an analyzer called "whitespace_lowercase" which is based on the 'whitespace' analyser with a 'lowercase' filter.
             
             */

            createIndexRequest.Settings.Analysis.Analyzers.Add( "whitespace_lowercase", new CustomAnalyzer { Tokenizer = "whitespace", Filter = new string[1] { "lowercase" } } );
            createIndexRequest.Settings.NumberOfShards = _shardCount;

            var typeMapping = new TypeMapping
            {
                Dynamic = true,
                Properties = new Properties()
            };

            createIndexRequest.Mappings = typeMapping;

            // Get properties from the model and add them to the index.
            // Attributes will be added dynamically as the documents are loaded.
            var modelProperties = documentType.GetProperties();

            foreach ( var propertyInfo in modelProperties )
            {
                var indexAttribute = propertyInfo.GetCustomAttribute<IndexFieldAttribute>();

                if ( indexAttribute == null )
                {
                    continue;
                }

                var fieldType = indexAttribute.FieldType;

                if ( fieldType == IndexFieldType.Boolean )
                {
                    typeMapping.Properties.Add( propertyInfo, new BooleanProperty()
                    {
                        Name = propertyInfo,
                        Index = indexAttribute.IsSearched
                    } );
                }
                else if ( fieldType == IndexFieldType.DateTime )
                {
                    typeMapping.Properties.Add( propertyInfo, new DateProperty()
                    {
                        Name = propertyInfo,
                        Index = indexAttribute.IsSearched,

                        // Our DateTime data gets serialized as '2022-04-11T16:40:47.4070819'
                        // But by default ElasticSearch wants the Z portion (DateTimeOffset)
                        // Since we don't use DateTimeOffset, we'll add a couple more options for how query DateTime terms are processed
                        // See all the formatting options here https://www.elastic.co/guide/en/elasticsearch/reference/current/mapping-date-format.html
                        Format = $"{DateFormat.date_optional_time}||{DateFormat.epoch_millis}||MM-dd-yyyy||yyyy-MM-dd'T'HH:mm:ss.SSS"
                    } );
                }
                else if ( fieldType == IndexFieldType.Integer )
                {
                    typeMapping.Properties.Add( propertyInfo, new NumberProperty()
                    {
                        Name = propertyInfo,
                        Index = indexAttribute.IsSearched
                    } );
                }
                else
                {
                    // Text field.
                    if ( indexAttribute.IsSearched )
                    {
                        typeMapping.Properties.Add( propertyInfo, new TextProperty
                        {
                            Name = propertyInfo,
                            Index = true
                        } );
                    }
                    else
                    {
                        typeMapping.Properties.Add( propertyInfo, new KeywordProperty
                        {
                            Name = propertyInfo
                        } );
                    }
                }
            }

            await _client.Indices.CreateAsync( createIndexRequest );
        }

        /// <inheritdoc/>
        public override async Task IndexDocumentAsync<TDocument>( TDocument document )
        {
            var indexName = GetIndexName( document.GetType() );

            // Check if index already exists and if not create it.
            var existsResponse = await _client.Indices.ExistsAsync( indexName );
            if ( !existsResponse.Exists )
            {
                await CreateIndexAsync( document.GetType() );
            }

            try
            {
                await _client.IndexAsync( document, s => s.Index( indexName ) );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
            }
        }

        #endregion

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
