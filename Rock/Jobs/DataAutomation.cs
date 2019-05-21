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

using Rock.Data;
using Rock.Model;
using Rock.SystemKey;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to update people/families based on the Data Automation settings.
    /// Data Automation tasks are tasks that update the status of data.
    /// </summary>
    [DisallowConcurrentExecution]
    public class DataAutomation : IJob
    {
        private const string SOURCE_OF_CHANGE = "Data Automation";
        private HttpContext _httpContext = null;

        #region Constructor

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public DataAutomation()
        {
        }

        #endregion Constructor

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Execute( IJobExecutionContext context )
        {
            _httpContext = HttpContext.Current;

            string reactivateResult = ReactivatePeople( context );
            string inactivateResult = InactivatePeople( context );
            string updateFamilyCampusResult = UpdateFamilyCampus( context );
            string moveAdultChildrenResult = MoveAdultChildren( context );
            string genderAutofill = GenderAutoFill( context );
            string updatePersonConnectionStatus = UpdatePersonConnectionStatus( context );
            string updateFamilyStatus = UpdateFamilyStatus( context );

            context.UpdateLastStatusMessage( $@"Reactivate People: {reactivateResult}
Inactivate People: {inactivateResult}
Update Family Campus: {updateFamilyCampusResult}
Move Adult Children: {moveAdultChildrenResult}
Gender Autofill: {genderAutofill}
Update Connection Status: {updatePersonConnectionStatus}
Update Family Status: {updateFamilyStatus}
" );
        }

        /// <summary>
        /// Autofill Person.Gender based on the first name if the confidence level meets the min threshold specified in SystemSetting.GENDER_AUTO_FILL_CONFIDENCE.
        /// Children autofill is based on confidence level alone.
        /// Adults will not autofill a gender that is already taken by another adult in the same family.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private string GenderAutoFill( IJobExecutionContext context )
        {
            context.UpdateLastStatusMessage( $"Processing Gender Autofill" );

            decimal? autofillConfidence = Web.SystemSettings.GetValue( SystemSetting.GENDER_AUTO_FILL_CONFIDENCE ).AsDecimalOrNull();
            if ( autofillConfidence == null || autofillConfidence == 0 )
            {
                return "Not Enabled";
            }

            int recordsProcessed = 0;
            int recordsUpdated = 0;
            int recordsWithError = 0;

            var persons = new PersonService( new RockContext() )
                .Queryable()
                .AsNoTracking()
                .Where( p => !string.IsNullOrEmpty( p.FirstName ) && p.Gender == Gender.Unknown )
                .ToList();

            var firstNameGenderDictionary = new MetaFirstNameGenderLookupService( new RockContext() )
                            .Queryable()
                            .Where( n => n.FemalePercent >= autofillConfidence || n.MalePercent >= autofillConfidence )
                            .ToDictionary( k => k.FirstName, v => new { v.MalePercent, v.FemalePercent }, StringComparer.OrdinalIgnoreCase );

            foreach ( var person in persons )
            {
                try
                {
                    using ( RockContext rockContext = new RockContext() )
                    {
                        rockContext.SourceOfChange = SOURCE_OF_CHANGE;
                        // attach the person object to this rockContext so that it will do changetracking on it
                        rockContext.People.Attach( person );

                        // find the name
                        var metaFirstNameGenderLookup = firstNameGenderDictionary.GetValueOrNull( person.FirstName );

                        if ( metaFirstNameGenderLookup != null )
                        {
                            List<Gender> otherAdultsGender = new List<Gender>();

                            // If the person is an adult we want to get the other adults in the family
                            // Adults will not update their gender if there is another adult in the family with the same gender
                            if ( person.AgeClassification == AgeClassification.Adult )
                            {
                                otherAdultsGender = person.GetFamilyMembers( false, rockContext )
                                    .AsNoTracking()
                                    .Where( m => m.Person.AgeClassification == AgeClassification.Adult )
                                    .Select( m => m.Person.Gender )
                                    .ToList();
                            }

                            // Adults = Change based on the confidence unless they are in a family as an adult where there is another adult with the same gender
                            if ( metaFirstNameGenderLookup.FemalePercent >= autofillConfidence && !otherAdultsGender.Any( a => a == Gender.Female ) )
                            {
                                person.Gender = Gender.Female;
                                rockContext.SaveChanges();
                                recordsUpdated += 1;
                            }
                            else if ( metaFirstNameGenderLookup.MalePercent >= autofillConfidence && !otherAdultsGender.Any( a => a == Gender.Male ) )
                            {
                                person.Gender = Gender.Male;
                                rockContext.SaveChanges();
                                recordsUpdated += 1;
                            }
                        }

                        recordsProcessed += 1;
                    }
                }
                catch ( Exception ex )
                {
                    // log but don't throw
                    ExceptionLogService.LogException( new Exception( $"Exception occurred trying to autofill gender for PersonId:{person.Id}.", ex ), _httpContext );
                    recordsWithError += 1;
                }
            }

            return $"{recordsProcessed:N0} people were processed; {recordsUpdated:N0} genders were updated; {recordsWithError:N0} records logged an exception";
        }

        #region Reactivate People

        /// <summary>
        /// Reactivates the people.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// Could not determine the 'Family' group type.
        /// or
        /// Could not determine the 'Active' record status value.
        /// or
        /// Could not determine the 'Inactive' record status value.
        /// </exception>
        private string ReactivatePeople( IJobExecutionContext context )
        {
            try
            {
                context.UpdateLastStatusMessage( $"Processing person reactivate." );

                var settings = Rock.Web.SystemSettings.GetValue( SystemSetting.DATA_AUTOMATION_REACTIVATE_PEOPLE ).FromJsonOrNull<Utility.Settings.DataAutomation.ReactivatePeople>();
                if ( settings == null || !settings.IsEnabled )
                {
                    return "Not Enabled";
                }

                // Get the family group type
                var familyGroupType = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
                if ( familyGroupType == null )
                {
                    throw new Exception( "Could not determine the 'Family' group type." );
                }

                // Get the active record status defined value
                var activeStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() );
                if ( activeStatus == null )
                {
                    throw new Exception( "Could not determine the 'Active' record status value." );
                }

                // Get the inactive record status defined value
                var inactiveStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() );
                if ( inactiveStatus == null )
                {
                    throw new Exception( "Could not determine the 'Inactive' record status value." );
                }

                var personIds = new List<int>();

                using ( var rockContext = new RockContext() )
                {
                    rockContext.SourceOfChange = SOURCE_OF_CHANGE;
                    // increase the timeout just in case.
                    rockContext.Database.CommandTimeout = 180;

                    // Get all the person ids with selected activity
                    personIds = GetPeopleWhoContributed( settings.IsLastContributionEnabled, settings.LastContributionPeriod, rockContext );
                    personIds.AddRange( GetPeopleWhoAttendedServiceGroup( settings.IsAttendanceInServiceGroupEnabled, settings.AttendanceInServiceGroupPeriod, rockContext ) );
                    personIds.AddRange( GetPeopleWhoAttendedGroupType( settings.IsAttendanceInGroupTypeEnabled, settings.AttendanceInGroupType, null, settings.AttendanceInGroupTypeDays, rockContext ) );
                    personIds.AddRange( GetPeopleWhoSubmittedPrayerRequest( settings.IsPrayerRequestEnabled, settings.PrayerRequestPeriod, rockContext ) );
                    personIds.AddRange( GetPeopleWithPersonAttributUpdates( settings.IsPersonAttributesEnabled, settings.PersonAttributes, null, settings.PersonAttributesDays, rockContext ) );
                    personIds.AddRange( GetPeopleWithInteractions( settings.IsInteractionsEnabled, settings.Interactions, rockContext ) );

                    var dataViewQry = GetPeopleInDataViewQuery( settings.IsIncludeDataViewEnabled, settings.IncludeDataView, rockContext );
                    if ( dataViewQry != null )
                    {
                        personIds.AddRange( dataViewQry.ToList() );
                    }

                    // Get the distinct person ids
                    personIds = personIds.Distinct().ToList();

                    // Create a queryable of the person ids
                    var personIdQry = CreateEntitySetIdQuery( personIds, rockContext );

                    // Expand the list to all family members.
                    personIds = new GroupMemberService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( m =>
                            m.Group != null &&
                            m.Group.GroupTypeId == familyGroupType.Id &&
                            personIdQry.Contains( m.PersonId ) )
                        .SelectMany( m => m.Group.Members )
                        .Select( p => p.PersonId )
                        .ToList();
                    personIds = personIds.Distinct().ToList();

                    // Create a new queryable of family member person ids
                    personIdQry = CreateEntitySetIdQuery( personIds, rockContext );

                    // Start the person qry by getting any of the people who are currently inactive
                    var personQry = new PersonService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( p =>
                            personIdQry.Contains( p.Id ) &&
                            p.RecordStatusValueId == inactiveStatus.Id );

                    // Check to see if any inactive reasons should be ignored, and if so filter the list to exclude those
                    var invalidReasonDt = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON.AsGuid() );
                    if ( invalidReasonDt != null )
                    {
                        var invalidReasonIds = invalidReasonDt.DefinedValues
                            .Where( a =>
                                a.AttributeValues.ContainsKey( "AllowAutomatedReactivation" ) &&
                                !a.AttributeValues["AllowAutomatedReactivation"].Value.AsBoolean() )
                            .Select( a => a.Id )
                            .ToList();
                        if ( invalidReasonIds.Any() )
                        {
                            personQry = personQry.Where( p =>
                                !p.RecordStatusReasonValueId.HasValue ||
                                !invalidReasonIds.Contains( p.RecordStatusReasonValueId.Value ) );
                        }
                    }

                    // If any people should be excluded based on being part of a dataview, exclude those people
                    var excludePersonIdQry = GetPeopleInDataViewQuery( settings.IsExcludeDataViewEnabled, settings.ExcludeDataView, rockContext );
                    if ( excludePersonIdQry != null )
                    {
                        personQry = personQry.Where( p =>
                            !excludePersonIdQry.Contains( p.Id ) );
                    }

                    // Run the query
                    personIds = personQry.Select( p => p.Id ).ToList();
                }

                // Counter for displaying results
                int recordsProcessed = 0;
                int recordsUpdated = 0;
                int totalRecords = personIds.Count();

                // Loop through each person
                foreach ( var personId in personIds )
                {
                    try
                    {
                        // Update the status on every 100th record
                        if ( recordsProcessed % 100 == 0 )
                        {
                            context.UpdateLastStatusMessage( $"Processing person reactivate: Activated {recordsUpdated:N0} of {totalRecords:N0} person records." );
                        }

                        recordsProcessed++;

                        // Reactivate the person
                        using ( var rockContext = new RockContext() )
                        {
                            rockContext.SourceOfChange = SOURCE_OF_CHANGE;
                            var person = new PersonService( rockContext ).Get( personId );
                            if ( person != null )
                            {
                                person.RecordStatusValueId = activeStatus.Id;
                                person.RecordStatusReasonValueId = null;
                                rockContext.SaveChanges();

                                recordsUpdated++;
                            }
                        }
                    }
                    catch ( Exception ex )
                    {
                        // Log exception and keep on trucking.
                        ExceptionLogService.LogException( new Exception( $"Exception occurred trying to activate PersonId:{personId}.", ex ), _httpContext );
                    }
                }

                // Format the result message
                return $"{recordsProcessed:N0} people were processed; {recordsUpdated:N0} were activated.";
            }
            catch ( Exception ex )
            {
                // Log exception and return the exception messages.
                ExceptionLogService.LogException( ex, _httpContext );

                return ex.Messages().AsDelimited( "; " );
            }
        }

        #endregion

        #region Inactivate People

        /// <summary>
        /// Inactivates the people.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// Could not determine the 'Family' group type.
        /// or
        /// Could not determine the 'Active' record status value.
        /// or
        /// Could not determine the 'Inactive' record status value.
        /// or
        /// Could not determine the 'No Activity' record status reason value.
        /// </exception>
        private string InactivatePeople( IJobExecutionContext context )
        {
            try
            {
                context.UpdateLastStatusMessage( $"Processing person inactivate." );

                var settings = Rock.Web.SystemSettings.GetValue( SystemSetting.DATA_AUTOMATION_INACTIVATE_PEOPLE ).FromJsonOrNull<Utility.Settings.DataAutomation.InactivatePeople>();
                if ( settings == null || !settings.IsEnabled )
                {
                    return "Not Enabled";
                }

                // Get the family group type
                var familyGroupType = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
                if ( familyGroupType == null )
                {
                    throw new Exception( "Could not determine the 'Family' group type." );
                }

                // Get the active record status defined value
                var activeStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() );
                if ( activeStatus == null )
                {
                    throw new Exception( "Could not determine the 'Active' record status value." );
                }

                // Get the inactive record status defined value
                var inactiveStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() );
                if ( inactiveStatus == null )
                {
                    throw new Exception( "Could not determine the 'Inactive' record status value." );
                }

                // Get the inactive record status defined value
                var inactiveReason = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_REASON_NO_ACTIVITY.AsGuid() );
                if ( inactiveReason == null )
                {
                    throw new Exception( "Could not determine the 'No Activity' record status reason value." );
                }

                var personIds = new List<int>();
                using ( var rockContext = new RockContext() )
                {
                    // increase the timeout just in case.
                    rockContext.Database.CommandTimeout = 180;
                    rockContext.SourceOfChange = SOURCE_OF_CHANGE;

                    // Get all the person ids with selected activity
                    personIds = GetPeopleWhoContributed( settings.IsNoLastContributionEnabled, settings.NoLastContributionPeriod, rockContext );
                    personIds.AddRange( GetPeopleWhoAttendedGroupType( settings.IsNoAttendanceInGroupTypeEnabled, null, settings.AttendanceInGroupType, settings.NoAttendanceInGroupTypeDays, rockContext ) );
                    personIds.AddRange( GetPeopleWhoSubmittedPrayerRequest( settings.IsNoPrayerRequestEnabled, settings.NoPrayerRequestPeriod, rockContext ) );
                    personIds.AddRange( GetPeopleWithPersonAttributUpdates( settings.IsNoPersonAttributesEnabled, null, settings.PersonAttributes, settings.NoPersonAttributesDays, rockContext ) );
                    personIds.AddRange( GetPeopleWithInteractions( settings.IsNoInteractionsEnabled, settings.NoInteractions, rockContext ) );

                    // Get the distinct person ids
                    personIds = personIds.Distinct().ToList();

                    // Create a queryable of the person ids
                    var personIdQry = CreateEntitySetIdQuery( personIds, rockContext );

                    // Expand the list to all family members.
                    personIds = new GroupMemberService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( m =>
                            m.Group != null &&
                            m.Group.GroupTypeId == familyGroupType.Id &&
                            personIdQry.Contains( m.PersonId ) )
                        .SelectMany( m => m.Group.Members )
                        .Select( p => p.PersonId )
                        .ToList();
                    personIds = personIds.Distinct().ToList();

                    // Create a new queryable of family member person ids
                    personIdQry = CreateEntitySetIdQuery( personIds, rockContext );

                    var maxRecordCreationDate = RockDateTime.Now.AddDays( settings.RecordsOlderThan * -1 );
                    // Start the person qry by getting any of the people who are currently active and not in the list of people with activity
                    var personQry = new PersonService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( p =>
                            !personIdQry.Contains( p.Id ) &&
                            p.RecordStatusValueId == activeStatus.Id &&
                            p.CreatedDateTime < maxRecordCreationDate );

                    // If any people should be excluded based on being part of a dataview, exclude those people
                    var excludePersonIdQry = GetPeopleInDataViewQuery( settings.IsNotInDataviewEnabled, settings.NotInDataview, rockContext );
                    if ( excludePersonIdQry != null )
                    {
                        personQry = personQry.Where( p =>
                            !excludePersonIdQry.Contains( p.Id ) );
                    }

                    // Run the query
                    personIds = personQry.Select( p => p.Id ).ToList();
                }

                // Counter for displaying results
                int recordsProcessed = 0;
                int recordsUpdated = 0;
                int totalRecords = personIds.Count();

                // Get a formatted date
                string dateStamp = RockDateTime.Now.ToShortDateString();

                // Loop through each person
                foreach ( var personId in personIds )
                {
                    // Update the status on every 100th record
                    if ( recordsProcessed % 100 == 0 )
                    {
                        context.UpdateLastStatusMessage( $"Processing person inactivate: Inactivated {recordsUpdated:N0} of {totalRecords:N0} person records." );
                    }

                    recordsProcessed++;

                    // Inactivate the person
                    using ( var rockContext = new RockContext() )
                    {
                        rockContext.SourceOfChange = SOURCE_OF_CHANGE;
                        try
                        {
                            var person = new PersonService( rockContext ).Get( personId );
                            if ( person != null )
                            {
                                person.RecordStatusValueId = inactiveStatus.Id;
                                person.RecordStatusReasonValueId = inactiveReason.Id;
                                person.InactiveReasonNote = $"Inactivated by the Data Automation Job on {dateStamp}";
                                rockContext.SaveChanges();

                                recordsUpdated++;
                            }
                        }
                        catch ( Exception ex )
                        {
                            // Log exception and keep on trucking.
                            ExceptionLogService.LogException( new Exception( $"Exception occurred trying to inactivate PersonId:{personId}.", ex ), _httpContext );
                        }
                    }
                }

                // Format the result message
                return $"{recordsProcessed:N0} people were processed; {recordsUpdated:N0} were inactivated.";
            }
            catch ( Exception ex )
            {
                // Log exception and return the exception messages.
                ExceptionLogService.LogException( ex, _httpContext );
                return ex.Messages().AsDelimited( "; " );
            }
        }

        #endregion

        #region Update Family Campus 

        /// <summary>
        /// Updates the family campus.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Could not determine the 'Family' group type.</exception>
        private string UpdateFamilyCampus( IJobExecutionContext context )
        {
            try
            {
                context.UpdateLastStatusMessage( $"Processing campus updates." );

                var settings = Rock.Web.SystemSettings.GetValue( SystemSetting.DATA_AUTOMATION_CAMPUS_UPDATE ).FromJsonOrNull<Utility.Settings.DataAutomation.UpdateFamilyCampus>();
                if ( settings == null || !settings.IsEnabled )
                {
                    return "Not Enabled";
                }

                // Get the family group type and roles
                var familyGroupType = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
                if ( familyGroupType == null )
                {
                    throw new Exception( "Could not determine the 'Family' group type." );
                }

                var familyIds = new List<int>();
                var personCampusAttendance = new List<PersonCampus>();
                var personCampusGiving = new List<PersonCampus>();

                using ( RockContext rockContext = new RockContext() )
                {
                    rockContext.SourceOfChange = SOURCE_OF_CHANGE;
                    // increase the timeout just in case.
                    rockContext.Database.CommandTimeout = 180;

                    // Start a qry for all family ids
                    var familyIdQry = new GroupService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( g => g.GroupTypeId == familyGroupType.Id );

                    // Check to see if we should ignore any families that had a manual update
                    if ( settings.IsIgnoreIfManualUpdateEnabled )
                    {
                        // Figure out how far back to look
                        var startPeriod = RockDateTime.Now.AddDays( -settings.IgnoreIfManualUpdatePeriod );

                        // Find any families that has a campus manually added/updated within the configured number of days
                        var personEntityTypeId = EntityTypeCache.Get( typeof( Person ) ).Id;
                        var familyIdsWithManualUpdate = new HistoryService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( m =>
                                m.CreatedDateTime >= startPeriod &&
                                m.EntityTypeId == personEntityTypeId &&
                                m.RelatedEntityId.HasValue &&
                                m.ValueName == "Campus" )
                            .Select( a => a.RelatedEntityId.Value )
                            .ToList()
                            .Distinct();

                        familyIdQry = familyIdQry.Where( f => !familyIdsWithManualUpdate.Contains( f.Id ) );
                    }

                    // Query for the family ids
                    familyIds = familyIdQry.Select( f => f.Id ).ToList();

                    // Query all of the attendance tied to a campus 
                    if ( settings.IsMostFamilyAttendanceEnabled )
                    {
                        var startPeriod = RockDateTime.Now.AddDays( -settings.MostFamilyAttendancePeriod );
                        personCampusAttendance = new AttendanceService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( a =>
                                a.StartDateTime >= startPeriod &&
                                a.DidAttend == true &&
                                a.CampusId.HasValue &&
                                a.PersonAlias != null )
                            .Select( a => new PersonCampus
                            {
                                PersonId = a.PersonAlias.PersonId,
                                CampusId = a.CampusId.Value
                            } )
                            .ToList();
                    }

                    // Query all of the giving tied to a campus 
                    if ( settings.IsMostFamilyGivingEnabled )
                    {
                        var startPeriod = RockDateTime.Now.AddDays( -settings.MostFamilyAttendancePeriod );
                        personCampusGiving = new FinancialTransactionDetailService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( a =>
                                a.Transaction != null &&
                                a.Transaction.TransactionDateTime.HasValue &&
                                a.Transaction.TransactionDateTime >= startPeriod &&
                                a.Transaction.AuthorizedPersonAlias != null &&
                                a.Account.CampusId.HasValue )
                            .Select( a => new PersonCampus
                            {
                                PersonId = a.Transaction.AuthorizedPersonAlias.PersonId,
                                CampusId = a.Account.CampusId.Value
                            } )
                            .ToList();
                    }
                }

                // Counters for displaying results
                int recordsProcessed = 0;
                int recordsUpdated = 0;
                int totalRecords = familyIds.Count();

                // Loop through each family
                foreach ( var familyId in familyIds )
                {
                    try
                    {
                        // Update the status on every 100th record
                        if ( recordsProcessed % 100 == 0 )
                        {
                            context.UpdateLastStatusMessage( $"Processing campus updates: {recordsProcessed:N0} of {totalRecords:N0} families processed; campus has been updated for {recordsUpdated:N0} of them." );
                        }

                        recordsProcessed++;

                        // Using a new rockcontext for each one (to improve performance)
                        using ( var rockContext = new RockContext() )
                        {
                            rockContext.SourceOfChange = SOURCE_OF_CHANGE;

                            // Get the family
                            var groupService = new GroupService( rockContext );
                            var family = groupService.Get( familyId );

                            var personIds = family.Members.Select( m => m.PersonId ).ToList();

                            // Calculate the campus based on family attendance
                            int? attendanceCampusId = null;
                            int attendanceCampusCount = 0;
                            if ( personCampusAttendance.Any() )
                            {
                                var startPeriod = RockDateTime.Now.AddDays( -settings.MostFamilyAttendancePeriod );
                                var attendanceCampus = personCampusAttendance
                                    .Where( a => personIds.Contains( a.PersonId ) )
                                    .GroupBy( a => a.CampusId )
                                    .OrderByDescending( a => a.Count() )
                                    .Select( a => new
                                    {
                                        CampusId = a.Key,
                                        Count = a.Count()
                                    } )
                                    .FirstOrDefault();
                                if ( attendanceCampus != null )
                                {
                                    attendanceCampusId = attendanceCampus.CampusId;
                                    attendanceCampusCount = attendanceCampus.Count;
                                }
                            }

                            // Calculate the campus based on giving
                            int? givingCampusId = null;
                            int givingCampusCount = 0;
                            if ( settings.IsMostFamilyGivingEnabled )
                            {
                                var startPeriod = RockDateTime.Now.AddDays( -settings.MostFamilyAttendancePeriod );
                                var givingCampus = personCampusGiving
                                    .Where( a => personIds.Contains( a.PersonId ) )
                                    .GroupBy( a => a.CampusId )
                                    .OrderByDescending( a => a.Count() )
                                    .Select( a => new
                                    {
                                        CampusId = a.Key,
                                        Count = a.Count()
                                    } )
                                    .FirstOrDefault();
                                if ( givingCampus != null )
                                {
                                    givingCampusId = givingCampus.CampusId;
                                    givingCampusCount = givingCampus.Count;
                                }
                            }

                            // If a campus could not be calculated for attendance or giving, move to next family.
                            if ( !attendanceCampusId.HasValue && !givingCampusId.HasValue )
                            {
                                continue;
                            }

                            // Figure out what the campus should be
                            int? currentCampusId = family.CampusId;
                            int? newCampusId = null;
                            if ( attendanceCampusId.HasValue )
                            {
                                if ( givingCampusId.HasValue && givingCampusId.Value != attendanceCampusId.Value )
                                {
                                    // If campus from attendance and giving are different
                                    switch ( settings.MostAttendanceOrGiving )
                                    {
                                        case Utility.Settings.DataAutomation.CampusCriteria.UseGiving:
                                            newCampusId = givingCampusId;
                                            break;

                                        case Utility.Settings.DataAutomation.CampusCriteria.UseAttendance:
                                            newCampusId = attendanceCampusId;
                                            break;

                                        case Utility.Settings.DataAutomation.CampusCriteria.UseHighestFrequency:

                                            // If frequency is the same for both, and either of the values are same as current, then don't change the campus
                                            if ( attendanceCampusCount == givingCampusCount &&
                                                currentCampusId.HasValue &&
                                                ( currentCampusId.Value == attendanceCampusId.Value || currentCampusId.Value == givingCampusId.Value ) )
                                            {
                                                newCampusId = null;
                                            }
                                            else
                                            {
                                                newCampusId = ( attendanceCampusCount > givingCampusCount ) ? attendanceCampusId : givingCampusId;
                                            }

                                            break;

                                            // if none of those, just ignore.
                                    }
                                }
                                else
                                {
                                    newCampusId = attendanceCampusId;
                                }
                            }
                            else
                            {
                                newCampusId = givingCampusId;
                            }

                            // Campus did not change
                            if ( !newCampusId.HasValue || newCampusId.Value == ( currentCampusId ?? 0 ) )
                            {
                                continue;
                            }

                            // Check to see if the campus change should be ignored
                            if ( currentCampusId.HasValue )
                            {
                                bool ignore = false;
                                foreach ( var exclusion in settings.IgnoreCampusChanges )
                                {
                                    if ( exclusion.FromCampus == currentCampusId.Value && exclusion.ToCampus == newCampusId )
                                    {
                                        if ( exclusion.BasedOn == Utility.Settings.DataAutomation.CampusCriteria.UseGiving )
                                        {
                                            if ( givingCampusId == exclusion.ToCampus )
                                            {
                                                ignore = true;
                                                break;
                                            }
                                        }
                                        else if ( exclusion.BasedOn == Utility.Settings.DataAutomation.CampusCriteria.UseAttendance )
                                        {
                                            if ( attendanceCampusId == exclusion.ToCampus )
                                            {
                                                ignore = true;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            ignore = true;
                                            break;
                                        }
                                    }
                                }

                                if ( ignore )
                                {
                                    continue;
                                }
                            }

                            // Update the campus
                            family.CampusId = newCampusId.Value;
                            rockContext.SaveChanges();

                            // Since we just successfully saved the change, increment the update counter
                            recordsUpdated++;
                        }
                    }
                    catch ( Exception ex )
                    {
                        // Log exception and keep on trucking.
                        ExceptionLogService.LogException( new Exception( $"Exception occurred trying to update campus for GroupId:{familyId}.", ex ), _httpContext );
                    }
                }

                // Format the result message
                return $"{recordsProcessed:N0} families were processed; campus was updated for {recordsUpdated:N0} of them.";
            }
            catch ( Exception ex )
            {
                // Log exception and return the exception messages.
                ExceptionLogService.LogException( ex, _httpContext );
                return ex.Messages().AsDelimited( "; " );
            }
        }

        #endregion

        #region Move Adult Children 

        /// <summary>
        /// Moves the adult children.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// Could not determine the 'Family' group type.
        /// or
        /// Could not determine the 'Adult' and 'Child' roles.
        /// </exception>
        private string MoveAdultChildren( IJobExecutionContext context )
        {
            try
            {
                context.UpdateLastStatusMessage( $"Processing Adult Children." );

                var settings = Rock.Web.SystemSettings.GetValue( SystemSetting.DATA_AUTOMATION_ADULT_CHILDREN ).FromJsonOrNull<Utility.Settings.DataAutomation.MoveAdultChildren>();
                if ( settings == null || !settings.IsEnabled )
                {
                    return "Not Enabled";
                }

                // Get some system guids
                var activeRecordStatusGuid = SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid();
                var homeAddressGuid = SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid();
                var homePhoneGuid = SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid();
                var personChangesGuid = SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid();
                var familyChangesGuid = SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid();

                // Get the family group type and roles
                var familyGroupType = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
                if ( familyGroupType == null )
                {
                    throw new Exception( "Could not determine the 'Family' group type." );
                }

                var childRole = familyGroupType.Roles.FirstOrDefault( r => r.Guid == SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() );
                var adultRole = familyGroupType.Roles.FirstOrDefault( r => r.Guid == SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() );
                if ( childRole == null || adultRole == null )
                {
                    throw new Exception( "Could not determine the 'Adult' and 'Child' roles." );
                }

                // Calculate the date to use for determining if someone is an adult based on their age (birthdate)
                var adultBirthdate = RockDateTime.Today.AddYears( 0 - settings.AdultAge );

                // Get a list of people marked as a child in any family, but who are now an "adult" based on their age
                var adultChildIds = new List<int>();
                using ( var rockContext = new RockContext() )
                {
                    // increase the timeout just in case.
                    rockContext.Database.CommandTimeout = 180;
                    rockContext.SourceOfChange = SOURCE_OF_CHANGE;

                    var qry = new GroupMemberService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( m =>
                            m.GroupRoleId == childRole.Id &&
                            m.Person.BirthDate.HasValue &&
                            m.Person.BirthDate <= adultBirthdate &&
                            m.Person.RecordStatusValue != null &&
                            m.Person.RecordStatusValue.Guid == activeRecordStatusGuid &&
                            !m.Person.IsLockedAsChild );

                    if ( settings.IsOnlyMoveGraduated )
                    {
                        int maxGradYear = CalculateMaxGradYear();
                        // Children who have a graduation year and have graduated
                        qry = qry.Where( gm => gm.Person.GraduationYear != null && gm.Person.GraduationYear <= maxGradYear );
                    }

                    adultChildIds = qry
                        .OrderBy( m => m.PersonId )
                        .Select( m => m.PersonId )
                        .Distinct()
                        .Take( settings.MaximumRecords )
                        .ToList();
                }

                // Counter for displaying results
                int recordsProcessed = 0;
                int recordsUpdated = 0;
                int totalRecords = adultChildIds.Count();

                // Loop through each person
                foreach ( int personId in adultChildIds )
                {
                    try
                    {
                        // Update the status on every 100th record
                        if ( recordsProcessed % 100 == 0 )
                        {
                            context.UpdateLastStatusMessage( $"Processing Adult Children: {recordsProcessed:N0} of {totalRecords:N0} children processed; {recordsUpdated:N0} have been moved to their own family." );
                        }

                        recordsProcessed++;

                        // Using a new rockcontext for each one (to improve performance)
                        using ( var rockContext = new RockContext() )
                        {
                            rockContext.SourceOfChange = SOURCE_OF_CHANGE;

                            // Get all the 'family' group member records for this person.
                            var groupMemberService = new GroupMemberService( rockContext );
                            var groupMembers = groupMemberService.Queryable()
                                .Where( m =>
                                    m.PersonId == personId &&
                                    m.Group.GroupTypeId == familyGroupType.Id )
                                .ToList();

                            // If there are no group members (shouldn't happen), just ignore and keep going
                            if ( !groupMembers.Any() )
                            {
                                continue;
                            }

                            // Get a reference to the person
                            var person = groupMembers.First().Person;

                            // Get the person's primary family, and if we can't get that (something else that shouldn't happen), just ignore this person.
                            var primaryFamily = person.PrimaryFamily;
                            if ( primaryFamily == null )
                            {
                                continue;
                            }

                            // Setup a variable for tracking person changes
                            var personChanges = new List<string>();

                            // Get all the parent and sibling ids (for adding relationships later)
                            var parentIds = groupMembers
                                .SelectMany( m => m.Group.Members )
                                .Where( m =>
                                    m.PersonId != personId &&
                                    m.GroupRoleId == adultRole.Id )
                                .Select( m => m.PersonId )
                                .Distinct()
                                .ToList();

                            var siblingIds = groupMembers
                                .SelectMany( m => m.Group.Members )
                                .Where( m =>
                                    m.PersonId != personId &&
                                    m.GroupRoleId == childRole.Id )
                                .Select( m => m.PersonId )
                                .Distinct()
                                .ToList();

                            // If person is already an adult in any family, lets find the first one, and use that as the new family
                            var newFamily = groupMembers
                                .Where( m => m.GroupRoleId == adultRole.Id )
                                .OrderBy( m => m.GroupOrder )
                                .Select( m => m.Group )
                                .FirstOrDefault();

                            // If person was not already an adult in any family, let's look for a family where they are the only person, or create a new one
                            if ( newFamily == null )
                            {
                                // Try to find a family where they are the only one in the family.
                                newFamily = groupMembers
                                    .Select( m => m.Group )
                                    .Where( g => !g.Members.Any( m => m.PersonId != personId ) )
                                    .FirstOrDefault();

                                // If we found one, make them an adult in that family
                                if ( newFamily != null )
                                {
                                    // The current person should be the only one in this family, but lets loop through each member anyway
                                    foreach ( var groupMember in groupMembers.Where( m => m.GroupId == newFamily.Id ) )
                                    {
                                        groupMember.GroupRoleId = adultRole.Id;
                                    }

                                    // Save role change to history
                                    var memberChanges = new History.HistoryChangeList();
                                    History.EvaluateChange( memberChanges, "Role", string.Empty, adultRole.Name );
                                    HistoryService.SaveChanges( rockContext, typeof( Person ), familyChangesGuid, personId, memberChanges, newFamily.Name, typeof( Group ), newFamily.Id, false, null, SOURCE_OF_CHANGE );
                                }
                                else
                                {
                                    // If they are not already an adult in a family, and they're not in any family by themeselves, we need to create a new family for them.
                                    // The SaveNewFamily adds history records for this

                                    // Create a new group member and family
                                    var groupMember = new GroupMember
                                    {
                                        Person = person,
                                        GroupRoleId = adultRole.Id,
                                        GroupMemberStatus = GroupMemberStatus.Active
                                    };
                                    newFamily = GroupService.SaveNewFamily( rockContext, new List<GroupMember> { groupMember }, primaryFamily.CampusId, false );
                                }
                            }

                            // If user configured the job to copy home address and this person's family does not have any home addresses, copy them from the primary family
                            if ( settings.UseSameHomeAddress && !newFamily.GroupLocations.Any( l => l.GroupLocationTypeValue != null && l.GroupLocationTypeValue.Guid == homeAddressGuid ) )
                            {
                                var familyChanges = new History.HistoryChangeList();

                                foreach ( var groupLocation in primaryFamily.GroupLocations.Where( l => l.GroupLocationTypeValue != null && l.GroupLocationTypeValue.Guid == homeAddressGuid ) )
                                {
                                    newFamily.GroupLocations.Add( new GroupLocation
                                    {
                                        LocationId = groupLocation.LocationId,
                                        GroupLocationTypeValueId = groupLocation.GroupLocationTypeValueId,
                                        IsMailingLocation = groupLocation.IsMailingLocation,
                                        IsMappedLocation = groupLocation.IsMappedLocation
                                    } );

                                    History.EvaluateChange( familyChanges, groupLocation.GroupLocationTypeValue.Value + " Location", string.Empty, groupLocation.Location.ToString() );
                                }

                                HistoryService.SaveChanges( rockContext, typeof( Person ), familyChangesGuid, personId, familyChanges, false,null, SOURCE_OF_CHANGE );
                            }

                            // If user configured the job to copy home phone and this person does not have a home phone, copy the first home phone number from another adult in original family(s)
                            if ( settings.UseSameHomePhone && !person.PhoneNumbers.Any( p => p.NumberTypeValue != null && p.NumberTypeValue.Guid == homePhoneGuid ) )
                            {
                                // First look for adults in primary family
                                var homePhone = primaryFamily.Members
                                    .Where( m =>
                                        m.PersonId != person.Id &&
                                        m.GroupRoleId == adultRole.Id )
                                    .SelectMany( m => m.Person.PhoneNumbers )
                                    .FirstOrDefault( p => p.NumberTypeValue != null && p.NumberTypeValue.Guid == homePhoneGuid );

                                // If one was not found in primary family, look in any of the person's other families
                                if ( homePhone == null )
                                {
                                    homePhone = groupMembers
                                        .Where( m => m.GroupId != primaryFamily.Id )
                                        .SelectMany( m => m.Group.Members )
                                        .Where( m =>
                                            m.PersonId != person.Id &&
                                            m.GroupRoleId == adultRole.Id )
                                        .SelectMany( m => m.Person.PhoneNumbers )
                                        .FirstOrDefault( p => p.NumberTypeValue != null && p.NumberTypeValue.Guid == homePhoneGuid );
                                }

                                // If we found one, add it to the person
                                if ( homePhone != null )
                                {
                                    person.PhoneNumbers.Add( new PhoneNumber
                                    {
                                        CountryCode = homePhone.CountryCode,
                                        Number = homePhone.Number,
                                        NumberFormatted = homePhone.NumberFormatted,
                                        NumberReversed = homePhone.NumberReversed,
                                        Extension = homePhone.Extension,
                                        NumberTypeValueId = homePhone.NumberTypeValueId,
                                        IsMessagingEnabled = homePhone.IsMessagingEnabled,
                                        IsUnlisted = homePhone.IsUnlisted,
                                        Description = homePhone.Description
                                    } );
                                }
                            }

                            // At this point, the person was either already an adult in one or more families, 
                            //   or we updated one of their records to be an adult, 
                            //   or we created a new family with them as an adult. 
                            // So now we should delete any of the remaining family member records where they are still a child.
                            foreach ( var groupMember in groupMembers.Where( m => m.GroupRoleId == childRole.Id ) )
                            {
                                groupMemberService.Delete( groupMember );
                            }

                            // Save all the changes
                            rockContext.SaveChanges();

                            // Since we just successfully saved the change, increment the update counter
                            recordsUpdated++;

                            // If configured to do so, add any parent relationships (these methods take care of logging changes)
                            if ( settings.ParentRelationshipId.HasValue )
                            {
                                foreach ( int parentId in parentIds )
                                {
                                    groupMemberService.CreateKnownRelationship( personId, parentId, settings.ParentRelationshipId.Value );
                                }
                            }

                            // If configured to do so, add any sibling relationships
                            if ( settings.SiblingRelationshipId.HasValue )
                            {
                                foreach ( int siblingId in siblingIds )
                                {
                                    groupMemberService.CreateKnownRelationship( personId, siblingId, settings.SiblingRelationshipId.Value );
                                }
                            }

                            // Look for any workflows
                            if ( settings.WorkflowTypeIds.Any() )
                            {
                                // Create parameters for the old/new family
                                var workflowParameters = new Dictionary<string, string>
                            {
                                { "OldFamily", primaryFamily.Guid.ToString() },
                                { "NewFamily", newFamily.Guid.ToString() }
                            };

                                // Launch all the workflows
                                foreach ( var wfId in settings.WorkflowTypeIds )
                                {
                                    person.LaunchWorkflow( wfId, person.FullName, workflowParameters );
                                }
                            }
                        }
                    }
                    catch ( Exception ex )
                    {
                        // Log exception and keep on trucking.
                        ExceptionLogService.LogException( new Exception( $"Exception occurred trying to check for adult child mode on PersonId:{personId}.", ex ), _httpContext );
                    }
                }

                // Format the result message
                return $"{recordsProcessed:N0} children were processed; {recordsUpdated:N0} were moved to their own family.";
            }
            catch ( Exception ex )
            {
                // Log exception and return the exception messages.
                ExceptionLogService.LogException( ex, _httpContext );
                return ex.Messages().AsDelimited( "; " );
            }
        }

        /// <summary>
        /// Calculates the last graduation year which children can have graduated in order to be considered an adult. I.E. Any year greater than this and they have not graduated yet
        /// </summary>
        /// <returns></returns>
        private int CalculateMaxGradYear()
        {
            var graduationDateWithCurrentYear = GlobalAttributesCache.Get().GetValue( "GradeTransitionDate" ).MonthDayStringAsDateTime() ?? new DateTime( RockDateTime.Today.Year, 6, 1 );
            if ( !( graduationDateWithCurrentYear < RockDateTime.Today ) )
            {
                // if the graduation date hasn't occurred this year yet, return last year's graduation date as any children with this years date have not graduated yet
                return graduationDateWithCurrentYear.AddYears( -1 ).Year;
            }
            else
            {
                return graduationDateWithCurrentYear.Year;
            }
        }


        #endregion Move Adult Children

        #region Update Person Connection Status

        /// <summary>
        /// Updates the person connection status.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private string UpdatePersonConnectionStatus( IJobExecutionContext context )
        {
            var settings = Rock.Web.SystemSettings.GetValue( SystemSetting.DATA_AUTOMATION_UPDATE_PERSON_CONNECTION_STATUS ).FromJsonOrNull<Utility.Settings.DataAutomation.UpdatePersonConnectionStatus>();
            if ( settings == null || !settings.IsEnabled )
            {
                return "Not Enabled";
            }

            int recordsUpdated = 0;
            int totalToUpdate = 0;
            int recordsWithError = 0;

            context.UpdateLastStatusMessage( $"Processing Connection Status Update" );

            foreach ( var connectionStatusDataviewMapping in settings.ConnectionStatusValueIdDataviewIdMapping.Where( a => a.Value.HasValue ) )
            {
                int connectionStatusValueId = connectionStatusDataviewMapping.Key;
                var cacheConnectionStatusValue = DefinedValueCache.Get( connectionStatusValueId );
                context.UpdateLastStatusMessage( $"Processing Connection Status Update for {cacheConnectionStatusValue}" );
                int dataViewId = connectionStatusDataviewMapping.Value.Value;
                using ( var dataViewRockContext = new RockContext() )
                {
                    var dataView = new DataViewService( dataViewRockContext ).Get( dataViewId );
                    if ( dataView != null )
                    {
                        List<string> errorMessages = new List<string>();
                        var qryPersonsInDataView = dataView.GetQuery( null, dataViewRockContext, null, out errorMessages ) as IQueryable<Person>;
                        if ( qryPersonsInDataView != null )
                        {
                            var personsToUpdate = qryPersonsInDataView.Where( a => a.ConnectionStatusValueId != connectionStatusValueId ).AsNoTracking().ToList();
                            totalToUpdate += personsToUpdate.Count();
                            foreach ( var person in personsToUpdate )
                            {
                                try
                                {
                                    using ( var updateRockContext = new RockContext() )
                                    {
                                        updateRockContext.SourceOfChange = SOURCE_OF_CHANGE;
                                        // Attach the person to the updateRockContext so that it'll be tracked/saved using updateRockContext 
                                        updateRockContext.People.Attach( person );

                                        recordsUpdated++;
                                        person.ConnectionStatusValueId = connectionStatusValueId;
                                        updateRockContext.SaveChanges();

                                        if ( recordsUpdated % 100 == 0 )
                                        {
                                            context.UpdateLastStatusMessage( $"Processing Connection Status Update for {cacheConnectionStatusValue}: {recordsUpdated:N0} of {totalToUpdate:N0}" );
                                        }
                                    }
                                }
                                catch ( Exception ex )
                                {
                                    // log but don't throw
                                    ExceptionLogService.LogException( new Exception( $"Exception occurred trying to update connection status for PersonId:{person.Id}.", ex ), _httpContext );
                                    recordsWithError += 1;
                                }
                            }
                        }
                    }
                }
            }

            // Format the result message
            string result = $"{recordsUpdated:N0} person records were updated with new connection status.";
            if ( recordsWithError > 0 )
            {
                result += " {recordsWithError:N0} records logged an exception.";
            }

            return result;
        }

        #endregion  Update Person Connection Status

        #region Update Family Status

        /// <summary>
        /// Updates the family status.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private string UpdateFamilyStatus( IJobExecutionContext context )
        {
            var settings = Rock.Web.SystemSettings.GetValue( SystemSetting.DATA_AUTOMATION_UPDATE_FAMILY_STATUS ).FromJsonOrNull<Utility.Settings.DataAutomation.UpdateFamilyStatus>();
            if ( settings == null || !settings.IsEnabled )
            {
                return "Not Enabled";
            }

            int recordsUpdated = 0;
            int totalToUpdate = 0;

            context.UpdateLastStatusMessage( $"Processing Family Status Update" );

            foreach ( var groupStatusDataviewMapping in settings.GroupStatusValueIdDataviewIdMapping.Where( a => a.Value.HasValue ) )
            {
                int groupStatusValueId = groupStatusDataviewMapping.Key;
                int dataViewId = groupStatusDataviewMapping.Value.Value;
                using ( var dataViewRockContext = new RockContext() )
                {
                    var dataView = new DataViewService( dataViewRockContext ).Get( dataViewId );
                    if ( dataView != null )
                    {
                        List<string> errorMessages = new List<string>();
                        var qryGroupsInDataView = dataView.GetQuery( null, dataViewRockContext, null, out errorMessages ) as IQueryable<Group>;
                        if ( qryGroupsInDataView != null )
                        {
                            var groupsToUpdate = qryGroupsInDataView.Where( a => a.StatusValueId != groupStatusValueId ).AsNoTracking().ToList();
                            totalToUpdate += groupsToUpdate.Count();
                            foreach ( var group in groupsToUpdate )
                            {
                                using ( var updateRockContext = new RockContext() )
                                {
                                    updateRockContext.SourceOfChange = SOURCE_OF_CHANGE;
                                    // Attach the group to the updateRockContext so that it'll be tracked/saved using updateRockContext 
                                    updateRockContext.Groups.Attach( group );

                                    recordsUpdated++;
                                    group.StatusValueId = groupStatusValueId;
                                    updateRockContext.SaveChanges();

                                    if ( recordsUpdated % 100 == 0 )
                                    {
                                        context.UpdateLastStatusMessage( $"Processing Family Status Update: {recordsUpdated:N0} of {totalToUpdate:N0}" );
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Format the result message
            return $"{recordsUpdated:N0} families were updated with new status.";
        }

        #endregion  Update Family Status

        #region Helper Methods

        /// <summary>
        /// Gets the people who contributed.
        /// </summary>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        /// <param name="periodInDays">The period in days.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<int> GetPeopleWhoContributed( bool enabled, int periodInDays, RockContext rockContext )
        {
            if ( enabled )
            {
                var contributionType = DefinedValueCache.Get( SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );
                if ( contributionType != null )
                {
                    var startDate = RockDateTime.Now.AddDays( -periodInDays );
                    return new FinancialTransactionService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( t =>
                            t.TransactionTypeValueId == contributionType.Id &&
                            t.TransactionDateTime.HasValue &&
                            t.TransactionDateTime.Value >= startDate &&
                            t.AuthorizedPersonAliasId.HasValue )
                        .Select( a => a.AuthorizedPersonAlias.PersonId )
                        .Distinct()
                        .ToList();
                }
            }

            return new List<int>();
        }

        /// <summary>
        /// Gets the people who attended service group.
        /// </summary>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        /// <param name="periodInDays">The period in days.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<int> GetPeopleWhoAttendedServiceGroup( bool enabled, int periodInDays, RockContext rockContext )
        {
            if ( enabled )
            {
                var startDate = RockDateTime.Now.AddDays( -periodInDays );

                return new AttendanceService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( a =>
                        a.Occurrence.Group != null &&
                        a.Occurrence.Group.GroupType != null &&
                        a.Occurrence.Group.GroupType.AttendanceCountsAsWeekendService &&
                        a.StartDateTime >= startDate &&
                        a.DidAttend.HasValue &&
                        a.DidAttend.Value == true &&
                        a.PersonAlias != null )
                    .Select( a => a.PersonAlias.PersonId )
                    .Distinct()
                    .ToList();
            }

            return new List<int>();
        }

        /// <summary>
        /// Gets the type of the people who attended group.
        /// </summary>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        /// <param name="includeGroupTypeIds">The include group type ids.</param>
        /// <param name="excludeGroupTypeIds">The exclude group type ids.</param>
        /// <param name="periodInDays">The period in days.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<int> GetPeopleWhoAttendedGroupType( bool enabled, List<int> includeGroupTypeIds, List<int> excludeGroupTypeIds, int periodInDays, RockContext rockContext )
        {
            if ( enabled )
            {
                var startDate = RockDateTime.Now.AddDays( -periodInDays );

                var qry = new AttendanceService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( a =>
                        a.Occurrence.Group != null &&
                        a.StartDateTime >= startDate &&
                        a.DidAttend.HasValue &&
                        a.DidAttend.Value == true &&
                        a.PersonAlias != null );

                if ( includeGroupTypeIds != null && includeGroupTypeIds.Any() )
                {
                    qry = qry.Where( t => includeGroupTypeIds.Contains( t.Occurrence.Group.GroupTypeId ) );
                }

                if ( excludeGroupTypeIds != null && excludeGroupTypeIds.Any() )
                {
                    qry = qry.Where( t => !excludeGroupTypeIds.Contains( t.Occurrence.Group.GroupTypeId ) );
                }

                return qry
                    .Select( a => a.PersonAlias.PersonId )
                    .Distinct()
                    .ToList();
            }

            return new List<int>();
        }

        /// <summary>
        /// Gets the people who submitted prayer request.
        /// </summary>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        /// <param name="periodInDays">The period in days.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<int> GetPeopleWhoSubmittedPrayerRequest( bool enabled, int periodInDays, RockContext rockContext )
        {
            if ( enabled )
            {
                var startDate = RockDateTime.Now.AddDays( -periodInDays );

                return new PrayerRequestService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( a =>
                        a.EnteredDateTime >= startDate &&
                        a.RequestedByPersonAlias != null )
                    .Select( a => a.RequestedByPersonAlias.PersonId )
                    .Distinct()
                    .ToList();
            }

            return new List<int>();
        }

        /// <summary>
        /// Gets the people with person attribute updates.
        /// </summary>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        /// <param name="includeAttributeIds">The include attribute ids.</param>
        /// <param name="excludeAttributeIds">The exclude attribute ids.</param>
        /// <param name="periodInDays">The period in days.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<int> GetPeopleWithPersonAttributUpdates( bool enabled, List<int> includeAttributeIds, List<int> excludeAttributeIds, int periodInDays, RockContext rockContext )
        {
            if ( enabled )
            {
                var startDate = RockDateTime.Now.AddDays( -periodInDays );

                var personEntityTypeId = EntityTypeCache.Get( typeof( Person ) ).Id;

                var qry = new AttributeValueService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( a =>
                        a.ModifiedDateTime.HasValue &&
                        a.ModifiedDateTime.Value >= startDate &&
                        a.Attribute.EntityTypeId == personEntityTypeId &&
                        a.EntityId.HasValue );

                if ( includeAttributeIds != null && includeAttributeIds.Any() )
                {
                    qry = qry.Where( t => includeAttributeIds.Contains( t.AttributeId ) );
                }

                if ( excludeAttributeIds != null && excludeAttributeIds.Any() )
                {
                    qry = qry.Where( t => !excludeAttributeIds.Contains( t.AttributeId ) );
                }

                return qry
                    .Select( a => a.EntityId.Value )
                    .Distinct()
                    .ToList();
            }

            return new List<int>();
        }

        /// <summary>
        /// Gets the people with interactions.
        /// </summary>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        /// <param name="interactionItems">The interaction items.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<int> GetPeopleWithInteractions( bool enabled, List<Utility.Settings.DataAutomation.InteractionItem> interactionItems, RockContext rockContext )
        {
            if ( enabled && interactionItems != null && interactionItems.Any() )
            {
                var personIdList = new List<int>();

                foreach ( var interactionItem in interactionItems )
                {
                    var startDate = RockDateTime.Now.AddDays( -interactionItem.LastInteractionDays );

                    personIdList.AddRange( new InteractionService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( a =>
                            a.InteractionDateTime >= startDate &&
                            a.InteractionComponent.Channel.Guid == interactionItem.Guid &&
                            a.PersonAlias != null )
                        .Select( a => a.PersonAlias.PersonId )
                        .Distinct()
                        .ToList() );
                }

                return personIdList.Distinct().ToList();
            }

            return new List<int>();
        }

        /// <summary>
        /// Gets the people in data view query.
        /// </summary>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        /// <param name="dataviewId">The dataview identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private IQueryable<int> GetPeopleInDataViewQuery( bool enabled, int? dataviewId, RockContext rockContext )
        {
            if ( enabled && dataviewId.HasValue )
            {
                var dataView = new DataViewService( rockContext ).Get( dataviewId.Value );
                if ( dataView != null )
                {
                    List<string> errorMessages = new List<string>();
                    var qry = dataView.GetQuery( null, rockContext, null, out errorMessages );
                    if ( qry != null )
                    {
                        return qry.Select( e => e.Id );
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Creates the entity set identifier query.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private IQueryable<int> CreateEntitySetIdQuery( List<int> ids, RockContext rockContext )
        {
            var entitySet = new EntitySet();
            entitySet.EntityTypeId = EntityTypeCache.Get<Rock.Model.Person>().Id;
            entitySet.ExpireDateTime = RockDateTime.Now.AddDays( 1 );

            var service = new EntitySetService( rockContext );
            service.Add( entitySet );
            rockContext.SaveChanges();

            if ( ids != null && ids.Any() )
            {
                List<EntitySetItem> entitySetItems = new List<EntitySetItem>();

                foreach ( var id in ids )
                {
                    var item = new EntitySetItem();
                    item.EntitySetId = entitySet.Id;
                    item.EntityId = id;
                    entitySetItems.Add( item );
                }

                rockContext.BulkInsert( entitySetItems );
            }

            return new EntitySetItemService( rockContext )
                .Queryable().AsNoTracking()
                .Where( i => i.EntitySetId == entitySet.Id )
                .Select( i => i.EntityId );
        }

        #endregion Helper Methods

        #region Helper Classes

        /// <summary>
        /// Helper class for tracking person/campus values
        /// </summary>
        public class PersonCampus
        {
            /// <summary>
            /// Gets or sets the person identifier.
            /// </summary>
            /// <value>
            /// The person identifier.
            /// </value>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the campus identifier.
            /// </summary>
            /// <value>
            /// The campus identifier.
            /// </value>
            public int CampusId { get; set; }
        }

        #endregion
    }
}
