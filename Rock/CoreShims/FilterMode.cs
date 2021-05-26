using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    public enum FilterMode
    {
        /// <summary>
        /// Render the UI and process the filter as a simple filter
        /// This mode can be set if the filter just needs to be simple with minimal UI (like on a public page)
        /// </summary>
        SimpleFilter,

        /// <summary>
        /// Render and process as an advanced filter 
        /// This will be the mode when configuring as a Data Filter
        /// </summary>
        AdvancedFilter
    }
}
