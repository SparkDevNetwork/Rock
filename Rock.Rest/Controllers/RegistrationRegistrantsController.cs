using System.Collections.Generic;
using System.Web.Http;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class RegistrationRegistrantsController
    {
        #region Group Placement Related

        /// <summary>
        /// Gets the group placement registrants.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/RegistrationRegistrants/GetGroupPlacementRegistrants" )]
        [HttpPost]
        public IEnumerable<GroupPlacementRegistrant> GetGroupPlacementRegistrants( [FromBody]GetGroupPlacementRegistrantsParameters options )
        {
            var rockContext = new RockContext();
            var registrantService = new RegistrationRegistrantService( rockContext );
            return registrantService.GetGroupPlacementRegistrants( options, this.GetPerson() );
        }

        #endregion Group Placement Related
    }
}
