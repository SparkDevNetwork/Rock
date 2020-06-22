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
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Communication;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Communication
{
    /// <summary>
    /// User control for editing a system communication
    /// </summary>
    [DisplayName( "System Communication Detail" )]
    [Category( "Communication" )]
    [Description( "Allows the administration of a system communication." )]
    public partial class SystemCommunicationDetail : RockBlock
    {
        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string CommunicationId = "CommunicationId";
        }

        #endregion Page Parameter Keys

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                // Get the CommunicationId if it is specified as a parameter.
                // If not found, check for the legacy parameter "EmailId".
                var communicationIdentifier = PageParameter( PageParameterKey.CommunicationId );

                if ( string.IsNullOrEmpty( communicationIdentifier ) )
                {
                    communicationIdentifier = PageParameter( "emailId" );
                }

                ShowEdit( communicationIdentifier.AsInteger() );
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
        public override List<Rock.Web.UI.BreadCrumb> GetBreadCrumbs( Rock.Web.PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            string pageTitle = "New " + SystemCommunication.FriendlyTypeName;

            // Get the CommunicationId if it is specified as a parameter.
            // If not found, check for the legacy parameter "EmailId".
            var communicationIdentifier = PageParameter( PageParameterKey.CommunicationId );

            if ( string.IsNullOrEmpty( communicationIdentifier ) )
            {
                communicationIdentifier = PageParameter( "emailId" );
            }

            int? communicationId = communicationIdentifier.AsIntegerOrNull();

            if ( communicationId.HasValue )
            {
                var communication = new SystemCommunicationService( new RockContext() ).Get( communicationId.Value );

                if ( communication != null )
                {
                    pageTitle = communication.Title;
                    breadCrumbs.Add( new BreadCrumb( communication.Title, pageReference ) );
                }
            }

            RockPage.Title = pageTitle;

            return breadCrumbs;
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
            var rockContext = new RockContext();

            SystemCommunicationService emailTemplateService = new SystemCommunicationService( rockContext );
            SystemCommunication emailTemplate;

            int emailTemplateId = int.Parse( hfEmailTemplateId.Value );

            if ( emailTemplateId == 0 )
            {
                emailTemplate = new SystemCommunication();
                emailTemplateService.Add( emailTemplate );
            }
            else
            {
                emailTemplate = emailTemplateService.Get( emailTemplateId );
            }

            emailTemplate.IsActive = cbIsActive.Checked;
            emailTemplate.CategoryId = cpCategory.SelectedValueAsInt();
            emailTemplate.Title = tbTitle.Text;
            emailTemplate.FromName = tbFromName.Text;
            emailTemplate.From = tbFrom.Text;
            emailTemplate.To = tbTo.Text;
            emailTemplate.Cc = tbCc.Text;
            emailTemplate.Bcc = tbBcc.Text;
            emailTemplate.Subject = tbSubject.Text;
            emailTemplate.Body = ceEmailTemplate.Text;
            emailTemplate.LavaFields = kvlMergeFields.Value.AsDictionaryOrNull();
            emailTemplate.CssInliningEnabled = cbCssInliningEnabled.Checked;

            emailTemplate.SMSFromDefinedValueId = dvpSMSFrom.SelectedValue.AsIntegerOrNull();
            emailTemplate.SMSMessage = tbSMSTextMessage.Text;

            if ( !emailTemplate.IsValid )
            {
                // If CodeEditor is hidden, we need to manually add the Required error message or it will not be shown.
                if ( string.IsNullOrWhiteSpace( ceEmailTemplate.Text ) && !ceEmailTemplate.Visible )
                {
                    var customValidator = new CustomValidator();

                    customValidator.ValidationGroup = ceEmailTemplate.ValidationGroup;
                    customValidator.ControlToValidate = ceEmailTemplate.ID;
                    customValidator.ErrorMessage = "Email Message Body is required.";
                    customValidator.IsValid = false;

                    Page.Validators.Add( customValidator );
                }

                // Controls will render the error messages
                return;
            }

            rockContext.SaveChanges();

            NavigateToParentPage();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="emailTemplateId">The email template id.</param>
        protected void ShowEdit( int emailTemplateId )
        {
            var globalAttributes = GlobalAttributesCache.Get();

            string globalFromName = globalAttributes.GetValue( "OrganizationName" );
            tbFromName.Help = string.Format( "If a From Name value is not entered the 'Organization Name' Global Attribute value of '{0}' will be used when this template is sent. <small><span class='tip tip-lava'></span></small>", globalFromName );

            string globalFrom = globalAttributes.GetValue( "OrganizationEmail" );
            tbFrom.Help = string.Format( "If a From Address value is not entered the 'Organization Email' Global Attribute value of '{0}' will be used when this template is sent. <small><span class='tip tip-lava'></span></small>", globalFrom );

            tbTo.Help = "You can specify multiple email addresses by separating them with a comma.";

            SystemCommunicationService emailTemplateService = new SystemCommunicationService( new RockContext() );
            SystemCommunication emailTemplate = emailTemplateService.Get( emailTemplateId );

            bool showMessagePreview = false;

            if ( emailTemplate != null )
            {
                pdAuditDetails.Visible = true;
                pdAuditDetails.SetEntity( emailTemplate, ResolveRockUrl( "~" ) );

                lActionTitle.Text = ActionTitle.Edit( SystemCommunication.FriendlyTypeName ).FormatAsHtmlTitle();
                hfEmailTemplateId.Value = emailTemplate.Id.ToString();

                cbIsActive.Checked = emailTemplate.IsActive.GetValueOrDefault();
                cpCategory.SetValue( emailTemplate.CategoryId );
                tbTitle.Text = emailTemplate.Title;
                tbFromName.Text = emailTemplate.FromName;
                tbFrom.Text = emailTemplate.From;
                tbTo.Text = emailTemplate.To;
                tbCc.Text = emailTemplate.Cc;
                tbBcc.Text = emailTemplate.Bcc;
                tbSubject.Text = emailTemplate.Subject;
                ceEmailTemplate.Text = emailTemplate.Body;

                nbTemplateHelp.InnerHtml = CommunicationTemplateHelper.GetTemplateHelp( false );

                kvlMergeFields.Value = emailTemplate.LavaFields.Select( a => string.Format( "{0}^{1}", a.Key, a.Value ) ).ToList().AsDelimited( "|" );

                hfShowAdditionalFields.Value = ( !string.IsNullOrEmpty( emailTemplate.Cc ) || !string.IsNullOrEmpty( emailTemplate.Bcc ) ).ToTrueFalse().ToLower();

                cbCssInliningEnabled.Checked = emailTemplate.CssInliningEnabled;

                showMessagePreview = true;
            }
            else
            {
                pdAuditDetails.Visible = false;
                lActionTitle.Text = ActionTitle.Add( SystemCommunication.FriendlyTypeName ).FormatAsHtmlTitle();
                hfEmailTemplateId.Value = 0.ToString();

                cbIsActive.Checked = true;
                cpCategory.SetValue( ( int? ) null );
                tbTitle.Text = string.Empty;
                tbFromName.Text = string.Empty;
                tbFrom.Text = string.Empty;
                tbTo.Text = string.Empty;
                tbCc.Text = string.Empty;
                tbBcc.Text = string.Empty;
                tbSubject.Text = string.Empty;
                ceEmailTemplate.Text = string.Empty;
            }

            SetEmailMessagePreviewModeEnabled( showMessagePreview );

            LoadDropDowns();

            // SMS Fields
            mfpSMSMessage.MergeFields.Clear();
            mfpSMSMessage.MergeFields.Add( "GlobalAttribute" );
            mfpSMSMessage.MergeFields.Add( "Rock.Model.Person" );

            if ( emailTemplate != null )
            {
                dvpSMSFrom.SetValue( emailTemplate.SMSFromDefinedValueId );
                tbSMSTextMessage.Text = emailTemplate.SMSMessage;
            }
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

        #endregion

        #region Email Message Editor

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

        #endregion
    }
}