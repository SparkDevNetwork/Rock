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
using Rock.Security;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Binary File List" )]
    [Category( "Core" )]
    [Description( "Shows a list of all binary files." )]

    [LinkedPage("Detail Page")]
    [BinaryFileTypeField]
    public partial class BinaryFileList : RockBlock
    {
        private BinaryFileType binaryFileType = null;

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            Guid binaryFileTypeGuid = Guid.NewGuid();
            if ( Guid.TryParse( GetAttributeValue( "BinaryFileType" ), out binaryFileTypeGuid ) )
            {
                var service = new BinaryFileTypeService( new RockContext() );
                binaryFileType = service.Get( binaryFileTypeGuid );
            }

            BindFilter();
            fBinaryFile.ApplyFilterClick += fBinaryFile_ApplyFilterClick;
            
            gBinaryFile.DataKeyNames = new string[] { "Id" };
            gBinaryFile.Actions.ShowAdd = true;
            gBinaryFile.Actions.AddClick += gBinaryFile_Add;
            gBinaryFile.GridRebind += gBinaryFile_GridRebind;
            gBinaryFile.RowItemText = binaryFileType != null ? binaryFileType.Name : "Binary File";

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gBinaryFile.Actions.ShowAdd = canAddEditDelete;
            gBinaryFile.IsDeleteEnabled = canAddEditDelete;
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

        #region Grid Events (main grid)

        /// <summary>
        /// Handles the ApplyFilterClick event of the fBinaryFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void fBinaryFile_ApplyFilterClick( object sender, EventArgs e )
        {
            fBinaryFile.SaveUserPreference( "File Name", tbName.Text );
            fBinaryFile.SaveUserPreference( "Mime Type", tbType.Text );
            fBinaryFile.SaveUserPreference( "Include Temporary", dbIncludeTemporary.Checked.ToString() );

            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the gBinaryFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gBinaryFile_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "BinaryFileId", 0, "BinaryFileTypeId", binaryFileType != null ? binaryFileType.Id : 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gBinaryFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gBinaryFile_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "BinaryFileId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gBinaryFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gBinaryFile_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            BinaryFileService binaryFileService = new BinaryFileService( rockContext );
            BinaryFile binaryFile = binaryFileService.Get( e.RowKeyId );

            if ( binaryFile != null )
            {
                string errorMessage;
                if ( !binaryFileService.CanDelete( binaryFile, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                binaryFileService.Delete( binaryFile );
                rockContext.SaveChanges();
            }

            BindGrid();
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

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            if ( !Page.IsPostBack )
            {
                tbName.Text = fBinaryFile.GetUserPreference( "File Name" );
                tbType.Text = fBinaryFile.GetUserPreference( "Mime Type" );
                bool includeTemp = false;
                dbIncludeTemporary.Checked = bool.TryParse(fBinaryFile.GetUserPreference("Include Temporary"), out includeTemp) && includeTemp;
            }            
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            Guid binaryFileTypeGuid = binaryFileType != null ? binaryFileType.Guid : Guid.NewGuid();
            var binaryFileService = new BinaryFileService( new RockContext() );
            var queryable = binaryFileService.Queryable().Where( f => f.BinaryFileType.Guid == binaryFileTypeGuid );

            bool includeTemp = false;
            if (!bool.TryParse( fBinaryFile.GetUserPreference( "Include Temporary" ), out includeTemp ) || !includeTemp)
            {
                queryable = queryable.Where( f => f.IsTemporary == false );
            }

            var sortProperty = gBinaryFile.SortProperty;
            string name = fBinaryFile.GetUserPreference( "File Name" );
            if ( !string.IsNullOrWhiteSpace( name ) )
            {
                queryable = queryable.Where( f => f.FileName.Contains( name ) );
            }

            string type = fBinaryFile.GetUserPreference( "Mime Type" );
            if ( !string.IsNullOrWhiteSpace( type ) )
            {

                queryable = queryable.Where( f => f.MimeType.Contains( name ) );
            }

            if ( sortProperty != null )
            {
                gBinaryFile.DataSource = queryable.Sort( sortProperty ).ToList();
            }
            else
            {
                gBinaryFile.DataSource = queryable.OrderBy( d => d.FileName ).ToList();
            }


            gBinaryFile.DataBind();
        }

        #endregion
    }
}