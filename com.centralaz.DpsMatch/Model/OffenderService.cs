using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using com.centralaz.DpsMatch.Data;
using Rock.Model;

namespace com.centralaz.DpsMatch.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class OffenderService : DpsMatchService<Offender>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OffenderService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public OffenderService( DpsMatchContext context ) : base( context ) { }

    }
}
