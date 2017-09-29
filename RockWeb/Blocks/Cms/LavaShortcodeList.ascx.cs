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
using System.Web.UI.WebControls;

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

        public bool canAddEditDelete = false;

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
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            canAddEditDelete = IsUserAuthorized( Authorization.EDIT );

            if ( !Page.IsPostBack )
            {
                LoadShortcodes();
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
        private void Block_BlockUpdated( object sender, EventArgs e )
        {
            LoadShortcodes();
        }

        /// <summary>
        /// Handles the Click event of the btnAddShortcut control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddShortcut_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "lavaShortcodeId", 0 );
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            LinkButton btn = ( LinkButton ) sender;
            RepeaterItem item = ( RepeaterItem ) btn.NamingContainer;
            HiddenField hfShortcodeId = ( HiddenField ) item.FindControl( "hfShortcodeId" );

            NavigateToLinkedPage( "DetailPage", "lavaShortcodeId", hfShortcodeId.ValueAsInt() );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            LinkButton btn = ( LinkButton ) sender;
            RepeaterItem item = ( RepeaterItem ) btn.NamingContainer;
            HiddenField hfShortcodeId = ( HiddenField ) item.FindControl( "hfShortcodeId" );

            var rockContext = new RockContext();
            LavaShortcodeService lavaShortcodeService = new LavaShortcodeService( rockContext );
            LavaShortcode lavaShortcode = lavaShortcodeService.Get( hfShortcodeId.ValueAsInt() );

            if ( lavaShortcode != null )
            {
                // unregister the shortcode
                Template.UnregisterShortcode( lavaShortcode.TagName );

                lavaShortcodeService.Delete( lavaShortcode );
                rockContext.SaveChanges();
            }

            LoadShortcodes();
        }

        protected void rptShortcodes_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem ) 
            {
                if ( !canAddEditDelete )
                {
                    e.Item.FindControl( "btnEdit" ).Visible = false;
                    e.Item.FindControl( "btnDelete" ).Visible = false;
                }

                LavaShortcode dataItem = (LavaShortcode)e.Item.DataItem;

                e.Item.FindControl( "divEditPanel" ).Visible = !dataItem.IsSystem;
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tglShowActive control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglShowActive_CheckedChanged( object sender, EventArgs e )
        {
            LoadShortcodes();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Loads the shortcodes.
        /// </summary>
        private void LoadShortcodes()
        {
            LavaShortcodeService lavaShortcodeService = new LavaShortcodeService( new RockContext() );
            var lavaShortcodes = lavaShortcodeService.Queryable();

            if ( tglShowActive.Checked )
            {
                lavaShortcodes = lavaShortcodes.Where( s => s.IsActive == true );
            }

            rptShortcodes.DataSource = lavaShortcodes.ToList().OrderBy( s => s.Name );
            rptShortcodes.DataBind();
        }

        #endregion



        
    }
}