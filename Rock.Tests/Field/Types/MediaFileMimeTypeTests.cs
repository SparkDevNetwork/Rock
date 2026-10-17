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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Field.Types;

namespace Rock.Tests.Field.Types
{
    /// <summary>
    /// Unit tests for the MIME type checks used by the <see cref="VideoFileFieldType"/>
    /// and <see cref="AudioFileFieldType"/> players that do not require any
    /// database access.
    /// </summary>
    [TestClass]
    public class MediaFileMimeTypeTests
    {
        [TestMethod]
        [DataRow( "video/mp4" )]
        [DataRow( "video/quicktime" )]
        [DataRow( "VIDEO/MP4" )]
        [DataRow( "video/x-ms-wmv" )]
        public void GetSafeVideoMimeType_WithPlainVideoType_ReturnsSameValue( string mimeType )
        {
            Assert.AreEqual( mimeType, VideoFileFieldType.GetSafeVideoMimeType( mimeType ) );
        }

        [TestMethod]
        [DataRow( "video/mp4'><b>TH120-marker</b>" )]
        [DataRow( "video/mp4;x=\"'><img src=x onerror=alert(1)>\"" )]
        [DataRow( "video/mp4; codecs=avc1" )]
        [DataRow( "video/mp4\n" )]
        [DataRow( "application/octet-stream" )]
        [DataRow( "audio/mpeg" )]
        [DataRow( "" )]
        [DataRow( null )]
        public void GetSafeVideoMimeType_WithOtherValue_ReturnsEmptyString( string mimeType )
        {
            Assert.AreEqual( string.Empty, VideoFileFieldType.GetSafeVideoMimeType( mimeType ) );
        }

        [TestMethod]
        [DataRow( "audio/mpeg" )]
        [DataRow( "audio/x-wav" )]
        [DataRow( "AUDIO/MP4" )]
        public void GetSafeAudioMimeType_WithPlainAudioType_ReturnsSameValue( string mimeType )
        {
            Assert.AreEqual( mimeType, AudioFileFieldType.GetSafeAudioMimeType( mimeType ) );
        }

        [TestMethod]
        [DataRow( "audio/mpeg'><b>TH120-marker</b>" )]
        [DataRow( "audio/mpeg; rate=44100" )]
        [DataRow( "audio/mpeg\n" )]
        [DataRow( "video/mp4" )]
        [DataRow( "" )]
        [DataRow( null )]
        public void GetSafeAudioMimeType_WithOtherValue_ReturnsEmptyString( string mimeType )
        {
            Assert.AreEqual( string.Empty, AudioFileFieldType.GetSafeAudioMimeType( mimeType ) );
        }
    }
}
