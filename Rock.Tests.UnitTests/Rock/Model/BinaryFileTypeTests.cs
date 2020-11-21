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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Model;
using Rock.Tests.Shared;

namespace Rock.Tests.UnitTests.Rock.Model
{
    [TestClass]
    public class BinaryFileTypeTests
    {
        [TestMethod]
        public void CacheControlHeaderSettingsChangesShouldResetCacheControlHeader()
        {
            var model = new BinaryFileType();

            model.CacheControlHeaderSettings = "{\"RockCacheablityType\":3,\"MaxAge\":null,\"MaxSharedAge\":null}";
            var header = model.CacheControlHeader;

            Assert.That.IsNotNull( header );

            model.CacheControlHeaderSettings = "{\"RockCacheablityType\":0,\"MaxAge\":{\"Value\":31556952,\"Unit\":0},\"MaxSharedAge\":null}";
            var header2 = model.CacheControlHeader;

            Assert.That.IsNotNull( header2 );
            Assert.That.AreNotEqual( header.RockCacheablityType, header2.RockCacheablityType );
        }

        [TestMethod]
        public void CacheControlHeaderSettingsNoChangeShouldNotResetCacheControlHeader()
        {
            var model = new BinaryFileType();

            model.CacheControlHeaderSettings = "{\"RockCacheablityType\":3,\"MaxAge\":null,\"MaxSharedAge\":null}";

            var header = model.CacheControlHeader;
            Assert.That.IsNotNull( header );

            model.CacheControlHeaderSettings = "{\"RockCacheablityType\":3,\"MaxAge\":null,\"MaxSharedAge\":null}";
            var header2 = model.CacheControlHeader;

            Assert.That.IsNotNull( header2 );
            Assert.That.AreEqual( header, header2 );
        }
    }
}
