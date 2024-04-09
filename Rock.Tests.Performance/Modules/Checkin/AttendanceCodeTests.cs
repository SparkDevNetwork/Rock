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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Performance.Modules.Checkin
{
    /// <summary>
    /// Used for testing anything regarding AttendanceCode.
    /// </summary>
    [TestClass]
    public class AttendanceCodeTests : DatabaseTestsBase
    {
        #region Setup

        /// <summary>
        /// Runs after each test in this class is executed.
        /// Deletes the test data added to the database for each tests.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            using ( var rockContext = new RockContext() )
            {
                var acService = new AttendanceCodeService( rockContext );
                var attendanceService = new AttendanceService( rockContext );

                DateTime today = RockDateTime.Today;
                DateTime tomorrow = today.AddDays( 1 );
                var todaysCodes = acService.Queryable()
                        .Where( c => c.IssueDateTime >= today && c.IssueDateTime < tomorrow )
                        .ToList();
                if ( todaysCodes.Any() )
                {
                    var ids = todaysCodes.Select( c => c.Id ).ToList();

                    // get the corresponding attendance records and delete them first.
                    var todayTestAttendance = attendanceService.Queryable().Where( a => ids.Contains( a.AttendanceCodeId.Value ) );
                    if ( todayTestAttendance.Any() )
                    {
                        attendanceService.DeleteRange( todayTestAttendance );
                    }

                    acService.DeleteRange( todaysCodes );
                    rockContext.SaveChanges();
                }
            }

            AttendanceCodeService.FlushTodaysCodes();
        }

        #endregion

        #region Alpha-numeric codes

        /// <summary>
        /// Runs the test three times.
        /// </summary>
        [Ignore( "Sometimes with caching you have to throw out the first result." )]
        [TestMethod]
        public void RunTestThreeTimes()
        {
            for ( int i = 0; i < 3; i++ )
            {
                GenerateLotsOfCodes();
            }
        }

        /// <summary>
        /// Generates lots of codes to test performance.
        /// </summary>
        [TestMethod]
        public void GenerateLotsOfCodes()
        {
            int interations = 6000;
            int alphaNumbericDigits = 0;
            int alphaDigits = 0;
            int numericDigits = 4;
            bool isRandom = false;

            int lastAttendanceCodeId;

            using ( var rockContext = new RockContext() )
            {
                lastAttendanceCodeId = new AttendanceCodeService( rockContext )
                    .Queryable()
                    .Select( ac => ac.Id )
                    .Max();
            }

            try
            {
                var stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();

                for ( int i = 0; i < interations; i++ )
                {
                    AttendanceCodeService.GetNew( alphaNumbericDigits, alphaDigits, numericDigits, isRandom );
                }

                stopWatch.Stop();
                System.Diagnostics.Trace.Listeners.Add( new System.Diagnostics.TextWriterTraceListener( Console.Out ) );
                System.Diagnostics.Trace.WriteLine( $"New GetNew Method AlphaNumericDigits: {alphaNumbericDigits}, AlphaDigits: {alphaDigits}, NumericDigits: {numericDigits}, IsRandom: {isRandom}, Number of Codes Generated: {interations}. Total Time: {stopWatch.ElapsedMilliseconds} ms." );
            }
            finally
            {
                using ( var rockContext = new RockContext() )
                {
                    rockContext.Database.ExecuteSqlCommand( $"delete from AttendanceCode where id > {lastAttendanceCodeId}" );
                }

                AttendanceCodeService.FlushTodaysCodes();
            }
        }

        #endregion
    }
}
