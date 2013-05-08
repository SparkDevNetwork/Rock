//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Web;
using Rock.CheckIn;

namespace RockWeb.Blocks.CheckIn
{
    /// <summary>
    /// This block is responsible for setting the IsMobile cookie on the client for
    /// subsequent use by other checkin blocks.
    /// </summary>
    public partial class MobileEntry : CheckInBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            HttpCookie isMobileCookie = new HttpCookie( CheckInCookie.ISMOBILE, "true" );
            isMobileCookie.Expires = DateTime.MaxValue;
            Response.Cookies.Set( isMobileCookie );
            NavigateToNextPage();
        }
    }
}