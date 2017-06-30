using System.Web.Http;
using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class RestControllersController 
    {
        /// <summary>
        /// Ensures that rest controllers have been registered to the Rock Database
        /// </summary>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/RestControllers/EnsureRestControllers" )]
        public bool EnsureRestControllers()
        {
            RestControllerService.RegisterControllers();

            return true;
        }
    }
}
