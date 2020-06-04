// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Security;
using System.IO;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Binary File Detail" )]
    [Category( "Core" )]
    [Description( "Shows the details of a particular binary file item." )]

    [BooleanField( "Show Binary File Type",
        Key = AttributeKey.ShowBinaryFileType )]

    [LinkedPage( "Edit Label Page",
        Description = "Page used to edit and test the contents of a label file.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.EditLabelPage )]

    [WorkflowTypeField( "Workflow",
        Description = "An optional workflow to activate for any new file uploaded",
        AllowMultiple = false,
        IsRequired = false,
        Category = "Advanced",
        Order = 0,
        Key = AttributeKey.Workflow )]

    [TextField( "Workflow Button Text",
        Description = "The button text to show for the rerun workflow button.",
        IsRequired = false,
        DefaultValue = "Rerun Workflow",
        Category = "Advanced",
        Order = 1,
        Key = AttributeKey.WorkflowButtonText )]

    public partial class BinaryFileDetail : RockBlock, IDetailBlock
    {
        public static class AttributeKey
        {
            public const string ShowBinaryFileType = "ShowBinaryFileType";
            public const string EditLabelPage = "EditLabelPage";
            public const string Workflow = "Workflow";
            public const string WorkflowButtonText = "WorkflowButtonText";
        }

        #region Properties

        /// <summary>
        /// Gets or sets the orphaned binary file identifier list.
        /// </summary>
        /// <value>
        /// The orphaned binary file identifier list.
        /// </value>
        public List<int> OrphanedBinaryFileIdList
        {
            get
            {
                return ViewState["OrphanedBinaryFileIdList"] as List<int> ?? new List<int>();
            }

            set
            {
                ViewState["OrphanedBinaryFileIdList"] = value;
            }
        }

        #endregion

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

            nbWorkflowSuccess.Visible = false;

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "BinaryFileId" ).AsInteger(), PageParameter( "BinaryFileTypeId" ).AsIntegerOrNull() );

                ddlBinaryFileType.Visible = GetAttributeValue( AttributeKey.ShowBinaryFileType ).AsBoolean();

                btnRerunWorkflow.Text = GetAttributeValue( AttributeKey.WorkflowButtonText );
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
                        Rock.Attribute.Helper.AddEditControls( binaryFile, phAttributes, false, BlockValidationGroup );
                    }
                }
            }
        }

        /// <summary>
        /// Launches the file upload workflow.
        /// </summary>
        /// <param name="binaryFile">The binary file.</param>
        /// <param name="binaryFileService">The binary file service.</param>
        private void LaunchFileUploadWorkflow( BinaryFile binaryFile, BinaryFileService binaryFileService )
        {
            // Process uploaded file using an optional workflow (which will probably populate attribute values)
            Guid workflowTypeGuid = Guid.NewGuid();
            if ( Guid.TryParse( GetAttributeValue( AttributeKey.Workflow ), out workflowTypeGuid ) )
            {
                try
                {
                    // temporarily set the binaryFile.Id to the uploaded binaryFile.Id so that workflow can do stuff with it
                    binaryFile.Id = fsFile.BinaryFileId ?? 0;

                    // create a rockContext for the workflow so that it can save it's changes, without 
                    var workflowRockContext = new RockContext();
                    var workflowType = WorkflowTypeCache.Get( workflowTypeGuid );
                    if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                    {
                        var workflow = Workflow.Activate( workflowType, binaryFile.FileName );

                        List<string> workflowErrors;
                        if ( new Rock.Model.WorkflowService( workflowRockContext ).Process( workflow, binaryFile, out workflowErrors ) )
                        {
                            binaryFile = binaryFileService.Get( binaryFile.Id );
                        }

                        nbWorkflowSuccess.Text = string.Format( "Successfully processed a <strong>{0}</strong> workflow!", workflowType.Name );
                        nbWorkflowSuccess.Visible = true;
                    }
                }
                finally
                {
                    // set binaryFile.Id to original id again since the UploadedFile is a temporary binaryFile with a different id
                    binaryFile.Id = hfBinaryFileId.ValueAsInt();
                }
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="binaryFileId">The binary file identifier.</param>
        public void ShowDetail( int binaryFileId )
        {
            ShowDetail( binaryFileId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="binaryFileId">The binary file identifier.</param>
        /// <param name="binaryFileTypeId">The binary file type id.</param>
        public void ShowDetail( int binaryFileId, int? binaryFileTypeId )
        {
            var rockContext = new RockContext();
            var binaryFileService = new BinaryFileService( rockContext );
            BinaryFile binaryFile = null;

            if ( !binaryFileId.Equals( 0 ) )
            {
                binaryFile = binaryFileService.Get( binaryFileId );
                pdAuditDetails.SetEntity( binaryFile, ResolveRockUrl( "~" ) );
            }

            if ( binaryFile == null )
            {
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;

                BinaryFileType binaryFileType = null;
                if ( binaryFileTypeId.HasValue )
                {
                    binaryFileType = new BinaryFileTypeService( rockContext ).Get( binaryFileTypeId.Value );
                }

                if ( binaryFileType != null )
                {
                    binaryFile = new BinaryFile { Id = 0, IsSystem = false, BinaryFileTypeId = binaryFileTypeId };
                    lActionTitle.Text = ActionTitle.Add( binaryFileType.Name ).FormatAsHtmlTitle();
                }
                else
                {
                    pnlDetails.Visible = false;
                    return;
                }
            }
            else
            {
                lActionTitle.Text = ActionTitle.Edit( binaryFile.BinaryFileType.Name ).FormatAsHtmlTitle();
            }

            binaryFile.LoadAttributes( rockContext );

            // initialize the fileUploader BinaryFileId to whatever file we are editing/viewing
            fsFile.BinaryFileId = binaryFile.Id;

            ShowBinaryFileDetail( binaryFile );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="binaryFile">The binary file.</param>
        public void ShowBinaryFileDetail( BinaryFile binaryFile )
        {
            pnlDetails.Visible = true;
            hfBinaryFileId.SetValue( binaryFile.Id );

            if ( binaryFile.BinaryFileTypeId.HasValue )
            {
                fsFile.BinaryFileTypeGuid = new BinaryFileTypeService( new RockContext() ).Get( binaryFile.BinaryFileTypeId.Value ).Guid;
            }

            tbName.Text = binaryFile.FileName;
            tbDescription.Text = binaryFile.Description;
            tbMimeType.Text = binaryFile.MimeType;
            ddlBinaryFileType.SetValue( binaryFile.BinaryFileTypeId );

            btnEditLabelContents.Visible = IsLabelFile();

            Guid? workflowTypeGuid = GetAttributeValue( AttributeKey.Workflow ).AsGuidOrNull();
            btnRerunWorkflow.Visible = workflowTypeGuid.HasValue;

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( BinaryFile.FriendlyTypeName );
            }

            phAttributes.Controls.Clear();

            if ( readOnly )
            {
                lActionTitle.Text = ActionTitle.View( BinaryFile.FriendlyTypeName );
                btnCancel.Text = "Close";
                Rock.Attribute.Helper.AddDisplayControls( binaryFile, phAttributes );
                fsFile.Enabled = false;
                fsFile.Label = "View File";
            }
            else
            {
                Rock.Attribute.Helper.AddEditControls( binaryFile, phAttributes, true, BlockValidationGroup );
            }

            tbName.ReadOnly = readOnly;
            tbDescription.ReadOnly = readOnly;
            tbMimeType.ReadOnly = readOnly;
            ddlBinaryFileType.Enabled = !readOnly;

            btnSave.Visible = !readOnly;
        }

        /// <summary>
        /// Determines whether the instance is holding a label file
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is label file]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsLabelFile()
        {
            return fsFile.BinaryFileId.HasValue && !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.EditLabelPage ) ) && fsFile.BinaryFileTypeGuid == Rock.SystemGuid.BinaryFiletype.CHECKIN_LABEL.AsGuid();
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
            if ( OrphanedBinaryFileIdList.Count > 0 )
            {
                var rockContext = new RockContext();
                BinaryFileService binaryFileService = new BinaryFileService( rockContext );

                foreach ( var id in OrphanedBinaryFileIdList )
                {
                    var tempBinaryFile = binaryFileService.Get( id );
                    if ( tempBinaryFile != null && tempBinaryFile.IsTemporary )
                    {
                        binaryFileService.Delete( tempBinaryFile );
                    }
                }

                rockContext.SaveChanges();
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            BinaryFile binaryFile;
            var rockContext = new RockContext();
            BinaryFileService binaryFileService = new BinaryFileService( rockContext );
            AttributeService attributeService = new AttributeService( rockContext );

            int? prevBinaryFileTypeId = null;

            int binaryFileId = int.Parse( hfBinaryFileId.Value );

            if ( binaryFileId == 0 )
            {
                binaryFile = new BinaryFile();
                binaryFileService.Add( binaryFile );
            }
            else
            {
                binaryFile = binaryFileService.Get( binaryFileId );
                prevBinaryFileTypeId = binaryFile != null ? binaryFile.BinaryFileTypeId : ( int? ) null;
            }

            // if a new file was uploaded, copy the uploaded file to this binaryFile (uploaded files are always new temporary binaryFiles)
            if ( fsFile.BinaryFileId != binaryFile.Id )
            {
                var uploadedBinaryFile = binaryFileService.Get( fsFile.BinaryFileId ?? 0 );
                if ( uploadedBinaryFile != null )
                {
                    binaryFile.BinaryFileTypeId = uploadedBinaryFile.BinaryFileTypeId;
                    binaryFile.FileSize = uploadedBinaryFile.FileSize;
                    var memoryStream = new MemoryStream();

                    // If this is a label file then we need to cleanup some settings that most templates will use by default
                    if ( IsLabelFile() )
                    {
                        // ^JUS will save changes to EEPROM, doing this for each label is not needed, slows printing dramatically, and shortens the printer's memory life.
                        string label = uploadedBinaryFile.ContentsToString().Replace( "^JUS", string.Empty );

                        // Use UTF-8 instead of ASCII
                        label = label.Replace( "^CI0", "^CI28" );

                        var writer = new StreamWriter( memoryStream );
                        writer.Write( label );
                        writer.Flush();
                    }
                    else
                    {
                        uploadedBinaryFile.ContentStream.CopyTo( memoryStream );
                    }

                    binaryFile.ContentStream = memoryStream;
                }
            }

            binaryFile.IsTemporary = false;
            binaryFile.FileName = tbName.Text;
            binaryFile.Description = tbDescription.Text;
            binaryFile.MimeType = tbMimeType.Text;
            binaryFile.BinaryFileTypeId = ddlBinaryFileType.SelectedValueAsInt();

            binaryFile.LoadAttributes( rockContext );
            Rock.Attribute.Helper.GetEditValues( phAttributes, binaryFile );

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !binaryFile.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            rockContext.WrapTransaction( () =>
            {
                foreach ( var id in OrphanedBinaryFileIdList )
                {
                    var tempBinaryFile = binaryFileService.Get( id );
                    if ( tempBinaryFile != null && tempBinaryFile.IsTemporary )
                    {
                        binaryFileService.Delete( tempBinaryFile );
                    }
                }

                rockContext.SaveChanges();
                binaryFile.SaveAttributeValues( rockContext );
            } );

            Rock.CheckIn.KioskLabel.Remove( binaryFile.Guid );

            if ( !prevBinaryFileTypeId.Equals( binaryFile.BinaryFileTypeId ) )
            {
                var checkInBinaryFileType = new BinaryFileTypeService( rockContext ).Get( Rock.SystemGuid.BinaryFiletype.CHECKIN_LABEL.AsGuid() );
                if ( checkInBinaryFileType != null && (
                    ( prevBinaryFileTypeId.HasValue && prevBinaryFileTypeId.Value == checkInBinaryFileType.Id ) ||
                    ( binaryFile.BinaryFileTypeId.HasValue && binaryFile.BinaryFileTypeId.Value == checkInBinaryFileType.Id ) ) )
                {
                    Rock.CheckIn.KioskDevice.Clear();
                }
            }

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
            var rockContext = new RockContext();
            var binaryFileService = new BinaryFileService( rockContext );
            BinaryFile binaryFile = null;
            if ( fsFile.BinaryFileId.HasValue )
            {
                binaryFile = binaryFileService.Get( fsFile.BinaryFileId.Value );
            }

            if ( binaryFile != null )
            {
                if ( !string.IsNullOrWhiteSpace( tbName.Text ) )
                {
                    binaryFile.FileName = tbName.Text;
                }

                // set binaryFile.Id to original id since the UploadedFile is a temporary binaryFile with a different id
                binaryFile.Id = hfBinaryFileId.ValueAsInt();
                binaryFile.Description = tbDescription.Text;
                binaryFile.BinaryFileTypeId = ddlBinaryFileType.SelectedValueAsInt();
                if ( binaryFile.BinaryFileTypeId.HasValue )
                {
                    binaryFile.BinaryFileType = new BinaryFileTypeService( rockContext ).Get( binaryFile.BinaryFileTypeId.Value );
                }

                var tempList = OrphanedBinaryFileIdList;
                tempList.Add( fsFile.BinaryFileId.Value );
                OrphanedBinaryFileIdList = tempList;

                // load attributes, then get the attribute values from the UI
                binaryFile.LoadAttributes();
                Helper.GetEditValues( phAttributes, binaryFile );

                LaunchFileUploadWorkflow( binaryFile, binaryFileService );
                ShowBinaryFileDetail( binaryFile );
            }
        }

        protected void btnEditLabelContents_Click( object sender, EventArgs e )
        {
            if ( fsFile.BinaryFileId.HasValue )
            {
                NavigateToLinkedPage( AttributeKey.EditLabelPage, new Dictionary<string, string> { { "BinaryFileId", fsFile.BinaryFileId.Value.ToString() } } );
            }
        }

        protected void btnRerunWorkflow_Click( object sender, EventArgs e )
        {
            if ( fsFile.BinaryFileId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var binaryFileService = new BinaryFileService( rockContext );
                    var binaryFile = binaryFileService.Get( fsFile.BinaryFileId.Value );
                    if ( binaryFile != null )
                    {
                        LaunchFileUploadWorkflow( binaryFile, binaryFileService );
                        ShowBinaryFileDetail( binaryFile );
                    }
                }
            }
        }

        #endregion
    }
}