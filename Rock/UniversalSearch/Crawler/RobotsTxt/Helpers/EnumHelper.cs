using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.UniversalSearch.Crawler.RobotsTxt.Enums;

namespace Rock.UniversalSearch.Crawler.RobotsTxt.Helpers
{
    internal static class EnumHelper
    {
        internal static LineType GetLineType( string field )
        {
            switch ( field )
            {
                case "user-agent":
                    return LineType.UserAgent;
                case "allow":
                case "disallow":
                    return LineType.AccessRule;
                case "crawl-delay":
                    return LineType.CrawlDelayRule;
                case "sitemap":
                    return LineType.Sitemap;
                default:
                    return LineType.Unknown;
            }
        }
    }
}
