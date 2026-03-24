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
using System.Linq;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Model;
using Rock.Tests.Shared;

namespace Rock.Tests.Model
{
    [TestClass]
    public class ScheduleServiceTests
    {
        [TestMethod]
        public void UpdateScheduleDates_ShouldNotCreateScheduleDates_WhenScheduleIsNotActive()
        {
            using ( var scope = TestHelper.CreateScopedRockAppWithMockDatabase() )
            {
                var rockContext = scope.App.CreateRockContext();

                rockContext.Set<Schedule>().Add( new Schedule
                {
                    Id = 1,
                    Name = "Test Schedule",
                    IsActive = false,
                    iCalendarContent = @"BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T235900
DTSTART:20130501T000100
RRULE:FREQ=DAILY
END:VEVENT
END:VCALENDAR",
                } );

                var startDate = RockDateTime.New( 2022, 1, 1 ).Value;
                var endDate = RockDateTime.New( 2022, 2, 1 ).Value.AddMilliseconds( -1 );

                ScheduleService.UpdateScheduleDates( startDate, endDate, false, CancellationToken.None );

                Assert.AreEqual( 0, rockContext.Set<ScheduleDate>().Count() );
            }
        }

        [TestMethod]
        public void UpdateScheduleDates_ShoulCreateScheduleDates_WhenScheduleIsActive()
        {
            using ( var scope = TestHelper.CreateScopedRockAppWithMockDatabase() )
            {
                var rockContext = scope.App.CreateRockContext();

                rockContext.Set<Schedule>().Add( new Schedule
                {
                    Id = 1,
                    Name = "Test Schedule",
                    IsActive = true,
                    iCalendarContent = @"BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T235900
DTSTART:20130501T000100
RRULE:FREQ=DAILY
END:VEVENT
END:VCALENDAR",
                } );

                var startDate = RockDateTime.New( 2022, 1, 1 ).Value;
                var endDate = RockDateTime.New( 2022, 2, 1 ).Value.AddMilliseconds( -1 );

                ScheduleService.UpdateScheduleDates( startDate, endDate, false, CancellationToken.None );

                Assert.AreEqual( 31, rockContext.Set<ScheduleDate>().Count() );
            }
        }

        [TestMethod]
        public void UpdateScheduleDates_ShouldNotCreateScheduleDates_ThatAlreadyExist()
        {
            using ( var scope = TestHelper.CreateScopedRockAppWithMockDatabase() )
            {
                var rockContext = scope.App.CreateRockContext();

                rockContext.Set<Schedule>().Add( new Schedule
                {
                    Id = 1,
                    Name = "Test Schedule",
                    IsActive = true,
                    iCalendarContent = @"BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T235900
DTSTART:20130501T000100
RRULE:FREQ=DAILY
END:VEVENT
END:VCALENDAR",
                } );

                rockContext.Set<ScheduleDate>().Add( new ScheduleDate
                {
                    ScheduleId = 1,
                    StartDateTime = RockDateTime.New( 2022, 1, 1, 0, 1, 0, 0 ).Value,
                    EndDateTime = RockDateTime.New( 2022, 1, 1, 23, 59, 0, 0 ).Value,
                } );

                var startDate = RockDateTime.New( 2022, 1, 1 ).Value;
                var endDate = RockDateTime.New( 2022, 2, 1 ).Value.AddMilliseconds( -1 );

                var count = ScheduleService.UpdateScheduleDates( startDate, endDate, false, CancellationToken.None );

                Assert.AreEqual( 30, count );
                Assert.AreEqual( 31, rockContext.Set<ScheduleDate>().Count() );
            }
        }

