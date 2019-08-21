using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Model;
using Rock.Rest.Controllers;
using System.Net;

namespace Rock.Tests.Integration.RestTests.ControllersTests
{
    [TestClass]
    [Ignore()]
    public class AttendanceControllerTests
    {
        /// <summary>
        /// Runs before any tests in this class are executed.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize( TestContext testContext )
        {
        }

        [TestMethod]
        public void AddAttendance()
        {
            int groupId = 111;
            int locationId = 14;
            int scheduleId = 19;
            DateTime occurrenceDate = new DateTime(2019, 8, 12);
            int? personId = 4;
            int? personAliasId = null;

            var attendancesController = new AttendancesController();

            var attendance = attendancesController.AddAttendance( groupId, locationId, scheduleId, occurrenceDate, personId, personAliasId );
            Assert.IsNotNull( attendance );
        }

        [TestMethod]
        public void AddAttendanceNoPerson()
        {
            int groupId = 111;
            int locationId = 14;
            int scheduleId = 19;
            DateTime occurrenceDate = new DateTime(2019, 8, 5);
            int? personId = null;
            int? personAliasId = null;

            var attendancesController = new AttendancesController();
            Attendance attendance = new Attendance();
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
                Assert.IsTrue( exception.Response.StatusCode == System.Net.HttpStatusCode.BadRequest );
            }
        }

    }
}
