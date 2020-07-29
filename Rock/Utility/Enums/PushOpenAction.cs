using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Utility
{
    /// <summary>
    /// The Push Open Action used by communications.
    /// </summary>
    public enum PushOpenAction
    {
        /// <summary>
        /// The no action
        /// </summary>
        NoAction,
        /// <summary>
        /// The show details
        /// </summary>
        ShowDetails,
        /// <summary>
        /// The link to mobile page
        /// </summary>
        LinkToMobilePage,
        /// <summary>
        /// The link to URL
        /// </summary>
        LinkToUrl
    }
}
