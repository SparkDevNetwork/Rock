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
using System.Diagnostics;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Crm.ConnectionStatusChangeReport;
using Rock.Data;
using Rock.Tests.Integration.TestData;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Crm
{
    /// <summary>
    /// Create and manage test data for the Rock CRM module.
    /// </summary>
    [TestClass]
    public class ConnectionStatusChangeReportTests : DatabaseTestsBase
    {
        private readonly int _InvalidCampusId = 99;
        private readonly int _MainCampusId = 1;

        #region Initialization

        /// <summary>
        /// Runs before any tests in this class are executed.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize( TestContext testContext )
        {
            HistoryDataFactory.AddSampleData();
        }

        /// <summary>
        /// Runs after all tests in this class is executed.
        /// </summary>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            HistoryDataFactory.RemoveSampleData();
        }

        #endregion

        #region Tests

        /// <summary>
        /// Verify the report can be correctly filtered by Campus.
        /// </summary>
        [TestMethod]
        [TestCategory( "Rock.Crm.ConnectionStatusChangeReport.Tests" )]
        public void FilterByCampus_MatchesExist_ShouldReturnPeopleInMatchedCampusOnly()
        {
            var dataContext = new RockContext();

            // Get an unfiltered report and verify that it contains records for Campus "Main".
            // This establishes the baseline for the test.
            var baselineReport = GetBaselineReport( dataContext );

            Assert.That.IsTrue( baselineReport.ChangeEvents.Any( x => x.CampusId == _MainCampusId ), "History events expected but not found. [Campus=(unfiltered)" );

            // The standard test data set does not currently have data for people in multiple campuses.
            // As an alternative, create a filtered report for a non-existent Campus and verify that no records are returned.
            var settings = new ConnectionStatusChangeReportSettings();

            settings.CampusId = _InvalidCampusId;

            var reportService = new ConnectionStatusChangeReportBuilder( dataContext, settings );

            var reportFiltered = reportService.CreateReport();

            Assert.That.IsFalse( reportFiltered.ChangeEvents.Any( x => x.CampusId == _MainCampusId ), "History events found but not expected. [CampusId=999]" );
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
        public void FilterByCurrentYear_MatchesExist_ShouldReturnChangesInCurrentYearOnly()
        {
            var dataContext = new RockContext();
            var baselineReport = GetBaselineReport( dataContext );

            // Get an unfiltered report and verify that it contains records for this year and previous years.
            // This establishes the baseline for the test.
            var currentYear = RockDateTime.Now.Year;

            Assert.That.IsTrue( baselineReport.ChangeEvents.Any( x => x.EventDate.Year == currentYear ), "History events expected but not found. [EventDate=(current year)" );
            Assert.That.IsTrue( baselineReport.ChangeEvents.Any( x => x.EventDate.Year == ( currentYear - 1 ) ), "History events expected but not found. [EventDate=(previous year)" );

            // Create a filtered report for current year only.
            var settings = new ConnectionStatusChangeReportSettings();

            settings.ReportPeriod.SetToCurrentPeriod( TimePeriodUnitSpecifier.Year );

            var reportService = new ConnectionStatusChangeReportBuilder( dataContext, settings );

            var reportFiltered = reportService.CreateReport();

            Assert.That.IsFalse( reportFiltered.ChangeEvents.Any( x => x.EventDate.Year != currentYear ), "History events found but not expected. [EventDate != (current year)]" );
        }

        /// <summary>
        /// Verify the correct result when a Status Filter is set for both Original Value and Updated Value.
        /// </summary>
        [TestMethod]
        [TestCategory( "Rock.Crm.ConnectionStatusChangeReport.Tests" )]
        public void FilterByStatus_OriginalAndUpdatedStatusSpecified_ShouldReturnMatchingStatusesOnly()
        {
            var dataContext = new RockContext();

            var settings = new ConnectionStatusChangeReportSettings();

            ConnectionStatusChangeReportBuilder reportBuilder;
            ConnectionStatusChangeReportData report;

            var memberConnectionStatusId = GetStatusValueIdOrThrow( "Member" );
            var attendeeConnectionStatusId = GetStatusValueIdOrThrow( "Attendee" );
            var visitorConnectionStatusId = GetStatusValueIdOrThrow( "Visitor" );
            var prospectConnectionStatusId = GetStatusValueIdOrThrow( "Prospect" );

            // Get an unfiltered report.
            // The unfiltered data should contain Original Status=[Attendee|Prospect].
            var baselineReport = GetBaselineReport( dataContext );

            Assert.That.IsTrue( baselineReport.ChangeEvents.Any( x => x.OldConnectionStatusId == attendeeConnectionStatusId ), "Status expected but not found. [Status=Attendee]" );
            Assert.That.IsTrue( baselineReport.ChangeEvents.Any( x => x.OldConnectionStatusId == prospectConnectionStatusId ), "Status expected but not found. [Status=Prospect]" );

            // Get a filtered report: Original Status=Attendee, UpdatedStatus=Member.
            // The filtered data should only contain Original Status=Attendee.
            settings.FromConnectionStatusId = attendeeConnectionStatusId;
            settings.ToConnectionStatusId = memberConnectionStatusId;

            reportBuilder = new ConnectionStatusChangeReportBuilder( dataContext, settings );

            report = reportBuilder.CreateReport();

            Assert.That.IsTrue( !report.ChangeEvents.Any( x => x.OldConnectionStatusId != attendeeConnectionStatusId ), "Status found but not expected." );
        }

        /// <summary>
        /// Verify the correct result when a Status Filter is set for Updated Value only.
        /// </summary>
        [TestMethod]
        [TestCategory( "Rock.Crm.ConnectionStatusChangeReport.Tests" )]
        public void FilterByStatus_OriginalStatusIsUnspecified_ShouldReturnAllStatuses()
        {
            var dataContext = new RockContext();

            var settings = new ConnectionStatusChangeReportSettings();

            ConnectionStatusChangeReportBuilder reportBuilder;
            ConnectionStatusChangeReportData report;

            var attendeeStatusId = GetStatusValueIdOrThrow( "Attendee" );
            var memberStatusId = GetStatusValueIdOrThrow( "Member" );

            // Get an unfiltered report.
            // The unfiltered data should contain at least one record that is a transition from Visitor.
            var baselineReport = GetBaselineReport( dataContext );

            Assert.That.IsTrue( baselineReport.ChangeEvents.Any( x => x.OldConnectionStatusId == attendeeStatusId ), "Status expected but not found. [Status=Attendee]" );

            // Get a filtered report: Original Status=Attendee, UpdatedStatus=Member.
            reportBuilder = new ConnectionStatusChangeReportBuilder( dataContext, settings );

            settings.ToConnectionStatusId = GetStatusValueIdOrThrow( "Member" );

            report = reportBuilder.CreateReport();

            // The report should include events for new people, represented by a change from (null) --> (some status).
            Assert.That.IsTrue( report.ChangeEvents.Any( x => string.IsNullOrEmpty( x.OldConnectionStatusName ) ), "Status expected but not found. [OldStatus=(empty)]" );
        }

        /// <summary>
        /// Verify the correct result when a Status Filter is set for Original Value only.
        /// </summary>
        [TestMethod]
        [TestCategory( "Rock.Crm.ConnectionStatusChangeReport.Tests" )]
        public void FilterByStatus_UpdatedStatusUnspecified_ShouldReturnAllStatuses()
        {
            var dataContext = new RockContext();

            var settings = new ConnectionStatusChangeReportSettings();

            ConnectionStatusChangeReportBuilder reportBuilder;
            ConnectionStatusChangeReportData report;

            var attendeeStatusId = GetStatusValueIdOrThrow( "Attendee" );
            var memberStatusId = GetStatusValueIdOrThrow( "Member" );

            // Get an unfiltered report.
            // The unfiltered data should contain at least one record that is a transition from Visitor.
            var baselineReport = GetBaselineReport( dataContext );

            Assert.That.IsTrue( baselineReport.ChangeEvents.Any( x => x.OldConnectionStatusId == attendeeStatusId ), "Status expected but not found. [Status=Attendee]" );

            // Get a filtered report: Original Status=Attendee, UpdatedStatus=Member.
            settings.ToConnectionStatusId = GetStatusValueIdOrThrow( "Member" );

            reportBuilder = new ConnectionStatusChangeReportBuilder( dataContext, settings );

            report = reportBuilder.CreateReport();

            // The report should include events for new people, represented by a change from (null) --> (some status).
            Assert.That.IsTrue( report.ChangeEvents.Any( x => string.IsNullOrEmpty( x.OldConnectionStatusName ) ), "Status expected but not found. [OldStatus=(empty)]" );
        }

        /// <summary>
        /// Verify the correct result when a Status Filter is set for Original Value only.
        /// </summary>
        [TestMethod]
        [TestProperty( "Purpose", TestPurposes.Performance )]
        [TestCategory( "Rock.Crm.ConnectionStatusChangeReport.Tests" )]
        public void Performance_LargeHistoryDataSet_ShouldNotTimeout()
        {
            int monthsToInclude = 2;

            var periodStart = new DateTime( RockDateTime.Now.Year, RockDateTime.Now.Month, 1 );

            // Run a series of monthly reports throughout the year to test performance for various time periods.
            for ( int i = 1; i <= 12; i++ )
            {
                var dataContext = new RockContext();

                periodStart = periodStart.AddMonths( monthsToInclude * -1 );

                var nextPeriodEnd = periodStart.AddMonths( monthsToInclude ).AddDays( -1 );

                var settings = new ConnectionStatusChangeReportSettings();

                settings.ReportPeriod.SetToSpecificDateRange( periodStart, nextPeriodEnd );

                ConnectionStatusChangeReportBuilder reportBuilder;
                ConnectionStatusChangeReportData report;

                // Get an unfiltered report.
                // The unfiltered data should contain at least one record that is a transition from Visitor.
                reportBuilder = new ConnectionStatusChangeReportBuilder( dataContext, settings );

                var watch = new Stopwatch();

                watch.Start();

                report = reportBuilder.CreateReport();

                watch.Stop();

                Debug.Print( $"Pass {i:00}: Period={report.StartDate:dd-MMM-yy} - {report.EndDate:dd-MMM-yy}, Events={report.ChangeEvents.Count}, Execution Time={watch.Elapsed.TotalSeconds}s" );
            }
        }

        #endregion

        #region Support Methods

        private static ConnectionStatusChangeReportData _BaselineReport;

        /// <summary>
        /// Returns a report that contains all of the available test data for comparison results.
        /// </summary>
        /// <returns></returns>
        private static ConnectionStatusChangeReportData GetBaselineReport( RockContext dataContext )
        {
            if ( _BaselineReport == null )
            {
                var settings = new ConnectionStatusChangeReportSettings();

                ConnectionStatusChangeReportBuilder reportBuilder;

                // Get an unfiltered report.
                reportBuilder = new ConnectionStatusChangeReportBuilder( dataContext, settings );

                _BaselineReport = reportBuilder.CreateReport();
            }

            return _BaselineReport;
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

            Assert.That.IsNotNull( connectionStatusType, $"Connection Status Type not found. [TypeName={statusName}]" );

            return connectionStatusType.Id;
        }

        #endregion
    }
}
