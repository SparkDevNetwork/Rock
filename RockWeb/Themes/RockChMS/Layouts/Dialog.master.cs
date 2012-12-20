//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Web.UI;

namespace RockWeb.Themes.RockChMS.Layouts
{
    public partial class Dialog : Rock.Web.UI.DialogMasterPage
    {
        protected void btnSave_Click( object sender, EventArgs e )
        {
            base.FireSave( sender, e );
        }

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            string script = @"
if ( window.parent.closeModal != null)
{
    window.parent.closeModal();
}
";
            ScriptManager.RegisterStartupScript( this.Page, this.GetType(), "close-modal", script, true );
        }

        protected override void OnInit( EventArgs e )
        {
            lTitle.Text = Request.QueryString["t"] ?? "Title";

            btnSave.Text = Request.QueryString["pb"] ?? "Save";
            btnSave.Visible = btnSave.Text.Trim() != string.Empty;

            btnCancel.Text = Request.QueryString["sb"] ?? "Cancel";
            btnCancel.Visible = btnCancel.Text.Trim() != string.Empty;
        }

    }
}
