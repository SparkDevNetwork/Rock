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

using System.Net;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Net;

namespace Rock.Tests.Net
{
    /// <summary>
    /// Unit tests for the <see cref="RemoteImageDownloader"/> class. Every
    /// URL here is rejected before a request is sent, so no network access
    /// is needed.
    /// </summary>
    [TestClass]
    public class RemoteImageDownloaderTests
    {
        [TestMethod]
        [DataRow( null )]
        [DataRow( "" )]
        [DataRow( "not a url" )]
        [DataRow( "/relative/picture.jpg" )]
        [DataRow( "http://example.com/picture.jpg" )]
        [DataRow( "ftp://example.com/picture.jpg" )]
        [DataRow( "file:///C:/Windows/win.ini" )]
        [DataRow( "https://example.com:8443/picture.jpg" )]
        [DataRow( "https://user:pass@example.com/picture.jpg" )]
        [DataRow( "https://127.0.0.1/" )]
        [DataRow( "https://localhost/" )]
        [DataRow( "https://10.0.0.1/" )]
        [DataRow( "https://169.254.169.254/latest/meta-data/" )]
        [DataRow( "https://[::1]/" )]
        [DataRow( "https://[::ffff:10.0.0.1]/" )]
        public void TryDownloadImage_WithDisallowedUrl_ReturnsFalse( string url )
        {
            var result = RemoteImageDownloader.TryDownloadImage( url, out var bytes, out var mimeType, out var fileExtension );

            Assert.IsFalse( result );
            Assert.IsNull( bytes );
            Assert.IsNull( mimeType );
            Assert.IsNull( fileExtension );
        }

        [TestMethod]
        [DataRow( "0.0.0.0" )]
        [DataRow( "0.1.2.3" )]
        [DataRow( "10.0.0.1" )]
        [DataRow( "10.255.255.255" )]
        [DataRow( "100.64.0.1" )]
        [DataRow( "100.127.255.255" )]
        [DataRow( "127.0.0.1" )]
        [DataRow( "127.10.20.30" )]
        [DataRow( "169.254.169.254" )]
        [DataRow( "172.16.0.1" )]
        [DataRow( "172.31.255.255" )]
        [DataRow( "192.0.0.1" )]
        [DataRow( "192.168.1.1" )]
        [DataRow( "198.18.0.1" )]
        [DataRow( "198.19.255.255" )]
        [DataRow( "224.0.0.1" )]
        [DataRow( "239.255.255.250" )]
        [DataRow( "255.255.255.255" )]
        [DataRow( "::" )]
        [DataRow( "::1" )]
        [DataRow( "fe80::1" )]
        [DataRow( "fec0::1" )]
        [DataRow( "fc00::1" )]
        [DataRow( "fd12:3456:789a::1" )]
        [DataRow( "ff02::1" )]
        [DataRow( "::ffff:127.0.0.1" )]
        [DataRow( "::ffff:10.0.0.1" )]
        [DataRow( "::ffff:169.254.169.254" )]
        public void IsPublicAddress_WithNonPublicAddress_ReturnsFalse( string address )
        {
            Assert.IsFalse( RemoteImageDownloader.IsPublicAddress( IPAddress.Parse( address ) ) );
        }

        [TestMethod]
        [DataRow( "1.1.1.1" )]
        [DataRow( "8.8.8.8" )]
        [DataRow( "100.63.255.255" )]
        [DataRow( "100.128.0.1" )]
        [DataRow( "172.15.255.255" )]
        [DataRow( "172.32.0.1" )]
        [DataRow( "192.0.1.1" )]
        [DataRow( "192.169.0.1" )]
        [DataRow( "198.17.255.255" )]
        [DataRow( "198.20.0.1" )]
        [DataRow( "223.255.255.255" )]
        [DataRow( "2606:4700:4700::1111" )]
        [DataRow( "::ffff:8.8.8.8" )]
        public void IsPublicAddress_WithPublicAddress_ReturnsTrue( string address )
        {
            Assert.IsTrue( RemoteImageDownloader.IsPublicAddress( IPAddress.Parse( address ) ) );
        }
    }
}
