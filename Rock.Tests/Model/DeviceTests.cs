//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Data.Spatial;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Newtonsoft.Json;
using Rock.Model;
using Assert = NUnit.Framework.Assert;

namespace Rock.Tests.Model
{
    [TestFixture]
    public class DeviceTests
    {
        /// <summary>
        /// Note: Using these Microsoft.VisualStudio.TestTools.UnitTesting Attribute markup
        /// allows the tests to be grouped by "trait" (ie, the TestCategory defined below).
        /// </summary>
        [TestClass]
        public class TheGeoFenceProperty
        {
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Device" )]
            public void ShouldFallWithinGeoFence()
            {
                var deviceWithGeoFence = BufferedDeviceAtCCV();

                // (point near CCV parking lot about 400 meters from the CCV point above)
                var aLat = "33.70836";
                var aLong = "-112.20765";
                var aPointInside = DbGeography.FromText( string.Format( "POINT({0} {1})", aLong, aLat ) ); // NOTE: long, lat

                Assert.True( aPointInside.Intersects( deviceWithGeoFence.GeoFence ) );
            }

            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Device" )]
            public void ShouldFallOutsideGeoFence()
            {
                var deviceWithGeoFence = BufferedDeviceAtCCV();

                // (a point about half a mile away from CCV)
                var farLat = "33.6961";
                var farLong = "-112.2030";
                var aPointOutside = DbGeography.FromText( string.Format( "POINT({0} {1})", farLong, farLat ) ); // NOTE: long, lat

                // This point should NOT intersect our buffered CCV location
                Assert.False( aPointOutside.Intersects( deviceWithGeoFence.GeoFence ) );
            }
        }

        [TestClass]
        public class TheToJsonMethod
        {
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Device" )]
            public void ShouldNotBeEmpty()
            {
                var device = DeviceAtCCV();
                var result = device.ToJson();
                Assert.IsNotEmpty( result );
            }

            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Device" )]
            public void ShouldSerializeAsJsonViaCustomConverter()
            {
                var device = DeviceAtCCV();

                var result = device.ToJson();
                string key1 = "\"GeoPoint\": {";
                string key2 = "\"latitude\": ";
                Assert.Greater( result.IndexOf( key1 ), -1, string.Format( "'{0}' was not found in '{1}'.", key1, result ) );
                Assert.Greater( result.IndexOf( key2 ), -1, string.Format( "'{0}' was not found in '{1}'.", key2, result ) );
            }

            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Device" )]
            public void ShouldDeserializeFromJsonViaCustomConverter()
            {
                var device = DeviceAtCCV();

                var deviceAsJson = device.ToJson();
                var deviceFromJson = JsonConvert.DeserializeObject( deviceAsJson, typeof( Device ) ) as Device;

               Assert.AreEqual( device.Guid, deviceFromJson.Guid );
               Assert.AreEqual( device.GeoPoint.Latitude, deviceFromJson.GeoPoint.Latitude );
               Assert.AreEqual( device.GeoPoint.Longitude, deviceFromJson.GeoPoint.Longitude );
            }
        }

        private static Device DeviceAtCCV()
        {
            // 33.71060, -112.20884 (CCV)
            var ccvLat = "33.71060";
            var ccvLon = "-112.20884";

            var aDevice = new Device();
            var aPointAtCCV = DbGeography.PointFromText(
                string.Format( "POINT({0} {1})", ccvLon, ccvLat ), 4326 );  // NOTE: long, lat
            aDevice.GeoPoint = aPointAtCCV;
            return aDevice;
        }

        private static Device BufferedDeviceAtCCV()
        {
            // 33.71060, -112.20884 (CCV)
            var ccvLat = "33.71060";
            var ccvLon = "-112.20884";
            var radiusInMeters = 600;

            var aBufferedDevice = new Device();
            var aPointAtCCV = DbGeography.PointFromText(
                string.Format( "POINT({0} {1})", ccvLon, ccvLat ), 4326 );  // NOTE: long, lat
            // now make this 'buffered device' the GeoPoint of CCV. 
            aBufferedDevice.GeoFence = aPointAtCCV.Buffer( radiusInMeters );
            return aBufferedDevice;
        }
    }
}
