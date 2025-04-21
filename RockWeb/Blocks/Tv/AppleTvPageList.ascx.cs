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
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Tv
{
    /*
    
    10/15/2021 - JME
    The code from this block was taken primarily from the MobileApplicationDetail.ascx block.
    
    */
    [DisplayName( "Apple TV Page List" )]
    [Category( "TV > TV Apps" )]
    [Description( "Lists pages for TV apps (Apple or other)." )]

    #region Block Attributes
    [LinkedPage( "Page Detail", "", true, "", "", 0, AttributeKey.PageDetail )]
    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "7BD1B79C-BF27-42C6-8359-F80EC7FEE397" )]
    public partial class AppleTvPageList : Rock.Web.UI.RockBlock, ISecondaryBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string PageDetail = "PageDetail";
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string SiteId = "SiteId";
            public const string SitePageId = "SitePageId";
        }

        #endregion PageParameterKeys

        #region Base Control Methods

        // Overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // This event gets fired after block settings are updated. It's nice to repaint the screen if these settings would alter it.
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            // Block Security and special attributes (RockPage takes care of View)
            bool canEdit = IsUserAuthorized( Authorization.EDIT );
            gPages.Actions.ShowAdd = canEdit;
            
            // Wire-up Security Buttons
            var securityField = gPages.ColumnsOfType<SecurityField>().FirstOrDefault();
            if ( securityField != null )
            {
                securityField.Visible = canEdit;
                securityField.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Page ) ).Id;
            }

            // Show/Hide Delete Buttons
            var deleteField = gPages.ColumnsOfType<DeleteField>().FirstOrDefault();
            if ( deleteField != null )
            {
                deleteField.Visible = canEdit;
            }

            gPages.Actions.ShowMergeTemplate = false;
            gPages.Actions.ShowExcelExport = false;
            gPages.Actions.AddClick += gPages_AddClick;
            gPages.Actions.ShowAdd = true;
            gPages.DataKeyNames = new[] { "Id" };
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindPages();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        // Handlers called by the controls on your block.

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindPages();
        }

        /// <summary>
        /// Handles the AddClick event of the gPages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gPages_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.PageDetail, new Dictionary<string, string>
            {
                { PageParameterKey.SiteId, PageParameter( PageParameterKey.SiteId ) },
                { PageParameterKey.SitePageId, "0" }
            } );
        }

        /// <summary>
        /// Handles the RowSelected event of the gPages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gPages_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.PageDetail, new Dictionary<string, string>
            {
                { PageParameterKey.SiteId, PageParameter( PageParameterKey.SiteId ) },
                { PageParameterKey.SitePageId, e.RowKeyId.ToString() }
            } );
        }

        /// <summary>
        /// Handles the GridRebind event of the gPages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gPages_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindPages();
        }

        /// <summary>
        /// Handles the GridReorder event of the gPages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gPages_GridReorder( object sender, Rock.Web.UI.Controls.GridReorderEventArgs e )
        {
            var applicationId = PageParameter( PageParameterKey.SiteId ).AsInteger();
            using ( var rockContext = new RockContext() )
            {
                PageService pageService = new PageService( rockContext );
                var pages = pageService.GetBySiteId( applicationId )
                    .OrderBy( p => p.Order )
                    .ThenBy( p => p.InternalName )
                    .ToList();
                pageService.Reorder( pages, e.OldIndex, e.NewIndex );
                rockContext.SaveChanges();
            }

            BindPages( );
        }

        /// <summary>
        /// Handles the RowDataBound event of the gPages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gPages_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {

            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            // Add cache information
            var pageInfo = ( PageRowInfo ) e.Row.DataItem;
            var lCacheSettings = e.Row.FindControl( "lCacheSettings" ) as Literal;

            if ( pageInfo.CacheControlHeaderSettings != null )
            {
                var cacheControlHeader = pageInfo.CacheControlHeaderSettings.FromJsonOrNull<RockCacheability>() ?? new RockCacheability();

                var cacheDescription = new StringBuilder();
                cacheDescription.Append( $"{cacheControlHeader.RockCacheablityType} " );

                if ( cacheControlHeader.MaxAge != null || cacheControlHeader.SharedMaxAge != null )
                {
                    if ( cacheControlHeader.MaxAge != null )
                    {
                        cacheDescription.Append( $"<span class=\"label label-default\">Max Age: {cacheControlHeader.MaxAge.ToSeconds() / 60}m</span> " );
                    }

                    if ( cacheControlHeader.SharedMaxAge != null )
                    {
                        cacheDescription.Append( $" <span class=\"label label-default\">Max Shared Age: {cacheControlHeader.SharedMaxAge.ToSeconds() / 60}m</span>" );
                    }
                }
                lCacheSettings.Text = cacheDescription.ToString();
            }

            // Hide the delete button if this page is the application's default page
            var applicationId = PageParameter( PageParameterKey.SiteId ).AsInteger();
            if ( applicationId == 0 )
            {
                return;
            }

            int? defaultPageId = SiteCache.Get( applicationId ).DefaultPageId;
            if ( defaultPageId == null )
            {
                return;
            }

            var deleteField = gPages.ColumnsOfType<DeleteField>().FirstOrDefault();
            if ( deleteField == null || !deleteField.Visible )
            {
                return;
            }

            int? pageId = gPages.DataKeys[e.Row.RowIndex].Values[0].ToString().AsIntegerOrNull();
            if ( pageId == defaultPageId )
            {
                var deleteFieldColumnIndex = gPages.GetColumnIndex( deleteField );
                var deleteButton = e.Row.Cells[deleteFieldColumnIndex].ControlsOfTypeRecursive<LinkButton>().FirstOrDefault();
                if ( deleteButton != null )
                {
                    deleteButton.Visible = false;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the gPages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gPages_DeleteClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            var pageService = new PageService( rockContext );
            var page = pageService.Get( e.RowKeyId );

            if ( page != null )
            {
                string errorMessage;
                if ( !pageService.CanDelete( page, out errorMessage ) )
                {
                    mdWarning.Show( errorMessage, ModalAlertType.Warning );
                    return;
                }

                pageService.Delete( page );

                rockContext.SaveChanges();
            }

            BindPages();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the pages.
        /// </summary>
        private void BindPages()
        {
            var applicationId = PageParameter( PageParameterKey.SiteId ).AsInteger();

            var pages = new PageService( new RockContext() )
                    .GetBySiteId( applicationId )
                    .OrderBy( p => p.Order )
                    .ThenBy( p => p.InternalName )
                    .Select( p => new PageRowInfo
                    {
                        Id = p.Id,
                        InternalName = p.InternalName,
                        DisplayInNav = p.DisplayInNavWhen != DisplayInNavWhen.Never,
                        Description = p.Description,
                        CacheControlHeaderSettings = p.CacheControlHeaderSettings
                    } )
                    .ToList();

            gPages.EntityTypeId = EntityTypeCache.Get<Rock.Model.Site>().Id;
            gPages.DataSource = pages;
            gPages.DataBind();
        }

        public void SetVisible( bool visible )
        {
            pnlBlock.Visible = visible;
        }


        /// <summary>
        /// POCO for showing data on the grid
        /// </summary>
        public class PageRowInfo
        {
            public int Id { get; set; }
            public string InternalName { get; set; }
            public bool DisplayInNav { get; set; }
            public string Description { get; set; }
            public string CacheControlHeaderSettings { get; set; }
        }
        #endregion
    }
}