using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Rest.Controllers;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Rest.ControllersTests
{
    [TestClass]
    public class AttendanceControllerTests : DatabaseTestsBase
    {
        [TestMethod]
        public void AddAttendance()
        {
            int groupId = 111;
            int locationId = 14;
            int scheduleId = 19;
            DateTime occurrenceDate = new DateTime( 2019, 8, 12 );
            int? personId = 4;
            int? personAliasId = null;

            var attendancesController = new AttendancesController();

            var attendance = attendancesController.AddAttendance( groupId, locationId, scheduleId, occurrenceDate, personId, personAliasId );
            Assert.That.IsNotNull( attendance );
        }

        [TestMethod]
        public void AddAttendanceNoPerson()
        {
            int groupId = 111;
            int locationId = 14;
            int scheduleId = 19;
            DateTime occurrenceDate = new DateTime( 2019, 8, 5 );
            int? personId = null;
            int? personAliasId = null;

            var attendancesController = new AttendancesController();
            Rock.Model.Attendance attendance = new Rock.Model.Attendance();
            System.Web.Http.HttpResponseException exception = null;

            try
            {
                attendance = attendancesController.AddAttendance( groupId, locationId, scheduleId, occurrenceDate, personId, personAliasId );
            }
            catch ( System.Web.Http.HttpResponseException ex )
            {
                exception = ex;
            }
            finally
            {
                Assert.That.IsTrue( exception.Response.StatusCode == System.Net.HttpStatusCode.BadRequest );
            }
        }
    }
}
