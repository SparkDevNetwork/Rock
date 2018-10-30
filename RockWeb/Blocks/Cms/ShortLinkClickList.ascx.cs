﻿// <copyright>
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
using System.Data.Entity;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName("Short Link Click List")]
    [Category("CMS")]
    [Description("Lists clicks for a particular short link.")]
    public partial class ShortLinkClickList : RockBlock, ISecondaryBlock, ICustomGridColumns
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gShortLinkClicks.DataKeyNames = new string[] { "Id" };
            gShortLinkClicks.Actions.ShowAdd = false;
            gShortLinkClicks.GridRebind += gShortLinkClicks_GridRebind;
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
                BindShortLinkClicksGrid();
            }
        }

        #endregion

        #region ShortLinkClicks Grid

        /// <summary>
        /// Handles the GridRebind event of the gShortLinkClicks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gShortLinkClicks_GridRebind( object sender, EventArgs e )
        {
            BindShortLinkClicksGrid();
        }

        /// <summary>
        /// Binds the group members grid.
        /// </summary>
        protected void BindShortLinkClicksGrid()
        {
            int shortLinkId = PageParameter( "ShortLinkId" ).AsInteger();
            if ( shortLinkId == 0 )
            {
                pnlContent.Visible = false;
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var dv = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_URLSHORTENER );
                if ( dv != null )
                {
                    var qry = new InteractionService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( i =>
                            i.InteractionComponent.Channel.ChannelTypeMediumValueId == dv.Id &&
                            i.InteractionComponent.EntityId == shortLinkId );

                    SortProperty sortProperty = gShortLinkClicks.SortProperty;
                    if ( sortProperty != null )
                    {
                        gShortLinkClicks.SetLinqDataSource( qry.Sort( sortProperty ) );
                    }
                    else
                    {
                        gShortLinkClicks.SetLinqDataSource( qry.OrderByDescending( i => i.InteractionDateTime ) );
                    }

                    gShortLinkClicks.DataBind();
                    pnlContent.Visible = true;
                }
                else
                {
                    pnlContent.Visible = false;
                }
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