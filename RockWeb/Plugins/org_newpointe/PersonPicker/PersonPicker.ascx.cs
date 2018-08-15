using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Entity;
using System.Data;
using System.Text;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Workflow;


using System.Text.RegularExpressions;
using System.Web.UI.HtmlControls;
using Rock.Security;
using Rock.Web.UI;

namespace RockWeb.Plugins.org_newpointe.PersonPicker
{
    /// <summary>
    /// Template block for a TreeView.
    /// </summary>
    [DisplayName("Person Picker")]
    [Category("NewPointe Core")]
    [Description("Person Picker")]

    public partial class PersonPicker : Rock.Web.UI.RockBlock
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!string.IsNullOrWhiteSpace(PageParameter("PersonId")) && PageParameter("PersonId").AsInteger() > 0)
                {
                    ppPerson.SetValue(new PersonService(new RockContext()).Get(PageParameter("PersonId").AsInteger()));
                }
            }
        }

        protected void ppPerson_SelectPerson(object sender, EventArgs e)
        {
            String qstring = "";
            foreach (string s in Request.QueryString)
            {
                foreach (string v in Request.QueryString.GetValues(s))
                {
                    if (s != "PersonId")
                    {
                        qstring += "&" + s + "=" + v;
                    }
                }
            }
            qstring += "&PersonId=" + ppPerson.SelectedValue.ToStringSafe();

            Response.Redirect(Request.Url.AbsolutePath + "?" + qstring.Substring(1));
        }
    }
}