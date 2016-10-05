﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
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
                if (_client == null )
                {
                    ConnectToServer();
                }

                if ( _client != null )
                {
                    var results = _client.ClusterState();

                    if (results != null )
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
            if (indexName == null )
            {
                indexName = document.GetType().Name.ToLower();
            }

            if (mappingType == null )
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

            _client.DeleteByQueryAsync<T>(indexName, typeof( T ).Name.ToLower(), d => d.MatchAll() );
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
        public override void CreateIndex(Type documentType, bool deleteIfExists = true)
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
            if (instance is IndexModelBase )
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

                foreach(var property in modelProperties )
                {
                    var indexAttributes = property.GetCustomAttributes(false);
                    var indexAttribute = property.GetCustomAttributes( typeof( RockIndexField ), false );
                    if(indexAttribute.Length > 0 )
                    {
                        var attribute = (RockIndexField)indexAttribute[0];
                        
                        var propertyName = Char.ToLowerInvariant( property.Name[0] ) + property.Name.Substring( 1 );

                        // rewrite non-string index option (would be nice if they made the enums match up...)
                        NonStringIndexOption nsIndexOption = NonStringIndexOption.NotAnalyzed;
                        if (attribute.Type != IndexFieldType.String )
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
                                    typeMapping.Properties.Add( propertyName, new StringProperty() { Name = propertyName, Boost = attribute.Boost, Index = (FieldIndexOption)attribute.Index } );
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
        /// <returns></returns>
        public override IEnumerable<SearchResultModel> Search( string query, SearchType searchType = SearchType.ExactMatch, List<int> entities = null )
        {
            ISearchResponse<dynamic> results = null;
            List<SearchResultModel> searchResults = new List<SearchResultModel>();

            if (searchType == SearchType.ExactMatch )
            {
                var searchDescriptor = new SearchDescriptor<dynamic>().AllIndices();

                if (entities == null || entities.Count == 0 )
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

                    searchDescriptor = searchDescriptor.Type( string.Join( ",", entityTypes ) ); // todo: considter adding indexmodeltype to the entity cache
                }

                searchDescriptor = searchDescriptor.Query( q => q.QueryString( s => s.Query( query ) ) );

                results = _client.Search<dynamic>( searchDescriptor );
            }
            else
            {
                results = _client.Search<dynamic>( d => 
                                    d.AllIndices().AllTypes()
                                    .Query( q => 
                                        q.Fuzzy( f => f.Value( query ) ) 
                                    )
                                    .Explain( true ) // todo remove before flight 
                                );
            }

            //var presults = _client.Search<PersonIndex>( s => s.AllIndices().Query( q => q.QueryString( qs => qs.Query( query ) ) ) );

            // normallize the results to rock search results
            if (results != null )
            {
                foreach(var hit in results.Hits )
                {
                    var searchResult = new SearchResultModel();
                    searchResult.Score = hit.Score;
                    searchResult.Type = hit.Type;
                    searchResult.Index = hit.Index;
                    searchResult.EntityId = hit.Id.AsInteger();

                    try {
                        if ( hit.Source != null )
                        {

                            Type indexModelType = Type.GetType( (string)((JObject)hit.Source)["indexModelType"] );

                            if ( indexModelType != null )
                            {
                                searchResult.Document = (IndexModelBase)((JObject)hit.Source).ToObject( indexModelType ); // return the source document as the derived type
                            }
                            else
                            {
                                searchResult.Document = ((JObject)hit.Source).ToObject<IndexModelBase>(); // return the source document as the base type
                            }
                        }

                        if ( hit.Explanation != null )
                        {
                            searchResult.Explain = hit.Explanation.ToJson();
                        }

                        searchResults.Add( searchResult );
                    }
                    catch { } // ignore if the result if an exception resulted (most likely cause is getting a result from a non-rock index)
                }
            }

            return searchResults;

            
        }

        /// <summary>
        /// Deletes the document by property.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">The property value.</param>
        public override void DeleteDocumentByProperty( Type documentType, string propertyName, object propertyValue ) {

            string jsonSearch = string.Format( @"{{
""term"": {{
      ""{0}"": {{
                ""value"": ""{1}""
      }}
        }}
}}", Char.ToLowerInvariant( propertyName[0] ) + propertyName.Substring( 1 ), propertyValue );

            var response = _client.DeleteByQuery<IndexModelBase>( documentType.Name.ToLower(), documentType.Name.ToLower(), qd => qd.Query( q => q.Raw( jsonSearch ) ));
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
    }
}


// forbidden characters in field names _ . , #

// cluster state: http://localhost:9200/_cluster/state?filter_nodes=false&filter_metadata=true&filter_routing_table=true&filter_blocks=true&filter_indices=true