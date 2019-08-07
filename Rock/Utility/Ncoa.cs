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
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Communication;
using Rock.Data;
using Rock.Jobs;
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
        /// Get the people's Ids inside a DataView filter.
        /// </summary>
        /// <param name="dataViewId">The data view identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>Returns a directory of people IDs that result from applying the DataView filter</returns>
        public Dictionary<int, int> DataViewPeopleDirectory( int dataViewId, RockContext rockContext )
        {
            var dataViewService = new DataViewService( rockContext );
            var dataView = dataViewService.GetNoTracking( dataViewId );

            // Verify that there is not a child filter that uses this view (would result in stack-overflow error)
            if ( dataViewService.IsViewInFilter( dataView.Id, dataView.DataViewFilter ) )
            {
                throw new Exception( "Data View Filter issue(s): One of the filters contains a circular reference to the Data View itself." );
            }

            // Evaluate the Data View that defines the candidate population.
            List<string> errorMessages;

            var personService = new PersonService( rockContext );

            var personQuery = personService.Queryable().AsNoTracking();

            var paramExpression = personService.ParameterExpression;

            var whereExpression = dataView.GetExpression( personService, paramExpression, out errorMessages );

            if ( errorMessages.Any() )
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
            if ( !dataViewId.HasValue )
            {
                throw new Exception( "Get Address: No Person Data View has been set. Please check System Settings > Spark Data Settings." );
            }

            using ( RockContext rockContext = new RockContext() )
            {
                var familyGroupType = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
                var homeLoc = DefinedValueCache.Get( SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
                var inactiveStatus = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() );

                if ( familyGroupType != null && homeLoc != null && inactiveStatus != null )
                {
                    var groupMembers = new GroupMemberService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( m =>
                            m.Group.GroupTypeId == familyGroupType.Id && // SystemGuid.GroupType.GROUPTYPE_FAMILY
                            m.Person.RecordStatusValueId != inactiveStatus.Id && // SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE
                            m.Group.GroupLocations.Any( gl => gl.GroupLocationTypeValueId.HasValue &&
                                     gl.GroupLocationTypeValueId == homeLoc.Id && // DefinedValueCache.Get( SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME
                                     ( gl.Location.Country == null ||
                                     gl.Location.Country == string.Empty ||
                                     gl.Location.Country.ToUpper() == "US" ) &&
                                     ( gl.Location.State != null &&
                                     gl.Location.State != string.Empty ) &&
                                     gl.Location.Street1 != null &&
                                     gl.Location.Street1 != string.Empty ) );

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
                                gl.GroupLocationTypeValueId == homeLoc.Id ) // DefinedValueCache.Get( SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME
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

                    if ( dataViewId.HasValue )
                    {
                        var dataViewQuery = DataViewPeopleDirectory( dataViewId.Value, rockContext );
                        peopleHomelocation = peopleHomelocation.Where( p => dataViewQuery.ContainsKey( p.PersonId ) );
                    }

                    var definedType = DefinedTypeCache.Get( new Guid( SystemGuid.DefinedType.LOCATION_ADDRESS_STATE ) );
                    var stateList = definedType
                        .DefinedValues
                        .Where( v => v.ContainsKey( "Country" ) && v["Country"] != null )
                        .Select( v => new { State = v.Value, Country = v["Country"], Description = v.Description } ).ToLookup( v => v.Description, StringComparer.OrdinalIgnoreCase );

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
                                State = stateList.Contains( g.HomeLocation.State ) ? stateList[g.HomeLocation.State].First().State : g.HomeLocation.State,
                                PostalCode = g.HomeLocation.PostalCode,
                                Country = g.HomeLocation.Country.IsNullOrWhiteSpace() ? "US" : g.HomeLocation.Country
                            }
                        } )
                        .Where( g => g.HomeLocation.State.IsNotNullOrWhiteSpace() && g.HomeLocation.State.Length == 2 )
                        .ToDictionary( k => k.PersonId, v => v.HomeLocation );
                }

                throw new Exception( "Get Address: Could not find expected constant, type or value" );
            }
        }

        #endregion

        #region Executing NCOA states

        /// <summary>
        /// Starts the NCOA request: Check if there is a valid credit card on the Spark Data server. If there is a valid credit card, then get the addresses, initialize a report on NCOA, upload the addresses to NCOA and delete the previous NcoaHistory data.
        /// </summary>
        /// <param name="sparkDataConfig">The spark data configuration.</param>
        public void Start( SparkDataConfig sparkDataConfig )
        {
            if ( sparkDataConfig == null )
            {
                sparkDataConfig = GetSettings();
            }

            SparkDataApi.SparkDataApi sparkDataApi = new SparkDataApi.SparkDataApi();
            var accountStatus = sparkDataApi.CheckAccount( sparkDataConfig.SparkDataApiKey );
            switch ( accountStatus )
            {
                case SparkDataApi.SparkDataApi.AccountStatus.AccountNoName:
                    throw new NoRetryException( "Init NCOA: Account does not have a name." );
                case SparkDataApi.SparkDataApi.AccountStatus.AccountNotFound:
                    throw new NoRetryException( "Init NCOA: Account not found." );
                case SparkDataApi.SparkDataApi.AccountStatus.Disabled:
                    throw new NoRetryException( "Init NCOA: Account is disabled." );
                case SparkDataApi.SparkDataApi.AccountStatus.EnabledCardExpired:
                    throw new NoRetryException( "Init NCOA: Credit card on Spark server expired." );
                case SparkDataApi.SparkDataApi.AccountStatus.EnabledNoCard:
                    throw new NoRetryException( "Init NCOA: No credit card found on Spark server." );
                case SparkDataApi.SparkDataApi.AccountStatus.InvalidSparkDataKey:
                    throw new NoRetryException( "Init NCOA: Invalid Spark Data Key." );
            }

            var addresses = GetAddresses( sparkDataConfig.NcoaSettings.PersonDataViewId );
            if ( addresses.Count < SparkDataConfig.NCOA_MIN_ADDRESSES )
            {
                throw new NoRetryException( string.Format( "Init NCOA: Only {0} addresses were selected to be processed. NCOA will not run because it is below the minimum of {1} addresses.", addresses.Count, SparkDataConfig.NCOA_MIN_ADDRESSES ) );
            }

            GroupNameTransactionKey groupNameTransactionKey = null;
            if ( sparkDataConfig.NcoaSettings.FileName.IsNotNullOrWhiteSpace() )
            {
                groupNameTransactionKey = sparkDataApi.NcoaRetryReport( sparkDataConfig.SparkDataApiKey, sparkDataConfig.NcoaSettings.FileName );
            }

            if ( groupNameTransactionKey == null )
            {
                groupNameTransactionKey = sparkDataApi.NcoaInitiateReport( sparkDataConfig.SparkDataApiKey, addresses.Count, sparkDataConfig.NcoaSettings.PersonFullName );
            }

            if ( groupNameTransactionKey == null )
            {
                if ( sparkDataConfig.NcoaSettings.CurrentReportStatus == "Failed" )
                {
                    // To avoid trying to charging customer over and over again...
                    throw new NoRetryException( "Init NCOA: Failed to initialize request." );
                }

                throw new Exception( "Init NCOA: Failed to initialize request." );
            }

            sparkDataConfig.NcoaSettings.FileName = groupNameTransactionKey.TransactionKey;

            var credentials = sparkDataApi.NcoaGetCredentials( sparkDataConfig.SparkDataApiKey );
            var ncoaApi = new NcoaApi( credentials );

            string id;
            ncoaApi.CreateFile( sparkDataConfig.NcoaSettings.FileName, groupNameTransactionKey.GroupName, out id );
            sparkDataConfig.NcoaSettings.CurrentReportKey = id;

            ncoaApi.UploadAddresses( addresses, sparkDataConfig.NcoaSettings.CurrentReportKey );

            sparkDataConfig.NcoaSettings.CurrentUploadCount = addresses.Count;
            ncoaApi.CreateReport( sparkDataConfig.NcoaSettings.CurrentReportKey );

            // Delete previous NcoaHistory entries. This prevent an user thinking the previous run's data is the current run's data.
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
        /// Resume a pending report: Checks if the report is complete. If the report is complete, then sent a create report command to NCOA.
        /// </summary>
        /// <param name="sparkDataConfig">The spark data configuration.</param>
        public void PendingReport( SparkDataConfig sparkDataConfig = null )
        {
            if ( sparkDataConfig == null )
            {
                sparkDataConfig = GetSettings();
            }

            SparkDataApi.SparkDataApi sparkDataApi = new SparkDataApi.SparkDataApi();
            var credentials = sparkDataApi.NcoaGetCredentials( sparkDataConfig.SparkDataApiKey );
            var ncoaApi = new NcoaApi( credentials );
            if ( !ncoaApi.IsReportCreated( sparkDataConfig.NcoaSettings.CurrentReportKey ) )
            {
                return;
            }

            string exportFileId;
            ncoaApi.CreateReportExport( sparkDataConfig.NcoaSettings.CurrentReportKey, out exportFileId );

            sparkDataConfig.NcoaSettings.CurrentReportExportKey = exportFileId;
            sparkDataConfig.NcoaSettings.CurrentReportStatus = "Pending: Export";
            SaveSettings( sparkDataConfig );
        }

        /// <summary>
        /// Resume a pending export: Checks if the export is complete. If the export is complete, then download the export, process the addresses and sent a notification.
        /// </summary>
        /// <param name="sparkDataConfig">The spark data configuration.</param>
        public void PendingExport( SparkDataConfig sparkDataConfig = null )
        {
            if ( sparkDataConfig == null )
            {
                sparkDataConfig = GetSettings();
            }

            SparkDataApi.SparkDataApi sparkDataApi = new SparkDataApi.SparkDataApi();
            var credentials = sparkDataApi.NcoaGetCredentials( sparkDataConfig.SparkDataApiKey );

            var ncoaApi = new NcoaApi( credentials );
            if ( !ncoaApi.IsReportExportCreated( sparkDataConfig.NcoaSettings.FileName ) )
            {
                return;
            }

            List<NcoaReturnRecord> ncoaReturnRecords;
            ncoaApi.DownloadExport( sparkDataConfig.NcoaSettings.CurrentReportExportKey, out ncoaReturnRecords );

            try
            {
                var ncoaHistoryList = ncoaReturnRecords.Select( r => r.ToNcoaHistory() ).ToList();
                FilterDuplicateLocations( ncoaHistoryList );

                // Making sure that the database is empty to avoid adding duplicate data.
                using ( var rockContext = new RockContext() )
                {
                    NcoaHistoryService ncoaHistoryService = new NcoaHistoryService( rockContext );
                    ncoaHistoryService.DeleteRange( ncoaHistoryService.Queryable() );
                    rockContext.SaveChanges();
                }

                if ( ncoaReturnRecords != null && ncoaReturnRecords.Count != 0 )
                {

                    using ( var rockContext = new RockContext() )
                    {
                        var ncoaHistoryService = new NcoaHistoryService( rockContext );
                        ncoaHistoryService.AddRange( ncoaHistoryList );
                        rockContext.SaveChanges();
                    }
                }

                ProcessNcoaResults( ncoaReturnRecords, sparkDataConfig );

                sparkDataApi.NcoaCompleteReport( sparkDataConfig.SparkDataApiKey, sparkDataConfig.NcoaSettings.FileName, sparkDataConfig.NcoaSettings.CurrentReportExportKey );

                //Notify group
                SendNotification( sparkDataConfig, "finished" );

                sparkDataConfig.NcoaSettings.LastRunDate = RockDateTime.Now;
                sparkDataConfig.NcoaSettings.CurrentReportStatus = "Complete";
                sparkDataConfig.NcoaSettings.FileName = null;
                SaveSettings( sparkDataConfig );
            }
            catch ( Exception ex )
            {
                throw new NoRetryAggregateException( "Failed to process NCOA export.", ex );
            }
        }

        /// <summary>
        /// Processes the NCOA results.
        /// </summary>
        /// <param name="ncoaReturnRecords">The NCOA return records.</param>
        /// <param name="sparkDataConfig">The spark data configuration.</param>
        /// <exception cref="NoRetryException">Inactive Record Reason value is empty.</exception>
        private void ProcessNcoaResults( List<NcoaReturnRecord> ncoaReturnRecords, SparkDataConfig sparkDataConfig = null )
        {
            if ( sparkDataConfig == null )
            {
                sparkDataConfig = GetSettings();
            }

            if ( sparkDataConfig.NcoaSettings.InactiveRecordReasonId == null )
            {
                throw new NoRetryException( "Inactive Record Reason value is empty." );
            }

            // Get the inactive reason
            var inactiveReason = DefinedValueCache.Get( sparkDataConfig.NcoaSettings.InactiveRecordReasonId.Value );

            // Get minimum move distance that is required to make the person as moved
            var minMoveDistance = SystemSettings.GetValue( SystemKey.SystemSetting.NCOA_MINIMUM_MOVE_DISTANCE_TO_INACTIVATE ).AsDecimalOrNull();

            // Process the 'None' and 'NoMove' NCOA Types (these will always have an address state as 'invalid')
            var markInvalidAsPrevious = SystemSettings.GetValue( SystemKey.SystemSetting.NCOA_SET_INVALID_AS_PREVIOUS ).AsBoolean();

            // Process the '48 Month Move' NCOA Types
            var mark48MonthAsPrevious = SystemSettings.GetValue( SystemKey.SystemSetting.NCOA_SET_48_MONTH_AS_PREVIOUS ).AsBoolean();

            ProcessNcoaResults( inactiveReason, markInvalidAsPrevious, mark48MonthAsPrevious, minMoveDistance, ncoaReturnRecords );
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
            foreach ( var duplicateLocationsGroup in duplicateLocations )
            {
                bool foundLocation = false;
                foreach ( var location in duplicateLocationsGroup )
                {
                    if ( !foundLocation )
                    {
                        if ( location.MoveType != MoveType.None && location.AddressStatus == AddressStatus.Valid )
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
                if ( !foundLocation )
                {
                    foreach ( var location in duplicateLocationsGroup )
                    {
                        if ( !foundLocation )
                        {
                            if ( location.AddressStatus == AddressStatus.Valid )
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
                if ( !foundLocation )
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
        /// <param name="ncoaReturnRecords">The NCOA return records.</param>
        private void ProcessNcoaResults( DefinedValueCache inactiveReason, bool markInvalidAsPrevious, bool mark48MonthAsPrevious, decimal? minMoveDistance, List<NcoaReturnRecord> ncoaReturnRecords )
        {
            // Get the ID's for the "Home" and "Previous" family group location types
            int? homeValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() )?.Id;
            int? previousValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid() )?.Id;
            var campusGeoPoints = GetCampusGeoPoints();

            ProcessNcoaResultsInvalidAddress( markInvalidAsPrevious, previousValueId );
            ProcessNcoaResults48MonthMove( mark48MonthAsPrevious, previousValueId );
            ProcessNcoaResultsFamilyMove( inactiveReason, minMoveDistance, homeValueId, previousValueId, ncoaReturnRecords, campusGeoPoints );
            ProcessNcoaResultsIndividualMove( inactiveReason, minMoveDistance, homeValueId, previousValueId, ncoaReturnRecords, campusGeoPoints );
        }

        /// <summary>
        /// Processes the NCOA results: Mark all invalid addresses as previous and processed if enabled, otherwise mark as manual update required.
        /// </summary>
        /// <param name="markInvalidAsPrevious">If invalid addresses should be marked as previous, set to <c>true</c>.</param>
        /// <param name="previousValueId">The previous value identifier.</param>
        private void ProcessNcoaResultsInvalidAddress( bool markInvalidAsPrevious, int? previousValueId )
        {
            List<int> ncoaIds = null;
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

                        var changes = new History.HistoryChangeList();

                        // If configured to mark these as previous, and we're able to mark it as previous set the status to 'Complete'
                        // otherwise set it to require a manual update
                        if ( markInvalidAsPrevious && MarkAsPreviousLocation( ncoaHistory, groupLocationService, previousValueId, changes ) != null )
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
                                            SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
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

                        try
                        {
                            rockContext.SaveChanges();
                        }
                        catch ( Exception ex )
                        {
                            ExceptionLogService.LogException( new AggregateException( string.Format( "NCOA Failed to set an address as invalid. NcoaHistoryId:'{0}' GroupId: '{1}'", ncoaHistory.Id, ncoaHistory.FamilyId ), ex ) );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Processes the NCOA results: Mark all 48 month move addresses as previous and processed if enabled, otherwise mark as manual update required.
        /// </summary>
        /// <param name="mark48MonthAsPrevious">if a 48 month move should be marked as previous, set to <c>true</c>.</param>
        /// <param name="previousValueId">The previous value identifier.</param>
        private void ProcessNcoaResults48MonthMove( bool mark48MonthAsPrevious, int? previousValueId )
        {
            List<int> ncoaIds = null;
            // Process the '48 Month Move' NCOA Types
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

                        var changes = new History.HistoryChangeList();

                        // If configured to mark these as previous, and we're able to mark it as previous set the status to 'Complete'
                        // otherwise set it to require a manual update
                        if ( mark48MonthAsPrevious && MarkAsPreviousLocation( ncoaHistory, groupLocationService, previousValueId, changes ) != null )
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
                                            SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
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

                        try
                        {
                            rockContext.SaveChanges();
                        }
                        catch ( Exception ex )
                        {
                            ExceptionLogService.LogException( new AggregateException( string.Format( "NCOA Failed to apply 48 months family move. NcoaHistoryId:'{0}' GroupId: '{1}'", ncoaHistory.Id, ncoaHistory.FamilyId ), ex ) );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the Geo points of all the campuses.
        /// </summary>
        /// <returns></returns>
        private List<DbGeography> GetCampusGeoPoints()
        {
            List<DbGeography> campusGeoPoints = new List<DbGeography>();
            var campusLocations = CampusCache.All( false ).Select( c => c.Location );
            if ( campusLocations != null )
            {
                foreach ( var campusLocation in campusLocations )
                {
                    try
                    {
                        if ( campusLocation.Longitude != null && campusLocation.Latitude != null )
                        {
                            var geo = DbGeography.PointFromText( string.Format( "POINT({0} {1})", campusLocation.Longitude, campusLocation.Latitude ), 4326 );
                            if ( geo != null )
                            {
                                campusGeoPoints.Add( geo );
                            }
                        }
                    }
                    catch { }
                }
            }

            return campusGeoPoints;
        }

        /// <summary>
        /// Checks if the person/family moved away from all campuses
        /// If minMoveDistance is specified check if the move is more than the min move distance
        /// If the move distance is more than the min move distance, check the distance between the campuses and updated address.
        /// </summary>
        /// <param name="ncoaHistory">The NCOA history.</param>
        /// <param name="minMoveDistance">The minimum move distance.</param>
        /// <param name="campusGeoPoints">The Geo points of all the campuses.</param>
        /// <param name="ncoaReturnRecords">The NCOA return records.</param>
        /// <returns></returns>
        private bool IsCloseToCampus( NcoaHistory ncoaHistory, decimal? minMoveDistance, List<DbGeography> campusGeoPoints, List<NcoaReturnRecord> ncoaReturnRecords )
        {
            if ( ncoaHistory.MoveDistance.HasValue && minMoveDistance.HasValue &&
                ncoaHistory.MoveDistance.Value >= minMoveDistance.Value )
            {
                if ( campusGeoPoints.Count() == 0 )
                {
                    // If unsure rather return the fail safe state
                    return true;
                }

                // Barcode is unique per address. We only need the co-ordinates, so we don't care if it is the same entry.
                var ncoaReturnRecord = ncoaReturnRecords.FirstOrDefault( n => n.Barcode == ncoaHistory.UpdatedBarcode );
                if ( ncoaReturnRecord == null )
                {
                    // If unsure rather return the fail safe state
                    return true;
                }

                bool isCloseToCampus = false;
                if ( ncoaReturnRecord.Longitude != null && ncoaReturnRecord.Latitude != null )
                {
                    try
                    {
                        var geoPoint = DbGeography.PointFromText( string.Format( "POINT({0} {1})", ncoaReturnRecord.Longitude, ncoaReturnRecord.Latitude ), 4326 );
                        if ( geoPoint != null )
                        {
                            foreach ( var campusGeoPoint in campusGeoPoints )
                            {

                                var distance = campusGeoPoint.Distance( geoPoint ) ?? 0.0D * 0.00062137505D;
                                if ( distance < ( double ) minMoveDistance.Value )
                                {
                                    isCloseToCampus = true;
                                    break;
                                }
                            }

                            return isCloseToCampus;
                        }
                    }
                    catch { }
                }
            }

            return true;
        }

        /// <summary>
        /// Processes the NCOA results: Mark all family move addresses as previous, add the new address as current; and processed.
        /// If minMoveDistance is specified, mark the family as inactive if the family moved further than the specified distance.
        /// If the move is more than the min move distance, check the distance between the campuses and updated address.
        /// If the person is further away than the min move distance from all campuses, then only mark the person as inactive. 
        /// </summary>
        /// <param name="inactiveReason">The inactive reason.</param>
        /// <param name="minMoveDistance">The minimum move distance.</param>
        /// <param name="homeValueId">The home value identifier.</param>
        /// <param name="previousValueId">The previous value identifier.</param>
        /// <param name="ncoaReturnRecords">The NCOA return records.</param>
        /// <param name="campusGeoPoints">The campus Geo points.</param>
        private void ProcessNcoaResultsFamilyMove( DefinedValueCache inactiveReason, decimal? minMoveDistance, int? homeValueId, int? previousValueId, List<NcoaReturnRecord> ncoaReturnRecords, List<DbGeography> campusGeoPoints )
        {
            List<int> ncoaIds = null;
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

                        var familyChanges = new History.HistoryChangeList();

                        ncoaHistory.Processed = Processed.ManualUpdateRequired;

                        // If we're able to mark the existing address as previous and successfully create a new home address..
                        var previousGroupLocation = MarkAsPreviousLocation( ncoaHistory, groupLocationService, previousValueId, familyChanges );
                        if ( previousGroupLocation != null )
                        {
                            if ( AddNewHomeLocation( ncoaHistory, locationService, groupLocationService, homeValueId, familyChanges, previousGroupLocation.IsMailingLocation, previousGroupLocation.IsMappedLocation ) )
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
                                        var movedAway = !IsCloseToCampus( ncoaHistory, minMoveDistance, campusGeoPoints, ncoaReturnRecords );
                                        foreach ( var fm in family.Members )
                                        {
                                            if ( movedAway )
                                            {
                                                History.HistoryChangeList personChanges;

                                                personService.InactivatePerson( fm.Person, inactiveReason,
                                                    $"Received a Change of Address (NCOA) notice that was for more than {minMoveDistance} miles away.", out personChanges );

                                                if ( personChanges.Any() )
                                                {
                                                    HistoryService.SaveChanges(
                                                        rockContext,
                                                        typeof( Person ),
                                                        SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                                                        fm.PersonId,
                                                        personChanges,
                                                        false );
                                                }
                                            }

                                            HistoryService.SaveChanges(
                                            rockContext,
                                            typeof( Person ),
                                            SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
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
                        }

                        try
                        {
                            rockContext.SaveChanges();
                        }
                        catch ( Exception ex )
                        {
                            ExceptionLogService.LogException( new AggregateException( string.Format( "NCOA Failed to apply family move. NcoaHistoryId:'{0}' GroupId: '{1}'", ncoaHistory.Id, ncoaHistory.FamilyId ), ex ) );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Processes the NCOA results: Mark all individual move addresses as previous, add the new address as current; and processed.
        /// If minMoveDistance is specified, mark the individual as inactive if the individual moved further than the specified distance.
        /// If the move is more than the min move distance, check the distance between the campuses and updated address.
        /// If the person is further away than the min move distance from all campuses, then only mark the person as inactive. 
        /// </summary>
        /// <param name="inactiveReason">The inactive reason.</param>
        /// <param name="minMoveDistance">The minimum move distance.</param>
        /// <param name="homeValueId">The home value identifier.</param>
        /// <param name="previousValueId">The previous value identifier.</param>
        /// <param name="ncoaReturnRecords">The NCOA return records.</param>
        /// <param name="campusGeoPoints">The campus Geo points.</param>
        private void ProcessNcoaResultsIndividualMove( DefinedValueCache inactiveReason, decimal? minMoveDistance, int? homeValueId, int? previousValueId, List<NcoaReturnRecord> ncoaReturnRecords, List<DbGeography> campusGeoPoints )
        {
            List<int> ncoaIds = null;
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
                int personId = 0;
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

                        var changes = new History.HistoryChangeList();

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
                                personId = personAlias.PersonId;
                                var previousGroupLocation = MarkAsPreviousLocation( ncoaHistory, groupLocationService, previousValueId, changes );
                                if ( previousGroupLocation != null )
                                {
                                    if ( AddNewHomeLocation( ncoaHistory, locationService, groupLocationService, homeValueId, changes, previousGroupLocation.IsMailingLocation, previousGroupLocation.IsMappedLocation ) )
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
                                            var movedAway = !IsCloseToCampus( ncoaHistory, minMoveDistance, campusGeoPoints, ncoaReturnRecords );

                                            if ( movedAway )
                                            {
                                                History.HistoryChangeList personChanges;

                                                personService.InactivatePerson( familyMember.Person, inactiveReason,
                                                    $"Received a Change of Address (NCOA) notice that was for more than {minMoveDistance} miles away.", out personChanges );

                                                if ( personChanges.Any() )
                                                {
                                                    HistoryService.SaveChanges(
                                                        rockContext,
                                                        typeof( Person ),
                                                        SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                                                        familyMember.PersonId,
                                                        personChanges,
                                                        false );
                                                }
                                            }

                                            HistoryService.SaveChanges(
                                                rockContext,
                                                typeof( Person ),
                                                SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
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
                        }

                        try
                        {
                            rockContext.SaveChanges();
                        }
                        catch ( Exception ex )
                        {
                            var personAlias = personAliasService.Get( ncoaHistory.PersonAliasId );
                            ExceptionLogService.LogException( new AggregateException( string.Format( "NCOA Failed to apply individual move. NcoaHistoryId:'{0}' PersonId: '{1}'", ncoaHistory.Id, personId ), ex ) );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Marks an address as previous location.
        /// </summary>
        /// <param name="ncoaHistory">The NCOA history.</param>
        /// <param name="groupLocationService">The group location service.</param>
        /// <param name="previousValueId">The previous value identifier.</param>
        /// <param name="changes">The changes.</param>
        /// <returns></returns>
        public GroupLocation MarkAsPreviousLocation( NcoaHistory ncoaHistory, GroupLocationService groupLocationService, int? previousValueId, History.HistoryChangeList changes )
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
                        changes.AddChange( History.HistoryVerb.Modify, History.HistoryChangeType.Property, $"Location Type for {groupLocation.Location} " ).SetNewValue( "Previous" ).SourceOfChange = "NCOA Request";

                        groupLocation.GroupLocationTypeValueId = previousValueId.Value;
                    }

                    return groupLocation;
                }
            }

            return null;
        }

        /// <summary>
        /// Adds the new home location.
        /// </summary>
        /// <param name="ncoaHistory">The NCOA history.</param>
        /// <param name="locationService">The location service.</param>
        /// <param name="groupLocationService">The group location service.</param>
        /// <param name="homeValueId">The home value identifier.</param>
        /// <param name="changes">The changes.</param>
        /// <param name="isMailingLocation">Is the location a mailing location.</param>
        /// <param name="isMappedLocation">Is the location a mapped location.</param>
        /// <returns></returns>
        private bool AddNewHomeLocation( NcoaHistory ncoaHistory, LocationService locationService, GroupLocationService groupLocationService, int? homeValueId, History.HistoryChangeList changes, bool isMailingLocation, bool isMappedLocation )
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
                groupLocation.IsMailingLocation = isMailingLocation;
                groupLocation.IsMappedLocation = isMappedLocation;
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
        /// Sends a notification that NCOA finished or failed
        /// </summary>
        /// <param name="sparkDataConfig">The spark data configuration.</param>
        /// <param name="status">The status to put in the notification.</param>
        public void SendNotification( SparkDataConfig sparkDataConfig, string status )
        {
            if ( !sparkDataConfig.GlobalNotificationApplicationGroupId.HasValue || sparkDataConfig.GlobalNotificationApplicationGroupId.Value == 0 )
            {
                return;
            }

            var recipients = new List<RecipientData>();
            using ( RockContext rockContext = new RockContext() )
            {
                Group group = new GroupService( rockContext ).GetNoTracking( sparkDataConfig.GlobalNotificationApplicationGroupId.Value );

                foreach ( var groupMember in group.Members )
                {
                    if ( groupMember.GroupMemberStatus == GroupMemberStatus.Active )
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
            if ( sparkDataConfig == null )
            {
                sparkDataConfig = Rock.Web.SystemSettings.GetValue( SystemSetting.SPARK_DATA ).FromJsonOrNull<SparkDataConfig>() ?? new SparkDataConfig();
            }

            if ( sparkDataConfig.NcoaSettings == null )
            {
                sparkDataConfig.NcoaSettings = new NcoaSettings();
            }

            if ( sparkDataConfig.Messages == null )
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
