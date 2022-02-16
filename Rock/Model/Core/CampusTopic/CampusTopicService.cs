using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// CampusTopicService Service class
    /// </summary>
    public class CampusTopicService : Service<CampusTopic>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CampusScheduleService"/> class
        /// </summary>
        /// <param name="context">The context.</param>
        public CampusTopicService( DbContext context ) : base( context )
        {
        }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( CampusTopic item, out string errorMessage )
        {
            errorMessage = string.Empty;
            return true;
        }
    }
}
