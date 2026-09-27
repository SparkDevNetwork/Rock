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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Lava;
using Rock.Tests.Integration.TestData;
using Rock.Tests.Integration.TestData.Core;
using Rock.Tests.Integration.TestData.Crm;
using Rock.Tests.Lava.Shared;
using Rock.Tests.Shared.Constants;

namespace Rock.Tests.Integration.Core.Lava.Filters
{
    /// <summary>
    /// Tests for Lava Filters categorized as "Miscellaneous".
    /// </summary>
    /// <remarks>
    /// These tests require the standard Rock sample data set to be present in the target database.
    /// </remarks>
    [TestClass]
    public class PersonalizationFilterTests : LavaIntegrationTestBase
    {
        private const string PersistedDatasetGuid = "00693C79-3BAC-4629-AE9D-9E1E355534B9";
        private const string PersistedDatasetKey = "MyContentChannelItemsDataset";

        [ClassInitialize]
        public static void ClassInitialize( TestContext context )
        {
            PersonalizationDataManager.Instance.AddDataForTestPersonalization();

            var contentChannelItems = new List<string>
            {
                "Of Myths and Money",
                "Of Faith and Firsts",
                "Are You Dealing With Insecurity?",
                "Hallelujah!",
                "The Secret That Could Cost You Your Marriage",
                "How To Make Your Marriage Better Today",
                "Extended Family",
                "Immediate Family",
                "Momentum at Home",
                "Momentum at Work",
                "Rich Fool",
                "Two Debtors"
            };

            PersistedDatasetDataManager.Instance.AddDatasetForContentChannelItemInfo( PersistedDatasetGuid.AsGuid(),
                PersistedDatasetKey,
                contentChannelItems );
        }

        #region Support Methods

        /// <summary>
        /// Builds the render options for a template that reads CurrentPerson.
        /// </summary>
        /// <remarks>
        /// CurrentPerson is an ordinary merge field. The predecessor helper hid
        /// that behind a WithCurrentPerson option, which also hid that the person
        /// is read from the database as the options are built.
        /// </remarks>
        /// <param name="personIdentifier">The identifier of the person to render as.</param>
        /// <returns>The render options.</returns>
        private static LavaRenderOptions GetOptionsForCurrentPerson( string personIdentifier )
        {
            var person = TestDataHelper.GetTestPerson( personIdentifier );

            return new LavaRenderOptions
            {
                EnabledCommands = "rockentity",
                MergeFields = new LavaDataDictionary { { "CurrentPerson", person } }
            };
        }

        #endregion

        #region AppendSegments

