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

using Rock.Field.Types;
using Rock.Lava.Fluid;
using Rock.Tests.Integration.Crm;
using Rock.Tests.Integration.TestFramework.Database;
using Rock.Tests.Lava.Shared;
using Rock.Tests.Shared.Constants;
using Rock.Web.UI.Controls;

namespace Rock.Tests.Integration.Issues
{
    /// <summary>
    /// Tests that verify specific bug fixes for a Rock version.
    /// </summary>
    /// <remarks>
    /// These tests are developed to verify bugs and fixes that are difficult or time-consuming to reproduce.
    /// They are only relevant to the Rock version in which the bug is fixed, and should be removed in subsequent versions.
    /// </remarks>
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    [RockObsolete( "1.16" )]
    public class BugFixVerificationTests_v16 : DatabaseTestsBase
    {

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

            // The template's own indentation, and the blank line the assign tag
            // leaves behind, are part of the output: neither tag uses whitespace
            // control. The expected value is written as concatenated parts so the
            // leading spaces are visible. What the test is about is that both
            // access paths report a size of 1.
            var expectedOutput = "\n"
                + "\n"
                + "    Dot Notation: 1<br>\n"
                + "    \n"
                + "    Property Filter: 1\n"
                + "\n";

            var options = new LavaRenderOptions
            {
                EnabledCommands = "RockEntity"
            };
            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input, options );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        public void Issue3760_PhoneNumberFieldTypeWithCountryCode_PreservesCountryCode()
        {
            /*
             * Issue:
             * The Phone Number Field Type does not store the country code associated with the phone number,
             * so the information is lost.
             * For details, see:
             * https://github.com/SparkDevNetwork/Rock/issues/3760
             * https://github.com/SparkDevNetwork/Rock/issues/5468
             * 
             * Resolution:
             * Modify the Phone Number Field Type to parse and format country code information.
             */

            // Add a second country code.
            GlobalSettingsDataManager.Instance.AddOrUpdatePhoneNumberCountryCode( "81",
                "Japan",
                @"^(\d{2})(\d{4})(\d{4})$",
                @"$1-$2-$3" );

            var phoneNumberFieldType = new PhoneNumberFieldType();
            var phoneNumberControl = new PhoneNumberBox();

            // Test 1: Set the edit control to a phone number value with a non-default country code.
            phoneNumberControl.CountryCode = "81";
            phoneNumberControl.Number = "1122223333";

            // Read the field value from the control and verify that the country code is preserved.
            var editValue = phoneNumberFieldType.GetEditValue( phoneNumberControl, null );
            var textValue = phoneNumberFieldType.GetTextValue( editValue, null );

            Assert.AreEqual( "+81 11-2222-3333", textValue );

            // Test 2: Set the edit control to a phone number value with the default country code.
            phoneNumberControl.CountryCode = "1";
            phoneNumberControl.Number = "1122223333";

            // Read the field value from the control and verify that the country code is omitted.
            editValue = phoneNumberFieldType.GetEditValue( phoneNumberControl, null );
            textValue = phoneNumberFieldType.GetTextValue( editValue, null );

            Assert.AreEqual( "(112) 222-3333", textValue );
        }

        /// <summary>
        /// Verifies the resolution of a specific Issue.
        /// </summary>
        [TestMethod]
        [TestCategory( TestFeatures.Lava )]
        public void Issue5560_LavaCommentsDisplayedInOutput()
        {
            /* The Lava Engine may render inline comments to output where an unmatched quote delimiter is present in the preceding template text.
             * For details, see https://github.com/SparkDevNetwork/Rock/issues/5560.
             * 
             * Resolution: This issue is caused by the inadequacy of Regex to encapsulate the complex logic required to identify comments vs literal text.
             * This issue has been fixed for the Fluid Engine by implementing shorthand comments in the custom parser.
             */

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
            // Each comment is removed, but the line it occupied is not: what is
            // left behind is the whitespace that surrounded it. The runs of blank
            // lines below are those remains, and that they hold no comment text is
            // the point of the test. The expected value is written as concatenated
            // parts so that the blank lines and trailing spaces are visible.
            var expectedOutput = "\n"
                + "<h3>Testing issue 5560</h3>\n"
                + "\n\n\n"
                + " \n"
                + "\n\n"
                + "Did you see those comments ^^^\n"
                + "\n\n\n\n"
                + "            \n"
                + "\n"
                + "            \n";

            var options = new LavaRenderOptions
            {
                EnabledCommands = "RockEntity"
            };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template, options );

                Assert.AreEqual( expectedOutput, output );

