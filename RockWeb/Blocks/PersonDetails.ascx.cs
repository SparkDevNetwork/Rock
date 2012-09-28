//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Web.UI;

using Rock.Crm;

namespace RockWeb.Blocks
{
    public partial class PersonDetails : Rock.Web.UI.Block
    {
		protected override void OnInit( EventArgs e )
		{
			base.OnInit( e );
			
			Rock.Web.UI.Page.AddCSSLink( Page, ResolveUrl( "~/CSS/jquery.tagsinput.css" ) );
			Rock.Web.UI.Page.AddCSSLink( Page, ResolveUrl( "~/CSS/PersonDetailsCore.css" ) );
			Rock.Web.UI.Page.AddScriptLink( Page, ResolveUrl( "~/Scripts/jquery.tagsinput.js" ) );
			Rock.Web.UI.Page.AddScriptLink( Page, ResolveUrl( "~/Scripts/tinyscrollbar.min.js" ) );
		}
    }
}