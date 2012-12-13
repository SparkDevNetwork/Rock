//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

using Rock.CheckIn;
using Rock.Model;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Kiosk REST API
    /// </summary>
    public partial class KiosksController : ApiController
    {
        public virtual KioskStatus Get( int id )
        {
            var kioskStatus = KioskCache.Kiosks.Where( k => k.Id == id ).FirstOrDefault();
            return kioskStatus;
        }
    }
}
