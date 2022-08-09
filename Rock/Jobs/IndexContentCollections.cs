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
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Quartz;

using Rock.Attribute;
using Rock.Cms.ContentCollection;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// A job that updates the content collections search index.
    /// </summary>
    [DisplayName( "Index Content Collections" )]
    [Description( "A job that updates the content collections search index." )]

    #region Job Attributes

    [IntegerField( "Maximum Concurrency",
        Description = "The maximum number of concurrent tasks to use when indexing content collections.",
        DefaultIntegerValue = 10,
        IsRequired = true,
        Key = AttributeKey.MaxConcurrency,
        Order = 0 )]

    #endregion

    [DisallowConcurrentExecution]
    public class IndexContentCollections : IJob
    {
        /// <summary>
        /// Keys to use for Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string MaxConcurrency = "MaxConcurrency";
        }

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public IndexContentCollections()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            var dataMap = context.JobDetail.JobDataMap;
            var maxConcurrency = dataMap.GetString( AttributeKey.MaxConcurrency ).AsIntegerOrNull() ?? 10;

            var processDocumentIndexTask = Task.Run( async () => await GenerateDocumentIndexAsync( maxConcurrency ) );
            processDocumentIndexTask.Wait();
            var documentCount = processDocumentIndexTask.Result;

            context.Result = $"Indexed {documentCount:N0} {"document".PluralizeIf( documentCount != 1 )}.";
        }

        /// <summary>
        /// Process the document index and ensures it is in sync.
        /// </summary>
        /// <param name="maxConcurrency">The maximum number of concurrent operations allowed.</param>
        private async Task<int> GenerateDocumentIndexAsync( int maxConcurrency )
        {
            var allDocumentsIndexed = 0;
            var indexableEntityTypes = EntityTypeCache.All().Where( et => et.IsContentCollectionIndexingEnabled );
            var options = new IndexDocumentOptions
            {
                MaxConcurrency = maxConcurrency,
                IsTrendingEnabled = true
            };

            // First delete all indexes so we start clean.
            foreach ( var entityTypeCache in indexableEntityTypes )
            {
                await ContentIndexContainer.DeleteIndexAsync( entityTypeCache.ContentCollectionDocumentType );
            }

            // Next create all the indexes again.
            await ContentIndexContainer.CreateAllIndexesAsync();

            // Index each content collection.
            foreach ( var contentCollection in ContentCollectionCache.All() )
            {
                var collectionDocumentsIndexed = 0;

                // Update all sources in this collection.
                foreach ( var entityTypeCache in indexableEntityTypes )
                {
                    var indexer = ( IContentCollectionIndexer ) Activator.CreateInstance( entityTypeCache.ContentCollectionIndexerType );
                    var sources = ContentCollectionSourceCache.All().Where( s => s.ContentCollectionId == contentCollection.Id );

                    foreach ( var source in sources )
                    {
                        var count = await indexer.IndexAllContentCollectionSourceDocumentsAsync( source.Id, options );

                        collectionDocumentsIndexed += count;
                        allDocumentsIndexed += count;
                    }
                }

                // Try to update the last index values.
                try
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var updateContentCollection = new ContentCollectionService( rockContext ).Get( contentCollection.Id );

                        updateContentCollection.LastIndexDateTime = RockDateTime.Now;
                        updateContentCollection.LastIndexItemCount = collectionDocumentsIndexed;

                        rockContext.SaveChanges();
                    }
                }
                catch ( Exception ex )
                {
                    // Continue past exceptions updating the content collection,
                    // but do log them.
                    ExceptionLogService.LogException( ex );
                }
            }

            return allDocumentsIndexed;
        }
    }
}
