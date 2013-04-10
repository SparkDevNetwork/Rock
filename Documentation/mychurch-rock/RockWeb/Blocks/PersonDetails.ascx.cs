//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using Rock.Web.UI;

namespace RockWeb.Blocks
{
    public partial class PersonDetails : RockBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddCSSLink( Page, ResolveUrl( "~/CSS/jquery.tagsinput.css" ) );
            RockPage.AddCSSLink( Page, ResolveUrl( "~/CSS/PersonDetailsCore.css" ) );
            RockPage.AddScriptLink( Page, ResolveUrl( "~/Scripts/jquery.tagsinput.js" ) );
        }
    }
}