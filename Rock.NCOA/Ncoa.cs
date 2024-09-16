using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.IO;
using System.Linq;
using CsvHelper;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.ViewModels.Blocks.Communication.NcoaProcess;
using Rock.SystemKey;
using Rock.Utility.Settings.SparkData;
using Rock.Jobs;

namespace Rock.NCOA
{
    public class Ncoa
    {

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

        #region Methods

        /// <summary>
        /// Get the people's Ids inside a DataView filter.
        /// </summary>
        /// <param name="dataViewValue">The data view identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>Returns a directory of people IDs that result from applying the DataView filter</returns>
        public Dictionary<int, int> DataViewPeopleDirectory( string dataViewValue, RockContext rockContext )
        {
            var dataViewService = new DataViewService( rockContext );
            var dataView = dataViewService.GetNoTracking( dataViewValue );

            // Verify that there is not a child filter that uses this view (would result in stack-overflow error)
            if ( dataViewService.IsViewInFilter( dataView.Id, dataView.DataViewFilter ) )
            {
                throw new Exception( "Data View Filter issue(s): One of the filters contains a circular reference to the Data View itself." );
            }

            var personService = new PersonService( rockContext );

            var personQuery = personService.Queryable().AsNoTracking();

            var paramExpression = personService.ParameterExpression;

            var whereExpression = dataView.GetExpression( personService, paramExpression );

            return personQuery.Where( paramExpression, whereExpression, null ).Select( p => p.Id ).ToDictionary( p => p, p => p );
        }

        /// <summary>
        /// Gets the addresses.
        /// </summary>
        /// <param name="dataViewValue">The data view identifier.</param>
        /// <returns>Directory of addresses</returns>
        public Dictionary<int, NcoaProcessPersonAddressBag> GetAddresses( string dataViewValue )
        {
            if ( dataViewValue == null )
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
                        .Queryable()
                        .AsNoTracking()
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

                    if ( dataViewValue != null )
                    {
                        var dataViewQuery = DataViewPeopleDirectory( dataViewValue, rockContext );
                        peopleHomelocation = peopleHomelocation.Where( p => dataViewQuery.ContainsKey( p.PersonId ) );
                    }

                    var definedType = DefinedTypeCache.Get( new Guid( SystemGuid.DefinedType.LOCATION_ADDRESS_STATE ) );
                    var stateList = definedType
                        .DefinedValues
                        .Where( v => v.ContainsKey( "Country" ) && v["Country"] != null )
                        .Select( v => new { State = v.Value, Country = v["Country"], Description = v.Description } )
                        .ToLookup( v => v.Description, StringComparer.OrdinalIgnoreCase );

                    var badSequences = new List<string>() { "<", "&#" };

                    return peopleHomelocation
                        .Select( g => new
                        {
                            g.PersonId,
                            HomeLocation = new NcoaProcessPersonAddressBag()
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
                        .Where( g => g.HomeLocation.State.IsNotNullOrWhiteSpace() )
                        .Where( g => g.HomeLocation.State.Length == 2 )
                        .Where( g => !badSequences.Any( x => g.HomeLocation.Street1?.Contains( x ) ?? false ) )
                        .Where( g => !badSequences.Any( x => g.HomeLocation.Street2?.Contains( x ) ?? false ) )
                        .Where( g => !badSequences.Any( x => g.HomeLocation.City?.Contains( x ) ?? false ) )
                        .ToDictionary( k => k.PersonId, v => v.HomeLocation );
                }

                throw new Exception( "Get Address: Could not find expected constant, type or value" );
            }
        }

