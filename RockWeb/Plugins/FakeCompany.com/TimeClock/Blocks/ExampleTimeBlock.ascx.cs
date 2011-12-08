using System;

namespace FakeCompany.Examples
{
    public partial class ExampleTimeBlock : Rock.Web.UI.Block
	{
		protected void Page_Load( object sender, EventArgs e )
		{
			lTime.Text = DateTime.Now.ToShortTimeString();
		}
	}
}

