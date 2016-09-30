using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.UniversalSearch.Crawler.RobotsTxt
{
    /// <summary>
    /// Represents a sitemap directive in a robots.txt file.
    /// </summary>
    public class Sitemap
    {
        /// <summary>
        /// The URL to the sitemap.
        /// WARNING : This property could be null if the file declared a relative path to the sitemap rather than absolute, which is the standard.
        /// </summary>
        public Uri Url { get; private set; }

        /// <summary>
        /// Gets value of the sitemap directive.
        /// </summary>
        public string Value { get; private set; }

        internal static Sitemap FromLine( Line line )
        {
            Sitemap s = new Sitemap { Value = line.Value };
            try
            {
                s.Url = new Uri( line.Value );
            }
            catch ( UriFormatException )
            {
                // fail silently, we can't do anything about the uri being invalid.
            }
            return s;
        }
    }
}
