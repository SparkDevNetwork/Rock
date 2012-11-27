//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;
using Rock;
using Rock.Cms;
using Rock.Data;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Sites : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( CurrentPage.IsAuthorized( "Configure", CurrentPerson ) )
            {
                gSites.DataKeyNames = new string[] { "id" };
                gSites.Actions.IsAddEnabled = true;
                gSites.Actions.AddClick += gSites_Add;
                gSites.GridRebind += gSites_GridRebind;
            }

            SecurityField securityField = gSites.Columns[3] as SecurityField;
            securityField.EntityType = typeof( Rock.Cms.Site );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            nbMessage.Visible = false;

            if ( CurrentPage.IsAuthorized( "Configure", CurrentPerson ) )
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

        /// <summary>
        /// Handles the Add event of the gSites control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gSites_Add( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gSites control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSites_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( (int)gSites.DataKeys[e.RowIndex]["id"] );
        }

        /// <summary>
        /// Handles the Delete event of the gSites control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSites_Delete( object sender, RowEventArgs e )
        {
            SiteService siteService = new SiteService();
            Site site = siteService.Get( (int)gSites.DataKeys[e.RowIndex]["id"] );
            if ( CurrentBlock != null )
            {
                siteService.Delete( site, CurrentPersonId );
                siteService.Save( site, CurrentPersonId );

                SiteCache.Flush( site.Id );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gSites control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gSites_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = false;
            pnlList.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            Site site;
            SiteDomain sd;
            bool newSite = false;

            using ( new UnitOfWorkScope() )
            {
                SiteService siteService = new SiteService();
                SiteDomainService siteDomainService = new SiteDomainService();

                int siteId = 0;
                if ( !int.TryParse( hfSiteId.Value, out siteId ) )
                {
                    siteId = 0;
                }

                if ( siteId == 0 )
                {
                    newSite = true;
                    site = new Rock.Cms.Site();
                    siteService.Add( site, CurrentPersonId );
                }
                else
                {
                    site = siteService.Get( siteId );
                    foreach ( var domain in site.SiteDomains.ToList() )
                    {
                        siteDomainService.Delete( domain, CurrentPersonId );
                    }

                    site.SiteDomains.Clear();
                }

                site.Name = tbSiteName.Text;
                site.Description = tbDescription.Text;
                site.Theme = ddlTheme.Text;
                site.DefaultPageId = int.Parse( ddlDefaultPage.SelectedValue );

                foreach ( string domain in tbSiteDomains.Text.SplitDelimitedValues() )
                {
                    sd = new SiteDomain();
                    sd.Domain = domain;
                    sd.Guid = Guid.NewGuid();
                    site.SiteDomains.Add( sd );
                }

                site.FaviconUrl = tbFaviconUrl.Text;
                site.AppleTouchIconUrl = tbAppleTouchIconUrl.Text;
                site.FacebookAppId = tbFacebookAppId.Text;
                site.FacebookAppSecret = tbFacebookAppSecret.Text;

                if ( !site.IsValid )
                {
                    // Controls will render the error messages                    
                    return;
                }

                siteService.Save( site, CurrentPersonId );

                if ( newSite )
                {
                    Rock.Security.Authorization.CopyAuthorization( CurrentPage.Site, site, CurrentPersonId );
                }

                SiteCache.Flush( site.Id );

                BindGrid();

                pnlDetails.Visible = false;
                pnlList.Visible = true;
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            SiteService siteService = new SiteService();
            gSites.DataSource = siteService.Queryable().OrderBy( s => s.Name ).ToList();
            gSites.DataBind();
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            PageService pageService = new PageService();
            List<Rock.Cms.Page> allPages = pageService.Queryable().ToList();
            ddlDefaultPage.DataSource = allPages.OrderBy( a => a.PageSortHash );
            ddlDefaultPage.DataBind();

            ddlTheme.Items.Clear();
            DirectoryInfo di = new DirectoryInfo( this.Page.Request.MapPath( this.CurrentTheme ) );
            foreach ( var themeDir in di.Parent.EnumerateDirectories().OrderBy( a => a.Name ) )
            {
                ddlTheme.Items.Add( new ListItem( themeDir.Name, themeDir.Name ) );
            }
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="siteId">The site id.</param>
        protected void ShowEdit( int siteId )
        {
            SiteService siteService = new SiteService();
            Site site = siteService.Get( siteId );

            if ( site != null )
            {
                lAction.Text = "Edit";
                hfSiteId.Value = site.Id.ToString();

                tbSiteName.Text = site.Name;
                tbDescription.Text = site.Description;
                ddlTheme.SetValue( site.Theme );
                if ( site.DefaultPageId.HasValue )
                {
                    ddlDefaultPage.SelectedValue = site.DefaultPageId.Value.ToString();
                }

                tbSiteDomains.Text = string.Join( "\n", site.SiteDomains.Select( dom => dom.Domain ).ToArray() );
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
                ddlTheme.Text = CurrentPage.Site.Theme;
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