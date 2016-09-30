using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.UniversalSearch.Crawler.RobotsTxt
{
    internal abstract class Rule
    {
        public string For { get; private set; }
        public int Order { get; private set; }

        public Rule( string userAgent, int order )
        {
            For = userAgent;
            Order = order;
        }
    }
}
