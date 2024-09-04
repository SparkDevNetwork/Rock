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
using Rock.Data;
using Rock.Lava.Fluid;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.Lava;

namespace Rock.Tests.Integration.Modules.Core.Lava.Shortcodes
{
    /// <summary>
    /// Test for shortcodes that are defined and implemented as code components rather than as parameterized Lava templates.
    /// </summary>
    [TestClass]
    public class ScheduledContentShortcodeTests : LavaIntegrationTestBase
    {
        [TestMethod]
        public void ScheduledContentShortcode_Basic_EmitsCorrectHtml()
        {
            var rockContext = new RockContext();
            var scheduleService = new ScheduleService( rockContext );
            var schedule1630 = scheduleService.GetByIdentifierOrThrow( TestGuids.Schedules.ScheduleSat1630Guid );

            var input = @"
{[ scheduledcontent scheduleid:'$scheduleId' asatdate:'2020-10-17 16:35' ]}
Schedule Name: {{ Schedule.Name }}
<br>
Schedule Live: {{ IsLive }}
<br>
{[ endscheduledcontent ]}
";

            input = input.Replace( "$scheduleId", schedule1630.Id.ToString() );

            var expectedOutput = @"
ScheduleName:Saturday4:30pm<br>ScheduleLive:true<br>
";

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        [TestMethod]
        public void ScheduledContentShortcode_ContainedInCaptureBlock_EmitsCorrectOutput()
        {
            var rockContext = new RockContext();
            var scheduleService = new ScheduleService( rockContext );
            var schedule = scheduleService.GetByIdentifierOrThrow( TestGuids.Schedules.ScheduleDaily1630Guid );

            var input = @"
{% capture isScheduleActive %}{[ scheduledcontent scheduleid:'$scheduleId' ]}true{[ endscheduledcontent ]}
{% endcapture %}
Schedule Active = {{isScheduleActive}}
";
            input = input.Replace( "$scheduleId", schedule.Id.ToString() );

            var expectedOutput = @"Schedule Active = true";

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        [TestMethod]
        public void ScheduledContentShortcode_WithLookAheadDays_ShowsContentWithinLookAheadPeriod()
        {
            var rockContext = new RockContext();
            var scheduleService = new ScheduleService( rockContext );

            var inputTemplate = @"
Next Occurrence:
{[ scheduledcontent scheduleid:'$scheduleId' asatdate:'$asAtDate' showwhen:'both' lookaheaddays:0 ]}
1. Look Ahead - 0 Days --> {{ NextOccurrenceDateTime | Date:'yyyy-MM-dd' }}
{[ endscheduledcontent ]}
{[ scheduledcontent scheduleid:'$scheduleId' asatdate:'$asAtDate' showwhen:'both' lookaheaddays:8 ]}
2. Look Ahead - 8 Days --> {{ NextOccurrenceDateTime | Date:'yyyy-MM-dd' }}
{[ endscheduledcontent ]}
{[ scheduledcontent scheduleid:'$scheduleId' asatdate:'$asAtDate' showwhen:'both' ]}
3. Look Ahead - Default --> {{ NextOccurrenceDateTime | Date:'yyyy-MM-dd' }}
{[ endscheduledcontent ]}
";

            var renderOptions = new LavaTestRenderOptions { IgnoreWhiteSpace = true };
            string input;
            string expectedOutput;

            // The LookAheadDays parameter specifies the number of days to search beyond the current date for the next
            // scheduled date, *and* it is only effective if the scheduled is currently Live.
            // Execute as at Saturday 16:35, when the schedule is active.
            var asAtDate = RockDateTime.New( 2020, 10, 17, 16, 35, 0, 0 );
            var liveSchedule = scheduleService.GetByIdentifierOrThrow( TestGuids.Schedules.ScheduleSat1630Guid );

            input = inputTemplate.Replace( "$scheduleId", liveSchedule.Id.ToString() )
                        .Replace( "$asAtDate", asAtDate.Value.ToString( "yyyy-MM-dd HH:mm:ss" ) );

            expectedOutput = @"
Next Occurrence:
1. Look Ahead - 0 Days -->
2. Look Ahead - 8 Days --> 2020-10-24
3. Look Ahead - Default --> 2020-10-24
";

            TestHelper.AssertTemplateOutput( expectedOutput, input, renderOptions );
        }

        [TestMethod]
        public void ScheduledContentShortcode_WithShowWhenParameter_ShowsContentAppropriateToScheduleStatus()
        {
            var rockContext = new RockContext();
            var scheduleService = new ScheduleService( rockContext );

            var now = RockDateTime.Now;

            var inputTemplate = @"
{[ scheduledcontent scheduleid:'$scheduleId' showwhen:'both' ]}1. IsLive = {{ IsLive }}: Visible always.{[ endscheduledcontent ]}
{[ scheduledcontent scheduleid:'$scheduleId' showwhen:'live' ]}2. showwhen = 'live': Visible when Live.{[ endscheduledcontent ]}
{[ scheduledcontent scheduleid:'$scheduleId' showwhen:'notlive' ]}3. showwhen = 'notlive': Visible when Pending.{[ endscheduledcontent ]}
{[ scheduledcontent scheduleid:'$scheduleId' ]}4. showwhen = (unspecified): Visible when Live.{[ endscheduledcontent ]}
";

            var renderOptions = new LavaTestRenderOptions { IgnoreWhiteSpace = true };
            string input;
            string expectedOutput;

            // Verify for Open Schedule.
            var liveSchedule = scheduleService.GetByIdentifierOrThrow( TestGuids.Schedules.ScheduleDaily1630Guid );

            input = inputTemplate.Replace( "$scheduleId", liveSchedule.Id.ToString() );

            expectedOutput = @"
1. IsLive = true: Visible always.
2. showwhen = 'live': Visible when Live.
4. showwhen = (unspecified): Visible when Live.
";

            TestHelper.AssertTemplateOutput( expectedOutput, input, renderOptions );

            // Verify for Pending Schedule.
            var pendingSchedule = scheduleService.GetByIdentifierOrThrow( TestGuids.Schedules.ScheduleSat1630Guid );
            if ( pendingSchedule.WasScheduleActive( now ) )
            {
                pendingSchedule = scheduleService.GetByIdentifierOrThrow( TestGuids.Schedules.ScheduleSun1200Guid );
            }

            input = inputTemplate.Replace( "$scheduleId", pendingSchedule.Id.ToString() );

            expectedOutput = @"
1. IsLive = false: Visible always.
3. showwhen = 'notlive': Visible when Pending.
";

            TestHelper.AssertTemplateOutput( expectedOutput, input, renderOptions );
        }

    }
}
