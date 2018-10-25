using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.CheckIn
{
    /// <summary>
    /// A RockBlock specific to check-in Edit/Add Family for registration during checkin
    /// </summary>
    /// <seealso cref="Rock.CheckIn.CheckInBlock" />
    public abstract class CheckInEditFamilyBlock : CheckInBlock
    {
        /// <summary>
        /// Shows the Edit Family block in Edit Family mode
        /// </summary>
        /// <param name="checkInFamily">The check in family.</param>
        public abstract void ShowEditFamily( CheckInFamily checkInFamily );

        /// <summary>
        /// Shows the Edit Family block in New Family mode
        /// </summary>
        public abstract void ShowAddFamily();
    }
}
