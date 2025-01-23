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

namespace Rock.Tests.Field.Types
{
    /// <summary>
    /// Unit tests for the <see cref="StructureContentEditorFieldType"/> field type
    /// that do not require any database access.
    /// </summary>
    [TestClass]
    public class StructureContentEditorFieldTypeTests
    {
        /// <summary>
        /// Verify that <see cref="StructureContentEditorFieldType.FormatValue(System.Web.UI.Control, string, Dictionary{string, ConfigurationValue}, bool)"/>
        /// returns the content as HTML instead of returning the raw value.
        /// </summary>
        /// <seealso href="https://github.com/SparkDevNetwork/Rock/issues/4997"/>
        [TestMethod]
        public void FormatValue_ShouldReturnHtml()
        {
            var structureContentEditorField = new StructureContentEditorFieldType();
            var value = "{\"time\": 1652107072869, \"blocks\": [{\"id\": \"1\", \"type\": \"paragraph\", \"data\": {\"text\": \"hello world.\"}}], \"version\": \"2.24.3\"}";
            var expectedResult = "<p>hello world.</p>\r\n";

            Assert.That.AreEqual( expectedResult, structureContentEditorField.FormatValue( null, value, new Dictionary<string, ConfigurationValue>(), false ) );
        }
    }
}
