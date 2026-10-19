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

using Rock.Model;

namespace Rock.Tests.Model
{
    [TestClass]
    public class LocationFormattedHtmlAddressTests
    {
        [TestMethod]
        public void FormattedHtmlAddress_WithHtmlInAddress_EncodesTextAndKeepsLineBreaks()
        {
            var location = new TestLocation( "1 Main <b>x</b>\nPhoenix" );

            Assert.AreEqual( "1 Main &lt;b&gt;x&lt;/b&gt;<br>Phoenix", location.FormattedHtmlAddress );
        }

        [TestMethod]
        public void FormattedHtmlAddress_WithCrLf_ConvertsToBr()
        {
            var location = new TestLocation( "A\r\nB" );

            Assert.AreEqual( "A<br>B", location.FormattedHtmlAddress );
        }

        [TestMethod]
        public void FormattedHtmlAddress_WithAmpersandAndApostrophe_EncodesOnce()
        {
            var location = new TestLocation( "O'Hare & 5th" );

            Assert.AreEqual( "O&#39;Hare &amp; 5th", location.FormattedHtmlAddress );
        }

        [TestMethod]
        public void FormattedHtmlAddress_WithNullOrEmpty_ReturnsEmpty()
        {
            Assert.AreEqual( string.Empty, new TestLocation( null ).FormattedHtmlAddress );
            Assert.AreEqual( string.Empty, new TestLocation( string.Empty ).FormattedHtmlAddress );
        }

        /// <summary>
        /// A location that returns a fixed address so the test does not need
        /// the address format defined values from the database.
        /// </summary>
        private class TestLocation : Location
        {
            private readonly string _formattedAddress;

            public TestLocation( string formattedAddress )
            {
                _formattedAddress = formattedAddress;
            }

            public override string FormattedAddress => _formattedAddress;
        }
    }
}
