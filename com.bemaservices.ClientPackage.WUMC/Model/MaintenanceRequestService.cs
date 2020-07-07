using Rock.Data;

namespace com.bemaservices.ClientPackage.WUMC.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class MaintenanceRequestService : Service<MaintenanceRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public MaintenanceRequestService( RockContext context ) : base( context ) { }
    }
}