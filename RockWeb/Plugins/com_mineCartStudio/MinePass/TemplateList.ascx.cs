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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.UI;
using com.minecartstudio.MinePass.Client.Model;
using System.Data.Entity;

namespace RockWeb.Plugins.com_mineCartStudio.MinePass
{
    /// <summary>
    /// Block for listing Mine Pass Templates
    /// </summary>
    [DisplayName( "Mine Pass Template List" )]
    [Category( "Mine Cart Studio > Mine Pass" )]
    [Description( "Block for listing Mine Pass Templates." )]
    [LinkedPage( "Detail Page", "The detail page for Mine Passes.", true)]
    public partial class TemplateList : RockBlock, ICustomGridColumns, ISecondaryBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gTemplateList.GridRebind += gTemplateList_GridRebind;
            gTemplateList.DataKeyNames = new string[] { "Id" };
            gTemplateList.Actions.ShowAdd = true;
            gTemplateList.Actions.AddClick += gTemplateList_Add;
            gTemplateList.ShowConfirmDeleteDialog = true;

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
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                BindGrid();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on its page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlView.Visible = visible;
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

        /// <summary>
        /// Handles the GridRebind event of the gPledges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gTemplateList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Grid Events
        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var templateService = new MinePassTemplateService( rockContext );

            var qry = templateService.Queryable().AsNoTracking().Where( t => t.IsActive == true);

            gTemplateList.DataSource = qry.ToList();
            gTemplateList.DataBind();
        }

        /// <summary>
        /// Handles the Add event of the gAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gTemplateList_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "TemplateId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gTemplateList_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "TemplateId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Click event of the gTemplateDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gTemplateDelete_Click( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var templateService = new MinePassTemplateService( rockContext );

            var template = templateService.Get( e.RowKeyId );

            if ( template != null )
            {
                string errorMessage;
                if ( !templateService.CanDelete( template, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                templateService.Delete( template );

                rockContext.SaveChanges();
            }

            BindGrid();
        }

       

        #endregion
    }
}