                // Named separately from the comparison above, so that a failure
                // says a comment leaked rather than just that the text differs.
                Assert.DoesNotContain( "By Jim M", output );
                Assert.DoesNotContain( "By Stan Y", output );
                Assert.DoesNotContain( "GroupType 67", output );
                Assert.DoesNotContain( "proceed if we found a group", output );
            } );
        }

        [TestMethod]
        [TestCategory( "ShortcodeScopeBehavior" )]
        public void Issue5102_VariableScopingInWorkflowActivateTag()
        {
            /* The WorkflowActivate tag does not allow persistent changes to variables declared outside the block in Fluid.
             * For details, see https://github.com/SparkDevNetwork/Rock/issues/5102.
             * 
             * Resolution: This issue has been closed by a fix for the Fluid framework.
             * For details, see https://github.com/sebastienros/fluid/issues/553.
             */

            // Activate Workflow: IT Support
            var input = @"
{% assign list = '1,2,3' | Split: ',' %}
{% assign counter = 0 %}

{% for i in list %}
    <Pass {{ forloop.index }}>
    {% workflowactivate workflowtype:'51FE9641-FB8F-41BF-B09E-235900C3E53E' %}
        {% assign counter = counter | Plus:1 %}
        Inner Scope: counter={{ counter }},
    {% endworkflowactivate %}
    Outer Scope: counter={{ counter }}
{% endfor %}
";

            // What the test is about is that the counter assigned inside the
            // workflowactivate block is still that value outside it, so the inner
            // and outer readings agree on every pass. The indentation and blank
            // lines come from the template, which uses no whitespace control; the
            // expected value is built per pass so that the pairing stays visible.
            var expectedOutput = "\n\n\n\n\n";

            for ( var pass = 1; pass <= 3; pass++ )
            {
                expectedOutput += "    <Pass " + pass + ">\n"
                    + "    \n"
                    + "        \n"
                    + "        Inner Scope: counter=" + pass + ",\n"
                    + "    \n"
                    + "    Outer Scope: counter=" + pass + "\n"
                    + "\n";
            }

            var options = new LavaRenderOptions() { EnabledCommands = "WorkflowActivate" };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, input, options );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        /// <summary>
        /// Verifies the resolution of a specific Issue.
        /// </summary>
        [TestMethod]
        [TestCategory( TestFeatures.Lava )]
        public void Issue5632_ScheduleStartTimeReturnsUtc()
        {
            /* The Fluid Lava Engine incorrectly renders the Schedule.StartTimeOfDay property as a UTC DateTime rather than a TimeSpan.
             * For details, see https://github.com/SparkDevNetwork/Rock/issues/5632.
             * 
             * Resolution: This issue is caused because the Fluid Engine converts and stores the TimeSpan as a DateTime value.
             * This issue has been fixed by adding a specific Fluid value converter for the TimeSpan type.
             */

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
            // What the test is about is the "(Raw)" line: StartTimeOfDay renders as
            // the TimeSpan 10:30:00 rather than as a UTC DateTime. The rest is the
            // surrounding template, written out exactly because the comparison is
            // now exact. The iCalendar body carries the line terminators the row
            // was stored with, which Render normalizes to "\n" along with the rest
            // of the output.
            var expectedOutput = "\n"
                + "<h3>Testing issue 5632</h3>\n"
                + "\n"
                + "Standard Date Format: 03:30 PM\n"
                + "<br/>\n"
                + "\n"
                + "Schedule Name: Sunday 10:30am<br/>\n"
                + "StartTimeOfDay (Raw): 10:30:00<br/>\n"
                // The offset is the one the machine running the test is in, which
                // is what the Date filter's "K" specifier renders here.
                + $"StartTimeOfDay (Formatted): 10:30 AM {System.DateTime.Now:%K}<br/>\n"
                + "<pre>BEGIN:VCALENDAR\n"
                + "BEGIN:VEVENT\n"
                + "DTEND:20130501T113000\n"
                + "DTSTART:20130501T103000\n"
                + "RRULE:FREQ=WEEKLY;BYDAY=SU\n"
                + "END:VEVENT\n"
                + "END:VCALENDAR</pre>\n"
                + "\n";
            var options = new LavaRenderOptions
            {
                EnabledCommands = "RockEntity"
            };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, template, options );

                Assert.AreEqual( expectedOutput, output );
            } );
        }

        [TestMethod]
        [TestCategory( TestFeatures.Lava )]
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

            /*
                9/27/26 - CLAUDE

                The test is about the template parsing at all: before the fix,
                nesting two entity commands whose names share a root prefix threw.
                So the parse result is asserted directly rather than inferred from
                the rendered text.

                The previous assertion compared the output against an empty string,
                which only held because the comparison stripped whitespace. The
                template emits nothing of its own, but the whitespace between its
                tags survives, so what the output must be is blank rather than
                empty. Asserting the exact run of spaces and newlines would say
                nothing about the issue and would break on any unrelated change to
                whitespace handling.

                Reason: Assert that the template parsed and rendered no content,
                which is what the issue was about.
            */
            var options = new LavaRenderOptions() { EnabledCommands = "RockEntity" };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var result = LavaRenderTestHelper.RenderResult( engine, input, options );

                Assert.IsNull( result.Error, "Template parsing failed." );
                Assert.IsTrue( result.Text.IsNullOrWhiteSpace(), $"Expected no rendered content. [Output=\"{result.Text}\"]" );
            } );
        }
    }
}
