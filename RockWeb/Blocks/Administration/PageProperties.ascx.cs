// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Services.NuGet;
using Rock.Web;
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

    [BooleanField( "Enable Full Edit Mode", "Have the block initially show a readonly summary view, in a panel, with Edit and Delete buttons. Also include Save and Cancel buttons.", false )]
    public partial class PageProperties : RockBlock
    {
        #region Fields

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
            int? pageId = PageParameter( "Page" ).AsIntegerOrNull();

            btnSecurity.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Page ) ).Id;

            // only show if there was a Page parameter specified
            this.Visible = pageId.HasValue;

            if ( pageId.HasValue )
            {
                // hide the current page in the page picker to prevent setting this page's parent page to itself (or one of it's child pages)
                ppParentPage.HiddenPageIds = new int[] { pageId.Value };

                var pageCache = Rock.Web.Cache.PageCache.Read( pageId.Value );

                DialogPage dialogPage = this.Page as DialogPage;
                if ( dialogPage != null )
                {
                    dialogPage.OnSave += new EventHandler<EventArgs>( masterPage_OnSave );
                    dialogPage.SubTitle = string.Format( "Id: {0}", pageId );
                }

                ddlMenuWhen.BindToEnum<DisplayInNavWhen>();

                if ( pageCache != null && pageCache.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                {
                    var blockContexts = new Dictionary<string, string>();
                    foreach ( var block in pageCache.Blocks )
                    {

                        try
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
                        catch
                        {
                            // if the blocktype can't compile, just ignore it since we are just trying to find out if it had a blockContext
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
                }
            }
            else
            {
                // no 'Page' parameter specified so just leave hidden
            }

            base.OnInit( e );
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="page">The page.</param>
        private void ShowReadonlyDetails( Rock.Model.Page page )
        {
            SetEditMode( false );

            string pageIconHtml = !string.IsNullOrWhiteSpace( page.IconCssClass ) ?
                pageIconHtml = string.Format( "<i class='{0} fa-2x' ></i>", page.IconCssClass ) : string.Empty;

            lTitle.Text = page.InternalName.FormatAsHtmlTitle();
            if ( !string.IsNullOrEmpty( page.IconCssClass ) )
            {
                lIcon.Text = string.Format( "<i class='{0}'></i>", page.IconCssClass );
            }
            else
            {
                lIcon.Text = "<i class='fa fa-file-text-o'></i>";
            }

            var site = SiteCache.Read( page.Layout.SiteId );
            hlblSiteName.Text = "Site: " + site.Name;

            lblMainDetailsCol1.Text = new DescriptionList()
                .Add( "Internal Name", page.InternalName )
                .Add( "Page Title", page.PageTitle )
                .Add( "Browser Title", page.BrowserTitle )
                .Add( "Description", page.Description )
                .Html;

            var pageReference = new PageReference( page.Id );
            var pageUrl = pageReference.BuildUrl();
            var pageLink = string.Format( "<a href='{0}'>{0}</a>", pageUrl );

            lblMainDetailsCol2.Text = new DescriptionList()
                .Add( "Layout", page.Layout )
                .Add( "Url", pageLink )
                .Html;
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewDetails.Visible = !editable;
            mdCopyPage.Visible = !editable;
            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                string pageIdParam = PageParameter( "Page" );
                if ( !string.IsNullOrEmpty( pageIdParam ) )
                {
                    ShowDetail( pageIdParam.AsInteger(), PageParameter( "ParentPageId" ).AsIntegerOrNull() );
                }
                else
                {
                    this.Visible = false;
                }
            }
            else
            {
                if ( pnlEditDetails.Visible )
                {
                    // Load the Attribute Controls
                    var pageService = new PageService( new RockContext() );
                    var page = pageService.Get( hfPageId.Value.AsInteger() );
                    if ( page == null )
                    {
                        page = new Rock.Model.Page();
                        page.LayoutId = ddlLayout.SelectedValue.AsInteger();
                    }

                    page.LoadAttributes();
                    phPageAttributes.Controls.Clear();

                    Rock.Attribute.Helper.AddEditControls( page, phPageAttributes, false, BlockValidationGroup );
                }
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
        /// Shows the detail.
        /// </summary>
        /// <param name="pageId">The page identifier.</param>
        /// <param name="parentPageId">The parent page identifier.</param>
        public void ShowDetail( int pageId, int? parentPageId )
        {
            var rockContext = new RockContext();

            LoadSites( rockContext );

            PageService pageService = new PageService( rockContext );
            Rock.Model.Page page = pageService.Queryable( "Layout,PageRoutes" )
                .Where( p => p.Id == pageId )
                .FirstOrDefault();

            if ( page == null )
            {
                page = new Rock.Model.Page { Id = 0, IsSystem = false, ParentPageId = parentPageId };

                // fetch the ParentCategory (if there is one) so that security can check it
                page.ParentPage = pageService.Get( parentPageId ?? 0 );
            }

            hfPageId.Value = page.Id.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;

            // if the person is Authorized to EDIT the page, or has EDIT for the block
            var canEdit = page.IsAuthorized( Authorization.EDIT, CurrentPerson ) || this.IsUserAuthorized( Authorization.EDIT );
            if ( !canEdit )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Rock.Model.Page.FriendlyTypeName );
            }

            btnSecurity.Visible = page.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
            btnSecurity.Title = page.InternalName;
            btnSecurity.EntityId = page.Id;
            
            aChildPages.HRef = string.Format( "javascript: Rock.controls.modal.show($(this), '/pages/{0}?t=Child Pages&amp;pb=&amp;sb=Done')", page.Id );

            // this will be true when used in the Page Builder page, and false when used in the System Dialog
            var enableFullEditMode = this.GetAttributeValue( "EnableFullEditMode" ).AsBooleanOrNull() ?? false;

            pnlEditModeActions.Visible = enableFullEditMode;
            pnlReadOnlyModeActions.Visible = enableFullEditMode;
            pnlHeading.Visible = enableFullEditMode;
            pnlDetails.CssClass = enableFullEditMode ? "panel panel-block" : string.Empty;
            pnlBody.CssClass = enableFullEditMode ? "panel-body" : string.Empty;

            if ( readOnly )
            {
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                ShowReadonlyDetails( page );
            }
            else
            {
                btnEdit.Visible = true;
                string errorMessage = string.Empty;
                btnDelete.Visible = true;
                btnDelete.Enabled = pageService.CanDelete( page, out errorMessage );
                btnDelete.ToolTip = btnDelete.Enabled ? string.Empty : errorMessage;

                if ( page.Id > 0 && enableFullEditMode )
                {
                    ShowReadonlyDetails( page );
                }
                else
                {
                    ShowEditDetails( page );
                }
            }

            if ( btnDelete.Visible && btnDelete.Enabled )
            {
                btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", Rock.Model.Page.FriendlyTypeName.ToLower() );
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="page">The page.</param>
        private void ShowEditDetails( Rock.Model.Page page )
        {
            if ( page.Id > 0 )
            {
                lTitle.Text = ActionTitle.Edit( Rock.Model.Page.FriendlyTypeName ).FormatAsHtmlTitle();
                lIcon.Text = "<i class='fa fa-square-o'></i>";
            }
            else
            {
                lTitle.Text = ActionTitle.Add( Rock.Model.Page.FriendlyTypeName ).FormatAsHtmlTitle();

                if ( !string.IsNullOrEmpty( page.IconCssClass ) )
                {
                    lIcon.Text = string.Format( "<i class='{0}'></i>", page.IconCssClass );
                }
                else
                {
                    lIcon.Text = "<i class='fa fa-file-text-o'></i>";
                }
            }

            SetEditMode( true );

            var rockContext = new RockContext();
            PageService pageService = new PageService( rockContext );

            if ( page.Layout != null )
            {
                ddlSite.SetValue( page.Layout.SiteId );
            }
            else if ( page.ParentPageId.HasValue )
            {
                var parentPageCache = PageCache.Read( page.ParentPageId.Value );
                if ( parentPageCache != null && parentPageCache.Layout != null )
                {
                    ddlSite.SetValue( parentPageCache.Layout.SiteId );
                }
            }

            LoadLayouts( rockContext, SiteCache.Read( ddlSite.SelectedValue.AsInteger() ) );
            if ( page.LayoutId == 0 )
            {
                // default a new page's layout to whatever the parent page's layout is
                if ( page.ParentPage != null )
                {
                    page.LayoutId = page.ParentPage.LayoutId;
                }
            }

            ddlLayout.SetValue( page.LayoutId );

            phPageAttributes.Controls.Clear();
            page.LoadAttributes();

            if ( page.Attributes != null && page.Attributes.Any() )
            {
                wpPageAttributes.Visible = true;
                Rock.Attribute.Helper.AddEditControls( page, phPageAttributes, true, BlockValidationGroup );
            }
            else
            {
                wpPageAttributes.Visible = false;
            }

            rptProperties.DataSource = _tabs;
            rptProperties.DataBind();

            tbPageName.Text = page.InternalName;
            tbPageTitle.Text = page.PageTitle;
            tbBrowserTitle.Text = page.BrowserTitle;
            tbBodyCssClass.Text = page.BodyCssClass;
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
            cbAllowIndexing.Checked = page.AllowIndexing;
            tbCacheDuration.Text = page.OutputCacheDuration.ToString();
            tbDescription.Text = page.Description;
            ceHeaderContent.Text = page.HeaderContent;
            tbPageRoute.Text = string.Join( ",", page.PageRoutes.Select( route => route.Route ).ToArray() );

            // Add enctype attribute to page's <form> tag to allow file upload control to function
            Page.Form.Attributes.Add( "enctype", "multipart/form-data" );
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
            if ( Page.IsValid )
            {
                var rockContext = new RockContext();
                var pageService = new PageService( rockContext );
                var routeService = new PageRouteService( rockContext );
                var contextService = new PageContextService( rockContext );

                int pageId = hfPageId.Value.AsInteger();

                var page = pageService.Get( pageId );
                if ( page == null )
                {
                    page = new Rock.Model.Page();
                    pageService.Add( page );
                }

                // validate/check for removed routes
                var editorRoutes = tbPageRoute.Text.SplitDelimitedValues().Distinct();
                var databasePageRoutes = page.PageRoutes.ToList();
                var deletedRouteIds = new List<int>();
                var addedRoutes = new List<string>();

                if ( editorRoutes.Any() )
                {
                    int? siteId = null;
                    if ( page != null && page.Layout != null )
                    {
                        siteId = page.Layout.SiteId;
                    }

                    // validate for any duplicate routes
                    var duplicateRouteQry = routeService.Queryable()
                        .Where( r =>
                            r.PageId != pageId &&
                            editorRoutes.Contains( r.Route ) );
                    if ( siteId.HasValue )
                    {
                        duplicateRouteQry = duplicateRouteQry
                            .Where( r =>
                                r.Page != null &&
                                r.Page.Layout != null &&
                                r.Page.Layout.SiteId == siteId.Value );
                    }

                    var duplicateRoutes = duplicateRouteQry
                        .Select( r => r.Route )
                        .Distinct()
                        .ToList();

                    if ( duplicateRoutes.Any() )
                    {
                        // Duplicate routes
                        nbPageRouteWarning.Title = "Duplicate Route(s)";
                        nbPageRouteWarning.Text = string.Format( "<p>The page route <strong>{0}</strong>, already exists for another page in the same site. Please choose a different route name.</p>", duplicateRoutes.AsDelimited( "</strong> and <strong>" ) );
                        nbPageRouteWarning.Dismissable = true;
                        nbPageRouteWarning.Visible = true;
                        CurrentTab = "Advanced Settings";

                        rptProperties.DataSource = _tabs;
                        rptProperties.DataBind();
                        ShowSelectedPane();
                        return;
                    }
                }

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
                page.BodyCssClass = tbBodyCssClass.Text;

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
                page.AllowIndexing = cbAllowIndexing.Checked;
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

                // Page Attributes
                page.LoadAttributes();

                Rock.Attribute.Helper.GetEditValues( phPageAttributes, page );

                // save page and it's routes
                if ( page.IsValid )
                {
                    // use WrapTransaction since SaveAttributeValues does its own RockContext.SaveChanges()
                    rockContext.WrapTransaction( () =>
                    {
                        rockContext.SaveChanges();

                        page.SaveAttributeValues( rockContext );
                    } );

                    // remove any routes for this page that are no longer configured
                    foreach ( var existingRoute in RouteTable.Routes.OfType<Route>().Where( a => a.PageIds().Contains( page.Id ) ) )
                    {
                        if ( !editorRoutes.Any( a => a == existingRoute.Url ) )
                        {
                            var pageAndRouteIds = existingRoute.DataTokens["PageRoutes"] as List<Rock.Web.PageAndRouteId>;
                            pageAndRouteIds = pageAndRouteIds.Where( p => p.PageId != page.Id ).ToList();
                            if ( pageAndRouteIds.Any() )
                            {
                                existingRoute.DataTokens["PageRoutes"] = pageAndRouteIds;
                            }
                            else
                            {
                                RouteTable.Routes.Remove( existingRoute );
                            }
                        }
                    }

                    // Remove the '{shortlink}' route (will be added back after specific routes)
                    var shortLinkRoute = RouteTable.Routes.OfType<Route>().Where( r => r.Url == "{shortlink}" ).FirstOrDefault();
                    if ( shortLinkRoute != null )
                    {
                        RouteTable.Routes.Remove( shortLinkRoute );
                    }

                    // Add any routes that were added
                    foreach ( var pageRoute in new PageRouteService( rockContext ).GetByPageId( page.Id ) )
                    {
                        if ( addedRoutes.Contains( pageRoute.Route ) )
                        {
                            var pageAndRouteId = new Rock.Web.PageAndRouteId { PageId = pageRoute.PageId, RouteId = pageRoute.Id };

                            var existingRoute = RouteTable.Routes.OfType<Route>().FirstOrDefault( r => r.Url == pageRoute.Route );
                            if ( existingRoute != null )
                            {
                                var pageAndRouteIds = existingRoute.DataTokens["PageRoutes"] as List<Rock.Web.PageAndRouteId>;
                                pageAndRouteIds.Add( pageAndRouteId );
                                existingRoute.DataTokens["PageRoutes"] = pageAndRouteIds;
                            }
                            else
                            {
                                var pageAndRouteIds = new List<Rock.Web.PageAndRouteId>();
                                pageAndRouteIds.Add( pageAndRouteId );
                                RouteTable.Routes.AddPageRoute( pageRoute.Route, pageAndRouteIds );
                            }
                        }
                    }

                    RouteTable.Routes.Add( new Route( "{shortlink}", new Rock.Web.RockRouteHandler() ) );

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

                    hfPageId.Value = page.Id.ToString();
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
            int? pageId = hfPageId.Value.AsIntegerOrNull();
            if ( pageId.HasValue )
            {
                var pageService = new PageService( new RockContext() );
                var page = pageService.Get( pageId.Value );
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
            int? pageId = hfPageId.Value.AsIntegerOrNull();
            var page = PageCache.Read( pageId ?? 0 );
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
            foreach ( SiteCache site in new SiteService( rockContext ).Queryable().OrderBy( s => s.Name ).Select( a => a.Id ).ToList().Select( a => SiteCache.Read( a ) ) )
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

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            PageService service = new PageService( new RockContext() );
            Rock.Model.Page page = service.Get( hfPageId.ValueAsInt() );
            ShowEditDetails( page );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var pageService = new PageService( rockContext );
            var siteService = new SiteService( rockContext );

            int pageId = hfPageId.Value.AsInteger();
            var page = pageService.Get( pageId );
            if ( page != null )
            {
                string errorMessage = string.Empty;
                if ( !pageService.CanDelete( page, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Alert );
                    return;
                }

                foreach ( var site in siteService.Queryable() )
                {
                    if ( site.DefaultPageId == page.Id )
                    {
                        site.DefaultPageId = null;
                        site.DefaultPageRouteId = null;
                    }

                    if ( site.LoginPageId == page.Id )
                    {
                        site.LoginPageId = null;
                        site.LoginPageRouteId = null;
                    }

                    if ( site.RegistrationPageId == page.Id )
                    {
                        site.RegistrationPageId = null;
                        site.RegistrationPageRouteId = null;
                    }
                }

                int? parentPageId = page.ParentPageId;

                pageService.Delete( page );

                rockContext.SaveChanges();

                Rock.Web.Cache.PageCache.Flush( page.Id );

                // reload page, selecting the deleted page's parent
                var qryParams = new Dictionary<string, string>();
                if ( parentPageId.HasValue )
                {
                    qryParams["Page"] = parentPageId.ToString();

                    string expandedIds = this.Request.Params["ExpandedIds"];
                    if ( expandedIds != null )
                    {
                        // remove the current pageId param to avoid extra treeview flash
                        var expandedIdList = expandedIds.SplitDelimitedValues().AsIntegerList();
                        expandedIdList.Remove( parentPageId.Value );

                        qryParams["ExpandedIds"] = expandedIdList.AsDelimited( "," );
                    }
                }

                NavigateToPage( RockPage.Guid, qryParams );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            masterPage_OnSave( sender, e );

            // reload page using the current page
            var pageId = hfPageId.Value.AsIntegerOrNull();
            var qryParams = new Dictionary<string, string>();
            if ( pageId.HasValue )
            {
                qryParams["Page"] = pageId.ToString();

                string expandedIds = this.Request.Params["ExpandedIds"];
                if ( expandedIds != null )
                {
                    // remove the current pageId param to avoid extra treeview flash
                    var expandedIdList = expandedIds.SplitDelimitedValues().AsIntegerList();
                    expandedIdList.Remove( pageId.Value );

                    // add the parentPageId to the expanded ids
                    var parentPageParam = this.Request.Params["ParentPageId"];
                    if ( !string.IsNullOrEmpty( parentPageParam ) )
                    {
                        var parentPageId = parentPageParam.AsIntegerOrNull();
                        if ( parentPageId.HasValue )
                        {
                            if ( !expandedIdList.Contains( parentPageId.Value ) )
                            {
                                expandedIdList.Add( parentPageId.Value );
                            }
                        }
                    }

                    qryParams["ExpandedIds"] = expandedIdList.AsDelimited( "," );
                }
            }

            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfPageId.Value.Equals( "0" ) )
            {
                int? parentPageId = PageParameter( "ParentPageId" ).AsIntegerOrNull();
                if ( parentPageId.HasValue )
                {
                    // Cancelling on Add, and we know the parentPageId, so we are probably in treeview mode, so navigate to the current page
                    var qryParams = new Dictionary<string, string>();
                    qryParams["Page"] = parentPageId.ToString();

                    string expandedIds = this.Request.Params["ExpandedIds"];
                    if ( expandedIds != null )
                    {
                        // remove the current pageId param to avoid extra treeview flash
                        var expandedIdList = expandedIds.SplitDelimitedValues().AsIntegerList();
                        expandedIdList.Remove( parentPageId.Value );
                        qryParams["ExpandedIds"] = expandedIdList.AsDelimited( "," );
                    }

                    NavigateToPage( RockPage.Guid, qryParams );
                }
                else
                {
                    // Cancelling on Add.  Return to Grid
                    NavigateToParentPage();
                }
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                PageService service = new PageService( new RockContext() );
                Rock.Model.Page page = service.Get( hfPageId.ValueAsInt() );
                ShowReadonlyDetails( page );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCopy control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCopy_Click( object sender, EventArgs e )
        {
            mdCopyPage.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdCopyPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdCopyPage_SaveClick( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            PageService pageService = new PageService( rockContext );
            int sourcePageId = hfPageId.ValueAsInt();

            Guid? copiedPageGuid = pageService.CopyPage( sourcePageId, cbCopyPageIncludeChildPages.Checked, this.CurrentPersonAliasId );
            if ( copiedPageGuid.HasValue )
            {
                var copiedPage = PageCache.Read( copiedPageGuid.Value );

                // reload page (Assuming we are using Page Builder UI) to the new copied page
                var qryParams = new Dictionary<string, string>();
                if ( copiedPage != null )
                {
                    qryParams["Page"] = copiedPage.Id.ToString();

                    string expandedIds = this.Request.Params["ExpandedIds"];
                    if ( expandedIds != null )
                    {
                        // remove the current pageId param to avoid extra treeview flash
                        var expandedIdList = expandedIds.SplitDelimitedValues().AsIntegerList();
                        expandedIdList.Remove( copiedPage.Id );

                        // add the parentPageId to the expanded ids
                        var parentPageParam = this.Request.Params["ParentPageId"];
                        if ( !string.IsNullOrEmpty( parentPageParam ) )
                        {
                            var parentPageId = parentPageParam.AsIntegerOrNull();
                            if ( parentPageId.HasValue )
                            {
                                if ( !expandedIdList.Contains( parentPageId.Value ) )
                                {
                                    expandedIdList.Add( parentPageId.Value );
                                }
                            }
                        }

                        qryParams["ExpandedIds"] = expandedIdList.AsDelimited( "," );
                    }
                }

                NavigateToPage( RockPage.Guid, qryParams );
            }
        }
    }
}