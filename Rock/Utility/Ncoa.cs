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
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;
using Rock;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.SystemKey;
using Rock.Utility.Settings.SparkData;
using Rock.Utility.SparkDataApi;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Utility
{
    /// <summary>
    /// Make NCOA calls to get change of address information
    /// </summary>
    public class Ncoa
    {
        #region Get Addresses

        /// <summary>
        /// PeopleIds inside a DataView filter.
        /// </summary>
        /// <param name="dataViewId">The data view identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>Returns a directory of people IDs that result from applying the DataView filter</returns>
        public Dictionary<int, int> DataViewPeopleDirectory( int dataViewId, RockContext rockContext )
        {
            var dataViewService = new DataViewService( rockContext );
            var dataView = dataViewService.GetNoTracking( dataViewId );

            // Verify that there is not a child filter that uses this view (would result in stack-overflow error)
            if (dataViewService.IsViewInFilter( dataView.Id, dataView.DataViewFilter ))
            {
                throw new Exception( "Data View Filter issue(s): One of the filters contains a circular reference to the Data View itself." );
            }

            // Evaluate the Data View that defines the candidate population.
            List<string> errorMessages;

            var personService = new PersonService( rockContext );

            var personQuery = personService.Queryable().AsNoTracking();

            var paramExpression = personService.ParameterExpression;

            var whereExpression = dataView.GetExpression( personService, paramExpression, out errorMessages );

            if (errorMessages.Any())
            {
                throw new Exception( "Data View Filter issue(s): " + errorMessages.AsDelimited( "; " ) );
            }

            return personQuery.Where( paramExpression, whereExpression, null ).Select( p => p.Id ).ToDictionary( p => p, p => p );
        }

        /// <summary>
        /// Gets the addresses.
        /// </summary>
        /// <param name="dataViewId">The data view identifier.</param>
        /// <returns>Directory of addresses</returns>
        public Dictionary<int, PersonAddressItem> GetAddresses( int? dataViewId )
        {
            using (RockContext rockContext = new RockContext())
            {
                var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
                var homeLoc = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
                var inactiveStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() );

                if (familyGroupType != null && homeLoc != null && inactiveStatus != null)
                {
                    var groupMembers = new GroupMemberService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( m =>
                            m.Group.GroupTypeId == familyGroupType.Id && // Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY
                            m.Person.RecordStatusValueId != inactiveStatus.Id && // Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE
                            m.Group.GroupLocations.Any( gl => gl.GroupLocationTypeValueId.HasValue &&
                                     gl.GroupLocationTypeValueId == homeLoc.Id ) ); // DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME

                    var peopleHomelocation = groupMembers.Select( m => new
                    {
                        m.PersonId,
                        m.GroupId,
                        m.Person.FirstName,
                        m.Person.LastName,
                        m.Person.Aliases,
                        HomeLocation = m.Group.GroupLocations
                            .Where( gl =>
                                gl.GroupLocationTypeValueId.HasValue &&
                                gl.GroupLocationTypeValueId == homeLoc.Id ) // DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME
                            .Select( gl => new
                            {
                                gl.LocationId,
                                gl.Location.Street1,
                                gl.Location.Street2,
                                gl.Location.City,
                                gl.Location.State,
                                gl.Location.PostalCode,
                                gl.Location.Country
                            } ).FirstOrDefault()
                    } ).Where( m => m.HomeLocation != null ).DistinctBy( m => m.PersonId );

                    if (dataViewId.HasValue)
                    {
                        var dataViewQuery = DataViewPeopleDirectory( dataViewId.Value, rockContext );
                        peopleHomelocation = peopleHomelocation.Where( p => dataViewQuery.ContainsKey( p.PersonId ) );
                    }

                    return peopleHomelocation
                        .Select( g => new
                        {
                            g.PersonId,
                            HomeLocation = new PersonAddressItem()
                            {
                                PersonId = g.PersonId,
                                FamilyId = g.GroupId,
                                LocationId = g.HomeLocation.LocationId,
                                PersonAliasId = g.Aliases.Count == 0 ? 0 : g.Aliases.FirstOrDefault().Id,
                                FirstName = g.FirstName,
                                LastName = g.LastName,
                                Street1 = g.HomeLocation.Street1,
                                Street2 = g.HomeLocation.Street2,
                                City = g.HomeLocation.City,
                                State = g.HomeLocation.State,
                                PostalCode = g.HomeLocation.PostalCode,
                                Country = g.HomeLocation.Country
                            }
                        } )
                        .ToDictionary( k => k.PersonId, v => v.HomeLocation );
                }

                throw new Exception( "Get Address: Could not find expected constant, type or value" );
            }
        }

        #endregion

        #region Executing NCOA states

        /// <summary>
        /// Starts the NCOA request.
        /// </summary>
        /// <param name="sparkDataConfig">The spark data configuration.</param>
        public void Start( SparkDataConfig sparkDataConfig )
        {
            if (sparkDataConfig == null)
            {
                sparkDataConfig = GetSettings();
            }

            SparkDataApi.SparkDataApi sparkDataApi = new SparkDataApi.SparkDataApi();
            var accountStatus = sparkDataApi.CheckAccount( sparkDataConfig.SparkDataApiKey );
            switch (accountStatus)
            {
                case SparkDataApi.SparkDataApi.AccountStatus.AccountNoName:
                    throw new UnauthorizedAccessException( "Account does not have a name." );
                case SparkDataApi.SparkDataApi.AccountStatus.AccountNotFound:
                    throw new UnauthorizedAccessException( "Account not found." );
                case SparkDataApi.SparkDataApi.AccountStatus.Disabled:
                    throw new UnauthorizedAccessException( "Account is disabled." );
                case SparkDataApi.SparkDataApi.AccountStatus.EnabledCardExpired:
                    throw new UnauthorizedAccessException( "Credit card on Spark server expired." );
                case SparkDataApi.SparkDataApi.AccountStatus.EnabledNoCard:
                    throw new UnauthorizedAccessException( "No credit card found on Spark server." );
                case SparkDataApi.SparkDataApi.AccountStatus.InvalidSparkDataKey:
                    throw new UnauthorizedAccessException( "Invalid Spark Data Key." );
            }

            var addresses = GetAddresses( sparkDataConfig.NcoaSettings.PersonDataViewId );
            if (addresses.Count == 0)
            {
                sparkDataConfig.NcoaSettings.LastRunDate = RockDateTime.Now;
                sparkDataConfig.NcoaSettings.CurrentReportStatus = "Complete";
                SaveSettings( sparkDataConfig );
                return;
            }

            GroupNameTransactionKey groupNameTransactionKey;
            if ( sparkDataConfig.NcoaSettings.FileName.IsNotNullOrWhiteSpace() )
            {
                groupNameTransactionKey = sparkDataApi.NcoaRetryReport( sparkDataConfig.SparkDataApiKey, sparkDataConfig.NcoaSettings.FileName );
            }
            else
            {
                groupNameTransactionKey = sparkDataApi.NcoaInitiateReport( sparkDataConfig.SparkDataApiKey, addresses.Count, sparkDataConfig.NcoaSettings.PersonFullName );
            }

            sparkDataConfig.NcoaSettings.FileName = groupNameTransactionKey.TransactionKey;

            var credentials = sparkDataApi.NcoaGetCredentials( sparkDataConfig.SparkDataApiKey );
            var trueNcoaApi = new NcoaApi( credentials );

            trueNcoaApi.CreateFile( sparkDataConfig.NcoaSettings.FileName, groupNameTransactionKey.GroupName, out string id );
            sparkDataConfig.NcoaSettings.CurrentReportKey = id;

            trueNcoaApi.UploadAddresses( addresses, sparkDataConfig.NcoaSettings.CurrentReportKey );

            sparkDataConfig.NcoaSettings.CurrentUploadCount = addresses.Count;
            trueNcoaApi.CreateReport( sparkDataConfig.NcoaSettings.CurrentReportKey );

            // Delete previous NcoaHistory entries
            using ( RockContext rockContext = new RockContext() )
            {
                NcoaHistoryService ncoaHistoryService = new NcoaHistoryService( rockContext );
                ncoaHistoryService.DeleteRange( ncoaHistoryService.Queryable() );
                rockContext.SaveChanges();
            }

            sparkDataConfig.NcoaSettings.CurrentReportStatus = "Pending: Report";
            SaveSettings( sparkDataConfig );
        }

        /// <summary>
        /// Resume a pending report.
        /// </summary>
        /// <param name="sparkDataConfig">The spark data configuration.</param>
        public void PendingReport( SparkDataConfig sparkDataConfig = null )
        {
            if (sparkDataConfig == null)
            {
                sparkDataConfig = GetSettings();
            }

            SparkDataApi.SparkDataApi sparkDataApi = new SparkDataApi.SparkDataApi();
            var credentials = sparkDataApi.NcoaGetCredentials( sparkDataConfig.SparkDataApiKey );
            var trueNcoaApi = new NcoaApi( credentials );
            if (!trueNcoaApi.IsReportCreated( sparkDataConfig.NcoaSettings.CurrentReportKey ))
            {
                return;
            }

            string exportFileId;
            trueNcoaApi.CreateReportExport( sparkDataConfig.NcoaSettings.CurrentReportKey, out exportFileId );

            sparkDataConfig.NcoaSettings.CurrentReportExportKey = exportFileId;
            sparkDataConfig.NcoaSettings.CurrentReportStatus = "Pending: Export";
            SaveSettings( sparkDataConfig );
        }

        /// <summary>
        /// Resume a pending export.
        /// </summary>
        /// <param name="sparkDataConfig">The spark data configuration.</param>
        public void PendingExport( SparkDataConfig sparkDataConfig = null )
        {
            if (sparkDataConfig == null)
            {
                sparkDataConfig = GetSettings();
            }

            SparkDataApi.SparkDataApi sparkDataApi = new SparkDataApi.SparkDataApi();
            var credentials = sparkDataApi.NcoaGetCredentials( sparkDataConfig.SparkDataApiKey );

            var trueNcoaApi = new NcoaApi( credentials );
            if (!trueNcoaApi.IsReportExportCreated( sparkDataConfig.NcoaSettings.FileName ))
            {
                return;
            }

            List<NcoaReturnRecord> trueNcoaReturnRecords;
            trueNcoaApi.DownloadExport( sparkDataConfig.NcoaSettings.CurrentReportExportKey, out trueNcoaReturnRecords );
            var ncoaHistoryList = trueNcoaReturnRecords.Select( r => r.ToNcoaHistory() ).ToList();
            FilterDuplicateLocations( ncoaHistoryList );

            if (trueNcoaReturnRecords != null && trueNcoaReturnRecords.Count != 0)
            {

                using (var rockContext = new RockContext())
                {
                    var ncoaHistoryService = new NcoaHistoryService( rockContext );
                    ncoaHistoryService.AddRange( ncoaHistoryList );
                    rockContext.SaveChanges();
                }
            }

            ProcessNcoaResults( sparkDataConfig );

            sparkDataApi.NcoaCompleteReport( sparkDataConfig.SparkDataApiKey, sparkDataConfig.NcoaSettings.FileName, sparkDataConfig.NcoaSettings.CurrentReportExportKey );

            //Notify group
            SentNotification( sparkDataConfig, "finished" );

            sparkDataConfig.NcoaSettings.LastRunDate = RockDateTime.Now;
            sparkDataConfig.NcoaSettings.CurrentReportStatus = "Complete";
            sparkDataConfig.NcoaSettings.FileName = null;
            SaveSettings( sparkDataConfig );
        }

        /// <summary>
        /// Processes the NCOA History results.
        /// </summary>
        /// <param name="sparkDataConfig">The spark data configuration.</param>
        public void ProcessNcoaResults( SparkDataConfig sparkDataConfig = null )
        {
            if (sparkDataConfig == null)
            {
                sparkDataConfig = GetSettings();
            }

            if (sparkDataConfig.NcoaSettings.InactiveRecordReasonId == null)
            {
                throw new NullReferenceException( "Inactive Record Reason value is empty." );
            }

            // Get the inactive reason
            var inactiveReason = DefinedValueCache.Get( sparkDataConfig.NcoaSettings.InactiveRecordReasonId.Value );

            // Get minimum move distance that is required to make the person as moved
            var minMoveDistance = SystemSettings.GetValue( SystemKey.SystemSetting.NCOA_MINIMUM_MOVE_DISTANCE_TO_INACTIVATE ).AsDecimalOrNull();

            // Process the 'None' and 'NoMove' NCOA Types (these will always have an address state as 'invalid')
            var markInvalidAsPrevious = SystemSettings.GetValue( SystemKey.SystemSetting.NCOA_SET_INVALID_AS_PREVIOUS ).AsBoolean();

            // Process the '48 Month Move' NCOA Types
            var mark48MonthAsPrevious = SystemSettings.GetValue( SystemKey.SystemSetting.NCOA_SET_48_MONTH_AS_PREVIOUS ).AsBoolean();

            ProcessNcoaResults( inactiveReason, markInvalidAsPrevious, mark48MonthAsPrevious, minMoveDistance );
        }

        #endregion

        #region Process NCOA results

        /// <summary>
        /// Marks the duplicate locations as ManualUpdateRequired so that they are not processed.
        /// </summary>
        /// <param name="ncoaHistoryList">The NCOA history list.</param>
        private void FilterDuplicateLocations( List<NcoaHistory> ncoaHistoryList )
        {
            // Get all duplicate addresses/locations and sort the result by move date descending.
            var duplicateLocations = ncoaHistoryList.OrderByDescending( x => x.MoveDate ).GroupBy( x => x.PersonAliasId + "_" + x.FamilyId + "_" + x.LocationId )
          .Where( g => g.Count() > 1 )
          .ToList();

            // Find the latest valid move address
            foreach (var duplicateLocationsGroup in duplicateLocations)
            {
                bool foundLocation = false;
                foreach (var location in duplicateLocationsGroup)
                {
                    if (!foundLocation)
                    {
                        if (location.MoveType != MoveType.None && location.AddressStatus == AddressStatus.Valid)
                        {
                            foundLocation = true;
                            location.Processed = Processed.NotProcessed;
                        }
                        else
                        {
                            location.Processed = Processed.ManualUpdateRequired;
                        }
                    }
                    else
                    {
                        location.Processed = Processed.ManualUpdateRequired;
                    }
                }

                // If there is no valid move address, find latest valid address
                if (!foundLocation)
                {
                    foreach (var location in duplicateLocationsGroup)
                    {
                        if (!foundLocation)
                        {
                            if (location.AddressStatus == AddressStatus.Valid)
                            {
                                foundLocation = true;
                                location.Processed = Processed.NotProcessed;
                            }
                            else
                            {
                                location.Processed = Processed.ManualUpdateRequired;
                            }
                        }
                        else
                        {
                            location.Processed = Processed.ManualUpdateRequired;
                        }
                    }
                }

                // If there is no valid address, use the latest address
                if (!foundLocation)
                {
                    duplicateLocationsGroup.First().Processed = Processed.NotProcessed;
                }
            }
        }

        /// <summary>
        /// Processes the NCOA History results.
        /// </summary>
        /// <param name="inactiveReason">The inactive reason.</param>
        /// <param name="markInvalidAsPrevious">If invalid addresses should be marked as previous, set to <c>true</c>.</param>
        /// <param name="mark48MonthAsPrevious">if a 48 month move should be marked as previous, set to <c>true</c>.</param>
        /// <param name="minMoveDistance">The minimum move distance.</param>
        public void ProcessNcoaResults( DefinedValueCache inactiveReason, bool markInvalidAsPrevious, bool mark48MonthAsPrevious, decimal? minMoveDistance )
        {
            // Get the ID's for the "Home" and "Previous" family group location types
            int? homeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() )?.Id;
            int? previousValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid() )?.Id;

            ProcessNcoaResultsInvalidAddress( inactiveReason, markInvalidAsPrevious, mark48MonthAsPrevious, minMoveDistance, homeValueId, previousValueId );
            ProcessNcoaResults48MonthMove( inactiveReason, markInvalidAsPrevious, mark48MonthAsPrevious, minMoveDistance, homeValueId, previousValueId );
            ProcessNcoaResultsFamilyMove( inactiveReason, markInvalidAsPrevious, mark48MonthAsPrevious, minMoveDistance, homeValueId, previousValueId );
            ProcessNcoaResultsIndividualMove( inactiveReason, markInvalidAsPrevious, mark48MonthAsPrevious, minMoveDistance, homeValueId, previousValueId );
        }

        /// <summary>
        /// Processes the NCOA results: invalid address.
        /// </summary>
        /// <param name="inactiveReason">The inactive reason.</param>
        /// <param name="markInvalidAsPrevious">If invalid addresses should be marked as previous, set to <c>true</c>.</param>
        /// <param name="mark48MonthAsPrevious">if a 48 month move should be marked as previous, set to <c>true</c>.</param>
        /// <param name="minMoveDistance">The minimum move distance.</param>
        /// <param name="homeValueId">The home value identifier.</param>
        /// <param name="previousValueId">The previous value identifier.</param>
        private void ProcessNcoaResultsInvalidAddress( DefinedValueCache inactiveReason, bool markInvalidAsPrevious, bool mark48MonthAsPrevious, decimal? minMoveDistance, int? homeValueId, int? previousValueId )
        {
            List<int> ncoaIds = null;
            using (var rockContext = new RockContext())
            {
                ncoaIds = new NcoaHistoryService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( n =>
                        n.Processed == Processed.NotProcessed &&
                        n.AddressStatus == AddressStatus.Invalid )
                    .Select( n => n.Id )
                    .ToList();
            }

            foreach (int id in ncoaIds)
            {
                using (var rockContext = new RockContext())
                {
                    var ncoaHistory = new NcoaHistoryService( rockContext ).Get( id );
                    if (ncoaHistory != null)
                    {
                        var groupService = new GroupService( rockContext );
                        var groupLocationService = new GroupLocationService( rockContext );

                        var changes = new History.HistoryChangeList();

                        // If configured to mark these as previous, and we're able to mark it as previous set the status to 'Complete'
                        // otherwise set it to require a manual update
                        if (markInvalidAsPrevious && MarkAsPreviousLocation( ncoaHistory, groupLocationService, previousValueId, changes ))
                        {
                            ncoaHistory.Processed = Processed.Complete;

                            // If there were any changes, write to history
                            if (changes.Any())
                            {
                                var family = groupService.Get( ncoaHistory.FamilyId );
                                if (family != null)
                                {
                                    foreach (var fm in family.Members)
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
        }

        /// <summary>
        /// Processes the NCOA results: 48 month move.
        /// </summary>
        /// <param name="inactiveReason">The inactive reason.</param>
        /// <param name="markInvalidAsPrevious">If invalid addresses should be marked as previous, set to <c>true</c>.</param>
        /// <param name="mark48MonthAsPrevious">if a 48 month move should be marked as previous, set to <c>true</c>.</param>
        /// <param name="minMoveDistance">The minimum move distance.</param>
        /// <param name="homeValueId">The home value identifier.</param>
        /// <param name="previousValueId">The previous value identifier.</param>
        private void ProcessNcoaResults48MonthMove( DefinedValueCache inactiveReason, bool markInvalidAsPrevious, bool mark48MonthAsPrevious, decimal? minMoveDistance, int? homeValueId, int? previousValueId )
        {
            List<int> ncoaIds = null;
            // Process the '48 Month Move' NCOA Types
            using (var rockContext = new RockContext())
            {
                ncoaIds = new NcoaHistoryService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( n =>
                        n.Processed == Processed.NotProcessed &&
                        n.NcoaType == NcoaType.Month48Move )
                    .Select( n => n.Id )
                    .ToList();
            }

            foreach (int id in ncoaIds)
            {
                using (var rockContext = new RockContext())
                {
                    var ncoaHistory = new NcoaHistoryService( rockContext ).Get( id );
                    if (ncoaHistory != null)
                    {
                        var groupService = new GroupService( rockContext );
                        var groupLocationService = new GroupLocationService( rockContext );

                        var changes = new History.HistoryChangeList();

                        // If configured to mark these as previous, and we're able to mark it as previous set the status to 'Complete'
                        // otherwise set it to require a manual update
                        if (mark48MonthAsPrevious && MarkAsPreviousLocation( ncoaHistory, groupLocationService, previousValueId, changes ))
                        {
                            ncoaHistory.Processed = Processed.Complete;

                            // If there were any changes, write to history
                            if (changes.Any())
                            {
                                var family = groupService.Get( ncoaHistory.FamilyId );
                                if (family != null)
                                {
                                    foreach (var fm in family.Members)
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
        }

        /// <summary>
        /// Processes the NCOA results: family move.
        /// </summary>
        /// <param name="inactiveReason">The inactive reason.</param>
        /// <param name="markInvalidAsPrevious">If invalid addresses should be marked as previous, set to <c>true</c>.</param>
        /// <param name="mark48MonthAsPrevious">if a 48 month move should be marked as previous, set to <c>true</c>.</param>
        /// <param name="minMoveDistance">The minimum move distance.</param>
        /// <param name="homeValueId">The home value identifier.</param>
        /// <param name="previousValueId">The previous value identifier.</param>
        private void ProcessNcoaResultsFamilyMove( DefinedValueCache inactiveReason, bool markInvalidAsPrevious, bool mark48MonthAsPrevious, decimal? minMoveDistance, int? homeValueId, int? previousValueId )
        {
            List<int> ncoaIds = null;
            // Process 'Move' NCOA Types (The 'Family' move types will be processed first)
            using (var rockContext = new RockContext())
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

            foreach (int id in ncoaIds)
            {
                using (var rockContext = new RockContext())
                {
                    // Get the NCOA record and make sure it still hasn't been processed
                    var ncoaHistory = new NcoaHistoryService( rockContext ).Get( id );
                    if (ncoaHistory != null && ncoaHistory.Processed == Processed.NotProcessed)
                    {
                        var ncoaHistoryService = new NcoaHistoryService( rockContext );
                        var groupService = new GroupService( rockContext );
                        var groupLocationService = new GroupLocationService( rockContext );
                        var locationService = new LocationService( rockContext );
                        var personService = new PersonService( rockContext );

                        var familyChanges = new History.HistoryChangeList();

                        // If we're able to mark the existing address as previous and successfully create a new home address..
                        if (MarkAsPreviousLocation( ncoaHistory, groupLocationService, previousValueId, familyChanges ) &&
                            AddNewHomeLocation( ncoaHistory, locationService, groupLocationService, homeValueId, familyChanges ))
                        {
                            // set the status to 'Complete'
                            ncoaHistory.Processed = Processed.Complete;

                            // Look for any other moves for the same family and to same address, and set their status to complete as well
                            foreach (var ncoaIndividual in ncoaHistoryService
                                .Queryable().Where( n =>
                                    n.Processed == Processed.NotProcessed &&
                                    n.NcoaType == NcoaType.Move &&
                                    n.FamilyId == ncoaHistory.FamilyId &&
                                    n.Id != ncoaHistory.Id &&
                                    n.UpdatedStreet1 == ncoaHistory.UpdatedStreet1 ))
                            {
                                ncoaIndividual.Processed = Processed.Complete;
                            }

                            // If there were any changes, write to history and check to see if person should be inactivated
                            if (familyChanges.Any())
                            {
                                var family = groupService.Get( ncoaHistory.FamilyId );
                                if (family != null)
                                {
                                    foreach (var fm in family.Members)
                                    {
                                        if (ncoaHistory.MoveDistance.HasValue && minMoveDistance.HasValue &&
                                            ncoaHistory.MoveDistance.Value >= minMoveDistance.Value)
                                        {
                                            History.HistoryChangeList personChanges;

                                            personService.InactivatePerson( fm.Person, inactiveReason,
                                                $"Received a Change of Address (NCOA) notice that was for more than {minMoveDistance} miles away.", out personChanges );

                                            if (personChanges.Any())
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
        }

        /// <summary>
        /// Processes the NCOA results: individual move.
        /// </summary>
        /// <param name="inactiveReason">The inactive reason.</param>
        /// <param name="markInvalidAsPrevious">If invalid addresses should be marked as previous, set to <c>true</c>.</param>
        /// <param name="mark48MonthAsPrevious">if a 48 month move should be marked as previous, set to <c>true</c>.</param>
        /// <param name="minMoveDistance">The minimum move distance.</param>
        /// <param name="homeValueId">The home value identifier.</param>
        /// <param name="previousValueId">The previous value identifier.</param>
        private void ProcessNcoaResultsIndividualMove( DefinedValueCache inactiveReason, bool markInvalidAsPrevious, bool mark48MonthAsPrevious, decimal? minMoveDistance, int? homeValueId, int? previousValueId )
        {
            List<int> ncoaIds = null;
            // Process 'Move' NCOA Types (For the remaining Individual move types that weren't updated with the family move)
            using (var rockContext = new RockContext())
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

            foreach (int id in ncoaIds)
            {
                using (var rockContext = new RockContext())
                {
                    // Get the NCOA record and make sure it still hasn't been processed
                    var ncoaHistory = new NcoaHistoryService( rockContext ).Get( id );
                    if (ncoaHistory != null && ncoaHistory.Processed == Processed.NotProcessed)
                    {
                        var ncoaHistoryService = new NcoaHistoryService( rockContext );
                        var groupMemberService = new GroupMemberService( rockContext );
                        var personAliasService = new PersonAliasService( rockContext );
                        var groupService = new GroupService( rockContext );
                        var groupLocationService = new GroupLocationService( rockContext );
                        var locationService = new LocationService( rockContext );
                        var personService = new PersonService( rockContext );

                        var changes = new History.HistoryChangeList();

                        // Default the status to requiring a manual update (we might change this though)
                        ncoaHistory.Processed = Processed.ManualUpdateRequired;

                        // Find the existing family 
                        var family = groupService.Get( ncoaHistory.FamilyId );

                        // If there's only one person in the family
                        if (family.Members.Count == 1)
                        {
                            // And that person is the same as the move record's person then we can process it.
                            var personAlias = personAliasService.Get( ncoaHistory.PersonAliasId );
                            var familyMember = family.Members.First();
                            if (personAlias != null && familyMember.PersonId == personAlias.PersonId)
                            {
                                // If were able to mark their existing address as previous and add a new updated Home address, 
                                // then set the status to complete (otherwise leave it as needing a manual update).
                                if (MarkAsPreviousLocation( ncoaHistory, groupLocationService, previousValueId, changes ) &&
                                    AddNewHomeLocation( ncoaHistory, locationService, groupLocationService, homeValueId, changes ))
                                {
                                    ncoaHistory.Processed = Processed.Complete;

                                    // Look for any other moves for the same person to same address, and set their process to complete also
                                    foreach (var ncoaIndividual in ncoaHistoryService
                                        .Queryable().Where( n =>
                                            n.Processed == Processed.NotProcessed &&
                                            n.NcoaType == NcoaType.Move &&
                                            n.MoveType == MoveType.Individual &&
                                            n.PersonAliasId == ncoaHistory.PersonAliasId &&
                                            n.Id != ncoaHistory.Id &&
                                            n.UpdatedStreet1 == ncoaHistory.UpdatedStreet1 ))
                                    {
                                        ncoaIndividual.Processed = Processed.Complete;
                                    }

                                    // If there were any changes, write to history and check to see if person should be inactivated
                                    if (changes.Any())
                                    {
                                        if (ncoaHistory.MoveDistance.HasValue && minMoveDistance.HasValue &&
                                            ncoaHistory.MoveDistance.Value >= minMoveDistance.Value)
                                        {
                                            History.HistoryChangeList personChanges;

                                            personService.InactivatePerson( familyMember.Person, inactiveReason,
                                                $"Received a Change of Address (NCOA) notice that was for more than {minMoveDistance} miles away.", out personChanges );

                                            if (personChanges.Any())
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

        /// <summary>
        /// Marks as address as previous location.
        /// </summary>
        /// <param name="ncoaHistory">The NCOA history.</param>
        /// <param name="groupLocationService">The group location service.</param>
        /// <param name="previousValueId">The previous value identifier.</param>
        /// <param name="changes">The changes.</param>
        /// <returns></returns>
        public bool MarkAsPreviousLocation( NcoaHistory ncoaHistory, GroupLocationService groupLocationService, int? previousValueId, History.HistoryChangeList changes )
        {
            if (ncoaHistory.LocationId.HasValue && previousValueId.HasValue)
            {
                var groupLocation = groupLocationService.Queryable()
                    .Where( gl =>
                        gl.GroupId == ncoaHistory.FamilyId &&
                        gl.LocationId == ncoaHistory.LocationId &&
                        gl.Location.Street1 == ncoaHistory.OriginalStreet1 )
                    .FirstOrDefault();
                if (groupLocation != null)
                {
                    if (groupLocation.GroupLocationTypeValueId != previousValueId.Value)
                    {
                        changes.AddChange( History.HistoryVerb.Modify, History.HistoryChangeType.Property, $"Location Type for {groupLocation.Location} " ).SetNewValue( "Previous" ).SourceOfChange = "NCOA Request";

                        groupLocation.GroupLocationTypeValueId = previousValueId.Value;
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Adds the new home location.
        /// </summary>
        /// <param name="ncoaHistory">The NCOA history.</param>
        /// <param name="locationService">The location service.</param>
        /// <param name="groupLocationService">The group location service.</param>
        /// <param name="homeValueId">The home value identifier.</param>
        /// <param name="changes">The changes.</param>
        /// <returns></returns>
        private bool AddNewHomeLocation( NcoaHistory ncoaHistory, LocationService locationService, GroupLocationService groupLocationService, int? homeValueId, History.HistoryChangeList changes )
        {
            if (homeValueId.HasValue)
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

                changes.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Property, "Location" ).SetNewValue( groupLocation.Location.ToString() ).SourceOfChange = "NCOA Request";

                return true;
            }

            return false;
        }

        #endregion

        /// <summary>
        /// Sends the notification that NCOA finished
        /// </summary>
        /// <param name="sparkDataConfig">The spark data configuration.</param>
        /// <param name="status">The status to put in the notification.</param>
        public void SentNotification( SparkDataConfig sparkDataConfig, string status )
        {
            if (!sparkDataConfig.GlobalNotificationApplicationGroupId.HasValue || sparkDataConfig.GlobalNotificationApplicationGroupId.Value == 0)
            {
                return;
            }

            var recipients = new List<RecipientData>();
            using (RockContext rockContext = new RockContext())
            {
                Group group = new GroupService( rockContext ).GetNoTracking( sparkDataConfig.GlobalNotificationApplicationGroupId.Value );

                foreach (var groupMember in group.Members)
                {
                    if (groupMember.GroupMemberStatus == GroupMemberStatus.Active)
                    {
                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                        mergeFields.Add( "Person", groupMember.Person );
                        mergeFields.Add( "GroupMember", groupMember );
                        mergeFields.Add( "Group", groupMember.Group );
                        mergeFields.Add( "SparkDataService", "National Change of Address (NCOA)" );
                        mergeFields.Add( "SparkDataConfig", sparkDataConfig );
                        mergeFields.Add( "Status", status );
                        recipients.Add( new RecipientData( groupMember.Person.Email, mergeFields ) );
                    }
                }

                SystemEmailService emailService = new SystemEmailService( rockContext );
                SystemEmail systemEmail = emailService.GetNoTracking( SystemGuid.SystemEmail.SPARK_DATA_NOTIFICATION.AsGuid() );

                var emailMessage = new RockEmailMessage( systemEmail.Guid );
                emailMessage.SetRecipients( recipients );
                emailMessage.Send();
            }
        }

        #region Get/Set Settings
        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <returns>The spark data configuration.</returns>
        public static SparkDataConfig GetSettings( SparkDataConfig sparkDataConfig = null )
        {
            // Get Spark Data settings
            if (sparkDataConfig == null)
            {
                sparkDataConfig = Rock.Web.SystemSettings.GetValue( SystemSetting.SPARK_DATA ).FromJsonOrNull<SparkDataConfig>() ?? new SparkDataConfig();
            }

            if (sparkDataConfig.NcoaSettings == null)
            {
                sparkDataConfig.NcoaSettings = new NcoaSettings();
            }

            if (sparkDataConfig.Messages == null)
            {
                sparkDataConfig.Messages = new Extension.FixedSizeList<string>( 30 );
            }

            return sparkDataConfig;
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        /// <param name="sparkDataConfig">The spark data configuration.</param>
        public static void SaveSettings( SparkDataConfig sparkDataConfig )
        {
            Rock.Web.SystemSettings.SetValue( SystemSetting.SPARK_DATA, sparkDataConfig.ToJson() );
        }

        #endregion

    }
}
