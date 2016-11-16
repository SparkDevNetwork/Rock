﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Nest;
using Newtonsoft.Json;
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
    [Description( "Elasticsearch Universal Search Index" )]
    [Export( typeof( IndexComponent ) )]
    [ExportMetadata( "ComponentName", "Elasticsearch" )]

    [TextField( "Node URL", "The URL of the ElasticSearch node (http://myserver:9200)", true, key: "NodeUrl" )]
    public class Elasticsearch : IndexComponent
    {
        private ElasticClient _client;

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
                }

                if ( _client != null )
                {
                    var results = _client.ClusterState();

                    if ( results != null )
                    {
                        return results.IsValid;
                    }
                }
                return false;
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
                return GetAttributeValue( "NodeUrl" );
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
        /// Connects to server.
        /// </summary>
        private void ConnectToServer()
        {
            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "NodeUrl" ) ) )
            {
                var node = new Uri( GetAttributeValue( "NodeUrl" ) );
                var config = new ConnectionSettings( node );
                config.DisableDirectStreaming();
                _client = new ElasticClient( config );
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

            _client.IndexAsync<T>( document, c => c.Index( indexName ).Type( mappingType ) );
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
        /// Creates the index.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        /// <param name="deleteIfExists">if set to <c>true</c> [delete if exists].</param>
        public override void CreateIndex( Type documentType, bool deleteIfExists = true )
        {
            var indexName = documentType.Name.ToLower();

            object instance = Activator.CreateInstance( documentType );

            // check if index already exists
            var existsResponse = _client.IndexExists( indexName );

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

            // make sure this is an index document
            if ( instance is IndexModelBase )
            {
                // create a new index request
                var createIndexRequest = new CreateIndexRequest( indexName );
                createIndexRequest.Mappings = new Mappings();

                var typeMapping = new TypeMapping();
                typeMapping.Properties = new Properties();

                createIndexRequest.Mappings.Add( indexName, typeMapping );

                var model = (IndexModelBase)instance;

                // get properties from the model and add them to the index (hint: attributes will be added dynamically as the documents are loaded)
                var modelProperties = documentType.GetProperties();

                foreach ( var property in modelProperties )
                {
                    var indexAttributes = property.GetCustomAttributes( false );
                    var indexAttribute = property.GetCustomAttributes( typeof( RockIndexField ), false );
                    if ( indexAttribute.Length > 0 )
                    {
                        var attribute = (RockIndexField)indexAttribute[0];

                        

                        var propertyName = Char.ToLowerInvariant( property.Name[0] ) + property.Name.Substring( 1 );

                        // rewrite non-string index option (would be nice if they made the enums match up...)
                        NonStringIndexOption nsIndexOption = NonStringIndexOption.NotAnalyzed;
                        if ( attribute.Type != IndexFieldType.String )
                        {
                            if ( attribute.Index == IndexType.NotIndexed )
                            {
                                nsIndexOption = NonStringIndexOption.No;
                            }
                        }

                        switch ( attribute.Type )
                        {
                            case IndexFieldType.Boolean:
                                {
                                    typeMapping.Properties.Add( propertyName, new BooleanProperty() { Name = propertyName, Boost = attribute.Boost, Index = nsIndexOption } );
                                    break;
                                }
                            case IndexFieldType.Date:
                                {
                                    typeMapping.Properties.Add( propertyName, new DateProperty() { Name = propertyName, Boost = attribute.Boost, Index = nsIndexOption } );
                                    break;
                                }
                            case IndexFieldType.Number:
                                {
                                    typeMapping.Properties.Add( propertyName, new NumberProperty() { Name = propertyName, Boost = attribute.Boost, Index = nsIndexOption } );
                                    break;
                                }
                            default:
                                {
                                    var stringProperty = new StringProperty();
                                    stringProperty.Name = propertyName;
                                    stringProperty.Boost = attribute.Boost;
                                    stringProperty.Index = (FieldIndexOption)attribute.Index;

                                    if ( !string.IsNullOrWhiteSpace(attribute.Analyzer) )
                                    {
                                        stringProperty.Analyzer = attribute.Analyzer;
                                    }

                                    typeMapping.Properties.Add( propertyName, stringProperty );
                                    break;
                                }
                        }
                    }
                }

                var response = _client.CreateIndex( createIndexRequest );
            }
        }

        /// <summary>
        /// Deletes the index.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        public override void DeleteIndex( Type documentType )
        {
            _client.DeleteIndex( documentType.Name.ToLower() );
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
        private bool SupportsIndexFieldFiltering( Type entityType )
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
            totalResultsAvailable = 0;

            ISearchResponse<dynamic> results = null;
            List<SearchResultModel> searchResults = new List<SearchResultModel>();

            QueryContainer queryContainer = new QueryContainer();

            // add and field constraints
            var searchDescriptor = new SearchDescriptor<dynamic>().AllIndices();

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
                }

                searchDescriptor = searchDescriptor.Type( string.Join( ",", entityTypes ) ); // todo: consider adding indexmodeltype to the entity cache
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
                        matchQuery &= new MatchQuery { Field = match.Field, Query = match.Value, Boost = match.Boost };
                    }
                }
            }

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
                            queryContainer |= new QueryStringQuery { Query = "phoneNumbers:*" + query + "*", AnalyzeWildcard = true };
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

                        results = _client.Search<dynamic>( searchDescriptor );
                        break;
                    }
                case SearchType.Fuzzy:
                    {
                        results = _client.Search<dynamic>( d =>
                                    d.AllIndices().AllTypes()
                                    .Query( q =>
                                        q.Fuzzy( f => f.Value( query )
                                        .Rewrite( RewriteMultiTerm.TopTermsN ) )
                                    )
                                );
                        break;
                    }
                case SearchType.Wildcard:
                    {
                        if ( !string.IsNullOrWhiteSpace( query ) )
                        {
                            QueryContainer wildcardQuery = null;

                            // break each search term into a separate query and add the * to the end of each
                            var queryTerms = query.Split( ' ' ).Select( p => p.Trim() ).ToList();

                            foreach ( var queryTerm in queryTerms )
                            {
                                if ( !string.IsNullOrWhiteSpace( queryTerm ) )
                                {
                                    wildcardQuery &= new QueryStringQuery { Query = queryTerm + "*", Analyzer = "whitespace", Rewrite = RewriteMultiTerm.ScoringBoolean  }; // without the rewrite all results come back with the score of 1; analyzer of whitespaces says don't fancy parse things like check-in to 'check' and 'in'
                                }
                            }

                            // special logic to support emails
                            if ( queryTerms.Count == 1 && query.Contains( "@" ) )
                            {
                                wildcardQuery |= new QueryStringQuery { Query = "email:*" + query + "*", Analyzer = "whitespace" };
                            }

                            // special logic to support phone search
                            if ( query.IsDigitsOnly() )
                            {
                                wildcardQuery |= new QueryStringQuery { Query = "phoneNumbers:*" + query, Analyzer = "whitespace" };
                            }

                            queryContainer &= wildcardQuery;
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

                        results = _client.Search<dynamic>( searchDescriptor );
                        break;
                    }
            }

            totalResultsAvailable = results.Total;

            List<IndexModelBase> documents = new List<IndexModelBase>();

            // normallize the results to rock search results
            if ( results != null )
            {
                foreach ( var hit in results.Hits )
                {
                    IndexModelBase document = new IndexModelBase();

                    try
                    {
                        if ( hit.Source != null )
                        {

                            Type indexModelType = Type.GetType( (string)((JObject)hit.Source)["indexModelType"] );

                            if ( indexModelType != null )
                            {
                                document = (IndexModelBase)((JObject)hit.Source).ToObject( indexModelType ); // return the source document as the derived type
                            }
                            else
                            {
                                document = ((JObject)hit.Source).ToObject<IndexModelBase>(); // return the source document as the base type
                            }
                        }

                        if ( hit.Explanation != null )
                        {
                            document["Explain"] = hit.Explanation.ToJson();
                        }

                        document.Score = hit.Score;

                        documents.Add( document );
                    }
                    catch { } // ignore if the result if an exception resulted (most likely cause is getting a result from a non-rock index)
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

            var response = _client.DeleteByQuery<IndexModelBase>( documentType.Name.ToLower(), documentType.Name.ToLower(), qd => qd.Query( q => q.Raw( jsonSearch ) ) );
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
            var indexName = documentType.Name.ToLower();

            var request = new GetRequest( indexName, indexName, id.ToString() ) { };

            var result = _client.Get<dynamic>( request );

            IndexModelBase document = new IndexModelBase();

            if ( result.Source != null )
            {
                Type indexModelType = Type.GetType( (string)((JObject)result.Source)["indexModelType"] );

                if ( indexModelType != null )
                {
                    document = (IndexModelBase)((JObject)result.Source).ToObject( indexModelType ); // return the source document as the derived type
                }
                else
                {
                    document = ((JObject)result.Source).ToObject<IndexModelBase>(); // return the source document as the base type
                }
            }

            return document;

        }
    }
}


// forbidden characters in field names _ . , #

// cluster state: http://localhost:9200/_cluster/state?filter_nodes=false&filter_metadata=true&filter_routing_table=true&filter_blocks=true&filter_indices=true