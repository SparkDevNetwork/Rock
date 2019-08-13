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
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock;
using Rock.Crm.ConnectionStatusChangeReport;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.Utility;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Crm
{
    /// <summary>
    /// Create and manage test data for the Rock CRM module.
    /// </summary>
    [TestClass]
    public class ConnectionStatusChangeReportTests
    {
        #region Test Data

        private const string _TestDataSourceOfChange = "ConnectionStatusChangeReportTest";
        private const int _HistoryTestMaxPeople = 100;
        private readonly DateTime _HistoryTestEndDate = RockDateTime.Now;
        private readonly int _InvalidCampusId = 99;
        private readonly int _MainCampusId = 1;

        /// <summary>
        /// Adds the required test data to the current database.
        /// </summary>
        [TestMethod]
        [TestCategory( "Rock.Crm.ConnectionStatusChangeReport.Setup" )]
        public void AddTestDataToCurrentDatabase()
        {
            this.RemoveTestDataFromCurrentDatabase();

            this.AddTestDataConnectionStatusChangeHistory();
        }

        /// <summary>
        /// Removes the test data from the current database.
        /// </summary>
        [TestMethod]
        [TestCategory( "Rock.Crm.ConnectionStatusChangeReport.Setup" )]
        public void RemoveTestDataFromCurrentDatabase()
        {
            var dataContext = new RockContext();

            // History table has no dependencies, so use SQL delete for efficiency.
            var recordsDeleted = dataContext.Database.ExecuteSqlCommand( "delete from [History] where [SourceOfChange] = @p0 ", _TestDataSourceOfChange );

            Debug.Print( $"Delete History: {recordsDeleted} records deleted." );
        }

        /// <summary>
        /// Adds a predictable set of Connection Status changes to the test database that are used for integration testing.
        /// </summary>
        [TestMethod]
        [TestCategory( "Rock.Crm.ConnectionStatusChangeReport.Setup.Maintenance" )]
        public void AddTestDataConnectionStatusChangeHistory()
        {
            var dataContext = new RockContext();

            this.ThrowIfTestHistoryDataExists( dataContext, _TestDataSourceOfChange );

            var memberConnectionStatusId = GetStatusValueIdOrThrow( "Member" );
            var attenderConnectionStatusId = GetStatusValueIdOrThrow( "Attendee" );
            var visitorConnectionStatusId = GetStatusValueIdOrThrow( "Visitor" );
            var prospectConnectionStatusId = GetStatusValueIdOrThrow( "Web Prospect" );

            var personService = new PersonService( dataContext );

            var personList = personService.Queryable()
                .AsNoTracking()
                .Where( x => !x.IsSystem )
                .Take( _HistoryTestMaxPeople )
                .ToList();

            var currentDate = _HistoryTestEndDate;

            var rng = new Random();

            var adminPerson = this.GetAdminPersonOrThrow( personService );

            // Create new change history entries for each person.
            int entriesAdded = 0;
            History.HistoryChange historyEntry;

            foreach ( var person in personList )
            {
                var historyChanges = new History.HistoryChangeList();

                var dateOfChange = currentDate;

                if ( person.ConnectionStatusValueId == memberConnectionStatusId )
                {
                    // For each Person who has a Connection Status of Member, add prior change events for Visitor --> Attendee --> Member.
                    
                    // Add a change for Attendee --> Member.
                    dateOfChange = dateOfChange.AddDays( rng.Next( 1, 365 ) * -1 );

                    historyEntry = historyChanges.AddChange( History.HistoryVerb.Modify, History.HistoryChangeType.Property, "Connection Status" )
                        .SetOldValue( "Attendee" )
                        .SetNewValue( "Member" )
                        .SetRawValues( attenderConnectionStatusId.ToString(), memberConnectionStatusId.ToString() )
                        .SetDateOfChange( dateOfChange );

                    // Add a change for Visitor --> Attendee.
                    // This change record has only the OldValue and NewValue, compatible with records created prior to Rock v1.8
                    dateOfChange = dateOfChange.AddDays( rng.Next( 1, 365 ) * -1 );

                    historyEntry = historyChanges.AddChange( History.HistoryVerb.Modify, History.HistoryChangeType.Property, "Connection Status" )
                        .SetOldValue( "Visitor" )
                        .SetNewValue( "Attendee" )
                        .SetDateOfChange( dateOfChange );

                    entriesAdded += 2;
                }
                else if ( person.ConnectionStatusValueId == visitorConnectionStatusId )
                {
                    // For each Person who has a Connection Status of Visitor, add prior change events for Web Prospect --> Visitor.
                    dateOfChange = dateOfChange.AddDays( rng.Next( 1, 365 ) * -1 );

                    historyEntry = historyChanges.AddChange( History.HistoryVerb.Modify, History.HistoryChangeType.Property, "Connection Status" )
                        .SetOldValue( "Web Prospect" )
                        .SetNewValue( "Visitor" )
                        .SetRawValues( prospectConnectionStatusId.ToString(), visitorConnectionStatusId.ToString() )
                        .SetDateOfChange( dateOfChange );

                    entriesAdded += 1;
                }

                Debug.Print( $"Processed History Entries... [PersonId={person.Id}, Date={dateOfChange}]" );

                HistoryService.SaveChanges( dataContext, typeof( Person ), global::Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(), person.Id, historyChanges, true, adminPerson.PrimaryAliasId, _TestDataSourceOfChange );
            }

            Debug.Print( $"Create Data completed: { entriesAdded } history entries created." );

            Assert.IsTrue( entriesAdded > 0, "No history entries created." );
        }

        /// <summary>
        /// Adds a randomized set of Connection Status changes to the test database.
        /// This is only required for performance testing.
        /// </summary>
        [TestMethod]
        [TestCategory( "Rock.Crm.ConnectionStatusChangeReport.Setup.Maintenance" )]
        public void AddTestDataConnectionStatusChangeHistoryRandom()
        {
            var dataContext = new RockContext();

            this.ThrowIfTestHistoryDataExists( dataContext, _TestDataSourceOfChange );

            // Add Connection Status Change History Entries.
            var personService = new PersonService( dataContext );

            var personQuery = personService.Queryable().AsNoTracking().Where( x => !x.IsSystem )
                .Take( 100 ).ToList();

            var statusType = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSON_CONNECTION_STATUS );

            var statusValues = statusType.DefinedValues.Select( x => x.Id ).ToList();

            int? toStatusId;
            int? fromStatusId;

            var currentDate = _HistoryTestEndDate;

            var rng = new Random();

            var adminPerson = this.GetAdminPersonOrThrow( personService );

            // For each person, add a random number of status changes over a random period of time prior to today.
            int entriesAdded = 0;

            foreach ( var person in personQuery )
            {
                var historyChanges = new History.HistoryChangeList();

                var numberOfChanges = rng.Next( 0, 3 );

                var dateOfChange = currentDate;

                // Set the initial value to the current status.
                fromStatusId = person.ConnectionStatusValueId;

                for ( int i = 1; i <= numberOfChanges; i++ )
                {
                    dateOfChange = dateOfChange.AddDays( rng.Next( 1, 365 ) * -1 );

                    // Set the target status for this change to be the status prior to the most recent status change.
                    toStatusId = fromStatusId;

                    do
                    {
                        fromStatusId = statusValues.GetRandomElement();
                    }
                    while ( toStatusId == fromStatusId );

                    var historyEntry = historyChanges.AddChange( History.HistoryVerb.Modify, History.HistoryChangeType.Property, "Connection Status" )
                        .SetNewValue( toStatusId.ToStringSafe() )
                        .SetOldValue( fromStatusId.ToStringSafe() )
                        .SetRawValues( fromStatusId.ToStringSafe(), toStatusId.ToStringSafe() )
                        .SetDateOfChange( dateOfChange );

                    Debug.Print( $"Added History Entry [PersonId={person.Id}, Date={dateOfChange}, OldValue={fromStatusId}, NewValue={toStatusId}" );

                    entriesAdded++;
                }

                HistoryService.SaveChanges( dataContext, typeof( Person ), global::Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(), person.Id, historyChanges, true, adminPerson.PrimaryAliasId, _TestDataSourceOfChange );
            }

            Debug.Print( $"Create Data completed: { entriesAdded } history entries created." );

            Assert.IsTrue( entriesAdded > 0, "No history entries created." );
        }

        /// <summary>
        /// Check if test records already exist in the current database.
        /// </summary>
        /// <param name="dataContext"></param>
        /// <param name="sourceOfChangeIdentifier"></param>
        private void ThrowIfTestHistoryDataExists( RockContext dataContext, string sourceOfChangeIdentifier )
        {
            var historyService = new HistoryService( dataContext );

            var testEntryExists = historyService.Queryable().AsNoTracking().Any( x => x.SourceOfChange == sourceOfChangeIdentifier );

            Assert.IsFalse( testEntryExists, $"History Test Data already exists in this database. [SourceOfChange={sourceOfChangeIdentifier}]" );
        }

        #endregion

        #region Tests

        /// <summary>
        /// Verify that the test data in the target database is valid.
        /// </summary>
        [TestMethod]
        [TestCategory( "Rock.Crm.ConnectionStatusChangeReport.Tests" )]
        public void TestDataIsValid()
        {
            var dataContext = new RockContext();

            // Get an unfiltered report and verify that it does not contain PrimaryCampusId=null for any record.
            var reportBase = this.GetBaselineReport();

            int hasCampusCount = reportBase.ChangeEvents.Count( x => x.CampusId != null );

            Assert.IsTrue( (hasCampusCount > 0), "Person.CampusId field is not populated. To calculate this value, run the \"Rock Cleanup\" Job for the current database." );
        }

        /// <summary>
        /// Verify the report can be correctly filtered by Campus.
        /// </summary>
        [TestMethod]
        [TestCategory( "Rock.Crm.ConnectionStatusChangeReport.Tests" )]
        public void FilterByCampusShouldReturnPeopleInMatchedCampusOnly()
        {
            var dataContext = new RockContext();

            // Get an unfiltered report and verify that it contains records for Campus "Main".
            // This establishes the baseline for the test.
            var reportBase = this.GetBaselineReport();

            Assert.IsTrue( reportBase.ChangeEvents.Any( x => x.CampusId == _MainCampusId ), "History events expected but not found. [Campus=(unfiltered)" );

            // The standard test data set does not currently have data for people in multiple campuses.
            // As an alternative, create a filtered report for a non-existent Campus and verify that no records are returned.
            var settings = new ConnectionStatusChangeReportSettings();

            settings.CampusId = _InvalidCampusId;

            var reportService = new ConnectionStatusChangeReportBuilder( dataContext, settings );

            var reportFiltered = reportService.CreateReport();

            Assert.IsFalse( reportFiltered.ChangeEvents.Any( x => x.CampusId == _MainCampusId ), "History events found but not expected. [CampusId=999]" );
        }

        /// <summary>
        /// Verify the report can be correctly filtered by Date.
        /// </summary>
        /// <remarks>
        /// We only test one type of time period filter here, but if it passes we can assume that all are good;
        /// the time period filter should always resolve to a start and end date if it is working correctly.
        /// </remarks>
        [TestMethod]
        [TestCategory( "Rock.Crm.ConnectionStatusChangeReport.Tests" )]
        public void FilterByCurrentYearShouldReturnChangesInCurrentYearOnly()
        {
            var dataContext = new RockContext();

            // Get an unfiltered report and verify that it contains records for this year and previous years.
            // This establishes the baseline for the test.
            var reportBase = this.GetBaselineReport();

            int currentYear = RockDateTime.Now.Year;

            Assert.IsTrue( reportBase.ChangeEvents.Any( x => x.EventDate.Year == currentYear ), "History events expected but not found. [EventDate=(current year)" );
            Assert.IsTrue( reportBase.ChangeEvents.Any( x => x.EventDate.Year == ( currentYear - 1 ) ), "History events expected but not found. [EventDate=(previous year)" );

            // Create a filtered report for current year only.
            var settings = new ConnectionStatusChangeReportSettings();

            settings.ReportPeriod.SetToCurrentPeriod( TimePeriodUnitSpecifier.Year );

            var reportService = new ConnectionStatusChangeReportBuilder( dataContext, settings );

            var reportFiltered = reportService.CreateReport();

            Assert.IsFalse( reportFiltered.ChangeEvents.Any( x => x.EventDate.Year != currentYear ), "History events found but not expected. [EventDate != (current year)]" );
        }

        /// <summary>
        /// Verify the correct result when a Status Filter is set for both Original Value and Updated Value.
        /// </summary>
        [TestMethod]
        [TestCategory( "Rock.Crm.ConnectionStatusChangeReport.Tests" )]
        public void FilterByOriginalAndUpdatedStatusesShouldReturnMatchingStatusesOnly()
        {
            var dataContext = new RockContext();

            var settings = new ConnectionStatusChangeReportSettings();

            ConnectionStatusChangeReportBuilder reportBuilder;
            ConnectionStatusChangeReportData report;

            var memberConnectionStatusId = GetStatusValueIdOrThrow( "Member" );
            var attendeeConnectionStatusId = GetStatusValueIdOrThrow( "Attendee" );
            var visitorConnectionStatusId = GetStatusValueIdOrThrow( "Visitor" );
            var prospectConnectionStatusId = GetStatusValueIdOrThrow( "Web Prospect" );

            // Get an unfiltered report.
            // The unfiltered data should contain Original Status=[Attendee|Web Prospect].
            reportBuilder = new ConnectionStatusChangeReportBuilder( dataContext, settings );

            report = reportBuilder.CreateReport();

            Assert.IsTrue( report.ChangeEvents.Any( x => x.OldConnectionStatusId == attendeeConnectionStatusId ), "Status expected but not found. [Status=Attendee]" );
            Assert.IsTrue( report.ChangeEvents.Any( x => x.OldConnectionStatusId == prospectConnectionStatusId ), "Status expected but not found. [Status=Web Prospect]" );

            // Get a filtered report: Original Status=Attendee, UpdatedStatus=Member.
            // The filtered data should only contain Original Status=Attendee.
            settings.FromConnectionStatusId = attendeeConnectionStatusId;
            settings.ToConnectionStatusId = memberConnectionStatusId;

            report = reportBuilder.CreateReport();

            Assert.IsTrue( !report.ChangeEvents.Any( x => x.OldConnectionStatusId != attendeeConnectionStatusId ), "Status found but not expected." );
        }

        /// <summary>
        /// Verify the correct result when a Status Filter is set for Updated Value only.
        /// </summary>
        [TestMethod]
        [TestCategory( "Rock.Crm.ConnectionStatusChangeReport.Tests" )]
        public void FilterByUnspecifiedOriginalStatusShouldReturnAllStatuses()
        {
            var dataContext = new RockContext();

            var settings = new ConnectionStatusChangeReportSettings();

            ConnectionStatusChangeReportBuilder reportBuilder;
            ConnectionStatusChangeReportData report;

            var attendeeStatusId = GetStatusValueIdOrThrow( "Attendee" );
            var memberStatusId = GetStatusValueIdOrThrow( "Member" );

            // Get an unfiltered report.
            // The unfiltered data should contain at least one record that is a transition from Visitor.
            reportBuilder = new ConnectionStatusChangeReportBuilder( dataContext, settings );

            report = reportBuilder.CreateReport();

            Assert.IsTrue( report.ChangeEvents.Any( x => x.OldConnectionStatusId == attendeeStatusId ), "Status expected but not found. [Status=Attendee]" );

            // Get a filtered report: Original Status=Attendee, UpdatedStatus=Member.
            settings.ToConnectionStatusId = GetStatusValueIdOrThrow( "Member" );

            report = reportBuilder.CreateReport();

            // The report should include events for new people, represented by a change from (null) --> (some status).
            Assert.IsTrue( report.ChangeEvents.Any( x => string.IsNullOrEmpty( x.OldConnectionStatusName ) ), "Status expected but not found. [OldStatus=(empty)]" );
        }

        /// <summary>
        /// Verify the correct result when a Status Filter is set for Original Value only.
        /// </summary>
        [TestMethod]
        [TestCategory( "Rock.Crm.ConnectionStatusChangeReport.Tests" )]
        public void FilterByUnspecifiedUpdatedStatusShouldReturnAllStatuses()
        {
            var dataContext = new RockContext();

            var settings = new ConnectionStatusChangeReportSettings();

            ConnectionStatusChangeReportBuilder reportBuilder;
            ConnectionStatusChangeReportData report;

            var attendeeStatusId = GetStatusValueIdOrThrow( "Attendee" );
            var memberStatusId = GetStatusValueIdOrThrow( "Member" );

            // Get an unfiltered report.
            // The unfiltered data should contain at least one record that is a transition from Visitor.
            reportBuilder = new ConnectionStatusChangeReportBuilder( dataContext, settings );

            report = reportBuilder.CreateReport();

            Assert.IsTrue( report.ChangeEvents.Any( x => x.OldConnectionStatusId == attendeeStatusId ), "Status expected but not found. [Status=Attendee]" );

            // Get a filtered report: Original Status=Attendee, UpdatedStatus=Member.
            settings.ToConnectionStatusId = GetStatusValueIdOrThrow( "Member" );

            report = reportBuilder.CreateReport();

            // The report should include events for new people, represented by a change from (null) --> (some status).
            Assert.IsTrue( report.ChangeEvents.Any( x => string.IsNullOrEmpty( x.OldConnectionStatusName ) ), "Status expected but not found. [OldStatus=(empty)]" );
        }

        #endregion

        #region Support Methods

        private ConnectionStatusChangeReportData _BaselineReport;

        /// <summary>
        /// Returns a report that contains all of the available test data for comparison results.
        /// </summary>
        /// <returns></returns>
        private ConnectionStatusChangeReportData GetBaselineReport()
        {
            if ( _BaselineReport == null )
            {
                var dataContext = new RockContext();

                var settings = new ConnectionStatusChangeReportSettings();

                ConnectionStatusChangeReportBuilder reportBuilder;

                // Get an unfiltered report.
                reportBuilder = new ConnectionStatusChangeReportBuilder( dataContext, settings );

                _BaselineReport = reportBuilder.CreateReport();
            }

            return _BaselineReport;
        }

        /// <summary>
        /// Get a known Person who has been assigned a security role of Administrator.
        /// </summary>
        /// <param name="personService"></param>
        /// <returns></returns>
        private Person GetAdminPersonOrThrow( PersonService personService )
        {
            var adminPerson = personService.Queryable().FirstOrDefault( x => x.FirstName == "Alisha" && x.LastName == "Marble" );

            if ( adminPerson == null )
            {
                throw new Exception( "Admin Person not found in test data set." );
            }

            return adminPerson;
        }

        /// <summary>
        /// Returns a Connection Status DefinedValueId by name or throws an exception.
        /// </summary>
        /// <param name="statusName"></param>
        /// <returns></returns>
        private int GetStatusValueIdOrThrow( string statusName )
        {
            var statusType = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSON_CONNECTION_STATUS );

            var connectionStatusType = statusType.DefinedValues.FirstOrDefault( x => x.Value == statusName );

            Assert.IsNotNull( connectionStatusType, $"Connection Status Type not found. [TypeName={statusName}]" );

            return connectionStatusType.Id;
        }

        #endregion
    }
}
