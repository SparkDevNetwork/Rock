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
using System.Linq;
using System.Reflection;

using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.UniversalSearch;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Re-indexes the selected entity types in Universal Search
    /// </summary>
    [BooleanField("Index All Entities", "Indexes all entities, the entity filter will be ignored.", true, order: 0)]
    [CustomCheckboxListField("Entity Filter", "Entities to re-index. Not selecting a value will re-index all index enabled entities.", "SELECT CAST([Id] AS VARCHAR) [Value], [FriendlyName] [Text] FROM [EntityType] WHERE [IsIndexingEnabled] = 1 AND [FriendlyName] != 'Site'", false, order: 1 )]
    [DisallowConcurrentExecution]
    public class IndexEntities : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public IndexEntities()
        {
        }

        /// <summary>
        /// Re-indexes the selected entity types in Universal Search
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            string selectedEntitiesSetting = dataMap.GetString( "EntityFilter" );
            bool allEntities = dataMap.GetBoolean( "IndexAllEntities" );

            RockContext rockContext = new RockContext();

            var selectedEntityTypes = EntityTypeCache.All().Where( e => e.IsIndexingSupported && e.IsIndexingEnabled && e.FriendlyName != "Site" );

            // if 'All' wasn't selected the filter out the ones that weren't selected
            if ( !allEntities )
            {
                if ( selectedEntitiesSetting.IsNotNullOrWhiteSpace() )
                {
                    var selectedEntityIds = selectedEntitiesSetting.Split( ',' ).Select( int.Parse ).ToList();
                    selectedEntityTypes = selectedEntityTypes.Where( e => selectedEntityIds.Contains( e.Id ) );
                }
            }            

            string results = string.Empty;
            var timerTotal = System.Diagnostics.Stopwatch.StartNew();
            foreach (var entityTypeCache in selectedEntityTypes )
            {
                EntityTypeService entityTypeService = new EntityTypeService( rockContext );
                var entityType = entityTypeService.Get( entityTypeCache.Id );

                IndexContainer.DeleteIndex( entityType.IndexModelType );
                IndexContainer.CreateIndex( entityType.IndexModelType );

                Type type = entityTypeCache.GetEntityType();

                if ( type != null )
                {
                    object classInstance = Activator.CreateInstance( type, null );
                    MethodInfo bulkItemsMethod = type.GetMethod( "BulkIndexDocuments" );

                    if ( classInstance != null && bulkItemsMethod != null )
                    {
                        var timer = System.Diagnostics.Stopwatch.StartNew();
                        bulkItemsMethod.Invoke( classInstance, null );
                        timer.Stop();
                        results += $"{entityType.FriendlyName}: {timer.ElapsedMilliseconds/1000}s,";
                    }
                }
            }

            results += $"Total Time: {timerTotal.ElapsedMilliseconds / 1000}s,";
            context.Result = "Indexing results: " + results.Trim( ',' );
        }
    }
}
