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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
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

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Short Link List" )]
    [Category( "CMS" )]
    [Description( "Lists all the short Links ." )]

    #region Block Attributes

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Order = 0 )]

    #endregion
    [Rock.SystemGuid.BlockTypeGuid( "D6D87CCC-DB6D-4138-A4B5-30F0707A5300" )]
    public partial class ShortLinkList : RockBlock, ISecondaryBlock, ICustomGridColumns
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Attribute Keys

        #region Filter Attribute Keys

        private class FilterAttributeKeys
        {
            public const string Token = "Token";
            public const string Site = "Site";
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gShortLinks.DataKeyNames = new string[] { "Id" };
            gShortLinks.Actions.AddClick += gShortLinks_AddClick;
            gShortLinks.GridRebind += gShortLinks_GridRebind;

            gfShortLink.ApplyFilterClick += gfShortLink_ApplyFilterClick;
            gfShortLink.DisplayFilterValue += gfShortLink_DisplayFilterValue;
            gfShortLink.ClearFilterClick += gfShortLink_ClearFilterClick;

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gShortLinks.Actions.ShowAdd = canAddEditDelete;
            gShortLinks.IsDeleteEnabled = canAddEditDelete;

            RockPage.AddScriptLink( this.Page, "~/Scripts/clipboard.js/clipboard.min.js" );
            string script = @"
    new ClipboardJS('.js-copy-clipboard');
    $('.js-copy-clipboard').tooltip();
";
            ScriptManager.RegisterStartupScript( gShortLinks, gShortLinks.GetType(), "copy-short-link", script, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Filter

        /// <summary>
        /// Displays the text of the gfShortLink control
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gfShortLink_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Site":
                    int? siteId = e.Value.AsIntegerOrNull();
                    if ( siteId.HasValue )
                    {
                        var site = SiteCache.Get( siteId.Value );
                        if ( site != null )
                        {
                            e.Value = site.Name;
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfShortLink control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfShortLink_ApplyFilterClick( object sender, EventArgs e )
        {
            gfShortLink.SetFilterPreference( FilterAttributeKeys.Site, ddlSite.SelectedValue );
            gfShortLink.SetFilterPreference( FilterAttributeKeys.Token, txtToken.Text );

            BindGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfShortLink control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfShortLink_ClearFilterClick( object sender, EventArgs e )
        {
            gfShortLink.DeleteFilterPreferences();
            BindFilter();
        }

        #endregion

        #region ShortLinks Grid

        /// <summary>
        /// Handles the Click event of the DeleteShortLink control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs" /> instance containing the event data.</param>
        protected void DeleteShortLink_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            bool canDelete = false;

            var rockContext = new RockContext();
            PageShortLinkService pageShortLinkService = new PageShortLinkService( rockContext );

            var shortLink = pageShortLinkService.Get( e.RowKeyId );
            if ( shortLink != null )
            {
                string errorMessage;
                canDelete = pageShortLinkService.CanDelete( shortLink, out errorMessage );
                if ( !canDelete )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Alert );
                    return;
                }

                int siteId = shortLink.SiteId;

                pageShortLinkService.Delete( shortLink );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the gShortLinks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gShortLinks_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, "ShortLinkId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gShortLinks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gShortLinks_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, "ShortLinkId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the GridRebind event of the gShortLinks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gShortLinks_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            txtToken.Text = gfShortLink.GetFilterPreference( FilterAttributeKeys.Token );

            ddlSite.Items.Clear();

            foreach ( SiteCache site in new SiteService( new RockContext() ).Queryable().AsNoTracking().OrderBy( s => s.Name ).Select( a => a.Id ).ToList().Select( a => SiteCache.Get( a ) ) )
            {
                ddlSite.Items.Add( new ListItem( site.Name, site.Id.ToString() ) );
            }
            ddlSite.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );
            ddlSite.SetValue( gfShortLink.GetFilterPreference( FilterAttributeKeys.Site ) );
        }

        /// <summary>
        /// Binds the group members grid.
        /// </summary>
        protected void BindGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                var pageShortLinkService = new PageShortLinkService( rockContext );

                var shortLinkQry = pageShortLinkService.Queryable();

                string token = gfShortLink.GetFilterPreference( FilterAttributeKeys.Token );
                if ( !string.IsNullOrEmpty( token ) )
                {
                    shortLinkQry = shortLinkQry.Where( s => s.Token.Contains( token ) );
                }

                int? siteId = gfShortLink.GetFilterPreference( FilterAttributeKeys.Site ).AsIntegerOrNull();
                if ( siteId.HasValue )
                {
                    shortLinkQry = shortLinkQry.Where( s => s.SiteId == siteId.Value );
                }


                var qry = shortLinkQry.ToList()
                    .Select( s => new ShortLinkRow( s ) )
                    .ToList()
                    .AsQueryable();

                SortProperty sortProperty = gShortLinks.SortProperty;
                if ( sortProperty != null )
                {
                    gShortLinks.DataSource = qry.Sort( sortProperty ).ToList();
                }
                else
                {
                    gShortLinks.DataSource = qry.OrderBy( l => l.Token ).ToList();
                }

                gShortLinks.DataBind();
            }

        }

        #endregion

        #region ISecondaryBlock

        /// <summary>
        /// Sets the visible.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        #endregion

        protected class ShortLinkRow : RockDynamic
        {
            public int Id { get; set; }
            public int SiteId { get; set; }
            public string SiteName { get; set; }
            public string Token { get; set; }
            public string Url { get; set; }
            public string ShortLink { get; set; }

            public ShortLinkRow ( PageShortLink pageShortLink )
            {
                Id = pageShortLink.Id;
                SiteId = pageShortLink.Site.Id;
                SiteName = pageShortLink.Site.Name;
                Token = pageShortLink.Token;
                Url = pageShortLink.Url;

                var url = pageShortLink.Site.DefaultDomainUri.ToString();
                ShortLink = url.EnsureTrailingForwardslash() + Token;
            }
        }
    }
}