        /// <summary>
        /// Resume a pending export: Checks if the export is complete. If the export is complete, then download the export, process the addresses and sent a notification.
        /// </summary>
        /// <param name="sparkDataConfig">The spark data configuration.</param>
        public (string successMessage, string errorMessage) PendingExport( DefinedValueCache inactiveReason, bool markInvalidAsPrevious, bool mark48MonthAsPrevious, decimal? minMoveDistance, List<NcoaReturnRecord> ncoaReturnRecords, SparkDataConfig sparkDataConfig = null )
        {
            if ( sparkDataConfig == null )
            {
                sparkDataConfig = GetSettings();
            }

            try
            {
                var ncoaHistoryList = ncoaReturnRecords.Select( r => r.ToNcoaHistory() ).ToList();
                FilterDuplicateLocations( ncoaHistoryList );

                ncoaHistoryList.ForEach( r => {
                    r.OriginalStreet1 = NormalizeInput( r.OriginalStreet1 );
                    r.OriginalStreet2 = NormalizeInput( r.OriginalStreet2 );
                    r.OriginalCity = NormalizeInput( r.OriginalCity );
                    r.OriginalState = NormalizeInput( r.OriginalState );
                    r.OriginalPostalCode = NormalizeInput( r.OriginalPostalCode );
                } );

                // Making sure that the database is empty to avoid adding duplicate data.
                using ( var rockContext = new RockContext() )
                {
                    var ncoaHistoryService = new NcoaHistoryService( rockContext );
                    ncoaHistoryService.DeleteRange( ncoaHistoryService.Queryable() );
                    rockContext.SaveChanges();

                    if ( ncoaReturnRecords != null && ncoaReturnRecords.Count != 0 )
                    {
                        ncoaHistoryService.AddRange( ncoaHistoryList );
                        rockContext.SaveChanges();
                    }
                }

                var (successMessage, errorMessage) = ProcessNcoaResults( inactiveReason, markInvalidAsPrevious, mark48MonthAsPrevious, minMoveDistance, ncoaReturnRecords );

                sparkDataConfig.NcoaSettings.LastRunDate = RockDateTime.Now;
                sparkDataConfig.NcoaSettings.CurrentReportStatus = "Complete";
                sparkDataConfig.NcoaSettings.FileName = null;
                SaveSettings( sparkDataConfig );

                if ( !string.IsNullOrEmpty( errorMessage ) )
                {
                    return ("", errorMessage);
                }
                return (successMessage, "");
            }
            catch ( Exception ex )
            {
                throw new NoRetryAggregateException( "Failed to process NCOA export.", ex );
            }
        }

        /// <summary>
        /// Checks for the case where the Ncoa import file contains data formatted as "null".
        /// </summary>
        /// <param name="input">The imported column from Ncoa being checked</param>
        /// <returns></returns>
        private string NormalizeInput( string input )
        {
            return input == "null" ? string.Empty : input;
        }

        /// <summary>
        /// Marks the duplicate locations as ManualUpdateRequired so that they are not processed.
        /// </summary>
        /// <param name="ncoaHistoryList">The NCOA history list.</param>
        private void FilterDuplicateLocations( List<NcoaHistory> ncoaHistoryList )
        {
            // Get all duplicate addresses/locations and sort the result by move date descending.
            var duplicateLocations = ncoaHistoryList
                .OrderByDescending( x => x.MoveDate )
                .GroupBy( x => x.PersonAliasId + "_" + x.FamilyId + "_" + x.LocationId )
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
        public (string successMessage, string errorMessage) ProcessNcoaResults( DefinedValueCache inactiveReason, bool markInvalidAsPrevious, bool mark48MonthAsPrevious, decimal? minMoveDistance, List<NcoaReturnRecord> ncoaReturnRecords )
        {
            // Get the ID's for the "Home" and "Previous" family group location types
            int? homeValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() )?.Id;
            int? previousValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid() )?.Id;
            var campusGeoPoints = GetCampusGeoPoints();

            var invalidAddressErrorMessage = ProcessNcoaResultsInvalidAddress( markInvalidAsPrevious, previousValueId );
            var marked48MonthMoveErrorMessage = ProcessNcoaResults48MonthMove( mark48MonthAsPrevious, previousValueId );
            var familyMoveErrorMessage = ProcessNcoaResultsFamilyMove( inactiveReason, minMoveDistance, homeValueId, previousValueId, ncoaReturnRecords, campusGeoPoints );
            var individualMoveErrorMessage = ProcessNcoaResultsIndividualMove( inactiveReason, minMoveDistance, homeValueId, previousValueId, ncoaReturnRecords, campusGeoPoints );

            if ( !string.IsNullOrEmpty( invalidAddressErrorMessage ) )
            {
                return ("", invalidAddressErrorMessage);
            }

            if ( !string.IsNullOrEmpty( marked48MonthMoveErrorMessage ) )
            {
                return ("", marked48MonthMoveErrorMessage);
            }

            if ( !string.IsNullOrEmpty( familyMoveErrorMessage ) )
            {
                return ("", familyMoveErrorMessage);
            }

