using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Security;
using Rock.Web.UI;

using com.bemaservices.RemoteCheckDeposit;
using com.bemaservices.RemoteCheckDeposit.Model;
using Rock.Web;
using Rock.Web.UI.Controls;
using Rock.Web.Cache;

namespace RockWeb.Plugins.com_bemaservices.RemoteCheckDeposit
{
    [DisplayName( "File Format Detail" )]
    [Category( "BEMA Services > Remote Check Deposit" )]
    [Description( "Displays the details for a file format." )]

    public partial class FileFormatDetail : RockBlock
    {
        #region Properties

        public int FileFormatId
        {
            get
            {
                return ViewState["FileFormatId"] as int? ?? 0;
            }
            set
            {
                ViewState["FileFormatId"] = value;
            }
        }

        public int? FileFormatEntityTypeId
        {
            get
            {
                return ViewState["FileFormatEntityTypeId"] as int?;
            }
            set
            {
                ViewState["FileFormatEntityTypeId"] = value;
            }
        }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Load the view state.
        /// </summary>
        /// <param name="savedState">The previously saved view state.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var fileFormat = new ImageCashLetterFileFormat { Id = FileFormatId, EntityTypeId = FileFormatEntityTypeId };
            BuildDynamicControls( fileFormat, false );
        }

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
                ShowDetail( PageParameter( "Id" ).AsInteger() );
            }
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Shows the edit panel.
        /// </summary>
        /// <param name="fileFormatId">The file format identifier.</param>
        public void ShowDetail( int fileFormatId )
        {
            var rockContext = new RockContext();
            ImageCashLetterFileFormat fileFormat = null;
            bool editAllowed = IsUserAuthorized( Authorization.EDIT );

            if ( fileFormatId != 0 )
            {
                fileFormat = new ImageCashLetterFileFormatService( rockContext ).Get( fileFormatId );
                editAllowed = editAllowed || fileFormat.IsAuthorized( Authorization.EDIT, CurrentPerson );
                pdAuditDetails.SetEntity( fileFormat, ResolveRockUrl( "~" ) );
            }

            if ( fileFormat == null )
            {
                fileFormat = new ImageCashLetterFileFormat { Id = 0 };
                fileFormat.FileNameTemplate = "{{ 'Now' | Date:'yyyyMMddHHmm.x937' }}";
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            FileFormatId = fileFormat.Id;

            bool readOnly = false;
            nbEditModeMessage.Text = string.Empty;
            if ( !editAllowed || !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( "File Format" );
            }

            if ( readOnly )
            {
                ShowReadonlyDetails( fileFormat );
            }
            else
            {
                ShowEditDetails( fileFormat );
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="fileFormat">The file format.</param>
        private void ShowEditDetails( ImageCashLetterFileFormat fileFormat )
        {
            if ( fileFormat.Id == 0 )
            {
                lActionTitle.Text = ActionTitle.Add( "File Format" );
            }
            else
            {
                lActionTitle.Text = fileFormat.Name.FormatAsHtmlTitle();
            }

            hlInactive.Visible = !fileFormat.IsActive;

            SetEditMode( true );

            tbName.Text = fileFormat.Name;
            cbIsActive.Checked = fileFormat.IsActive;
            tbDescription.Text = fileFormat.Description;
            tbFileNameTemplate.Text = fileFormat.FileNameTemplate;
            cpFileFormatType.SetValue( fileFormat.EntityType != null ? fileFormat.EntityType.Guid.ToString().ToUpper() : string.Empty );

            BuildDynamicControls( fileFormat, true );
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="fileFormat">The file format.</param>
        private void ShowReadonlyDetails( ImageCashLetterFileFormat fileFormat )
        {
            SetEditMode( false );

            lActionTitle.Text = fileFormat.Name.FormatAsHtmlTitle();
            hlInactive.Visible = !fileFormat.IsActive;
            lFileFormatDescription.Text = fileFormat.Description;

            var descriptionList = new DescriptionList();

            if ( fileFormat.EntityType != null )
            {
                descriptionList.Add( "File Format Type", fileFormat.EntityType.Name );
            }

            descriptionList.Add( "File Name Template", fileFormat.FileNameTemplate );

            lblMainDetails.Text = descriptionList.Html;

            phAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddDisplayControls( fileFormat, phAttributes, new List<string> { "Active", "Order" } );
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">If set to <c>true</c> the block is in the editable state.</param>
        protected void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            pnlViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Build the dynamic edit controls for this file format.
        /// </summary>
        /// <param name="fileFormat">The file format whose controls need to be built.</param>
        /// <param name="SetValues">Whether or not to set the initial values.</param>
        protected void BuildDynamicControls( ImageCashLetterFileFormat fileFormat, bool SetValues )
        {
            FileFormatEntityTypeId = fileFormat.EntityTypeId;

            fileFormat.LoadAttributes();
            phEditAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddEditControls( fileFormat, phEditAttributes, SetValues, BlockValidationGroup, new List<string> { "Active", "Order" }, false, 2 );
            foreach ( var tb in phAttributes.ControlsOfTypeRecursive<TextBox>() )
            {
                tb.AutoCompleteType = AutoCompleteType.Disabled;
                tb.Attributes["autocomplete"] = "off";
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            ImageCashLetterFileFormat fileFormat = null;

            var fileFormatService = new ImageCashLetterFileFormatService( rockContext );

            if ( FileFormatId != 0 )
            {
                fileFormat = fileFormatService.Get( FileFormatId );
            }

            if ( fileFormat == null )
            {
                fileFormat = new ImageCashLetterFileFormat();
                fileFormatService.Add( fileFormat );
            }

            if ( fileFormat != null )
            {
                var completedProjectIds = new List<int>();

                fileFormat.Name = tbName.Text;
                fileFormat.Description = tbDescription.Text;
                fileFormat.IsActive = cbIsActive.Checked;
                fileFormat.FileNameTemplate = tbFileNameTemplate.Text;
                fileFormat.EntityTypeId = cpFileFormatType.SelectedEntityTypeId;

                //
                // Store the attribute values.
                //
                fileFormat.LoadAttributes( rockContext );
                Rock.Attribute.Helper.GetEditValues( phEditAttributes, fileFormat );

                if ( !Page.IsValid )
                {
                    // Controls will render the error messages                    
                    return;
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    fileFormat.SaveAttributeValues( rockContext );
                } );

                NavigateToParentPage();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail( PageParameter( "Id" ).AsInteger() );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cpFileFormatType_SelectedIndexChanged( object sender, EventArgs e )
        {
            var fileFormat = new ImageCashLetterFileFormat { Id = FileFormatId, EntityTypeId = cpFileFormatType.SelectedEntityTypeId };
            BuildDynamicControls( fileFormat, true );
        }

        #endregion
    }
}