//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Linq;
using System.Web.UI;
using Rock.Attribute;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
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
                var service = new BinaryFileTypeService();
                binaryFileType = service.Get( binaryFileTypeGuid );
            }

            BindFilter();
            fBinaryFile.ApplyFilterClick += fBinaryFile_ApplyFilterClick;
            
            gBinaryFile.DataKeyNames = new string[] { "id" };
            gBinaryFile.Actions.ShowAdd = true;
            gBinaryFile.Actions.AddClick += gBinaryFile_Add;
            gBinaryFile.GridRebind += gBinaryFile_GridRebind;
            gBinaryFile.RowItemText = binaryFileType != null ? binaryFileType.Name : "Binary File";

            // Block Security and special attributes (RockPage takes care of "View")
            bool canAddEditDelete = IsUserAuthorized( "Edit" );
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
            NavigateToLinkedPage( "DetailPage", "BinaryFileId", (int)e.RowKeyValue );
        }

        /// <summary>
        /// Handles the Delete event of the gBinaryFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gBinaryFile_Delete( object sender, RowEventArgs e )
        {
            RockTransactionScope.WrapTransaction( () =>
            {
                BinaryFileService BinaryFileService = new BinaryFileService();
                BinaryFile BinaryFile = BinaryFileService.Get( (int)e.RowKeyValue );

                if ( BinaryFile != null )
                {
                    string errorMessage;
                    if ( !BinaryFileService.CanDelete( BinaryFile, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    BinaryFileService.Delete( BinaryFile, CurrentPersonId );
                    BinaryFileService.Save( BinaryFile, CurrentPersonId );
                }
            } );

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
            var binaryFileService = new BinaryFileService();
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