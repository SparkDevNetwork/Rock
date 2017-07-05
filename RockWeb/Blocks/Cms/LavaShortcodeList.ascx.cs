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
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System.ComponentModel;
using Rock.Security;
using Rock.Web.Cache;
using System.Collections.Generic;
using DotLiquid;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName("Lava Shortcode List")]
    [Category("CMS")]
    [Description( "Lists Lava Shortcode in the system." )]

    [LinkedPage("Detail Page")]
    public partial class LavaShortcodeList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );

            gLavaShortcode.DataKeyNames = new string[] { "Id" };
            gLavaShortcode.Actions.AddClick += gLavaShortcode_Add;
            gLavaShortcode.GridRebind += gLavaShortcode_GridRebind;
            gLavaShortcode.Actions.ShowAdd = canAddEditDelete;
            gLavaShortcode.IsDeleteEnabled = canAddEditDelete;
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
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the gLavaShortcode control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gLavaShortcode_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "lavaShortcodeId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gLavaShortcode control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gLavaShortCode_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "lavaShortcodeId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gLavaShortcode control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gLavaShortCode_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            LavaShortcodeService lavaShortcodeService = new LavaShortcodeService( rockContext );
            LavaShortcode lavaShortcode = lavaShortcodeService.Get( e.RowKeyId );

            if ( lavaShortcode != null )
            {
                // unregister the shortcode
                Template.UnregisterShortcode( lavaShortcode.TagName );

                string errorMessage;
                if ( !lavaShortcodeService.CanDelete( lavaShortcode, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                lavaShortcodeService.Delete( lavaShortcode );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gLavaShortcode control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gLavaShortcode_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            LavaShortcodeService lavaShortcodeService = new LavaShortcodeService( new RockContext() );
            SortProperty sortProperty = gLavaShortcode.SortProperty;

            var lavaShortcodes = lavaShortcodeService.Queryable();
           
            if ( sortProperty != null )
            {
                lavaShortcodes = lavaShortcodes.Sort( sortProperty );
            }
            else
            {
                lavaShortcodes = lavaShortcodes.OrderBy( p => p.Name );
            }

            gLavaShortcode.EntityTypeId = EntityTypeCache.Read<LavaShortcode>().Id;
            gLavaShortcode.DataSource = lavaShortcodes.ToList();
            gLavaShortcode.DataBind();
        }

        #endregion
    }
}