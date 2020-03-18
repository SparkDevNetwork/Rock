using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

using com.bemaservices.RemoteCheckDeposit;
using com.bemaservices.RemoteCheckDeposit.Model;

namespace RockWeb.Plugins.com_bemaservices.RemoteCheckDeposit
{
    [DisplayName( "File Format List" )]
    [Category( "BEMA Services > Remote Check Deposit" )]
    [Description( "Lists file formats in the system." )]

    [LinkedPage( "Detail Page", "The page that allows the user to edit the details of a file format.", true, "", "", 0 )]
    public partial class FileFormatList : RockBlock
    {
        #region Base Method Overrides

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

            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );

            gFileFormat.DataKeyNames = new string[] { "Id" };
            gFileFormat.Actions.AddClick += gFileFormat_Add;
            gFileFormat.GridRebind += gFileFormat_GridRebind;
            gFileFormat.Actions.ShowAdd = canAddEditDelete;
            gFileFormat.IsDeleteEnabled = canAddEditDelete;
        }

        /// <summary>
        /// Initialize basic information about the page structure and setup the default content.
        /// </summary>
        /// <param name="sender">Object that is generating this event.</param>
        /// <param name="e">Arguments that describe this event.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !IsPostBack )
            {
                BindGrid();
            }
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Bind the data grid to the list of file formats in the system.
        /// </summary>
        private void BindGrid()
        {
            var fileFormatService = new ImageCashLetterFileFormatService( new RockContext() );

            var types = fileFormatService.Queryable( "EntityType" )
                .OrderBy( f => f.Name )
                .ThenBy( f => f.Id )
                .ToList();

            gFileFormat.EntityTypeId = EntityTypeCache.Get<ImageCashLetterFileFormat>().Id;
            gFileFormat.DataSource = types;
            gFileFormat.DataBind();
        }

        /// <summary>
        /// Get the friendly name of the component.
        /// </summary>
        /// <param name="entityTypeObject">The entity type object whose name we want.</param>
        /// <returns>A string representing the name of the component.</returns>
        protected string GetComponentName( object entityTypeObject )
        {
            var entityType = entityTypeObject as Rock.Model.EntityType;

            if ( entityType != null )
            {
                string name = FileFormatTypeContainer.GetComponentName( entityType.Name );
                if ( !string.IsNullOrWhiteSpace( name ) )
                {
                    return name.SplitCase();
                }
            }

            return string.Empty;
        }

        #endregion

        #region Event Handlers

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
        /// Handles the RowSelected event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gFileFormat_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "Id", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gFileFormat_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            var fileFormatService = new ImageCashLetterFileFormatService( rockContext );
            var fileFormat = fileFormatService.Get( e.RowKeyId );

            if ( fileFormat != null )
            {
                int fileFormatId = fileFormat.Id;

                if ( !fileFormat.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                {
                    mdGridWarning.Show( "Sorry, you're not authorized to delete this file format.", ModalAlertType.Alert );
                    return;
                }

                fileFormatService.Delete( fileFormat );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gFileFormat_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gFileFormat_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "Id", 0 );
        }

        #endregion
    }
}