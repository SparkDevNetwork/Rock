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
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// Lists all the Rest controllers.
    /// </summary>
    [DisplayName( "REST Controller List" )]
    [Category( "Core" )]
    [Description( "Lists all the REST controllers." )]

    [LinkedPage( "Detail Page" )]
    public partial class RestControllerList : RockBlock
    {

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gControllers.DataKeyNames = new string[] { "Id" };
            gControllers.GridRebind += gControllers_GridRebind;
            gControllers.Actions.ShowAdd = false;
            gControllers.IsDeleteEnabled = false;

            SecurityField securityField = gControllers.Columns[3] as SecurityField;
            securityField.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.RestController ) ).Id;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                var service = new RestControllerService( new RockContext() );
                if ( !service.Queryable().Any() )
                {
                    RefreshControllerList();
                }

                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the GridRebind event of the gControllers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gControllers_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

       /// <summary>
        /// Handles the RowSelected event of the gControllers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gControllers_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "controller", e.RowKeyValue.ToString() );
            NavigateToLinkedPage( "DetailPage", queryParams );
        }

        protected void btnRefreshAll_Click( object sender, EventArgs e )
        {
            RefreshControllerList();
        }
        
        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var service = new RestControllerService( new RockContext() );
            var sortProperty = gControllers.SortProperty;

            var qry = service.Queryable().Select( c => new
            {
                c.Id,
                c.Name,
                c.ClassName,
                Actions = c.Actions.Count()
            } );

            if (sortProperty != null)
            {
                qry = qry.Sort(sortProperty);
            }
            else
            {
                qry = qry.OrderBy( c => c.Name);
            }

            gControllers.EntityTypeId = EntityTypeCache.Read<RestController>().Id;
            gControllers.DataSource = qry.ToList();
            gControllers.DataBind();
        }

        private void RefreshControllerList()
        {
            RestControllerService.RegisterControllers();
        }

        #endregion


}
}