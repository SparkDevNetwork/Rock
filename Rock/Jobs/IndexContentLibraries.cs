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
using Rock.Cms.ContentLibrary;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// A job that updates the content libraries search index.
    /// </summary>
    [DisplayName( "Index Content Libraries" )]
    [Description( "A job that updates the content libraries search index." )]

    #region Job Attributes

    [IntegerField( "Maximum Concurrency",
        Description = "The maximum number of concurrent tasks to use when indexing content libraries.",
        DefaultIntegerValue = 10,
        IsRequired = true,
        Key = AttributeKey.MaxConcurrency,
        Order = 0 )]

    #endregion

    [DisallowConcurrentExecution]
    public class IndexContentLibraries : IJob
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
        public IndexContentLibraries()
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
            int documentsIndexed = 0;
            var indexableEntityTypes = EntityTypeCache.All().Where( et => et.IsContentLibraryIndexingEnabled );
            var options = new IndexDocumentOptions
            {
                MaxConcurrency = maxConcurrency,
                IsTrendingEnabled = true
            };

            // First delete all indexes so we start clean.
            foreach ( var entityTypeCache in indexableEntityTypes )
            {
                await ContentIndexContainer.DeleteIndexAsync( entityTypeCache.ContentLibraryDocumentType );
            }

            // Next create all the indexes again.
            await ContentIndexContainer.CreateAllIndexesAsync();

            // Then update all sources.
            foreach ( var entityTypeCache in indexableEntityTypes )
            {
                var indexer = ( IContentLibraryIndexer ) Activator.CreateInstance( entityTypeCache.ContentLibraryIndexerType );

                foreach ( var source in ContentLibrarySourceCache.All() )
                {
                    documentsIndexed += await indexer.IndexAllContentLibrarySourceDocumentsAsync( source.Id, options );
                }
            }

            return documentsIndexed;
        }
    }
}
