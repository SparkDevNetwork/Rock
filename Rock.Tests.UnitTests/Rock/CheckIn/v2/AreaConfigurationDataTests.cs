using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.CheckIn.v2;
using Rock.Enums.CheckIn;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.UnitTests.Rock.CheckIn.v2
{
    /// <summary>
    /// This suite checks the area configuration data objects to make sure
    /// they continue to work as expected. This primarily covers just the
    /// constructors as they currently have no other logic.
    /// </summary>
    /// <seealso cref="AreaConfigurationData"/>
    [TestClass]
    public class AreaConfigurationDataTests : CheckInMockDatabase
    {
        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithGroupTypeCache_InitializesProperties()
        {
            var expectedAttendanceRule = AttendanceRule.AddOnCheckIn;
            var expectedAlreadyEnrolledMatchingLogic = AlreadyEnrolledMatchingLogic.PreferEnrolledGroups;
            var expectedPrintTo = PrintTo.Location;
            var expectedLocationSelectionStrategy = LocationSelectionStrategy.Balance;

            var rockContextMock = GetRockContextMock();
            var groupType = CreateEntityMock<GroupType>( 1, new Guid( "4b8fd000-2043-4f4b-a2f6-31d58e26123c" ) );

            groupType.Object.AttendanceRule = expectedAttendanceRule;
            groupType.Object.AttendancePrintTo = expectedPrintTo;
            groupType.Object.AlreadyEnrolledMatchingLogic = expectedAlreadyEnrolledMatchingLogic;
            groupType.Object.IsConcurrentCheckInPrevented = true;
            groupType.Object.IsSchedulingEnabled = true;
            groupType.SetMockAttributeValue( SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_LOCATION_SELECTION_STRATEGY, LocationSelectionStrategy.Balance.ConvertToInt().ToString() );

            var groupTypeCache = new GroupTypeCache();
            groupTypeCache.SetFromEntity( groupType.Object );

            var instance = new AreaConfigurationData( groupTypeCache, rockContextMock.Object );

            Assert.AreEqual( expectedAttendanceRule, instance.AttendanceRule );
            Assert.AreEqual( expectedAlreadyEnrolledMatchingLogic, instance.AlreadyEnrolledMatchingLogic );
            Assert.IsTrue( instance.IsConcurrentCheckInPrevented );
            Assert.IsTrue( instance.IsSchedulingEnabled );
            Assert.AreEqual( expectedPrintTo, instance.PrintTo );
            Assert.AreEqual( expectedLocationSelectionStrategy, instance.LocationSelectionStrategy );
        }

        [TestMethod]
        public void DeclaredType_HasExpectedPropertyCount()
        {
            // This is a simple test to help us know when new properties are
            // added so we can update the other tests to check for those
            // properties.
            var type = typeof( AreaConfigurationData );
            var expectedPropertyCount = 6;

            var propertyCount = type.GetProperties().Length;

            Assert.AreEqual( expectedPropertyCount, propertyCount );
        }

        #endregion
    }
}
