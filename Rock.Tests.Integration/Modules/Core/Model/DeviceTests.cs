using System.Data.Entity.Spatial;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Core.Model
{
    public class DeviceTests : DatabaseTestsBase
    {
        /* These DbGeography calls require the SqlServerTypes package on machines without full SQL Server.
         * However, none of them will actually find the SqlServerSpatial110.dll in the /bin folder. */

        [TestMethod]
        [Ignore( "Need the SqlServerTypes library to resolve" )]
       
        public void FallsWithinGeoFence()
        {
            var deviceWithGeoFence = BufferedDevice();

            // point about 400 meters away, buffer is set to 600 meters
            var aLat = "33.70836";
            var aLong = "-112.20765";
            // NOTE: generate in format: long, lat
            var pointInside = DbGeography.FromText( string.Format( "POINT({0} {1})", aLong, aLat ) );

            Assert.That.IsTrue( pointInside.Intersects( deviceWithGeoFence.Location.GeoFence ) );
        }

        [TestMethod] [Ignore( "Need the SqlServerTypes library to resolve" )]
        public void FallsOutsideGeoFence()
        {
            var deviceWithGeoFence = BufferedDevice();

            // point about half a mile away
            var farLat = "33.6961";
            var farLong = "-112.2030";
            // NOTE: generate in format: long, lat
            var pointOutside = DbGeography.FromText( string.Format( "POINT({0} {1})", farLong, farLat ) );

            // point should NOT intersect the location
            Assert.That.IsFalse( pointOutside.Intersects( deviceWithGeoFence.Location.GeoFence ) );
        }

        /// <summary>
        /// Should verify the device isn't empty.
        /// </summary>
        [TestMethod] [Ignore( "Need the SqlServerTypes library to resolve" )]
        public void NotEmpty()
        {
            var device = StandardDevice();
            var result = device.ToJson();
            Assert.That.IsNotEmpty( result );
        }

        /// <summary>
        /// Should serialize the Device into a non-empty string.
        /// </summary>
        [TestMethod] [Ignore( "Need the SqlServerTypes library to resolve" )]
        public void ToJson()
        {
            var device = StandardDevice();

            var result = device.ToJson();
            string key1 = "\"GeoPoint\": {";
            string key2 = "\"WellKnownText\": ";
            Assert.That.AreNotEqual( result.IndexOf( key1 ), -1 );
            Assert.That.AreNotEqual( result.IndexOf( key2 ), -1 );
        }

        /// <summary>
        /// Should take a JSON string and copy its contents to a new Device
        /// </summary>
        [TestMethod] [Ignore( "Need the SqlServerTypes library to resolve" )]
        public void FromJson()
        {
            var device = StandardDevice();

            var deviceAsJson = device.ToJson();
            var deviceFromJson = JsonConvert.DeserializeObject( deviceAsJson, typeof( Device ) ) as Device;

            Assert.That.AreEqual( device.Guid, deviceFromJson.Guid );
            Assert.That.AreEqual( device.Location.GeoPoint.Latitude, deviceFromJson.Location.GeoPoint.Latitude );
            Assert.That.AreEqual( device.Location.GeoPoint.Longitude, deviceFromJson.Location.GeoPoint.Longitude );
        }

        /// <summary>
        /// Creates a standard device with a specific geopoint.
        /// </summary>
        /// <returns></returns>
        private static Device StandardDevice()
        {
            var deviceLat = "33.71060";
            var deviceLon = "-112.20884";

            var device = new Device();
            device.Location = new Location();

            // NOTE: generate in format: long, lat
            device.Location.GeoPoint = DbGeography.PointFromText( string.Format( "POINT({0} {1})", deviceLon, deviceLat ), 4326 );
            return device;
        }

        /// <summary>
        /// Creates a device with a geopoint buffered by 600 meters.
        /// </summary>
        /// <returns></returns>
        private static Device BufferedDevice()
        {
            var deviceLat = "33.71060";
            var deviceLon = "-112.20884";
            var radiusInMeters = 600;

            var bufferedDevice = new Device();
            bufferedDevice.Location = new Location();

            // NOTE: generate in format: long, lat
            var bufferedGeopoint = DbGeography.PointFromText( string.Format( "POINT({0} {1})", deviceLon, deviceLat ), 4326 );
            bufferedDevice.Location.GeoFence = bufferedGeopoint.Buffer( radiusInMeters );
            return bufferedDevice;
        }
    }
}