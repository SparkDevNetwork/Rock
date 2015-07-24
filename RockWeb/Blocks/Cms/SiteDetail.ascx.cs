// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Model;
using Rock.Data;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Site = Rock.Model.Site;
using Rock.Constants;
using Rock.Web;
using System.ComponentModel;
using Rock.Security;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Site Detail" )]
    [Category( "CMS" )]
    [Description( "Displays the details of a specific site." )]
    public partial class SiteDetail : RockBlock, IDetailBlock
    {

        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", Rock.Model.Site.FriendlyTypeName );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "siteId" ).AsInteger() );
            }

        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var site = new SiteService( new RockContext() ).Get( int.Parse( hfSiteId.Value ) );
            ShowEditDetails( site );
        }


        /// <summary>
        /// Handles the Click event of the btnDeleteCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDeleteCancel_Click( object sender, EventArgs e )
        {
            btnDelete.Visible = true;
            btnEdit.Visible = true;
            pnlDeleteConfirm.Visible = false;
        }


        /// <summary>
        /// Handles the Click event of the btnDeleteConfirm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDeleteConfirm_Click( object sender, EventArgs e )
        {
            bool canDelete = false;

            var rockContext = new RockContext();
            SiteService siteService = new SiteService( rockContext );
            Site site = siteService.Get( int.Parse( hfSiteId.Value ) );
            LayoutService layoutService = new LayoutService( rockContext );
            PageService pageService = new PageService( rockContext );
            PageViewService pageViewService = new PageViewService( rockContext );

            if ( site != null )
            {
                var sitePages = new List<int> {
                    site.DefaultPageId ?? -1,
                    site.LoginPageId ?? -1,
                    site.RegistrationPageId ?? -1, 
                    site.PageNotFoundPageId ?? -1
                };

                var pageViewQry = pageViewService.Queryable()
                    .Where( t =>
                        t.SiteId == site.Id);

                pageViewService.DeleteRange( pageViewQry );

                var pageQry = pageService.Queryable( "Layout" )
                    .Where( t =>
                        t.Layout.SiteId == site.Id ||
                        sitePages.Contains( t.Id ) );

                pageService.DeleteRange( pageQry );

                var layoutQry = layoutService.Queryable()
                    .Where( l =>
                        l.SiteId == site.Id );
                layoutService.DeleteRange( layoutQry );
                rockContext.SaveChanges( true );

                string errorMessage;
                canDelete = siteService.CanDelete( site, out errorMessage, includeSecondLvl: true );
                if ( !canDelete )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Alert );
                    return;
                }

                siteService.Delete( site );

                rockContext.SaveChanges();

                SiteCache.Flush( site.Id );
            }

            NavigateToParentPage();
        }
        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            btnDelete.Visible = false;
            btnEdit.Visible = false;
            pnlDeleteConfirm.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            Site site;

            if ( Page.IsValid )
            {
                var rockContext = new RockContext();
                SiteService siteService = new SiteService( rockContext );
                SiteDomainService siteDomainService = new SiteDomainService( rockContext );
                bool newSite = false;

                int siteId = int.Parse( hfSiteId.Value );

                if ( siteId == 0 )
                {
                    newSite = true;
                    site = new Rock.Model.Site();
                    siteService.Add( site );
                }
                else
                {
                    site = siteService.Get( siteId );
                }

                site.Name = tbSiteName.Text;
                site.Description = tbDescription.Text;
                site.Theme = ddlTheme.Text;
                site.DefaultPageId = ppDefaultPage.PageId;
                site.DefaultPageRouteId = ppDefaultPage.PageRouteId;
                site.LoginPageId = ppLoginPage.PageId;
                site.LoginPageRouteId = ppLoginPage.PageRouteId;
                site.CommunicationPageId = ppCommunicationPage.PageId;
                site.CommunicationPageRouteId = ppCommunicationPage.PageRouteId;
                site.RegistrationPageId = ppRegistrationPage.PageId;
                site.RegistrationPageRouteId = ppRegistrationPage.PageRouteId;
                site.PageNotFoundPageId = ppPageNotFoundPage.PageId;
                site.PageNotFoundPageRouteId = ppPageNotFoundPage.PageRouteId;
                site.ErrorPage = tbErrorPage.Text;
                site.GoogleAnalyticsCode = tbGoogleAnalytics.Text;

                var currentDomains = tbSiteDomains.Text.SplitDelimitedValues().ToList<string>();
                site.SiteDomains = site.SiteDomains ?? new List<SiteDomain>();

                // Remove any deleted domains
                foreach ( var domain in site.SiteDomains.Where( w => !currentDomains.Contains( w.Domain ) ).ToList() )
                {
                    site.SiteDomains.Remove( domain );
                    siteDomainService.Delete( domain );
                }

                foreach ( string domain in currentDomains )
                {
                    SiteDomain sd = site.SiteDomains.Where( d => d.Domain == domain ).FirstOrDefault();
                    if ( sd == null )
                    {
                        sd = new SiteDomain();
                        sd.Domain = domain;
                        sd.Guid = Guid.NewGuid();
                        site.SiteDomains.Add( sd );
                    }
                }

                if ( !site.DefaultPageId.HasValue && !newSite )
                {
                    ppDefaultPage.ShowErrorMessage( "Default Page is required." );
                    return;
                }

                if ( !site.IsValid )
                {
                    // Controls will render the error messages                    
                    return;
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();

                    if ( newSite )
                    {
                        Rock.Security.Authorization.CopyAuthorization( RockPage.Layout.Site, site, rockContext );
                    }
                } );

                SiteCache.Flush( site.Id );

                // Create the default page is this is a new site
                if ( !site.DefaultPageId.HasValue && newSite )
                {
                    var siteCache = SiteCache.Read( site.Id );

                    // Create the layouts for the site, and find the first one
                    LayoutService.RegisterLayouts( Request.MapPath( "~" ), siteCache );

                    var layoutService = new LayoutService( rockContext );
                    var layouts = layoutService.GetBySiteId( siteCache.Id );
                    Layout layout = layouts.FirstOrDefault( l => l.FileName.Equals( "FullWidth", StringComparison.OrdinalIgnoreCase ) );
                    if ( layout == null )
                    {
                        layout = layouts.FirstOrDefault();
                    }
                    if ( layout != null )
                    {
                        var pageService = new PageService( rockContext );
                        var page = new Page();
                        page.LayoutId = layout.Id;
                        page.PageTitle = siteCache.Name + " Home Page";
                        page.InternalName = page.PageTitle;
                        page.BrowserTitle = page.PageTitle;
                        page.EnableViewState = true;
                        page.IncludeAdminFooter = true;
                        page.MenuDisplayChildPages = true;

                        var lastPage = pageService.GetByParentPageId( null ).
                            OrderByDescending( b => b.Order ).FirstOrDefault();

                        page.Order = lastPage != null ? lastPage.Order + 1 : 0;
                        pageService.Add( page );

                        rockContext.SaveChanges();

                        site = siteService.Get( siteCache.Id );
                        site.DefaultPageId = page.Id;

                        rockContext.SaveChanges();

                        SiteCache.Flush( site.Id );
                    }
                }

                var qryParams = new Dictionary<string, string>();
                qryParams["siteId"] = site.Id.ToString();

                NavigateToPage( RockPage.Guid, qryParams );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfSiteId.Value.Equals( "0" ) )
            {
                // Cancelling on Add return to site list
                NavigateToParentPage();
            }
            else
            {
                // Cancelling on edit, return to details
                var site = new SiteService( new RockContext() ).Get( int.Parse( hfSiteId.Value ) );
                ShowReadonlyDetails( site );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            ddlTheme.Items.Clear();
            DirectoryInfo di = new DirectoryInfo( this.Page.Request.MapPath( ResolveRockUrl( "~~" ) ) );
            foreach ( var themeDir in di.Parent.EnumerateDirectories().OrderBy( a => a.Name ) )
            {
                ddlTheme.Items.Add( new ListItem( themeDir.Name, themeDir.Name ) );
            }
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="siteId">The site id.</param>
        public void ShowDetail( int siteId )
        {
            pnlDetails.Visible = false;

            Site site = null;

            if ( !siteId.Equals( 0 ) )
            {
                site = new SiteService( new RockContext() ).Get( siteId );
            }

            if ( site == null )
            {
                site = new Site { Id = 0 };
                site.SiteDomains = new List<SiteDomain>();
                site.Theme = RockPage.Layout.Site.Theme;
            }

            pnlDetails.Visible = true;
            hfSiteId.Value = site.Id.ToString();

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Rock.Model.Site.FriendlyTypeName );
            }

            if ( site.IsSystem )
            {
                nbEditModeMessage.Text = EditModeMessage.System( Rock.Model.Site.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                ShowReadonlyDetails( site );
            }
            else
            {
                btnEdit.Visible = true;
                btnDelete.Visible = !site.IsSystem;
                if ( site.Id > 0 )
                {
                    ShowReadonlyDetails( site );
                }
                else
                {
                    ShowEditDetails( site );
                }
            }

        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="group">The group.</param>
        private void ShowEditDetails( Rock.Model.Site site )
        {
            if ( site.Id == 0 )
            {
                nbDefaultPageNotice.Visible = true;
                lReadOnlyTitle.Text = ActionTitle.Add( Rock.Model.Site.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                nbDefaultPageNotice.Visible = false;
                lReadOnlyTitle.Text = site.Name.FormatAsHtmlTitle();
            }

            SetEditMode( true );

            LoadDropDowns();

            tbSiteName.ReadOnly = site.IsSystem;
            tbSiteName.Text = site.Name;

            tbDescription.ReadOnly = site.IsSystem;
            tbDescription.Text = site.Description;

            ddlTheme.Enabled = !site.IsSystem;
            ddlTheme.SetValue( site.Theme );

            if ( site.DefaultPageRoute != null )
            {
                ppDefaultPage.SetValue( site.DefaultPageRoute );
            }
            else
            {
                ppDefaultPage.SetValue( site.DefaultPage );
            }

            if ( site.LoginPageRoute != null )
            {
                ppLoginPage.SetValue( site.LoginPageRoute );
            }
            else
            {
                ppLoginPage.SetValue( site.LoginPage );
            }

            if ( site.CommunicationPageRoute != null )
            {
                ppCommunicationPage.SetValue( site.CommunicationPageRoute );
            }
            else
            {
                ppCommunicationPage.SetValue( site.CommunicationPage );
            }

            if ( site.RegistrationPageRoute != null )
            {
                ppRegistrationPage.SetValue( site.RegistrationPageRoute );
            }
            else
            {
                ppRegistrationPage.SetValue( site.RegistrationPage );
            }

            if ( site.PageNotFoundPageRoute != null )
            {
                ppPageNotFoundPage.SetValue( site.PageNotFoundPageRoute );
            }
            else
            {
                ppPageNotFoundPage.SetValue( site.PageNotFoundPage );
            }

            tbErrorPage.Text = site.ErrorPage;

            tbSiteDomains.Text = string.Join( "\n", site.SiteDomains.Select( dom => dom.Domain ).ToArray() );
            tbGoogleAnalytics.Text = site.GoogleAnalyticsCode;
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="group">The group.</param>
        private void ShowReadonlyDetails( Rock.Model.Site site )
        {
            SetEditMode( false );

            hfSiteId.SetValue( site.Id );
            lReadOnlyTitle.Text = site.Name.FormatAsHtmlTitle();

            lSiteDescription.Text = site.Description;

            DescriptionList descriptionList = new DescriptionList();
            descriptionList.Add( "Domain(s)", site.SiteDomains.Select( d => d.Domain ).ToList().AsDelimited( ", " ) );
            descriptionList.Add( "Theme", site.Theme );
            descriptionList.Add( "Default Page", site.DefaultPageRoute );
            lblMainDetails.Text = descriptionList.Html;
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        #endregion
    }
}