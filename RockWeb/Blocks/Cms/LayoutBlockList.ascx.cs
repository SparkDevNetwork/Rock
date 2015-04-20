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
    [DisplayName("Layout Block List")]
    [Category("CMS")]
    [Description("Lists blocks that are on a given site layout.")]
    public partial class LayoutBlockList : RockBlock, ISecondaryBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( RockPage.Layout.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
            {
                gLayoutBlocks.DataKeyNames = new string[] { "Id" };
                gLayoutBlocks.Actions.ShowAdd = false;
                gLayoutBlocks.GridRebind += gLayoutBlocks_GridRebind;
                //SecurityField securityField = gLayoutBlocks.Columns[4] as SecurityField;
                //securityField.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Block ) ).Id;
            }
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
                BindLayoutBlocksGrid();
            }
        }

        #endregion

        #region LayoutBlocks Grid

        /// <summary>
        /// Handles the Delete click event for the grid.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs" /> instance containing the event data.</param>
        protected void DeleteBlock_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            BlockService blockService = new BlockService( rockContext );
            Block block = blockService.Get( e.RowKeyId );
            if ( block != null )
            {
                string errorMessage;
                if ( !blockService.CanDelete( block, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                blockService.Delete( block );
                rockContext.SaveChanges();

                BlockCache.Flush( e.RowKeyId );
            }

            BindLayoutBlocksGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gLayoutBlocks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gLayoutBlocks_GridRebind( object sender, EventArgs e )
        {
            BindLayoutBlocksGrid();
        }

        /// <summary>
        /// Binds the group members grid.
        /// </summary>
        protected void BindLayoutBlocksGrid()
        {
            pnlBlocks.Visible = false;

            int layoutId = PageParameter( "layoutId" ).AsInteger();
            if ( layoutId == 0 )
            {
                pnlContent.Visible = false;
                return;
            }

            var rockContext = new RockContext();
            var layout = LayoutCache.Read( layoutId, rockContext );
            if (layout == null)
            {
                pnlContent.Visible = false;
                return;
            }
                
            hfLayoutId.SetValue( layoutId );

            pnlBlocks.Visible = true;

            BlockService blockService = new BlockService( new RockContext() );

            gLayoutBlocks.DataSource = blockService.GetByLayout( layoutId ).ToList();
            gLayoutBlocks.DataBind();
        }

        /// <summary>
        /// Creates the block config icon.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <returns></returns>
        protected string CreateConfigIcon( string blockId )
        {
            if ( ! string.IsNullOrWhiteSpace( blockId ) )
            {
                var blockPropertyUrl = ResolveUrl( string.Format( "~/BlockProperties/{0}?t=Block Properties", blockId ) );

                return string.Format( "<a class=\"btn btn-default btn-sm\" href=\"javascript: Rock.controls.modal.show($(this), '{0}')\" title=\"Block Properties\"><i class=\"fa fa-cog\"></i></a>",
                    blockPropertyUrl );
            }
            else
            {
                return "";
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
    }
}