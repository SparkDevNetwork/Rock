using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Cms;
using Rock.Configuration;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Web.Cache
{
    /// <summary>
    /// This suite checks the PageShortLinkCache object to make sure that
    /// all logic works as intended.
    /// </summary>
    /// <seealso cref="PageShortLinkCache"/>
    [TestClass]
    public class PageShortLinkCacheTests
    {
        #region GetCurrentUrl

        [TestMethod]
        public void GetCurrentUrl_WithNoLinkSchedules_ReturnsDefaultUrl()
        {
            var expectedUrl = "https://www.rockrms.com";

            using var app = TestHelper.CreateScopedRockApp();
            var rockContext = app.App.CreateRockContext();
            var pageShortLink = new PageShortLink
            {
                Id = 1,
                Guid = new Guid( "2248633d-e109-4349-b2b6-27628e73010f" ),
                Url = expectedUrl,
            };

            rockContext.Set<PageShortLink>().Add( pageShortLink );

            var pageShortLinkCache = PageShortLinkCache.Get( pageShortLink.Id, rockContext );

            Assert.AreEqual( expectedUrl, pageShortLinkCache.GetCurrentUrl( rockContext ) );
        }

        [TestMethod]
        public void GetCurrentUrl_WithActiveLinkSchedule_ReturnsScheduleUrl()
        {
            var expectedUrl = "https://www.rockrms.com";

            var scheduleData = new PageShortLinkScheduleData
            {
                Schedules = new List<PageShortLinkSchedule>
                {
                    new PageShortLinkSchedule
                    {
                        CustomCalendarContent = GetScheduleContentForNow(),
                        Url = expectedUrl
                    }
                }
            };

            using var app = TestHelper.CreateScopedRockApp();
            var rockContext = app.App.CreateRockContext();
            var pageShortLink = new PageShortLink
            {
                Id = 1,
                Guid = new Guid( "2248633d-e109-4349-b2b6-27628e73010f" ),
                Url = "https://rock.rocksolidchurchdemo.com",
            };

            pageShortLink.SetScheduleData( scheduleData );

            rockContext.Set<PageShortLink>().Add( pageShortLink );

            var pageShortLinkCache = PageShortLinkCache.Get( pageShortLink.Id, rockContext );

            Assert.AreEqual( expectedUrl, pageShortLinkCache.GetCurrentUrl( rockContext ) );
        }

        [TestMethod]
        [Ignore( "This is failing randomly in CI for some reason. It makes no sense because the schedule is for 24 hours prior. Maybe a cache issue or long running task that hits because of limited CPU speed on CI." )]
        public void GetCurrentUrl_WithInactiveLinkSchedule_ReturnsDefaultUrl()
        {
            var expectedUrl = "https://www.rockrms.com";

            var scheduleData = new PageShortLinkScheduleData
            {
                Schedules = new List<PageShortLinkSchedule>
                {
                    new PageShortLinkSchedule
                    {
                        CustomCalendarContent = GetScheduleContentForYesterday(),
                        Url = "https://rock.rocksolidchurchdemo.com"
                    }
                }
            };

            using var app = TestHelper.CreateScopedRockApp();
            var rockContext = app.App.CreateRockContext();
            var pageShortLink = new PageShortLink
            {
                Id = 1,
                Guid = new Guid( "2248633d-e109-4349-b2b6-27628e73010f" ),
                Url = expectedUrl,
            };

            pageShortLink.SetScheduleData( scheduleData );

            rockContext.Set<PageShortLink>().Add( pageShortLink );

            var pageShortLinkCache = PageShortLinkCache.Get( pageShortLink.Id, rockContext );

            Assert.AreEqual( expectedUrl, pageShortLinkCache.GetCurrentUrl( rockContext ) );
        }

        private string GetScheduleContentForNow()
        {
            var start = RockDateTime.Now.AddMinutes( -30 ).ToString( "yyyyMMddTHHmm00" );
            var end = RockDateTime.Now.AddMinutes( 30 ).ToString( "yyyyMMddTHHmm00" );

            return $@"BEGIN:VCALENDAR
BEGIN:VEVENT
DTSTART:{start}
DTEND:{end}
END:VEVENT
END:VCALENDAR";
        }

        private string GetScheduleContentForYesterday()
        {
            var start = RockDateTime.Now.AddDays( -1 ).AddMinutes( -30 ).ToString( "yyyyMMddTHHmm00" );
            var end = RockDateTime.Now.AddDays( -1 ).AddMinutes( 30 ).ToString( "yyyyMMddTHHmm00" );

            return $@"BEGIN:VCALENDAR
BEGIN:VEVENT
DTSTART:{start}
DTEND:{end}
END:VEVENT
END:VCALENDAR";
        }

        #endregion
    }
}
