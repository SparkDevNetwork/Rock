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
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Core.Lava
{
    [TestClass]
    public class EncodingFilterTests : LavaIntegrationTestBase
    {
        #region Filter Tests: Base64Encode (for BinaryFile)

        /// <summary>
        /// Applying the Base64Encode filter to a BinaryFile object returns a Base64 encoded string.
        /// </summary>
        [TestMethod]
        public void Base64EncodeFilter_WithBinaryFileObjectParameter_ReturnsExpectedEncoding()
        {
            var rockContext = new RockContext();

            var contentChannelItem = new ContentChannelItemService( rockContext )
                .Queryable()
                .FirstOrDefault( x => x.ContentChannel.Name == "External Website Ads" && x.Title == "SAMPLE: Easter" );

            Assert.That.IsNotNull( contentChannelItem, "Required test data not found." );

            var values = new LavaDataDictionary { { "Item", contentChannelItem } };

            var input = @"
{% assign image = Item | Attribute:'Image','Object' %}
Base64Format: {{ image | Base64Encode }}<br/>
";

            var expectedOutput = @"Base64Format: /9j/4AAQSkZJRgABAQEAAAAAAAD/{moreBase64Data}<br/>";

            var options = new LavaTestRenderOptions() { MergeFields = values, Wildcards = new List<string> { "{moreBase64Data}" } };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        #endregion

        #region IdHash Tests

        [TestMethod]
        public void ToIdHash_WithIntegerInput_ReturnsHashedValue()
        {
            var person = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker );

            var input = @"{{ <personId> | ToIdHash }}";
            input = input.Replace( "<personId>", person.Id.ToString() );

            TestHelper.AssertTemplateOutput( person.IdKey, input );
        }

        /// <summary>
        /// This is the documentation example for Lava Filter: ToIdHash.
        /// </summary>
        [TestMethod]
        public void ToIdHash_WithPersonEntityInput_ReturnsHashedValue()
        {
            var person = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker );

            var input = @"
{% person where:'LastName == ""Decker"" && NickName ==""Ted""' %}
Hello {{ person.NickName }}! Your Id is {{ person.Id }}, and your IdHash is '{{ person | ToIdHash }}'.
{% endperson %}
";

            var expectedOutput = @"
Hello Ted! Your Id is <Id>, and your IdHash is'<IdHash>'.
";
            expectedOutput = expectedOutput
                .Replace( "<Id>", person.Id.ToString() )
                .Replace( "<IdHash>", person.IdKey );

            expectedOutput = expectedOutput.Replace( "<personId>", person.Guid.ToString() );
            var options = new LavaTestRenderOptions() { EnabledCommands = "RockEntity" };
            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        [DataTestMethod]
        [DataRow( "" )]
        [DataRow( "abc" )]
        [DataRow( "123abc" )]
        public void ToIdHash_WithNonIntegerInput_ReturnsEmptyOutput( string inputHash )
        {
            var input = @"{{ '<input>' | ToIdHash }}";
            input = input.Replace( "<input>", inputHash );

            TestHelper.AssertTemplateOutput( string.Empty, input );
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
{% person where:'LastName == ""Decker"" && NickName ==""Ted""' %}
{% assign idHash = person.IdKey %}
{% endperson %}
Ted's IdHash is: {{ idHash }}.<br>
//- Use the IdHash to retrieve a Person entity.
{% assign personFromHash = idHash | FromIdHash | PersonById %}
Hello {{ personFromHash.NickName }}!
";

            var expectedOutput = @"
Ted's IdHash is: <IdHash>.<br>
Hello Ted!
";
            expectedOutput = expectedOutput
                .Replace( "<IdHash>", person.IdKey );

            var options = new LavaTestRenderOptions() { EnabledCommands = "RockEntity" };
            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        [DataTestMethod]
        [DataRow( "" )]
        [DataRow( "123" )]
        [DataRow( "abc" )]
        public void FromIdHash_WithInvalidHashInput_ReturnsEmptyOutput( string inputHash )
        {
            var input = @"{{ '<input>' | FromIdHash }}";
            input = input.Replace( "<input>", inputHash );

            TestHelper.AssertTemplateOutput( string.Empty, input );
        }

        #endregion
    }
}