        [TestMethod]
        public void UpdateScheduleDates_ShouldRemoveScheduleDates_ThatAreNoLongerValid()
        {
            using ( var scope = TestHelper.CreateScopedRockAppWithMockDatabase() )
            {
                var rockContext = scope.App.CreateRockContext();

                rockContext.Set<Schedule>().Add( new Schedule
                {
                    Id = 1,
                    Name = "Test Schedule",
                    IsActive = true,
                    iCalendarContent = @"BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T235900
DTSTART:20130501T000100
RRULE:FREQ=DAILY
END:VEVENT
END:VCALENDAR",
                } );

                var existingDate = RockDateTime.New( 2021, 12, 31, 0, 1, 0, 0 ).Value;

                rockContext.Set<ScheduleDate>().Add( new ScheduleDate
                {
                    ScheduleId = 1,
                    StartDateTime = existingDate,
                } );

                var startDate = RockDateTime.New( 2022, 1, 1 ).Value;
                var endDate = RockDateTime.New( 2022, 2, 1 ).Value.AddMilliseconds( -1 );

                var count = ScheduleService.UpdateScheduleDates( startDate, endDate, false, CancellationToken.None );

                Assert.AreEqual( 32, count );
                Assert.AreEqual( 31, rockContext.Set<ScheduleDate>().Count() );
                Assert.IsFalse( rockContext.Set<ScheduleDate>().Any( d => d.StartDateTime == existingDate ) );
            }
        }

        [TestMethod]
        public void UpdateScheduleDates_ShouldNotCreateScheduleDates_ThatAreExcluded()
        {
            using ( var scope = TestHelper.CreateScopedRockAppWithMockDatabase() )
            {
                var rockContext = scope.App.CreateRockContext();

                rockContext.Set<Schedule>().Add( new Schedule
                {
                    Id = 1,
                    CategoryId = 1,
                    Name = "Test Schedule",
                    IsActive = true,
                    iCalendarContent = @"BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T235900
DTSTART:20130501T000100
RRULE:FREQ=DAILY
END:VEVENT
END:VCALENDAR",
                } );

                rockContext.Set<Category>().Add( new Category
                {
                    Id = 1,
                    Name = "Exclude Category",
                } );

                // Exclude Jan 10 - Jan 19
                rockContext.Set<ScheduleCategoryExclusion>().Add( new ScheduleCategoryExclusion
                {
                    CategoryId = 1,
                    StartDate = RockDateTime.New( 2022, 1, 10 ).Value,
                    EndDate = RockDateTime.New( 2022, 1, 20 ).Value.AddMilliseconds( -1 ),
                } );

                var startDate = RockDateTime.New( 2022, 1, 1 ).Value;
                var endDate = RockDateTime.New( 2022, 2, 1 ).Value.AddMilliseconds( -1 );

                var count = ScheduleService.UpdateScheduleDates( startDate, endDate, false, CancellationToken.None );

                Assert.AreEqual( 21, count );
                Assert.AreEqual( 21, rockContext.Set<ScheduleDate>().Count() );
            }
        }

        [TestMethod]
        public void UpdateScheduleDates_ShouldReturnZero_WhenNoSchedulesExist()
        {
            using ( var scope = TestHelper.CreateScopedRockAppWithMockDatabase() )
            {
                var rockContext = scope.App.CreateRockContext();
                var startDate = RockDateTime.New( 2022, 1, 1 ).Value;
                var endDate = RockDateTime.New( 2022, 2, 1 ).Value.AddMilliseconds( -1 );

                var count = ScheduleService.UpdateScheduleDates( startDate, endDate, false, CancellationToken.None );

                Assert.AreEqual( 0, count );
                Assert.AreEqual( 0, rockContext.Set<ScheduleDate>().Count() );
            }
        }

