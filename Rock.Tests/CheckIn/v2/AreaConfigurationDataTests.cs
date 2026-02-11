using System;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.CheckIn.v2;
using Rock.Enums.CheckIn;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.CheckIn.v2
{
    /// <summary>
    /// This suite checks the area configuration data objects to make sure
    /// they continue to work as expected. This primarily covers just the
    /// constructors as they currently have no other logic.
    /// </summary>
    /// <seealso cref="AreaConfigurationData"/>
    [TestClass]
    public class AreaConfigurationDataTests : MockDatabaseTestsBase
    {
        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithGroupTypeCache_InitializesProperties()
        {
            var expectedAttendanceRule = AttendanceRule.AddOnCheckIn;
            var expectedAlreadyEnrolledMatchingLogic = AlreadyEnrolledMatchingLogic.PreferEnrolledGroups;
            var expectedPrintTo = PrintTo.Location;
            var expectedLocationSelectionStrategy = LocationSelectionStrategy.Balance;

            var rockContextMock = MockDatabaseHelper.CreateRockContextMock();
            var rockContextFactory = MockDatabaseHelper.CreateRockContextFactory( rockContextMock );

            var groupType = new GroupType
            {
                Id = 1,
                AttendanceRule = expectedAttendanceRule,
                AttendancePrintTo = expectedPrintTo,
                AlreadyEnrolledMatchingLogic = expectedAlreadyEnrolledMatchingLogic,
                IsConcurrentCheckInPrevented = true,
                IsSchedulingEnabled = true,
                Attributes = new Dictionary<string, AttributeCache>(),
                AttributeValues = new Dictionary<string, AttributeValueCache>(),
            };

            groupType.AttributeValues.Add( SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_LOCATION_SELECTION_STRATEGY, new AttributeValueCache
            {
                Value = expectedLocationSelectionStrategy.ConvertToInt().ToString()
            } );

            rockContextMock.Object.Set<GroupType>().Add( groupType );

            using ( TestHelper.CreateScopedRockApp( sc => sc.AddSingleton( rockContextFactory ) ) )
            {
                var groupTypeCache = GroupTypeCache.Get( groupType.Id, rockContextMock.Object );
                var instance = new AreaConfigurationData( groupTypeCache, rockContextMock.Object );

                Assert.AreEqual( expectedAttendanceRule, instance.AttendanceRule );
                Assert.AreEqual( expectedAlreadyEnrolledMatchingLogic, instance.AlreadyEnrolledMatchingLogic );
                Assert.IsTrue( instance.IsConcurrentCheckInPrevented );
                Assert.IsTrue( instance.IsSchedulingEnabled );
                Assert.AreEqual( expectedPrintTo, instance.PrintTo );
                Assert.AreEqual( expectedLocationSelectionStrategy, instance.LocationSelectionStrategy );
            }
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
