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
using System.Globalization;

namespace Rock.UniversalSearch.Crawler.RobotsTxt
{
    internal class CrawlDelayRule : Rule
    {
        public long Delay { get; private set; } // milliseconds

        public CrawlDelayRule( String userAgent, Line line, int order )
            : base( userAgent, order )
        {
            double delay = 0;
            Double.TryParse( line.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out delay );
            Delay = (long)(delay * 1000);
        }
    }
}
