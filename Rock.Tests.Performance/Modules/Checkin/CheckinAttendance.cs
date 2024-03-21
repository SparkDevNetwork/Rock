using System.Diagnostics;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Attendances
{
    [TestClass]
    public class CheckinAttendance : DatabaseTestsBase
    {
        [TestMethod]
        public void PerformanceTest()
        {
            var rockContext = new RockContext();

            var personAliasIdList = TestDataHelper.GetPersonIdWithAliasIdList().Select( a => a.PersonAliasId ).ToList();

            var locationIdList = new LocationService( rockContext ).Queryable().Where( a => a.ParentLocationId == 3 || a.Name == "A/V Booth" ).Select( a => a.Id ).ToList();
            var groupIdList = new GroupService( rockContext ).Queryable().Where( a => a.GroupType.TakesAttendance || a.GroupType.IsSchedulingEnabled ).Select( a => a.Id ).ToList();
            var serviceTimesGuid = SystemGuid.Category.SCHEDULE_SERVICE_TIMES.AsGuid();
            var scheduleIdList = new ScheduleService( rockContext ).Queryable().Where( a => a.Category.Guid == serviceTimesGuid ).Select( a => a.Id ).ToList();
            var mainCampusId = 1;
            int attendanceCount = 0;
            Stopwatch stopwatch = Stopwatch.StartNew();

            foreach ( var locationId in locationIdList )
            {
                foreach ( var groupId in groupIdList )
                {
                    foreach ( var scheduleId in scheduleIdList )
                    {
                        foreach ( var personAliasId in personAliasIdList )
                        {
                            if ( attendanceCount > 1000 )
                            {
                                break;
                            }

                            using ( var rockContextAttendance = new RockContext() )
                            {
                                var attendanceService = new AttendanceService( rockContextAttendance );
                                var checkinDateTime = RockDateTime.Now;
                                var attendance = attendanceService.AddOrUpdate( personAliasId, checkinDateTime, groupId, locationId, scheduleId, mainCampusId, TestDataHelper.KioskDeviceId, null, null, null, null );
                                rockContextAttendance.SaveChanges();
                                attendanceCount++;
                            }
                        }
                    }
                }
            }

            stopwatch.Stop();
            Debug.WriteLine( $"attendanceCount:{attendanceCount},stopwatchMS:{stopwatch.Elapsed.TotalMilliseconds}, stopwatch.Elapsed.TotalMilliseconds/ attendanceCount {stopwatch.Elapsed.TotalMilliseconds / attendanceCount} " );
        }
    }
}
