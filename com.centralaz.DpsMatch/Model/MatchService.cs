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
    public class MatchService : DpsMatchService<Match>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MatchService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public MatchService( DpsMatchContext context ) : base( context ) { }

    }
}
