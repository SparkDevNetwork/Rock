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

using Rock.Lava;
using Rock.Lava.Fluid;
using Rock.Tests.Integration.Modules.Core.Lava;
using Rock.Tests.Shared;
using Rock.Tests.Shared.Lava;

namespace Rock.Tests.Integration.BugFixes
{
    /// <summary>
    /// Tests that verify specific bug fixes for a Rock version.
    /// </summary>
    /// <remarks>
    /// These tests are developed to verify bugs and fixes that are difficult or time-consuming to reproduce.
    /// They are only relevant to the Rock version in which the bug is fixed, and should be removed in subsequent versions.
    /// </remarks>
    /// 
    [TestClass]
    [RockObsolete( "1.16" )]
    public class BugFixVerificationTests_v16 : LavaIntegrationTestBase
    {
        private LavaIntegrationTestHelper _TestHelper = LavaIntegrationTestHelper.CurrentInstance;

        /// <summary>
        /// Verifies the resolution of Issue #5389.
        /// </summary>
        [TestMethod]
        public void Issue5389_ChildContentChannelsProperty_IsAvailableInLava()
        {
            /* Issue:
             * The ContentChannel.ChildContentChannels property is visible in the Model Map,
             * but is not consistently accessible in Lava.
             * For details, see https://github.com/SparkDevNetwork/Rock/issues/5389.
             * 
             * Resolution:
             * The ChildContentChannels property should be accessible in Lava using
             * standard property notation.
             */

            var input = @"
{% contentchannel where:'[Name] == ""Messages""' %}
    Dot Notation: {{ contentchannel.ChildContentChannels | Size }}<br>
    {% assign childChannels = contentchannel | Property:'ChildContentChannels' %}
    Property Filter: {{ childChannels | Size }}
{% endcontentchannel %}
";

            var expectedOutput = @"
Dot Notation: 1
<br>
Property Filter: 1
";

            var options = new LavaTestRenderOptions
            {
                EnabledCommands = "RockEntity"
            };
            TestHelper.AssertTemplateOutput( expectedOutput, input, options );
        }

        /// <summary>
        /// Verifies the resolution of a specific Issue.
        /// </summary>
        [TestMethod]
        public void Issue5560_LavaCommentsDisplayedInOutput()
        {
            /* The Lava Engine may render inline comments to output where an unmatched quote delimiter is present in the preceding template text.
             * For details, see https://github.com/SparkDevNetwork/Rock/issues/5560.
             * 
             * Resolution: This issue is caused by the inadequacy of Regex to encapsulate the complex logic required to identify comments vs literal text.
             * This issue has been fixed for the Fluid Engine by implementing shorthand comments in the custom parser.
             * A fix for DotLiquid would require additional work to replace Regex with a custom parser to strip comments from the source template,
             * which is not justified because the DotLiquid engine will be removed in v17.
             */

            var engineOptions = new LavaEngineConfigurationOptions
            {
                InitializeDynamicShortcodes = false
            };
            var engine = LavaService.NewEngineInstance( typeof( FluidEngine ), engineOptions );

            LavaIntegrationTestHelper.SetEngineInstance( engine );

            var template = @"
<h3>Testing issue 5560</h3>

{% comment %}By Jim M...{% endcomment %}
{% comment %}By Stan Y...{% endcomment %}
{% comment %} By Jim M Jan 2021. This block gets a person's Explo Online group, Zoom Link, schedule, and Leader details.{% endcomment %} 

/- GroupType 67 = Explo Online - assume person is in only 1 group of this type -/
Did you see those comments ^^^

{% assign groupMember = CurrentPerson | Groups: ""67"" | First %}
{% assign grp = groupMember.Group.Id | GroupById %}

            //- proceed if we found a group

            {% if grp != null and grp != empty %}
    < b > Welcome...</ b >
{% endif %}
";
            var expectedOutput = @"
<h3>Testing issue 5560</h3>
Did you see those comments ^^^
";
            var actualOutput = LavaService.RenderTemplate( template ).Text;

            Assert.That.AreEqualIgnoreWhitespace( expectedOutput, actualOutput );
        }

