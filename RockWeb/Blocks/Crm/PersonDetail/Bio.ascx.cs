//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Web.UI;

using Rock.CRM;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    public partial class Bio : Rock.Web.UI.PersonBlock
    {
        protected void Page_Load( object sender, EventArgs e )
        {
			if ( Person != null )
			{
				var page = Page as Rock.Web.UI.Page;
				if ( page != null )
					page.SetTitle( Person.FullName );
			}
        }
    }
}