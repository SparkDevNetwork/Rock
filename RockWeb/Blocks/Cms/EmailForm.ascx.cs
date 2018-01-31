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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Web;
using System.Text.RegularExpressions;
using System.Net.Mail;
using Rock.Communication;
using Rock.Security;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Email Form" )]
    [Category( "CMS" )]
    [Description( "Block that takes and HTML form and emails the contents to an address of your choosing." )]

    [TextField( "Receipient Email(s)", "Email addresses (comma delimited) to send the contents to.", true, "", "", 0, "RecipientEmail" )]
    [TextField( "Subject", "The subject line for the email. <span class='tip tip-lava'></span>", true, "", "", 1 )]
    [TextField( "From Email", "The email address to use for the from. <span class='tip tip-lava'></span>", true, "", "", 2 )]
    [TextField( "From Name", "The name to use for the from address. <span class='tip tip-lava'></span>", true, "", "", 3 )]
    [CodeEditorField( "HTML Form", "The HTML for the form the user will complete. <span class='tip tip-lava'></span>", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, false, @"{% if CurentUser %}
    {{ CurrentPerson.NickName }}, could you please complete the form below.
{% else %}
    Please complete the form below.
{% endif %}

<div class=""form-group"">
    <label for=""firstname"">First Name</label>
    {% if CurrentPerson %}
        <p>{{ CurrentPerson.NickName }}</p>
        <input type=""hidden"" id=""firstname"" name=""FirstName"" value=""{{ CurrentPerson.NickName }}"" />
    {% else %}
        <input class=""form-control"" id=""firstname"" name=""FirstName"" placeholder=""First Name"" required />
    {% endif %}
</div>

<div class=""form-group"">
    <label for=""lastname"">Last Name</label>
    
    {% if CurrentPerson %}
        <p>{{ CurrentPerson.LastName }}</p>
        <input type=""hidden"" id=""lastname"" name=""LastName"" value=""{{ CurrentPerson.LastName }}"" />
    {% else %}
        <input class=""form-control"" id=""lastname"" name=""LastName"" placeholder=""Last Name"" required />
    {% endif %}
</div>

<div class=""form-group"">
    <label for=""email"">Email</label>
    {% if CurrentPerson %}
        <input class=""form-control"" id=""email"" name=""Email"" value=""{{ CurrentPerson.Email }}"" placeholder=""Email"" required />
    {% else %}
        <input class=""form-control"" id=""email"" name=""Email"" placeholder=""Email"" required />
    {% endif %}
</div>

<div class=""form-group"">
    <label for=""email"">Message</label>
    <textarea id=""message"" rows=""4"" class=""form-control"" name=""Message"" placeholder=""Message"" required></textarea>
</div>

<div class=""form-group"">
    <label for=""email"">Attachment</label>
    <input type=""file"" id=""attachment"" name=""attachment"" /> <br />
    <input type=""file"" id=""attachment2"" name=""attachment2"" />
</div>
", "", 4 )]
    [CodeEditorField( "Message Body", "The email message body. <span class='tip tip-lava'></span>", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, false, @"{{ 'Global' | Attribute:'EmailHeader' }}

<p>
    A email form has been submitted. Please find the information below:
</p>

{% for field in FormFields %}
    {% assign fieldParts = field | PropertyToKeyValue %}

    <strong>{{ fieldParts.Key | Humanize | Capitalize }}</strong>: {{ fieldParts.Value }} <br/>
{% endfor %}

<p>&nbsp;</p>

{{ 'Global' | Attribute:'EmailFooter' }}", "", 5 )]
    [CodeEditorField( "Response Message", "The message the user will see when they submit the form if no response page if provided. Lava merege fields are available for you to use in your message.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, false, @"<div class=""alert alert-info"">
    Thank you for your response. We appreciate your feedback!
</div>", "", 6 )]
    [LinkedPage( "Response Page", "The page the use will be taken to after submitting the form. Use the 'Response Message' field if you just need a simple message.", false, "", "", 7 )]
    [TextField( "Submit Button Text", "The text to display for the submit button.", true, "Submit", "", 8 )]
    [TextField( "Submit Button Wrap CSS Class", "CSS class to add to the div wrapping the button.", false, "", "", 9, key: "SubmitButtonWrapCssClass" )]
    [TextField( "Submit Button CSS Class", "The CSS class add to the submit button.", false, "btn btn-primary", "", 10, key: "SubmitButtonCssClass" )]
    [BooleanField( "Enable Debug", "Shows the fields available to merge in lava.", false, "", 11 )]
    [BooleanField( "Save Communication History", "Should a record of this communication be saved to the recipient's profile", false, "", 12 )]
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for this HTML block.", false, order: 13 )]

    public partial class EmailForm : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

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

            // provide some good default values for from email / from name
            string fromEmail = GetAttributeValue( "FromEmail" );
            if ( string.IsNullOrWhiteSpace( fromEmail ) )
            {
                SetAttributeValue( "FromEmail", GlobalAttributesCache.Value( "OrganizationEmail" ) );
                SaveAttributeValues();
            }

            string fromName = GetAttributeValue( "FromName" );
            if ( string.IsNullOrWhiteSpace( fromEmail ) )
            {
                SetAttributeValue( "FromName", GlobalAttributesCache.Value( "OrganizationName" ) );
                SaveAttributeValues();
            }

            Page.Form.Enctype = "multipart/form-data";

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
                ShowForm();
                pnlEmailForm.Visible = true;

                if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "SubmitButtonText" ) ) )
                {
                    btnSubmit.Text = GetAttributeValue( "SubmitButtonText" );
                }

                if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "SubmitButtonWrapCssClass" ) ) )
                {
                    divButtonWrap.Attributes.Add( "class", GetAttributeValue( "SubmitButtonWrapCssClass" ) );
                }

                if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "SubmitButtonCssClass" ) ) )
                {
                    btnSubmit.CssClass = GetAttributeValue( "SubmitButtonCssClass" );
                }

                if ( string.IsNullOrWhiteSpace( GetAttributeValue( "RecipientEmail" ) ) )
                {
                    lError.Text = "<div class='alert alert-warning'>A recipient has not been provided for this form.</div>";
                }

                if ( string.IsNullOrWhiteSpace( GetAttributeValue( "Subject" ) ) )
                {
                    lError.Text += "<div class='alert alert-warning'>A subject has not been provided for this form.</div>";
                }
            }

            RockPage.AddScriptLink( ResolveRockUrl( "~/Scripts/jquery.visible.min.js" ) );
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowForm();
        }

        protected void btnSubmit_Click( object sender, EventArgs e )
        {
            SendEmail();
            pnlEmailForm.Visible = false;

            ScriptManager.RegisterStartupScript(
                Page,
                GetType(),
                "ScrollToMessage",
                "scrollToMessage();",
                true );
        }

        #endregion

        #region Methods

        private void ShowForm()
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );

            lEmailForm.Text = GetAttributeValue( "HTMLForm" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );
        }

        private void SendEmail()
        {
            // ensure this is not from a bot
            string[] bots = GlobalAttributesCache.Value( "EmailExceptionsFilter" ).Split( '|' );
            string test = GlobalAttributesCache.Value( "EmailExceptionsFilter" );
            var serverVarList = Context.Request.ServerVariables;
            bool isBot = false;

            foreach ( var bot in bots )
            {
                string[] botParms = bot.Split( '^' );
                if ( botParms.Length == 2 )
                {
                    var serverValue = serverVarList[botParms[0]];
                    if ( serverValue != null && serverValue.ToUpper().Contains( botParms[1].ToUpper().Trim() ) )
                    {
                        isBot = true;
                    }
                }
            }

            if ( !isBot )
            {
                var message = new RockEmailMessage();
                message.EnabledLavaCommands = GetAttributeValue( "EnabledLavaCommands" );

                // create merge objects
                var mergeFields = new Dictionary<string, object>();

                // create merge object for fields
                Regex rgxRockControls = new Regex( @"^ctl\d*\$.*" );
                var formFields = new Dictionary<string, object>();
                for ( int i = 0; i < Request.Form.Count; i++ )
                {
                    string formFieldKey = Request.Form.GetKey( i );
                    if ( formFieldKey != null &&
                        formFieldKey.Substring( 0, 1 ) != "_" &&
                        formFieldKey != "searchField_hSearchFilter" &&
                        formFieldKey != "send" &&
                        !rgxRockControls.IsMatch( formFieldKey ) )
                    {
                        formFields.Add( formFieldKey, Request.Form[formFieldKey] );
                    }
                }
                mergeFields.Add( "FormFields", formFields );

                // get attachments
                var rockContext = new RockContext();
                var binaryFileService = new BinaryFileService( rockContext );
                var binaryFileType = new BinaryFileTypeService( rockContext ).Get( Rock.SystemGuid.BinaryFiletype.DEFAULT.AsGuid() );
                for ( int i = 0; i < Request.Files.Count; i++ )
                {
                    var uploadedFile = Request.Files[i];
                    if ( uploadedFile.ContentLength > 0 && uploadedFile.FileName.IsNotNullOrWhitespace() )
                    {
                        var binaryFile = new BinaryFile();
                        binaryFileService.Add( binaryFile );
                        binaryFile.BinaryFileTypeId = binaryFileType.Id;
                        binaryFile.IsTemporary = false;
                        binaryFile.MimeType = uploadedFile.ContentType;
                        binaryFile.FileSize = uploadedFile.ContentLength;
                        binaryFile.FileName = Path.GetFileName( uploadedFile.FileName );
                        binaryFile.ContentStream = uploadedFile.InputStream;
                        rockContext.SaveChanges();

                        message.Attachments.Add( binaryFileService.Get( binaryFile.Id ) );
                    }
                }

                mergeFields.Add( "AttachmentCount", message.Attachments.Count );

                // send email
                foreach ( string recipient in GetAttributeValue( "RecipientEmail" ).Split( ',' ).ToList() )
                {
                    message.AddRecipient( new RecipientData( recipient, mergeFields ) );
                }
                message.Message = GetAttributeValue( "MessageBody" );
                message.FromEmail = GetAttributeValue( "FromEmail" );
                message.FromName = GetAttributeValue( "FromName" );
                message.Subject = GetAttributeValue( "Subject" );
                message.AppRoot = ResolveRockUrl( "~/" );
                message.ThemeRoot = ResolveRockUrl( "~~/" );
                message.CreateCommunicationRecord = GetAttributeValue( "SaveCommunicationHistory" ).AsBoolean();
                message.Send();

                // set response
                if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "ResponsePage" ) ) )
                {
                    NavigateToLinkedPage( "ResponsePage" );
                }

                // display response message
                lResponse.Visible = true;
                lEmailForm.Visible = false;
                lResponse.Text = GetAttributeValue( "ResponseMessage" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );

                // show debug info
                if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
                {
                    lDebug.Visible = true;
                    lDebug.Text = mergeFields.lavaDebugInfo();
                }
            }
            else
            {
                lResponse.Visible = true;
                lEmailForm.Visible = false;
                lResponse.Text = "You appear to be a computer. Check the global attribute 'Email Exceptions Filter' if you are getting this in error.";
            }
        }

        #endregion

    }
}