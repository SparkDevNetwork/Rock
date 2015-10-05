using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Constants;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web;


namespace RockWeb.Plugins.com_centralaz.Widgets
{
    [DisplayName("Simple Prayer Block")]
    [Category("Prayer")]
    [Description("Adds simple prayer input box to page.")]
    [LinkedPage("Target Page", "Target page containing Prayer Request Entry Block.")]

    public partial class SimplePrayerBlock : Rock.Web.UI.RockBlock
    {

        protected void btnComplete_Click(object sender, EventArgs e)
        {        
            var parms = new Dictionary<string, string>();
            parms.Add("Request", dtbRequest.Value);
            Response.Redirect(new PageReference(GetAttributeValue("TargetPage"), parms).BuildUrl(), false);
        }
    }
}
