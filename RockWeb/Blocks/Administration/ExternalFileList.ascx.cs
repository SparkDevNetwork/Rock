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
    [DisplayName("External File List")]
    [Category( "Administration" )]
    [Description( "Will list all of the binary files with the type of External File.  This provides a way for users to select any one of these files." )]
    public partial class ExternalFileList : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gBinaryFile.DataKeyNames = new string[] { "id" };
            gBinaryFile.Actions.ShowAdd = false;
            gBinaryFile.GridRebind += gBinaryFile_GridRebind;
            gBinaryFile.RowItemText = "External File";
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
        /// Handles the Click event of the Download control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void Download_Click( object sender, RowEventArgs e )
        {
            string fileUrl = string.Format( "{0}GetFile.ashx?id={1}", ResolveUrl( "~" ), e.RowKeyId );
            Response.Redirect( fileUrl, false );
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void gBinaryFile_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            var site = RockPage.Site;
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var downloadCellIndex = gBinaryFile.Columns.IndexOf( gBinaryFile.Columns.OfType<HyperLinkField>().First( a => a.Text == "Download" ) );
                if ( downloadCellIndex >= 0 )
                {
                    string fileUrl = string.Format( "{0}GetFile.ashx?id={1}", ResolveUrl( "~" ), e.Row.DataItem.GetPropertyValue( "Id" ).ToString() );
                    e.Row.Cells[downloadCellIndex].Text = string.Format( "<a href='{0}' class='btn btn-action btn-xs'><i class='fa fa-download'></i> Download</a>", fileUrl );
                }

                Literal lAppName = e.Row.FindControl( "lAppName" ) as Literal;
                if ( lAppName != null )
                {
                    var binaryFile = e.Row.DataItem as BinaryFile;
                    binaryFile.LoadAttributes();
                    lAppName.Text = binaryFile.GetAttributeValue( "Name" );
                }

            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gBinaryFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gBinaryFile_GridRebind( object sender, EventArgs e )
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
            Guid binaryFileTypeGuid = Rock.SystemGuid.BinaryFiletype.EXTERNAL_FILE.AsGuid();

            var binaryFileService = new BinaryFileService();
            var queryable = binaryFileService.Queryable().Where( f => f.BinaryFileType.Guid == binaryFileTypeGuid );

            var sortProperty = gBinaryFile.SortProperty;

            List<BinaryFile> list;

            if ( sortProperty != null )
            {
                list = queryable.Sort( sortProperty ).ToList();
            }
            else
            {
                list = queryable.OrderBy( d => d.FileName ).ToList();
            }

            foreach ( var item in list )
            {
                item.LoadAttributes();
            }

            gBinaryFile.DataSource = list;

            gBinaryFile.DataBind();
        }


        #endregion

    }
}