            if ( !string.IsNullOrEmpty( individualMoveErrorMessage ) )
            {
                return ("", individualMoveErrorMessage);
            }
            using ( var rockContext = new RockContext() )
            {
                NcoaHistoryService ncoaHistoryService = new NcoaHistoryService( rockContext );
                var successMessage = $"NCOA request processed, {ncoaHistoryService.Count()} {( ncoaHistoryService.Count() == 1 ? "address" : "addresses" )} processed, {ncoaHistoryService.MovedCount()} {( ncoaHistoryService.MovedCount() > 1 ? "were" : "was" )} marked as 'moved'";
                return (successMessage, "");
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
                            var geo = DbGeography.PointFromText( string.Format( "POINT({0} {1})", campusLocation.Longitude, campusLocation.Latitude ), DbGeography.DefaultCoordinateSystemId );
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
        /// Processes the NCOA results: Mark all invalid addresses as previous and processed if enabled, otherwise mark as manual update required.
        /// </summary>
        /// <param name="markInvalidAsPrevious">If invalid addresses should be marked as previous, set to <c>true</c>.</param>
        /// <param name="previousValueId">The previous value identifier.</param>
        private string ProcessNcoaResultsInvalidAddress( bool markInvalidAsPrevious, int? previousValueId )
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
                                            typeof( Rock.Model.Person ),
                                            SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
                                            fm.PersonId,
                                            changes,
                                            family.Name,
                                            typeof( Rock.Model.Group ),
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
                            string errorMessage = string.Format( "NCOA Failed to set an address as invalid. NcoaHistoryId:'{0}' GroupId: '{1}'", ncoaHistory.Id, ncoaHistory.FamilyId );
                            ExceptionLogService.LogException( new AggregateException( errorMessage, ex ) );
                            return errorMessage;
                        }
                    }
                }
            }
            return "";
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
                var groupLocation = groupLocationService
                    .Queryable()
                    .FirstOrDefault( gl =>
                        gl.GroupId == ncoaHistory.FamilyId &&
                        gl.LocationId == ncoaHistory.LocationId &&
                        gl.Location.Street1 == ncoaHistory.OriginalStreet1 );
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
        /// Processes the NCOA results: Mark all 48 month move addresses as previous and processed if enabled, otherwise mark as manual update required.
        /// </summary>
        /// <param name="mark48MonthAsPrevious">if a 48 month move should be marked as previous, set to <c>true</c>.</param>
        /// <param name="previousValueId">The previous value identifier.</param>
        private string ProcessNcoaResults48MonthMove( bool mark48MonthAsPrevious, int? previousValueId )
        {
            List<int> ncoaIds = null;
            // Process the '48 Month Move' NCOA Types
            using ( var rockContext = new RockContext() )
            {
                ncoaIds = new NcoaHistoryService( rockContext )
                    .Queryable()
                    .AsNoTracking()
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
                                            typeof( Rock.Model.Person ),
                                            SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
                                            fm.PersonId,
                                            changes,
                                            family.Name,
                                            typeof( Rock.Model.Group ),
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
                            var marked48MonthMoveErrorMessage = string.Format( "NCOA Failed to apply 48 months family move. NcoaHistoryId:'{0}' GroupId: '{1}'", ncoaHistory.Id, ncoaHistory.FamilyId );
                            ExceptionLogService.LogException( new AggregateException( marked48MonthMoveErrorMessage, ex ) );
                            return marked48MonthMoveErrorMessage;
                        }
                    }
                }
            }
            return "";
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
        private string ProcessNcoaResultsFamilyMove( DefinedValueCache inactiveReason, decimal? minMoveDistance, int? homeValueId, int? previousValueId, List<NcoaReturnRecord> ncoaReturnRecords, List<DbGeography> campusGeoPoints )
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
                                    .Queryable()
                                    .Where( n =>
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
                                                personService.InactivatePerson(
                                                    fm.Person,
                                                    inactiveReason,
                                                    $"Received a Change of Address (NCOA) notice that was for more than {minMoveDistance} miles away." );
                                            }

                                            HistoryService.SaveChanges(
                                                rockContext,
                                                typeof( Rock.Model.Person ),
                                                SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
                                                fm.PersonId,
                                                familyChanges,
                                                family.Name,
                                                typeof( Rock.Model.Group ),
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
                            var familyMoveErrorMessage = string.Format( "NCOA Failed to apply family move. NcoaHistoryId:'{0}' GroupId: '{1}'", ncoaHistory.Id, ncoaHistory.FamilyId );
                            ExceptionLogService.LogException( new AggregateException( familyMoveErrorMessage, ex ) );
                            return familyMoveErrorMessage;
                        }
                    }
                }
            }
            return "";
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
                        var geoPoint = DbGeography.PointFromText( string.Format( "POINT({0} {1})", ncoaReturnRecord.Longitude, ncoaReturnRecord.Latitude ), DbGeography.DefaultCoordinateSystemId );
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

        public string ProcessCsvData( string csvData )
        {
            List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();

            using ( var reader = new StringReader( csvData ) )
            using ( var csv = new CsvReader( reader ) )
            {

                var records = csv.GetRecords<dynamic>();

                foreach ( var record in records )
                {
                    var recordDict = new Dictionary<string, object>();

                    foreach ( var field in record )
                    {
                        recordDict.Add( field.Key, field.Value.ToString() );
                    }
                    results.Add( recordDict );
                }
                return results.ToJson();
            }
        }

        /// <summary>
        /// Builds the list of NCOA return records from the imported file content
        /// </summary>
        /// <param name="ncoaContent">The content coming back from NCOA file.</param>
        /// <param name="records">The records.</param>
        public void NcoaRecordBuilder( string ncoaContent, out List<NcoaReturnRecord> records )
        {
            records = null;
            var dt = RockDateTime.Now;
            int start = 1;
            int end = 1000;
            int step = 1000;
            bool finished = false;
            records = new List<NcoaReturnRecord>();
            while ( !finished )
            {
                try
                {
                    string jsonContent = ProcessCsvData( ncoaContent );

                    List<NcoaReturnRecord> instanceRecords = jsonContent.FromJsonOrThrow<List<NcoaReturnRecord>>();

                    finished = true;
                    if ( instanceRecords == null )
                    {
                        instanceRecords = new List<NcoaReturnRecord>();
                    }

                    instanceRecords.ForEach( r => r.NcoaRunDateTime = dt );
                    records.AddRange( instanceRecords );
                    if ( instanceRecords.Count < step )
                    {
                        finished = true;
                    }
                    else
                    {
                        start += step;
                        end += step;
                    }
                }
                catch ( Exception ex )
                {
                    throw new AggregateException( $"Failed to deserialize NCOA response: {ncoaContent}", ex );
                }
            }

            // NCOA return two entries for each move. One for the new address, and one for the previous address. Remove the old address because it is not required.
            var movedRecords = records.Where( r => r.RecordType == "C" && r.MatchFlag == "M" ).ToList(); // Find all the moved addresses with a current address
            records.RemoveAll( r => r.RecordType == "H" && movedRecords.Any( m => m.InputIndividualId == r.InputIndividualId ) ); // Delete all the moved addresses's previous addresses
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
        private string ProcessNcoaResultsIndividualMove( DefinedValueCache inactiveReason, decimal? minMoveDistance, int? homeValueId, int? previousValueId, List<NcoaReturnRecord> ncoaReturnRecords, List<DbGeography> campusGeoPoints )
        {
            List<int> ncoaIds = null;
            // Process 'Move' NCOA Types (For the remaining Individual move types that weren't updated with the family move)
            using ( var rockContext = new RockContext() )
            {
                ncoaIds = new NcoaHistoryService( rockContext )
                    .Queryable()
                    .AsNoTracking()
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
                                            .Queryable()
                                            .Where( n =>
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
                                                personService.InactivatePerson(
                                                    familyMember.Person,
                                                    inactiveReason,
                                                    $"Received a Change of Address (NCOA) notice that was for more than {minMoveDistance} miles away." );
                                            }

                                            HistoryService.SaveChanges(
                                                rockContext,
                                                typeof( Rock.Model.Person ),
                                                SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
                                                familyMember.PersonId,
                                                changes,
                                                family.Name,
                                                typeof( Rock.Model.Group ),
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
                            var individualMoveErrorMessage = string.Format( "NCOA Failed to apply individual move. NcoaHistoryId:'{0}' PersonId: '{1}'", ncoaHistory.Id, personId );
                            ExceptionLogService.LogException( new AggregateException( individualMoveErrorMessage, ex ) );
                            return individualMoveErrorMessage;
                        }
                    }
                }
            }
            return "";
        }

        #endregion

    }
}
