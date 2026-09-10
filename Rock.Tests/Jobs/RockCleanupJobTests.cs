using System;
using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock;
using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Jobs
{
    [TestClass]
    public class RockCleanupJobTests
    {
        #region Cleanup Task: ClearCache

        [TestMethod]
        public void RockCleanup_ClearCache_RemovesExpiredFilesOnly()
        {
            // Create some test files in the cache.
            var avatarCachePath = Path.GetTempPath() + "App_Data/Image/Cache";
            var imageCachePath = Path.GetTempPath() + "App_Data/Avatar/Cache";

            CreateCacheFilesTestData( avatarCachePath, imageCachePath );

            var job = new Rock.Jobs.RockCleanup();
            var args = new Rock.Jobs.RockCleanup.RockCleanupActionArgs
            {
                AvatarCachePath = avatarCachePath,
                ImageCachePath = imageCachePath,
                CacheDurationDays = 6,
                HostName = "test-host",
                IsUnitTest = true
            };

            _ = job.CleanCachedFileDirectories( args );

            AssertExpectedCacheFiles( avatarCachePath );
            AssertExpectedCacheFiles( imageCachePath );
        }

        [TestMethod]
        public void RockCleanup_ClearCache_EnforcesFileRemoveLimit()
        {
            // Create some test files in the cache.
            var avatarCachePath = Path.GetTempPath() + "App_Data/Image/Cache";
            var imageCachePath = Path.GetTempPath() + "App_Data/Avatar/Cache";

            CreateCacheFilesTestData( avatarCachePath, imageCachePath );

            var job = new Rock.Jobs.RockCleanup();
            var args = new Rock.Jobs.RockCleanup.RockCleanupActionArgs
            {
                AvatarCachePath = avatarCachePath,
                ImageCachePath = imageCachePath,
                CacheDurationDays = 6,
                HostName = "test-host",
                CacheMaximumFilesToRemove = 1,
                IsUnitTest = true
            };

            var avatarFileCount = Directory.GetFiles( avatarCachePath, searchPattern: "*", searchOption: SearchOption.AllDirectories ).Count();
            var imageFileCount = Directory.GetFiles( imageCachePath, searchPattern: "*", searchOption: SearchOption.AllDirectories ).Count();

            _ = job.CleanCachedFileDirectories( args );

            var newAvatarFileCount = Directory.GetFiles( avatarCachePath, searchPattern: "*", searchOption: SearchOption.AllDirectories ).Count();
            var newImageFileCount = Directory.GetFiles( imageCachePath, searchPattern: "*", searchOption: SearchOption.AllDirectories ).Count();

            Assert.AreEqual( avatarFileCount - 1, newAvatarFileCount, "Invalid avatar cache file count." );
            Assert.AreEqual( imageFileCount - 1, newImageFileCount, "Invalid image cache file count." );
        }

        [TestMethod]
        public void RockCleanup_ClearCache_TargetDirectoryMustIncludeAppData()
        {
            // Create some test files in the cache.
            var avatarCachePath = Path.GetTempPath() + "/RockAvatarCacheTest";
            var imageCachePath = Path.GetTempPath() + "/RockImageCacheTest";

            CreateCacheFilesTestData( avatarCachePath, imageCachePath );

            var job = new Rock.Jobs.RockCleanup();
            var args = new Rock.Jobs.RockCleanup.RockCleanupActionArgs
            {
                AvatarCachePath = avatarCachePath,
                ImageCachePath = imageCachePath,
                CacheDurationDays = 6,
                HostName = "test-host",
                IsUnitTest = true
            };

            try
            {
                _ = job.CleanCachedFileDirectories( args );
            }
            catch ( Exception ex )
            {
                // Verify the exception message.
                // This exception is processed internally when the Job is executed via the Rock application.
                Assert.That.MatchesWildcard( $@"%Path ""%/RockAvatarCacheTest"" does not match the required pattern ""*\App_Data\*\Cache\*""%",
                    ex.Message,
                    ignoreCase: true,
                    ignoreWhiteSpace: true,
                    wildcard: "%" );
            }
        }

        private void CreateCacheFilesTestData( string avatarCachePath, string imageCachePath )
        {
            // Create some test files in the cache.
            var baseDate = RockDateTime.Now;

            CreateTestFile( avatarCachePath + $"/avatar_current_1.txt", lastModifiedTime: baseDate );
            CreateTestFile( avatarCachePath + $"/avatar_current_2.txt", lastModifiedTime: baseDate.AddDays( -5 ) );
            CreateTestFile( avatarCachePath + $"/avatar_old.txt", lastModifiedTime: baseDate.AddDays( -7 ) );
            CreateTestFile( avatarCachePath + $"/avatar_future.txt", lastModifiedTime: baseDate.AddDays( 1 ) );
            CreateTestFile( avatarCachePath + $"/subdir1/avatar_old.txt", lastModifiedTime: baseDate.AddDays( -7 ) );
            CreateTestFile( avatarCachePath + $"/subdir2/avatar_old.txt", lastModifiedTime: baseDate.AddDays( -7 ) );

            CreateTestFile( imageCachePath + $"/image_current_1.txt", createdTime: baseDate );
            CreateTestFile( imageCachePath + $"/image_current_2.txt", createdTime: baseDate.AddDays( -5 ) );
            CreateTestFile( imageCachePath + $"/image_old.txt", createdTime: baseDate.AddDays( -7 ) );
            CreateTestFile( imageCachePath + $"/image_future.txt", createdTime: baseDate.AddDays( 1 ) );
            CreateTestFile( imageCachePath + $"/subdir1/image_old.txt", createdTime: baseDate.AddDays( -7 ) );
            CreateTestFile( imageCachePath + $"/subdir2/image_old.txt", createdTime: baseDate.AddDays( -7 ) );
        }

        private void AssertExpectedCacheFiles( string cacheDirectory )
        {
            var remainingFiles = Directory.EnumerateFiles( cacheDirectory, searchPattern: "*", searchOption: SearchOption.AllDirectories ).ToList();

            var oldFiles = remainingFiles.Where( f => f.Contains( "_old" ) ).ToList();
            Assert.IsEmpty( oldFiles, "Unexpected files found. Old files not removed from cache." );

            var currentFiles = remainingFiles.Where( f => f.Contains( "_current" ) ).ToList();
            Assert.HasCount( 2, currentFiles, "Expected files not found. Current files removed from cache." );

            var futureFiles = remainingFiles.Where( f => f.Contains( "_future" ) ).ToList();
            Assert.HasCount( 1, futureFiles, "Expected files not found. Future files removed from cache." );
        }

        private FileInfo CreateTestFile( string filePath, DateTime? createdTime = null, DateTime? lastModifiedTime = null )
        {
            var directory = Path.GetDirectoryName( filePath );
            Directory.CreateDirectory( directory );

            var fileInfo = new FileInfo( filePath );

            using ( var sw = fileInfo.CreateText() )
            {
                sw.WriteLine( Guid.NewGuid().ToString() );
            }

            if ( createdTime != null )
            {
                fileInfo.CreationTime = createdTime.Value;
            }

            if ( lastModifiedTime != null )
            {
                fileInfo.LastWriteTime = lastModifiedTime.Value;
            }

            return fileInfo;
        }

        #endregion

        #region Cleanup Task: Update EventItemOccurrence.NextDateTime

        private static readonly Guid _testEvent1Guid = new Guid( "1DC19F1B-8FD1-41ED-80AE-6F112AEDBE8A" );
        private static readonly Guid _testEventOccurrence11Guid = new Guid( "A7FD20AD-0349-4125-8ABF-04437CEA31C0" );
        private static readonly Guid _testEventOccurrence12Guid = new Guid( "D9958C1F-F485-4147-87C9-A0523216A2B6" );
        private static readonly Guid _testEventOccurrence13Guid = new Guid( "37CA5E63-9464-4E44-9F9B-378D22DB8300" );
        private static readonly Guid _inactiveScheduleGuid = new Guid( "8FF2529A-778F-4190-A7D8-6C0506D43D84" );
        private static readonly Guid _testEvent2Guid = new Guid( "75906B33-84F3-45DD-B79E-B31B1523E573" );
        private static readonly Guid _testEventOccurrence21Guid = new Guid( "96453FB1-1F8C-4E47-BFC1-C7B9DEC94446" );
        private static readonly Guid _activeScheduleGuid = new Guid( "7883CAC8-6E30-482B-95A7-2F0DEE859BE1" );

        // A perpetual "every Saturday at 4:30pm" schedule so GetNextStartDateTime always returns a future occurrence.
        private const string _weeklySaturdayICalendarContent = @"BEGIN:VCALENDAR
PRODID:-//github.com/SparkDevNetwork/Rock//NONSGML Rock//EN
VERSION:2.0
BEGIN:VEVENT
DTEND:20130504T170000
DTSTAMP:20200101T000000
DTSTART:20130504T163000
RRULE:FREQ=WEEKLY;BYDAY=SA
SEQUENCE:0
UID:2d4f1b9c-8b1e-4d2a-9c1a-7f0b6e5d4c3b
END:VEVENT
END:VCALENDAR";

        [TestMethod]
        public void RockCleanup_Execute_ShouldUpdateEventItemOccurrences()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var referenceDate = new DateTime( 2020, 1, 1 );

            SeedEventOccurrenceData( app.App.CreateRockContext(), referenceDate );

            // Run the cleanup task to verify the results for the reference date.
            RunRockCleanupTaskUpdateEventNextOccurrenceDatesAndVerify( app, referenceDate );

            // Re-run the task to verify that the results are adjusted for the current date.
            RunRockCleanupTaskUpdateEventNextOccurrenceDatesAndVerify( app, RockDateTime.Now );
        }

        /// <summary>
        /// Seeds two events (one active, one inactive), an active and an inactive schedule, and
        /// four occurrences covering the update/reset cases. Navigation properties are wired
        /// explicitly and each occurrence is added to its own DbSet because the mocked context
        /// performs no FK/navigation fixup or child-collection cascade.
        /// </summary>
        private static void SeedEventOccurrenceData( RockContext rockContext, DateTime referenceDate )
        {
            var activeSchedule = new Schedule
            {
                Id = 1,
                Guid = _activeScheduleGuid,
                Name = "Saturday 4:30pm",
                IsActive = true,
                iCalendarContent = _weeklySaturdayICalendarContent
            };
            var inactiveSchedule = new Schedule
            {
                Id = 2,
                Guid = _inactiveScheduleGuid,
                Name = "Test Schedule",
                IsActive = false
            };
            rockContext.Set<Schedule>().Add( activeSchedule );
            rockContext.Set<Schedule>().Add( inactiveSchedule );

            var activeEvent = new EventItem { Id = 1, Guid = _testEvent1Guid, Name = "Test Event 1", IsActive = true };
            var inactiveEvent = new EventItem { Id = 2, Guid = _testEvent2Guid, Name = "Test Event 2", IsActive = false };
            rockContext.Set<EventItem>().Add( activeEvent );
            rockContext.Set<EventItem>().Add( inactiveEvent );

            var occurrences = rockContext.Set<EventItemOccurrence>();

            // Active event + active schedule, no NextStartDateTime yet -> should be set to the next occurrence.
            occurrences.Add( new EventItemOccurrence
            {
                Id = 1,
                Guid = _testEventOccurrence11Guid,
                EventItemId = activeEvent.Id,
                EventItem = activeEvent,
                ScheduleId = activeSchedule.Id,
                Schedule = activeSchedule,
                NextStartDateTime = null
            } );

            // Active event + active schedule, stale NextStartDateTime -> should be recalculated.
            occurrences.Add( new EventItemOccurrence
            {
                Id = 2,
                Guid = _testEventOccurrence12Guid,
                EventItemId = activeEvent.Id,
                EventItem = activeEvent,
                ScheduleId = activeSchedule.Id,
                Schedule = activeSchedule,
                NextStartDateTime = referenceDate.AddDays( -1 )
            } );

            // Active event + inactive schedule -> NextStartDateTime should be reset to null.
            occurrences.Add( new EventItemOccurrence
            {
                Id = 3,
                Guid = _testEventOccurrence13Guid,
                EventItemId = activeEvent.Id,
                EventItem = activeEvent,
                ScheduleId = inactiveSchedule.Id,
                Schedule = inactiveSchedule,
                NextStartDateTime = referenceDate.AddDays( 7 )
            } );

            // Inactive event + active schedule -> NextStartDateTime should be reset to null.
            occurrences.Add( new EventItemOccurrence
            {
                Id = 4,
                Guid = _testEventOccurrence21Guid,
                EventItemId = inactiveEvent.Id,
                EventItem = inactiveEvent,
                ScheduleId = activeSchedule.Id,
                Schedule = activeSchedule,
                NextStartDateTime = referenceDate
            } );
        }

        private static void RunRockCleanupTaskUpdateEventNextOccurrenceDatesAndVerify( TestHelper.RockAppScope app, DateTime referenceDate )
        {
            // Execute the process to update the Event Occurrence next dates.
            Rock.Jobs.RockCleanup.UpdateEventNextOccurrenceDates( app.App.CreateRockContext(), referenceDate );

            // Verify the results of the cleanup.
            var eventOccurrenceService = new EventItemOccurrenceService( app.App.CreateRockContext() );

            // Event 1.1 should be updated to the next occurrence after the reference date.
            var event11 = eventOccurrenceService.Get( _testEventOccurrence11Guid );
            Assert.AreEqual( event11.NextStartDateTime, event11.Schedule.GetNextStartDateTime( referenceDate ) );

            // Event 1.2 should be updated to the next occurrence after the reference date.
            var event12 = eventOccurrenceService.Get( _testEventOccurrence12Guid );
            Assert.AreEqual( event12.NextStartDateTime, event12.Schedule.GetNextStartDateTime( referenceDate ) );

            // Event 1.3 should be set to null because the schedule is inactive.
            var event13 = eventOccurrenceService.Get( _testEventOccurrence13Guid );
            Assert.IsNull( event13.NextStartDateTime );

            // Event 2.1 should be set to null because the Event is inactive.
            var event21 = eventOccurrenceService.Get( _testEventOccurrence21Guid );
            Assert.IsNull( event21.NextStartDateTime );
        }

        #endregion
    }
}
