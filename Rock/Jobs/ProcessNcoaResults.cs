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
using System.Web;

using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to process NCOA results
    /// </summary>
    [DefinedValueField(Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON, "Inactive Reason", 
        "The reason to use when inactivating people due to move that is beyond the configured number of miles.", true, false, 
        Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_REASON_MOVED, "", 0 )]
    [DisallowConcurrentExecution]
    public class ProcessNcoaResults : RockBlock, IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public ProcessNcoaResults()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void Execute( IJobExecutionContext context )
        {
            // Get the inactive reason
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var inactiveReason = DefinedValueCache.Read( dataMap.GetString( "InactiveReason" ).AsGuid() );

            List<int> ncoaIds = null;

            var minMoveDistance = SystemSettings.GetValue( SystemKey.SystemSetting.NCOA_MINIMUM_MOVE_DISTANCE_TO_INACTIVATE ).AsDecimalOrNull();

            // Get the ID's for the "Home" and "Previous" family group location types
            int? homeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() )?.Id;
            int? previousValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid() )?.Id;

            // Process the 'None' and 'NoMove' NCOA Types (these will always have an address state as 'invalid')
            var markInvalidAsPrevious = SystemSettings.GetValue( SystemKey.SystemSetting.NCOA_SET_INVALID_AS_PREVIOUS ).AsBoolean();
            using ( var rockContext = new RockContext() )
            {
                ncoaIds = new NcoaHistoryService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( n =>
                        n.Processed == Processed.NotProcessed &&
                        n.AddressStatus == AddressStatus.Invalid )
                    .Select( n => n.Id )
                    .ToList();
            }

            foreach ( int id in ncoaIds )
            {
                using ( var rockContext = new RockContext() )
                {
                    var ncoaHistory = new NcoaHistoryService( rockContext ).Get( id );
                    if ( ncoaHistory != null )
                    {
                        var groupService = new GroupService( rockContext );
                        var groupLocationService = new GroupLocationService( rockContext );

                        var changes = new List<string>();

                        // If configured to mark these as previous, and we're able to mark it as previous set the status to 'Complete'
                        // otherwise set it to require a manual update
                        if ( markInvalidAsPrevious && MarkAsPreviousLocation( ncoaHistory, groupLocationService, previousValueId, changes ) )
                        {
                            ncoaHistory.Processed = Processed.Complete;

                            // If there were any changes, write to history
                            if ( changes.Any() )
                            {
                                var family = groupService.Get( ncoaHistory.FamilyId );
                                if ( family != null )
                                {
                                    foreach ( var fm in family.Members )
                                    {
                                        HistoryService.SaveChanges(
                                            rockContext,
                                            typeof( Person ),
                                            Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
                                            fm.PersonId,
                                            changes,
                                            family.Name,
                                            typeof( Group ),
                                            family.Id,
                                            false );
                                    }
                                }
                            }
                        }
                        else
                        {
                            ncoaHistory.Processed = Processed.ManualUpdateRequired;
                        }

                        rockContext.SaveChanges();
                    }
                }
            }

            // Process the '48 Month Move' NCOA Types
            var mark48MonthAsPrevious = SystemSettings.GetValue( SystemKey.SystemSetting.NCOA_SET_48_MONTH_AS_PREVIOUS ).AsBoolean();
            using ( var rockContext = new RockContext() )
            {
                ncoaIds = new NcoaHistoryService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( n =>
                        n.Processed == Processed.NotProcessed &&
                        n.NcoaType == NcoaType.Month48Move )
                    .Select( n => n.Id )
                    .ToList();
            }

            foreach ( int id in ncoaIds )
            {
                using ( var rockContext = new RockContext() )
                {
                    var ncoaHistory = new NcoaHistoryService( rockContext ).Get( id );
                    if ( ncoaHistory != null )
                    {
                        var groupService = new GroupService( rockContext );
                        var groupLocationService = new GroupLocationService( rockContext );

                        var changes = new List<string>();

                        // If configured to mark these as previous, and we're able to mark it as previous set the status to 'Complete'
                        // otherwise set it to require a manual update
                        if ( mark48MonthAsPrevious && MarkAsPreviousLocation( ncoaHistory, groupLocationService, previousValueId, changes ) )
                        {
                            ncoaHistory.Processed = Processed.Complete;

                            // If there were any changes, write to history
                            if ( changes.Any() )
                            {
                                var family = groupService.Get( ncoaHistory.FamilyId );
                                if ( family != null )
                                {
                                    foreach ( var fm in family.Members )
                                    {
                                        HistoryService.SaveChanges(
                                            rockContext,
                                            typeof( Person ),
                                            Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
                                            fm.PersonId,
                                            changes,
                                            family.Name,
                                            typeof( Group ),
                                            family.Id,
                                            false );
                                    }
                                }
                            }
                        }
                        else
                        {
                            ncoaHistory.Processed = Processed.ManualUpdateRequired;
                        }

                        rockContext.SaveChanges();

                    }
                }
            }

            // Process 'Move' NCOA Types (The 'Family' move types will be processed first)
            using ( var rockContext = new RockContext() )
            {
                ncoaIds = new NcoaHistoryService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( n =>
                        n.Processed == Processed.NotProcessed &&
                        n.NcoaType == NcoaType.Move &&
                        n.MoveType == MoveType.Family )
                    .Select( n => n.Id )
                    .ToList();
            }

            foreach ( int id in ncoaIds )
            {
                using ( var rockContext = new RockContext() )
                {
                    // Get the NCOA record and make sure it still hasn't been processed
                    var ncoaHistory = new NcoaHistoryService( rockContext ).Get( id );
                    if ( ncoaHistory != null && ncoaHistory.Processed == Processed.NotProcessed )
                    {
                        var ncoaHistoryService = new NcoaHistoryService( rockContext );
                        var groupService = new GroupService( rockContext );
                        var groupLocationService = new GroupLocationService( rockContext );
                        var locationService = new LocationService( rockContext );
                        var personService = new PersonService( rockContext );

                        var familyChanges = new List<string>();

                        // If we're able to mark the existing address as previous and successfully create a new home address..
                        if ( MarkAsPreviousLocation( ncoaHistory, groupLocationService, previousValueId, familyChanges ) &&
                            AddNewHomeLocation( ncoaHistory, locationService, groupLocationService, homeValueId, familyChanges ) )
                        {
                            // set the status to 'Complete'
                            ncoaHistory.Processed = Processed.Complete;

                            // Look for any other moves for the same family and to same address, and set their status to complete as well
                            foreach ( var ncoaIndividual in ncoaHistoryService
                                .Queryable().Where( n =>
                                    n.Processed == Processed.NotProcessed &&
                                    n.NcoaType == NcoaType.Move &&
                                    n.FamilyId == ncoaHistory.FamilyId &&
                                    n.Id != ncoaHistory.Id &&
                                    n.UpdatedStreet1 == ncoaHistory.UpdatedStreet1 ) )
                            {
                                ncoaIndividual.Processed = Processed.Complete;
                            }

                            // If there were any changes, write to history and check to see if person should be inactivated
                            if ( familyChanges.Any() )
                            {
                                var family = groupService.Get( ncoaHistory.FamilyId );
                                if ( family != null )
                                {
                                    foreach ( var fm in family.Members )
                                    {
                                        if ( ncoaHistory.MoveDistance.HasValue && minMoveDistance.HasValue &&
                                            ncoaHistory.MoveDistance.Value >= minMoveDistance.Value )
                                        {
                                            var personChanges = personService.InactivatePerson( fm.Person, inactiveReason,
                                                $"Received a Change of Address (NCOA) notice that was for more than {minMoveDistance} miles away." );
                                            if ( personChanges.Any() )
                                            {
                                                HistoryService.SaveChanges(
                                                    rockContext,
                                                    typeof( Person ),
                                                    Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                                                    fm.PersonId,
                                                    personChanges,
                                                    false );
                                            }
                                        }

                                        HistoryService.SaveChanges(
                                        rockContext,
                                        typeof( Person ),
                                        Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
                                        fm.PersonId,
                                        familyChanges,
                                        family.Name,
                                        typeof( Group ),
                                        family.Id,
                                        false );
                                    }
                                }
                            }
                        }
                        else
                        {
                            ncoaHistory.Processed = Processed.ManualUpdateRequired;
                        }

                        rockContext.SaveChanges();
                    }
                }
            }

            // Process 'Move' NCOA Types (For the remaining Individual move types that weren't updated with the family move)
            using ( var rockContext = new RockContext() )
            {
                ncoaIds = new NcoaHistoryService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( n =>
                        n.Processed == Processed.NotProcessed &&
                        n.NcoaType == NcoaType.Move &&
                        n.MoveType == MoveType.Individual )
                    .Select( n => n.Id )
                    .ToList();
            }

            foreach ( int id in ncoaIds )
            {
                using ( var rockContext = new RockContext() )
                {
                    // Get the NCOA record and make sure it still hasn't been processed
                    var ncoaHistory = new NcoaHistoryService( rockContext ).Get( id );
                    if ( ncoaHistory != null && ncoaHistory.Processed == Processed.NotProcessed )
                    {
                        var ncoaHistoryService = new NcoaHistoryService( rockContext );
                        var groupMemberService = new GroupMemberService( rockContext );
                        var personAliasService = new PersonAliasService( rockContext );
                        var groupService = new GroupService( rockContext );
                        var groupLocationService = new GroupLocationService( rockContext );
                        var locationService = new LocationService( rockContext );
                        var personService = new PersonService( rockContext );

                        var changes = new List<string>();

                        // Default the status to requiring a manual update (we might change this though)
                        ncoaHistory.Processed = Processed.ManualUpdateRequired;

                        // Find the existing family 
                        var family = groupService.Get( ncoaHistory.FamilyId );

                        // If there's only one person in the family
                        if ( family.Members.Count == 1 )
                        {
                            // And that person is the same as the move record's person then we can process it.
                            var personAlias = personAliasService.Get( ncoaHistory.PersonAliasId );
                            var familyMember = family.Members.First();
                            if ( personAlias != null && familyMember.PersonId == personAlias.PersonId )
                            {
                                // If were able to mark their existing address as previous and add a new updated Home address, 
                                // then set the status to complete (otherwise leave it as needing a manual update).
                                if ( MarkAsPreviousLocation( ncoaHistory, groupLocationService, previousValueId, changes ) &&
                                    AddNewHomeLocation( ncoaHistory, locationService, groupLocationService, homeValueId, changes ) )
                                {
                                    ncoaHistory.Processed = Processed.Complete;

                                    // Look for any other moves for the same person to same address, and set their process to complete also
                                    foreach ( var ncoaIndividual in ncoaHistoryService
                                        .Queryable().Where( n =>
                                            n.Processed == Processed.NotProcessed &&
                                            n.NcoaType == NcoaType.Move &&
                                            n.MoveType == MoveType.Individual &&
                                            n.PersonAliasId == ncoaHistory.PersonAliasId &&
                                            n.Id != ncoaHistory.Id &&
                                            n.UpdatedStreet1 == ncoaHistory.UpdatedStreet1 ) )
                                    {
                                        ncoaIndividual.Processed = Processed.Complete;
                                    }

                                    // If there were any changes, write to history and check to see if person should be inactivated
                                    if ( changes.Any() )
                                    {
                                        if ( ncoaHistory.MoveDistance.HasValue && minMoveDistance.HasValue &&
                                            ncoaHistory.MoveDistance.Value >= minMoveDistance.Value )
                                        {
                                            var personChanges = personService.InactivatePerson( familyMember.Person, inactiveReason,
                                            $"Received a Change of Address (NCOA) notice that was for more than {minMoveDistance} miles away." );
                                            if ( personChanges.Any() )
                                            {
                                                HistoryService.SaveChanges(
                                                    rockContext,
                                                    typeof( Person ),
                                                    Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                                                    familyMember.PersonId,
                                                    personChanges,
                                                    false );
                                            }
                                        }

                                        HistoryService.SaveChanges(
                                            rockContext,
                                            typeof( Person ),
                                            Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
                                            familyMember.PersonId,
                                            changes,
                                            family.Name,
                                            typeof( Group ),
                                            family.Id,
                                            false );
                                    }
                                }
                            }
                        }

                        rockContext.SaveChanges();
                    }
                }
            }
        }

        private bool MarkAsPreviousLocation( NcoaHistory ncoaHistory, GroupLocationService groupLocationService, int? previousValueId, List<string> changes )
        {
            if ( ncoaHistory.LocationId.HasValue && previousValueId.HasValue )
            {
                var groupLocation = groupLocationService.Queryable()
                    .Where( gl =>
                        gl.GroupId == ncoaHistory.FamilyId &&
                        gl.LocationId == ncoaHistory.LocationId &&
                        gl.Location.Street1 == ncoaHistory.OriginalStreet1 )
                    .FirstOrDefault();
                if ( groupLocation != null )
                {
                    if ( groupLocation.GroupLocationTypeValueId != previousValueId.Value )
                    {
                        changes.Add( $"Modifed Location Type for <span class='field-name'>{groupLocation.Location}</span> to <span class='field-value'>Previous</span> due to NCOA Request." );
                        groupLocation.GroupLocationTypeValueId = previousValueId.Value;
                    }

                    return true;
                }
            }

            return false;
        }

        private bool AddNewHomeLocation( NcoaHistory ncoaHistory, LocationService locationService, GroupLocationService groupLocationService, int? homeValueId, List<string> changes )
        {
            if ( homeValueId.HasValue )
            {
                var location = locationService.Get(
                    ncoaHistory.UpdatedStreet1,
                    ncoaHistory.UpdatedStreet2,
                    ncoaHistory.UpdatedCity,
                    ncoaHistory.UpdatedState,
                    ncoaHistory.UpdatedPostalCode,
                    ncoaHistory.UpdatedCountry );

                var groupLocation = new GroupLocation();
                groupLocation.Location = location;
                groupLocation.GroupId = ncoaHistory.FamilyId;
                groupLocation.GroupLocationTypeValueId = homeValueId.Value;
                groupLocation.IsMailingLocation = true;
                groupLocationService.Add( groupLocation );

                changes.Add( $"Added <span class='field-name'>{groupLocation.Location}</span> as a new <span class'field-value'>Home</span> Location Type due to NCOA Request." );

                return true;
            }

            return false;
        }
    }
}
