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
using System.Data.Entity;
using System.Linq;

using Rock;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service class for <see cref="InteractionEntity"/>. This provides various
    /// methods to work with <see cref="InteractionEntity"/> records.
    /// </summary>
    internal partial class InteractionEntityService
    {
        /// <summary>
        /// Updates any missing <see cref="InteractionEntity.InteractionId"/>
        /// values.
        /// </summary>
        /// <param name="cutoffDateTime">Only consider records that have been created since this timestamp.</param>
        /// <param name="maxItemsToProcess">If not <c>null</c> then processing will stop after this many items have been updated.</param>
        /// <param name="commandTimeout">The number of seconds to configure the database context to use for timeouts.</param>
        internal static int UpdateMissingInteractionIds( DateTime cutoffDateTime, int? maxItemsToProcess, int? commandTimeout )
        {
            List<InteractionEntity> interactionEntities;
            int itemsProcessed = 0;
            int itemsUpdated = 0;

            using ( var rockContext = new RockContext() )
            {
                if ( commandTimeout.HasValue )
                {
                    rockContext.Database.CommandTimeout = commandTimeout;
                }

                interactionEntities = rockContext.Set<InteractionEntity>()
                    .Where( ie => ie.CreatedDateTime >= cutoffDateTime
                        && !ie.InteractionId.HasValue )
                    .ToList();

                while ( interactionEntities.Any() && ( !maxItemsToProcess.HasValue || itemsProcessed < maxItemsToProcess.Value ) )
                {
                    // Work in batches of 100.
                    var interactionEntitiesChunk = interactionEntities.Take( 100 ).ToList();
                    var interactionGuids = interactionEntitiesChunk.Select( ie => ie.InteractionGuid ).ToList();

                    interactionEntities = interactionEntities.Skip( 100 ).ToList();

                    // Lookup the interaction identifiers by their Guid values.
                    var interactionLookup = new InteractionService( rockContext )
                        .Queryable()
                        .Where( i => interactionGuids.Contains( i.Guid ) )
                        .Select( i => new
                        {
                            i.Id,
                            i.Guid
                        } )
                        .ToDictionary( i => i.Guid, i => i.Id );

                    // Update each InteractionId if we found the guid.
                    foreach ( var interactionEntity in interactionEntitiesChunk )
                    {
                        if ( interactionLookup.TryGetValue( interactionEntity.InteractionGuid, out var interactionId ) )
                        {
                            interactionEntity.InteractionId = interactionId;
                        }
                    }

                    itemsProcessed += interactionEntitiesChunk.Count;
                }

                // This isn't a full IEntity so we don't need any of the normal
                // pre and post processing logic.
                var result = rockContext.SaveChanges( new SaveChangesArgs
                {
                    DisablePrePostProcessing = true
                } );

                itemsUpdated = result.RecordsUpdated;
            }

            return itemsUpdated;
        }

        /// <summary>
        /// Adds a set of <see cref="InteractionEntity"/> objects to the database
        /// that represent the passed items.
        /// </summary>
        /// <param name="items">The data representing the items to create.</param>
        internal static void AddRelatedEntities( IList<(int EntityTypeId, int EntityId, Guid InteractionGuid)> items )
        {
            var now = RockDateTime.Now;

            while ( items.Any() )
            {
                try
                {
                    var batch = items.Take( 250 );
                    items = items.Skip( 250 ).ToList();

                    // Create all the records in this batch.
                    using ( var rockContext = new RockContext() )
                    {
                        var dbSet = rockContext.Set<InteractionEntity>();

                        var itemsToAdd = batch.Select( item => new InteractionEntity
                        {
                            EntityTypeId = item.EntityTypeId,
                            EntityId = item.EntityId,
                            InteractionGuid = item.InteractionGuid,
                            CreatedDateTime = now,
                        } );

                        dbSet.AddRange( itemsToAdd );

                        // We don't need pre or post processing on this.
                        rockContext.SaveChanges( new SaveChangesArgs
                        {
                            DisablePrePostProcessing = true
                        } );
                    }
                }
                catch ( Exception ex )
                {
                    // Log the exception but keep going.
                    ExceptionLogService.LogException( ex );
                }
            }
        }
    }
}
