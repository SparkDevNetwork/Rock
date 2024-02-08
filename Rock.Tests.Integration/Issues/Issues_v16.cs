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

using Rock.Field.Types;
using Rock.Lava;
using Rock.Lava.Fluid;
using Rock.Tests.Integration.Modules.Crm;
using Rock.Tests.Shared;
using Rock.Tests.Shared.Lava;
using Rock.Web.UI.Controls;

namespace Rock.Tests.Integration.BugFixes
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
    public class BugFixVerificationTests_v16
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
            _TestHelper.AssertTemplateOutput( expectedOutput, input, options );
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

            Assert.That.AreEqual( "+81 11-2222-3333", textValue );

            // Test 2: Set the edit control to a phone number value with the default country code.
            phoneNumberControl.CountryCode = "1";
            phoneNumberControl.Number = "1122223333";

            // Read the field value from the control and verify that the country code is omitted.
            editValue = phoneNumberFieldType.GetEditValue( phoneNumberControl, null );
            textValue = phoneNumberFieldType.GetTextValue( editValue, null );

            Assert.That.AreEqual( "(112) 222-3333", textValue );
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

        [TestMethod]
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

            var expectedOutput = @"
<Pass1>InnerScope:counter=1,OuterScope:counter=1<Pass2>InnerScope:counter=2,OuterScope:counter=2<Pass3>InnerScope:counter=3,OuterScope:counter=3 
";

            var options = new LavaTestRenderOptions() { EnabledCommands = "WorkflowActivate" };

            _TestHelper.AssertTemplateOutput( expectedOutput, input, options );
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

            // Confirm that the template is parsed correctly, but ignore the output.
            var options = new LavaTestRenderOptions() { EnabledCommands = "RockEntity" };
            _TestHelper.AssertTemplateOutput( string.Empty, input, options );
        }
    }
}
