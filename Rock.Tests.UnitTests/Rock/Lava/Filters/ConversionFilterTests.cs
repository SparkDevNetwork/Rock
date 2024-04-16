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
using Rock.Tests.Shared;

namespace Rock.Tests.UnitTests.Lava
{
    [TestClass]
    public class ConversionFilterTests : LavaUnitTestBase
    {
        [TestMethod]
        public void AsGuidFilter_DocumentationExample_ReturnsExpectedOutput()
        {
            var input = @"
{% assign guidString1 = '8FEDC6EE-8630-41ED-9FC5-C7157FD1EAA4' %}
{% assign guidString2 = '8FEDC6EE863041ED9FC5C7157FD1EAA4' %}
<p>Value 1: {{ guidString1 }}</p>
<p>Value 2: {{ guidString2 }}</p>
{% if guidString1 != guidString2 %}
<p>Compared as Strings, these values are different.</p>
{% endif %}
{% assign guidValue1 = '8FEDC6EE-8630-41ED-9FC5-C7157FD1EAA4' | AsGuid %}
{% assign guidValue2 = '8FEDC6EE863041ED9FC5C7157FD1EAA4' | AsGuid %}
{% if guidValue1 == guidValue2 %}
<p>Compared as Guids, these values are the same.</p>
{% endif %}
";

            var expectedOutput = @"
<p>Value 1: 8FEDC6EE-8630-41ED-9FC5-C7157FD1EAA4</p>
<p>Value 2: 8FEDC6EE863041ED9FC5C7157FD1EAA4</p>
<p>Compared as Strings, these values are different.</p>
<p>Compared as Guids, these values are the same.</p>
";

            TestHelper.AssertTemplateOutput( expectedOutput.ToString(), input, ignoreWhitespace: true );
        }

        /// <summary>
        /// Common text representations of Guid values should return a Guid.
        /// </summary>
        [DataTestMethod]
        [DataRow( "FB464E73-9F0A-4D08-A95C-3AE8D1BC2344", DisplayName = "Standard Format" )]
        [DataRow( "FB464E739F0A4D08A95C3AE8D1BC2344", DisplayName = "No Separators" )]
        [DataRow( "00000000-0000-0000-0000-000000000000", DisplayName = "Empty Guid" )]
        public void AsGuidFilter_WithInputParsableAsGuid_ReturnsGuid( string input )
        {
            // Verify that the input can be converted to a Guid.
            var expectedResult = input.AsGuidOrNull();
            Assert.That.IsNotNull( expectedResult );

            // Verify the Lava filter output.
            TestHelper.AssertTemplateOutput( expectedResult.ToString(), "{{ '" + input + "' | AsGuid }}" );
        }

        /// <summary>
        /// Input that cannot be parsed as a Guid value should return an empty string.
        /// </summary>
        [DataTestMethod]
        [DataRow( "FB464E73-9F0A-4D08-A95C-3AE8D1BC234", DisplayName = "Invalid Length" )]
        [DataRow( "FB464E73^9F0A^4D08^A95C^3AE8D1BC234", DisplayName = "Invalid Separator" )]
        [DataRow( "ABCDEFGH-IJKL-MNOP-QRST-UVWXYZABCDE", DisplayName = "Invalid Values" )]
        public void AsGuidFilter_WithInputNotParsableAsGuid_ReturnsEmptyString( string input )
        {
            // Verify that the input cannot be converted to a Guid.
            var expectedResult = input.AsGuidOrNull();
            Assert.That.IsNull( expectedResult );

            // Verify the Lava filter output.
            TestHelper.AssertTemplateOutput( string.Empty, "{{ '" + input + "' | AsGuid }}" );
        }
    }

}
