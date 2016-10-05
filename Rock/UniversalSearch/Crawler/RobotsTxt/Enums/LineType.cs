using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.UniversalSearch.Crawler.RobotsTxt.Enums
{
    internal enum LineType
    {
        Comment,
        UserAgent,
        Sitemap,
        AccessRule,
        CrawlDelayRule,
        Unknown
    }
}