        /// <summary>
        /// Verifies the resolution of a specific Issue.
        /// </summary>
        [TestMethod]
        public void Issue5632_ScheduleStartTimeReturnsUtc()
        {
            /* The Fluid Lava Engine incorrectly renders the Schedule.StartTimeOfDay property as a UTC DateTime rather than a TimeSpan.
             * For details, see https://github.com/SparkDevNetwork/Rock/issues/5632.
             * 
             * Resolution: This issue is caused because the Fluid Engine converts and stores the TimeSpan as a DateTime value.
             * This issue has been fixed by adding a specific Fluid value converter for the TimeSpan type.
             */

            var engineOptions = new LavaEngineConfigurationOptions
            {
                InitializeDynamicShortcodes = false
            };
            var engine = LavaService.NewEngineInstance( typeof( FluidEngine ), engineOptions );

            LavaIntegrationTestHelper.SetEngineInstance( engine );

            var template = @"
<h3>Testing issue 5632</h3>

Standard Date Format: {{ '2023-10-01 15:30:00' | Date:'hh:mm tt' }}
<br/>
{% schedule where:'Name == ""Sunday 10:30am""' %}
Schedule Name: {{ schedule.Name }}<br/>
StartTimeOfDay (Raw): {{ schedule.StartTimeOfDay }}<br/>
StartTimeOfDay (Formatted): {{ schedule.StartTimeOfDay | Date:'hh:mm tt K' }}<br/>
<pre>{{ schedule.iCalendarContent }}</pre>
{% endschedule %}
";
            var expectedOutput = $@"
<h3>Testing issue 5632</h3>
Standard Date Format: 03:30 PM
<br/>
Schedule Name: Sunday 10:30am
<br/>
StartTimeOfDay (Raw): 10:30:00
<br/>
StartTimeOfDay (Formatted): 10:30 AM {System.DateTime.Now:%K}
<br/>
<pre>
    BEGIN:VCALENDAR
    BEGIN:VEVENT
    DTEND:20130501T113000
    DTSTART:20130501T103000
    RRULE:FREQ=WEEKLY;BYDAY=SU
    END:VEVENT
    END:VCALENDAR
</pre>
";
            var options = new LavaTestRenderOptions
            {
                EnabledCommands = "RockEntity",
                IgnoreWhiteSpace = true
            };

            _TestHelper.AssertTemplateOutput( expectedOutput, template, options );
        }

        [TestMethod]
        public void Issue5687_CannotNestEntityCommands()
        {
            /* The Fluid Lava Engine throws a parsing exception when trying to process embedded entity commands
             * with the same root prefix.
             * For details, see https://github.com/SparkDevNetwork/Rock/issues/5687.
             * 
             * Resolution: This issue occurred because the custom Lava tag parser for Fluid introduced in v16
             * did not detect or require a whitespace delimiter for the tag identifier.
             * The new parser was introduced to replace the existing less performant RegEx parser.
             */

            var input = @"
{% assign registrationInstanceId = 1 %}
{% registration where:'RegistrationInstanceId == {{ registrationInstanceId }}' %}
    {% assign currentRegistrantCount = 0 %}
    {% for registration in registrationItems %}
        {% assign registrationId = registration.Id %}
        {% registrationregistrant where:'RegistrationId == ""{{ registrationId }}""' %}
            {% for registrationregistrant in registrationregistrantItems %}
                {% assign currentRegistrantCount = currentRegistrantCount | Plus:1 %}
            {% endfor %}
        {% endregistrationregistrant %}
    {% endfor %}
{% endregistration %}
";

            // Confirm that the template is parsed correctly, but ignore the output.
            var options = new LavaTestRenderOptions() { EnabledCommands = "RockEntity" };
            TestHelper.AssertTemplateOutput( string.Empty, input, options );
        }
    }
}
