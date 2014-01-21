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
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.UI;
using Rock.Attribute;
using Rock.Web.UI;

namespace RockWeb.Blocks.Administration
{
    [DisplayName( "Rest Controller List" )]
    [Category( "Core" )]
    [Description( "Lists all the rest controllers." )]

    [LinkedPage( "Detail Page" )]
    public partial class RestControllerList : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gItems.DataKeyNames = new string[] { "ControllerType" };
            gItems.GridRebind += gItems_GridRebind;
            gItems.Actions.ShowAdd = false;
            gItems.IsDeleteEnabled = false;
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

        /// <summary>
        /// Handles the GridRebind event of the gItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gItems_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var config = GlobalConfiguration.Configuration;
            var explorer = config.Services.GetApiExplorer();

            var apiList = explorer.ApiDescriptions.OrderBy( a => a.RelativePath ).ThenBy( a => a.HttpMethod.Method ).ToList();
            var controllerList = apiList
                .GroupBy( a => a.ActionDescriptor.ControllerDescriptor.ControllerName )
                .Select( s => new
                {
                    ControllerName = s.Key,
                    ControllerType = s.Max( a => a.ActionDescriptor.ControllerDescriptor.ControllerType.FullName ),
                    Actions = s.Count()
                } ).ToList();

            gItems.DataSource = controllerList;
            gItems.DataBind();
        }

        /// <summary>
        /// Handles the RowSelected event of the gItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gItems_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "controllerType", e.RowKeyValue.ToString() );
            NavigateToLinkedPage( "DetailPage", queryParams );
        }
    }
}