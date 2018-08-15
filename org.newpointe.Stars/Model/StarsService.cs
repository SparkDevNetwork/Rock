using org.newpointe.Stars.Data;
using Rock.Model;

namespace org.newpointe.Stars.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class StarsService : StarsProjectService<Stars>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StarsService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public StarsService( StarsProjectContext context ) : base( context ) { }

    }
}
