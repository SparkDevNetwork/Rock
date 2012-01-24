//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;

namespace FakeCompany.Examples
{
    public partial class ExampleCustomBlock : Rock.Web.UI.Block
	{
		protected void Page_Load( object sender, EventArgs e )
		{
			lTime.Text = DateTime.Now.ToShortTimeString();
		}
	}
}

