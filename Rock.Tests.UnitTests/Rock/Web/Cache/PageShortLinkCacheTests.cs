using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Model;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.UnitTests.Rock.Web.Cache
{
    /// <summary>
    /// This suite checks the PageShortLinkCache object to make sure that
    /// all logic works as intended.
    /// </summary>
    /// <seealso cref="PageShortLinkCache"/>
    [TestClass]
    public class PageShortLinkCacheTests : MockDatabaseTestsBase
    {
        #region GetCurrentUrl

        [TestMethod]
        public void GetCurrentUrl_WithNoLinkSchedules_ReturnsDefaultUrl()
        {
            var expectedUrl = "https://www.rockrms.com";

            var rockContextMock = MockDatabaseHelper.GetRockContextMock();
            var pageShortLink = MockDatabaseHelper.CreateEntityMock<PageShortLink>( 1, new Guid( "2248633d-e109-4349-b2b6-27628e73010f" ) );

            pageShortLink.Object.Url = expectedUrl;

            var pageShortLinkCache = new PageShortLinkCache();
            pageShortLinkCache.SetFromEntity( pageShortLink.Object );

            Assert.AreEqual( expectedUrl, pageShortLinkCache.GetCurrentUrl( rockContextMock.Object ) );
        }

        [TestMethod]
        public void GetCurrentUrl_WithActiveLinkSchedule_ReturnsScheduleUrl()
        {
            var expectedUrl = "https://www.rockrms.com";

            var scheduleData = new Cms.PageShortLinkScheduleData
            {
                Schedules = new List<Cms.PageShortLinkSchedule>
                {
                    new Cms.PageShortLinkSchedule
                    {
                        CustomCalendarContent = GetScheduleContentForNow(),
                        Url = expectedUrl
                    }
                }
            };

            var rockContextMock = MockDatabaseHelper.GetRockContextMock();
            var pageShortLink = MockDatabaseHelper.CreateEntityMock<PageShortLink>( 1, new Guid( "2248633d-e109-4349-b2b6-27628e73010f" ) );

            pageShortLink.Object.Url = "https://rock.rocksolidchurchdemo.com";
            pageShortLink.Object.SetScheduleData( scheduleData );

            var pageShortLinkCache = new PageShortLinkCache();
            pageShortLinkCache.SetFromEntity( pageShortLink.Object );

            Assert.AreEqual( expectedUrl, pageShortLinkCache.GetCurrentUrl( rockContextMock.Object ) );
        }

        [TestMethod]
        public void GetCurrentUrl_WithInactiveLinkSchedule_ReturnsDefaultUrl()
        {
            var expectedUrl = "https://www.rockrms.com";

            var scheduleData = new Cms.PageShortLinkScheduleData
            {
                Schedules = new List<Cms.PageShortLinkSchedule>
                {
                    new Cms.PageShortLinkSchedule
                    {
                        CustomCalendarContent = GetScheduleContentForYesterday(),
                        Url = "https://rock.rocksolidchurchdemo.com"
                    }
                }
            };

            var rockContextMock = MockDatabaseHelper.GetRockContextMock();
            var pageShortLink = MockDatabaseHelper.CreateEntityMock<PageShortLink>( 1, new Guid( "2248633d-e109-4349-b2b6-27628e73010f" ) );

            pageShortLink.Object.Url = expectedUrl;
            pageShortLink.Object.SetScheduleData( scheduleData );

            var pageShortLinkCache = new PageShortLinkCache();
            pageShortLinkCache.SetFromEntity( pageShortLink.Object );

            Assert.AreEqual( expectedUrl, pageShortLinkCache.GetCurrentUrl( rockContextMock.Object ) );
        }

        private string GetScheduleContentForNow()
        {
            var start = DateTime.Now.AddMinutes( -30 ).ToString( "yyyyMMddTHHmm00" );
            var end = DateTime.Now.AddMinutes( 30 ).ToString( "yyyyMMddTHHmm00" );

            return $@"BEGIN:VCALENDAR
BEGIN:VEVENT
DTSTART:{start}
DTEND:{end}
END:VEVENT
END:VCALENDAR";
        }

        private string GetScheduleContentForYesterday()
        {
            var start = DateTime.Now.AddDays( -1 ).AddMinutes( -30 ).ToString( "yyyyMMddTHHmm00" );
            var end = DateTime.Now.AddDays( -1 ).AddMinutes( 30 ).ToString( "yyyyMMddTHHmm00" );

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
