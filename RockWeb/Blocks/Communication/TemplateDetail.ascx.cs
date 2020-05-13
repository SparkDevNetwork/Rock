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
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

using Humanizer;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Communication
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Template Detail" )]
    [Category( "Communication" )]
    [Description( "Used for editing a communication template that can be selected when creating a new communication, SMS, etc. to people." )]

    #region Block Attributes
    [BooleanField(
        "Personal Templates View",
        Description = "Is this block being used to display personal templates (only templates that current user is allowed to edit)?",
        DefaultBooleanValue = false,
        Order = 0,
        Key = AttributeKey.PersonalTemplatesView )]
    [BinaryFileTypeField(
        "Attachment Binary File Type",
        Description = "The FileType to use for files that are attached to an sms or email communication",
        IsRequired = true,
        DefaultBinaryFileTypeGuid = Rock.SystemGuid.BinaryFiletype.COMMUNICATION_ATTACHMENT,
        Order = 1,
        Key = AttributeKey.AttachmentBinaryFileType )]
    #endregion Block Attributes
    public partial class TemplateDetail : RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string PersonalTemplatesView = "PersonalTemplatesView";
            public const string AttachmentBinaryFileType = "AttachmentBinaryFileType";
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "TemplateId" ).AsInteger() );
            }
            else
            {
                // Create Controls for LavaFields Values
                var lavaFieldsTemplateDictionary = hfLavaFieldsState.Value.FromJsonOrNull<Dictionary<string, string>>() ?? new Dictionary<string, string>();

                // dictionary of keys and default values from Lava Fields KeyValueList control
                var lavaFieldsDefaultDictionary = kvlMergeFields.Value.AsDictionary();

                CommunicationTemplateHelper.CreateDynamicLavaValueControls( lavaFieldsTemplateDictionary, lavaFieldsDefaultDictionary, phLavaFieldsControls );
                btnUpdateTemplatePreview.Visible = lavaFieldsTemplateDictionary.Any();
            }
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="System.Collections.Generic.List{BreadCrumb}" /> of block related <see cref="Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( Rock.Web.PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            var pageTitle = "New Template";

            var templateId = PageParameter( "TemplateId" ).AsIntegerOrNull();
            if ( templateId.HasValue )
            {
                var template = new CommunicationTemplateService( new RockContext() ).Get( templateId.Value );
                if ( template != null )
                {
                    pageTitle = template.Name;
                }
            }

            breadCrumbs.Add( new BreadCrumb( pageTitle, pageReference ) );
            RockPage.Title = pageTitle;

            return breadCrumbs;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                return;
            }

            var rockContext = new RockContext();

            var communicationTemplateService = new CommunicationTemplateService( rockContext );
            var communicationTemplateAttachmentService = new CommunicationTemplateAttachmentService( rockContext );
            var binaryFileService = new BinaryFileService( rockContext );

            CommunicationTemplate communicationTemplate = null;
            var communicationTemplateId = hfCommunicationTemplateId.Value.AsIntegerOrNull();
            if ( communicationTemplateId.HasValue )
            {
                communicationTemplate = communicationTemplateService.Get( communicationTemplateId.Value );
            }

            var newTemplate = false;
            if ( communicationTemplate == null )
            {
                newTemplate = true;
                communicationTemplate = new CommunicationTemplate();
                communicationTemplateService.Add( communicationTemplate );
            }

            communicationTemplate.Name = tbName.Text;
            communicationTemplate.IsActive = cbIsActive.Checked;
            communicationTemplate.Description = tbDescription.Text;

            if ( communicationTemplate.ImageFileId != imgTemplatePreview.BinaryFileId )
            {
                var oldImageTemplatePreview = binaryFileService.Get( communicationTemplate.ImageFileId ?? 0 );
                if ( oldImageTemplatePreview != null )
                {
                    // the old image template preview won't be needed anymore, so make it IsTemporary and have it get cleaned up later
                    oldImageTemplatePreview.IsTemporary = true;
                }
            }

            communicationTemplate.ImageFileId = imgTemplatePreview.BinaryFileId;

            // Ensure that the ImagePreview is not set as IsTemporary=True
            if ( communicationTemplate.ImageFileId.HasValue )
            {
                var imageTemplatePreview = binaryFileService.Get( communicationTemplate.ImageFileId.Value );
                if ( imageTemplatePreview != null && imageTemplatePreview.IsTemporary )
                {
                    imageTemplatePreview.IsTemporary = false;
                }
            }

            // Note: If the Logo has changed, we can't get rid of it since existing communications might use it
            communicationTemplate.LogoBinaryFileId = imgTemplateLogo.BinaryFileId;

            // Ensure that the ImagePreview is not set as IsTemporary=True
            if ( communicationTemplate.LogoBinaryFileId.HasValue )
            {
                var newImageTemplateLogo = binaryFileService.Get( communicationTemplate.LogoBinaryFileId.Value );
                if ( newImageTemplateLogo != null && newImageTemplateLogo.IsTemporary )
                {
                    newImageTemplateLogo.IsTemporary = false;
                }
            }

            communicationTemplate.FromName = tbFromName.Text;
            communicationTemplate.FromEmail = tbFromAddress.Text;
            communicationTemplate.ReplyToEmail = tbReplyToAddress.Text;
            communicationTemplate.CCEmails = tbCCList.Text;
            communicationTemplate.BCCEmails = tbBCCList.Text;
            communicationTemplate.LavaFields = kvlMergeFields.Value.AsDictionaryOrNull();
            communicationTemplate.CssInliningEnabled = cbCssInliningEnabled.Checked;

            var binaryFileIds = hfAttachedBinaryFileIds.Value.SplitDelimitedValues().AsIntegerList();

            // delete any attachments that are no longer included
            foreach ( var attachment in communicationTemplate.Attachments
                .Where( a => !binaryFileIds.Contains( a.BinaryFileId ) ).ToList() )
            {
                communicationTemplate.Attachments.Remove( attachment );
                communicationTemplateAttachmentService.Delete( attachment );
            }

            // add any new attachments that were added
            foreach ( var attachmentBinaryFileId in binaryFileIds.Where( a => communicationTemplate.Attachments.All( x => x.BinaryFileId != a ) ) )
            {
                communicationTemplate.Attachments.Add( new CommunicationTemplateAttachment { BinaryFileId = attachmentBinaryFileId, CommunicationType = CommunicationType.Email } );
            }

            communicationTemplate.Subject = tbEmailSubject.Text;
            communicationTemplate.Message = ceEmailTemplate.Text;

            communicationTemplate.SMSFromDefinedValueId = dvpSMSFrom.SelectedValue.AsIntegerOrNull();
            communicationTemplate.SMSMessage = tbSMSTextMessage.Text;

            communicationTemplate.CategoryId = cpCategory.SelectedValueAsInt();

            rockContext.SaveChanges();

            var personalView = GetAttributeValue( AttributeKey.PersonalTemplatesView ).AsBoolean();
            if ( newTemplate )
            {
                communicationTemplate = communicationTemplateService.Get( communicationTemplate.Id );
                if ( communicationTemplate != null )
                {
                    if ( personalView )
                    {
                        // If editing personal templates, make the new template is private/personal to current user
                        communicationTemplate.MakePrivate( Authorization.VIEW, CurrentPerson );
                        communicationTemplate.MakePrivate( Authorization.EDIT, CurrentPerson );
                        communicationTemplate.MakePrivate( Authorization.ADMINISTRATE, CurrentPerson );
                    }
                    else
                    {
                        // Otherwise, make sure user can view and edit the new template.
                        if ( !communicationTemplate.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                        {
                            communicationTemplate.AllowPerson( Authorization.VIEW, CurrentPerson );
                        }

                        // Make sure user can edit the new template.
                        if ( !communicationTemplate.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                        {
                            communicationTemplate.AllowPerson( Authorization.EDIT, CurrentPerson );
                        }
                    }

                    // Always make sure RSR-Admin and Communication Admin can see
                    var groupService = new GroupService( rockContext );
                    var communicationAdministrators = groupService.Get( Rock.SystemGuid.Group.GROUP_COMMUNICATION_ADMINISTRATORS.AsGuid() );
                    if ( communicationAdministrators != null )
                    {
                        communicationTemplate.AllowSecurityRole( Authorization.VIEW, communicationAdministrators, rockContext );
                        communicationTemplate.AllowSecurityRole( Authorization.EDIT, communicationAdministrators, rockContext );
                        communicationTemplate.AllowSecurityRole( Authorization.ADMINISTRATE, communicationAdministrators, rockContext );
                    }

                    var rockAdministrators = groupService.Get( Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid() );
                    if ( rockAdministrators != null )
                    {
                        communicationTemplate.AllowSecurityRole( Authorization.VIEW, rockAdministrators, rockContext );
                        communicationTemplate.AllowSecurityRole( Authorization.EDIT, rockAdministrators, rockContext );
                        communicationTemplate.AllowSecurityRole( Authorization.ADMINISTRATE, rockAdministrators, rockContext );
                    }
                }
            }

            NavigateToParentPage();
        }

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
        /// Handles the ImageUploaded event of the imgTemplateLogo control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ImageUploaderEventArgs"/> instance containing the event data.</param>
        protected void imgTemplateLogo_ImageUploaded( object sender, ImageUploaderEventArgs e )
        {
            if ( e.EventArgument == ImageUploaderEventArgs.ArgumentType.ImageRemoved )
            {
                // ensure that BinaryFileId is set to null if this is an ImageRemoved event
                imgTemplateLogo.BinaryFileId = null;
            }

            UpdateControls();
        }

        /// <summary>
        /// Handles the Click event of the lbUpdateLavaFields control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbUpdateLavaFields_Click( object sender, EventArgs e )
        {
            // do an UpdateControls to make sure the "lava-fields" tag exists if there are lava fields defined in the UI
            UpdateControls();

            Dictionary<string, string> lavaFieldsTemplateDictionary = CommunicationTemplateHelper.GetLavaFieldsTemplateDictionaryFromTemplateHtml( ceEmailTemplate.Text );
            kvlMergeFields.Value = lavaFieldsTemplateDictionary.Select( a => string.Format( "{0}^{1}", a.Key, a.Value ) ).ToList().AsDelimited( "|" );
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="templateId">The template identifier.</param>
        private void ShowDetail( int templateId )
        {
            CommunicationTemplate communicationTemplate = null;
            var newTemplate = false;

            if ( !templateId.Equals( 0 ) )
            {
                communicationTemplate = new CommunicationTemplateService( new RockContext() ).Get( templateId );
                if ( communicationTemplate != null )
                {
                    lTitle.Text = communicationTemplate.Name.FormatAsHtmlTitle();
                    pdAuditDetails.SetEntity( communicationTemplate, ResolveRockUrl( "~" ) );
                }
            }

            if ( communicationTemplate == null )
            {
                RockPage.PageTitle = "New Communication Template";
                lTitle.Text = "New Communication Template".FormatAsHtmlTitle();
                communicationTemplate = new CommunicationTemplate();
                newTemplate = true;
            }

            LoadDropDowns();

            mfpSMSMessage.MergeFields.Clear();
            mfpSMSMessage.MergeFields.Add( "GlobalAttribute" );
            mfpSMSMessage.MergeFields.Add( "Rock.Model.Person" );

            hfCommunicationTemplateId.Value = templateId.ToString();

            tbName.Text = communicationTemplate.Name;
            cbIsActive.Checked = communicationTemplate.IsActive;
            tbDescription.Text = communicationTemplate.Description;
            cpCategory.SetValue( communicationTemplate.CategoryId );

            imgTemplatePreview.BinaryFileId = communicationTemplate.ImageFileId;
            imgTemplateLogo.BinaryFileId = communicationTemplate.LogoBinaryFileId;

            // Email Fields
            tbFromName.Text = communicationTemplate.FromName;
            tbFromAddress.Text = communicationTemplate.FromEmail;

            tbReplyToAddress.Text = communicationTemplate.ReplyToEmail;
            tbCCList.Text = communicationTemplate.CCEmails;
            tbBCCList.Text = communicationTemplate.BCCEmails;
            cbCssInliningEnabled.Checked = communicationTemplate.CssInliningEnabled;
            kvlMergeFields.Value = communicationTemplate.LavaFields.Select( a => string.Format( "{0}^{1}", a.Key, a.Value ) ).ToList().AsDelimited( "|" );

            hfShowAdditionalFields.Value = ( !string.IsNullOrEmpty( communicationTemplate.ReplyToEmail ) || !string.IsNullOrEmpty( communicationTemplate.CCEmails ) || !string.IsNullOrEmpty( communicationTemplate.BCCEmails ) ).ToTrueFalse().ToLower();

            tbEmailSubject.Text = communicationTemplate.Subject;

            nbTemplateHelp.InnerHtml = CommunicationTemplateHelper.GetTemplateHelp( true );
            ceEmailTemplate.Text = communicationTemplate.Message;

            hfAttachedBinaryFileIds.Value = communicationTemplate.Attachments.Select( a => a.BinaryFileId ).ToList().AsDelimited( "," );
            UpdateAttachedFiles( false );

            // SMS Fields
            dvpSMSFrom.SetValue( communicationTemplate.SMSFromDefinedValueId );
            tbSMSTextMessage.Text = communicationTemplate.SMSMessage;

            // render UI based on Authorized and IsSystem
            var readOnly = false;
            var restrictedEdit = false;

            if ( !newTemplate && !communicationTemplate.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                restrictedEdit = true;
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.NotAuthorizedToEdit( CommunicationTemplate.FriendlyTypeName );
                nbEditModeMessage.Visible = true;
            }

            if ( communicationTemplate.IsSystem )
            {
                restrictedEdit = true;
                nbEditModeMessage.Text = EditModeMessage.System( CommunicationTemplate.FriendlyTypeName );
                nbEditModeMessage.Visible = true;
            }

            tbName.ReadOnly = restrictedEdit;
            cbIsActive.Enabled = !restrictedEdit;

            tbFromName.ReadOnly = restrictedEdit;
            tbName.ReadOnly = restrictedEdit;
            tbFromAddress.ReadOnly = restrictedEdit;
            tbReplyToAddress.ReadOnly = restrictedEdit;
            tbCCList.ReadOnly = restrictedEdit;
            tbBCCList.ReadOnly = restrictedEdit;
            tbEmailSubject.ReadOnly = restrictedEdit;
            fupAttachments.Visible = !restrictedEdit;
            fupAttachments.BinaryFileTypeGuid = this.GetAttributeValue( AttributeKey.AttachmentBinaryFileType ).AsGuidOrNull() ?? Rock.SystemGuid.BinaryFiletype.DEFAULT.AsGuid();
            // Allow these to be Editable if they are IsSystem, but not if they don't have EDIT Auth
            tbDescription.ReadOnly = readOnly;
            imgTemplatePreview.Enabled = !readOnly;
            ceEmailTemplate.ReadOnly = readOnly;

            mfpSMSMessage.Visible = !restrictedEdit;
            dvpSMSFrom.Enabled = !restrictedEdit;
            tbSMSTextMessage.ReadOnly = restrictedEdit;
            ceEmailTemplate.ReadOnly = restrictedEdit;

            btnSave.Enabled = !readOnly;

            tglPreviewAdvanced.Checked = true;
            SetEmailMessagePreviewModeEnabled( tglPreviewAdvanced.Checked );
        }

        /// <summary>
        /// Sets the Email Message view mode to Prewiew or Advanced.
        /// </summary>
        /// <param name="isEnabled">If true, Preview mode will be enabled.</param>
        private void SetEmailMessagePreviewModeEnabled( bool isEnabled )
        {
            if ( tglPreviewAdvanced.Checked != isEnabled )
            {
                tglPreviewAdvanced.Checked = isEnabled;
            }

            pnlAdvanced.Visible = !isEnabled;
            pnlPreview.Visible = isEnabled;

            if ( isEnabled )
            {
                UpdateControls();
            }
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            dvpSMSFrom.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM ) ).Id;
            dvpSMSFrom.DisplayDescriptions = true;
        }

        /// <summary>
        /// Handles the FileUploaded event of the fupAttachments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FileUploaderEventArgs"/> instance containing the event data.</param>
        protected void fupAttachments_FileUploaded( object sender, FileUploaderEventArgs e )
        {
            UpdateAttachedFiles( true );
        }

        /// <summary>
        /// Updates the HTML for the Attached Files and optionally adds the file that was just uploaded using the file uploader
        /// </summary>
        private void UpdateAttachedFiles( bool addUploadedFile )
        {
            var attachmentList = hfAttachedBinaryFileIds.Value.SplitDelimitedValues().AsIntegerList();
            if ( addUploadedFile && fupAttachments.BinaryFileId.HasValue )
            {
                if ( !attachmentList.Contains( fupAttachments.BinaryFileId.Value ) )
                {
                    attachmentList.Add( fupAttachments.BinaryFileId.Value );
                }
            }

            hfAttachedBinaryFileIds.Value = attachmentList.AsDelimited( "," );
            fupAttachments.BinaryFileId = null;

            // pre-populate dictionary so that the attachments are listed in the order they were added
            var binaryFileAttachments = attachmentList.ToDictionary( k => k, v => string.Empty );
            using ( var rockContext = new RockContext() )
            {
                var binaryFileInfoList = new BinaryFileService( rockContext ).Queryable()
                        .Where( f => attachmentList.Contains( f.Id ) )
                        .Select( f => new
                        {
                            f.Id,
                            f.FileName
                        } );

                foreach ( var binaryFileInfo in binaryFileInfoList )
                {
                    binaryFileAttachments[binaryFileInfo.Id] = binaryFileInfo.FileName;
                }
            }

            var sbAttachmentsHtml = new StringBuilder();
            sbAttachmentsHtml.AppendLine( "<div class='attachment'>" );
            sbAttachmentsHtml.AppendLine( "  <ul class='attachment-content'>" );

            foreach ( var binaryFileAttachment in binaryFileAttachments )
            {
                var attachmentUrl = string.Format( "{0}GetFile.ashx?id={1}", System.Web.VirtualPathUtility.ToAbsolute( "~" ), binaryFileAttachment.Key );
                var removeAttachmentJS = string.Format( "removeAttachment( this, '{0}', '{1}' );", hfAttachedBinaryFileIds.ClientID, binaryFileAttachment.Key );
                sbAttachmentsHtml.AppendLine( string.Format( "    <li><a href='{0}' target='_blank'>{1}</a> <a><i class='fa fa-times' onclick=\"{2}\"></i></a></li>", attachmentUrl, binaryFileAttachment.Value, removeAttachmentJS ) );
            }

            sbAttachmentsHtml.AppendLine( "  </ul>" );
            sbAttachmentsHtml.AppendLine( "</div>" );

            lAttachmentListHtml.Text = sbAttachmentsHtml.ToString();
        }

        /// <summary>
        /// Handles the SelectItem event of the mfpMessage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mfpMessage_SelectItem( object sender, EventArgs e )
        {
            tbSMSTextMessage.Text += mfpSMSMessage.SelectedMergeField;
            mfpSMSMessage.SetValue( string.Empty );
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tglPreviewAdvanced control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglPreviewAdvanced_CheckedChanged( object sender, EventArgs e )
        {
            SetEmailMessagePreviewModeEnabled( tglPreviewAdvanced.Checked );
        }

        /// <summary>
        /// Handles the Click event of the btnUpdateTemplatePreview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnUpdateTemplatePreview_Click( object sender, EventArgs e )
        {
            UpdateControls();
        }

        /// <summary>
        /// Updates the controls
        /// </summary>
        private void UpdateControls()
        {
            pnlTemplateLogo.Visible = CommunicationTemplateHelper.HasTemplateLogo( ceEmailTemplate.Text );
            imgTemplateLogo.Help = CommunicationTemplateHelper.GetTemplateLogoHelpText( ceEmailTemplate.Text );

            var lavaFieldsTemplateDictionaryFromControls = hfLavaFieldsState.Value.FromJsonOrNull<Dictionary<string, string>>() ?? new Dictionary<string, string>();
            lavaFieldsTemplateDictionaryFromControls = CommunicationTemplateHelper.UpdateLavaFieldsTemplateDictionaryFromControls( phLavaFieldsControls, lavaFieldsTemplateDictionaryFromControls );

            // dictionary of keys and default values from Lava Fields KeyValueList control
            var lavaFieldsDefaultDictionary = kvlMergeFields.Value.AsDictionary();

            ceEmailTemplate.Text = CommunicationTemplateHelper.GetUpdatedTemplateHtml( ceEmailTemplate.Text, imgTemplateLogo.BinaryFileId, lavaFieldsTemplateDictionaryFromControls, lavaFieldsDefaultDictionary );

            var lavaFieldsTemplateDictionary = CommunicationTemplateHelper.GetLavaFieldsTemplateDictionaryFromTemplateHtml( ceEmailTemplate.Text );
            hfLavaFieldsState.Value = lavaFieldsTemplateDictionary.ToJson( Newtonsoft.Json.Formatting.None );
            btnUpdateTemplatePreview.Visible = lavaFieldsTemplateDictionary.Any();
            CommunicationTemplateHelper.CreateDynamicLavaValueControls( lavaFieldsTemplateDictionary, lavaFieldsDefaultDictionary, phLavaFieldsControls );

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage );
            string resolvedPreviewHtml = ceEmailTemplate.Text.ResolveMergeFields( mergeFields );
            if ( cbCssInliningEnabled.Checked )
            {
                resolvedPreviewHtml = resolvedPreviewHtml.ConvertHtmlStylesToInlineAttributes();
            }

            ifEmailPreview.Attributes["srcdoc"] = resolvedPreviewHtml;
            pnlEmailPreview.Visible = true;
            upnlEmailPreview.Update();
        }

        /// <summary>
        /// If there is a template logo node in the email template, show the template logo file uploader and set its help text
        /// </summary>
        private void ShowTemplateLogoPicker()
        {
            var templateDoc = new HtmlAgilityPack.HtmlDocument();
            templateDoc.LoadHtml( ceEmailTemplate.Text );

            // only show the template logo uploader if there is a div with id='template-logo'
            // then update the help-message on the loader based on the template-logo's data-instructions attribute and width and height
            // this gets called when the codeeditor is done initializing and when the cursor blurs out of the template code editor
            HtmlAgilityPack.HtmlNode templateLogoNode = templateDoc.GetElementbyId( "template-logo" );

            if ( templateLogoNode == null )
            {
                pnlTemplateLogo.Visible = false;
                return;
            }

            pnlTemplateLogo.Visible = true;
            string helpText = null;
            if ( templateLogoNode.Attributes.Contains( "data-instructions" ) )
            {
                helpText = templateLogoNode.Attributes["data-instructions"].Value;
            }

            if ( helpText.IsNullOrWhiteSpace() )
            {
                helpText = "The Logo that can be included in the contents of the message";
            }

            string helpWidth = null;
            string helpHeight = null;
            if ( templateLogoNode.Attributes.Contains( "width" ) )
            {
                helpWidth = templateLogoNode.Attributes["width"].Value;
            }

            if ( templateLogoNode.Attributes.Contains( "height" ) )
            {
                helpHeight = templateLogoNode.Attributes["height"].Value;
            }

            if ( helpWidth.IsNotNullOrWhiteSpace() && helpHeight.IsNotNullOrWhiteSpace() )
            {
                helpText += string.Format( " (Image size: {0}px x {1}px)", helpWidth, helpHeight );
            }

            imgTemplateLogo.Help = helpText;
        }

        /// <summary>
        /// Creates the dynamic lava value controls.
        /// </summary>
        private void CreateDynamicLavaValueControls()
        {
            // Create Controls for LavaFields Values
            var lavaFieldsTemplateDictionary = hfLavaFieldsState.Value.FromJsonOrNull<Dictionary<string, string>>() ?? new Dictionary<string, string>();

            // dictionary of keys and default values from Lava Fields KeyValueList control
            var lavaFieldsDefaultDictionary = kvlMergeFields.Value.AsDictionary();

            phLavaFieldsControls.Controls.Clear();

            btnUpdateTemplatePreview.Visible = lavaFieldsTemplateDictionary.Any();

            foreach ( var keyValue in lavaFieldsTemplateDictionary )
            {
                var lavaValueControl = keyValue.Key.EndsWith( "Color" ) ? new ColorPicker() : new RockTextBox() { CssClass = "input-width-lg" };

                var rcwLavaValue = new RockControlWrapper();
                phLavaFieldsControls.Controls.Add( rcwLavaValue );

                rcwLavaValue.Label = keyValue.Key.SplitCase().Transform( To.TitleCase );
                rcwLavaValue.ID = "rcwLavaValue_" + keyValue.Key;

                lavaValueControl.ID = "lavaValue_" + keyValue.Key;
                lavaValueControl.AddCssClass( "pull-left" );
                lavaValueControl.Text = keyValue.Value;
                rcwLavaValue.Controls.Add( lavaValueControl );

                var btnRevertLavaValue = new Literal { ID = "btnRevertLavaValue_" + keyValue.Key };
                var defaultValue = lavaFieldsDefaultDictionary.GetValueOrNull( keyValue.Key );
                var visibility = keyValue.Value != defaultValue ? "visible" : "hidden";
                btnRevertLavaValue.Text = string.Format( "<div class='btn js-revertlavavalue' title='Revert to default' data-value-control='{0}' data-default='{1}' style='visibility:{2}'><i class='fa fa-times'></i></div>", lavaValueControl.ClientID, defaultValue, visibility );
                rcwLavaValue.Controls.Add( btnRevertLavaValue );
            }
        }

        #endregion
    }
}
