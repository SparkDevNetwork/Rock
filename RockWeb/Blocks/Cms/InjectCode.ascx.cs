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
    [CodeEditorField("Code", "Source code to compile and put results on page.", CodeEditorMode.CSharp, CodeEditorTheme.Rock, true, "using%20System%3B%0Ausing%20System.Linq%3B%0Ausing%20Rock%3B%0Ausing%20Rock.Data%3B%0Ausing%20Rock.Model%3B%0Ausing%20Rock.Web%3B%0Ausing%20Rock.Web.UI%3B%0Ausing%20Rock.Web.UI.Controls%3B%0A%0Apublic%20class%20Script%0A%7B%20%0A%20%20%20%20%2F%2F%20the%20execute%20method%20method%20will%20be%20called%20by%20the%20block%0A%20%20%20%20%2F%2F%20the%20string%20returned%20will%20be%20placed%20on%20the%20page%0A%20%20%20%20public%20string%20Execute(Rock.Web.UI.RockPage%20currentPage)%0A%20%20%20%20%7B%0A%20%20%20%20%20%20%20%20%2F*%20sample%20items%0A%20%20%20%20%20%20%20%20%0A%20%20%20%20%20%20%20%20--%20change%20page%20title%0A%20%20%20%20%20%20%20%20currentPage.Title%20%3D%20%22I%20Now%20Own%20The%20Title%22%3B%0A%20%20%20%20%20%20%20%20%0A%20%20%20%20%20%20%20%20--%20get%20current%20user%27s%20name%0A%20%20%20%20%20%20%20%20string%20user%20%3D%20currentPage.CurrentPerson.FullName%3B%0A%20%20%20%20%20%20%20%20%0A%20%20%20%20%20%20%20%20--%20get%20current%20user%27s%20login%20id%0A%20%20%20%20%20%20%20%20string%20login%20%3D%20currentPage.CurrentUser.Username%3B%0A%20%20%20%20%20%20%20%20%0A%20%20%20%20%20%20%20%20--%20get%20request%20parm%0A%20%20%20%20%20%20%20%20string%20parm%20%3D%20currentPage.Request%5B%22parm%22%5D%3B%0A%20%20%20%20%20%20%20%20%0A%20%20%20%20%20%20%20%20--%20set%20the%20theme%0A%20%20%20%20%20%20%20%20currentPage.Theme%20%3D%20%27Stark%27%3B%0A%20%20%20%20%20%20%20%20%0A%20%20%20%20%20%20%20%20--%20redirect%20the%20page%0A%20%20%20%20%20%20%20%20currentPage.Response.Redirect(%22http%3A%2F%2Fwww.rockchms.com%22)%3B%0A%20%20%20%20%20%20%20%20%0A%20%20%20%20%20%20%20%20--%20modify%20breadcrumbs%0A%20%20%20%20%20%20%20%20currentPage.BreadCrumbs.Add(new%20Rock.Web.UI.BreadCrumb(%22Title%22%2C%20%22%2Fpage%2F12%22))%3B%20%2F%2F%20adds%20a%20new%20crumb%0A%20%20%20%20%20%20%20%20currentPage.BreadCrumbs.Clear()%3B%20%2F%2F%20removes%20all%20crumbs%0A%20%20%20%20%20%20%20%20currentPage.BreadCrumbs.RemoveAt(currentPage..BreadCrumbs.Count%20-1)%3B%20%2F%2F%20removes%20the%20last%20one%0A%20%20%20%20%20%20%20%20%0A%20%20%20%20%20%20%20%20--%20output%20data%20from%20the%20database%20(lists%20all%20campuses%20with%20an%20Id%20%3E%200)%0A%20%20%20%20%20%20%20%20string%20output%20%3D%20%22%22%3B%0A%20%20%20%20%20%20%20%20var%20service%20%3D%20new%20CampusService()%3B%0A%20%20%20%20%20%20%20%20var%20campuses%20%3D%20service.Queryable().Where(c%20%3D%3E%20c.Id%20%3E%200)%3B%0A%0A%20%20%20%20%20%20%20%20foreach%20(Campus%20campus%20in%20campuses)%0A%20%20%20%20%20%20%20%20%7B%0A%20%20%20%20%20%20%20%20%20%20%20%20output%20%2B%3D%20%22%22%20%2B%20campus.Name%20%2B%20%22%22%3B%0A%20%20%20%20%20%20%20%20%7D%0A%20%20%20%20%20%20%20%20output%20%2B%3D%20%22%22%3B%0A%20%20%20%20%20%20%20%20*%2F%0A%20%20%20%20%20%20%20%20%0A%20%20%20%20%20%20%20%20%2F%2F%20your%20code%20here%0A%20%20%20%20%20%20%20%0A%20%20%20%20%20%20%20%20%0A%20%20%20%20%20%20%20%20%0A%20%20%20%20%20%20%20%20return%20%22%20%22%3B%20%2F%2F%20replace%20blank%20string%20with%20your%20return%20HTML%0A%20%20%20%20%7D%0A%7D")]
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
                dynamic script = CSScript.Evaluator.LoadCode(HttpUtility.UrlDecode(scriptSource));
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