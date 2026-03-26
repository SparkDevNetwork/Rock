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
using System.Linq;
using Rock.Data;
using Rock.Enums.Connection;

namespace Rock.Model
{
    public partial class ConnectionType
    {
        /// <summary>
        /// Save hook implementation for <see cref="ConnectionType"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<ConnectionType>
        {
            /// <summary>
            /// Captured during <see cref="PreSave"/> so it is still available in
            /// <see cref="PostSave"/> after EF resets the entity state post-commit.
            /// </summary>
            private bool _shouldRecalculateRequestDueAndDueSoonDates;

            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                // Capture [NotMapped] flags now; their values are not guaranteed
                // to survive the EF commit that happens before PostSave runs.
                _shouldRecalculateRequestDueAndDueSoonDates = this.Entity.ShouldRecalculateRequestDueAndDueSoonDates;

                base.PreSave();
            }

            /// <summary>
            /// Called after the save operation has been executed
            /// </summary>
            /// <remarks>
            /// This method is only called if <see cref="M:Rock.Data.EntitySaveHook`1.PreSave" /> returns
            /// without error.
            /// </remarks>
            protected override void PostSave()
            {
                if ( this.State == EntityContextState.Deleted )
                {
                    var qualifierValue = Entity.Id.ToString();
                    var rockContext = ( RockContext ) this.RockContext;
                    var attributeService = new AttributeService( rockContext );
                    var existingAttributes = attributeService.GetByEntityTypeId( new ConnectionRequest().TypeId, true )
                        .AsQueryable()
                        .Where( a =>
                           a.EntityTypeQualifierColumn.Equals( "ConnectionTypeId", StringComparison.OrdinalIgnoreCase ) &&
                           a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                       .OrderBy( a => a.Order )
                       .ThenBy( a => a.Name )
                       .ToList();

                    foreach ( var attr in existingAttributes )
                    {
                        attributeService.Delete( attr );
                        rockContext.SaveChanges();
                    }
                }
                else if ( this.State == EntityContextState.Modified )
                {
                    var originalDueDateCalculationMode = this.Entry.OriginalValues[nameof( ConnectionType.DueDateCalculationMode )].ToStringSafe().ConvertToEnum<DueDateCalculationMode>( DueDateCalculationMode.FixedDaysFromStartTypeLevel );

                    if ( originalDueDateCalculationMode != DueDateCalculationMode.FixedDaysFromStartOpportunityLevel && this.Entity.DueDateCalculationMode == DueDateCalculationMode.FixedDaysFromStartOpportunityLevel )
                    {
                        var rockContext = ( RockContext ) this.RockContext;
                        var connectionOpportunityService = new ConnectionOpportunityService( rockContext );
                        var connectionTypeAdditionalSettings = this.Entity.GetConnectionTypeAdditionalSettings();

                        var defaultOpportunityDueDateOffsetInDays = connectionTypeAdditionalSettings?.DefaultOpportunityDueDateOffsetInDays;
                        var defaultOpportunityDueSoonOffsetInDays = connectionTypeAdditionalSettings?.DefaultOpportunityDueSoonOffsetInDays;

                        var connectionOpportunitiesToUpdate = connectionOpportunityService.Queryable()
                            .Where( o => o.ConnectionTypeId == this.Entity.Id &&
                                ( ( o.RequestDueDateOffsetInDays == null || o.RequestDueDateOffsetInDays <= 0 ) ||
                                  ( o.RequestDueSoonOffsetInDays == null || o.RequestDueSoonOffsetInDays <= 0 ) ) )
                            .ToList();


                        foreach ( var opportunity in connectionOpportunitiesToUpdate )
                        {
                            if ( !opportunity.RequestDueDateOffsetInDays.HasValue || opportunity.RequestDueDateOffsetInDays.Value <= 0 )
                            {
                                opportunity.RequestDueDateOffsetInDays = defaultOpportunityDueDateOffsetInDays;
                            }
                            if ( !opportunity.RequestDueSoonOffsetInDays.HasValue || opportunity.RequestDueSoonOffsetInDays.Value <= 0 )
                            {
                                opportunity.RequestDueSoonOffsetInDays = defaultOpportunityDueSoonOffsetInDays;
                            }
                        }

                        rockContext.SaveChanges();
                    }

                    if ( _shouldRecalculateRequestDueAndDueSoonDates )
                    {
                        var rockContext = ( RockContext ) this.RockContext;
                        var connectionRequestService = new ConnectionRequestService( rockContext );

                        var connectionRequests = connectionRequestService.Queryable()
                            .Where( c => c.ConnectionTypeId == this.Entity.Id )
                            .ToList();

                        var dueCalculationMode = this.Entity.DueDateCalculationMode;

                        // Keyed by status/opportunity Id; values are (dueDateOffset, dueSoonOffset).
                        Dictionary<int, (int? DueDateOffset, int? DueSoonOffset)> statusDueDateOffsets = null;
                        Dictionary<int, (int? DueDateOffset, int? DueSoonOffset)> opportunityDueDateOffsets = null;

                        if ( dueCalculationMode == DueDateCalculationMode.DurationPerStatus )
                        {
                            statusDueDateOffsets = new ConnectionStatusService( rockContext ).Queryable()
                                .Where( s => s.ConnectionTypeId == this.Entity.Id )
                                .Select( s => new
                                {
                                    s.Id,
                                    s.RequestStatusDueDateOffsetInDays,
                                    s.RequestStatusDueSoonOffsetInDays
                                } )
                                .ToDictionary( s => s.Id, s => ( ( int? ) s.RequestStatusDueDateOffsetInDays, ( int? ) s.RequestStatusDueSoonOffsetInDays ) );
                        }
                        else if ( dueCalculationMode == DueDateCalculationMode.FixedDaysFromStartOpportunityLevel )
                        {
                            opportunityDueDateOffsets = new ConnectionOpportunityService( rockContext ).Queryable()
                                .Where( o => o.ConnectionTypeId == this.Entity.Id )
                                .Select( o => new
                                {
                                    o.Id,
                                    o.RequestDueDateOffsetInDays,
                                    o.RequestDueSoonOffsetInDays
                                } )
                                .ToDictionary( o => o.Id, o => ( ( int? ) o.RequestDueDateOffsetInDays, ( int? ) o.RequestDueSoonOffsetInDays ) );
                        }

                        foreach ( var connectionRequest in connectionRequests )
                        {
                            var createdDateTime = connectionRequest.CreatedDateTime ?? RockDateTime.Now;

                            if ( dueCalculationMode == DueDateCalculationMode.DurationPerStatus )
                            {
                                connectionRequest.DueDate = createdDateTime.AddDays( statusDueDateOffsets[connectionRequest.ConnectionStatusId].DueDateOffset ?? 7 );
                                connectionRequest.DueSoonDate = createdDateTime.AddDays( statusDueDateOffsets[connectionRequest.ConnectionStatusId].DueSoonOffset ?? 7 );
                            }
                            else if ( dueCalculationMode == DueDateCalculationMode.FixedDaysFromStartOpportunityLevel )
                            {
                                connectionRequest.DueDate = createdDateTime.AddDays( opportunityDueDateOffsets[connectionRequest.ConnectionOpportunityId].DueDateOffset ?? 7 );
                                connectionRequest.DueSoonDate = createdDateTime.AddDays( opportunityDueDateOffsets[connectionRequest.ConnectionOpportunityId].DueSoonOffset ?? 7 );
                            }
                            else
                            {
                                connectionRequest.DueDate = createdDateTime.AddDays( this.Entity.RequestDueDateOffsetInDays ?? 7 );
                                connectionRequest.DueSoonDate = createdDateTime.AddDays( this.Entity.RequestDueSoonOffsetInDays ?? 5 );
                            }
                        }

                        rockContext.SaveChanges();
                    }
                }

                base.PostSave();
            }
        }
    }
}
