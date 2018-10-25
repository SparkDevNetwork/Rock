using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.CheckIn
{
    /// <summary>
    /// A RockBlock specific to check-in search
    /// </summary>
    /// <seealso cref="Rock.CheckIn.CheckInBlock" />
    public abstract class CheckInSearchBlock : CheckInBlock
    {
        /// <summary>
        /// Processes the search.
        /// </summary>
        /// <param name="searchString">The search string.</param>
        public abstract void ProcessSearch( string searchString );
    }
}
