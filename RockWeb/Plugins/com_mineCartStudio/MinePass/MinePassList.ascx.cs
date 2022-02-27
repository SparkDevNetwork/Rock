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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.UI;
using com.minecartstudio.MinePass.Client.Model;
using System.Data.Entity;
using com.minecartstudio.MinePass.Client;
using com.minecartstudio.MinePass.Client.MinePassApi;
using System.Net;

namespace RockWeb.Plugins.com_mineCartStudio.MinePass
{
    /// <summary>
    /// Block for listing Mine Pass Templates
    /// </summary>
    [DisplayName( "Mine Pass List" )]
    [Category( "Mine Cart Studio > Mine Pass" )]
    [Description( "Block for listing Mine Passes." )]
    public partial class MinePassList : RockBlock, ICustomGridColumns, ISecondaryBlock
    {
        #region Private Properties
        int _templateId = 0;
        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gMinePassList.GridRebind += gTemplateList_GridRebind;
            gMinePassList.DataKeyNames = new string[] { "PersonAliasId" };
            gMinePassList.Actions.ShowAdd = true;
            gMinePassList.Actions.AddClick += gMinePassList_AddClick;


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

            _templateId = PageParameter( "TemplateId" ).AsInteger();

            if ( !Page.IsPostBack )
            {
                BindGrid();
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

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
            RockContext rockContext = new RockContext();
            var minePassService = new MinePassService( rockContext );

            var qry = minePassService.Queryable().Include( "PersonAlias.Person" ).AsNoTracking()
                        .Where( m => m.MinePassTemplateId == _templateId )
                        .ToList();

            gMinePassList.DataSource = qry.ToList();
            gMinePassList.DataBind();
        }

        private void gMinePassList_AddClick( object sender, EventArgs e )
        {
            ppPassPerson.PersonId = null;

            mdPassDetail.Show();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gMinePassList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gMinePassList_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                HyperLink hl = e.Row.FindControl( "hlDownload" ) as HyperLink;
                var pass = ( com.minecartstudio.MinePass.Client.Model.MinePass ) e.Row.DataItem;

                if ( hl != null && pass != null )
                {
                    hl.NavigateUrl = string.Format( ResolveRockUrl( string.Format( "~/GetMinePass.ashx?PassTemplateId={0}&PersonKey={1}", pass.MinePassTemplateId, pass.PersonAlias.Guid ) ) );
                }
            }
        }

        /// <summary>
        /// Handles the Delete event of the gMinePassList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gMinePassList_Delete( object sender, RowEventArgs e )
        {
            var personAliasId = (int?)e.RowKeyValue;

            if ( personAliasId.HasValue )
            {
                var rockContext = new RockContext();
                var minePassService = new MinePassService( rockContext );

                var personId = new PersonAliasService( rockContext ).Queryable()
                                    .Where( p => p.Id == personAliasId )
                                    .Select( p => p.PersonId )
                                    .FirstOrDefault();

                var pass = minePassService.Queryable()
                                .Where( q =>
                                    q.MinePassTemplateId == _templateId
                                    && q.PersonAlias.PersonId == personId )
                                .FirstOrDefault();

                if ( pass != null )
                {
                    // Delete pass from Mine Cart server
                    MinePassApiManager.DeletePass( pass.SerialNumber );

                    minePassService.Delete( pass );
                    rockContext.SaveChanges();
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdBeaconDetail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdPassDetail_SaveClick( object sender, EventArgs e )
        {
            var personId = ppPassPerson.PersonId;
            var personAliasId = ppPassPerson.PersonAliasId;

            if ( personId.HasValue && personAliasId.HasValue )
            {
                var rockContext = new RockContext();
                var minePassService = new MinePassService( rockContext );

                // Check that the person does not already have a pass record
                var pass = minePassService.Queryable()
                                    .Where( q =>
                                        q.MinePassTemplateId == _templateId
                                        && q.PersonAlias.PersonId == personId.Value )
                                    .FirstOrDefault();

                if ( pass == null )
                {
                    pass = new com.minecartstudio.MinePass.Client.Model.MinePass();
                    minePassService.Add( pass );
                    pass.MinePassTemplateId = _templateId;
                    pass.PersonAliasId = personAliasId.Value;
                    pass.MinePassStatus = com.minecartstudio.MinePass.Common.Enums.MinePassStatus.NotUploaded;
                    pass.LastUpdateDateTime = RockDateTime.Now;

                    rockContext.SaveChanges();
                }
            }

            mdPassDetail.Hide();

            BindGrid();
        }

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on its page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlView.Visible = visible;
        }
        #endregion
    }
}