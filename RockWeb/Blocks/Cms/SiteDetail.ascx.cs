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
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;
using Site = Rock.Model.Site;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Site Detail" )]
    [Category( "CMS" )]
    [Description( "Displays the details of a specific site." )]

    [BinaryFileTypeField( "Default File Type",
        Key = AttributeKey.DefaultFileType,
        Description = "The default file type to use while uploading Favicon",
        IsRequired = true,
        DefaultValue = Rock.SystemGuid.BinaryFiletype.DEFAULT, // this was previously defaultBinaryFileTypeGuid which maps to base default value
        Category = "",
        Order = 0 )]

    public partial class SiteDetail : RockBlock, IDetailBlock
    {
        #region Attribute Keys
        protected static class AttributeKey
        {
            public const string DefaultFileType = "DefaultFileType";
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the state of the page attributes.
        /// </summary>
        /// <value>
        /// The state of the page attributes.
        /// </value>
        private List<Attribute> PageAttributesState { get; set; }

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

            gPageAttributes.DataKeyNames = new string[] { "Guid" };
            gPageAttributes.Actions.ShowAdd = true;
            gPageAttributes.Actions.AddClick += gPageAttributes_Add;
            gPageAttributes.EmptyDataText = Server.HtmlEncode( None.Text );
            gPageAttributes.GridRebind += gPageAttributes_GridRebind;
            gPageAttributes.GridReorder += gPageAttributes_GridReorder;
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

            if ( dlgPageAttribute.Visible )
            {
                HideSecondaryBlocks( true );
            }
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var json = ViewState["PageAttributesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                PageAttributesState = new List<Attribute>();
            }
            else
            {
                PageAttributesState = JsonConvert.DeserializeObject<List<Attribute>>( json );
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["PageAttributesState"] = JsonConvert.SerializeObject( PageAttributesState, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        #endregion

        #region PageAttributes Grid and Picker

        /// <summary>
        /// Handles the Add event of the gPageAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gPageAttributes_Add( object sender, EventArgs e )
        {
            gPageAttributes_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gPageAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gPageAttributes_Edit( object sender, RowEventArgs e )
        {
            Guid attributeGuid = ( Guid ) e.RowKeyValue;
            gPageAttributes_ShowEdit( attributeGuid );
        }

        /// <summary>
        /// gs the page attributes show edit.
        /// </summary>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        protected void gPageAttributes_ShowEdit( Guid attributeGuid )
        {
            Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                attribute.FieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT ).Id;
                edtPageAttributes.ActionTitle = ActionTitle.Add( "attribute for pages of site " + tbSiteName.Text );
            }
            else
            {
                attribute = PageAttributesState.First( a => a.Guid.Equals( attributeGuid ) );
                edtPageAttributes.ActionTitle = ActionTitle.Edit( "attribute for pages of site " + tbSiteName.Text );
            }

            var reservedKeyNames = new List<string>();
            PageAttributesState.Where( a => !a.Guid.Equals( attributeGuid ) ).Select( a => a.Key ).ToList().ForEach( a => reservedKeyNames.Add( a ) );
            edtPageAttributes.ReservedKeyNames = reservedKeyNames.ToList();

            edtPageAttributes.SetAttributeProperties( attribute, typeof( Rock.Model.Page ) );

            dlgPageAttribute.Show();
            HideSecondaryBlocks( true );
        }

        /// <summary>
        /// Handles the GridReorder event of the gPageAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gPageAttributes_GridReorder( object sender, GridReorderEventArgs e )
        {
            new AttributeService( new RockContext() ).Reorder( PageAttributesState, e.OldIndex, e.NewIndex );
            BindPageAttributesGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gPageAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gPageAttributes_Delete( object sender, RowEventArgs e )
        {
            Guid attributeGuid = ( Guid ) e.RowKeyValue;
            PageAttributesState.RemoveEntity( attributeGuid );

            BindPageAttributesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gPageAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gPageAttributes_GridRebind( object sender, EventArgs e )
        {
            BindPageAttributesGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgPageAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgPageAttribute_SaveClick( object sender, EventArgs e )
        {
            Rock.Model.Attribute attribute = new Rock.Model.Attribute();
            edtPageAttributes.GetAttributeProperties( attribute );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            if ( PageAttributesState.Any( a => a.Guid.Equals( attribute.Guid ) ) )
            {
                attribute.Order = PageAttributesState.Where( a => a.Guid.Equals( attribute.Guid ) ).FirstOrDefault().Order;
                PageAttributesState.RemoveEntity( attribute.Guid );
            }
            else
            {
                attribute.Order = PageAttributesState.Any() ? PageAttributesState.Max( a => a.Order ) + 1 : 0;
            }

            PageAttributesState.Add( attribute );

            BindPageAttributesGrid();

            dlgPageAttribute.Hide();
        }

        /// <summary>
        /// Binds the page attributes grid.
        /// </summary>
        private void BindPageAttributesGrid()
        {
            gPageAttributes.AddCssClass( "attribute-grid" );
            int order = 0;
            PageAttributesState.OrderBy( a => a.Order ).ToList().ForEach( a => a.Order = order++ );

            gPageAttributes.DataSource = PageAttributesState.OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            gPageAttributes.DataBind();
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
            var site = new SiteService( new RockContext() ).Get( hfSiteId.Value.AsInteger() );
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
        /// Handles the Click event of the btnCompileTheme control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCompileTheme_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            SiteService siteService = new SiteService( rockContext );
            Site site = siteService.Get( hfSiteId.Value.AsInteger() );

            string messages = string.Empty;
            var theme = new RockTheme( site.Theme );
            bool success = theme.Compile( out messages );

            if ( success )
            {
                mdThemeCompile.Show( "Theme was successfully compiled.", ModalAlertType.Information );
            }
            else
            {
                mdThemeCompile.Show( string.Format( "An error occurred compiling the theme {0}. Message: {1}.", site.Theme, messages ), ModalAlertType.Warning );
            }
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
            Site site = siteService.Get( hfSiteId.Value.AsInteger() );
            LayoutService layoutService = new LayoutService( rockContext );
            PageService pageService = new PageService( rockContext );

            if ( site != null )
            {
                var sitePages = new List<int> {
                    site.DefaultPageId ?? -1,
                    site.LoginPageId ?? -1,
                    site.RegistrationPageId ?? -1,
                    site.PageNotFoundPageId ?? -1
                };

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
                PageService pageService = new PageService( rockContext );
                SiteService siteService = new SiteService( rockContext );
                SiteDomainService siteDomainService = new SiteDomainService( rockContext );
                bool newSite = false;

                int siteId = hfSiteId.Value.AsInteger();

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
                site.ChangePasswordPageId = ppChangePasswordPage.PageId;
                site.ChangePasswordPageRouteId = ppChangePasswordPage.PageRouteId;
                site.CommunicationPageId = ppCommunicationPage.PageId;
                site.CommunicationPageRouteId = ppCommunicationPage.PageRouteId;
                site.RegistrationPageId = ppRegistrationPage.PageId;
                site.RegistrationPageRouteId = ppRegistrationPage.PageRouteId;
                site.PageNotFoundPageId = ppPageNotFoundPage.PageId;
                site.PageNotFoundPageRouteId = ppPageNotFoundPage.PageRouteId;
                site.ErrorPage = tbErrorPage.Text;
                site.GoogleAnalyticsCode = tbGoogleAnalytics.Text;
                site.RequiresEncryption = cbRequireEncryption.Checked;
                site.EnabledForShortening = cbEnableForShortening.Checked;
                site.EnableMobileRedirect = cbEnableMobileRedirect.Checked;
                site.MobilePageId = ppMobilePage.PageId;
                site.ExternalUrl = tbExternalURL.Text;
                site.AllowedFrameDomains = tbAllowedFrameDomains.Text;
                site.RedirectTablets = cbRedirectTablets.Checked;
                site.EnablePageViews = cbEnablePageViews.Checked;
                site.IsActive = cbIsActive.Checked;
                site.AllowIndexing = cbAllowIndexing.Checked;
                site.IsIndexEnabled = cbEnableIndexing.Checked;
                site.IndexStartingLocation = tbIndexStartingLocation.Text;

                site.PageHeaderContent = cePageHeaderContent.Text;

                int? existingIconId = null;
                if ( site.FavIconBinaryFileId != imgSiteIcon.BinaryFileId )
                {
                    existingIconId = site.FavIconBinaryFileId;
                    site.FavIconBinaryFileId = imgSiteIcon.BinaryFileId;
                }

                int? existingLogoId = null;
                if ( site.SiteLogoBinaryFileId != imgSiteLogo.BinaryFileId )
                {
                    existingLogoId = site.SiteLogoBinaryFileId;
                    site.SiteLogoBinaryFileId = imgSiteLogo.BinaryFileId;
                }

                var currentDomains = tbSiteDomains.Text.SplitDelimitedValues().ToList<string>();
                site.SiteDomains = site.SiteDomains ?? new List<SiteDomain>();

                // Remove any deleted domains
                foreach ( var domain in site.SiteDomains.Where( w => !currentDomains.Contains( w.Domain ) ).ToList() )
                {
                    site.SiteDomains.Remove( domain );
                    siteDomainService.Delete( domain );
                }

                int order = 0;
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
                    sd.Order = order++;
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

                    SaveAttributes( new Page().TypeId, "SiteId", site.Id.ToString(), PageAttributesState, rockContext );

                    if ( existingIconId.HasValue )
                    {
                        BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                        var binaryFile = binaryFileService.Get( existingIconId.Value );
                        if ( binaryFile != null )
                        {
                            // marked the old images as IsTemporary so they will get cleaned up later
                            binaryFile.IsTemporary = true;
                            rockContext.SaveChanges();
                        }
                    }

                    if ( existingLogoId.HasValue )
                    {
                        BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                        var binaryFile = binaryFileService.Get( existingLogoId.Value );
                        if ( binaryFile != null )
                        {
                            // marked the old images as IsTemporary so they will get cleaned up later
                            binaryFile.IsTemporary = true;
                            rockContext.SaveChanges();
                        }
                    }

                    if ( newSite )
                    {
                        Rock.Security.Authorization.CopyAuthorization( RockPage.Layout.Site, site, rockContext, Authorization.EDIT );
                        Rock.Security.Authorization.CopyAuthorization( RockPage.Layout.Site, site, rockContext, Authorization.ADMINISTRATE );
                        Rock.Security.Authorization.CopyAuthorization( RockPage.Layout.Site, site, rockContext, Authorization.APPROVE );
                    }
                } );

                // add/update for the InteractionChannel for this site and set the RetentionPeriod
                var interactionChannelService = new InteractionChannelService( rockContext );
                int channelMediumWebsiteValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE.AsGuid() ).Id;
                var interactionChannelForSite = interactionChannelService.Queryable()
                    .Where( a => a.ChannelTypeMediumValueId == channelMediumWebsiteValueId && a.ChannelEntityId == site.Id ).FirstOrDefault();

                if ( interactionChannelForSite == null )
                {
                    interactionChannelForSite = new InteractionChannel();
                    interactionChannelForSite.ChannelTypeMediumValueId = channelMediumWebsiteValueId;
                    interactionChannelForSite.ChannelEntityId = site.Id;
                    interactionChannelService.Add( interactionChannelForSite );
                }

                interactionChannelForSite.Name = site.Name;
                interactionChannelForSite.RetentionDuration = nbPageViewRetentionPeriodDays.Text.AsIntegerOrNull();
                interactionChannelForSite.ComponentEntityTypeId = EntityTypeCache.Get<Rock.Model.Page>().Id;

                rockContext.SaveChanges();


                // Create the default page is this is a new site
                if ( !site.DefaultPageId.HasValue && newSite )
                {
                    var siteCache = SiteCache.Get( site.Id );

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
                        var page = new Page();
                        page.LayoutId = layout.Id;
                        page.PageTitle = siteCache.Name + " Home Page";
                        page.InternalName = page.PageTitle;
                        page.BrowserTitle = page.PageTitle;
                        page.EnableViewState = true;
                        page.IncludeAdminFooter = true;
                        page.MenuDisplayChildPages = true;

                        var lastPage = pageService.GetByParentPageId( null ).OrderByDescending( b => b.Order ).FirstOrDefault();

                        page.Order = lastPage != null ? lastPage.Order + 1 : 0;
                        pageService.Add( page );

                        rockContext.SaveChanges();

                        site = siteService.Get( siteCache.Id );
                        site.DefaultPageId = page.Id;

                        rockContext.SaveChanges();
                    }
                }

                var qryParams = new Dictionary<string, string>();
                qryParams["siteId"] = site.Id.ToString();

                NavigateToPage( RockPage.Guid, qryParams );
            }
        }

        /// <summary>
        /// Saves the attributes.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="qualifierColumn">The qualifier column.</param>
        /// <param name="qualifierValue">The qualifier value.</param>
        /// <param name="viewStateAttributes">The view state attributes.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SaveAttributes( int entityTypeId, string qualifierColumn, string qualifierValue, List<Attribute> viewStateAttributes, RockContext rockContext )
        {
            // Get the existing attributes for this entity type and qualifier value
            var attributeService = new AttributeService( rockContext );
            var attributes = attributeService.GetByEntityTypeQualifier( entityTypeId, qualifierColumn, qualifierValue, true );

            // Delete any of those attributes that were removed in the UI
            var selectedAttributeGuids = viewStateAttributes.Select( a => a.Guid );
            foreach ( var attr in attributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ) )
            {
                attributeService.Delete( attr );
                rockContext.SaveChanges();
            }

            // Update the Attributes that were assigned in the UI
            foreach ( var attributeState in viewStateAttributes )
            {
                Helper.SaveAttributeEdits( attributeState, entityTypeId, qualifierColumn, qualifierValue, rockContext );
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
                var site = new SiteService( new RockContext() ).Get( hfSiteId.Value.AsInteger() );
                ShowReadonlyDetails( site );
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbEnableIndexing control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbEnableIndexing_CheckedChanged( object sender, EventArgs e )
        {
            SetControlsVisiblity();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbEnableMobileRedirect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbEnableMobileRedirect_CheckedChanged( object sender, EventArgs e )
        {
            SetControlsVisiblity();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbEnablePageViews control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbEnablePageViews_CheckedChanged( object sender, EventArgs e )
        {
            SetControlsVisiblity();
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
                pdAuditDetails.SetEntity( site, ResolveRockUrl( "~" ) );
            }

            if ( site == null )
            {
                site = new Site { Id = 0 };
                site.SiteDomains = new List<SiteDomain>();
                site.Theme = RockPage.Layout.Site.Theme;
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }
            else
            {
                if ( site.DefaultPageId.HasValue )
                {
                    lVisitSite.Text = string.Format( @"<a href=""{0}{1}"" target=""_blank""><span class=""label label-info"">Visit Site</span></a>", ResolveRockUrl( "~/page/" ), site.DefaultPageId );
                }
            }

            Guid fileTypeGuid = GetAttributeValue( AttributeKey.DefaultFileType ).AsGuid();
            imgSiteIcon.BinaryFileTypeGuid = fileTypeGuid;

            // set theme compile button
            if ( !new RockTheme( site.Theme ).AllowsCompile )
            {
                btnCompileTheme.Enabled = false;
                btnCompileTheme.Text = "Theme Doesn't Support Compiling";
            }

            pnlDetails.Visible = true;
            hfSiteId.Value = site.Id.ToString();

            cePageHeaderContent.Text = site.PageHeaderContent;
            cbAllowIndexing.Checked = site.AllowIndexing;

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
        /// <param name="site">The site.</param>
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

            imgSiteIcon.BinaryFileId = site.FavIconBinaryFileId;
            imgSiteLogo.BinaryFileId = site.SiteLogoBinaryFileId;

            cbIsActive.Checked = site.IsActive;

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

            if ( site.ChangePasswordPageRoute != null )
            {
                ppChangePasswordPage.SetValue( site.ChangePasswordPageRoute );
            }
            else
            {
                ppChangePasswordPage.SetValue( site.ChangePasswordPage );
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

            tbSiteDomains.Text = string.Join( "\n", site.SiteDomains.OrderBy( d => d.Order ).Select( d => d.Domain ).ToArray() );
            tbGoogleAnalytics.Text = site.GoogleAnalyticsCode;
            cbRequireEncryption.Checked = site.RequiresEncryption;
            cbEnableForShortening.Checked = site.EnabledForShortening;

            cbEnableMobileRedirect.Checked = site.EnableMobileRedirect;
            ppMobilePage.SetValue( site.MobilePage );
            tbExternalURL.Text = site.ExternalUrl;
            tbAllowedFrameDomains.Text = site.AllowedFrameDomains;
            cbRedirectTablets.Checked = site.RedirectTablets;
            cbEnablePageViews.Checked = site.EnablePageViews;

            int channelMediumWebsiteValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE.AsGuid() ).Id;
            var interactionChannelForSite = new InteractionChannelService( new RockContext() ).Queryable()
                .Where( a => a.ChannelTypeMediumValueId == channelMediumWebsiteValueId && a.ChannelEntityId == site.Id ).FirstOrDefault();

            if ( interactionChannelForSite != null )
            {
                nbPageViewRetentionPeriodDays.Text = interactionChannelForSite.RetentionDuration.ToString();
            }

            cbEnableIndexing.Checked = site.IsIndexEnabled;
            tbIndexStartingLocation.Text = site.IndexStartingLocation;

            // disable the indexing features if indexing on site is disabled
            var siteEntityType = EntityTypeCache.Get( "Rock.Model.Site" );
            if ( siteEntityType != null && !siteEntityType.IsIndexingEnabled )
            {
                cbEnableIndexing.Visible = false;
                tbIndexStartingLocation.Visible = false;
            }

            var attributeService = new AttributeService( new RockContext() );
            var siteIdQualifierValue = site.Id.ToString();
            PageAttributesState = attributeService.GetByEntityTypeId( new Page().TypeId, true ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "SiteId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( siteIdQualifierValue ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
            BindPageAttributesGrid();

            SetControlsVisiblity();
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="site">The site.</param>
        private void ShowReadonlyDetails( Rock.Model.Site site )
        {
            SetEditMode( false );

            hfSiteId.SetValue( site.Id );
            lReadOnlyTitle.Text = site.Name.FormatAsHtmlTitle();

            lSiteDescription.Text = site.Description;

            DescriptionList descriptionList = new DescriptionList();
            descriptionList.Add( "Domain(s)", site.SiteDomains.OrderBy( d => d.Order ).Select( d => d.Domain ).ToList().AsDelimited( ", " ) );
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

        /// <summary>
        /// Sets the controls visibility.
        /// </summary>
        private void SetControlsVisiblity()
        {
            bool mobileRedirectVisible = cbEnableMobileRedirect.Checked;
            ppMobilePage.Visible = mobileRedirectVisible;
            tbExternalURL.Visible = mobileRedirectVisible;
            cbRedirectTablets.Visible = mobileRedirectVisible;

            nbPageViewRetentionPeriodDays.Visible = cbEnablePageViews.Checked;

            tbIndexStartingLocation.Visible = cbEnableIndexing.Checked;
        }

        #endregion
    }
}