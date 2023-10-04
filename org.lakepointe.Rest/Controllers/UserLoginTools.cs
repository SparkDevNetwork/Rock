using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Web.Http;
using System.Web.Security;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Rest;
using Rock.Rest.Filters;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace org.lakepointe.Rest.Controllers
{
    public partial class UserLoginToolsController : ApiController
    {
        public UserLoginToolsController() { }
    }

    public partial class UserLoginToolsController
    {
        [Authenticate, Secured]
        [System.Web.Http.Route("api/org_lakepointe/UserLoginTools/GenerateRandomPassword")]
        [HttpGet]
        public string GenerateRandomPassword( int length, int numberofNonAlphaNumericCharacters )
        {
            return Membership.GeneratePassword( length, numberofNonAlphaNumericCharacters );
        }
    }
}
