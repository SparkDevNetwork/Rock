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
using System.IO;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using System.ComponentModel;
using Rock.Security;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName("Layout List")]
    [Category("CMS")]
    [Description("Lists layouts for a site.")]

    #region Block Attributes

    [LinkedPage(
       "Detail Page",
        Key = AttributeKey.DetailPage )]

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "5996BF81-F2E2-4702-B401-B0B1B6667DAE" )]
    public partial class LayoutList : RockBlock, ISecondaryBlock, ICustomGridColumns
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string SiteId = "SiteId";
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

            gLayouts.DataKeyNames = new string[] { "Id" };
            gLayouts.Actions.AddClick += gLayouts_AddClick;
            gLayouts.GridRebind += gLayouts_GridRebind;

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gLayouts.Actions.ShowAdd = canAddEditDelete;
            gLayouts.IsDeleteEnabled = canAddEditDelete;

            var securityField = gLayouts.ColumnsOfType<SecurityField>().FirstOrDefault();
            if ( securityField != null )
            {
                securityField.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Layout ) ).Id;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindLayoutsGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Layouts Grid

        /// <summary>
        /// Handles the Click event of the DeleteLayout control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs" /> instance containing the event data.</param>
        protected void DeleteLayout_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            bool canDelete = false;

            var rockContext = new RockContext();
            LayoutService layoutService = new LayoutService(rockContext);

            Layout layout = layoutService.Get( e.RowKeyId );
            if ( layout != null )
            {
                string errorMessage;
                canDelete = layoutService.CanDelete( layout, out errorMessage );
                if ( !canDelete )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Alert );
                    return; 
                }

                int siteId = layout.SiteId;

                layoutService.Delete( layout );
                rockContext.SaveChanges();
            }

            BindLayoutsGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the gLayouts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gLayouts_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, "LayoutId", 0, "SiteId", hfSiteId.ValueAsInt() );
        }

        /// <summary>
        /// Handles the Edit event of the gLayouts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gLayouts_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, "LayoutId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the GridRebind event of the gLayouts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gLayouts_GridRebind( object sender, EventArgs e )
        {
            BindLayoutsGrid();
        }

        /// <summary>
        /// Binds the group members grid.
        /// </summary>
        protected void BindLayoutsGrid()
        {
            pnlLayouts.Visible = false;

            int siteId = PageParameter( PageParameterKey.SiteId ).AsInteger();
            if ( siteId == 0 )
            {
                // quit if the siteId can't be determined
                return;
            }

            var rockContext = new RockContext();
            var site = SiteCache.Get( siteId, rockContext );
            if ( site == null )
            {
                return;
            }

            hfSiteId.SetValue( siteId );

            pnlLayouts.Visible = true;

            // Add any missing layouts
            LayoutService.RegisterLayouts( Request.MapPath( "~" ), site );

            LayoutService layoutService = new LayoutService( new RockContext() );
            var qry = layoutService.Queryable().Where( a => a.SiteId.Equals( siteId ) );

            SortProperty sortProperty = gLayouts.SortProperty;

            if ( sortProperty != null )
            {
                gLayouts.DataSource = qry.Sort( sortProperty ).ToList();
            }
            else
            {
                gLayouts.DataSource = qry.OrderBy( l => l.Name ).ToList();
            }

            gLayouts.DataBind();
        }

        protected string GetFilePath( string fileName )
        {
            string virtualPath = fileName;

            var siteCache = SiteCache.Get( hfSiteId.ValueAsInt() );
            if ( siteCache != null )
            {
                virtualPath = string.Format( "~/Themes/{0}/Layouts/{1}.aspx", siteCache.Theme, fileName );

                if ( !File.Exists( Request.MapPath( virtualPath ) ) )
                {
                    virtualPath = virtualPath += " <span class='label label-danger'>Missing</span>";
                }
            }

            return virtualPath;
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
    }
}