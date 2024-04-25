using System;
using System.Collections.Generic;
using System.Web;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Modules.Web.Utilities
{
    [TestClass]
    public class RockUpdateHelperTests
    {
        [TestMethod]
        public void GetEnvDataAsJsonShouldReturnCorrectData()
        {
            var request = new HttpRequest( "test", "http://localhost/test", "" );
            var data = Rock.Web.Utilities.RockUpdateHelper.GetEnvDataAsJson( request, "test" );

            var actualResult = data.FromJsonOrNull<Dictionary<string, string>>();
            Assert.That.AreEqual( "test", actualResult["AppRoot"] );
            Assert.That.AreEqual( ( IntPtr.Size == 4 ) ? "32bit" : "64bit", actualResult["Architecture"] );
            Assert.That.AreEqual( Rock.Web.Utilities.RockUpdateHelper.GetDotNetVersion(), actualResult["AspNetVersion"] );
            Assert.That.AreEqual( Environment.OSVersion.ToString(), actualResult["ServerOs"] );
        }

        [TestMethod]
        [DataRow( 528040, ".NET Framework 4.8" )]
        [DataRow( 500000, ".NET Framework 4.7.2" )]
        [DataRow( 461808, ".NET Framework 4.7.2" )]
        [DataRow( 461308, ".NET Framework 4.7.1" )]
        [DataRow( 460798, ".NET Framework 4.7" )]
        [DataRow( 394802, ".NET Framework 4.6.2" )]
        [DataRow( 394254, ".NET Framework 4.6.1" )]
        [DataRow( 393295, ".NET Framework 4.6" )]
        [DataRow( 379893, ".NET Framework 4.5.2" )]
        [DataRow( 378675, ".NET Framework 4.5.1" )]
        [DataRow( 378389, ".NET Framework 4.5" )]
        [DataRow( 378388, "Unknown" )]
        public void GetDotNetVersionShouldReturnCorrectString(int releaseNumber, string expectedResult )
        {
            var actualResult = Rock.Web.Utilities.RockUpdateHelper.GetDotNetVersion( releaseNumber );
            Assert.That.AreEqual( expectedResult, actualResult );
        }
    }
}
