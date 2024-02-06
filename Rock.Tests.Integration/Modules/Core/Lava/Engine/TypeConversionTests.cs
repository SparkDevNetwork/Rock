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
using Rock.Tests.Shared.Lava;

namespace Rock.Tests.Integration.Modules.Core.Lava.Engine
{
    /// <summary>
    /// Tests that verify the correct handling of various data types in the Lava Engine and associated Liquid framework.
    /// </summary>
    [TestClass]
    public class TypeConversionTests
    {
        private LavaIntegrationTestHelper _TestHelper = LavaIntegrationTestHelper.CurrentInstance;

        /// <summary>
        /// Verifies the resolution of a specific Issue.
        /// </summary>
        [TestMethod]
        [Ignore( "Expected output needs to be re-written to work with multiple locale settings for date formats." )]
        public void TimeSpan_TimeSpanValue_IsCorrectlyParsedAndRendered()
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
Standard Date Format: {{ '2023-10-01 15:30:00' | Date:'hh:mm tt' }}
<br/>
{% schedule where:'Name == ""Sunday 10:30am""' %}
Schedule Name: {{ schedule.Name }}<br/>
StartTimeOfDay (Raw): {{ schedule.StartTimeOfDay }}<br/>
StartTimeOfDay (Formatted): {{ schedule.StartTimeOfDay | Date:'hh:mm tt K' }}<br/>
<pre>{{ schedule.iCalendarContent }}</pre>
{% endschedule %}
";
            var expectedOutput = @"
Standard Date Format: 03:30 PM
<br/>
Schedule Name: Sunday 10:30am
<br/>
StartTimeOfDay (Raw): 10:30:00
<br/>
StartTimeOfDay (Formatted): 10:30 AM +11:00
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
    }
}