        [TestMethod]
        public void UpdateScheduleDates_ShouldReturnZero_WhenNoDateChangesNeeded()
        {
            using ( var scope = TestHelper.CreateScopedRockAppWithMockDatabase() )
            {
                var rockContext = scope.App.CreateRockContext();

                rockContext.Set<Schedule>().Add( new Schedule
                {
                    Id = 1,
                    Name = "Test Schedule",
                    IsActive = true,
                    iCalendarContent = @"BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T235900
DTSTART:20130501T000100
RRULE:FREQ=DAILY
END:VEVENT
END:VCALENDAR",
                } );

                // Pre-populate all expected dates
                var startDate = RockDateTime.New( 2022, 1, 1 ).Value;
                var endDate = RockDateTime.New( 2022, 2, 1 ).Value.AddMilliseconds( -1 );
                for ( int i = 0; i < 31; i++ )
                {
                    var date = startDate.AddDays( i ).AddMinutes( 1 );
                    rockContext.Set<ScheduleDate>().Add( new ScheduleDate
                    {
                        ScheduleId = 1,
                        StartDateTime = date,
                        EndDateTime = date.AddMinutes( 1438 ), // 23:59 - 00:01
                    } );
                }

                var count = ScheduleService.UpdateScheduleDates( startDate, endDate, false, CancellationToken.None );

                Assert.AreEqual( 0, count );
                Assert.AreEqual( 31, rockContext.Set<ScheduleDate>().Count() );
            }
        }

        [TestMethod]
        public void UpdateScheduleDates_ShouldNotThrow_WhenScheduleHasNoICalendarContent()
        {
            using ( var scope = TestHelper.CreateScopedRockAppWithMockDatabase() )
            {
                var rockContext = scope.App.CreateRockContext();

                rockContext.Set<Schedule>().Add( new Schedule
                {
                    Id = 1,
                    Name = "No iCal",
                    IsActive = true,
                    iCalendarContent = string.Empty
                } );

                var startDate = RockDateTime.New( 2022, 1, 1 ).Value;
                var endDate = RockDateTime.New( 2022, 2, 1 ).Value.AddMilliseconds( -1 );

                var count = ScheduleService.UpdateScheduleDates( startDate, endDate, false, CancellationToken.None );

                Assert.AreEqual( 0, count );
                Assert.AreEqual( 0, rockContext.Set<ScheduleDate>().Count() );
            }
        }

        [TestMethod]
        public void UpdateScheduleDates_ShouldSetEndDateTime_WhenScheduleHasZeroDuration()
        {
            using ( var scope = TestHelper.CreateScopedRockAppWithMockDatabase() )
            {
                var rockContext = scope.App.CreateRockContext();
                var schedule = new Schedule
                {
                    Id = 1,
                    Name = "Zero Duration",
                    IsActive = true,
                    iCalendarContent = @"BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T000100
DTSTART:20130501T000100
RRULE:FREQ=DAILY
END:VEVENT
END:VCALENDAR",
                };

                rockContext.Set<Schedule>().Add( schedule );

                // Make sure the test setup is correct and the schedule has
                // zero duration.
                Assert.AreEqual( 0, schedule.DurationInMinutes );

                var startDate = RockDateTime.New( 2022, 1, 1 ).Value;
                var endDate = RockDateTime.New( 2022, 1, 2 ).Value.AddMilliseconds( -1 );

                ScheduleService.UpdateScheduleDates( startDate, endDate, false, CancellationToken.None );

                var scheduleDate = rockContext.Set<ScheduleDate>().FirstOrDefault();
                Assert.IsNotNull( scheduleDate );
                Assert.AreEqual( scheduleDate.StartDateTime, scheduleDate.EndDateTime );
            }
        }

