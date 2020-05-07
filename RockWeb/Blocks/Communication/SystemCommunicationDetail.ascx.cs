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
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Text.RegularExpressions;

using Humanizer;

using Rock;
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

        private string _ItemFriendlyName = SystemCommunication.FriendlyTypeName;

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
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
                CreateDynamicLavaValueControls();
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

            string pageTitle = "New " + _ItemFriendlyName;

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

                lActionTitle.Text = ActionTitle.Edit( _ItemFriendlyName ).FormatAsHtmlTitle();
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

                kvlMergeFields.Value = emailTemplate.LavaFields.Select( a => string.Format( "{0}^{1}", a.Key, a.Value ) ).ToList().AsDelimited( "|" );

                hfShowAdditionalFields.Value = ( !string.IsNullOrEmpty( emailTemplate.Cc ) || !string.IsNullOrEmpty( emailTemplate.Bcc ) ).ToTrueFalse().ToLower();

                cbCssInliningEnabled.Checked = emailTemplate.CssInliningEnabled;

                showMessagePreview = true;
            }
            else
            {
                pdAuditDetails.Visible = false;
                lActionTitle.Text = ActionTitle.Add( _ItemFriendlyName ).FormatAsHtmlTitle();
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
                UpdatePreview();
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

            var templateLogoHtmlMatch = new Regex( "<img[^>]+id=[',\"]template-logo[',\"].*src=[',\"]([^\">]+)[',\"].*>" ).Match( ceEmailTemplate.Text );

            if ( templateLogoHtmlMatch.Groups.Count == 2 )
            {
                string originalTemplateLogoHtml = templateLogoHtmlMatch.Groups[0].Value;
                string originalTemplateLogoSrc = templateLogoHtmlMatch.Groups[1].Value;

                string newTemplateLogoSrc;

                // if a template-logo exists in the template, update the src attribute to whatever the uploaded logo is (or set it to the placeholder if it is not set)
                if ( imgTemplateLogo.BinaryFileId != null && imgTemplateLogo.BinaryFileId > 0 )
                {
                    newTemplateLogoSrc = ResolveRockUrl( string.Format( "~/GetImage.ashx?Id={0}", imgTemplateLogo.BinaryFileId ) );
                }
                else
                {
                    newTemplateLogoSrc = "/Content/EmailTemplates/placeholder-logo.png";
                }

                string newTemplateLogoHtml = originalTemplateLogoHtml.Replace( originalTemplateLogoSrc, newTemplateLogoSrc );

                ceEmailTemplate.Text = ceEmailTemplate.Text.Replace( originalTemplateLogoHtml, newTemplateLogoHtml );
            }


            UpdatePreview();
        }

        /// <summary>
        /// Handles the Click event of the lbUpdateLavaFields control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbUpdateLavaFields_Click( object sender, EventArgs e )
        {
            // do an UpdatePreview to make sure the "lava-fields" tag exists if there are lava fields defined in the UI
            UpdatePreview();

            var templateDoc = new HtmlAgilityPack.HtmlDocument();
            templateDoc.LoadHtml( ceEmailTemplate.Text );

            // take care of the lava fields stuff
            var lavaFieldsNode = templateDoc.GetElementbyId( "lava-fields" );
            var lavaFieldsTemplateDictionary = new Dictionary<string, string>();

            if ( lavaFieldsNode != null )
            {
                var templateDocLavaFieldLines = lavaFieldsNode.InnerText.Split( new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries ).Select( a => a.Trim() ).Where( a => a.IsNotNullOrWhiteSpace() ).ToList();

                // dictionary of keys and values from the lava fields in the 'lava-fields' div
                foreach ( var templateDocLavaFieldLine in templateDocLavaFieldLines )
                {
                    var match = Regex.Match( templateDocLavaFieldLine, @"{% assign (.*)\=(.*) %}" );
                    if ( match.Groups.Count != 3 )
                    {
                        continue;
                    }

                    var key = match.Groups[1].Value.Trim().RemoveSpaces();
                    var value = match.Groups[2].Value.Trim().Trim( '\'' );
                    lavaFieldsTemplateDictionary.Add( key, value );
                }
            }

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
            UpdatePreview();
        }

        /// <summary>
        /// Updates the preview.
        /// </summary>
        protected void UpdatePreview()
        {
            ShowTemplateLogoPicker();

            var origHtml = ceEmailTemplate.Text;

            /* We want to update the lava-fields node of template so that the lava-fields tag
             * is in sync with the lava merge fields in kvlMergeFields (which comes from CommunicationTemplate.LavaFields).
             * NOTE: We don't want to use a HTML Parser (like HtmlAgilityPack or AngleSharp),
             * because this is a mix of html and lava, and html parsers will end up corrupting the html+lava
             * So we'll..
             * 1) Use regex to find the lava-fields html and remove it from the code editor text
             * 2) make a new lava-fields tag with the values from kvlMergeFields (which comes from CommunicationTemplate.LavaFields)
             * 3) Put thge new lava-fields html back into the code editor text in the head in a noscript tag
             */

            // First, we'll take out the lava-fields tag (could be a div or a noscript, depending on which version of Rock last edited it)
            var lavaFieldsRegExLegacy = new System.Text.RegularExpressions.Regex( @"<div[\s\S]*lava-fields[\s\S]+?</div>([\n,\r])?", RegexOptions.Multiline );
            var lavaFieldsHtmlLegacy = lavaFieldsRegExLegacy.Match( ceEmailTemplate.Text ).Value;
            if ( lavaFieldsHtmlLegacy.IsNotNullOrWhiteSpace() )
            {
                ceEmailTemplate.Text = ceEmailTemplate.Text.Replace( lavaFieldsHtmlLegacy, string.Empty );
            }

            var lavaFieldsRegEx = new System.Text.RegularExpressions.Regex( @"<noscript[\s\S]*lava-fields[\s\S]+?</noscript>([\n,\r])?", RegexOptions.Multiline );
            var lavaFieldsHtml = lavaFieldsRegEx.Match( ceEmailTemplate.Text ).Value;
            if ( lavaFieldsHtml.IsNotNullOrWhiteSpace() )
            {
                ceEmailTemplate.Text = ceEmailTemplate.Text.Replace( lavaFieldsHtml, string.Empty );
            }

            var templateDocLavaFieldLines = lavaFieldsHtml.Split( new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries ).Select( a => a.Trim() ).Where( a => a.IsNotNullOrWhiteSpace() ).ToList();

            // dictionary of keys and default values from Lava Fields KeyValueList control
            var lavaFieldsDefaultDictionary = kvlMergeFields.Value.AsDictionary();

            // add any new lava fields that were added to the KeyValueList editor
            foreach ( var keyValue in lavaFieldsDefaultDictionary )
            {
                string pattern = string.Format( @"{{%\s+assign\s+{0}.*\s+=\s", keyValue.Key );
                if ( !templateDocLavaFieldLines.Any( a => Regex.IsMatch( a, pattern ) ) )
                {
                    templateDocLavaFieldLines.Add( "{% assign " + keyValue.Key + " = '" + keyValue.Value + "' %}" );
                }
            }

            // remove any lava fields that are not in the KeyValueList editor
            foreach ( var templateDocLavaFieldLine in templateDocLavaFieldLines.ToList() )
            {
                var found = false;
                foreach ( var keyValue in lavaFieldsDefaultDictionary )
                {
                    var pattern = string.Format( @"{{%\s+assign\s+{0}.*\s+=\s", keyValue.Key );
                    if ( !Regex.IsMatch( templateDocLavaFieldLine, pattern ) )
                    {
                        continue;
                    }

                    found = true;
                    break;
                }

                // if not found, delete it
                if ( !found )
                {
                    templateDocLavaFieldLines.Remove( templateDocLavaFieldLine );
                }
            }

            // dictionary of keys and values from the lava fields in the 'lava-fields' div
            var lavaFieldsTemplateDictionary = new Dictionary<string, string>();
            foreach ( var templateDocLavaFieldLine in templateDocLavaFieldLines )
            {
                var match = Regex.Match( templateDocLavaFieldLine, @"{% assign (.*)\=(.*) %}" );
                if ( match.Groups.Count != 3 )
                {
                    continue;
                }

                var key = match.Groups[1].Value.Trim().RemoveSpaces();
                var value = match.Groups[2].Value.Trim().Trim( '\'' );

                // If this is a postback, there will be a control that holds the value
                var lavaValueControl = phLavaFieldsControls.FindControl( "lavaValue_" + key ) as RockTextBox;
                if ( lavaValueControl != null && lavaValueControl.Text != value )
                {
                    value = lavaValueControl.Text;
                }

                lavaFieldsTemplateDictionary.Add( key, value );
            }

            if ( lavaFieldsTemplateDictionary.Any() )
            {
                // there are 3 cases of where the <noscript> tag should be
                // 1) There is a head tag (usually the case)
                // 2) There is not head tag, but there is a html tag
                // 3) There isn't a head or html tag
                var headTagRegex = new Regex( "<head(.*?)>" );
                var htmlTagRegex = new Regex( "<html(.*?)>" );

                // indent the lava-fields tag if there is a head or html tag
                string indent = string.Empty;
                if ( headTagRegex.Match( ceEmailTemplate.Text ).Success || htmlTagRegex.Match( ceEmailTemplate.Text ).Success )
                {
                    indent = " ";
                }

                var lavaAssignsHtmlBuilder = new StringBuilder();
                lavaAssignsHtmlBuilder.Append( indent + "<noscript id=\"lava-fields\">\n" );
                lavaAssignsHtmlBuilder.Append( indent + "  {% comment %}  Lava Fields: Code-Generated from Template Editor {% endcomment %}\n" );
                foreach ( var lavaFieldsTemplateItem in lavaFieldsTemplateDictionary )
                {
                    lavaAssignsHtmlBuilder.Append( indent + string.Format( "  {{% assign {0} = '{1}' %}}\n", lavaFieldsTemplateItem.Key, lavaFieldsTemplateItem.Value ) );
                }

                lavaAssignsHtmlBuilder.Append( indent + "</noscript>" );
                var lavaAssignsHtml = lavaAssignsHtmlBuilder.ToString();

                if ( headTagRegex.Match( ceEmailTemplate.Text ).Success )
                {
                    ceEmailTemplate.Text = headTagRegex.Replace( ceEmailTemplate.Text, ( m ) =>
                    {
                        return m.Value.TrimEnd() + "\n" + lavaAssignsHtml;
                    } );
                }
                else if ( htmlTagRegex.Match( ceEmailTemplate.Text ).Success )
                {
                    ceEmailTemplate.Text = htmlTagRegex.Replace( ceEmailTemplate.Text, ( m ) =>
                    {
                        return m.Value.TrimEnd() + "\n" + lavaAssignsHtml;
                    } );
                }
                else
                {
                    ceEmailTemplate.Text = lavaAssignsHtml + "\n\n" + ceEmailTemplate.Text.TrimStart();
                }
            }

            hfLavaFieldsState.Value = lavaFieldsTemplateDictionary.ToJson( Newtonsoft.Json.Formatting.None );

            CreateDynamicLavaValueControls();

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage );
            var resolvedPreviewHtml = ceEmailTemplate.Text.ResolveMergeFields( mergeFields );

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