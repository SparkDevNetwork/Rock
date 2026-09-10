using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Model;
using Rock.Tests.Shared.Utility;

namespace Rock.Tests.Model
{
    [TestClass]
    public class AttendanceOccurrenceTests
    {
        [TestMethod]
        public void AttendanceOccurrenceDateKeyGetsSetCorrectly()
        {
            var testList = DateTimeTestHelper.GetDateKeyTestData();

            foreach ( var keyValue in testList )
            {
                AttendanceOccurrence attendanceOccurrence = new AttendanceOccurrence();
                attendanceOccurrence.OccurrenceDate = keyValue.Value;
                Assert.AreEqual( keyValue.Key, attendanceOccurrence.OccurrenceDateKey );
            }
        }
    }
}
