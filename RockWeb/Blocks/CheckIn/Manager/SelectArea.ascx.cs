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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn.Manager
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Select Check-In Area" )]
    [Category( "Check-in > Manager" )]
    [Description( "Block used to select a type of check-in area before managing locations." )]

    [LinkedPage( "Location Page", "Page used to display locations", order: 2 )]
    public partial class SelectArea : Rock.Web.UI.RockBlock
    {
        #region Fields
    
        #endregion

        #region Properties

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            upnlContent.OnPostBack += upnlContent_OnPostBack;

            rptNavItems.ItemDataBound += rptNavItems_ItemDataBound;
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
                BindData();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
        }

        /// <summary>
        /// Handles the OnPostBack event of the upnlContent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PostBackEventArgs"/> instance containing the event data.</param>
        void upnlContent_OnPostBack( object sender, PostBackEventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add("Area", e.EventArgument);
            NavigateToLinkedPage( "LocationPage", parms );
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptNavItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        void rptNavItems_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var groupType = e.Item.DataItem as GroupType;
            var li = e.Item.FindControl( "liNavItem" ) as HtmlGenericControl;
            if ( groupType != null && li != null )
            {
                li.Attributes["onClick"] = upnlContent.GetPostBackEventReference( groupType.Guid.ToString() );
            }
        }


        #endregion

        #region Methods

        private void BindData()
        {
            Guid? guid = Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid();
            if ( guid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    rptNavItems.DataSource = new GroupTypeService( rockContext )
                        .Queryable()
                        .Where( g => g.GroupTypePurposeValue.Guid.Equals( guid.Value ) )
                        .OrderBy( g => g.Name )
                        .ToList();
                    rptNavItems.DataBind();
                }
            }
        }

        #endregion

    }
}