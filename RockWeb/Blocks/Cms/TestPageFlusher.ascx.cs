using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Blocks_Cms_TestPageFlusher : System.Web.UI.UserControl
{
    protected void Page_Load(object sender, EventArgs e)
    {
		int id = -1;
		if ( ! string.IsNullOrEmpty(TextBox1.Text) && int.TryParse( TextBox1.Text, out id ) )
		{
			Rock.Cms.Cached.Page.Flush( id );
		}
    }
}