using Microsoft.AspNetCore.Mvc;

namespace Rock.Rest
{
    /// <summary>
    /// API endpoints for the <see cref="Rock.Model.Group"/> entity type.
    /// </summary>
    /// <seealso cref="Rock.Rest.EntityController{TEntity}" />
    [ApiController]
    [Route( "api/v2/models/[controller]" )]
    public class GroupsController : EntityController<Rock.Model.Group>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupsController"/> class.
        /// </summary>
        /// <param name="dependencies">The required dependencies.</param>
        public GroupsController( EntityControllerDependencies dependencies )
            : base( dependencies )
        {
        }
    }
}
