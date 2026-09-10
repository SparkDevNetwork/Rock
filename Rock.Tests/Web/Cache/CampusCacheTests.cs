using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Web.Cache
{
    /// <summary>
    /// This suite checks the CampusCache object to make sure that
    /// all logic works as intended.
    /// </summary>
    /// <seealso cref="CampusCache"/>
    [TestClass]
    public class CampusCacheTests
    {
        /// <summary>
        /// Test to verify that RawServiceTimes is correctly built from the legacy Campus.ServiceTimes
        /// string when it exists (and there is no Campus.CampusSchedules collection).
        /// </summary>
        [TestMethod]
        public void RawServiceTimes_FromLegacyServiceTimes_Succeeds()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var rockContext = app.App.CreateRockContext();
            var campus = BasicTestCampus();

            rockContext.Set<Campus>().Add( campus );

            var campusCache = CampusCache.Get( 1, rockContext );
#pragma warning disable CS0612, CS0618
            // When this property is removed from Rock, this entire test can be removed too.
            Assert.AreEqual( "Sat^4:30pm|Sat^6pm", campusCache.RawServiceTimes );
#pragma warning restore CS0612, CS0618
        }

        /// <summary>
        /// Test to verify that RawServiceTimes is correctly built from CampusSchedules even if the
        /// legacy Campus.ServiceTimes field has a value.
        /// </summary>
        [TestMethod]
        public void RawServiceTimes_FromCampusSchedules_Succeeds()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var rockContext = app.App.CreateRockContext();

            var campusMock = BasicTestCampus();

            var schedule1 = new Schedule
            {
                Id = 1,
                Guid = new Guid( "53E1BD3C-E103-4E43-80CE-C8AE4C76392A" ),
                iCalendarContent = @"
BEGIN:VCALENDAR
PRODID:-//github.com/SparkDevNetwork/Rock//NONSGML Rock//EN
VERSION:2.0
BEGIN:VEVENT
DTEND:20130501T183000
DTSTAMP:20241216T133100
DTSTART:20130501T163000
RRULE:FREQ=WEEKLY;BYDAY=SA
SEQUENCE:0
UID:270a288d-a90c-4cc9-ae37-f1049f71e3e2
END:VEVENT
END:VCALENDAR
",
            };

            var schedule2 = new Schedule
            {
                Id = 2,
                Guid = new Guid( "d7c65ca0-64b7-4ba7-a4f0-dcb309ac0e0f" ),
                iCalendarContent = @"
BEGIN:VCALENDAR
PRODID:-//github.com/SparkDevNetwork/Rock//NONSGML Rock//EN
VERSION:2.0
BEGIN:VEVENT
DTEND:20130501T190000
DTSTAMP:20241216T133109
DTSTART:20130501T180000
RRULE:FREQ=WEEKLY;BYDAY=SA
SEQUENCE:0
UID:d7c65ca0-64b7-4ba7-a4f0-dcb309ac0e0f
END:VEVENT
END:VCALENDAR
",
            };

            var saturday430pm = new CampusSchedule
            {
                Id = 1,
                CampusId = 1,
                ScheduleId = 1,
                Schedule = schedule1
            };

            var saturday6pm = new CampusSchedule
            {
                Id = 2,
                CampusId = 1,
                ScheduleId = 2,
                Schedule = schedule2
            };

            campusMock.CampusSchedules.Add( saturday430pm );
            campusMock.CampusSchedules.Add( saturday6pm );

            rockContext.Set<Campus>().Add( campusMock );

            var campusCache = CampusCache.Get( 1, rockContext );

#pragma warning disable CS0612, CS0618
            // When this property is removed from Rock, this entire test can be removed too.
            Assert.AreEqual( "Saturday^4:30 PM|Saturday^6:00 PM", campusCache.RawServiceTimes );
#pragma warning restore CS0612, CS0618
        }

        /// <summary>
        /// Test to verify that CondensedName returns the Campus Name (without the word Campus).
        /// </summary>
        [TestMethod]
        public void CondensedName_WithoutShortCode_Succeeds()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var rockContext = app.App.CreateRockContext();
            var campus = BasicTestCampus();

            rockContext.Set<Campus>().Add( campus );

            var campusCache = CampusCache.Get( 1, rockContext );

            Assert.AreEqual( "Test", campusCache.CondensedName );
        }

        /// <summary>
        /// Test to verify that CondensedName returns the Campus ShortCode when it is set.
        /// </summary>
        [TestMethod]
        public void CondensedName_WithShortCode_Succeeds()
        {
            using var app = TestHelper.CreateScopedRockApp();
            var rockContext = app.App.CreateRockContext();
            var campus = BasicTestCampus();
            campus.ShortCode = "TC";

            rockContext.Set<Campus>().Add( campus );

            var campusCache = CampusCache.Get( 1, rockContext );

            Assert.AreEqual( "TC", campusCache.CondensedName );
        }

        #region Helper Methods

        private static Campus BasicTestCampus()
        {
            return new Campus
            {
                Id = 1,
                Guid = new Guid( "7320D3F4-D14F-4FA4-9F54-F01D0752E9E1" ),
                Name = "Test Campus",
#pragma warning disable CS0612, CS0618
                // When this property is removed from Rock, this should probably start using a mocked CampusSchedules
                // collection as seen in the RawServiceTimes_FromCampusSchedules_Succeeds() test above.
                ServiceTimes = "Sat^4:30pm|Sat^6pm",
            };
#pragma warning restore CS0612, CS0618
        }
        #endregion
    }
}
