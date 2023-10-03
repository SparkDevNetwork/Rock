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
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Mobile;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Site List" )]
    [Category( "CMS" )]
    [Description( "Lists sites defined in the system." )]

    #region Block Attributes
    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Order = 0 )]

    [EnumsField(
        "Site Type",
        "Includes Items with the following Type.",
        typeof( SiteType ),
        false, "",
        order: 1,
        key: AttributeKey.SiteType )]

    [TextField(
        "Block Title",
        Key = AttributeKey.BlockTitle,
        Description = "The title for the block.",
        IsRequired = false,
        DefaultValue = "Site List",
        Order = 2 )]

    [TextField(
        "Block Icon CSS Class",
        Key = AttributeKey.BlockIconCssClass,
        Description = "The icon CSS class for the block.",
        IsRequired = false,
        DefaultValue = "fa fa-desktop",
        Order = 3)]

    [TextField(
        "Block Icon CSS Class",
        Key = AttributeKey.BlockIconCssClass,
        Description = "The icon CSS class for the block.",
        IsRequired = false,
        DefaultValue = "fa fa-desktop",
        Order = 3 )]

    [BooleanField( "Show Delete Column",
        Description = "Determines if the delete column should be shown.",
        DefaultBooleanValue = false,
        IsRequired = true,
        Key = AttributeKey.ShowDeleteColumn,
        Order = 5 )]
    #endregion
    [Rock.SystemGuid.BlockTypeGuid( "441D5A71-C250-4FF5-90C3-DEEAD3AC028D" )]
    public partial class SiteList : RockBlock, ICustomGridColumns
    {
        #region Attribute Keys
        private static class AttributeKey
        {
            public const string SiteType = "SiteType";
            public const string BlockTitle = "BlockTitle";
            public const string BlockIconCssClass = "BlockIcon";
            public const string ShowDeleteColumn = "ShowDeleteColumn";
            public const string DetailPage = "DetailPage";
        }
        #endregion
        private const string INCLUE_INACTIVE = "Include Inactive";


        #region Control Methods
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gSites.DataKeyNames = new string[] { "Id" };
            gSites.Actions.AddClick += gSites_Add;
            gSites.GridRebind += gSites_GridRebind;
            gSites.ShowConfirmDeleteDialog = false;
            gSites.IsDeleteEnabled = true;

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEdit = IsUserAuthorized( Authorization.EDIT );
            gSites.Actions.ShowAdd = canAddEdit;

            var securityField = gSites.ColumnsOfType<SecurityField>().FirstOrDefault();
            if ( securityField != null )
            {
                securityField.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Site ) ).Id;
            }

            var deleteField = new DeleteField();
            gSites.Columns.Add( deleteField );
            deleteField.Click += gSites_Delete;

            var deleteColumn = gSites.Columns.OfType<DeleteField>().FirstOrDefault();
            if ( deleteColumn != null )
            {
                deleteColumn.Visible = GetAttributeValue( AttributeKey.ShowDeleteColumn ).AsBoolean();
            }

            string deleteScript = @"
                $('table.js-grid-site-list a.grid-delete-button').on('click', function( e ){
                    var $btn = $(this);
                    e.preventDefault();
                    var siteName = $btn.closest('tr').find('.js-name').text();
                    Rock.dialogs.confirm('Deleting a site will delete all layouts and pages related to it. Are you sure you want to delete the <strong>' + siteName + '</strong> site?', function (result) {
                        if (result) {
                                Rock.dialogs.confirm('Due to the large nature of the delete please confirm again your intent to delete the <strong>' + siteName + '</strong> site.', function (result) {
                                    if (result) {
                                        window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                                    }
                                });
                        }
                    });
                });";

            ScriptManager.RegisterStartupScript( gSites, gSites.GetType(), "deleteSiteScript", deleteScript, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                lBlockIcon.Text = string.Format( @"<i class=""fa {0}""></i>", GetAttributeValue( AttributeKey.BlockIconCssClass ) );
                lBlockTitle.Text = GetAttributeValue( AttributeKey.BlockTitle );

                BindGrid();
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
            NavigateToLinkedPage( AttributeKey.DetailPage, "SiteId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gSites control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSites_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, "SiteId", e.RowKeyId );
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

        /// <summary>
        /// Handles the Delete event of the gSites control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSites_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            SiteService siteService = new SiteService( rockContext );
            Site site = siteService.Get( e.RowKeyId );
            LayoutService layoutService = new LayoutService( rockContext );
            PageService pageService = new PageService( rockContext );
            UserLoginService userLoginService = new UserLoginService( rockContext );
            if ( site != null )
            {
                var additionalSettings = site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>() ?? new AdditionalSiteSettings();

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
                var canDelete = siteService.CanDelete( site, out errorMessage, includeSecondLvl: true );
                if ( !canDelete )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Alert );
                    return;
                }

                UserLogin userLogin = null;
                if ( additionalSettings.ApiKeyId.HasValue )
                {
                    userLogin = userLoginService.Get( additionalSettings.ApiKeyId.Value );
                }

                rockContext.WrapTransaction( () =>
                {
                    siteService.Delete( site );
                    if ( userLogin != null )
                    {
                        userLoginService.Delete( userLogin );
                    }
                    rockContext.SaveChanges();

                } );

            }

            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            SiteService siteService = new SiteService( new RockContext() );
            SortProperty sortProperty = gSites.SortProperty;
            var qry = siteService.Queryable();

            var siteType = GetAttributeValue( AttributeKey.SiteType ).SplitDelimitedValues().Select( a => a.ConvertToEnumOrNull<SiteType>() ).ToList();

            if ( !siteType.Contains( SiteType.Web ) )
            {
                gSites.ColumnsOfType<RockBoundField>().First( c => c.DataField == "Theme" ).Visible = false;
                gSites.ColumnsOfType<RockTemplateField>().First( c => c.ID == "colDomains" ).Visible = false;
            }

            // Default show inactive to false if no filter (user preference) applied.
            bool showInactiveSites = rFilterSite.GetFilterPreference( INCLUE_INACTIVE ).AsBoolean();

            if ( siteType.Count() > 0 )
            {
                // Filter by block setting Site type
                qry = qry.Where( s => siteType.Contains( s.SiteType ) );
            }
            // Filter by selected filter
            if ( !showInactiveSites )
            {
                qry = qry.Where( s => s.IsActive == true );
            }

            if ( sortProperty != null )
            {
                gSites.DataSource = qry.Sort( sortProperty ).ToList();
            }
            else
            {
                gSites.DataSource = qry.OrderBy( s => s.Name ).ToList();
            }

            gSites.EntityTypeId = EntityTypeCache.Get<Site>().Id;
            gSites.DataBind();
        }

        /// <summary>
        /// Gets the domains for the site.
        /// </summary>
        /// <param name="siteID">The site identifier.</param>
        /// <returns></returns>
        protected string GetDomains( int siteID )
        {
            return new SiteDomainService( new RockContext() ).Queryable()
                .Where( d => d.SiteId == siteID )
                .OrderBy( d => d.Domain )
                .Select( d => d.Domain )
                .ToList()
                .AsDelimited( ", " );
        }

        #endregion

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilterSite control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rFilterSite_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilterSite.SetFilterPreference( INCLUE_INACTIVE, cbShowInactive.Checked.ToString() );
            BindGrid();
        }
    }
}