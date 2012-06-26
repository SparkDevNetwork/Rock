//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;

public partial class Blocks_Cms_TestPageFlusher : System.Web.UI.UserControl
{
    protected void Page_Load(object sender, EventArgs e)
    {
		int id = -1;
		if ( ! string.IsNullOrEmpty(TextBox1.Text) && int.TryParse( TextBox1.Text, out id ) )
		{
			// Flush the page's block instances
			Rock.Web.Cache.Page.Read( id ).FlushBlockInstances();

			// Flush the page
			Rock.Web.Cache.Page.Flush( id );

		}
    }
}