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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

using Rock.Tests.Shared;
using Rock.Lava.Blocks;
using Rock.Lava;

namespace Rock.Tests.Integration.Lava
{
    [TestClass]
    public class RockEntityTests : LavaIntegrationTestBase
    {
        /// <summary>
        /// Tests the EventsCalendarItem to make sure that an item's EventItem and EventItem.Summary are returned.
        /// </summary>
        [TestMethod]
        [Ignore("This test requires specific test data that does not exist in the sample database.")]
        public void EventCalendarItemAllowsEventItemSummary()
        {
            RockEntityBlock.RegisterEntityCommands( LavaService.GetCurrentEngine() );

            var expectedOutput = @"
3 [2]: <br>
5 [3]: <br>
7 [4]: <br>
8 [5]: Scelerisque eleifend donec pretium vulputate sapien. Proin sed libero enim sed faucibus turpis in eu mi. Vel elit scelerisque mauris pellentesque pulvinar pellentesque habitant morbi. Egestas erat imperdiet sed euismod. Metus aliquam eleifend mi in.<br>
".Trim();

            var mergeFields = new Dictionary<string, object>();

            var lava = @"{% eventcalendaritem where:'EventCalendarId == 1' %}
{% for item in eventcalendaritemItems %}
{{ item.Id }} [{{ item.EventItemId }}]: {{ item.EventItem.Summary }}<br>
{% endfor %}
{% endeventcalendaritem %}";
            string output = lava.ResolveMergeFields( mergeFields, "RockEntity" ).Trim();

            Assert.That.AreEqualIgnoreNewline( expectedOutput, output );
        }

        [TestMethod]
        //[Ignore( "In the Fluid framework, this test incorrectly displays the same EventItemOccurrence on each pass through the loop." )]
        // This bug may be fixed, included in the next release: https://github.com/sebastienros/fluid/issues/317
        public void EntityCommandBlock_ContainingForBlock_ExecutesCorrectly()
        {
            var template = @"
{%- eventscheduledinstance eventid:'Rock Solid Finances Class' startdate:'2020-1-1' maxoccurrences:'25' daterange:'2m' -%}
    {%- for occurrence in EventItems -%}
        <b>Series {{forloop.index}}</b><br>

        {%- for item in occurrence -%}
            {%- if forloop.first -%}
                {{ item.Name }}
                <b>{{ item.DateTime | Date:'dddd' }} Series</b><br>
                <ol>
            {% endif %}

            <li>{{ item.DateTime | Date:'MMM d, yyyy' }} in {{ item.LocationDescription }}</li>

            {%- if forloop.last -%}
                </ol>
            {% endif %}
        {% endfor %}

    {% endfor %}
{% endeventscheduledinstance %}
";
            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = TestHelper.GetTemplateOutput( engine.EngineType, template );

                TestHelper.DebugWriteRenderResult( engine.EngineType, template, output );

                // Verify that the output contains series headings and relevant dates for both schedules.
                Assert.That.Contains( output, "<b>Series 1</b>" );
                Assert.That.Contains( output, "<li>Jan 4, 2020 in Meeting Room 1</li>" );
                Assert.That.Contains( output, "<b>Series 2</b>" );
                Assert.That.Contains( output, "<li>Jan 5, 2020 in Meeting Room 2</li>" );
            } );
        }

        [TestMethod]
        public void EntityCommandBlock_NestedInParentEntityCommandBlock_ExecutesCorrectly()
        {
            var template = @"
{%- contentchannelitem where:'ContentChannelId == 21' limit:'1000' sort:'StartDateTime desc' iterator:'MessageSeries' -%}
  {%- for item in MessageSeries -%}

      {%- contentchannelitem ids:'391,392' sort:'StartDateTime desc' iterator:'Messages' -%}
        {%- for message in Messages -%}
          {{ message.Title }}
        {%- endfor -%}
      {%- endcontentchannelitem -%}
      
  {%- endfor -%}
{%- endcontentchannelitem -%}
";
            TestHelper.ExecuteForActiveEngines( ( engine ) =>
            {
                var output = TestHelper.GetTemplateOutput( engine, template, engine.NewRenderContext( new List<string> { "All" } ) );

                TestHelper.DebugWriteRenderResult( engine.EngineType, template, output );

                // Verify that the output contains series headings and relevant dates for both schedules.
                //Assert.That.Contains( output, "<b>Series 1</b>" );
                //Assert.That.Contains( output, "<li>Jan 4, 2020 in Meeting Room 1</li>" );
                //Assert.That.Contains( output, "<b>Series 2</b>" );
                //Assert.That.Contains( output, "<li>Jan 5, 2020 in Meeting Room 2</li>" );
            } );
        }
    }
}
