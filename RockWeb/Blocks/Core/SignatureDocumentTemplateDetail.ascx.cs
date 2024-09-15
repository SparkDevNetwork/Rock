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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.ElectronicSignature;
using Rock.Lava;
using Rock.Model;
using Rock.Pdf;
using Rock.Security;
using Rock.Utility;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// User controls for managing signature document templates
    /// </summary>
    [DisplayName( "Signature Document Template Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of the given signature document template." )]

    [BinaryFileTypeField(
        "Default File Type",
        Description = "The default file type to use when creating new documents.",
        Key = AttributeKey.DefaultFileType,
        IsRequired = false,
        DefaultBinaryFileTypeGuid = Rock.SystemGuid.BinaryFiletype.SIGNED_DOCUMENT_FILE_TYPE,
        Order = 0 )]

    [BooleanField(
        "Show Legacy Signature Providers",
        "Enable this setting to see the configuration for legacy signature providers. Note that support for these providers will be fully removed in the next full release.",
        Key = AttributeKey.ShowLegacyExternalProviders,
        DefaultBooleanValue = false,
        Order = 1 )]

    [Rock.SystemGuid.BlockTypeGuid( "9F26A1DA-74AE-4CB7-BABC-6AE81A581A06" )]
    public partial class SignatureDocumentTemplateDetail : RockBlock
    {
        public static class AttributeKey
        {
            public const string DefaultFileType = "DefaultFileType";
            public const string ShowLegacyExternalProviders = "ShowLegacyExternalProviders";
        }

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlSettings );

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", SignatureDocumentTemplate.FriendlyTypeName );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbEditModeMessage.Visible = false;

            if ( !Page.IsPostBack )
            {
                LoadDropDowns();
                ShowDetail( PageParameter( "SignatureDocumentTemplateId" ).AsInteger() );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail( PageParameter( "SignatureDocumentTemplateId" ).AsInteger() );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cpProvider control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cpExternalProvider_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateProviderControls();
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            SignatureDocumentTemplateService signatureDocumentTemplateService = new SignatureDocumentTemplateService( new RockContext() );
            SignatureDocumentTemplate signatureDocumentTemplate = signatureDocumentTemplateService.Get( hfSignatureDocumentTemplateId.ValueAsInt() );
            ShowEditDetails( signatureDocumentTemplate );
        }

        /// <summary>
        /// Handles the Click event of the btnSaveType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveType_Click( object sender, EventArgs e )
        {
            if ( ceESignatureLavaTemplate.Text.IsNullOrWhiteSpace() && !cpExternalProvider.SelectedEntityTypeId.HasValue )
            {
                nbSignatureLavaTemplateWarning.Visible = true;
                return;
            }

            var rockContext = new RockContext();

            SignatureDocumentTemplate signatureDocumentTemplate;
            SignatureDocumentTemplateService typeService = new SignatureDocumentTemplateService( rockContext );

            int signatureDocumentTemplateId = hfSignatureDocumentTemplateId.ValueAsInt();

            if ( signatureDocumentTemplateId == 0 )
            {
                signatureDocumentTemplate = new SignatureDocumentTemplate();
                typeService.Add( signatureDocumentTemplate );
            }
            else
            {
                signatureDocumentTemplate = typeService.Get( signatureDocumentTemplateId );
            }

            signatureDocumentTemplate.Name = tbTypeName.Text;
            signatureDocumentTemplate.Description = tbTypeDescription.Text;
            signatureDocumentTemplate.BinaryFileTypeId = bftpFileType.SelectedValueAsInt();
            signatureDocumentTemplate.ProviderEntityTypeId = cpExternalProvider.SelectedEntityTypeId;
            signatureDocumentTemplate.ProviderTemplateKey = ddlExternalProviderTemplate.SelectedValue;
            signatureDocumentTemplate.CompletionSystemCommunicationId = ddlCompletionSystemCommunication.SelectedValueAsId();
            signatureDocumentTemplate.SignatureType = rblSignatureType.SelectedValueAsEnumOrNull<SignatureType>() ?? SignatureType.Typed;
            signatureDocumentTemplate.LavaTemplate = ceESignatureLavaTemplate.Text;
            signatureDocumentTemplate.DocumentTerm = tbDocumentTerm.Text;
            signatureDocumentTemplate.IsActive = cbIsActive.Checked;
            signatureDocumentTemplate.IsValidInFuture = cbValidInFuture.Checked;
            if ( cbValidInFuture.Checked )
            {
                signatureDocumentTemplate.ValidityDurationInDays = nbValidDurationDays.IntegerValue;
            }

            if ( !signatureDocumentTemplate.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            rockContext.SaveChanges();

            var qryParams = new Dictionary<string, string>();
            qryParams["SignatureDocumentTemplateId"] = signatureDocumentTemplate.Id.ToString();
            NavigateToCurrentPage( qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            SignatureDocumentTemplateService signatureDocumentTemplateService = new SignatureDocumentTemplateService( rockContext );
            SignatureDocumentTemplate signatureDocumentTemplate = signatureDocumentTemplateService.Get( int.Parse( hfSignatureDocumentTemplateId.Value ) );

            if ( signatureDocumentTemplate != null )
            {
                if ( !signatureDocumentTemplate.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
                {
                    mdDeleteWarning.Show( "Sorry, You are not authorized to delete this document template.", ModalAlertType.Information );
                    return;
                }

                string errorMessage;
                if ( !signatureDocumentTemplateService.CanDelete( signatureDocumentTemplate, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                signatureDocumentTemplateService.Delete( signatureDocumentTemplate );

                rockContext.SaveChanges();
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancelType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancelType_Click( object sender, EventArgs e )
        {
            if ( hfSignatureDocumentTemplateId.IsZero() )
            {
                // Cancelling on Add.  Return to Grid
                NavigateToParentPage();
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                SignatureDocumentTemplateService signatureDocumentTemplateService = new SignatureDocumentTemplateService( new RockContext() );
                SignatureDocumentTemplate signatureDocumentTemplate = signatureDocumentTemplateService.Get( hfSignatureDocumentTemplateId.ValueAsInt() );
                ShowReadonlyDetails( signatureDocumentTemplate );
            }
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="signatureDocumentTemplate">Type of the defined.</param>
        private void ShowReadonlyDetails( SignatureDocumentTemplate signatureDocumentTemplate )
        {
            SetEditMode( false );

            hfSignatureDocumentTemplateId.SetValue( signatureDocumentTemplate.Id );

            lTitle.Text = signatureDocumentTemplate.Name.FormatAsHtmlTitle();
            lDescription.Text = signatureDocumentTemplate.Description;

            if ( signatureDocumentTemplate.BinaryFileType != null )
            {
                lLeftDetails.Text = new DescriptionList()
                    .Add( "File Type", signatureDocumentTemplate.BinaryFileType.Name )
                    .Html;
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="signatureDocumentTemplate">Type of the defined.</param>
        private void ShowEditDetails( SignatureDocumentTemplate signatureDocumentTemplate )
        {
            if ( signatureDocumentTemplate.Id > 0 )
            {
                lTitle.Text = ActionTitle.Edit( SignatureDocumentTemplate.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lTitle.Text = ActionTitle.Add( SignatureDocumentTemplate.FriendlyTypeName ).FormatAsHtmlTitle();
            }

            SetEditMode( true );

            tbTypeName.Text = signatureDocumentTemplate.Name;
            tbTypeDescription.Text = signatureDocumentTemplate.Description;
            bftpFileType.SetValue( signatureDocumentTemplate.BinaryFileTypeId );

            bool showLegacyExternalProviderOption = GetAttributeValue( AttributeKey.ShowLegacyExternalProviders ).AsBoolean();
            pnlLegacySignatureProviderSettings.Visible = showLegacyExternalProviderOption;
            if ( showLegacyExternalProviderOption )
            {
                // External Provider (SignNow) - This will be obsolete
                cpExternalProvider.SetValue( signatureDocumentTemplate.ProviderEntityType != null ? signatureDocumentTemplate.ProviderEntityType.Guid.ToString().ToUpper() : string.Empty );
                UpdateProviderControls(); // SNS 20230324 Moved this line up before the following line so the control knows about the value we're about to set it to
                ddlExternalProviderTemplate.SetValue( signatureDocumentTemplate.ProviderTemplateKey );
            }
            else
            {
                cpExternalProvider.SetValue( string.Empty );
                ddlExternalProviderTemplate.SetValue( string.Empty );
            }

            // Rock eSignature
            ceESignatureLavaTemplate.Text = signatureDocumentTemplate.LavaTemplate;
            cbIsActive.Checked = signatureDocumentTemplate.IsActive;
            rblSignatureType.SetValue( signatureDocumentTemplate.SignatureType.ConvertToInt() );
            tbDocumentTerm.Text = signatureDocumentTemplate.DocumentTerm;
            ddlCompletionSystemCommunication.SetValue( signatureDocumentTemplate.CompletionSystemCommunicationId );
            cbValidInFuture.Checked = signatureDocumentTemplate.IsValidInFuture;

            // If the Is Valid In Future is set to true, enable and make mandatory the NumberBox for Valid Duration Days.
            if ( signatureDocumentTemplate.IsValidInFuture )
            {
                nbValidDurationDays.Enabled = true;
                nbValidDurationDays.Visible = true;
                nbValidDurationDays.Required = true;
            }
            nbValidDurationDays.IntegerValue = signatureDocumentTemplate.ValidityDurationInDays;
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            vsDetails.Enabled = editable;
            fieldsetViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            ddlCompletionSystemCommunication.Items.Clear();
            ddlCompletionSystemCommunication.Items.Add( new ListItem() );
            using ( var rockContext = new RockContext() )
            {
                foreach ( var systemEmail in new SystemCommunicationService( rockContext )
                    .Queryable().AsNoTracking()
                    .OrderBy( e => e.Title )
                    .Select( e => new
                    {
                        e.Id,
                        e.Title
                    } ) )
                {
                    ddlCompletionSystemCommunication.Items.Add( new ListItem( systemEmail.Title, systemEmail.Id.ToString() ) );
                }
            }

            rblSignatureType.BindToEnum<SignatureType>();
        }

        /// <summary>
        /// </summary>
        private void UpdateProviderControls()
        {
            int? entityTypeId = cpExternalProvider.SelectedEntityTypeId;
            if ( !entityTypeId.HasValue )
            {
                return;
            }

            var entityType = EntityTypeCache.Get( entityTypeId.Value );

            if ( entityType == null )
            {
                return;
            }

            var component = DigitalSignatureContainer.GetComponent( entityType.Name );
            if ( component == null )
            {
                return;
            }

            ddlExternalProviderTemplate.Items.Clear();
            ddlExternalProviderTemplate.Items.Add( new ListItem() );

            var errors = new List<string>();
            var templates = component.GetTemplates( out errors );

            if ( templates != null )
            {
                foreach ( var keyVal in templates.OrderBy( d => d.Value ) )
                {
                    ddlExternalProviderTemplate.Items.Add( new ListItem( keyVal.Value, keyVal.Key ) );
                }
            }
            else
            {
                nbEditModeMessage.Text = string.Format( "<ul><li>{0}</li></ul>", errors.AsDelimited( "</li><li>" ) );
                nbEditModeMessage.Visible = true;
            }

            ddlExternalProviderTemplate.Visible = true;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="signatureDocumentTemplateId">The signature document type identifier.</param>
        public void ShowDetail( int signatureDocumentTemplateId )
        {
            SignatureDocumentTemplate signatureDocumentTemplate = null;

            using ( var rockContext = new RockContext() )
            {
                if ( !signatureDocumentTemplateId.Equals( 0 ) )
                {
                    signatureDocumentTemplate = new SignatureDocumentTemplateService( rockContext ).Get( signatureDocumentTemplateId );
                }

                if ( signatureDocumentTemplate == null )
                {
                    signatureDocumentTemplate = new SignatureDocumentTemplate { Id = 0 };
                    Guid? fileTypeGuid = GetAttributeValue( AttributeKey.DefaultFileType ).AsGuidOrNull();
                    if ( fileTypeGuid.HasValue )
                    {
                        var binaryFileType = new BinaryFileTypeService( rockContext ).Get( fileTypeGuid.Value );
                        if ( binaryFileType != null )
                        {
                            signatureDocumentTemplate.BinaryFileType = binaryFileType;
                            signatureDocumentTemplate.BinaryFileTypeId = binaryFileType.Id;
                        }
                    }

                    signatureDocumentTemplate.LavaTemplate = SignatureDocumentTemplate.DefaultLavaTemplate;
                }

                hfSignatureDocumentTemplateId.SetValue( signatureDocumentTemplate.Id );

                // render UI based on Authorized and IsSystem
                bool readOnly = false;

                nbEditModeMessage.Text = string.Empty;
                bool canEdit = signatureDocumentTemplate.IsAuthorized( Authorization.EDIT, CurrentPerson );
                bool canView = canEdit || signatureDocumentTemplate.IsAuthorized( Authorization.VIEW, CurrentPerson );

                if ( !canView )
                {
                    pnlDetails.Visible = false;
                }
                else
                {
                    pnlDetails.Visible = true;

                    if ( !canEdit )
                    {
                        readOnly = true;
                        nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( SignatureDocumentTemplate.FriendlyTypeName );
                    }

                    if ( readOnly )
                    {
                        btnEdit.Visible = false;
                        btnDelete.Visible = false;
                        ShowReadonlyDetails( signatureDocumentTemplate );
                    }
                    else
                    {
                        btnEdit.Visible = true;
                        btnDelete.Visible = false;
                        if ( signatureDocumentTemplate.Id > 0 )
                        {
                            ShowReadonlyDetails( signatureDocumentTemplate );
                        }
                        else
                        {
                            ShowEditDetails( signatureDocumentTemplate );
                        }
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Handles the CheckedChanged event of the tglTemplatePreview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglTemplatePreview_CheckedChanged( object sender, EventArgs e )
        {
            if ( tglTemplatePreview.Checked )
            {
                string pdfPreviewUrl = GetPdfPreviewUrl();

                pdfSignatureDocumentPreview.SourceUrl = pdfPreviewUrl;

                // toggle to show PDF and hide Lava Template
                rcwSignatureDocumentPreview.Visible = true;
                ceESignatureLavaTemplate.Visible = false;
            }
            else
            {
                // toggle to hide PDF and show Lava Template
                rcwSignatureDocumentPreview.Visible = false;
                ceESignatureLavaTemplate.Visible = true;
            }
        }

        /// <summary>
        /// Handles the CheckChanged event of the cbValidInFuture CheckBox Control
        /// When the Checkbox is checked, we need to display the ValidDurationDays NumberBox and make it a required control
        /// Otherwise, the NumberBox needs to be hidden.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void cbValidInFuture_CheckChanged( object sender, EventArgs e )
        {
            if ( cbValidInFuture.Checked )
            {
                nbValidDurationDays.Visible = true;
                nbValidDurationDays.Enabled = true;
                nbValidDurationDays.Required = true;
            }
            else
            {
                nbValidDurationDays.Visible = false;
                nbValidDurationDays.Enabled = false;
            }
        }

        /// <summary>
        /// Gets the PDF preview URL.
        /// </summary>
        /// <returns>System.String.</returns>
        private string GetPdfPreviewUrl()
        {
            var mergeFields = LavaHelper.GetCommonMergeFields( null );
            var signatureDocumentHtml = ElectronicSignatureHelper.GetSignatureDocumentHtml( ceESignatureLavaTemplate.Text, mergeFields );
            var fakeRandomHash = Rock.Security.Encryption.GetSHA1Hash( Guid.NewGuid().ToString() );

            var signatureInformationHtml = ElectronicSignatureHelper.GetSignatureInformationHtml( new GetSignatureInformationHtmlOptions
            {
                SignatureType = rblSignatureType.SelectedValueAsEnumOrNull<SignatureType>() ?? SignatureType.Typed,
                DrawnSignatureDataUrl = ElectronicSignatureHelper.SampleSignatureDataURL,
                SignedByPerson = this.CurrentPerson,
                SignedDateTime = RockDateTime.Now,
                SignedClientIp = this.GetClientIpAddress(),
                SignedName = this.CurrentPerson?.FullName,
                SignatureVerificationHash = fakeRandomHash
            } );

            string pdfPreviewUrl;

            using ( var pdfGenerator = new PdfGenerator() )
            {
                var signedDocumentHtml = ElectronicSignatureHelper.GetSignedDocumentHtml( signatureDocumentHtml, signatureInformationHtml );

                // put the pdf into a BinaryFile. We'll mark it IsTemporary so it'll eventually get cleaned up by RockCleanup
                BinaryFile binaryFile = pdfGenerator.GetAsBinaryFileFromHtml( bftpFileType.SelectedValueAsInt() ?? 0, "preview.pdf", signedDocumentHtml );
                binaryFile.IsTemporary = true;

                using ( var rockContext = new RockContext() )
                {
                    new BinaryFileService( rockContext ).Add( binaryFile );
                    rockContext.SaveChanges();
                }

                pdfPreviewUrl = FileUrlHelper.GetFileUrl( binaryFile.Guid );

            }

            return pdfPreviewUrl;
        }
    }
}