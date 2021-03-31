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

namespace Rock.Tests.Integration.Lava
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
                .FirstOrDefault( x=>x.ContentChannel.Name == "External Website Ads" && x.Title == "SAMPLE: Easter" );

            Assert.That.IsNotNull( contentChannelItem, "Required test data not found." );

            var values = new LavaDataDictionary { { "Item", contentChannelItem } };

            var input = @"
{% assign image = Item | Attribute:'Image','Object' %}
Base64Format: {{ image | Base64Encode }}<br/>
";

            var expectedOutput = @"/9j/4AAQSkZJRgABAQEAAAAAAAD/{moreBase64Data}";

            var options = new LavaTestRenderOptions() { MergeFields = values, Wildcards = new List<string> { "{moreBase64Data}" } };

            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        #endregion
    }
}