        [TestMethod]
        public void UpdateScheduleDates_ShouldRespectExclusion_ThatOverlapsStartOrEnd()
        {
            using ( var scope = TestHelper.CreateScopedRockAppWithMockDatabase() )
            {
                var rockContext = scope.App.CreateRockContext();

                rockContext.Set<Schedule>().Add( new Schedule
                {
                    Id = 1,
                    CategoryId = 1,
                    Name = "Test Schedule",
                    IsActive = true,
                    iCalendarContent = @"BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T235900
DTSTART:20130501T000100
RRULE:FREQ=DAILY
END:VEVENT
END:VCALENDAR",
                } );

                rockContext.Set<Category>().Add( new Category
                {
                    Id = 1,
                    Name = "Exclude Category",
                } );

                // Exclude Jan 1 - Jan 5 (overlaps start)
                rockContext.Set<ScheduleCategoryExclusion>().Add( new ScheduleCategoryExclusion
                {
                    CategoryId = 1,
                    StartDate = RockDateTime.New( 2022, 1, 1 ).Value,
                    EndDate = RockDateTime.New( 2022, 1, 6 ).Value.AddMilliseconds( -1 ),
                } );

                var startDate = RockDateTime.New( 2022, 1, 1 ).Value;
                var endDate = RockDateTime.New( 2022, 1, 11 ).Value.AddMilliseconds( -1 );

                var count = ScheduleService.UpdateScheduleDates( startDate, endDate, false, CancellationToken.None );

                // Only Jan 6-10 should be included (5 days)
                Assert.AreEqual( 5, count );
                Assert.AreEqual( 5, rockContext.Set<ScheduleDate>().Count() );
                Assert.IsFalse( rockContext.Set<ScheduleDate>().Any( d => d.StartDateTime < RockDateTime.New( 2022, 1, 6 ).Value ) );
            }
        }

        [TestMethod]
        public void UpdateScheduleDates_ShouldThrowOperationCanceledException_WhenCancellationRequested()
        {
            using ( var scope = TestHelper.CreateScopedRockAppWithMockDatabase() )
            {
                var rockContext = scope.App.CreateRockContext();

                rockContext.Set<Schedule>().Add( new Schedule
                {
                    Id = 1,
                    Name = "Test Schedule",
                    IsActive = true,
                    iCalendarContent = @"BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T235900
DTSTART:20130501T000100
RRULE:FREQ=DAILY
END:VEVENT
END:VCALENDAR",
                } );

                var startDate = RockDateTime.New( 2022, 1, 1 ).Value;
                var endDate = RockDateTime.New( 2022, 2, 1 ).Value.AddMilliseconds( -1 );

                var cts = new CancellationTokenSource();
                cts.Cancel();

                Assert.ThrowsExactly<System.OperationCanceledException>( () =>
                {
                    ScheduleService.UpdateScheduleDates( startDate, endDate, false, cts.Token );
                } );
            }
        }

        [TestMethod]
        public void UpdateScheduleDates_ShouldUpdateEndDateTime_WhenDurationChanges()
        {
            using ( var scope = TestHelper.CreateScopedRockAppWithMockDatabase() )
            {
                var rockContext = scope.App.CreateRockContext();

                // Initial schedule with 60-minute duration
                rockContext.Set<Schedule>().Add( new Schedule
                {
                    Id = 1,
                    Name = "Test Schedule",
                    IsActive = true,
                    iCalendarContent = @"BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T020100
DTSTART:20130501T000100
RRULE:FREQ=DAILY
END:VEVENT
END:VCALENDAR",
                } );

                var startDate = RockDateTime.New( 2022, 1, 1 ).Value;
                var endDate = RockDateTime.New( 2022, 1, 2 ).Value.AddMilliseconds( -1 );

                // Add an existing ScheduleDate with the old duration (60 minutes)
                var existingStart = startDate.AddMinutes( 1 );
                var existingEnd = existingStart.AddMinutes( 60 );
                rockContext.Set<ScheduleDate>().Add( new ScheduleDate
                {
                    ScheduleId = 1,
                    StartDateTime = existingStart,
                    EndDateTime = existingEnd
                } );

                var count = ScheduleService.UpdateScheduleDates( startDate, endDate, false, CancellationToken.None );
                var updated = rockContext.Set<ScheduleDate>().Single();

                Assert.AreEqual( 2, count );
                Assert.AreEqual( existingStart.AddMinutes( 120 ), updated.EndDateTime, "EndDateTime should be updated to reflect the new duration." );
            }
        }
    }
}
