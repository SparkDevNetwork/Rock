using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.IO;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace RockWeb.Blocks.Administration
{
    public partial class PageProperties : Rock.Cms.CmsBlock
    {
        private Rock.Cms.Cached.Page _page = null;
        private string _zoneName = string.Empty;

        protected override void OnInit( EventArgs e )
        {
            try
            {
                int pageId = Convert.ToInt32( PageParameter( "Page" ) );
                _page = Rock.Cms.Cached.Page.Read( pageId );

                if ( _page.Authorized( "Configure", CurrentUser ) )
                {
                    foreach ( HtmlGenericControl li in Rock.Attribute.Helper.GetEditControls( _page, !Page.IsPostBack ) )
                        olProperties.Controls.Add( li );
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
        }
        protected void btnSave_Click(object sender, EventArgs e)
        {
            using ( new Rock.Helpers.UnitOfWorkScope() )
            {
                Rock.Services.Cms.PageService pageService = new Rock.Services.Cms.PageService();
                Rock.Models.Cms.Page page = pageService.Get( _page.Id );

                int parentPage = Int32.Parse( ddlParentPage.SelectedValue );
                if ( page.ParentPageId != parentPage )
                {
                    if (page.ParentPageId.HasValue)
                        Rock.Cms.Cached.Page.Flush( page.ParentPageId.Value );

                    if (parentPage != 0)
                        Rock.Cms.Cached.Page.Flush( parentPage );
                }

                page.Name = tbPageName.Text;
                page.Title = tbPageTitle.Text;
                page.ParentPageId = parentPage;
                page.Layout = ddlLayout.Text;
                page.DisplayInNavWhen = ( Rock.Models.Cms.DisplayInNavWhen )Enum.Parse( typeof( Rock.Models.Cms.DisplayInNavWhen ), ddlMenuWhen.SelectedValue );
                page.MenuDisplayDescription = cbMenuDescription.Checked;
                page.MenuDisplayIcon = cbMenuIcon.Checked;
                page.MenuDisplayChildPages = cbMenuChildPages.Checked;
                page.RequiresEncryption = cbRequiresEncryption.Checked;
                page.EnableViewState = cbRequiresEncryption.Checked;
                page.IncludeAdminFooter = cbIncludeAdminFooter.Checked;
                page.OutputCacheDuration = Int32.Parse( tbCacheDuration.Text );
                page.Description = tbDescription.Text;
                
                pageService.Save( page, CurrentPersonId );

                Rock.Attribute.Helper.GetEditValues( olProperties, page );
                _page.SaveAttributeValues( CurrentPersonId );

                Rock.Cms.Cached.Page.Flush( _page.Id );
            }

            phClose.Controls.AddAt(0, new LiteralControl( @"
    <script type='text/javascript'>
        window.parent.$('#modalDiv').dialog('close');
    </script>
" ));
        }

        private void LoadDropdowns()
        {
            ddlParentPage.Items.Clear();
            ddlParentPage.Items.Add( new ListItem( "Root", "0" ) );
            foreach(var page in new Rock.Services.Cms.PageService().GetByParentPageId(null))
                AddPage(page, 1);

            ddlLayout.Items.Clear();
            DirectoryInfo di = new DirectoryInfo( Path.Combine( this.Page.Request.MapPath( this.ThemePath ), "Layouts" ) );
            foreach ( FileInfo fi in di.GetFiles( "*.aspx.cs" ) )
                ddlLayout.Items.Add( new ListItem( fi.Name.Remove( fi.Name.IndexOf( ".aspx.cs" ) ) ) );

            ddlMenuWhen.BindToEnum( typeof( Rock.Models.Cms.DisplayInNavWhen ) );
        }

        private void AddPage( Rock.Models.Cms.Page page, int level )
        {
            string pageName = new string('-', level) + page.Name;
            ddlParentPage.Items.Add(new ListItem(pageName, page.Id.ToString()));
            foreach(var childPage in page.Pages)
                AddPage(childPage, level + 1);
        }
    }
}