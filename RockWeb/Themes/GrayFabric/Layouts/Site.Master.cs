//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace RockWeb.Themes.GrayFabric.Layouts
{
    public partial class Site : System.Web.UI.MasterPage
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            //string validationScript = string.Format("<script src=\"{0}\"></script>", ResolveUrl("~/Scripts/Rock/validation.js"));
            //System.Web.UI.ScriptManager.RegisterStartupScript(Page, Page.GetType(), "validationScript", validationScript, false );
        }
    }
}