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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Integration.TestData;
using Rock.Tests.Lava.Shared;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Integration.Core.Lava.Filters
{
    [TestClass]
    public class EncodingFilterTests : LavaIntegrationTestBase
    {
        #region Filter Tests: Base64Encode (for BinaryFile) and ToBase64 (for text/binary data)

        /// <summary>
        /// Applying the Base64Encode filter to a BinaryFile object returns a Base64 encoded string.
        /// </summary>
        [TestMethod]
        public void Base64EncodeFilter_WithBinaryFileObjectParameter_ReturnsExpectedEncoding()
        {
            var rockContext = RockApp.Current.CreateRockContext();

            var contentChannelItem = new ContentChannelItemService( rockContext )
                .Queryable()
                .FirstOrDefault( x => x.ContentChannel.Name == "External Website Ads" && x.Title == "SAMPLE: Easter" );

            Assert.IsNotNull( contentChannelItem, "Required test data not found." );

            var values = new LavaDataDictionary { { "Item", contentChannelItem } };

            var input = @"
{%- assign image = Item | Attribute:'Image','Object' -%}
Base64Format: {{ image | Base64Encode }}<br/>
";

            var expectedOutput = "Base64Format: /9j/4AAQSkZJRgABAQEAAAAAAAD/{moreBase64Data}<br/>\n";

            var options = new LavaRenderOptions { MergeFields = values };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input, options );

                LavaAssert.Matches( expectedOutput, output, "{moreBase64Data}" );
            } );
        }

        [TestMethod]
        public void ToBase64Filter_WithBinaryDataParameter_ReturnsExpectedEncoding()
        {
            var rockContext = RockApp.Current.CreateRockContext();

            var contentChannelItem = new ContentChannelItemService( rockContext )
                .Queryable()
                .FirstOrDefault( x => x.ContentChannel.Name == "External Website Ads" && x.Title == "SAMPLE: Easter" );

            Assert.IsNotNull( contentChannelItem, "Required test data not found." );

            var values = new LavaDataDictionary { { "Item", contentChannelItem } };

            var input = @"
{%- assign image = Item | Attribute:'Image','Object' -%}
ToBase64: {{ image.DatabaseData.Content | ToBase64 }}<br/>
";

            var expectedOutput = "ToBase64: /9j/4AAQSkZJRgABAQEAAAAAAAD/{moreBase64Data}<br/>\n";

            var options = new LavaRenderOptions { MergeFields = values };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input, options );

                LavaAssert.Matches( expectedOutput, output, "{moreBase64Data}" );
            } );
        }

        #endregion

        #region IdHash Tests

        [TestMethod]
        public void ToIdHash_WithIntegerInput_ReturnsHashedValue()
        {
            var person = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker );

            var input = @"{{ <personId> | ToIdHash }}";
            input = input.Replace( "<personId>", person.Id.ToString() );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input );

                Assert.AreEqual( person.IdKey, output );
            } );
        }

        /// <summary>
        /// This is the documentation example for Lava Filter: ToIdHash.
        /// </summary>
        [TestMethod]
        public void ToIdHash_WithPersonEntityInput_ReturnsHashedValue()
        {
            var person = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker );

            var input = @"
{%- person where:'LastName == ""Decker"" && NickName ==""Ted""' -%}
Hello {{ person.NickName }}! Your Id is {{ person.Id }}, and your IdHash is '{{ person | ToIdHash }}'.
{%- endperson -%}
";

            // The person block's tags are trimmed, so the only newline left is
            // the one the template opens with.
            var expectedOutput = "\nHello Ted! Your Id is <Id>, and your IdHash is '<IdHash>'.\n";

            expectedOutput = expectedOutput
                .Replace( "<Id>", person.Id.ToString() )
                .Replace( "<IdHash>", person.IdKey );

            var options = new LavaRenderOptions { EnabledCommands = "RockEntity" };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input, options );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        /// <summary>
        /// This is the documentation example for Lava Filter: FromIdHash.
        /// </summary>
        [TestMethod]
        public void FromIdHash_WithPersonIdHashInput_ReturnsPersonId()
        {
            var person = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker );

            var input = @"
//- Get the IdHash for Ted Decker.
{%- person where:'LastName == ""Decker"" && NickName ==""Ted""' -%}
{%- assign idHash = person.IdKey -%}
{%- endperson -%}
Ted's IdHash is: {{ idHash }}.<br>
//- Use the IdHash to retrieve a Person entity.
{%- assign personFromHash = idHash | FromIdHash | PersonById -%}
Hello {{ personFromHash.NickName }}!
";

            // With the tags trimmed, the Lava comments and the person block
            // leave nothing behind, so the two lines run together.
            var expectedOutput = "Ted's IdHash is: <IdHash>.<br>Hello Ted!\n";

            expectedOutput = expectedOutput
                .Replace( "<IdHash>", person.IdKey );

            var options = new LavaRenderOptions { EnabledCommands = "RockEntity" };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input, options );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        #endregion
    }
}
