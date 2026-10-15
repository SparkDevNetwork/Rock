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

using Newtonsoft.Json.Linq;

using Rock.Lava.Shortcodes;

namespace Rock.Tests.Lava.Shortcodes
{
    [TestClass]
    public class NetworkGraphShortcodeTests
    {
        private const string MaliciousLabel = "</script><img src=x onerror=alert('x')> & \"y\"";

        [TestMethod]
        public void SerializeOptions_WithHtmlInLabel_DoesNotEmitHtmlCharacters()
        {
            var options = new
            {
                Nodes = new[] { new { Id = "1", Label = MaliciousLabel } }
            };

            var json = NetworkGraphShortcode.SerializeOptions( options );

            Assert.IsFalse( json.Contains( "<" ), json );
            Assert.IsFalse( json.Contains( ">" ), json );
            Assert.IsFalse( json.Contains( "&" ), json );
            Assert.IsFalse( json.Contains( "'" ), json );
            Assert.IsFalse( json.Contains( "</script>" ), json );
        }

        [TestMethod]
        public void SerializeOptions_WithHtmlInLabel_PreservesLabelValueAndCamelCase()
        {
            var options = new
            {
                BaseNodeSize = 10,
                Nodes = new[] { new { Id = "1", Label = MaliciousLabel } }
            };

            var json = NetworkGraphShortcode.SerializeOptions( options );
            var parsed = JObject.Parse( json );

            Assert.AreEqual( 10, parsed["baseNodeSize"].Value<int>() );
            Assert.AreEqual( MaliciousLabel, parsed["nodes"][0]["label"].Value<string>() );
        }
    }
}
