using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.CheckIn.v2;
using Rock.CheckIn.v2.Filters;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tests.UnitTests.Rock.CheckIn.v2
{
    /// <summary>
    /// This suite checks the area configuration data objects to make sure
    /// they continue to work as expected. This primarily covers just the
    /// constructors as they currently have no other logic.
    /// </summary>
    /// <seealso cref="BirthMonthOpportunityFilter"/>
    [TestClass]
    public class AreaConfigurationDataTests : CheckInMockDatabase
    {
        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithGroupTypeCache_InitializesProperties()
        {
            var expectedAttendanceRule = AttendanceRule.AddOnCheckIn;
            var expectedPrintTo = PrintTo.Location;

            var rockContextMock = GetRockContextMock();
            var groupType = CreateEntityMock<GroupType>( 1, new Guid( "4b8fd000-2043-4f4b-a2f6-31d58e26123c" ) );

            groupType.Object.AttendanceRule = expectedAttendanceRule;
            groupType.Object.AttendancePrintTo = expectedPrintTo;

            var groupTypeCache = new GroupTypeCache();
            groupTypeCache.SetFromEntity( groupType.Object );

            var instance = new AreaConfigurationData( groupTypeCache, rockContextMock.Object );

            Assert.AreEqual( expectedAttendanceRule, instance.AttendanceRule );
            Assert.AreEqual( expectedPrintTo, instance.PrintTo );
        }

        #endregion
    }
}
