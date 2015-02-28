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
using System.ComponentModel;
using System.Linq;
using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Services.NuGet;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Page Properties" )]
    [Category( "Administration" )]
    [Description( "Displays the page properties." )]
    public partial class PageProperties : RockBlock
    {
        #region Fields

        private int? _pageId = null;

        //// Import/Export hidden until we have time to get it working again.
        //// private readonly List<string> _tabs = new List<string> { "Basic Settings", "Display Settings", "Advanced Settings", "Import/Export"} ;
        private readonly List<string> _tabs = new List<string> { "Basic Settings", "Display Settings", "Advanced Settings" };

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the current tab.
        /// </summary>
        /// <value>
        /// The current tab.
        /// </value>
        protected string CurrentTab
        {
            get
            {
                object currentProperty = ViewState["CurrentTab"];
                return currentProperty != null ? currentProperty.ToString() : "Basic Settings";
            }

            set
            {
                ViewState["CurrentTab"] = value;
            }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            try
            {
                int? pageId = PageParameter( "Page" ).AsIntegerOrNull();
                if ( pageId.HasValue )
                {
                    // hide the current page in the page picker to prevent setting this page's parent page to itself (or one of it's child pages)
                    ppParentPage.HiddenPageIds = new int[] { pageId.Value };

                    var pageCache = Rock.Web.Cache.PageCache.Read( pageId.Value );

                    DialogPage dialogPage = this.Page as DialogPage;
                    if ( dialogPage != null )
                    {
                        dialogPage.OnSave += new EventHandler<EventArgs>( masterPage_OnSave );
                        dialogPage.SubTitle = string.Format( "Id: {0}", pageCache.Id );
                    }

                    if ( pageCache.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                    {
                        ddlMenuWhen.BindToEnum<DisplayInNavWhen>();

                        var blockContexts = new Dictionary<string, string>();
                        foreach ( var block in pageCache.Blocks )
                        {
                            var blockControl = TemplateControl.LoadControl( block.BlockType.Path ) as RockBlock;
                            if ( blockControl != null )
                            {
                                blockControl.SetBlock( pageCache, block );
                                foreach ( var context in blockControl.ContextTypesRequired )
                                {
                                    if ( !blockContexts.ContainsKey( context.Name ) )
                                    {
                                        blockContexts.Add( context.Name, context.FriendlyName );
                                    }
                                }
                            }
                        }

                        phContextPanel.Visible = blockContexts.Count > 0;

                        foreach ( var context in blockContexts )
                        {
                            var tbContext = new RockTextBox();
                            tbContext.ID = string.Format( "context_{0}", context.Key.Replace( '.', '_' ) );
                            tbContext.Required = true;
                            tbContext.Label = context.Value + " Parameter Name";
                            tbContext.Help = "The page parameter name that contains the id of this context entity.";
                            if ( pageCache.PageContexts.ContainsKey( context.Key ) )
                            {
                                tbContext.Text = pageCache.PageContexts[context.Key];
                            }

                            phContext.Controls.Add( tbContext );
                        }

                        _pageId = pageCache.Id;
                    }
                    else
                    {
                        DisplayError( "You are not authorized to administrate this page" );
                    }
                }
                else
                {
                    DisplayError( "Invalid Page Id value" );
                }
            }
            catch ( SystemException ex )
            {
                DisplayError( ex.Message );
            }

            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack && _pageId.HasValue )
            {
                var rockContext = new RockContext();

                LoadSites( rockContext );

                PageService pageService = new PageService( rockContext );
                Rock.Model.Page page = pageService.Queryable( "Layout,PageRoutes" )
                    .Where( p => p.Id == _pageId.Value )
                    .FirstOrDefault();

                if ( page.Layout != null )
                {
                    ddlSite.SelectedValue = page.Layout.SiteId.ToString();
                    LoadLayouts( rockContext, SiteCache.Read( page.Layout.SiteId ) );
                    ddlLayout.SelectedValue = page.Layout.Id.ToString();
                }

                rptProperties.DataSource = _tabs;
                rptProperties.DataBind();

                tbPageName.Text = page.InternalName;
                tbPageTitle.Text = page.PageTitle;
                tbBrowserTitle.Text = page.BrowserTitle;
                ppParentPage.SetValue( pageService.Get( page.ParentPageId ?? 0 ) );
                tbIconCssClass.Text = page.IconCssClass;

                cbPageTitle.Checked = page.PageDisplayTitle;
                cbPageBreadCrumb.Checked = page.PageDisplayBreadCrumb;
                cbPageIcon.Checked = page.PageDisplayIcon;
                cbPageDescription.Checked = page.PageDisplayDescription;

                ddlMenuWhen.SelectedValue = ( (int)page.DisplayInNavWhen ).ToString();
                cbMenuDescription.Checked = page.MenuDisplayDescription;
                cbMenuIcon.Checked = page.MenuDisplayIcon;
                cbMenuChildPages.Checked = page.MenuDisplayChildPages;

                cbBreadCrumbIcon.Checked = page.BreadCrumbDisplayIcon;
                cbBreadCrumbName.Checked = page.BreadCrumbDisplayName;

                cbRequiresEncryption.Checked = page.RequiresEncryption;
                cbEnableViewState.Checked = page.EnableViewState;
                cbIncludeAdminFooter.Checked = page.IncludeAdminFooter;
                tbCacheDuration.Text = page.OutputCacheDuration.ToString();
                tbDescription.Text = page.Description;
                ceHeaderContent.Text = page.HeaderContent;
                tbPageRoute.Text = string.Join( ",", page.PageRoutes.Select( route => route.Route ).ToArray() );

                // Add enctype attribute to page's <form> tag to allow file upload control to function
                Page.Form.Attributes.Add( "enctype", "multipart/form-data" );
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbProperty control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbProperty_Click( object sender, EventArgs e )
        {
            LinkButton lb = sender as LinkButton;
            if ( lb != null )
            {
                CurrentTab = lb.Text;

                rptProperties.DataSource = _tabs;
                rptProperties.DataBind();
            }

            ShowSelectedPane();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlSite control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlSite_SelectedIndexChanged( object sender, EventArgs e )
        {
            LoadLayouts( new RockContext(), SiteCache.Read( ddlSite.SelectedValueAsInt().Value ) );
        }

        /// <summary>
        /// Handles the OnSave event of the masterPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void masterPage_OnSave( object sender, EventArgs e )
        {
            Page.Validate( BlockValidationGroup );
            if ( Page.IsValid && _pageId.HasValue )
            {
                var rockContext = new RockContext();
                var pageService = new PageService( rockContext );
                var routeService = new PageRouteService( rockContext );
                var contextService = new PageContextService( rockContext );

                var page = pageService.Get( _pageId.Value );

                // validate/check for removed routes
                var editorRoutes = tbPageRoute.Text.SplitDelimitedValues().Distinct();
                var databasePageRoutes = page.PageRoutes.ToList();
                var deletedRouteIds = new List<int>();
                var addedRoutes = new List<string>();

                // validate if removed routes can be deleted
                foreach ( var pageRoute in databasePageRoutes )
                {
                    if ( !editorRoutes.Contains( pageRoute.Route ) )
                    {
                        // make sure the route can be deleted
                        string errorMessage;
                        if ( !routeService.CanDelete( pageRoute, out errorMessage ) )
                        {
                            nbPageRouteWarning.Text = string.Format( "The page route <strong>{0}</strong>, cannot be removed. {1}", pageRoute.Route, errorMessage );
                            nbPageRouteWarning.NotificationBoxType = NotificationBoxType.Warning;
                            nbPageRouteWarning.Dismissable = true;
                            nbPageRouteWarning.Visible = true;
                            CurrentTab = "Advanced Settings";

                            rptProperties.DataSource = _tabs;
                            rptProperties.DataBind();
                            ShowSelectedPane();
                            return;
                        }
                    }
                }

                // take care of deleted routes
                foreach ( var pageRoute in databasePageRoutes )
                {
                    if ( !editorRoutes.Contains( pageRoute.Route ) )
                    {
                        // if they removed the Route, remove it from the database
                        page.PageRoutes.Remove( pageRoute );

                        routeService.Delete( pageRoute );
                        deletedRouteIds.Add( pageRoute.Id );
                    }
                }

                // take care of added routes
                foreach ( string route in editorRoutes )
                {
                    // if they added the Route, add it to the database
                    if ( !databasePageRoutes.Any( a => a.Route == route ) )
                    {
                        var pageRoute = new PageRoute();
                        pageRoute.Route = route.TrimStart( new char[] { '/' } );
                        pageRoute.Guid = Guid.NewGuid();
                        page.PageRoutes.Add( pageRoute );
                        addedRoutes.Add( route );
                    }
                }

                int parentPageId = ppParentPage.SelectedValueAsInt() ?? 0;
                if ( page.ParentPageId != parentPageId )
                {
                    if ( page.ParentPageId.HasValue )
                    {
                        PageCache.Flush( page.ParentPageId.Value );
                    }

                    if ( parentPageId != 0 )
                    {
                        PageCache.Flush( parentPageId );
                    }
                }

                page.InternalName = tbPageName.Text;
                page.PageTitle = tbPageTitle.Text;
                page.BrowserTitle = tbBrowserTitle.Text;
                if ( parentPageId != 0 )
                {
                    page.ParentPageId = parentPageId;
                }
                else
                {
                    page.ParentPageId = null;
                }

                page.LayoutId = ddlLayout.SelectedValueAsInt().Value;

                int? orphanedIconFileId = null;

                page.IconCssClass = tbIconCssClass.Text;

                page.PageDisplayTitle = cbPageTitle.Checked;
                page.PageDisplayBreadCrumb = cbPageBreadCrumb.Checked;
                page.PageDisplayIcon = cbPageIcon.Checked;
                page.PageDisplayDescription = cbPageDescription.Checked;

                page.DisplayInNavWhen = ddlMenuWhen.SelectedValue.ConvertToEnumOrNull<DisplayInNavWhen>() ?? DisplayInNavWhen.WhenAllowed;
                page.MenuDisplayDescription = cbMenuDescription.Checked;
                page.MenuDisplayIcon = cbMenuIcon.Checked;
                page.MenuDisplayChildPages = cbMenuChildPages.Checked;

                page.BreadCrumbDisplayName = cbBreadCrumbName.Checked;
                page.BreadCrumbDisplayIcon = cbBreadCrumbIcon.Checked;

                page.RequiresEncryption = cbRequiresEncryption.Checked;
                page.EnableViewState = cbEnableViewState.Checked;
                page.IncludeAdminFooter = cbIncludeAdminFooter.Checked;
                page.OutputCacheDuration = tbCacheDuration.Text.AsIntegerOrNull() ?? 0;
                page.Description = tbDescription.Text;
                page.HeaderContent = ceHeaderContent.Text;

                // update PageContexts
                foreach ( var pageContext in page.PageContexts.ToList() )
                {
                    contextService.Delete( pageContext );
                }

                page.PageContexts.Clear();
                foreach ( var control in phContext.Controls )
                {
                    if ( control is RockTextBox )
                    {
                        var tbContext = control as RockTextBox;
                        if ( !string.IsNullOrWhiteSpace( tbContext.Text ) )
                        {
                            var pageContext = new PageContext();
                            pageContext.Entity = tbContext.ID.Substring( 8 ).Replace( '_', '.' );
                            pageContext.IdParameter = tbContext.Text;
                            page.PageContexts.Add( pageContext );
                        }
                    }
                }

                // save page and it's routes
                if ( page.IsValid )
                {
                    rockContext.SaveChanges();

                    // remove any routes that were deleted
                    foreach (var deletedRouteId in deletedRouteIds )
                    {
                        var existingRoute = RouteTable.Routes.OfType<Route>().FirstOrDefault( a => a.RouteId() == deletedRouteId );
                        if ( existingRoute != null )
                        {
                            RouteTable.Routes.Remove( existingRoute );
                        }
                    }

                    // ensure that there aren't any other extra routes for this page in the RouteTable
                    foreach (var routeTableRoute in RouteTable.Routes.OfType<Route>().Where(a => a.PageId() == page.Id))
                    {
                        if ( !editorRoutes.Any( a => a == routeTableRoute.Url ) )
                        {
                            RouteTable.Routes.Remove( routeTableRoute );
                        }
                    }

                    // Add any routes that were added
                    foreach ( var pageRoute in new PageRouteService( rockContext ).GetByPageId( page.Id ) )
                    {
                        if ( addedRoutes.Contains( pageRoute.Route ) )
                        {
                            RouteTable.Routes.AddPageRoute( pageRoute );
                        }
                    }

                    if ( orphanedIconFileId.HasValue )
                    {
                        BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                        var binaryFile = binaryFileService.Get( orphanedIconFileId.Value );
                        if ( binaryFile != null )
                        {
                            // marked the old images as IsTemporary so they will get cleaned up later
                            binaryFile.IsTemporary = true;
                            rockContext.SaveChanges();
                        }
                    }

                    Rock.Web.Cache.PageCache.Flush( page.Id );

                    string script = "if (typeof window.parent.Rock.controls.modal.close === 'function') window.parent.Rock.controls.modal.close('PAGE_UPDATED');";
                    ScriptManager.RegisterStartupScript( this.Page, this.GetType(), "close-modal", script, true );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbExport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbExport_Click( object sender, EventArgs e )
        {
            if ( _pageId.HasValue )
            {
                var pageService = new PageService( new RockContext() );
                var page = pageService.Get( _pageId.Value );
                var packageService = new PackageService();
                var pageName = page.InternalName.Replace( " ", "_" ) + ( cbExportChildren.Checked ? "_wChildPages" : string.Empty );
                using ( var stream = packageService.ExportPage( page, cbExportChildren.Checked ) )
                {
                    EnableViewState = false;
                    Response.Clear();
                    Response.ContentType = "application/octet-stream";
                    Response.AddHeader( "content-disposition", "attachment; filename=" + pageName + ".nupkg" );
                    Response.Charset = string.Empty;
                    Response.BinaryWrite( stream.ToArray() );
                    Response.Flush();
                    Response.End();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbImport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbImport_Click( object sender, EventArgs e )
        {
            var page = PageCache.Read( _pageId ?? 0 );
            if ( page != null )
            {
                var extension = fuImport.FileName.Substring( fuImport.FileName.LastIndexOf( '.' ) );

                if ( fuImport.PostedFile == null && extension != ".nupkg" )
                {
                    var errors = new List<string> { "Please attach an export file when trying to import a package." };
                    rptImportErrors.DataSource = errors;
                    rptImportErrors.DataBind();
                    rptImportErrors.Visible = true;
                    pnlImportSuccess.Visible = false;
                    return;
                }

                var packageService = new PackageService();
                bool importResult;

                importResult = packageService.ImportPage( fuImport.FileBytes, fuImport.FileName, page.Id, page.Layout.SiteId );

                if ( !importResult )
                {
                    rptImportErrors.DataSource = packageService.ErrorMessages;
                    rptImportErrors.DataBind();
                    rptImportErrors.Visible = true;
                    pnlImportSuccess.Visible = false;
                }
                else
                {
                    pnlImportSuccess.Visible = true;
                    rptImportWarnings.Visible = false;
                    rptImportErrors.Visible = false;

                    if ( packageService.WarningMessages.Count > 0 )
                    {
                        rptImportErrors.DataSource = packageService.WarningMessages;
                        rptImportErrors.DataBind();
                        rptImportWarnings.Visible = true;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ServerValidate event of the cvPageRoute control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="ServerValidateEventArgs"/> instance containing the event data.</param>
        protected void cvPageRoute_ServerValidate( object source, ServerValidateEventArgs args )
        {
            var errorMessages = new List<string>();

            foreach ( string route in tbPageRoute.Text.SplitDelimitedValues() )
            {
                var pageRoute = new PageRoute();
                pageRoute.Route = route.TrimStart( new char[] { '/' } );
                pageRoute.Guid = Guid.NewGuid();
                if ( !pageRoute.IsValid )
                {
                    errorMessages.Add( string.Format( 
                        "The '{0}' route is invalid: {1}",
                        route,
                        pageRoute.ValidationResults.Select( r => r.ErrorMessage ).ToList().AsDelimited( "; " ) ) );
                }
            }

            cvPageRoute.ErrorMessage = errorMessages.AsDelimited( "<br/>" );

            args.IsValid = !errorMessages.Any();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the sites.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void LoadSites( RockContext rockContext )
        {
            ddlSite.Items.Clear();
            foreach ( Site site in new SiteService( rockContext ).Queryable().OrderBy( s => s.Name ) )
            {
                ddlSite.Items.Add( new ListItem( site.Name, site.Id.ToString() ) );
            }
        }

        /// <summary>
        /// Loads the layouts.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="site">The site.</param>
        private void LoadLayouts( RockContext rockContext, SiteCache site )
        {
            LayoutService.RegisterLayouts( Request.MapPath( "~" ), site );

            ddlLayout.Items.Clear();
            var layoutService = new LayoutService( rockContext );
            foreach ( var layout in layoutService.GetBySiteId( site.Id ) )
            {
                ddlLayout.Items.Add( new ListItem( layout.Name, layout.Id.ToString() ) );
            }
        }

        /// <summary>
        /// Displays the error.
        /// </summary>
        /// <param name="message">The message.</param>
        private void DisplayError( string message )
        {
            pnlError.Controls.Clear();
            pnlError.Controls.Add( new LiteralControl( message ) );
            pnlError.Visible = true;

            phContent.Visible = false;
        }

        /// <summary>
        /// Gets the tab class.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        protected string GetTabClass( object property )
        {
            if ( property.ToString() == CurrentTab )
            {
                return "active";
            }

            return string.Empty;
        }

        /// <summary>
        /// Shows the selected pane.
        /// </summary>
        private void ShowSelectedPane()
        {
            if ( CurrentTab.Equals( "Basic Settings" ) )
            {
                pnlBasicProperty.Visible = true;
                pnlDisplaySettings.Visible = false;
                pnlAdvancedSettings.Visible = false;
                pnlImportExport.Visible = false;
            }
            else if ( CurrentTab.Equals( "Display Settings" ) )
            {
                pnlBasicProperty.Visible = false;
                pnlDisplaySettings.Visible = true;
                pnlAdvancedSettings.Visible = false;
                pnlImportExport.Visible = false;
            }
            else if ( CurrentTab.Equals( "Advanced Settings" ) )
            {
                pnlBasicProperty.Visible = false;
                pnlDisplaySettings.Visible = false;
                pnlAdvancedSettings.Visible = true;
                pnlImportExport.Visible = false;
            }
            else if ( CurrentTab.Equals( "Import/Export" ) )
            {
                pnlBasicProperty.Visible = false;
                pnlDisplaySettings.Visible = false;
                pnlAdvancedSettings.Visible = false;
                pnlImportExport.Visible = true;
            }
        }

        #endregion
    }
}