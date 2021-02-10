using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

using Rock.Tests.Shared;
using Rock.Lava.Blocks;

namespace Rock.Tests.Integration.Lava
{
    [TestClass]
    public class RockEntityTests
    {
        /// <summary>
        /// Tests the EventsCalendarItem to make sure that an item's EventItem and EventItem.Summary are returned.
        /// </summary>
        [TestMethod]
        public void EventCalendarItemAllowsEventItemSummary()
        {
            RockEntity.RegisterEntityCommands();

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
    }
}
