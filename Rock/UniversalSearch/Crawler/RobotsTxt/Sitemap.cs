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
using System;

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
