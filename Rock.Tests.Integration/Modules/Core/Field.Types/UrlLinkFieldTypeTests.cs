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

using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Field;
using Rock.Field.Types;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

using static Rock.Field.Types.UrlLinkFieldType;

namespace Rock.Tests.Integration.Modules.Core.Field.Types
{
    [TestClass]
    public class UrlLinkFieldTypeTests : DatabaseTestsBase
    {
        [TestMethod]
        public void FormatValue_ShouldReturnCompleteATag()
        {
            var urlLinkField = new UrlLinkFieldType();
            var expectedUrl = "http://test.com/test";
            var expectedResult = $@"<a href='{expectedUrl}'>{expectedUrl}</a>";

            Assert.That.AreEqual( expectedResult, urlLinkField.FormatValue( null, expectedUrl, new Dictionary<string, ConfigurationValue>(), false ) );

            var configValues = new Dictionary<string, ConfigurationValue>
            {
                { ConfigurationKey.ShouldAlwaysShowCondensed, new ConfigurationValue("false") }
            };
            Assert.That.AreEqual( expectedUrl, urlLinkField.FormatValue( null, expectedUrl, configValues, true ) );
        }

        [TestMethod]
        public void FormatValue_ShouldReturnUrl()
        {
            var urlLinkField = new UrlLinkFieldType();
            var expectedUrl = "http://test.com/test";

            Assert.That.AreEqual( expectedUrl, urlLinkField.FormatValue( null, expectedUrl, new Dictionary<string, ConfigurationValue>(), true ) );

            var configValues = new Dictionary<string, ConfigurationValue>
            {
                { ConfigurationKey.ShouldAlwaysShowCondensed, new ConfigurationValue("false") }
            };
            Assert.That.AreEqual( expectedUrl, urlLinkField.FormatValue( null, expectedUrl, configValues, true ) );
        }

        [TestMethod]
        public void FormatValue_ShouldAlwaysReturnUrl()
        {
            var urlLinkField = new UrlLinkFieldType();
            var expectedUrl = "http://test.com/test";
            var configValues = new Dictionary<string, ConfigurationValue>
            {
                { ConfigurationKey.ShouldAlwaysShowCondensed, new ConfigurationValue("true") }
            };

            Assert.That.AreEqual( expectedUrl, urlLinkField.FormatValue( null, expectedUrl, configValues, true ) );
            Assert.That.AreEqual( expectedUrl, urlLinkField.FormatValue( null, expectedUrl, configValues, false ) );
        }
    }
}
