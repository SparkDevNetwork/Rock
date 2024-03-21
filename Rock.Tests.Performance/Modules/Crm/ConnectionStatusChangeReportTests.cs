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

namespace Rock.Tests.Performance.Crm
{
    /// <summary>
    /// Create and manage test data for the Rock CRM module.
    /// </summary>
    [TestClass]
    public class ConnectionStatusChangeReportTests : DatabaseTestsBase
    {
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
                var dataContext = GetDataContext();

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

        /// <summary>
        /// Get a new database context.
        /// </summary>
        /// <returns></returns>
        protected RockContext GetDataContext()
        {
            return new RockContext();
        }

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