        [TestMethod]
        public void AppendSegments_DocumentationExample_ProducesExpectedResult()
        {
            var template = @"
<h1>Entity Command Example</h1>
{%- contentchannel where:'Name == ""Messages""' -%}
    {%- assign channelId = contentchannel.Id -%}
    {%- contentchannelitem where:'ContentChannelId == {{ contentchannel.Id }}' iterator:'Items' -%}
    {%- assign segments = Items | AppendSegments:'MARRIED' | Sort:'Title' -%}
     <p>{{ CurrentPerson.NickName }}, you might be interested in these messages on the topic of Marriage:</p>
    <ul>
        {%- for item in segments -%}
            {%- if item.IsInSegment == true -%}
                <li>{{ item.Title }} [{{ item.MatchingSegments }}]</li>
            {%- endif -%}
        {%- endfor -%}
    </ul>
    {%- endcontentchannelitem -%}
{%- endcontentchannel -%}

<h1>Persisted Dataset Example</h1>
{%- assign matchedSegmentItems = 'MyContentChannelItemsDataset' | PersistedDataset | AppendSegments -%}
<p>Recommendations for {{ CurrentPerson.NickName }}:</p>
<ul>
  {%- for item in matchedSegmentItems -%}
    <li>{{ item.Title }} - {{ item.IsInSegment }}{%- if item.MatchingSegments > '' -%} [{{ item.MatchingSegments }}]{%- endif -%}</li>
  {%- endfor -%}
</ul>
";

            // Replace placeholder values to ensure that keys are correct.
            template = template.Replace( "MARRIED", PersonalizationDataManager.Constants.SegmentKeyMarried );
            template = template.Replace( "MyContentChannelItemsDataset", PersistedDatasetKey );

            /*
                9/26/26 - CLAUDE

                The indentation and the run-together lines below are what the
                template actually produces. The whitespace control in this
                template is applied unevenly, so the closing tag of one block
                ends up on the same line as the text that follows it, and the
                list items carry the indentation of the for loop that wrote them.

                The predecessor assertion stripped every whitespace character
                before comparing, so the expected value could be written as
                well-formed HTML that the template has never emitted.

                Reason: The expected value has to be what the template renders.
            */
            var expectedOutput = "\n"
                + "<h1>Entity Command Example</h1><p>Ted, you might be interested in these messages on the topic of Marriage:</p>\n"
                + "    <ul><li>Extended Family [Married]</li><li>How To Make Your Marriage Better Today [Married]</li><li>Immediate Family [Married]</li><li>The Secret That Could Cost You Your Marriage [Married]</li></ul>\n"
                + "    <h1>Persisted Dataset Example</h1><p>Recommendations for Ted:</p>\n"
                + "<ul><li>Of Myths and Money - true[Attender, Has Given]</li><li>Of Faith and Firsts - true[Attender, Has Given]</li><li>Are You Dealing With Insecurity? - false</li><li>Hallelujah! - false</li><li>The Secret That Could Cost You Your Marriage - true[Married]</li><li>How To Make Your Marriage Better Today - true[Married]</li><li>Extended Family - true[Married, Small Group]</li><li>Immediate Family - true[Married]</li><li>Momentum At Home - true[Small Group]</li><li>Momentum At Work - false</li><li>Rich Fool - false</li><li>Two Debtors - false</li></ul>\n";

            var options = GetOptionsForCurrentPerson( TestGuids.TestPeople.TedDecker );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template, options );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        [TestMethod]
        [DataRow( null, "" )]
        [DataRow( "", "" )]
        [DataRow( "abc", "abc" )]
        public void AppendSegments_InputIsInvalid_ReturnsInputUnchanged( object input, string expectedValue )
        {
            var template = @"
{%- assign segments = input | AppendSegments -%}
{{ input }}
";

            // With the assign tag trimmed, the value is all that is left apart
            // from the newline the template ends with.
            var expectedOutput = expectedValue + "\n";

            var options = new LavaRenderOptions
            {
                MergeFields = new LavaDataDictionary { { "input", input } }
            };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template, options );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        [TestMethod]
        public void AppendSegments_InputIsEntityCollection_ShowsExpectedOutput()
        {
            var template = @"
{%- contentchannel where:'Name == ""Messages""' -%}
    {%- assign channelId = contentchannel.Id -%}
    {%- contentchannelitem where:'ContentChannelId == {{channelId}}' iterator:'Items' -%}
    {%- assign segments = Items | AppendSegments | Sort:'Title' -%}
<h1>{{ CurrentPerson.NickName }}'s Personalized {{ contentchannel.Name }}</h1>
    <ul>
        {%- for item in segments -%}
            {%- if item.IsInSegment == true -%}
                <li>{{ item.Title }} ({{ item.MatchingSegments }})</li>
            {%- endif -%}
        {%- endfor -%}
    </ul>
    {%- endcontentchannelitem -%}
{%- endcontentchannel -%}
";

            // The last line is four spaces, left behind by the closing tag of
            // the outer block, so this is written with escapes rather than as a
            // verbatim literal where it would be invisible.
            var expectedOutput = "<h1>Ted's Personalized Messages</h1>\n"
                + "    <ul><li>Extended Family (Married, Small Group)</li><li>How To Make Your Marriage Better Today (Married)</li><li>Immediate Family (Married)</li><li>Momentum At Home (Small Group)</li><li>Of Faith and Firsts (Attender, Has Given)</li><li>Of Myths and Money (Attender, Has Given)</li><li>The Secret That Could Cost You Your Marriage (Married)</li></ul>\n"
                + "    ";

            var options = GetOptionsForCurrentPerson( TestGuids.TestPeople.TedDecker );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template, options );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        [TestMethod]
        public void AppendSegments_WithSegmentParameter_ShowsMatchesForSpecifiedSegmentOnly()
        {
            var template = @"
{%- contentchannel where:'Name == ""Messages""' -%}
    {%- contentchannelitem where:'ContentChannelId == {{ contentchannel.Id }}' iterator:'Items' -%}
    {%- assign segments = Items | AppendSegments:'SINGLE' | Sort:'Title' -%}
    <h1>{{ CurrentPerson.NickName }}, you might be interested in these messages on the topic of Singleness:</h1>
    <ul>
        {%- for item in segments -%}
            {%- if item.IsInSegment == true -%}
                <li>{{ item.Title }} ({{ item.MatchingSegments }})</li>
            {%- endif -%}
        {%- endfor -%}
    </ul>
    {%- endcontentchannelitem -%}
{%- endcontentchannel -%}
";

            // Replace placeholder values to ensure that keys are correct.
            template = template.Replace( "SINGLE", PersonalizationDataManager.Constants.SegmentKeySingle );

            // This template trims its leading newline, and the output ends with
            // four spaces and no newline, so escapes keep both visible.
            var expectedOutput = "<h1>Mariah, you might be interested in these messages on the topic of Singleness:</h1>\n"
                + "    <ul><li>Are You Dealing With Insecurity? (Single)</li><li>Hallelujah! (Single)</li><li>Momentum At Home (Single)</li><li>Momentum At Work (Single)</li></ul>\n"
                + "    ";

            var options = GetOptionsForCurrentPerson( TestGuids.TestPeople.MariahJackson );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template, options );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        [TestMethod]
        public void AppendSegments_InputIsPersistedDataset_ShowsMatchedSegmentsForCurrentUser()
        {
            var template = @"
{%- assign matchedSegmentItems = '$PersistedDatasetKey' | PersistedDataset | AppendSegments -%}
<h1>Recommendations for {{ CurrentPerson.NickName }}</h1>
<ul>
  {%- for item in matchedSegmentItems -%}
    <li>{{ item.Title }} - {{ item.IsInSegment }}{%- if item.MatchingSegments > '' -%} [{{ item.MatchingSegments }}]{%- endif -%}</li>
  {%- endfor -%}
</ul>
";

            template = template.Replace( "$PersistedDatasetKey", PersistedDatasetKey );

            // The opening <ul> runs into the first list item because the for
            // loop's whitespace control trims the newline before it.
            var expectedOutput = "<h1>Recommendations for Ted</h1>\n"
                + "<ul><li>Of Myths and Money - true[Attender, Has Given]</li><li>Of Faith and Firsts - true[Attender, Has Given]</li><li>Are You Dealing With Insecurity? - false</li><li>Hallelujah! - false</li><li>The Secret That Could Cost You Your Marriage - true[Married]</li><li>How To Make Your Marriage Better Today - true[Married]</li><li>Extended Family - true[Married, Small Group]</li><li>Immediate Family - true[Married]</li><li>Momentum At Home - true[Small Group]</li><li>Momentum At Work - false</li><li>Rich Fool - false</li><li>Two Debtors - false</li></ul>\n";

            var options = GetOptionsForCurrentPerson( TestGuids.TestPeople.TedDecker );

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template, options );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        #endregion
    }
}
