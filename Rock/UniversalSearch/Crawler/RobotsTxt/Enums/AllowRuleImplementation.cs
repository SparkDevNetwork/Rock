using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.UniversalSearch.Crawler.RobotsTxt.Enums
{
    /// <summary>
    /// Robots Allow Rule
    /// </summary>
    public enum AllowRuleImplementation
    {
        /// <summary>
        /// First matching rule will win.
        /// </summary>
        Standard,

        /// <summary>
        /// Disallow rules will only be checked if no allow rule matches.
        /// </summary>
        AllowOverrides,

        /// <summary>
        /// The more specific (the longer) rule will apply.
        /// </summary>
        MoreSpecific
    }
}
