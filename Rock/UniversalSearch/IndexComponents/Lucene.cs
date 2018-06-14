﻿// <copyright>
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
using System.IO;
using System.Linq;
using System.Reflection;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Snowball;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Newtonsoft.Json.Linq;
using Rock.Cache;
using Rock.Data;
using Rock.Model;
using Rock.UniversalSearch.IndexModels;
using Rock.UniversalSearch.IndexModels.Attributes;

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
        private static readonly LuceneVersion MatchVersion = LuceneVersion.LUCENE_48;
        private static Dictionary<string, Index> Indexes = new Dictionary<string, Index>();
        private static IndexWriterConfig indexWriterConfig = null;
        private static IndexWriter writer = null;
        private static SearcherManager searcherManager = null;
        private static string path = null;
        private static FSDirectory _directory;

        public Lucene()
        {
            if ( this.IsActive && writer == null )
            {
                path = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "App_Data", "LuceneSearchIndex" );
                // path = System.Web.HttpContext.Current.Server.MapPath( "~/App_Data/LuceneSearchIndex" ); // Do not work with jobs
                indexWriterConfig = new IndexWriterConfig( MatchVersion, null ) // No default Analyzer. Have to explicitly specify it when using IndexReader/IndexWriter
                {
                    OpenMode = OpenMode.CREATE_OR_APPEND,
                    WriteLockTimeout = 10000
                };

                indexWriterConfig.OpenMode = OpenMode.CREATE_OR_APPEND;
                if ( IndexWriter.IsLocked( directory ) )
                {
                    IndexWriter.Unlock( directory );
                }

                writer = new IndexWriter( directory, indexWriterConfig );
                writer.Flush( true, true );
                writer.Commit();

                searcherManager = new SearcherManager( writer, true, null );
            }

        }

        /// <summary>
        /// Gets the Lucene directory.
        /// </summary>
        /// <value>
        /// The Lucene directory.
        /// </value>
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

        /// <summary>
        /// Gets a value indicating whether Lucene IndexWriter is connected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if Lucene IndexWriter is connected; otherwise, <c>false</c>.
        /// </value>
        public override bool IsConnected
        {
            get
            {
                return writer != null && !writer.IsClosed;
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
                return path;

            }
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

            // Check if index already exists. If it exists, no need to create it again
            if ( Indexes.ContainsKey( indexName ) )
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
                                var tokenizer = new StandardTokenizer( MatchVersion, reader );
                                var sbpff = new SnowballPorterFilterFactory( new Dictionary<string, string>() { { "language", "English" } } );
                                TokenStream result = sbpff.Create( new StandardTokenizer( MatchVersion, reader ) );
                                return new TokenStreamComponents( tokenizer, result ); // https://github.com/apache/lucenenet/blob/master/src/Lucene.Net.Analysis.Common/Analysis/Snowball/SnowballAnalyzer.cs
                            } );
                        }
                        else if ( typeMappingProperty.Analyzer?.ToLowerInvariant() == "whitespace" )
                        {
                            fieldAnalyzers[typeMappingProperty.Name] = Analyzer.NewAnonymous( createComponents: ( fieldName, reader ) =>
                            {
                                var tokenizer = new WhitespaceTokenizer( MatchVersion, reader );
                                TokenStream result = new StandardFilter( MatchVersion, tokenizer );
                                return new TokenStreamComponents( tokenizer, result );
                            } );
                        }
                    }
                }

                index.MappingProperties = typeMapping;
                index.FieldAnalyzers = fieldAnalyzers;
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

            writer.DeleteDocuments( new Term( "type", typeof( T ).Name.ToLower() ) );
            writer.Flush( true, true );
            writer.Commit();
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

            IndexModelBase docIndexModelBase = document as IndexModelBase;
            writer.DeleteDocuments( new Term( "index", document.GetType().Name.ToLower() + "_" + docIndexModelBase.Id.ToString( "0000000" ) ) );
            writer.Flush( true, true );
            writer.Commit();
        }

        /// <summary>
        /// Deletes the document by identifier.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        /// <param name="id">The identifier.</param>
        public override void DeleteDocumentById( Type documentType, int id )
        {
            writer.DeleteDocuments( new Term( "index", documentType.Name.ToLower() + "_" + id.ToString( "0000000" ) ) );
            writer.Flush( true, true );
            writer.Commit();

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
            writer.DeleteDocuments( new Term[] { new Term( "type", documentType.Name.ToLower() ), new Term( propertyName, propertyValue.ToStringSafe() ) } );
            writer.Flush( true, true );
            writer.Commit();
        }

        /// <summary>
        /// Delete all documents by type.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        public override void DeleteIndex( Type documentType )
        {
            writer.DeleteDocuments( new Term( "type", documentType.Name.ToLower() ) );
            writer.Flush( true, true );
            writer.Commit();
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

        /// <summary>
        /// Indexes the document.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document">The document.</param>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="mappingType">Type of the mapping.</param>
        public override void IndexDocument<T>( T document, string indexName = null, string mappingType = null )
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

            if ( !Indexes.ContainsKey( mappingType ) )
            {
                CreateIndex( documentType );
            }

            var index = Indexes[mappingType];

            Document doc = new Document();
            foreach ( var typeMappingProperty in index.MappingProperties.Values )
            {
                TextField textField = new TextField( typeMappingProperty.Name, documentType.GetProperty( typeMappingProperty.Name ).GetValue( document, null ).ToStringSafe().ToLower(), global::Lucene.Net.Documents.Field.Store.YES );
                textField.Boost = typeMappingProperty.Boost;
                doc.Add( textField );
            }

            IndexModelBase docIndexModelBase = document as IndexModelBase;
            doc.AddStoredField( "type", mappingType );
            doc.AddStoredField( "id", docIndexModelBase.Id.ToString() );
            doc.AddStoredField( "JSON", document.ToJson() ); // Stores all the properties as JSON to retreive object on lookup

            // Use the analyzer in fieldAnalyzers if that field is in that dictionary, otherwise use StandardAnalyzer.
            PerFieldAnalyzerWrapper analyzer = new PerFieldAnalyzerWrapper( defaultAnalyzer: new StandardAnalyzer( MatchVersion ), fieldAnalyzers: index.FieldAnalyzers );

            writer.UpdateDocument( new Term( "index", mappingType + "_" + docIndexModelBase.Id.ToString( "0000000" ) ), doc, analyzer ); // Must specify analyzer because the default analyzer that is specified in indexWriterConfig is null.
            writer.Flush( true, true );
            writer.Commit();
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
                if ( !Indexes.ContainsKey( mappingTypeName ) )
                {
                    CreateIndex( mappingType );
                }

                var index = Indexes[mappingTypeName];

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
                        var selectedEntityTypes = CacheEntityType.All().Where( e => e.IsIndexingSupported && e.IsIndexingEnabled && e.FriendlyName != "Site" );

                        foreach ( var entityTypeCache in selectedEntityTypes )
                        {
                            entities.Add( entityTypeCache.Id );
                        }
                    }

                    foreach ( var entityId in entities )
                    {
                        // get entities search model name
                        var entityType = entityTypeService.Get( entityId );
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

                    if ( entities != null && entities.Count != 0 )
                    {
                        var indexModelTypesQuery = new BooleanQuery();
                        indexModelTypes.ForEach( f => indexModelTypesQuery.Add( new PrefixQuery( new Term( "type", f.Name.ToLower() ) ), Occur.SHOULD ) );
                        queryContainer.Add( indexModelTypesQuery, Occur.MUST );
                    }
                }

                TopDocs topDocs = null;
                // Use the analyzer in fieldAnalyzers if that field is in that dictionary, otherwise use StandardAnalyzer.
                PerFieldAnalyzerWrapper analyzer = new PerFieldAnalyzerWrapper( defaultAnalyzer: new StandardAnalyzer( MatchVersion ), fieldAnalyzers: combinedFieldAnalyzers );

                BooleanQuery fieldCriteriaQuery = new BooleanQuery();

                if ( fieldCriteria != null && fieldCriteria.FieldValues?.Count > 0 )
                {
                    Occur occur = fieldCriteria.SearchType == CriteriaSearchType.And ? Occur.MUST : Occur.SHOULD;
                    foreach ( var match in fieldCriteria.FieldValues )
                    {
                        BooleanClause booleanClause = new BooleanClause( new TermQuery( new Term( match.Field, match.Value ) ), occur );
                        booleanClause.Query.Boost = match.Boost;
                        fieldCriteriaQuery.Add( booleanClause );
                    }

                    queryContainer.Add( fieldCriteriaQuery, Occur.MUST );
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
                                queryContainer.Add( new BooleanClause( new TermQuery( new Term( "email", query ) ), Occur.SHOULD ) );
                            }

                            // special logic to support phone search
                            if ( query.IsDigitsOnly() )
                            {
                                queryContainer.Add( new BooleanClause( new WildcardQuery( new Term( "phone", "*" + query + "*" ) ), Occur.SHOULD ) );
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
                                    wildcardQuery.Add( new WildcardQuery( new Term( "email", "*" + query.ToLower() + "*" ) ), Occur.SHOULD );
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

                searcherManager.MaybeRefreshBlocking();
                IndexSearcher indexSearcher = searcherManager.Acquire();

                if ( from.HasValue )
                {
                    TopScoreDocCollector collector = TopScoreDocCollector.Create( returnSize * 10, true ); // Search for 10 pages with returnSize entries in each page
                    indexSearcher.Search( queryContainer, collector );
                    topDocs = collector.GetTopDocs( from.Value, returnSize );
                }
                else
                {
                    topDocs = indexSearcher.Search( queryContainer, returnSize );
                }

                totalResultsAvailable = topDocs.TotalHits;

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

                            Explanation explanation = indexSearcher.Explain( queryContainer, hit.Doc );
                            document["Explain"] = explanation.ToString();
                            document.Score = hit.Score;

                            documents.Add( document );
                        }
                        catch { } // ignore if the result if an exception resulted (most likely cause is getting a result from a non-rock index)
                    }
                }

                searcherManager.Release( indexSearcher );
                indexSearcher = null; // Don't use searcher after this point!
            }

            return documents;
        }

        /// <summary>
        /// Dispose of the index from memory and close all files.
        /// </summary>
        public static void Dispose()
        {
            if ( writer != null )
            {
                try
                {
                    writer.Dispose();
                }
                finally
                {
                    if ( IndexWriter.IsLocked( directory ) )
                    {
                        IndexWriter.Unlock( directory );
                    }

                    writer = null;
                }
            }
            searcherManager?.Dispose();
            searcherManager = null;
        }

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
    }
}
