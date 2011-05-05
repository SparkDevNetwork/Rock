using System;
using Rock.Cms;

namespace FakeCompany.Examples
{
	public partial class ExampleTimeBlock : CmsBlock
	{
		protected void Page_Load( object sender, EventArgs e )
		{
			lTime.Text = DateTime.Now.ToShortTimeString();
		}
	}
}

