//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace RockWeb.Blocks.Administration
{
    public partial class PageProperties : Rock.Web.UI.Block
    {
        private Rock.Web.Cache.Page _page = null;
        private string _zoneName = string.Empty;

        protected override void OnInit( EventArgs e )
        {
            try
            {
                int pageId = Convert.ToInt32( PageParameter( "Page" ) );
                _page = Rock.Web.Cache.Page.Read( pageId );

                if ( _page.Authorized( "Configure", CurrentUser ) )
                {
                    var attributeControls = Rock.Attribute.Helper.GetEditControls( _page, !Page.IsPostBack );
                    foreach ( HtmlGenericControl fs in attributeControls )
                        phAttributes.Controls.Add( fs );
                }
                else
                {
                    DisplayError( "You are not authorized to edit this page" );
                }
            }
            catch ( SystemException ex )
            {
                DisplayError( ex.Message );
            }

            base.OnInit( e );
        }

        protected override void OnLoad( EventArgs e )
        {
            if (!Page.IsPostBack && _page.Authorized( "Configure", CurrentUser ) )
            {
                LoadDropdowns();

                tbPageName.Text = _page.Name;
                tbPageTitle.Text = _page.Title;
                ddlParentPage.SelectedValue = _page.ParentPage != null ? _page.ParentPage.Id.ToString() : "0";
                ddlLayout.Text = _page.Layout;
                ddlMenuWhen.SelectedValue = ( ( Int32 )_page.DisplayInNavWhen ).ToString();
                cbMenuDescription.Checked = _page.MenuDisplayDescription;
                cbMenuIcon.Checked = _page.MenuDisplayIcon;
                cbMenuChildPages.Checked = _page.MenuDisplayChildPages;
                cbRequiresEncryption.Checked = _page.RequiresEncryption;
                cbEnableViewState.Checked = _page.EnableViewstate;
                cbIncludeAdminFooter.Checked = _page.IncludeAdminFooter;
                tbCacheDuration.Text = _page.OutputCacheDuration.ToString();
                tbDescription.Text = _page.Description;
            }

            base.OnLoad( e );

            if ( Page.IsPostBack )
                Rock.Attribute.Helper.SetErrorIndicators( phAttributes, _page );
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if ( Page.IsValid )
            {
                using ( new Rock.Data.UnitOfWorkScope() )
                {
                    Rock.CMS.PageService pageService = new Rock.CMS.PageService();
                    Rock.CMS.Page page = pageService.Get( _page.Id );

                    int parentPage = Int32.Parse( ddlParentPage.SelectedValue );
                    if ( page.ParentPageId != parentPage )
                    {
                        if ( page.ParentPageId.HasValue )
                            Rock.Web.Cache.Page.Flush( page.ParentPageId.Value );

                        if ( parentPage != 0 )
                            Rock.Web.Cache.Page.Flush( parentPage );
                    }

                    page.Name = tbPageName.Text;
                    page.Title = tbPageTitle.Text;
                    if ( parentPage != 0 )
                        page.ParentPageId = parentPage;
                    else
                        page.ParentPageId = null;
                    page.Layout = ddlLayout.Text;
                    page.DisplayInNavWhen = ( Rock.CMS.DisplayInNavWhen )Enum.Parse( typeof( Rock.CMS.DisplayInNavWhen ), ddlMenuWhen.SelectedValue );
                    page.MenuDisplayDescription = cbMenuDescription.Checked;
                    page.MenuDisplayIcon = cbMenuIcon.Checked;
                    page.MenuDisplayChildPages = cbMenuChildPages.Checked;
                    page.RequiresEncryption = cbRequiresEncryption.Checked;
                    page.EnableViewState = cbRequiresEncryption.Checked;
                    page.IncludeAdminFooter = cbIncludeAdminFooter.Checked;
                    page.OutputCacheDuration = Int32.Parse( tbCacheDuration.Text );
                    page.Description = tbDescription.Text;

                    pageService.Save( page, CurrentPersonId );

                    Rock.Attribute.Helper.GetEditValues( phAttributes, _page );
                    _page.SaveAttributeValues( CurrentPersonId );

                    Rock.Web.Cache.Page.Flush( _page.Id );
                }

                string script = "window.parent.closeModal()";
                this.Page.ClientScript.RegisterStartupScript( this.GetType(), "close-modal", script, true );
            }
        }

        private void LoadDropdowns()
        {
            ddlParentPage.Items.Clear();
            ddlParentPage.Items.Add( new ListItem( "Root", "0" ) );
            foreach(var page in new Rock.CMS.PageService().GetByParentPageId(null))
                AddPage(page, 1);

            ddlLayout.Items.Clear();
            DirectoryInfo di = new DirectoryInfo( Path.Combine( this.Page.Request.MapPath( this.ThemePath ), "Layouts" ) );
            foreach ( FileInfo fi in di.GetFiles( "*.aspx" ) )
                ddlLayout.Items.Add( new ListItem( fi.Name.Remove( fi.Name.IndexOf( ".aspx" ) ) ) );

            ddlMenuWhen.BindToEnum( typeof( Rock.CMS.DisplayInNavWhen ) );
        }

        private void AddPage( Rock.CMS.Page page, int level )
        {
            string pageName = new string('-', level) + page.Name;
            ddlParentPage.Items.Add(new ListItem(pageName, page.Id.ToString()));
            foreach(var childPage in page.Pages)
                AddPage(childPage, level + 1);
        }

        private void DisplayError( string message )
        {
            pnlMessage.Controls.Clear();
            pnlMessage.Controls.Add( new LiteralControl( message ) );
            pnlMessage.Visible = true;

            phContent.Visible = false;
        }
    }
}