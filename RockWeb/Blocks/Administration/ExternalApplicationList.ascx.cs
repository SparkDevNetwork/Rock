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
using System.Linq;
using System.Web.UI;

using Rock.Attribute;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName("External Application List")]
    [Category( "Administration" )]
    [Description( "Will list all of the defined type values with the type of External Application.  This provides a way for users to select any one of these files." )]
    public partial class ExternalApplicationList : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gExternalApplication.DataKeyNames = new string[] { "Id" };
            gExternalApplication.Actions.ShowAdd = false;
            gExternalApplication.GridRebind += gExternalApplication_GridRebind;
            gExternalApplication.RowItemText = "External Application";
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
        /// Handles the RowDataBound event of the gExternalApplication control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gExternalApplication_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            var site = RockPage.Site;
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var definedValue = e.Row.DataItem as DefinedValue;
                
                var downloadCellIndex = gExternalApplication.Columns.IndexOf( gExternalApplication.Columns.OfType<HyperLinkField>().First( a => a.Text == "Download" ) );
                if ( downloadCellIndex >= 0 )
                {
                    string fileUrl = definedValue.GetAttributeValue("DownloadUrl");
                    e.Row.Cells[downloadCellIndex].Text = string.Format( "<a href='{0}' class='btn btn-action btn-xs'><i class='fa fa-download'></i> Download</a>", fileUrl );
                }

                Literal lAppName = e.Row.FindControl( "lAppName" ) as Literal;
                if ( lAppName != null )
                {
                    lAppName.Text = definedValue.Value;
                }

                Literal lVendorName = e.Row.FindControl( "lVendorName" ) as Literal;
                if (lVendorName != null)
                {
                    lVendorName.Text = definedValue.GetAttributeValue( "Vendor" );
                }

            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gExternalApplication control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gExternalApplication_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            Guid externalApplicationGuid = Rock.SystemGuid.DefinedType.EXTERNAL_APPLICATION.AsGuid();

            var definedValueService = new DefinedValueService( new RockContext() );
            var queryable = definedValueService.Queryable().Where( f => f.DefinedType.Guid == externalApplicationGuid );

            var sortProperty = gExternalApplication.SortProperty;

            List<DefinedValue> list;

            if ( sortProperty != null )
            {
                list = queryable.Sort( sortProperty ).ToList();
            }
            else
            {
                list = queryable.OrderBy( d => d.Order).ThenBy( d => d.Value ).ToList();
            }

            foreach ( var item in list )
            {
                item.LoadAttributes();
            }

            gExternalApplication.DataSource = list;

            gExternalApplication.DataBind();
        }


        #endregion

    }
}