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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// Lists all the content library entities.
    /// </summary>
    [DisplayName( "Content Library List" )]
    [Category( "CMS" )]
    [Description( "Lists all the content library entities." )]

    #region Block Attributes

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage )]

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "F305FE35-2EFA-4653-AA1A-87AE990EAFEB" )]
    public partial class ContentLibraryList : RockBlock, ISecondaryBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        /// <summary>
        /// The keys used in page parameters.
        /// </summary>
        private class PageParameterKey
        {
            /// <summary>
            /// The content library identifier key.
            /// </summary>
            public const string ContentLibraryId = "ContentLibraryId";
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gfFilter.ApplyFilterClick += gfFilter_ApplyFilterClick;
            //gfFilter.DisplayFilterValue += gfFilter_DisplayFilterValue;

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );

            gContentLibrary.DataKeyNames = new string[] { "Id" };
            gContentLibrary.Actions.ShowAdd = canAddEditDelete;
            gContentLibrary.IsDeleteEnabled = canAddEditDelete;
            gContentLibrary.Actions.AddClick += gContentLibrary_Add;
            gContentLibrary.GridRebind += gContentLibrary_GridRebind;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void gfFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the gContentLibrary control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gContentLibrary_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.ContentLibraryId, 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gContentLibrary control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gContentLibrary_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.ContentLibraryId, e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gContentLibrary control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gContentLibrary_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var contentLibraryService = new ContentLibraryService( rockContext );

            var contentLibrary = contentLibraryService.Get( e.RowKeyId );

            if ( contentLibrary != null )
            {
                string errorMessage;
                if ( !contentLibraryService.CanDelete( contentLibrary, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Warning );
                    return;
                }

                contentLibraryService.Delete( contentLibrary );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gContentLibrary control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gContentLibrary_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on it's page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var contentLibraryService = new ContentLibraryService( new RockContext() );
            SortProperty sortProperty = gContentLibrary.SortProperty;
            var qry = contentLibraryService.Queryable().Include( a => a.ContentLibrarySources );

            var items = qry.Select( c => new
            {
                c.Id,
                c.Name,
                c.Description,
                c.LastIndexItemCount,
                Sources = c.ContentLibrarySources.Count(),
            } ).AsQueryable();

            gContentLibrary.EntityTypeId = EntityTypeCache.Get<ContentLibrary>().Id;

            if ( sortProperty != null )
            {
                gContentLibrary.DataSource = items.Sort( sortProperty ).ToList();
            }
            else
            {
                gContentLibrary.DataSource = items.OrderBy( p => p.Name ).ToList();
            }

            gContentLibrary.DataBind();
        }


        #endregion
    }
}