using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.CheckIn.v2;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.CheckIn.v2
{
    [TestClass]
    public class OpportunityCollectionTests : DatabaseTestsBase
    {
        /// <summary>
        /// Inactive groups should not be included in the opportunity collection.
        /// </summary>
        /// <seealso href="https://github.com/SparkDevNetwork/Rock/issues/6137"/>
        [TestMethod]
        [IsolatedTestDatabase]
        public void Create_ExcludesInactiveGroups()
        {
            var nurseryGroupGuid = new Guid( "dc1a2a83-1b5d-46bc-9e99-4571466827f5" );
            var nurseryPreschoolAreaGuid = new Guid( "cadb2d12-7836-44bc-8eea-3c6ab22fd5e8" );
            var bunniesRoomGuid = new Guid( "844336f4-88b4-4894-b416-769c95a4702d" );
            var kittensRoomGuid = new Guid( "c07b25a6-eb58-4f77-8ed3-952c5a4dee9f" );
            var puppiesRoomGuid = new Guid( "0bd7bf34-647d-4dc1-8c61-dd154b403687" );
            var sunday9amGuid = new Guid( "ff6fb240-0c32-4542-be40-159c522f7e51" );
            var centralKioskGuid = new Guid( "61111232-01d7-427d-9c1f-d45cf4d3f7cb" );

            using ( var rockContext = new RockContext() )
            {
                var nurseryGroup = new GroupService( rockContext ).Get( nurseryGroupGuid );
                var schedule = new ScheduleService( rockContext ).Get( sunday9amGuid );

                // Make the nursery group inactive for this test.
                nurseryGroup.IsActive = false;

                // Make the Sunday 9:00 am service a daily all day so we can
                // test the exclusion.
                schedule.iCalendarContent = @"BEGIN:VCALENDAR
BEGIN:VEVENT
DTEND:20130501T235900
DTSTART:20130501T000000
RRULE:FREQ=DAILY
END:VEVENT
END:VCALENDAR";
                schedule.CheckInStartOffsetMinutes = 0;
                schedule.CheckInEndOffsetMinutes = null;

                rockContext.SaveChanges( disablePrePostProcessing: true );

                var possibleAreas = new List<GroupTypeCache>
                {
                    GroupTypeCache.Get( nurseryPreschoolAreaGuid, rockContext )
                };

                var locations = new List<NamedLocationCache>
                {
                    NamedLocationCache.Get( bunniesRoomGuid, rockContext ), // Nursery Group Room
                    NamedLocationCache.Get( kittensRoomGuid, rockContext ), // Crawlers/Walkers Group Room
                    NamedLocationCache.Get( puppiesRoomGuid, rockContext )  // Preschool Group Room
                };

                var kiosk = DeviceCache.Get( centralKioskGuid, rockContext );

                var opportunities = OpportunityCollection.Create( possibleAreas, kiosk, locations, rockContext );

                Assert.IsNotNull( opportunities );
                Assert.AreNotEqual( 0, opportunities.Groups.Count );
                Assert.IsFalse( opportunities.Groups.Any( g => g.Id == nurseryGroup.IdKey ), "Inactive group was included in opportunity collection.");
            }
        }
    }
}
