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

using Rock.Tests.Integration.Core;
using Rock.Tests.Integration.Crm.Personalization;
using Rock.Tests.Shared;
using Rock.Tests.Shared.Lava;

namespace Rock.Tests.Integration.Modules.Core.Lava.Filters
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
            {% endif -%}
        {%- endfor -%}
    </ul>
    {%- endcontentchannelitem -%}
{%- endcontentchannel -%}

<h1>Persisted Dataset Example</h1>
{% assign matchedSegmentItems = 'MyContentChannelItemsDataset' | PersistedDataset | AppendSegments %}
<p>Recommendations for {{ CurrentPerson.NickName }}:</p>
<ul>
  {%- for item in matchedSegmentItems -%}
    <li>{{ item.Title }} - {{ item.IsInSegment }}{% if item.MatchingSegments > '' %} [{{ item.MatchingSegments }}]{% endif %}</li>
  {% endfor -%}
</ul>
";

            // Replace placeholder values to ensure that keys are correct.
            template = template.Replace( "MARRIED", PersonalizationDataManager.Constants.SegmentKeyMarried );
            template = template.Replace( "MyContentChannelItemsDataset", PersistedDatasetKey );

            var expectedOutput = @"
<h1>Entity Command Example</h1>
<p>Ted, you might be interested in these messages on the topic of Marriage:</p>
<ul>
  <li>Extended Family [Married]</li>
  <li>How To Make Your Marriage Better Today [Married]</li>
  <li>Immediate Family [Married]</li>
  <li>The Secret That Could Cost You Your Marriage [Married]</li>
</ul>

<h1>Persisted Dataset Example</h1>
<p>Recommendations for Ted:</p>
<ul>
  <li>Of Myths and Money - true [Attender, Has Given]</li>
  <li>Of Faith and Firsts - true [Attender, Has Given]</li>
  <li>Are You Dealing With Insecurity? - false</li>
  <li>Hallelujah! - false</li>
  <li>The Secret That Could Cost You Your Marriage - true [Married]</li>
  <li>How To Make Your Marriage Better Today - true [Married]</li>
  <li>Extended Family - true [Married, Small Group]</li>
  <li>Immediate Family - true [Married]</li>
  <li>Momentum At Home - true [Small Group]</li>
  <li>Momentum At Work - false</li>
  <li>Rich Fool - false</li>
  <li>Two Debtors - false</li>
</ul>
";

            TestHelper.AssertTemplateOutput( expectedOutput,
                template,
                LavaTestRenderOptions.WebApplicationSession
                    .WithEnabledCommands( "rockentity" )
                    .WithCurrentPerson( TestGuids.TestPeople.TedDecker ) );
        }

        [DataTestMethod]
        [DataRow( null, "" )]
        [DataRow( "", "" )]
        [DataRow( "abc", "abc" )]
        public void AppendSegments_InputIsInvalid_ReturnsInputUnchanged( object input, string expectedOutput )
        {
            var template = @"
{% assign segments = input | AppendSegments %}
{{ input }}
";

            TestHelper.AssertTemplateOutput( expectedOutput,
                template,
                LavaTestRenderOptions.WebApplicationSession
                    .WithContextVariable( "input", input ) );
        }

        [TestMethod]
        public void AppendSegments_InputIsEntityCollection_ShowsExpectedOutput()
        {
            var template = @"
{% contentchannel where:'Name == ""Messages""' %}
    {%- assign channelId = contentchannel.Id -%}
    {%- contentchannelitem where:'ContentChannelId == {{channelId}}' iterator:'Items' -%}
    {%- assign segments = Items | AppendSegments | Sort:'Title' -%}
<h1>{{ CurrentPerson.NickName }}'s Personalized {{ contentchannel.Name }}</h1>
    <ul>
        {%- for item in segments -%}
            {%- if item.IsInSegment == true -%}
                <li>{{ item.Title }} ({{ item.MatchingSegments }})</li>
            {% endif -%}
        {%- endfor -%}
    </ul>
    {%- endcontentchannelitem -%}
{% endcontentchannel %}
";

            var expectedOutput = @"
<h1>Ted's Personalized Messages</h1>
<ul>
    <li>Extended Family (Married, Small Group)</li>
    <li>How To Make Your Marriage Better Today (Married)</li>
    <li>Immediate Family (Married)</li>
    <li>Momentum At Home (Small Group)</li>
    <li>Of Faith and Firsts (Attender, Has Given)</li>
    <li>Of Myths and Money (Attender, Has Given)</li>
    <li>The Secret That Could Cost You Your Marriage (Married)</li>
</ul>
";

            TestHelper.AssertTemplateOutput( expectedOutput,
                template,
                LavaTestRenderOptions.WebApplicationSession
                    .WithEnabledCommands( "rockentity" )
                    .WithCurrentPerson( TestGuids.TestPeople.TedDecker ) );
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
            {% endif -%} 
        {%- endfor -%}
    </ul>
    {%- endcontentchannelitem -%}
{%- endcontentchannel -%}
";

            // Replace placeholder values to ensure that keys are correct.
            template = template.Replace( "SINGLE", PersonalizationDataManager.Constants.SegmentKeySingle );

            var expectedOutput = @"
<h1>Mariah, you might be interested in these messages on the topic of Singleness:</h1>
<ul>
    <li>Are You Dealing With Insecurity? (Single)</li>
    <li>Hallelujah! (Single)</li>
    <li>Momentum At Home (Single)</li>
    <li>Momentum At Work (Single)</li>
</ul>
";

            TestHelper.AssertTemplateOutput( expectedOutput,
                template,
                LavaTestRenderOptions.WebApplicationSession
                    .WithEnabledCommands( "rockentity" )
                    .WithCurrentPerson( TestGuids.TestPeople.MariahJackson ) );
        }

        [TestMethod]
        public void AppendSegments_InputIsPersistedDataset_ShowsMatchedSegmentsForCurrentUser()
        {
            var template = @"
{% assign matchedSegmentItems = '$PersistedDatasetKey' | PersistedDataset | AppendSegments %}
<h1>Recommendations for {{ CurrentPerson.NickName }}</h1>
<ul>
  {%- for item in matchedSegmentItems -%}
    <li>{{ item.Title }} - {{ item.IsInSegment }}{% if item.MatchingSegments > '' %} [{{ item.MatchingSegments }}]{% endif %}</li>
  {% endfor -%}
</ul>
";

            template = template.Replace( "$PersistedDatasetKey", PersistedDatasetKey );

            var expectedOutput = @"
<h1>Recommendations for Ted</h1>
<ul>
  <li>Of Myths and Money - true [Attender, Has Given]</li>
  <li>Of Faith and Firsts - true [Attender, Has Given]</li>
  <li>Are You Dealing With Insecurity? - false</li>
  <li>Hallelujah! - false</li>
  <li>The Secret That Could Cost You Your Marriage - true [Married]</li>
  <li>How To Make Your Marriage Better Today - true [Married]</li>
  <li>Extended Family - true [Married, Small Group]</li>
  <li>Immediate Family - true [Married]</li>
  <li>Momentum At Home - true [Small Group]</li>
  <li>Momentum At Work - false</li>
  <li>Rich Fool - false</li>
  <li>Two Debtors - false</li>
</ul>
";

            TestHelper.AssertTemplateOutput( expectedOutput,
                template,
                LavaTestRenderOptions.WebApplicationSession
                    .WithEnabledCommands( "rockentity" )
                    .WithCurrentPerson( TestGuids.TestPeople.TedDecker ) );
        }

        #endregion
    }
}
