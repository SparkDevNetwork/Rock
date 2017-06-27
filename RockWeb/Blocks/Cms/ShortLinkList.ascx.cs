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
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System.ComponentModel;
using Rock.Security;
using System.Web.UI;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Short Link List" )]
    [Category( "CMS" )]
    [Description( "Lists all the short Links ." )]
    [LinkedPage( "Detail Page" )]
    public partial class ShortLinkList : RockBlock, ISecondaryBlock
    {
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

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gShortLinks.Actions.ShowAdd = canAddEditDelete;
            gShortLinks.IsDeleteEnabled = canAddEditDelete;

            RockPage.AddScriptLink( this.Page, "~/Scripts/clipboard.js/clipboard.min.js" );
            string script = @"
    new Clipboard('.js-copy-clipboard');
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
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                BindGrid();
            }
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
            SiteUrlMapService siteUrlMapService = new SiteUrlMapService( rockContext );

            var shortLink = siteUrlMapService.Get( e.RowKeyId );
            if ( shortLink != null )
            {
                string errorMessage;
                canDelete = siteUrlMapService.CanDelete( shortLink, out errorMessage );
                if ( !canDelete )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Alert );
                    return;
                }

                int siteId = shortLink.SiteId;

                siteUrlMapService.Delete( shortLink );
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
            NavigateToLinkedPage( "DetailPage", "ShortLinkId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gShortLinks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gShortLinks_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "ShortLinkId", e.RowKeyId );
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
        /// Binds the group members grid.
        /// </summary>
        protected void BindGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                var siteUrlMapService = new SiteUrlMapService( rockContext );

                var qry = siteUrlMapService.Queryable().ToList()
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
        
        protected class ShortLinkRow
        {
            public int Id { get; set; }
            public int SiteId { get; set; }
            public string SiteName { get; set; }
            public string Token { get; set; }
            public string Url { get; set; }
            public string ShortLink { get; set; }

            public ShortLinkRow ( SiteUrlMap siteUrlMap )
            {
                Id = siteUrlMap.Id;
                SiteId = siteUrlMap.Site.Id;
                SiteName = siteUrlMap.Site.Name;
                Token = siteUrlMap.Token;
                Url = siteUrlMap.Url;

                var uri = siteUrlMap.Site.DefaultDomainUri;
                var url = uri != null ? uri.ToString() : Rock.Web.Cache.GlobalAttributesCache.Read().GetValue( "PublicApplicationRoot" );
                ShortLink = url.EnsureTrailingForwardslash() + Token;
            }
        }
    }
}