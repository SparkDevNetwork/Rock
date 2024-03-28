// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Reporting.DataFilter.Person;
using Rock.Tests.Shared;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Reporting.DataFilter.Person
{
    /// <summary>
    /// Test DataFilter: Person/PersonalDevices.
    /// </summary>
    [TestClass]
    public class PersonalDevicesDataFilterTests : DataFilterTestBase
    {
        private const string _TestCategory = "Rock.Crm.Personalization.Reporting.PersonalDevicesDataFilter.Tests";

        [ClassInitialize]
        public static void Initialize( TestContext context )
        {
            CreateTestData();
        }

        #region Personal Device Test Data

        private const string DeviceTedDeckerTv = "DA828F8E-447D-4135-AF37-68F6CBA9FCD3";
        private const string DeviceTedDeckerComputer = "18509100-B765-49CF-A305-CBE8B93A71F2";
        private const string DeviceBillMarbleMobile = "58B14DF2-0343-41D3-BD98-7E4D4424CCC1";
        private const string DeviceAlishaMarbleMobile = "54567D8E-778A-477C-9B54-6981DD7645CD";
        private const string DeviceAlishaMarbleTv = "E5987856-2B67-4659-95B5-B9C14F3B5297";
        private const string DeviceAlishaMarbleComputer = "DBCF612A-54F4-4407-B084-510A39CE449F";

        private static void CreateTestData()
        {
            var rockContext = new RockContext();

            // Add Personal Devices for Ted Decker
            // Sites: Internal, External
            AddPersonalDevice( rockContext,
                DeviceTedDeckerComputer,
                TestGuids.TestPeople.TedDecker,
                SystemGuid.Site.SITE_ROCK_INTERNAL,
                SystemGuid.DefinedValue.PERSONAL_DEVICE_TYPE_COMPUTER,
                SystemGuid.DefinedValue.PERSONAL_DEVICE_PLATFORM_OTHER,
                notificationsEnabled: true );
            AddPersonalDevice( rockContext,
                DeviceTedDeckerTv,
                TestGuids.TestPeople.TedDecker,
                SystemGuid.Site.EXTERNAL_SITE,
                SystemGuid.DefinedValue.PERSONAL_DEVICE_TYPE_TV,
                SystemGuid.DefinedValue.PERSONAL_DEVICE_PLATFORM_IOS,
                notificationsEnabled: false );

            // Add Personal Devices for Bill Marble
            // Sites: External
            AddPersonalDevice( rockContext,
                DeviceBillMarbleMobile,
                TestGuids.TestPeople.BillMarble,
                SystemGuid.Site.EXTERNAL_SITE,
                SystemGuid.DefinedValue.PERSONAL_DEVICE_TYPE_MOBILE,
                SystemGuid.DefinedValue.PERSONAL_DEVICE_PLATFORM_ANDROID,
                notificationsEnabled: false );

            // Add Personal Devices for Alisha Marble
            // Sites: Internal
            AddPersonalDevice( rockContext,
                DeviceAlishaMarbleComputer,
                TestGuids.TestPeople.AlishaMarble,
                SystemGuid.Site.SITE_ROCK_INTERNAL,
                SystemGuid.DefinedValue.PERSONAL_DEVICE_TYPE_COMPUTER,
                SystemGuid.DefinedValue.PERSONAL_DEVICE_PLATFORM_OTHER,
                notificationsEnabled: true );
            AddPersonalDevice( rockContext,
                DeviceAlishaMarbleMobile,
                TestGuids.TestPeople.AlishaMarble,
                SystemGuid.Site.SITE_ROCK_INTERNAL,
                SystemGuid.DefinedValue.PERSONAL_DEVICE_TYPE_MOBILE,
                SystemGuid.DefinedValue.PERSONAL_DEVICE_PLATFORM_IOS,
                notificationsEnabled: true );
            AddPersonalDevice( rockContext,
                DeviceAlishaMarbleTv,
                TestGuids.TestPeople.AlishaMarble,
                SystemGuid.Site.SITE_ROCK_INTERNAL,
                SystemGuid.DefinedValue.PERSONAL_DEVICE_TYPE_TV,
                SystemGuid.DefinedValue.PERSONAL_DEVICE_PLATFORM_ANDROID,
                notificationsEnabled: true );

            rockContext.SaveChanges();
        }

        private static PersonalDevice AddPersonalDevice( RockContext dataContext, string deviceGuid, string personGuid, string siteGuid, string deviceTypeGuid, string platformGuid, bool notificationsEnabled )
        {
            var deviceService = new PersonalDeviceService( dataContext );

            var person = TestDataHelper.GetTestPerson( personGuid );
            var site = SiteCache.Get( siteGuid );

            var device = deviceService.Get( deviceGuid.AsGuid() );

            if ( device == null )
            {
                device = new PersonalDevice();
                deviceService.Add( device );
            }

            device.Guid = deviceGuid.AsGuid();
            device.PersonAliasId = person.PrimaryAliasId;
            device.SiteId = site.Id;
            device.PersonalDeviceTypeValueId = DefinedValueCache.GetId( deviceTypeGuid.AsGuid() );
            device.PlatformValueId = DefinedValueCache.GetId( platformGuid.AsGuid() );
            device.NotificationsEnabled = notificationsEnabled;

            return device;
        }

        #endregion

        [TestMethod]
        [TestCategory( _TestCategory )]
        public void PersonalDevicesFilter_FilterBySite_ReturnsMatchedSitesOnly()
        {
            // Filter for Site=Internal
            var settings = new PersonalDevicesFilter.FilterSettings();
            settings.SiteGuids = new List<Guid> { SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid() };

            // Get Query Results.
            var personQuery = GetPersonQueryWithPersonalDevicesFilter( settings );
            var results = personQuery.ToList();

            // Verify Ted Decker found - Internal site only.
            Assert.That.IsTrue( results.Any( x => x.Guid == TestGuids.TestPeople.TedDecker.AsGuid() ) );
            // Verify Alisha Marble found - Internal and External Sites.
            Assert.That.IsTrue( results.Any( x => x.Guid == TestGuids.TestPeople.AlishaMarble.AsGuid() ) );
            // Verify Bill Marble not found - External Site only.
            Assert.That.IsFalse( results.Any( x => x.Guid == TestGuids.TestPeople.BillMarble.AsGuid() ) );
        }

        [TestMethod]
        [TestCategory( _TestCategory )]
        public void PersonalDevicesFilter_FilterByDeviceType_ShouldReturnMatchingDeviceTypes()
        {
            // Filter for DeviceType=Mobile.
            var settings = new PersonalDevicesFilter.FilterSettings();
            settings.DeviceTypeGuids = new List<Guid> { SystemGuid.DefinedValue.PERSONAL_DEVICE_TYPE_MOBILE.AsGuid() };

            var personQuery = GetPersonQueryWithPersonalDevicesFilter( settings );
            var results = personQuery.ToList();

            // Verify Alisha Marble found - has Mobile.
            Assert.That.IsTrue( results.Any( x => x.Guid == TestGuids.TestPeople.AlishaMarble.AsGuid() ) );
            // Verify Bill Marble found - has Mobile.
            Assert.That.IsTrue( results.Any( x => x.Guid == TestGuids.TestPeople.BillMarble.AsGuid() ) );
            // Verify Ted Decker not found - no Mobile.
            Assert.That.IsFalse( results.Any( x => x.Guid == TestGuids.TestPeople.TedDecker.AsGuid() ) );
        }

        [TestMethod]
        [TestCategory( _TestCategory )]
        public void PersonalDevicesFilter_FilterByDevicePlatform_ShouldReturnMatchingDevicePlatforms()
        {
            // Filter for DevicePlatform=Mobile.
            var settings = new PersonalDevicesFilter.FilterSettings();
            settings.DevicePlatformGuids = new List<Guid> { SystemGuid.DefinedValue.PERSONAL_DEVICE_PLATFORM_IOS.AsGuid() };

            var personQuery = GetPersonQueryWithPersonalDevicesFilter( settings );
            var results = personQuery.ToList();

            // Verify Ted Decker found - iOS and Other platforms.
            Assert.That.IsTrue( results.Any( x => x.Guid == TestGuids.TestPeople.TedDecker.AsGuid() ) );
            // Verify Alisha Marble found - iOS, Android and Other platforms.
            Assert.That.IsTrue( results.Any( x => x.Guid == TestGuids.TestPeople.AlishaMarble.AsGuid() ) );
            // Verify Bill Marble not found - Android platform only.
            Assert.That.IsFalse( results.Any( x => x.Guid == TestGuids.TestPeople.BillMarble.AsGuid() ) );
        }

        [TestMethod]
        [TestCategory( _TestCategory )]
        public void PersonalDevicesFilter_FilterByNotificationStatus_ShouldReturnMatchingNotificationStatus()
        {
            // Filter for DevicePlatform=Mobile.
            var settings = new PersonalDevicesFilter.FilterSettings();
            settings.NotificationsEnabled = true;

            var personQuery = GetPersonQueryWithPersonalDevicesFilter( settings );
            var results = personQuery.ToList();

            // Verify Ted Decker found - some devices with Notifications enabled.
            Assert.That.IsTrue( results.Any( x => x.Guid == TestGuids.TestPeople.TedDecker.AsGuid() ) );
            // Verify Alisha Marble found - all devices with Notifications enabled.
            Assert.That.IsTrue( results.Any( x => x.Guid == TestGuids.TestPeople.AlishaMarble.AsGuid() ) );
            // Verify Bill Marble not found - no devices with Notifications enabled.
            Assert.That.IsFalse( results.Any( x => x.Guid == TestGuids.TestPeople.BillMarble.AsGuid() ) );
        }

        /// <summary>
        /// Create a Person Query using the PersonalDevices filter with the specified settings.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        private IQueryable<IEntity> GetPersonQueryWithPersonalDevicesFilter( PersonalDevicesFilter.FilterSettings settings )
        {
            var settingsFilter = new PersonalDevicesFilter();
            var dataContext = new RockContext();
            var personService = new PersonService( dataContext );

            var parameterExpression = personService.ParameterExpression;

            var settingsString = DataComponentSettingsHelper.SerializeFilterSettings( settings );
            var predicate = settingsFilter.GetExpression( typeof( Rock.Model.Person ), personService, parameterExpression, settingsString );

            var personQuery = GetFilteredEntityQuery<Rock.Model.Person>( dataContext, predicate, parameterExpression );

            return personQuery;
        }
    }
}
