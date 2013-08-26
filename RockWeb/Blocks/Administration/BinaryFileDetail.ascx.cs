//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Administration
{
    [BooleanField("Show Binary File Type")]
    [WorkflowTypeField("Workflow", "An optional workflow to activate for any new file uploaded")]
    public partial class BinaryFileDetail : RockBlock, IDetailBlock
    {

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            fsFile.FileUploaded += fsFile_FileUploaded;
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
                string itemId = PageParameter( "BinaryFileId" );
                string typeId = PageParameter( "BinaryFileTypeId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    if ( string.IsNullOrWhiteSpace( typeId ) )
                    {

                        ShowDetail( "BinaryFileId", int.Parse( itemId ) );
                    }
                    else
                    {
                        ShowDetail( "BinaryFileId", int.Parse( itemId ), int.Parse( typeId ) );
                    }
                }
                else
                {
                    pnlDetails.Visible = false;
                }

                bool showBinaryFileType = false;
                if ( Boolean.TryParse( GetAttributeValue( "ShowBinaryFileType" ), out showBinaryFileType ) && showBinaryFileType )
                {
                    ddlBinaryFileType.Visible = true;
                }
                else
                {
                    ddlBinaryFileType.Visible = false;
                }
            }
            else
            {
                if ( pnlDetails.Visible )
                {
                    var binaryFile = new BinaryFile { BinaryFileTypeId = ddlBinaryFileType.SelectedValueAsInt() ?? 0 };
                    if ( binaryFile.BinaryFileTypeId > 0 )
                    {
                        binaryFile.LoadAttributes();
                        phAttributes.Controls.Clear();
                        Rock.Attribute.Helper.AddEditControls( binaryFile, phAttributes, false );
                    }
                }
            }

        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            ShowDetail( itemKey, itemKeyValue, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        /// <param name="binaryFileTypeId">The binary file type id.</param>
        public void ShowDetail( string itemKey, int itemKeyValue, int? binaryFileTypeId )
        {
            if ( !itemKey.Equals( "BinaryFileId" ) )
            {
                return;
            }

            var binaryFileService = new BinaryFileService();
            BinaryFile BinaryFile = null;

            if ( !itemKeyValue.Equals( 0 ) )
            {
                BinaryFile = binaryFileService.Get( itemKeyValue );
            }

            if (BinaryFile != null)
            {
                lActionTitle.Text = ActionTitle.Edit( BinaryFile.BinaryFileType.Name );
            }
            else
            {
                BinaryFile = new BinaryFile { Id = 0, IsSystem = false, BinaryFileTypeId = binaryFileTypeId };
                
                string friendlyName = BinaryFile.FriendlyTypeName;
                if ( binaryFileTypeId.HasValue )
                {
                    var binaryFileType = new BinaryFileTypeService().Get( binaryFileTypeId.Value );
                    if ( binaryFileType != null )
                    {
                        friendlyName = binaryFileType.Name;
                    }
                }

                lActionTitle.Text = ActionTitle.Add( friendlyName );
            }

            ShowDetail( BinaryFile );

        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="binaryFile">The binary file.</param>
        public void ShowDetail(BinaryFile binaryFile)
        {

            pnlDetails.Visible = true;
            hfBinaryFileId.SetValue( binaryFile.Id );

            fsFile.BinaryFileId = binaryFile.Id;
            tbName.Text = binaryFile.FileName;
            tbDescription.Text = binaryFile.Description;
            tbMimeType.Text = binaryFile.MimeType;
            ddlBinaryFileType.SetValue( binaryFile.BinaryFileTypeId );

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( "Edit" ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( BinaryFile.FriendlyTypeName );
            }

            phAttributes.Controls.Clear();
            binaryFile.LoadAttributes();

            if ( readOnly )
            {
                lActionTitle.Text = ActionTitle.View( BinaryFile.FriendlyTypeName );
                btnCancel.Text = "Close";
                Rock.Attribute.Helper.AddDisplayControls( binaryFile, phAttributes );
            }
            else
            {
                Rock.Attribute.Helper.AddEditControls( binaryFile, phAttributes, true );
            }

            tbName.ReadOnly = readOnly;
            tbDescription.ReadOnly = readOnly;
            tbMimeType.ReadOnly = readOnly;
            ddlBinaryFileType.Enabled = !readOnly;

            btnSave.Visible = !readOnly;

        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            BinaryFile BinaryFile;
            BinaryFileService BinaryFileService = new BinaryFileService();
            AttributeService attributeService = new AttributeService();

            int BinaryFileId = int.Parse( hfBinaryFileId.Value );

            if ( BinaryFileId == 0 )
            {
                BinaryFile = new BinaryFile();
                BinaryFileService.Add( BinaryFile, CurrentPersonId );
            }
            else
            {
                BinaryFile = BinaryFileService.Get( BinaryFileId );
            }

            BinaryFile.IsTemporary = false;
            BinaryFile.FileName = tbName.Text;
            BinaryFile.Description = tbDescription.Text;
            BinaryFile.MimeType = tbMimeType.Text;
            BinaryFile.BinaryFileTypeId = ddlBinaryFileType.SelectedValueAsInt();

            BinaryFile.LoadAttributes();
            Rock.Attribute.Helper.GetEditValues( phAttributes, BinaryFile );

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !BinaryFile.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            RockTransactionScope.WrapTransaction( () =>
            {
                BinaryFileService.Save( BinaryFile, CurrentPersonId );
                Rock.Attribute.Helper.SaveAttributeValues( BinaryFile, CurrentPersonId );
            } );

            NavigateToParentPage();
        }

        #endregion

        #region Activities and Actions

        /// <summary>
        /// Handles the FileUploaded event of the fsFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void fsFile_FileUploaded( object sender, EventArgs e )
        {
            using (new Rock.Data.UnitOfWorkScope() )
            {
                var binaryFileService = new BinaryFileService();
                var binaryFile = binaryFileService.Get(fsFile.BinaryFileId.Value);
                if ( binaryFile != null )
                {
                    if ( !String.IsNullOrWhiteSpace( tbName.Text ) )
                    {
                        binaryFile.FileName = tbName.Text;
                    }
                    binaryFile.Description = tbDescription.Text;
                    binaryFile.BinaryFileTypeId = ddlBinaryFileType.SelectedValueAsInt();
                    if ( binaryFile.BinaryFileTypeId.HasValue )
                    {
                        binaryFile.BinaryFileType = new BinaryFileTypeService().Get( binaryFile.BinaryFileTypeId.Value );
                    }
                    binaryFile.LoadAttributes();
                    Rock.Attribute.Helper.GetEditValues( phAttributes, binaryFile );

                    // Process uploaded file using an optional workflow
                    Guid workflowTypeGuid = Guid.NewGuid();
                    if ( Guid.TryParse( GetAttributeValue( "Workflow" ), out workflowTypeGuid ) )
                    {
                        var workflowTypeService = new WorkflowTypeService();
                        var workflowType = workflowTypeService.Get( workflowTypeGuid );
                        if ( workflowType != null )
                        {
                            var workflow = Workflow.Activate( workflowType, binaryFile.FileName );

                            List<string> workflowErrors;
                            if ( workflow.Process( binaryFile, out workflowErrors ) )
                            {
                                binaryFile = binaryFileService.Get( binaryFile.Id );

                                if ( workflowType.IsPersisted )
                                {
                                    var workflowService = new Rock.Model.WorkflowService();
                                    workflowService.Add( workflow, CurrentPersonId );
                                    workflowService.Save( workflow, CurrentPersonId );
                                }
                            }
                        }
                    }
                }

                ShowDetail( binaryFile );
            }
        }

        #endregion
    }
}