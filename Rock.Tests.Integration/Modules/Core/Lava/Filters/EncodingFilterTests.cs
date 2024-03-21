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
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Lava;
using Rock.Lava.Fluid;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.Lava;

namespace Rock.Tests.Integration.Modules.Core.Lava.Filters
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

        #region Encryption Tests

        [TestMethod]
        public void Decrypt_DocumentationExample_ReturnsExpectedOutput()
        {
            var text = "Hello there!";
            var encryptedString = Rock.Security.Encryption.EncryptString( text );

            var inputTemplate = @"{{ '<input>' | Decrypt }}";
            inputTemplate = inputTemplate.Replace( "<input>", encryptedString );

            TestHelper.AssertTemplateOutput( text, inputTemplate );
        }

        [TestMethod]
        public void Encrypt_DocumentationExample_ReturnsExpectedOutput()
        {
            // The same input text returns a different encrypted string on each execution, because 
            // the encryption method (AES) includes an initialization vector (IV) to randomize the result.
            var inputTemplate = @"
{% assign encryptedText = 'This is my secret!' | Encrypt %}
<p>The encrypted message is: {{ encryptedText }}</p>
{% assign decryptedText = encryptedText | Decrypt %}
<p>The decrypted message is: {{ decryptedText }}</p>
";

            var expectedOutput = @"
The encrypted message is: *
The decrypted message is: This is my secret!
";

            TestHelper.AssertTemplateOutput( typeof(FluidEngine), expectedOutput, inputTemplate, new LavaTestRenderOptions { Wildcards = new List<string> { "*" } } );
        }

        [DataTestMethod]
        [DataRow( "" )]
        [DataRow( null )]
        [DataRow( "This is my secret!" )]
        public void Encrypt_WithStringInput_ReturnsEncryptedOutput( string input )
        {
            // Verify that the filter has transformed the input in some way.
            var inputTemplate1 = @"
{% assign encryptedText = '<input>' | Encrypt %}
{{ encryptedText }}
";

            var output1 = TestHelper.GetTemplateOutput( typeof( FluidEngine ), inputTemplate1 );

            Assert.That.AreNotEqual( output1?.RemoveWhiteSpace(), input?.RemoveWhiteSpace() );

            // Verify that the input text can be encrypted and decrypted successfully.
            var inputTemplate2 = @"
{% assign encryptedText = '<input>' | Encrypt %}
{% assign decryptedText = encryptedText | Decrypt %}
{{ decryptedText }}
";

            inputTemplate2 = inputTemplate2.Replace( "<input>", input );

            TestHelper.AssertTemplateOutput( typeof( FluidEngine ), input, inputTemplate2 );
        }

        #endregion
    }
}
