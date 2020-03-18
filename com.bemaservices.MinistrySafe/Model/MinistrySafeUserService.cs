using Rock.Data;

namespace com.bemaservices.MinistrySafe.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class MinistrySafeUserService : Service<MinistrySafeUser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public MinistrySafeUserService( RockContext context ) : base( context ) { }
    }
}