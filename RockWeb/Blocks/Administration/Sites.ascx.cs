//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    public partial class Sites : Rock.Web.UI.Block
    {
        #region Fields
        
        Rock.CMS.SiteService siteService = new Rock.CMS.SiteService();
        Rock.CMS.SiteDomainService siteDomainService = new Rock.CMS.SiteDomainService();
        
        #endregion

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            if ( PageInstance.IsAuthorized( "Configure", CurrentUser ) )
            {
                gSites.DataKeyNames = new string[] { "id" };
                gSites.Actions.IsAddEnabled = true;
                gSites.Actions.AddClick += gSites_Add;
                gSites.GridRebind += gSites_GridRebind;
            }

            SecurityField securityField = gSites.Columns[3] as SecurityField;
            securityField.EntityType = typeof(Rock.CMS.Site);

            string script = @"
        Sys.Application.add_load(function () {
            $('td.grid-icon-cell.delete a').click(function(){
                return confirm('Are you sure you want to delete this site?');
                });
        });
    ";
            this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "grid-confirm-delete-{0}", gSites.ClientID ), script, true );

            base.OnInit( e );
        }

        protected override void OnLoad( EventArgs e )

        {
            nbMessage.Visible = false;

            if ( PageInstance.IsAuthorized( "Configure", CurrentUser ) )
            {
                if ( !Page.IsPostBack )
                {
                    BindGrid();
                    LoadDropDowns();
                }
            }
            else
            {
                gSites.Visible = false;
                nbMessage.Text = "You are not authorized to edit sites";
                nbMessage.Visible = true;
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        protected void gSites_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( ( int )gSites.DataKeys[e.RowIndex]["id"] );
        }

        protected void gSites_Delete( object sender, RowEventArgs e )
        {
            Rock.CMS.Site site = siteService.Get( ( int )gSites.DataKeys[e.RowIndex]["id"] );
            if ( BlockInstance != null )
            {
                siteService.Delete( site, CurrentPersonId );
                siteService.Save( site, CurrentPersonId );

                Rock.Web.Cache.Site.Flush( site.Id );
            }

            BindGrid();
        }

        void gSites_Add( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        void gSites_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Edit Events

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = false;
            pnlList.Visible = true;
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            Rock.CMS.Site site;
            Rock.CMS.SiteDomain sd;
            bool newSite = false;
                        
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                siteService = new Rock.CMS.SiteService();
                siteDomainService = new Rock.CMS.SiteDomainService();

                int siteId = 0;
                if ( !Int32.TryParse( hfSiteId.Value, out siteId ) )
                    siteId = 0;

                if ( siteId == 0 )
                {
                    newSite = true;
                    site = new Rock.CMS.Site();
                    siteService.Add( site, CurrentPersonId );
                }
                else
                {
                    site = siteService.Get( siteId );
                    foreach ( var domain in site.SiteDomains.ToList() )
                        siteDomainService.Delete( domain, CurrentPersonId );
                    site.SiteDomains.Clear();
                }

                site.Name = tbSiteName.Text;
                site.Description = tbDescription.Text;
                site.Theme = ddlTheme.Text;
                site.DefaultPageId = Convert.ToInt32( ddlDefaultPage.SelectedValue );

                foreach ( string domain in tbSiteDomains.Text.SplitDelimitedValues() )
                {
                    sd = new Rock.CMS.SiteDomain();
                    sd.Domain = domain;
                    sd.Guid = Guid.NewGuid();
                    site.SiteDomains.Add( sd );
                }

                site.FaviconUrl = tbFaviconUrl.Text;
                site.AppleTouchIconUrl = tbAppleTouchIconUrl.Text;
                site.FacebookAppId = tbFacebookAppId.Text;
                site.FacebookAppSecret = tbFacebookAppSecret.Text;

                siteService.Save( site, CurrentPersonId );

                if ( newSite )
                    Rock.Security.Authorization.CopyAuthorization( PageInstance.Site, site, CurrentPersonId );

                Rock.Web.Cache.Site.Flush( site.Id );

                BindGrid();

                pnlDetails.Visible = false;
                pnlList.Visible = true;
            }
        }

        #endregion

        #region Internal Methods

        private void BindGrid()
        {
            gSites.DataSource = siteService.Queryable().OrderBy( s => s.Name ).ToList();
            gSites.DataBind();
        }

        private void LoadDropDowns()
        {
            ddlDefaultPage.Items.Clear();
            foreach ( var page in new Rock.CMS.PageService().GetByParentPageId( null ) )
                AddPage( page, 1 );

            ddlTheme.Items.Clear();
            DirectoryInfo di = new DirectoryInfo( this.Page.Request.MapPath( this.ThemePath ) );
            foreach ( var themeDir in di.Parent.EnumerateDirectories())
                ddlTheme.Items.Add( new ListItem( themeDir.Name, themeDir.Name ) );
        }

        private void AddPage( Rock.CMS.Page page, int level )
        {
            string pageName = new string( '-', level ) + page.Name;
            ddlDefaultPage.Items.Add( new ListItem( pageName, page.Id.ToString() ) );
            foreach ( var childPage in page.Pages )
                AddPage( childPage, level + 1 );
        }

        protected void ShowEdit( int siteId )
        {
            Rock.CMS.Site site = siteService.Get( siteId );

            if ( site != null )
            {
                lAction.Text = "Edit";
                hfSiteId.Value = site.Id.ToString();

                tbSiteName.Text = site.Name;
                tbDescription.Text = site.Description;
                ddlTheme.SetValue( site.Theme );
                if ( site.DefaultPageId.HasValue )
                    ddlDefaultPage.SelectedValue = site.DefaultPageId.Value.ToString();

                tbSiteDomains.Text = string.Join("\n", site.SiteDomains.Select(dom => dom.Domain).ToArray());
                tbFaviconUrl.Text = site.FaviconUrl;
                tbAppleTouchIconUrl.Text = site.AppleTouchIconUrl;
                tbFacebookAppId.Text = site.FacebookAppId;
                tbFacebookAppSecret.Text = site.FacebookAppSecret;
            }
            else
            {
                lAction.Text = "Add";
                tbSiteName.Text = string.Empty;
                tbDescription.Text = string.Empty;
                ddlTheme.Text = PageInstance.Site.Theme;
                tbSiteDomains.Text = string.Empty;
                tbFaviconUrl.Text = string.Empty;
                tbAppleTouchIconUrl.Text = string.Empty;
                tbFacebookAppId.Text = string.Empty;
                tbFacebookAppSecret.Text = string.Empty;
            }

            pnlList.Visible = false;
            pnlDetails.Visible = true;
        }

        #endregion

    }
}