//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Web;
using Rock.Attribute;
using Rock.Web.UI.Controls;
using CSScriptLibrary;

namespace RockWeb.Blocks.Cms
{
    [CodeEditorField("Code", "Source code to compile and put results on page.", CodeEditorMode.CSharp, CodeEditorTheme.Rock, 600, true, @"
using System;
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

public class Script
{ 
    // the execute method method will be called by the block
    // the string returned will be placed on the page
    public string Execute(Rock.Web.UI.RockPage currentPage)
    {
        /* sample items
        
        -- change page title
        currentPage.Title = ""I Now Own The Title"";
        
        -- get current user's name
        string user = currentPage.CurrentPerson.FullName;
        
        -- get current user's login id
        string login = currentPage.CurrentUser.Username;
        
        -- get request parm
        string parm = currentPage.Request[""parm""];
        
        -- set the theme
        currentPage.CurrentPage.Layout.Site.Theme = ""Stark"";
        
        -- redirect the page
        currentPage.Response.Redirect(""http://www.rockchms.com"");
        
        -- modify breadcrumbs
        currentPage.BreadCrumbs.Add(new Rock.Web.UI.BreadCrumb(""Title"", ""/page/12"")); // adds a new crumb
        currentPage.BreadCrumbs.Clear(); // removes all crumbs
        currentPage.BreadCrumbs.RemoveAt(currentPage..BreadCrumbs.Count -1); // removes the last one
        
        -- output data from the database (lists all campuses with an Id > 0)
        string output = """";
        var service = new CampusService();
        var campuses = service.Queryable().Where(c => c.Id > 0);

        foreach (Campus campus in campuses)
        {
            campus.LoadAttributes();
            campus.GetAttributeValue(""test"");
            output += ""<li>"" + campus.Name + ""</li>"";
        }
        output += ""</ul>"";
        */
        
        // your code here
 
        return "" ""; // replace blank string with your return HTML
    }
}")]
    public partial class InjectCode : Rock.Web.UI.RockBlock
    {

        protected override void OnInit(EventArgs e)
        {
            this.EnableViewState = false;

            base.OnInit(e);
            this.AttributesUpdated += InjectCode_AttributesUpdated;
        }
        
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad(e);
            ExecuteScript();
        }

        /// <summary>
        /// Handles the AttributesUpdated event of the Inject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void InjectCode_AttributesUpdated(object sender, EventArgs e)
        {
            ExecuteScript();
        }

        protected void ExecuteScript()
        {
            // todo only show errors if user has edit rights to this block

            // put code results on the page
            string scriptSource = GetAttributeValue("Code");
            string scriptOutput = string.Empty;

            try
            {
                dynamic script = CSScript.Evaluator.LoadCode(scriptSource);
                scriptOutput = script.Execute(RockPage);
            }
            catch (Exception ex)
            {
                scriptOutput = "<div class='alert alert-warning'><h4>Script Error</h4><pre>" + ex.Message + "</pre></div>";
            }
            
            lOutput.Text += scriptOutput;
        }
    }
}