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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Attribute;
using Rock.Security;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Business List" )]
    [Category( "Finance" )]
    [Description( "Lists all businesses and provides filtering by business name and owner" )]
    [LinkedPage( "Detail Page" )]
    public partial class BusinessList : Rock.Web.UI.RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            bool canEdit = IsUserAuthorized( Authorization.EDIT );

            gfBusinessFilter.ApplyFilterClick += gfBusinessFilter_ApplyFilterClick;
            gfBusinessFilter.DisplayFilterValue += gfBusinessFilter_DisplayFilterValue;

            gBusinessList.DataKeyNames = new string[] { "id" };
            gBusinessList.Actions.ShowAdd = canEdit;
            gBusinessList.Actions.AddClick += gBusinessList_AddClick;
            gBusinessList.GridRebind += gBusinessList_GridRebind;
            gBusinessList.IsDeleteEnabled = canEdit;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }
        }

        #endregion

        #region Events

        void gfBusinessFilter_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
        {
            
        }

        void gfBusinessFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            
        }

        void gBusinessList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        void gBusinessList_AddClick( object sender, EventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "businessId", "0" );
            NavigateToLinkedPage( "DetailPage", parms );
        }

        protected void gBusinessList_RowDataBound( object sender, GridViewRowEventArgs e )
        {

        }

        protected void gBusinessList_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {

        }

        protected void gBusinessList_Edit( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {

        }

        protected void gBusinessList_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {

        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            // Need filters for Business Name & Owner
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            gBusinessList.DataBind();
        }

        /// <summary>
        /// Shows the detail form.
        /// </summary>
        /// <param name="id">The id.</param>
        protected void ShowDetailForm( int id )
        {
            NavigateToLinkedPage( "DetailPage", "businessId", id );
        }

        #endregion
}
}