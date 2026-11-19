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
using System.Linq;
using System.Reflection;

using Rock.Attribute;
using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.UniversalSearch;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Re-indexes the selected entity types in Universal Search
    /// </summary>
    [DisplayName( "Universal Search Re-Index" )]
    [Description( "Re-indexes the selected entity types in Universal Search." )]

    [BooleanField( "Index All Entities",
        Description = "Indexes all entities, the entity filter will be ignored.",
        DefaultBooleanValue = true,
        Order = 0)]
    [CustomCheckboxListField( "Entity Filter",
        Description = "Entities to re-index. Not selecting a value will re-index all index enabled entities.",
        ListSource = "SELECT CAST([Id] AS VARCHAR) [Value], [FriendlyName] [Text] FROM [EntityType] WHERE [IsIndexingEnabled] = 1 AND [FriendlyName] != 'Site'",
        IsRequired = false,
        Order = 1 )]
    public class IndexEntities : RockJob
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

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            string selectedEntitiesSetting = GetAttributeValue( "EntityFilter" );
            bool allEntities = GetAttributeValue( "IndexAllEntities" ).AsBoolean();

            RockContext rockContext = RockApp.Current.CreateRockContext();

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

            var entityTypesToIndex = selectedEntityTypes.ToList();
            var indexExceptions = new List<Exception>();

            string results = string.Empty;
            var timerTotal = System.Diagnostics.Stopwatch.StartNew();
            foreach (var entityTypeCache in entityTypesToIndex )
            {
                /*
                    10/30/2026 - MSE

                    Each entity type is indexed in its own try/catch. Before this, one
                    failure (for example a Person timeout on a large database, issue #7080)
                    ended the job and every entity type after it was never indexed.

                    The failures are not logged here. They are thrown together at the end
                    and RockJobListener logs each one to the Exception Log, the same way
                    CalculateMetrics reports failed metrics.

                    Reason: One failing entity type should not stop the others from indexing.
                */
                try
                {
                    EntityTypeService entityTypeService = new EntityTypeService( rockContext );
                    var entityType = entityTypeService.Get( entityTypeCache.Id );

                    IndexContainer.DeleteIndex( entityType.IndexModelType );
                    IndexContainer.CreateIndex( entityType.IndexModelType );

                    Type type = entityTypeCache.GetEntityType();
                    this.UpdateLastStatusMessage( $"Indexing {entityTypeCache.FriendlyName}..." );

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
                catch ( Exception ex )
                {
                    // The reflection Invoke wraps the real error, so report the inner exception.
                    var innerException = ex is TargetInvocationException && ex.InnerException != null ? ex.InnerException : ex;

                    indexExceptions.Add( new Exception( $"Error indexing {entityTypeCache.FriendlyName}: {innerException.Message}", innerException ) );
                    results += $"{entityTypeCache.FriendlyName}: failed,";
                }
            }

            results += $"Total Time: {timerTotal.ElapsedMilliseconds / 1000}s,";
            this.Result = "Indexing results: " + results.Trim( ',' );

            if ( indexExceptions.Any() )
            {
                var exceptionList = new AggregateException( "One or more entity types failed to index.", indexExceptions );

                // Throw the aggregate exception if every entity type failed, otherwise report a warning
                // so the job shows which entity types indexed and which failed.
                if ( entityTypesToIndex.Count == indexExceptions.Count )
                {
                    throw exceptionList;
                }
                else
                {
                    throw new RockJobWarningException( "Universal Search Re-Index completed with errors.", exceptionList );
                }
            }
        }
    }
}
